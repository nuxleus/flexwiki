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
using FlexWiki.Web;

namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Newsletter.
	/// </summary>
	public class Newsletter : AdminPage
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

		static string escWithBreaks(string input)
		{
			// replace HTML special characters with character entities
			// this has the side-effect of stripping all markup from text
			string str = input;
			str = Regex.Replace (str, "&", "&amp;") ;
			str = Regex.Replace (str, "<", "&lt;") ;
			str = Regex.Replace (str, ">", "&gt;") ;
			str = Regex.Replace (str, "\n", "<br />") ;
			return str;
		}

		protected void ShowPage()
		{
			Response.Write("<h1>Newsletter Daemon Status</h1>");

			Response.Write("<div style='margin: .25in; font-size: 9pt;'>");

			Response.Write("<p>Now: <b>" + DateTime.Now.ToString() + " </b></p>");

			Response.Write("<p>Work underway: <b>" + TheNewsletterDaemon.WorkUnderway.ToString() + " </b></p>");

			Response.Write("<p>Daemon started at <b>" + FancyTime(TheNewsletterDaemon.Started) + " </b></p>");

			Response.Write("<p>Daemon last checked in at <b>" + FancyTime(TheNewsletterDaemon.LastCheckin) + " </b></p>");

			Response.Write("<p>Work next due to start at <b>" + FancyTime(TheNewsletterDaemon.NextWorkDue) + " </b></p>");

			Response.Write("<p>Work last started at <b>" + FancyTime(TheNewsletterDaemon.WorkLastStarted) + " </b></p>");

			Response.Write("<p>Work last completed at <b>" + FancyTime(TheNewsletterDaemon.WorkLastCompleted) + " </b></p>");
			
			foreach (string s in TheNewsletterDaemon.Results)
			{
				Response.Write("<div style='margin: .25in; border 1px solid silver'>" + escWithBreaks(s) + "</div>");
			}

			Response.Write("</div'>");

		}

		protected string FancyTime(DateTime when)
		{
			string answer;
			DateTime now = DateTime.Now;

			if (when == DateTime.MinValue)
				return "[never]";
			TimeSpan diff = now.Subtract(when);
			if (diff.Hours > 1)
				return when.ToString();
			answer = when.ToString();
			string diffs = diff.ToString();
			int idx = diffs.LastIndexOf(".");
			if (idx != -1)
				diffs = diffs.Substring(0, idx);
			answer += " (" + diffs + ")";
			return answer;
		}
	}
}
