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

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for LastModified.
	/// </summary>
	public class LastModified : BasePage
	{
		private string preferredNamespace = string.Empty;
		private ArrayList uniqueNamespaces;

		private void Page_Load(object sender, System.EventArgs e)
		{
			uniqueNamespaces = new ArrayList(TheFederation.Namespaces);
			uniqueNamespaces.Sort();
			
			preferredNamespace = Request.QueryString["namespace"];
			if (preferredNamespace == null)
				preferredNamespace  = DefaultNamespace;


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

		static string esc(string input)
		{
			// replace HTML special characters with character entities
			// this has the side-effect of stripping all markup from text
			string str = input;
			str = Regex.Replace (str, "&", "&amp;") ;
			str = Regex.Replace (str, "<", "&lt;") ;
			str = Regex.Replace (str, ">", "&gt;") ;
			return str;
		}

		protected string NamespaceFilter()
		{
			string result = "<select onchange='changeNamespace()' class='SearchColumnFilterBox' id='NamespaceFilter'>";
			foreach (string ns in uniqueNamespaces)
			{
				string sel = (ns == preferredNamespace) ? " selected " : "";
				result += "<option " + sel + " value='"+ ns + "'>" + ns + "</option>";
			}
			result += "</select>";
			return result;
		}

		protected void DoSearch()
		{
			LinkMaker lm = TheLinkMaker;
			
			ContentBase cb = TheFederation.ContentBaseForNamespace(preferredNamespace);

			// Get the list of topics and authors from the cache (or generate if needed)
			IList history;
			IEnumerable topics;
			Hashtable authors;

			history = TheFederation.CacheManager.GetCachedNamespaceHistory(cb.Namespace);
			if (history == null)
			{
				IEnumerable t = cb.AllTopicsSortedLastModifiedDescending();
				
				Hashtable a = new Hashtable();
				foreach (AbsoluteTopicName topic in t)
					a[topic] = cb.GetTopicLastAuthor(topic.LocalName);
	
				history = new ArrayList();
				history.Add(t);
				history.Add(a);

				TheFederation.CacheManager.PutCachedNamespaceHistory(cb.Namespace, history, cb.CacheRule);
			}

			// Now pull from the cached data
			topics = (IEnumerable)(history[0]);
			authors = (Hashtable)(history[1]);

			Hashtable uniqueAuthorsHash = new Hashtable();
			foreach (string author in authors.Values)
				uniqueAuthorsHash[author] = null;
			ArrayList uniqueAuthors = new ArrayList(uniqueAuthorsHash.Keys);
			uniqueAuthors.Sort();



			Response.Write("<table cellspacing='0' cellpadding='2' border='0'>");
			Response.Write("<thead>");
			Response.Write("<td class=\"SearchColumnHeading\" width=\"300\">Topic</td>");
			Response.Write("<td class=\"SearchColumnHeading\" width=\"100\">Modified</td>");
			Response.Write("<td class=\"SearchColumnHeading\" width=\"200\">Author: ");
			Response.Write("<select  onchange='filter()' class='SearchColumnFilterBox' id='AuthorFilter'>");
			Response.Write("<option value='"+ All + "'>" + All + "</option>");
			foreach (string author in uniqueAuthors)
				Response.Write("<option value='"+ author + "'>" + author + "</option>");
			Response.Write(@"</select>");
			Response.Write("</td>");

			Response.Write("</thead><tbody id=\"MainTable\">");

			int row = 0;
			foreach (AbsoluteTopicName  topic in topics)
			{
				Response.Write("<tr id=\"row" + row + "\" class=\"" + (((row & 1) == 0) ? "SearchOddRow" : "SearchEvenRow") + "\">");
				row++;

				Response.Write("<td>");
				Response.Write("<b><a title=\"" + topic.Fullname + "\"  href=\"" + lm.LinkToTopic(topic) + "\">");
				Response.Write(topic.Name);
				Response.Write("</a></b>");
				Response.Write("</td>");
   
				Response.Write("<td>");
				DateTime stamp = cb.GetTopicLastWriteTime(topic.LocalName);
				if (stamp.Date == DateTime.Now.Date)
					Response.Write(stamp.ToString("h:mm tt"));
				else
					Response.Write(stamp.ToString("MM/dd/yyyy h:mm tt"));
				Response.Write("</td>");
				Response.Write("<td>");
				Response.Write(esc((string)(authors[topic])));
				Response.Write("</td>");
				Response.Write("</tr>");            
			}
			Response.Write("</tbody></table>");

		}
	}
}
