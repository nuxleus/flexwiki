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
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki.Web;

namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for ShowCache.
	/// </summary>
	public class ShowCache : AdminPage
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected void ShowPage()
		{
			UIResponse.ShowPage("Cache Information", new UIResponse.MenuWriter(ShowMenu), new UIResponse.BodyWriter(ShowMain));
		}

		private void ShowMenu()
		{
			UIResponse.WriteStartMenu("Cache");
			UIResponse.WriteMenuItem("ShowCache.aspx?clear=1", "Clear", "Clear the cache");
			UIResponse.WriteEndMenu();
			UIResponse.WritePara("&nbsp;");

			ShowAdminMenu();
		}
   
		protected void ShowMain()
		{
			string clear = Request.QueryString["clear"];
			if (clear == "1")
			{
				CacheManager.Clear();
				Response.Redirect("ShowCache.aspx");
				return;
			}

			string key = Request.QueryString["key"];
			if (key == null)
				ShowKeys();
			else
				ShowKey(key);
		}

		private void WriteFilterScript()
		{
			Response.Write("<script language='javascript'>");
			Response.Write(@"

function FilterChanged()
{
	var filterControl = document.getElementById('Filter');
	var filter = filterControl.value;
	var id = 0;
	while (true)
	{
		var headerPrefix = '" + headerPrefix + @"' + id;
		var valuePrefix = '" + valuePrefix + @"' + id;

		var valueElement = document.getElementById(valuePrefix);
		if (valueElement == null)
			break;
		var show = true;
		var test = """" + valueElement.innerText.toUpperCase();
		if (filter != """" && test.indexOf(filter.toUpperCase()) < 0)
			show = false;
		var styleString = show ? 'block' : 'none';

		var headerPrefixElement = document.getElementById(headerPrefix);
		headerPrefixElement.style.display = styleString;
		id++;
	}
}

");

			Response.Write("</script>");
		}

		private const string headerPrefix = "Row";
		private const string valuePrefix = "RowData";

		private void ShowKeys()
		{
			WriteFilterScript();
			Response.Write("<p>Search for key: <input id='Filter' onkeyup='javascript:FilterChanged()' length=30/></p>");
			int id = 0;
			ArrayList keys = new ArrayList();
			keys.AddRange(CacheManager.Keys);
			keys.Sort();
			foreach (string key in keys)
			{
				Response.Write("<div  id='" + headerPrefix + id + "' class='CacheKey'><a href='ShowCache.aspx?key=" + key + "'><span id='" + valuePrefix + id + "'>" + key + "</span></a></div>");
				id++;
			}
		}

		private void ShowKey(string key)
		{
			Response.Write("<h2>" + EscapeHTML(key) + "</h2>");
			Response.Write("<h3>Cache Rule:</h3>");
			CacheRule rule = CacheManager.GetRuleForKey(key);
			if (rule != null)
				WriteLineNicely(Response.Output, rule);
			Response.Write("<h3>Value</h3>");
			object shortValue = CacheManager[key];
			if (shortValue != null)
			{
				if (shortValue is IEnumerable && !(shortValue is string))
				{
					Response.Output.WriteLine("<table width='100%' border=1 cellpadding=3 cellspacing=0>");
					foreach (object each in (IEnumerable)shortValue)
					{
						Response.Output.WriteLine("<tr><td valign='top'>");
						WriteLineNicely(Response.Output, each);
						Response.Output.WriteLine("</tr>");
					}
					Response.Output.WriteLine("</table>");
				}
				else
					WriteLineNicely(Response.Output, shortValue);
			}
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
	}
}
