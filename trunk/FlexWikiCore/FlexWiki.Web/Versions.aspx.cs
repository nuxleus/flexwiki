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
	/// Summary description for Versions.
	/// </summary>
	public class Versions : BasePage
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
			string topicString = Request.QueryString["topic"];
			AbsoluteTopicName topic = new AbsoluteTopicName(topicString);
			Response.Write("<h1>All Versions for " + topic.Fullname + "</h1>");
			
			IEnumerable changeList = TheFederation.GetTopicChanges(topic);

			// Now generate the page!

			bool first = true;
			string mostRecentVersion = null;
			foreach (TopicChange change in changeList)
			{
				if (first)
					mostRecentVersion = change.Version;
				string s = "";
				if (change.Version == topic.Version || (first &&  topic.Version == null))
				{
					s += "&rarr;&nbsp;";
				}
				else
					s += "&nbsp;&nbsp;&nbsp;&nbsp;";
				if (change.Timestamp.Date == DateTime.Now.Date)
					s += change.Timestamp.ToString("HH:mm");
				else
				{
					if (change.Timestamp.Date.Year == DateTime.Now.Date.Year)
						s += change.Timestamp.ToString("MMM d  H:mm");
					else
						s += change.Timestamp.ToString("MMM d yyyy  H:mm");
				}	
				s += "&nbsp;&nbsp;(" + change.Author + ")";	
				AbsoluteTopicName linkTo = change.Topic;
				if (first)
					linkTo.Version = null;	// don't include the version for the latest one
				Response.Write("<li><a href='" + TheLinkMaker.LinkToTopic(linkTo)  + "'>" + s + "</a>");
				first = false;
			}

		}

			
	}
}
