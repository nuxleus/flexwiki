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
using FlexWiki.Web;
using System.Configuration;

namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for AdminPage.
	/// </summary>
	public class AdminPage : BasePage
	{
		public AdminPage() : base()
		{
		}

		protected override string RelativeBase
		{
			get
			{
				return "../";
			}
		}

		protected void ShowAdminMenu()
		{
			UIResponse.WriteStartMenu("Administration");
			UIResponse.WriteMenuItem("default.aspx", "Home", "Go to the home page for FlexWiki administration");
			UIResponse.WriteMenuItem("Providers.aspx", "Namespace providers", "View and edit namespace providers for this federation");
			UIResponse.WriteMenuItem("EditLinksBlacklist.aspx", "External Links Blacklist", "Edit the list of blacklisted external links");
			UIResponse.WriteMenuItem("config.aspx", "Validate Configuration", "Validate that your FlexWiki site is correctly configured");
			UIResponse.WriteMenuItem("Newsletter.aspx", "Newsletter Daemon", "Show information about the newsletter delivery daemon status");
			UIResponse.WriteMenuItem("ShowCache.aspx", "Show Cache", "Show a list of everything in the cache (and, optionally, clear the cache)");
			UIResponse.WriteMenuItem("ShowUpdates.aspx", "Show Updates", "Show recent changes to the federation");
			UIResponse.WriteEndMenu();
		}




	}
}
