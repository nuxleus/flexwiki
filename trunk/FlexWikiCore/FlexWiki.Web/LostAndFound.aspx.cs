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

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for LostAndFound.
	/// </summary>
	public class LostAndFound : BasePage
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
			Response.Write(@"
<fieldset><legend class='DialogTitle'>Lost And Found</legend>
<form id='Form'>");

			ArrayList uniqueNamespaces = new ArrayList(Federation.Namespaces);
			uniqueNamespaces.Sort();

			string preferredNamespace = Request.QueryString["namespace"];
			if (preferredNamespace == null)
				preferredNamespace  = DefaultNamespace;

			Response.Write("<p>Namespace:<br /><select title='to explore the list and found for another namespace, select it here' name='namespace' class='SearchColumnFilterBox' id='NamespaceFilter'>");
			foreach (string ns in uniqueNamespaces)
			{
				string sel = (ns == preferredNamespace) ? " selected " : "";
				Response.Write("<option " + sel + " rawValue='"+ ns + "'>" + ns + "</option>");
			}
			Response.Write(@"</select> <input title='click here to explore the lost and found for the selected namespace' type='submit' ID='Go' Value='Change namespace' /></p></form>");		


			NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(preferredNamespace);
			LinkMaker lm = TheLinkMaker;

			if (storeManager == null)
			{
				Response.Write(@"
<h1>Inaccessible namespace</h1>
<p>The namespace you have selected is not accessible.</p>");
			}
			else
			{
				Response.Write(@"
	<h1>Lost and Found</h1>
	<p>Below are listed pages that are not reachable from the home page of this namespace.</p>
	<p>Related pages (ones that link to each other) are listed together.  Bold topics are completely unreferenced.  Other topics are referenced, but only from within the related topic group.
	");
				ContentStoreAnalysis analysis = new ContentStoreAnalysis(storeManager);

				QualifiedTopicRevision home = new QualifiedTopicRevision(storeManager.HomePage, storeManager.Namespace);
				Response.Write("<ul>");
				foreach (Set eachIsland in analysis.Islands)
				{
					if (eachIsland.Contains(home))
						continue;		// skip the mainland!
					bool first = true;
					Response.Write("<li>");
					foreach (QualifiedTopicRevision eachTopic in eachIsland)
					{
						TopicAnalysis tan = analysis.AnalysisFor(eachTopic);
						if (!first)
							Response.Write(", ");
						first = false;
						int refs = tan.RefCount;
						if (refs == 0)
							Response.Write("<b>");
						Response.Write("<a href='" + lm.LinkToTopic(eachTopic) + "'>" + eachTopic.LocalName + "</a>");
						if (refs == 0)
							Response.Write("</b>");
					}
					Response.Write("</li>");
				}
				Response.Write("</ul>");
			}
			Response.Write("</fieldset>");
		}
	}

}
