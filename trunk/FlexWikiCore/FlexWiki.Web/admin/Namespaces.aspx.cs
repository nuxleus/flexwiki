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
using FlexWiki;
using FlexWiki.Web;


namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Admin.
	/// </summary>
	public class Namespaces : AdminPage
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
			Response.Write(@"<fieldset><legend class='DialogTitle'>Namespace Configuration</legend>");
			ShowNamespaces();
			Response.Write(@"<hr noshade size='1'><a href='EditNamespace.aspx'>Create <b>new namespace</b></a>");
			Response.Write(@"</fieldset>");
		}

		void ShowNamespaces()
		{
			Response.Write("<table class='NamespaceTable' width='100%' cellpadding=2 cellspacing=0>");
			foreach (ContentBase each in TheFederation.ContentBases)
			{
				Response.Write("<tr class='NamespaceTitleLine'>");
				Response.Write("<td><b><a href='EditNamespace.aspx?ns=" + each.Namespace + "'>" + each.Namespace + "</a></b></td>");
				Response.Write("<td><a href='" + TheLinkMaker.LinkToTopic(new AbsoluteTopicName(each.HomePage, each.Namespace)) + "'>" + EscapeHTML(each.HomePage) + "</a></td>");
				Response.Write("<td>" + EscapeHTML(each.Title) + "</td>");
				Response.Write("<td>" + EscapeHTML(each.Contact) + "</td>");
				Response.Write("</tr>");
				Response.Write("<tr>");
				Response.Write("<td colspan='4'>" + EscapeHTML(each.Description) + "</td>");
				Response.Write("</tr>");
			}
			Response.Write("</tr></table>");
		}

	}
}
