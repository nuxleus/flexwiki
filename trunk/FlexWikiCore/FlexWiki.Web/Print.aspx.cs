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
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for Print.
	/// </summary>
	public class Print : BasePage
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

		protected void DoPage()
		{
			AbsoluteTopicName topic = GetTopicName();
			ContentBase cb = DefaultContentBase;
			LinkMaker lm = TheLinkMaker;

			Response.Write("<div style='font-family: Verdana'>");
		
			Response.Write("<div style='font-size: 18pt; font-weight: bold; '>");
			Response.Write(topic.Name);
			Response.Write("</div>");

			Response.Write("<div style='font-size: 8pt'>");
			Response.Write("Last changed: " + cb.GetTopicLastAuthor(topic));
			Response.Write("</div>");

			Response.Write("<hr noshade size='2' />");
			Response.Write("</div>");

			Response.Write("<div class='PrintMain'>");

			// TODO - enable diffs for print too!
			// TODO - an opportunity for caching
			Response.Write(Formatter.FormattedTopic(topic, OutputFormat.HTML, false, TheFederation, TheLinkMaker, null));

			Response.Write("</div>");
		}
	}
}
