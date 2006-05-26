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

using FlexWiki.Web.Newsletters;

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
                    preferredNamespace = DefaultNamespace;
                }
                NamespaceFeed(preferredNamespace, newsletter);
            }
        }

        private void NewsletterFeed(string newsletterName, XmlTextWriter newsletter)
        {
            NewsletterManager nm = new NewsletterManager(Federation, TheLinkMaker);
            TopicVersionInfo info = Federation.GetTopicInfo(newsletterName);
            if (!info.Exists)
            {
                throw new Exception("Newsletter " + newsletterName + "  does not exist.");
            }
            if (!info.HasProperty("Topics"))
            {
                throw new Exception("Topic " + newsletterName + " is not a newsletter; no Topics property defined.");
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

            Uri link = new Uri(new Uri(FullRootUrl(Request)), TheLinkMaker.LinkToTopic(info.TopicRevision, true));
            newsletter.WriteElementString("link", link.AbsoluteUri);

            DateTime last = DateTime.MinValue;
            foreach (TopicName topic in nm.AllTopicsForNewsletter(info.TopicRevision))
            {
                FormatRssItem(topic, newsletter);
                TopicVersionInfo each = new TopicVersionInfo(Federation, new QualifiedTopicRevision(topic));
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


        private void NamespaceFeed(string preferredNamespace, XmlWriter newsletter)
        {
            ImportPolicy importPolicy = ImportPolicy.DoNotIncludeImports;

            if ("y".Equals(Request.QueryString["inherited"]))
            {
                importPolicy = ImportPolicy.IncludeImports;
            }

            NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(preferredNamespace);

            newsletter.WriteStartDocument();
            newsletter.WriteStartElement("rss");
            newsletter.WriteAttributeString("version", "2.0");
            newsletter.WriteStartElement("channel");
            newsletter.WriteElementString("title", (storeManager.Title != null ? storeManager.Title : storeManager.Namespace));
            newsletter.WriteElementString("description", storeManager.Description);

            Uri link = new Uri(
                new Uri(FullRootUrl(Request)),
                TheLinkMaker.LinkToTopic(
                  new QualifiedTopicRevision(
                    preferredNamespace +
                    "." +
                    Federation.NamespaceManagerForNamespace(preferredNamespace).HomePage
                  ),
                true
                ));

            newsletter.WriteElementString("link", link.AbsoluteUri);

            newsletter.WriteElementString(
                "lastBuildDate",
                storeManager.LastModified(ImportPolicy.IncludeImports).ToUniversalTime().ToString("r")
                );

            foreach (TopicName topic in storeManager.AllTopics(importPolicy))
            {
                FormatRssItem(topic, newsletter);
            }

            newsletter.WriteEndElement();
            newsletter.WriteEndElement();
        }

        /// <summary>
        /// Answer a formatted RSS <item> for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public void FormatRssItem(TopicName topic, XmlWriter newsletter)
        {
            NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(topic.Namespace);

            IEnumerable changes = Federation.GetTopicChanges(topic);

            IEnumerator e = changes.GetEnumerator();
            if (!e.MoveNext())
                return;	// No history!

            newsletter.WriteStartElement("item");
            newsletter.WriteElementString("title", topic.LocalName);

            newsletter.WriteStartElement("description");
            FormatRssTopicHistory(topic, false, newsletter, changes);
            newsletter.WriteEndElement();

            newsletter.WriteStartElement("body");
            newsletter.WriteAttributeString("xmlns", @"http://www.w3.org/1999/xhtml");
            FormatRssTopicHistory(topic, true, newsletter, changes);
            newsletter.WriteEndElement();

            newsletter.WriteElementString(
                "created",
                Federation.GetTopicCreationTime(topic).ToUniversalTime().ToString("r")
                );

            Uri link = new Uri(new Uri(FullRootUrl(Request)), TheLinkMaker.LinkToTopic(new QualifiedTopicRevision(topic), 
                true));
            newsletter.WriteElementString("link", link.AbsoluteUri);

            newsletter.WriteElementString(
                "pubDate",
                Federation.GetTopicModificationTime(topic).ToUniversalTime().ToString("r")
                );

            newsletter.WriteElementString("guid", link.ToString());

            newsletter.WriteEndElement();
        }

        private static int MaxChanges = 10;

        /// <summary>
        /// Answer the RSS topic history for a given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="useHTML"></param>
        /// <returns></returns>
        private void FormatRssTopicHistory(TopicName topic, bool useHTML, XmlWriter newsletter, IEnumerable changesForThisTopic)
        {
            ArrayList names = new ArrayList();
            Hashtable changeInfo = new Hashtable();		// key = author, rawValue = TopicChange
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
                Uri link = new Uri(new Uri(FullRootUrl(Request)), TheLinkMaker.LinkToTopic(
                    new QualifiedTopicRevision(topic), true));
                newsletter.WriteStartElement("a");
                newsletter.WriteAttributeString("title", HtmlWriter.Escape(topic.DottedName));
                newsletter.WriteAttributeString("href", link.AbsoluteUri);
                newsletter.WriteString(HtmlWriter.Escape(topic.LocalName));
                newsletter.WriteEndElement();
            }
            else
            {
                newsletter.WriteString(HtmlWriter.Escape(topic.LocalName));
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

                newsletter.WriteString(HtmlWriter.Escape(eachAuthor) + " (" + ((TopicChange) (changeInfo[eachAuthor])).Created.ToString() + ")");

                if (useHTML)
                {
                    newsletter.WriteEndElement();
                }
            }
            if (useHTML)
            {
                newsletter.WriteEndElement();
                Uri link = new Uri(new Uri(FullRootUrl(Request)), TheLinkMaker.LinkToVersions(topic.ToString()));

                newsletter.WriteString("View the ");
                newsletter.WriteStartElement("a");
                newsletter.WriteAttributeString("title", "Versions for " + HtmlWriter.Escape(topic.DottedName));
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
