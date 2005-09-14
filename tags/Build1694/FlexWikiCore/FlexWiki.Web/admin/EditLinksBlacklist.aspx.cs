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
using System.Configuration;
using FlexWiki.Web;


namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Admin.
	/// </summary>
	public class EditLinksBlacklist : AdminPage
	{
	
		private void Page_Load(object sender, System.EventArgs e)
		{
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
			UIResponse.ShowPage("External Links Blacklist", new UIResponse.MenuWriter(ShowAdminMenu), new UIResponse.BodyWriter(ShowList));
		}

		void ShowList()
		{
			if (IsPost)
				ProcessPost();
			UIResponse.WriteStartForm("EditLinksBlacklist.aspx");
			UIResponse.WriteStartFields();
			UIResponse.WriteTextAreaField("Additions", "New URLs to block", "Enter URLs prefixes to add to blacklist (each on its own line)", "");
			UIResponse.WriteTextAreaField("Removals", "URLs to unblock ", "Enter URLs prefixes to remove from the blacklist (each on its own line)", "");
			UIResponse.WriteEndFields();
			UIResponse.WriteStartButtons();
			UIResponse.WriteSubmitButton("Update", "Save");
			UIResponse.WriteEndButtons();
			UIResponse.WriteEndForm();

			UIResponse.WriteHeading("Current Blacklist",2);
			foreach (string each in TheFederation.BlacklistedExternalLinkPrefixes)
			{
				UIResponse.WriteLine(UIResponse.Escape(each));
			}
		}

		void ProcessPost()
		{
			string [] adds = ((string)(Request.Form["Additions"])).Split (new char[] {'\n'});
			string [] removals = ((string)(Request.Form["Removals"])).Split (new char[] {'\n'});
			int count = 0;

			foreach (string s in adds)
			{
				string each = s.Trim();
				if (each == "")
					continue;
				count++;
				if (TheFederation.BlacklistedExternalLinkPrefixes.Contains(each))
				{
					UIResponse.WriteLine("Ignored (already listed) " + UIResponse.Escape(each));
				}
				else
				{
					TheFederation.AddBlacklistedExternalLinkPrefix(each);
					UIResponse.WriteLine("Added " + UIResponse.Escape(each));
				}
			}
			foreach (string s in removals)
			{
				string each = s.Trim();
				if (each == "")
					continue;
				count++;
				if (!TheFederation.BlacklistedExternalLinkPrefixes.Contains(each))
				{
					UIResponse.WriteLine("Ignored (not listed) " + UIResponse.Escape(each));
				}
				else
				{
					TheFederation.RemoveBlacklistedExternalLinkPrefix(each);
					UIResponse.WriteLine("Removed " + UIResponse.Escape(each));
				}
			}

			if (count > 0)
			{
				UIResponse.WriteDivider();
				ArrayList newList = new ArrayList();
				foreach (string s in TheFederation.BlacklistedExternalLinkPrefixes)
					newList.Add(s);
				TheFederation.CurrentConfiguration.BlacklistedExternalLinks = newList;
				TheFederation.CurrentConfiguration.WriteToFile(TheFederation.CurrentConfiguration.FederationNamespaceMapFilename);
			}			
		}

	}
}
