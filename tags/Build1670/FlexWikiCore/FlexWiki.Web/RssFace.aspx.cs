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
using FlexWiki.Newsletters;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for RssFace.
	/// </summary>
	public class RssFace : BasePage
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

		protected void ShowPage()
		{
			string ns = Request.QueryString["namespace"];
			Response.Write(@"
<fieldset><legend class='DialogTitle'>Subscriptions</legend>
");
			Response.Write(@"<p>Listed below are the available <b>newsletters</b> and <b>RSS</b> subscriptions.");
			if (ns == null)
			{
				Response.Write(@" All subscriptions available in this Wiki federation are listed.");
			}
			else
			{
				Response.Write(@" Only the subscriptions for <i>" + EscapeHTML(ns) + "</i> are listed.");
				Response.Write(@"	You can also view <a href=" + Request.Path + @">all subscriptions on this site</a>.");
			}

			Response.Write(@"</p>");

			Response.Write(@"<p>There are three ways to subscribe to change notifications for this site:

<ul>
<li><b>Newsletters (email and RSS).</b>  Newsletters provide notification of changes to a set of related topics.  
To recieve newsletter updates by email, visit the newsletter topic and add your name to the Subscribers property.
To subscribe the the RSS feed for the newsletter directly, use the listed link below.
<li><b>Namespace feeds (RSS).</b>  You can subscribe to RSS feeds for all of the topics in this namespace.
</ul>
</p>");

			ShowNewsletters(ns);
			ShowNamespaceFeeds(ns);

			Response.Write(@"
</fieldset>");

		}

		void OpenTable()
		{
			Response.Write(@"
<table border='0'cellspacing='6' cellpadding='0'>
");
		}

		void CloseTable()
		{
			Response.Write(@"
</table>");
		}

		void ShowNewsletters(string ns)
		{
			Response.Write(@"<h1>Newsletters</h1>");
			
			NewsletterManager nm = new NewsletterManager(TheFederation, TheLinkMaker);

			Hashtable hash = new Hashtable();
			foreach (AbsoluteTopicName newsletterName in nm.GetAllNewsletterNames(ns))
			{
				ArrayList topicsInThisNamespace = (ArrayList)hash[newsletterName.Namespace];
				if (topicsInThisNamespace == null)
				{
					topicsInThisNamespace = new ArrayList();
					hash[newsletterName.Namespace] = topicsInThisNamespace;
				}
				topicsInThisNamespace.Add(newsletterName);
			}

			ArrayList bases = new ArrayList();
			bases.AddRange(hash.Keys);
			bases.Sort();
			
			OpenTable();
			foreach (string each in bases)
			{
				ContentBase cb = TheFederation.ContentBaseForNamespace(each);
				if (ns == null)
					Response.Write(@"<tr><td colspan='2'><div class='SubscriptionNamespace'>" + EscapeHTML(cb.FriendlyTitle)  + "</div></td></tr>");
				foreach (AbsoluteTopicName abs in (ArrayList)(hash[each]))
				{
					TopicInfo info = TheFederation.GetTopicInfo(abs.ToString());
					string desc = info.GetProperty("Description");
					Response.Write(@"
<tr>
<td><a class=""standardsButton"" href='" + RootUrl(Request) + @"rss.aspx?newsletter=" + abs.ToString() + @"'>rss</a></td>
<td><a href='" + TheLinkMaker.LinkToTopic(abs) + @"'>" + abs.Name + @"</a></td>
</tr>
<tr>
<td></td>
<td><span style='font-size: x-small; color: gray'>" + (desc == null ? "" : desc) + @"</span></td>
</tr>
<tr>");
				}
			}
			CloseTable();
		}

		void ShowNamespaceFeeds(string ns)
		{
			Response.Write(@"<h1>Namespace Feeds</h1>");
			ArrayList bases = new ArrayList();
			foreach (string each in TheFederation.Namespaces)
			{
				if (ns != null && ns != each)
					continue;
				ContentBase cb = TheFederation.ContentBaseForNamespace(each);
				bases.Add(cb);
			}
			bases.Sort();

			OpenTable();
			foreach (ContentBase cb in bases)
			{
				if (ns == null)
					Response.Write(@"<tr><td colspan='2'><div class='SubscriptionNamespace'>" + EscapeHTML(cb.FriendlyTitle)  + "</div></td></tr>");
				Response.Write(@"
<tr>
<td><a class=""standardsButton"" href='" + RootUrl(Request) + @"rss.aspx?namespace=" + cb.Namespace + @"'>rss</a></td>
<td>Only this namespace (<a href='" + TheLinkMaker.LinkToTopic(new AbsoluteTopicName(cb.Namespace + "." + cb.HomePage)) + @"'>" + cb.FriendlyTitle + @"</a>)</td>
</tr>
<tr>
<td><a class=""standardsButton"" href='" + RootUrl(Request) + @"rss.aspx?namespace=" + cb.Namespace + @"&inherited=y'>rss</a></td>
<td>This namespace and related namespaces (<a href='" + TheLinkMaker.LinkToTopic(new AbsoluteTopicName(cb.Namespace + "." + cb.HomePage)) + @"'>" + cb.FriendlyTitle + @"</a>");
				foreach (ContentBase import in cb.ImportedContentBases)
					Response.Write(", <a href='" + TheLinkMaker.LinkToTopic(new AbsoluteTopicName(import.Namespace + "." + import.HomePage)) + @"'>" + import.FriendlyTitle + @"</a>");
				Response.Write(@")</td>
</tr>");
			}
			CloseTable();

		}
	}
}
