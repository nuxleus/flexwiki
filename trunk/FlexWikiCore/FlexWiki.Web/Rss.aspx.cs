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

			if (newsletterName != null)
				NewsletterFeed(newsletterName);
			else
			{
				if (preferredNamespace == null)
					preferredNamespace  = DefaultNamespace;
				NamespaceFeed(preferredNamespace);
			}
		}

		void NewsletterFeed(string newsletterName)
		{
			NewsletterManager nm = new NewsletterManager(TheFederation, TheLinkMaker);
			TopicInfo info = TheFederation.GetTopicInfo(newsletterName);
			if (!info.Exists)
				throw new Exception("Newsletter " +  newsletterName + "  does not exist.");
			if (!info.HasProperty("Topics"))
				throw new Exception("Topic " +  newsletterName + " is not a newsletter; no Topics property defined.");
			string desc = info.GetProperty("Description");
			if (desc == null)
				desc = "";

			Response.Write(@"<rss version='2.0' >
<channel>
    <title>" + newsletterName + @"</title>
    <description>" + desc + @"</description>
    <link>" + TheLinkMaker.LinkToTopic(info.Fullname) + @"</link>");

			DateTime last = DateTime.MinValue;
			foreach (AbsoluteTopicName topic in nm.AllTopicsForNewsletter(info.Fullname))
			{
				Response.Write(FormmatedRSSItem(topic));
				TopicInfo each = new TopicInfo(TheFederation, topic);
				DateTime lm = each.LastModified;
				if (lm > last)
					last = lm;
			}

			Response.Write("<lastBuildDate>");
			Response.Write(last.ToUniversalTime().ToString("r"));
			Response.Write("</lastBuildDate>");

			Response.Write(@"</channel>
</rss>");

		}


		void NamespaceFeed(string preferredNamespace)
		{
			bool inherited = "y".Equals(Request.QueryString["inherited"]);
			
			ContentBase cb = TheFederation.ContentBaseForNamespace(preferredNamespace);

			Response.Write(@"<rss version='2.0' >
<channel>
    <title>" + (cb.Title != null ? cb.Title : cb.Namespace) + @"</title>
    <description>" + cb.Description + @"</description>
    <link>" + TheLinkMaker.LinkToTopic(new AbsoluteTopicName(preferredNamespace + "." + TheFederation.ContentBaseForNamespace(preferredNamespace).HomePage)) + @"</link>");

			Response.Write("<lastBuildDate>");
			Response.Write(cb.LastModified(true).ToUniversalTime().ToString("r"));
			Response.Write("</lastBuildDate>");

			foreach (AbsoluteTopicName topic in cb.AllTopicsSortedLastModifiedDescending(inherited))
			{
				Response.Write(FormmatedRSSItem(topic));
			}

			Response.Write(@"</channel>
</rss>");

		}

		/// <summary>
		/// Answer a formatted RSS <item> for the given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <returns></returns>
		public string FormmatedRSSItem(AbsoluteTopicName topic)
		{
			StringBuilder builder = new StringBuilder();
			ContentBase cb = TheFederation.ContentBaseForNamespace(topic.Namespace);

			string textHistory = FormattedRSSTopicHistory(topic, false);

			if (textHistory == null)
				return "";

			string htmlHistory = FormattedRSSTopicHistory(topic, true);

			builder.Append("\n    <item>\n");
            
			builder.Append("      <title>");
			builder.Append(topic.Name);
			builder.Append("</title>\n");
            
			builder.Append("      <description>");
			builder.Append(textHistory);
			builder.Append("</description>\n");
            
			builder.Append("      <body xmlns=\"http://www.w3.org/1999/xhtml\">");
			builder.Append(htmlHistory);
			builder.Append("</body>\n");
            
			builder.Append("      <created>");
			builder.Append(cb.GetTopicCreationTime(topic).ToUniversalTime().ToString("r"));
			builder.Append("</created>\n");
            
			builder.Append("      <link>");
			builder.Append(TheLinkMaker.LinkToTopic(topic));
			builder.Append("</link>\n");
            
			builder.Append("      <pubDate>");
			builder.Append(cb.GetTopicLastWriteTime(topic).ToUniversalTime().ToString("r"));
			builder.Append("</pubDate>\n");
            
			builder.Append("      <guid>");
			builder.Append(TheLinkMaker.LinkToTopic(topic));
			builder.Append("</guid>\n");
            
			builder.Append("    </item>\n");
			
			return builder.ToString();
		}

		/// <summary>
		/// Answer the RSS topic history for a given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <param name="useHTML"></param>
		/// <returns></returns>
		string FormattedRSSTopicHistory(AbsoluteTopicName topic, bool useHTML)
		{
			ContentBase cb = TheFederation.ContentBaseForNamespace(topic.Namespace);

			IEnumerable changesForThisTopic = cb.AllChangesForTopicSince(topic, DateTime.MinValue);
			StringBuilder builder = new StringBuilder();
			ArrayList names = new ArrayList();
			int count = 0;
			foreach (TopicChange change in changesForThisTopic)
			{
				count++;
				if (names.Contains(change.Author))
					continue;
				names.Add(change.Author);
			}
			if (count == 0)
				return null;

			if (useHTML)
			{
				builder.AppendFormat("{0} was changed {1} time{2} by \n",
					"<a title=\"" + topic.Fullname + "\" href='" + TheLinkMaker.LinkToTopic(topic) + "'>" + topic.Name + "</a>",
					count,
					(count == 1 ? "" : "s"));
			}
			else
			{
				builder.AppendFormat("{0} was changed {1} time{2} by \n",
					topic.Name,
					count,
					(count == 1 ? "" : "s"));
			}

			bool firstName = true;
			foreach (string eachAuthor in names)
			{
				if (!firstName)
					builder.Append(", ");
				firstName = false;
				builder.Append(eachAuthor);
			}	
			if (useHTML)
				builder.Append("<br />\n");
			else
				builder.Append("\n");
			return builder.ToString();
		}
	}
}
