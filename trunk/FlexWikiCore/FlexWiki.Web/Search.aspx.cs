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
using System.Text.RegularExpressions;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for Search.
	/// </summary>
	public class Search : BasePage
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

		static string All = "[All]";

		protected void DoSearch()
		{
			string search = Request.QueryString["search"];
			Response.Write(@"
<div id='TopicTip' class='TopicTip' ></div>
<fieldset><legend class='DialogTitle'>Search</legend>
<form id='SearchForm'>
<p>Search:<br /><input type='text'  name='search' value ='" + (search == null ? "[enter regular expression]" : HttpUtility.HtmlEncode(search)) + @"'>
<input type='submit' ID=Search Value='Go' />
</p>");

			ArrayList uniqueNamespaces = new ArrayList(TheFederation.Namespaces);
			uniqueNamespaces.Sort();

			string preferredNamespace = Request.QueryString["namespace"];
			if (preferredNamespace == null)
				preferredNamespace  = DefaultNamespace;

			Response.Write("<p>Namespace:<br /><select name='namespace' class='SearchColumnFilterBox' id='NamespaceFilter'>");
			Response.Write("<option value='"+ All + "'>" + All + "</option>");
			foreach (string ns in uniqueNamespaces)
			{
				string sel = (ns == preferredNamespace) ? " selected " : "";
				Response.Write("<option " + sel + " value='"+ ns + "'>" + ns + "</option>");
			}
			Response.Write(@"</select></p></form>");		

			if (search != null)
			{
				Response.Write(@"<fieldset><legend>Search Result</legend>");
				Response.Write(@"<div class='SearchMain'>");

				LinkMaker lm = TheLinkMaker;
			
				Hashtable searchTopics = new Hashtable();
				if (preferredNamespace == All)
				{
					foreach (string ns in uniqueNamespaces)
					{
						ContentBase cb = TheFederation.ContentBaseForNamespace(ns);
						if (cb == null)
							continue;
						searchTopics[cb] = cb.AllTopics(false);
					}
				}
				else
				{
					ContentBase cb = TheFederation.ContentBaseForNamespace(preferredNamespace);
					searchTopics[cb] = cb.AllTopics(false);
				}

				foreach (ContentBase cb in searchTopics.Keys)
				{
					string ns = cb.Namespace;
					bool header = false;
					foreach (AbsoluteTopicName topic in (ArrayList)(searchTopics[cb]))
					{
						string s = cb.Read(topic);
						string bodyWithTitle = topic.ToString() + s;
						
						if (Regex.IsMatch(bodyWithTitle, search, RegexOptions.IgnoreCase))
						{
							if (!header && searchTopics.Count > 1)
								Response.Write("<h1>" + ns + "</h1>");
							header = true;

							Response.Write("<div class='searchHitHead'>");
							Response.Write(@"<a title=""" + topic.Fullname + @"""  href=""" + lm.LinkToTopic(topic) + @""">");
							Response.Write(topic.Name);
							Response.Write("</a>");
							Response.Write("</div>");

							string [] lines = s.Split (new char[]{'\n'});
							Response.Write("<div class='searchHitBody'>");
							foreach (string each in lines)
							{
								if (Regex.IsMatch(each, search, RegexOptions.IgnoreCase))
								{
									Response.Write(Formatter.FormattedString(each, OutputFormat.HTML, cb, TheLinkMaker, null));
								}
							}
							Response.Write("</div>");
						}
					}
				}
				Response.Write(@"</div>");
			}
		}
	}
}
