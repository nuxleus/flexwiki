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
using System.Configuration;
using System.Web;
using System.Web.SessionState;
using System.Web.Security;
using FlexWikiSecurity;

namespace FlexWiki.Web 
{
	/// <summary>
	/// Summary description for Global.
	/// </summary>
	public class Global : System.Web.HttpApplication
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		public Global()
		{
			InitializeComponent();
		}	
		
		protected void Application_Start(Object sender, EventArgs e)
		{

		}
 
		protected void Session_Start(Object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_EndRequest(Object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e)
		{

			string security = ConfigurationSettings.AppSettings["Security"];
			if (security!="webconfig")
			{
				// We are doing some more complex security

				// Extract the forms authentication cookie
				string cookieName = FormsAuthentication.FormsCookieName;
				HttpCookie authCookie = Context.Request.Cookies[cookieName];

				if (authCookie == null)
				{
					// There is no authentication cookie
					return;
				}
				FormsAuthenticationTicket authTicket = null;
				try
				{
					authTicket = FormsAuthentication.Decrypt(authCookie.Value);
				}
				catch
				{
					return;
				}
				if (authTicket == null)
				{
					// Failed to decrypt
					return;
				}
				// When the ticket was created, the UserData property was assigned a pipe-delimited string of roles
				// Create a GenericIdentity object
				SiteIdentity id = new SiteIdentity(authTicket.Name);
				// Now we use our CustomPrincipal class that will flow throughout the request
				SitePrincipal principal = new SitePrincipal(id.UserID);
				// Attach the new Principal object to the current HttpContext.User object
				Context.User = principal;
			}
		}

		protected void Application_Error(Object sender, EventArgs e)
		{

		}

		protected void Session_End(Object sender, EventArgs e)
		{

		}

		protected void Application_End(Object sender, EventArgs e)
		{

		}
			
		#region Web Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.components = new System.ComponentModel.Container();
		}
		#endregion
	}
}

