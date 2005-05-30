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
using System.Web.UI;
using System.Text;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for Versions.
	/// </summary>
	public class Versions : BasePage
	{
		protected System.Web.UI.WebControls.PlaceHolder phResult;

		private AbsoluteTopicName _TheTopic	= null;
		private IEnumerable changeList		= null;
		
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

		private void Page_Load(object sender, System.EventArgs e)
		{
			changeList = TheFederation.GetTopicChanges(TheTopic);
			ShowPage();
		}

		protected string GetTitle()
		{
			string title = TheFederation.GetTopicProperty(TheTopic, "Title");
			if (title == null || title == "")
			{
				title = string.Format("{0} - {1}", GetTopicName().FormattedName, GetTopicName().Namespace);
			}
			return HTMLStringWriter.Escape(title) + " Versions ";
		}
		

		protected AbsoluteTopicName TheTopic
		{
			get
			{
				if (_TheTopic != null)
				{
					return _TheTopic;
				}
				string topic;
				if (IsPost)
				{
					topic = Request.Form["Topic"];
				}
				else
				{
					topic = Request.QueryString["topic"];
				}
				_TheTopic = new AbsoluteTopicName(topic);
				return _TheTopic;
			}
		}

		protected string LinkToCompare()
		{
			return TheLinkMaker.LinkToCompare(TheTopic.Fullname, int.MinValue, int.MinValue);
		}

		protected void ShowPage()
		{
			
			// Now generate the page!
			string output = string.Empty;
			if (changeList != null)
			{
				output += GenerateList();
			}

			phResult.Controls.Add(new LiteralControl(output + "\n"));
		}

		private string GenerateList ()
		{
			bool first = true;
			StringBuilder output = new StringBuilder();
			int row = 0;
			int lastRow = ((ArrayList)changeList).Count -1;
			
			foreach (TopicChange change in changeList)
			{
				output.Append("<li>");
				//logic for selecting two versions to compare
				if (change.Version == TheTopic.Version || (first &&  TheTopic.Version == null))
				{
					output.Append(" (Current)");
				}
				else
				{
					output.AppendFormat(" (<a href='{0}' title='Show Difference with current version'>Current</a>)", TheLinkMaker.LinkToCompare(TheTopic.Fullname, 0, row));
				}


				if (row < lastRow)
				{
					output.AppendFormat(" (<a href='{0}' style='standardsButton' title='Show Difference with preceding version' tabindex={1}>Previous</a>)", TheLinkMaker.LinkToCompare(TheTopic.Fullname, row, row+1), row+1);
				}
				else
				{
					output.Append(" (Previous)");
				}


				output.AppendFormat(" <input type='radio'{1}name='oldid' value='{0}' title='Select an older version to compare'{2} />&nbsp;", row, ((first)? " style='visibility:hidden' " : ""), ((row==1)? "  checked='checked'" : ""));
				output.AppendFormat(" <input type='radio' name='diff' value='{0}' title='Select a newer version to compare'{1} />", row, ((first)? "  checked='checked'" : ""));

				if (change.Timestamp == DateTime.MinValue)
				{
					output.Append("???");
				}
				else 
				{
					output.Append("&nbsp;&nbsp;<span class='version'><a href='" + TheLinkMaker.LinkToTopic(change.Topic)  + "' title='Show this version' >");
					if (change.Timestamp.Date == DateTime.Now.Date)
					{
						output.Append(" Today, " + change.Timestamp.ToString("HH:mm"));
					}
					else
					{
						if (change.Timestamp.Date.Year == DateTime.Now.Date.Year)
						{
							output.Append(change.Timestamp.ToString("MMM dd - HH:mm"));
						}
						else
						{
							output.Append(change.Timestamp.ToString("MMM dd yyyy - HH:mm"));
						}
					}
					output.Append("</a></span>");
				}


				output.Append(" <span class='user'>" + change.Author + "</span>");
				first = false;
				output.Append("</li>");
				row++;
			}
			return output.ToString();
		}


			
	}
}
