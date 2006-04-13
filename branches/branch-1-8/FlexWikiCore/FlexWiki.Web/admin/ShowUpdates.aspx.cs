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
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki.Web;

namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for ShowUpdates.
	/// </summary>
	public class ShowUpdates : AdminPage
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected void ShowUpdateInfo()
		{
			UIResponse.ShowPage("Federation Update History", new UIResponse.MenuWriter(ShowAdminMenu), new UIResponse.BodyWriter(ShowMain));
		}
   
		void ShowMain()
		{
			UIResponse.Write("<table cellpadding='2' cellspacing='0' border='0'>");
			for (int index = this.UpdateMonitor.Updates.Count - 1; index >= 0; index--)
			{
				UpdateMonitor.UpdateInfo update = (UpdateMonitor.UpdateInfo)(this.UpdateMonitor.Updates[index]);
				UIResponse.Write("<tr><td class='UpdateDivider' colspan='2' valign='top'>" + update.Timestamp.ToString() + "</td></tr>");

				if (update.Update.FederationPropertiesChanged)
					UIResponse.Write("<tr><td class='UpdateKey' valign='top'>FederationPropertiesChanged</td><td  class='UpdateValue' valign='top'>YES</td></tr>");

				if (update.Update.NamespaceListChanged)
					UIResponse.Write("<tr><td  class='UpdateKey' valign='top'>NamespaceListChanged</td><td  class='UpdateValue' valign='top'>YES</td></tr>");

				if (update.Update.CreatedTopics.Count > 0)
				{
					UIResponse.Write("<tr><td  class='UpdateKey' valign='top'>Topics Created</td><td  class='UpdateValue' valign='top'>");
					foreach (AbsoluteTopicName t in update.Update.CreatedTopics)
					{
						UIResponse.Write(EscapeHTML(t.FullnameWithVersion));
						UIResponse.Write("<br />");
					}
					UIResponse.Write("</td></tr>");
				}

				if (update.Update.UpdatedTopics.Count > 0)
				{
					UIResponse.Write("<tr><td  class='UpdateKey' valign='top'>Topics Updated</td><td  class='UpdateValue' valign='top'>");
					foreach (AbsoluteTopicName t in update.Update.UpdatedTopics)
					{
						UIResponse.Write(EscapeHTML(t.FullnameWithVersion));
						UIResponse.Write("<br />");
					}
					UIResponse.Write("</td></tr>");
				}


				if (update.Update.DeletedTopics.Count > 0)
				{
					UIResponse.Write("<tr><td  class='UpdateKey' valign='top'>Topics Deleted</td><td  class='UpdateValue' valign='top'>");
					foreach (AbsoluteTopicName t in update.Update.DeletedTopics)
					{
						UIResponse.Write(EscapeHTML(t.FullnameWithVersion));
						UIResponse.Write("<br />");
					}
					UIResponse.Write("</td></tr>");
				}

				// Now properties

				if (update.Update.AllTopicsWithChangedProperties.Count > 0)
				{
					UIResponse.Write("<tr><td  class='UpdateKey' valign='top'>Topics with Property Updates</td><td  class='UpdateValue' valign='top'>");
					UIResponse.Write("<table cellpadding='2' cellspacing='0' border='0'>");
					foreach (AbsoluteTopicName t in update.Update.AllTopicsWithChangedProperties)
					{
						UIResponse.Write("<tr><td class='UpdateDivider' colspan=2 valign='top'>" + t.FullnameWithVersion + "</td></tr>");
						
						IList props;

						props = update.Update.AddedPropertiesForTopic(t);
						if (props.Count > 0)
						{
							bool first = true;
							foreach (string propertyname in props)
							{
								UIResponse.Write("<tr><td class='UpdateKey' valign='top'>");
								if (first)
									UIResponse.Write("Added");
								UIResponse.Write("</td><td  class='UpdateValue' valign='top'>");
								UIResponse.Write(propertyname);
								UIResponse.Write("</td>");
								UIResponse.Write("</tr>");
								first = false;
							}
						}

						props = update.Update.ChangedPropertiesForTopic(t);
						if (props.Count > 0)
						{
							bool first = true;
							foreach (string propertyname in props)
							{
								UIResponse.Write("<tr><td class='UpdateKey' valign='top'>");
								if (first)
									UIResponse.Write("Updated");
								UIResponse.Write("</td><td  class='UpdateValue' valign='top'>");
								UIResponse.Write(propertyname);
								UIResponse.Write("</td>");
								UIResponse.Write("</tr>");
								first = false;
							}
						}

						props = update.Update.RemovedPropertiesForTopic(t);
						if (props.Count > 0)
						{
							bool first = true;
							foreach (string propertyname in props)
							{
								UIResponse.Write("<tr><td class='UpdateKey' valign='top'>");
								if (first)
									UIResponse.Write("Deleted");
								UIResponse.Write("</td><td  class='UpdateValue' valign='top'>");
								UIResponse.Write(propertyname);
								UIResponse.Write("</td>");
								UIResponse.Write("</tr>");
								first = false;
							}
						}


					}
					UIResponse.Write("</table>");
					UIResponse.Write("</td></tr>");
				}

			}
			UIResponse.Write("</table>");
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
	}
}
