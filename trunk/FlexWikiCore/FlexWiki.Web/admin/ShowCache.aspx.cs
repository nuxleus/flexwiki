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
			Response.Write(@"<fieldset><legend class='DialogTitle'>Cache</legend>");
			string key = Request.QueryString["key"];
			if (key == null)
				ShowKeys();
			else
				ShowKey(key);
			Response.Write(@"</fieldset>");
		}

		void ShowKeys()
		{
			Response.Write("<table cellpadding='2' cellspacing='0' border='0'>");

			bool isEmpty = true;
			foreach (string key in CacheManager.Keys)
			{
				isEmpty = false;
				const int ml = 150;
				object shortValue = CacheManager[key];
				string shortValueString = "";
				if (shortValue != null)
				{
					shortValueString = shortValue.ToString();
					if (shortValue is IEnumerable && !(shortValue is string))
					{
						int count = 0;
						StringBuilder b = new StringBuilder();
						foreach (object each in (IEnumerable)shortValue)
						{
							if (count != 0)
								b.Append(", ");
							b.Append(each.ToString());
							count++;
						}
						shortValueString += "([count=" + count + "]: " + b.ToString() + ")";
					}
					if (shortValueString.Length > ml)
						shortValueString = shortValueString.Substring(0, ml) + "...";
				}
				string ruleText = "";
				CacheRule rule = CacheManager.GetRuleForKey(key);
				if (rule != null)
				{
					ruleText = rule.Description;
					if (ruleText.Length > ml)
						ruleText = ruleText.Substring(0, ml) + "...";
					ruleText = EscapeHTML(ruleText);
				}

				Response.Write("<tr>");
				Response.Write("<td valign='top' class='CacheKey'><a href='ShowCache.aspx?key=" + key + "'>" + key + "</a></td>");
				Response.Write("<td valign='top' class='CacheRules'>" + ruleText + "</td>");
				Response.Write("</tr>");
				Response.Write("<tr>");
				Response.Write("<td colspan='2' valign='top' class='CacheValue'>" + EscapeHTML(shortValueString) + "</td>");
				Response.Write("</tr>");
			}

			Response.Write("</table>");
			if (isEmpty)
				Response.Write("Cache is empty");
		}

		void ShowKey(string key)
		{
			Response.Write("<b>Key:</b> " + EscapeHTML(key));
			Response.Write("<hr />");
			Response.Write("<b>Rule:</b><br />");
			string ruleText = "";
			CacheRule rule = CacheManager.GetRuleForKey(key);
			if (rule != null)
				ruleText = EscapeHTML(rule.Description);
			Response.Write(ruleText);
			Response.Write("<hr />");
			Response.Write("<b>Cached Value:</b><br />");
			object shortValue = CacheManager[key];
			string shortValueString = "";
			if (shortValue != null)
			{
				shortValueString = shortValue.ToString();
				if (shortValue is IEnumerable && !(shortValue is string))
				{
					int count = 0;
					StringBuilder b = new StringBuilder();
					foreach (object each in (IEnumerable)shortValue)
					{
						if (count != 0)
							b.Append(", ");
						b.Append(each.ToString());
						count++;
					}
					shortValueString += "([count=" + count + "]: " + b.ToString() + ")";
				}
			}
			Response.Write(EscapeHTML(shortValueString));
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
