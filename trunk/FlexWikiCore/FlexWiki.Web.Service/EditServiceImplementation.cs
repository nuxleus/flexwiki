#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using FlexWiki;
using System.Configuration;
using System.IO;

namespace FlexWiki.Web.Services
{
	/// <summary>
	/// Summary description for EditService.
	/// </summary>
	[WebService(Namespace="http://www.flexwiki.com/webservices/")]
	public class EditServiceImplementation : System.Web.Services.WebService
	{
		private LinkMaker _linkMaker;

		protected Federation TheFederation
		{
			get
			{
				return (Federation)(Application["---FEDERATION---"]);
			}
			set
			{
				Application["---FEDERATION---"] = value;
			}
		}

		protected LinkMaker TheLinkMaker
		{
			get
			{
				return _linkMaker;
			}
		}

		public EditServiceImplementation()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();

			EstablishFederation();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		/// <summary>
		/// CanEdit checks to see if the user is Authenticated using supplied credentials in the Web Service proxy.
		/// </summary>
		/// <returns>An attribution in the form of domain\username or null if the user isn't authenticated.</returns>
		// TODO: make this into something real, perhaps requiring the client to send a new Author object.
		[WebMethod]
		public string CanEdit()
		{
			string visitorIdentityString = null;
			
			return GetVisitorIdentity(visitorIdentityString);
		}

		/// <summary>
		/// Returns all the namespaces in the Federation.
		/// </summary>
		/// <returns>A ContentBaseCollection of all the ContentBases for the Federation.</returns>
		[WebMethod]
		public ContentBaseCollection GetAllNamespaces()
		{
			ContentBaseCollection contentBases = new ContentBaseCollection();

			foreach (ContentBase cb in TheFederation.ContentBases)
			{
				contentBases.Add(cb);
			}

			return contentBases;
		}

		/// <summary>
		/// Returns the default namespace in the Federation. 
		/// </summary>
		/// <returns>A ContentBase of the default namespace.</returns>
		[WebMethod]
		public ContentBase GetDefaultNamespace()
		{
			if (TheFederation.DefaultContentBase == null)
				throw new Exception("No default namespace defined in configuration file: " + TheFederation.FederationNamespaceMapFilename);

			return TheFederation.DefaultContentBase;
		}

		/// <summary>
		/// Returns the AbsoluteTopicNames for a given Namespace.
		/// </summary>
		/// <param name="cb">The ContentBase.</param>
		/// <returns>A AbsoluteTopicNameCollection of the AbsoluteTopicNames for the ContentBase</returns>
		[WebMethod]
		public AbsoluteTopicNameCollection GetAllTopics(ContentBase cb)
		{
			AbsoluteTopicNameCollection topicNames = new AbsoluteTopicNameCollection();
			 
			foreach (AbsoluteTopicName ab in TheFederation.ContentBaseForNamespace(cb.Namespace).AllTopics(false))
			{
        // We need to also return the current version
        ContentBase cb2 = TheFederation.ContentBaseForTopic(ab); 
        AbsoluteTopicName atn = new AbsoluteTopicName(ab.Name, ab.Namespace); 
        string version = cb2.LatestVersionForTopic(ab);
        if (version == null)
        {
          // There's only one version, so just use the empty string
          version = ""; 
        }
        atn.Version = version; 
				topicNames.Add(atn);
			}

			return topicNames;
		}

		/// <summary>
		/// Returns the formatted HTML for a given Topic.
		/// </summary>
		/// <param name="topicName">An AbsoluteTopicName.</param>
		/// <returns>Formatted HTML string.</returns>
		[WebMethod]
		public string GetHtmlForTopic(AbsoluteTopicName topicName)
		{
			return InternalGetHtmlForTopic(topicName, null);
		}

		/// <summary>
		/// Returns the formatted HTML for a previous version of a given Topic. 
		/// </summary>
		/// <param name="topicName">An AbsoluteTopicName.</param>
		/// <param name="version">The version string to return. The list of known version strings for a given Topic can be obtained by calling <see cref="GetVersionsForTopic"/>.</param>
		/// <returns>Formatted HTML string.</returns>
		[WebMethod]
		public string GetHtmlForTopicVersion(AbsoluteTopicName topicName, string version)
		{
			return InternalGetHtmlForTopic(topicName, version);
		}

		/// <summary>
		/// Returns the raw Text for a version of a given Topic. 
		/// </summary>
		/// <param name="topicName">An AbsoluteTopicName.</param>
		/// <returns>Raw Text.</returns>
		[WebMethod]
		public string GetTextForTopic(AbsoluteTopicName topicName)
		{
			string content = null;
			// OmarS: Do I need to check for Topic existence?
			if (TheFederation.ContentBaseForTopic(topicName).TopicExists(topicName))
				content = TheFederation.ContentBaseForTopic(topicName).Read(topicName);
			if (content == null)
				content = "[enter your text here]";

			return content;
		}

		/// <summary>
		/// Sets the text for a given Topic.
		/// </summary>
		/// <param name="topicName">An AbsoluteTopicName.</param>
		/// <param name="postedTopicText">The new unformatted text.</param>
		/// <param name="visitorIdentityString">The visitor identity string.</param>
		[WebMethod]
		public void SetTextForTopic(AbsoluteTopicName topicName, string postedTopicText, string visitorIdentityString)
		{
			WriteNewTopic(topicName, postedTopicText, GetVisitorIdentity(visitorIdentityString), null);
		}

		/// <summary>
		/// Returns a collection of versions for a given Topic.
		/// </summary>
		/// <param name="topicName">An AbsoluteTopicName.</param>
		/// <returns>StringCollection of version strings.</returns>
		[WebMethod]
		public StringCollection GetVersionsForTopic(AbsoluteTopicName topicName)
		{
			StringCollection topicVersions = new StringCollection();
			
			IEnumerable changeList;
			changeList = TheFederation.ContentBaseForTopic(topicName).AllChangesForTopic(topicName);

			foreach (TopicChange change in changeList)
			{
				topicVersions.Add(change.Version);
			}

			return topicVersions;
		}

		/// <summary>
		/// Returns the formatted HTML version of the given text for a Topic.
		/// </summary>
		/// <param name="topicName">An AbsoluteTopicName.</param>
		/// <param name="textToFormat">The text to format.</param>
		/// <returns>Formatted HTML string.</returns>
		[WebMethod]
		public string GetPreviewForTopic(AbsoluteTopicName topicName, string textToFormat)
		{
			_linkMaker = new LinkMaker(RootUrl(Context.Request));

			// OmarS: why do I have to do this?
			ContentBase relativeToBase = TheFederation.ContentBaseForNamespace(topicName.Namespace);
			
			return FlexWiki.Formatting.Formatter.FormattedString(textToFormat, Formatting.OutputFormat.HTML,  relativeToBase, _linkMaker, null);
		}

		/// <summary>
		/// Restores a given Topic to a previous version.
		/// </summary>
		/// <param name="topicName">An AbsoluteTopicName.</param>
		/// <param name="postedTopicText">The new unformatted text.</param>
		/// <param name="visitorIdentityString">The visitor identity string.</param>
		[WebMethod]
		public void RestoreTopic(AbsoluteTopicName topicName, string visitorIdentityString, string version)
		{
			if (version != null && version == topicName.Version)
			{
				throw new Exception("Version not found");
			}
			else
			{
				IEnumerable changeList;
				changeList = TheFederation.ContentBaseForTopic(topicName).AllChangesForTopic(topicName);

				foreach (TopicChange change in changeList)
				{
					if (change.Version == version)
					{
						WriteNewTopic(change.Topic, TheFederation.ContentBaseForTopic(topicName).Read(change.Topic), visitorIdentityString, version);
						break;
					}
				}
			}
		}

		private string InternalGetHtmlForTopic(AbsoluteTopicName topicName, string version)
		{
			_linkMaker = new LinkMaker(RootUrl(Context.Request));

			if (version != null && version == topicName.Version)
			{
				return FlexWiki.Formatting.Formatter.FormattedTopic(topicName, Formatting.OutputFormat.HTML, false,  TheFederation, _linkMaker, null);
			}
			else
			{
				IEnumerable changeList;
				changeList = TheFederation.ContentBaseForTopic(topicName).AllChangesForTopic(topicName);

				foreach (TopicChange change in changeList)
				{
					if (change.Version == version)
					{
						return FlexWiki.Formatting.Formatter.FormattedTopic(change.Topic, Formatting.OutputFormat.HTML, false,  TheFederation, _linkMaker, null);
					}
				}

				return FlexWiki.Formatting.Formatter.FormattedTopic(topicName, Formatting.OutputFormat.HTML, false,  TheFederation, _linkMaker, null);
			}
		}
		private void WriteNewTopic(AbsoluteTopicName theTopic, string postedTopicText, string visitorIdentityString, string version)
		{
			_linkMaker = new LinkMaker(RootUrl(Context.Request));

			AbsoluteTopicName newVersionName = new AbsoluteTopicName(theTopic.Name, theTopic.Namespace);
			newVersionName.Version = TopicName.NewVersionStringForUser(visitorIdentityString);
			TheFederation.ContentBaseForTopic(newVersionName).WriteTopicAndNewVersion(newVersionName, postedTopicText);
		}

		private string RootUrl(HttpRequest req)
		{
			string full = req.Url.ToString();
			if (req.Url.Query != null && req.Url.Query.Length > 0)
			{
				full = full.Substring(0, full.Length - req.Url.Query.Length);
			}
			if (req.PathInfo != null && req.PathInfo.Length > 0)
			{
				full = full.Substring(0, full.Length - (req.PathInfo.Length + 1));
			}
			full = full.Substring(0, full.LastIndexOf('/') + 1);
			return full;
		}

		private void EstablishFederation()
		{
			if (TheFederation != null)
			{
				// If we have one, just make sure it's valid
				TheFederation.Validate();
				return;
			}

			// nope - need a new one
			string federationNamespaceMap = ConfigurationSettings.AppSettings["FederationNamespaceMapFile"];
			if (federationNamespaceMap == null)
				throw new Exception("No namespace map file defined.  Please set the FederationNamespaceMapFile key in <appSettings> in web.config to point to a namespace map file.");
			string fsPath = Context.Request.MapPath(federationNamespaceMap);
			TheFederation = new Federation(fsPath, FlexWiki.Formatting.OutputFormat.HTML, new LinkMaker(RootUrl(Context.Request)));
		}
		private string GetVisitorIdentity(string visitorIdentityString)
		{
			// if we are using Windows Authenticaiton, override the attribution with the Windows domain/username
			if (User.Identity.IsAuthenticated)
				return User.Identity.Name;
			else if (visitorIdentityString == null || visitorIdentityString.Length == 0)
			{
				return Context.Request.UserHostAddress;
			}
			else
			{
				return visitorIdentityString;
			}
		}
	}
}
