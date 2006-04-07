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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Xml;

using FlexWiki.Newsletters;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for Rss.
	/// </summary>
	public class Rss : BasePage
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		protected void DoSearch()
		{
			string preferredNamespace = Request.QueryString["namespace"];
			string newsletterName = Request.QueryString["newsletter"];

			XmlTextWriter newsletter = new XmlTextWriter(Response.Output);

			newsletter.Formatting = System.Xml.Formatting.Indented;

			if (newsletterName != null) 
			{
				NewsletterFeed(newsletterName, newsletter);
			}
			else
			{
				if (preferredNamespace == null) 
				{
					preferredNamespace  = DefaultNamespace;
				}
				NamespaceFeed(preferredNamespace, newsletter);
			}
		}

		void NewsletterFeed(string newsletterName, XmlTextWriter newsletter)
		{
			NewsletterManager nm = new NewsletterManager(TheFederation, TheLinkMaker);
			TopicInfo info = TheFederation.GetTopicInfo(newsletterName);
			if (!info.Exists) 
			{
				throw new Exception("Newsletter " +  newsletterName + "  does not exist.");
			}
			if (!info.HasProperty("Topics")) 
			{
				throw new Exception("Topic " +  newsletterName + " is not a newsletter; no Topics property defined.");
			}
			string desc = info.GetProperty("Description");
			if (desc == null) 
			{
				desc = "";
			}

			newsletter.WriteStartDocument();
			newsletter.WriteStartElement("rss");
			newsletter.WriteAttributeString("version", "2.0");
			newsletter.WriteStartElement("channel");
			newsletter.WriteElementString("title", newsletterName);
			newsletter.WriteElementString("description", desc);

			Uri link = new Uri(new Uri(FullRootUrl(Request)), TheLinkMaker.LinkToTopic(info.Fullname, true), false);
			newsletter.WriteElementString("link", link.AbsoluteUri);

			DateTime last = DateTime.MinValue;
			foreach (AbsoluteTopicName topic in nm.AllTopicsForNewsletter(info.Fullname))
			{
				FormatRSSItem(topic, newsletter);
				TopicInfo each = new TopicInfo(TheFederation, topic);
				DateTime lm = each.LastModified;
				if (lm > last) 
				{
					last = lm;
				}
			}

			newsletter.WriteElementString("lastBuildDate", last.ToUniversalTime().ToString("r"));

			newsletter.WriteEndElement();
			newsletter.WriteEndElement();
		}


		void NamespaceFeed(string preferredNamespace, XmlWriter newsletter)
		{
			bool inherited = "y".Equals(Request.QueryString["inherited"]);
			
			ContentBase cb = TheFederation.ContentBaseForNamespace(preferredNamespace);

			newsletter.WriteStartDocument();
			newsletter.WriteStartElement("rss");
			newsletter.WriteAttributeString("version", "2.0");
			newsletter.WriteStartElement("channel");
			newsletter.WriteElementString("title", (cb.Title != null ? cb.Title : cb.Namespace));
			newsletter.WriteElementString("description", cb.Description);

			Uri link = new Uri(
        new Uri(FullRootUrl(Request)),
				TheLinkMaker.LinkToTopic(
				  new AbsoluteTopicName(
				    preferredNamespace + 
				    "." + 
				    TheFederation.ContentBaseForNamespace(preferredNamespace).HomePage
				  ),
			  	true
				),
				false
			);

			newsletter.WriteElementString("link", link.AbsoluteUri);

			newsletter.WriteElementString(
				"lastBuildDate", 
				cb.LastModified(true).ToUniversalTime().ToString("r")
				);

			foreach (AbsoluteTopicName topic in cb.AllTopics(inherited)) {
				FormatRSSItem(topic, newsletter);
			}

			newsletter.WriteEndElement();
			newsletter.WriteEndElement();
		}

		/// <summary>
		/// Answer a formatted RSS <item> for the given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <returns></returns>
		public void FormatRSSItem(AbsoluteTopicName topic, XmlWriter newsletter)
		{
			ContentBase cb = TheFederation.ContentBaseForNamespace(topic.Namespace);

			IEnumerable changes = TheFederation.GetTopicChanges(topic);

			IEnumerator e = changes.GetEnumerator();
			if (!e.MoveNext())
				return;	// No history!

			newsletter.WriteStartElement("item");
			newsletter.WriteElementString("title", topic.Name);
			
			newsletter.WriteStartElement("description");
			FormatRSSTopicHistory(topic, false, newsletter, changes);
			newsletter.WriteEndElement();
			
			newsletter.WriteStartElement("body");
			newsletter.WriteAttributeString("xmlns", @"http://www.w3.org/1999/xhtml");
			FormatRSSTopicHistory(topic, true, newsletter, changes);
			newsletter.WriteEndElement();

			newsletter.WriteElementString(
				"created", 
				TheFederation.GetTopicCreationTime(topic).ToUniversalTime().ToString("r")
				);
	 
			Uri link = new Uri(new Uri(FullRootUrl(Request)), TheLinkMaker.LinkToTopic(topic, true), false);
			newsletter.WriteElementString("link", link.AbsoluteUri);

			newsletter.WriteElementString(
				"pubDate", 
				TheFederation.GetTopicModificationTime(topic).ToUniversalTime().ToString("r")
				);

			newsletter.WriteElementString("guid", link.ToString());

			newsletter.WriteEndElement();
		}

		static int MaxChanges = 10;

		/// <summary>
		/// Answer the RSS topic history for a given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="useHTML"></param>
		/// <returns></returns>
		void FormatRSSTopicHistory(AbsoluteTopicName topic, bool useHTML, XmlWriter newsletter, IEnumerable changesForThisTopic)
		{
			ArrayList names = new ArrayList();
			Hashtable changeInfo = new Hashtable();		// key = author, value = TopicChange
			int count = 0;
			foreach (TopicChange change in changesForThisTopic)
			{
				if (count >= MaxChanges)
				{
					break;
				}
				count++;
				if (names.Contains(change.Author)) 
				{
					continue;
				}
				names.Add(change.Author);
				changeInfo[change.Author] = change;
			}

			if (count == 0) 
			{
				return;
			}

			if (useHTML)
			{
				Uri link = new Uri(new Uri(FullRootUrl(Request)), TheLinkMaker.LinkToTopic(topic, true), false);
				newsletter.WriteStartElement("a");
				newsletter.WriteAttributeString("title", HTMLWriter.Escape(topic.Fullname));
				newsletter.WriteAttributeString("href", link.AbsoluteUri);
				newsletter.WriteString(HTMLWriter.Escape(topic.Name));
				newsletter.WriteEndElement();
			}
			else
			{
				newsletter.WriteString(HTMLWriter.Escape(topic.Name));
			}

			newsletter.WriteString(
				string.Format(" was most recently changed by: ")
				);

			bool firstName = true;

			if (useHTML)
			{
				newsletter.WriteStartElement("ul");
			}

			foreach (string eachAuthor in names)
			{
				if (useHTML)
				{
					newsletter.WriteStartElement("li");
				}
				else 
				{
					if (!firstName)
					{
						newsletter.WriteString(", ");
					}
				}
				firstName = false;

				newsletter.WriteString(HTMLWriter.Escape(eachAuthor) + " (" + ((TopicChange)(changeInfo[eachAuthor])).Timestamp.ToString() + ")");
				
				if(useHTML)
				{
					newsletter.WriteEndElement();
				}
			}	
			if (useHTML)
			{
				newsletter.WriteEndElement();
				Uri link = new Uri(new Uri(FullRootUrl(Request)), TheLinkMaker.LinkToVersions(topic.ToString()), false);

				newsletter.WriteString("View the ");
				newsletter.WriteStartElement("a");
				newsletter.WriteAttributeString("title", "Versions for " + HTMLWriter.Escape(topic.Fullname));
				newsletter.WriteAttributeString("href", link.AbsoluteUri);
				newsletter.WriteString("complete version history");
				newsletter.WriteEndElement();

				newsletter.WriteStartElement("br");
				newsletter.WriteEndElement();
			}
			else
			{
				newsletter.WriteString("\n");
			}
			return;
		}

	}
}
