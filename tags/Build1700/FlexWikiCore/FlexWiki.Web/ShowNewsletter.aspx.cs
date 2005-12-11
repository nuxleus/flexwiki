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
using FlexWiki.Newsletters;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for ShowNewsletter.
	/// </summary>
	public class ShowNewsletter : BasePage
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

		protected AbsoluteTopicName Newsletter
		{
			get
			{
				string nl = Request.QueryString["newsletter"];
				if (nl == null)
					return null;
				return new AbsoluteTopicName(nl);
			}
		}

		protected void DoPage()
		{
			NewsletterManager manager = new NewsletterManager(TheFederation, TheLinkMaker, null, null, null);
			AbsoluteTopicName newsletter = Newsletter;
			
			DateTime since;
			string headInsert = null;
			string description = null;
			string newsletterName = null;
			string newsletterLink = null;
			IEnumerable topics = null;
			string homeNamespace = null;

			
			if (newsletter != null)
			{
				description = manager.GetDescriptionForNewsletter(newsletter);
				since = manager.GetLastUpdateForNewsletter(newsletter);
				newsletterName = newsletter.Name;
				newsletterLink = TheLinkMaker.LinkToTopic(newsletter);
				topics = manager.AllTopicsForNewsletter(newsletter);
				homeNamespace = newsletter.Namespace;
			}
			else
			{
				since = DateTime.Now;
				since = since.Subtract(new TimeSpan(24, 0, 0));
				// Arbitrary newsletter
				ArrayList al = new ArrayList();
				al.Add(new AbsoluteTopicName("Microsoft.Projects.Wiki.HomePage"));
				al.Add(new AbsoluteTopicName("Microsoft.Projects.Wiki.SecondPage"));
				topics = al;
				homeNamespace = TheFederation.DefaultNamespace;
			}

			since = since.Subtract(new TimeSpan(24, 0, 0));
	
			headInsert	= InsertStylesheetReferences();
			
			string html = manager.BuildArbitraryNewsletter(newsletterName, newsletterLink, topics, since, headInsert, description, homeNamespace);
			
			Response.Write(html);
		}
	}
}
