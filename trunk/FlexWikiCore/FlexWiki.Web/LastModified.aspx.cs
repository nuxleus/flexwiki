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


		protected void DoSearch()
		{
			LinkMaker lm = TheLinkMaker;
			ArrayList uniqueNamespaces = new ArrayList(TheFederation.Namespaces);
			uniqueNamespaces.Sort();

			string preferredNamespace = Request.QueryString["namespace"];
			if (preferredNamespace == null)
				preferredNamespace  = DefaultNamespace;

			ContentBase cb = TheFederation.ContentBaseForNamespace(preferredNamespace);

			// Get the list of topics and authors from the cache (or generate if needed)
			string historyKey = "HistoryForNamespace." + cb.Namespace.ToString();
			ArrayList history;
			IEnumerable topics;
			Hashtable authors;

			history = (ArrayList)(CacheManager[historyKey]);
			if (history == null)
			{
				IEnumerable t = cb.AllTopicsSortedLastModifiedDescending(false);
				
				Hashtable a = new Hashtable();
				foreach (AbsoluteTopicName topic in t)
					a[topic] = cb.GetTopicLastAuthor(topic);
	
				history = new ArrayList();
				history.Add(t);
				history.Add(a);

				CacheManager.Put(historyKey, history, cb.CacheRule);
			}

			// Now pull from the cached data
			topics = (IEnumerable)(history[0]);
			authors = (Hashtable)(history[1]);

			Hashtable uniqueAuthorsHash = new Hashtable();
			foreach (string author in authors.Values)
				uniqueAuthorsHash[author] = null;
			ArrayList uniqueAuthors = new ArrayList(uniqueAuthorsHash.Keys);
			uniqueAuthors.Sort();

			Response.Write(@"
<script  type=""text/javascript"" language='javascript'>

function filter()
{
	var author = AuthorFilter.options[AuthorFilter.selectedIndex].text;
	var table = document.getElementById('MainTable');
	for (var i = 0; i < table.rows.length; i++)
	{
		var row = table.rows.item(i);
		var authorcell = row.cells.item(2);
		var show = true;
		if (author != '" + All + @"' && authorcell.innerText != author)
			show = false;
		if (show)
			row.style.display = 'block';
		else
			row.style.display = 'none';
	}
}


function changeNamespace()
{
	var ns = NamespaceFilter.options[NamespaceFilter.selectedIndex].text;
	var newURL = '" + lm.LinkToRecentChanges(null) + @"?namespace=' + ns;
	window.location = newURL;	
}

</script>
<fieldset>
<legend class='DialogTitle'>Recent Changes</legend>

");

			Response.Write("<p>Namespace: <select onchange='changeNamespace()' class='SearchColumnFilterBox' id='NamespaceFilter'>");
			foreach (string ns in uniqueNamespaces)
			{
				string sel = (ns == preferredNamespace) ? " selected " : "";
				Response.Write("<option " + sel + " value='"+ ns + "'>" + ns + "</option>");
			}
			Response.Write(@"</select></p>");		


			Response.Write("<table cellspacing='0' cellpadding='2' border='0'>");
			Response.Write("<thead>");
			Response.Write("<td class='SearchColumnHeading'>Topic</td>");
			Response.Write("<td class='SearchColumnHeading'>Modified</td>");
			Response.Write("<td class='SearchColumnHeading'>Author: ");
			Response.Write("<select  onchange='filter()' class='SearchColumnFilterBox' id='AuthorFilter'>");
			Response.Write("<option value='"+ All + "'>" + All + "</option>");
			foreach (string author in uniqueAuthors)
				Response.Write("<option value='"+ author + "'>" + author + "</option>");
			Response.Write(@"</select>");
			Response.Write("</td>");

			Response.Write("</thead><tbody id='MainTable'>");

			int row = 0;
			foreach (AbsoluteTopicName  topic in topics)
			{
				Response.Write("<tr id='row" + row + "' style='display: block' class='" + (((row & 1) == 0) ? "SearchOddRow" : "SearchOddRow") + "'>");
				row++;

				Response.Write("<td>");
				Response.Write("<b><a  title=\"" + topic.Fullname + "\"  href=\"" + lm.LinkToTopic(topic) + "\">");
				Response.Write(topic.Name);
				Response.Write("</a></b>");
				Response.Write("</td>");

				Response.Write("<td>");
				DateTime stamp = cb.GetTopicLastWriteTime(topic);
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
			Response.Write("</fieldset>");

		}
	}
}
