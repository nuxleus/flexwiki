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

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for Rename.
	/// </summary>
	public class Rename : BasePage
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

		protected AbsoluteTopicName AbsTopicName
		{
			get
			{
				return new AbsoluteTopicName(Request.QueryString["topic"]);
			}
		}

		protected string Fixup
		{
			get
			{
				return Request.Form["Fixup"];
			}
		}

		protected string LeaveRedirect
		{
			get
			{
				return Request.Form["LeaveRedirect"];
			}
		}

		protected string NewName
		{
			get
			{
				return Request.Form["NewName"];
			}
		}
        
		protected string OldName
		{
			get
			{
				return Request.Form["OldName"];
			}
		}

		protected string Namespace
		{
			get
			{
				return Request.Form["Namespace"];
			}
		}

		protected void PerformRename()
		{
			AbsoluteTopicName oldName = new AbsoluteTopicName(OldName, Namespace);
			string defaultNamespace = DefaultNamespace;
			string oldAppearsAs = (oldName.Namespace == defaultNamespace) ? oldName.Name : oldName.Fullname;
			string newName = NewName;
			string newAppearsAs = (oldName.Namespace == defaultNamespace) ? newName : Namespace + "." + newName;

			// See if the new name already exists
			if (DefaultContentBase.TopicExistsLocally(newName))
			{
				Response.Write("<b>Topic (" + newName + ") already exists.  Choose another name...</b>");
				return;
			}


			bool fixup = Fixup == "on";
			ArrayList log = DefaultContentBase.RenameTopic(oldName, newName, fixup);
			Response.Write("Renamed <i>" + oldAppearsAs + "</i> to <i>" + newName + "</i><br/>");
			Response.Write("<br/>");
			foreach (string each in log)
			{
				Response.Write(each + "<br>");
			}
			bool redir = LeaveRedirect == "on";
			if (redir)
			{
				DateTime ts = DateTime.Now.ToLocalTime();
				DefaultContentBase.WriteTopic(oldName, 
					"Redirect: " + newName + @"

This page was automatically generated when this topic (" + oldName.Name + ") was renamed to " + newName + " on " +
					ts.ToShortDateString() + " at " + ts.ToShortTimeString() + " by " + VisitorIdentityString + "." +
					"\nPlease update references to point to the new topic.");
			}

		}
	}
}
