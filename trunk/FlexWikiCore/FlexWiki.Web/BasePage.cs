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
using System.Text.RegularExpressions;
using System.Web.UI;
using System.IO;
using System.Configuration;
using System.Web;
using FlexWiki;
using FlexWiki.Newsletters;
using FlexWiki.Formatting;
using System.Collections;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for Page.
	/// </summary>
	public class BasePage : System.Web.UI.Page
	{
		public BasePage()
		{
			Load += new EventHandler(Page_Load);
		}

		/// <summary>
		/// Escape the given string for special HTML characters (greater-than, etc.).
		/// </summary>
		/// <param name="input"></param>
		/// <returns>The new string</returns>
		static protected string EscapeHTML(string input)
		{
			if (input == null)
				return "";
			// replace HTML special characters with character entities
			// this has the side-effect of stripping all markup from text
			string str = input;
			str = Regex.Replace (str, "&", "&amp;") ;
			str = Regex.Replace (str, "\"", "&quot;") ;
			str = Regex.Replace (str, "<", "&lt;") ;
			str = Regex.Replace (str, ">", "&gt;") ;
			return str;
		}


		protected ContentBase DefaultContentBase
		{
			get
			{
				return TheFederation.ContentBaseForNamespace(DefaultNamespace);
			}
		}

		protected string DefaultNamespace
		{
			get
			{
				return TheFederation.DefaultNamespace;
			}
		}

		protected LinkMaker TheLinkMaker
		{
			get
			{
				return _LinkMaker;
			}
		}

		protected string RootUrl(HttpRequest req)
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

			return full + RelativeBase;
		}

		protected virtual string RelativeBase
		{
			get
			{
				return "";
			}
		}


		protected AbsoluteTopicName GetTopicName()
		{
			string topic = Request.PathInfo;

			RelativeTopicName rel;
			if (topic == null || topic.Length == 0)
				rel = new RelativeTopicName(DefaultContentBase.HomePage, DefaultContentBase.Namespace);
			else
				rel = new RelativeTopicName(topic.Substring(1));
			IList topics = DefaultContentBase.AllAbsoluteTopicNamesThatExist(rel);
			if (topics.Count == 0)
				return rel.AsAbsoluteTopicName(DefaultContentBase.Namespace);		// topic doesn't exist, assume in the wiki's home content base
			if (topics.Count > 1)
			{
				throw TopicIsAmbiguousException.ForTopic(rel);
			}
			AbsoluteTopicName answer = (AbsoluteTopicName)topics[0];
			answer.Version = rel.Version;
			return answer;
		}

		/// <summary>
		/// Add a visitor event to the session state VisitorEvents
		/// </summary>
		/// <param name="e"></param>
		protected void LogVisitorEvent(VisitorEvent e)
		{
			ArrayList list = (ArrayList)(System.Web.HttpContext.Current.Session["VisitorEvents"]);
			if (list == null)
			{
				list = new ArrayList();
				System.Web.HttpContext.Current.Session["VisitorEvents"] = list;
			}
			list.Insert(0, e);	// Adding to the front means we're keeping it sorted with latest first    
		}

		/// <summary>
		/// Answer a string to identify the current visitor.  Is authentication is up and the user is authenticated, answer the
		/// authenticated user's name (e.g., a Windows accoutn name).  Otherwise answer the IP address of the visitor (possibly 
		/// with a user specified prefix).
		/// </summary>
		public string VisitorIdentityString
		{
			get
			{
				if (User.Identity.IsAuthenticated)
					return User.Identity.Name;
				string prefix = _UserPrefix;
				if (prefix == null)
					return Request.UserHostAddress;
				return prefix + "-" + Request.UserHostAddress;
			}
		}

		string _UserPrefix;

		protected string UserPrefix
		{
			get
			{
				return _UserPrefix;
			}
		}

		public static void Spit(string s)
		{
			//			if (_Response != null)
			//				_Response.Write(s + "<br>");
		}

		[ThreadStatic]
		static HttpResponse _Response;

		private LinkMaker	_LinkMaker;
		
		protected void ResetFederation()
		{
			TheFederation = null;
		}

		protected Federation TheFederation
		{
			get
			{
				return (Federation)(Application[FederationCacheKey]);
			}
			set
			{
				Application[FederationCacheKey] = value;
			}
		}

		CacheManager _CacheManager;
		protected CacheManager CacheManager
		{
			get
			{
				if (_CacheManager != null)
					return _CacheManager;
				_CacheManager = new CacheManager(Page.Cache);
				return _CacheManager;
			}
		}

		static string FederationCacheKey = "---FEDERATION---";
		
		protected NewsletterDaemon TheNewsletterDaemon
		{
			get
			{
				return (NewsletterDaemon)(Application[NewsletterDaemonCacheKey]);
			}
			set
			{
				Application[NewsletterDaemonCacheKey] = value;
			}
		}

		static string NewsletterDaemonCacheKey = "---NEWSLETTERDAEMON---";


		void EstablishFederation()
		{
			if (TheFederation != null)
			{
				// If we have one, just make sure it's valid
				TheFederation.Validate();
				return;
			}

			// nope - need a new one
			string federationNamespaceMap = FederationNamespaceMapPath;
			if (federationNamespaceMap == null)
				throw new Exception("No namespace map file defined.  Please set the FederationNamespaceMapFile key in <appSettings> in web.config to point to a namespace map file.");
			string fsPath = MapPath(federationNamespaceMap);
			TheFederation = new Federation(fsPath, OutputFormat.HTML, TheLinkMaker);
			// Give the federation a cache to work with 
			TheFederation.FederationCache = CacheManager;
		}

		protected string FederationNamespaceMapPath
		{
			get
			{
				return ConfigurationSettings.AppSettings["FederationNamespaceMapFile"];
			}
		}


		void EstablishNewsletterDaemon(string SMTPServer, string SMTPUser, string SMTPPassword, string newslettersFrom)
		{
			if (TheNewsletterDaemon != null)
			{
				// If we have one, just make sure it's alive
				TheNewsletterDaemon.EnsureRunning();
				return;
			}

			// Collect up the styles that need to be inserted into the newsletters.  Do this by reading the wiki.css file and override css file (if there is one)
			string styles = "";
			try
			{
				string mainStylesFile = MapPath("wiki.css");
				using (TextReader s = new StreamReader(mainStylesFile))
				{
					styles += s.ReadToEnd();
				}
				string styleOverride = ConfigurationSettings.AppSettings["OverrideStylesheet"];
				if (styleOverride != null && styleOverride != "")
				{
					string overrideCSSFile = MapPath(styleOverride);
					using (TextReader s = new StreamReader(overrideCSSFile))
					{
						styles += s.ReadToEnd();
					}
				}
			}
			catch (FileNotFoundException ex)
			{
        // This call will compile away under release builds, but still
        // gives us a place to set a breakpoint and a variable to examine
        // under debug builds
				System.Diagnostics.Debug.WriteLine(ex.ToString()); 
			}
			styles = @"<style>" + styles + @"
</style>";

      bool sendAsAttachments = false; 
      try
      {
        sendAsAttachments = bool.Parse(ConfigurationSettings.AppSettings["SendNewslettersAsAttachments"]); 
      }
      catch (Exception ex)
      {
        // This call will compile away under release builds, but still
        // gives us a place to set a breakpoint and a variable to examine
        // under debug builds
        System.Diagnostics.Debug.WriteLine(ex.ToString()); 
      }

			NewsletterDaemon daemon = new NewsletterDaemon(TheFederation.FederationNamespaceMapFilename, TheLinkMaker.SiteURL(), 
        newslettersFrom, styles, sendAsAttachments);
			TheNewsletterDaemon = daemon;
			daemon.SMTPServer = SMTPServer;
			daemon.SMTPUser = SMTPUser;
			daemon.SMTPPassword = SMTPPassword;
			daemon.LogEventFactory = TheFederation.LogEventFactory;
			daemon.EnsureRunning();
		}

		protected string MainStylesheetReference()
		{
			return "<LINK href='" +RootUrl(Request) + "wiki.css' type='text/css' rel='stylesheet'>";
		}

		protected string InsertStylesheetReferences()
		{
			string answer = MainStylesheetReference();
			AbsoluteTopicName abs = GetTopicName();
			string styleSheet = null;
			if (abs.Namespace != null)
				styleSheet = TheFederation.GetTopicProperty(abs, "Stylesheet");

			if (styleSheet != null && styleSheet.Length > 0)
			{
				answer += "\n<LINK href='" + styleSheet + "' type='text/css' rel='stylesheet'>";
			}
			else
			{
				string styleOverride = ConfigurationSettings.AppSettings["OverrideStylesheet"];
				if (styleOverride != null && styleOverride.Length > 0)
				{
					answer += "\n<LINK href='" + styleOverride + "' type='text/css' rel='stylesheet'>";
				}
			}

			return answer;
		}

		private void Page_Load(object sender, EventArgs e)
		{
			// Setup the federation -- either find the existing one or create a new one
			_LinkMaker = new LinkMaker(RootUrl(Request));
			EstablishFederation();

			_Response = Response;

			// Make sure we've setup a LogEventFactory for the federation
			string logFile = ConfigurationSettings.AppSettings["LogPath"];
			string relativeTo = Server.MapPath("~");
			if (logFile != null)
			{
			  logFile = System.IO.Path.Combine(relativeTo, logFile); 
			}
			ILogEventFactory factory = TheFederation.LogEventFactory;
			if (factory == null)
				TheFederation.LogEventFactory = new WebApplicationLogEventFactory(Application, logFile);
			
			string ns = TheFederation.DefaultNamespace;
			if (ns == null)
                throw new Exception("No default namespace defined in configuration file: " + TheFederation.FederationNamespaceMapFilename);

			ContentBase cb  = TheFederation.ContentBaseForNamespace(ns);
			if (cb == null)
				throw new Exception("Default namespace (" + ns + ") doesn't exist.");
			TheFederation.DefaultNamespace = ns;

			EstablishNewsletterDaemon(
				ConfigurationSettings.AppSettings["SMTPServer"], 
				ConfigurationSettings.AppSettings["SMTPUser"], 
				ConfigurationSettings.AppSettings["SMTPPassword"],
				ConfigurationSettings.AppSettings["NewslettersFrom"]);

			ProcessUnauthenticatedUserName();
		}


		protected static void OpenPane(TextWriter w, string title)
		{
			OpenPane(w, title, null, null, null);
		}

		protected static void OpenPane(TextWriter w, string title, string imageURL, string imageLink, string imageTip)
		{
			w.Write("<table cellspacing='0' class='SidebarTile' cellpadding='2' border='0'>");
			w.Write("<tr><td valign='middle' class='SidebarTileTitle' >");
			if (imageURL != null)
			{
				if (imageLink != null)
					w.Write("<a href='" + imageLink + "'>");
				w.Write("<img align='absmiddle' border='0' style='margin-left: 1px; margin-right: 5px'  src='" + imageURL + @"' ");
				if (imageTip != null)
					w.Write("title='" + imageTip + "'");
				w.Write(">");
				if (imageLink != null)
					w.Write("</a>");
			}
			if (imageLink == null)
				w.Write(title);
			else
				w.Write("<a href=\"" + imageLink + "\">" + title + "</a>");
			w.Write("</td></tr>");
			w.Write("<tr class='SidebarTileBody'><td>");
		}

		protected static void ClosePane(TextWriter w)
		{
			w.Write("</td></tr>");
			w.Write("</table>");
		}


		/// <summary>
		/// Restores the passed in Topic version as the current version
		/// </summary>
		/// <param name="topic">Topic Version to Restore</param>
		/// <returns></returns>
		protected TopicName RestorePreviousVersion(AbsoluteTopicName topic)
		{
			LogEvent e = TheFederation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, topic.ToString(), LogEvent.LogEventType.WriteTopic);
			try
			{
				AbsoluteTopicName newVersionName = new AbsoluteTopicName(topic.Name, topic.Namespace);
				newVersionName.Version = TopicName.NewVersionStringForUser(VisitorIdentityString);
				DefaultContentBase.WriteTopicAndNewVersion(newVersionName, this.DefaultContentBase.Read(topic));
			}
			finally
			{
				e.Record();
			}
			return new AbsoluteTopicName(topic.Name, topic.Namespace);
		}


		/// <summary>
		/// Load the user's identity prefix from the incoming web request (and change it also if so requested).
		/// </summary>
		void ProcessUnauthenticatedUserName()
		{
			// First set the local value to whatever is in the incoming cookies
			HttpCookie incoming = Request.Cookies["User"];
			if (incoming != null)
				_UserPrefix = incoming.Value;

			// Now see if there's an incoming form field that changes this (only if we're non-authenticated)
			if (!Request.IsAuthenticated)
			{
				string pre = null;
				try 
				{
					string up = Request.Form["UserSuppliedName"];
					pre = up;
				}
				catch (ArgumentOutOfRangeException ex)
				{
					ex.ToString();	// quiet, compiler!
				}
				if (pre != null)
				{
					if (_UserPrefix != pre)
					{
						_UserPrefix = pre;
						HttpCookie cookie = new HttpCookie("User", pre);
						DateTime dt = DateTime.Now;
						TimeSpan ts = new TimeSpan(100, 0, 0, 0, 0);		// make it stick for 100 days
						cookie.Expires = dt.Add(ts);
						Response.Cookies.Add(cookie);
					}
				}
			}

			System.Web.HttpContext.Current.Items["VisitorIdentityString"] =VisitorIdentityString;
		}

	}
}
