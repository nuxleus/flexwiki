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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using FlexWiki.Formatting;
using System.Diagnostics;
using System.Xml;
using System.Xml.Xsl;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("ClassicBehaviors", "A collection of behaviors supported before WikiTalk was invented; mostly for backwards-compatibility")]
	public class ClassicBehaviors : BELObject
	{
		public ClassicBehaviors() : base()
		{
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

		public override string ToString()
		{
			return "classic behaviors object";
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer the name of this product (FlexWiki)")]
		public string ProductName
		{
			get
			{
				return "FlexWiki";
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer a formatted error message")]
		public IBELObject ErrorMessage(string title, string body)
		{
			return new ErrorMessage(title, body);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer a newline character")]
		public string Newline
		{
			get
			{
				return Environment.NewLine;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer a tab character")]
		public string Tab
		{
			get
			{
				return "\t";
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer the exact version number of this software")]
		public string ProductVersion
		{
			get
			{
				return FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNever, "Answer the current date and time")]
		public DateTime Now
		{
			get
			{
				return DateTime.Now;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyComplex, "Answer a list of all namespaces in the federation (with details)")]
		public string AllNamespacesWithDetails(ExecutionContext ctx)
		{
			ArrayList files = new ArrayList();
			StringBuilder b = new StringBuilder();
			ArrayList bases = new ArrayList();
			Federation fed =ctx.CurrentFederation;
			foreach (string ns in fed.Namespaces)
			{
				ContentBase cb = fed.ContentBaseForNamespace(ns);
				bases.Add(cb);
				ctx.AddCacheRule(cb.CacheRuleForDefinition);
			}
			// And now add the namespace map itself
			ctx.AddCacheRule(new FilesCacheRule(fed.FederationNamespaceMapFilename));
			bases.Sort();
			foreach (ContentBase info in bases)
			{
				b.Append(
					"||\"" + info.FriendlyTitle + "\":" + info.Namespace + "." + info.HomePage +
					"||" + info.Description + 
					"||" + (info.Contact == null ? "" : info.Contact) + 
					"||\n");
			}

			return b.ToString();
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNever, "Answer a list of topics that match the given criteria.")]
		public string TopicIndex(ExecutionContext ctx, string type, [ExposedParameter(true)] string arg2, [ExposedParameter(true)] string topicNamespace)
		{
			InvocationFrame frame = ctx.TopFrame;
			bool isTitleType;
			switch (type)
			{
				case "Title":
					isTitleType = true;
					break;

				case "Property":
					isTitleType = false;
					break;

				default:
					throw (new ArgumentException("Type must be either 'Title' or 'Property' for TopicIndex"));
			}

			if (!isTitleType && !frame.WasParameterSupplied(2))
			{
				throw (new ArgumentException("For 'Property' type of TopicIndex, property name must be supplied as second parameter"));				
			}

			string result = "";
			ArrayList uniqueNamespaces;
			if(topicNamespace == null)
			{
				uniqueNamespaces = new ArrayList(ctx.CurrentFederation.Namespaces);
				uniqueNamespaces.Sort();
			}
			else
			{
				uniqueNamespaces = new ArrayList(1);
				uniqueNamespaces.Add(topicNamespace);
			}

			Regex titleFilter = null;
			if(isTitleType && arg2 != null)
				titleFilter = new Regex(arg2, RegexOptions.Singleline | RegexOptions.Compiled);
			foreach (string ns in uniqueNamespaces)
			{
				ContentBase cb = ctx.CurrentFederation.ContentBaseForNamespace(ns);
				if (cb == null)
					continue;
				foreach(AbsoluteTopicName topic in cb.AllTopics(false))
				{
					if(topic.Name.StartsWith("_") || (ctx.CurrentTopicName != null && topic.Fullname == ctx.CurrentTopicName.Fullname)) // no sense listing the page we're currently viewing
						continue;
					if (isTitleType)
					{
						if(titleFilter == null || titleFilter.IsMatch(topic.Name))
						{
							result += "\t* \"" + topic.Fullname + "\":" + topic.Namespace + ".[" + topic.Name + "]" + Environment.NewLine;
						}
						string topicSummary = ctx.CurrentFederation.GetTopicProperty(topic, "Summary");
						if(topicSummary.Length > 0)
							result += "\t\t*" + topicSummary + Environment.NewLine;
					}
					else
					{
						string topicProperty = ctx.CurrentFederation.GetTopicProperty(topic, arg2);
						if(topicProperty.Length > 0)
						{
							result += "\t* \"" + topic.Fullname + "\":" + topic.Namespace + ".[" + topic.Name + "]" + Environment.NewLine + "\t\t* " + topicProperty + Environment.NewLine;
						}
						break;
					}
				}
			}
			return result;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNever, "Answer the result of transforming the given XML using the given transform")]
		public string XmlTransform(string xmlURL, string xslURL)
		{
			string result= null;
			XmlDocument xmlDoc = new XmlDocument();
			XslTransform xslTransform = new XslTransform();
			
			try
			{
				// Load the XML file
				xmlDoc.Load(xmlURL);
			}
			catch(Exception ex)
			{
				result = "Failed to load XML parameter (" + xmlURL + "): " + ex.Message;
			}
			if (result==null)
			{
				try
				{
					// Load the XSL File
					xslTransform.Load(xslURL);
				}
				catch(Exception ex)
				{
					result = "Failed to load XSL parameter (" + xslURL + "): " + ex.Message;
				}
			}
			if (result==null)
			{
				try
				{
					StringWriter sW = new StringWriter();
					XmlTextWriter XmlW = new XmlTextWriter(sW);
					xslTransform.Transform(xmlDoc,new XsltArgumentList(),XmlW,null);
					result = sW.ToString();
					XmlW.Close();
				}
				catch(Exception ex)
				{
					result = "Failed to Transform " + ex.Message;
				}
			}
			return result;
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNever, "Present the given image with the supplied additiona information")]
		public IBELObject Image(ExecutionContext ctx, string URL, string altText, [ExposedParameter(true)] string width, [ExposedParameter(true)]string height)
		{
			return new ImagePresentation(altText, URL, null, height, width);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyComplex  | ExposedMethodFlags.NeedContext, "Answer the value of the given property")]
		public string Property(ExecutionContext ctx, string topic, string property)
		{
			AbsoluteTopicName abs = null;
			bool ambig = false;
			string answer = null;
			try
			{
				ContentBase cb = ctx.CurrentContentBase;
				if (cb == null)
					cb = ctx.CurrentFederation.DefaultContentBase;
				RelativeTopicName rel = new RelativeTopicName(topic);
				ctx.AddCacheRule(cb.CacheRuleForAllPossibleInstancesOfTopic(rel));
				abs = cb.UnambiguousTopicNameFor(rel);
			}
			catch (TopicIsAmbiguousException)
			{
				ambig = true;
			}
			if (abs != null)
			{
				// Got a unique one!
				answer = ctx.CurrentFederation.GetTopicProperty(abs, property);
			}
			else
			{
				if (ambig)
					throw new ArgumentException("Ambiguous topic name: " + topic);
				else
					throw new ArgumentException("Unknown topic name: " + topic);
			}
			return answer;
		}

		// TODO - This method violates the rules: no behavior should ever directly output presentation strings.  To fix this,
		// we need a BELHyperlinkPresentation object that this behavior can produce and that can do IOutputSequence...
		[ExposedMethod(ExposedMethodFlags.CachePolicyNever | ExposedMethodFlags.AllowsVariableArguments, "Link to the given interWiki")]
		public IBELObject InterWiki(ExecutionContext ctx, string interWikiName, string linkText)
		{
			Hashtable map = ctx.ExternalWikiMap;
			Federation fed = ctx.CurrentFederation;

			string result = null;
			string safeName = Formatter.EscapeHTML(interWikiName);
			string safeLinkText = Formatter.EscapeHTML(linkText);
			if (map != null)
			{
				foreach(DictionaryEntry extWiki in map)
				{
					if ( extWiki.Key.ToString().ToUpper() == interWikiName.ToUpper() )
					{
						result = "<a class=ExternalLink title=\"External link to " + safeName + "\" target=\"ExternalLinks\" href=\"" + extWiki.Value.ToString().Replace("$$$", safeLinkText) + "\">" + safeLinkText + "</a>";
						break;
					}
				}
			}

			if(result == null)
			{
				string interWikisTopic = System.Configuration.ConfigurationSettings.AppSettings["InterWikisTopic"];
				if ( interWikisTopic == null )
				{
					interWikisTopic = "_InterWikis";
				}

				AbsoluteTopicName topic = new AbsoluteTopicName(interWikisTopic, fed.DefaultNamespace);
				if (!fed.DefaultContentBase.TopicExists(topic) )
				{
					throw new ArgumentException("Failed to find InterWikisTopic [" + interWikisTopic + "].");
				}

				Hashtable props = fed.DefaultContentBase.GetFieldsForTopic(topic);
				if ( props != null)
				{
					foreach( DictionaryEntry entry in props )
					{
						if ( entry.Key.ToString().ToUpper() == interWikiName.ToUpper() )
						{
							result = "<a class=ExternalLink title=\"External link to " + safeName + "\" target=\"ExternalLinks\" href=\"" + entry.Value.ToString().Replace("$$$", safeLinkText) + "\">" + safeLinkText +"</a>";
						}
					}
				}

				if ( result == null )
				{
					throw new ArgumentException("Failed to find InterWiki '" + safeName + "'.");
				}
			}

			for ( int p = 0; p < ctx.TopFrame.ExtraArguments.Count; p++ )
			{
				string replacementArg = Formatter.EscapeHTML(ctx.TopFrame.ExtraArguments[p] == null ? "null" : ctx.TopFrame.ExtraArguments[p].ToString());
				result = result.Replace("$" + (p + 1).ToString(), replacementArg);
			}

			return new StringPresenation(result);
		}




	}
}
				
		
