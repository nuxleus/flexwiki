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
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for WikiEdit.
	/// </summary>
	public class WikiEdit : BasePage
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

		private string PostedTopicText
		{
			get
			{
				return Request.Form["Text1"];
			}
		}

		protected string ReturnTopic
		{
			get
			{
				string answer = null;
				if (IsPost)
					answer = Request.Form["ReturnTopic"];
				else
					answer = Request.QueryString["return"];
				if (answer == "")
					answer = null;
				return answer;
			}
		}

		private void ProcessPost()
		{
			ProcessSave(true);
		}

		private bool IsConflictingChange
		{
			get
			{
				if (!IsPost)
					return false;
				string lastEdit = Request.Form["TopicLastWrite"];
				if (lastEdit == "" || lastEdit == null)
					return false;	// it's probably new
				DateTime currentStamp;
        
				return Federation.TopicExists(TheTopic) && 
					!(currentStamp = Federation.GetTopicModificationTime(TheTopic)).ToString("s").Equals(lastEdit);
			}
		}

		private void LogBannedAttempt()
		{
			string to = System.Configuration.ConfigurationManager.AppSettings["SendBanNotificationsToMailAddress"];
			if (to == null || to == "")
				return;

			HtmlStringWriter w = new HtmlStringWriter();
			w.WritePara(String.Format("{0} attempted to post a change with banned content to the topic {1} on the FlexWiki site at {2}.", 
				HtmlStringWriter.Escape(VisitorIdentityString), HtmlStringWriter.Escape(TheTopic.QualifiedName), HtmlStringWriter.Escape((Request.Url.Host))));
			w.WritePara("Banned content includes:");
			w.WriteStartUnorderedList();
			string proposed = PostedTopicText;
			foreach (string each in Federation.BlacklistedExternalLinkPrefixes)
			{
				if (proposed.ToUpper().IndexOf(each.ToUpper()) >= 0)
					w.WriteListItem(HtmlStringWriter.Escape(each));
			}
			w.WriteEndUnorderedList();

      string from = "noreply_spam_report@" + Request.Url.Host;
      MailMessage msg = new MailMessage(from, to);
			msg.Subject = "Banned content post attempt from " + VisitorIdentityString;
			msg.IsBodyHtml = true;
			msg.Body = w.ToString();
			SendMail(msg);
		}

		private bool IsBanned
		{
			get
			{
				string proposed = PostedTopicText;
				foreach (string each in Federation.BlacklistedExternalLinkPrefixes)
				{
					if (proposed.ToUpper().IndexOf(each.ToUpper()) >= 0)
						return true;
				}
				return false;
			}
		}

		protected void ProcessSave(bool back)
		{
			NamespaceQualifiedTopicVersionKey returnTo = null;

			//Check Null edits
			string oldContent = string.Empty;
			if (Federation.TopicExists(TheTopic))
				oldContent = Federation.Read(TheTopic);

			if (string.Compare(oldContent, PostedTopicText)!=0)
			{
				bool isDelete = Regex.IsMatch(PostedTopicText, "^delete$", RegexOptions.IgnoreCase);
				LogEvent ev;
				LogEventType evType = isDelete ? LogEventType.DeleteTopic : LogEventType.WriteTopic;
				ev = Federation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, TheTopic.ToString(), evType);
				
				try
				{
					NamespaceQualifiedTopicVersionKey newVersionName = new NamespaceQualifiedTopicVersionKey(TheTopic.LocalName, TheTopic.Namespace);
					newVersionName.Version = TopicVersionKey.NewVersionStringForUser(VisitorIdentityString);
					NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(TheTopic.Namespace);
					storeManager.WriteTopicAndNewVersion(newVersionName.LocalName, PostedTopicText, VisitorIdentityString);		
					returnTo = TheTopic;

					if (isDelete)
					{
						returnTo = null;	// we won't be able to go back here because we're deleting it!
						Federation.DeleteTopic(TheTopic);
					}

					if (back && ReturnTopic != null)
						returnTo = new NamespaceQualifiedTopicVersionKey(ReturnTopic);
				}
				finally
				{
					if (Federation.GetPerformanceCounter(PerformanceCounterNames.TopicWrite) != null)
						Federation.GetPerformanceCounter(PerformanceCounterNames.TopicWrite).Increment();
					ev.Record();
				}
			}

			if (returnTo == null)
				Response.Redirect(RootUrl(Request));
			else
				Response.Redirect(TheLinkMaker.LinkToTopic(returnTo));

		}

		private NamespaceQualifiedTopicVersionKey _TheTopic;
		protected NamespaceQualifiedTopicVersionKey TheTopic
		{
			get
			{
				if (_TheTopic != null)
					return _TheTopic;
				string topic;
				if (IsPost)
					topic = Request.Form["Topic"];
				else
					topic = Request.QueryString["topic"];
				_TheTopic = new NamespaceQualifiedTopicVersionKey(topic);
				return _TheTopic;
			}
		}

		private bool IsWritable
		{
			get
			{
				if (!Federation.TopicExists(TheTopic))
					return true;	// assume we can create
				return Federation.IsExistingTopicWritable(TheTopic);
			}
		}

		protected void DoPage()
		{
			if (IsPost && !IsConflictingChange && !IsBanned)
				ProcessPost();
			else
				ShowEditPage();
		}

		private void ShowEditPage()
		{
			Response.Write("<body class='EditBody' width='100%' height='100%' scroll='no'>");

			Response.Write("<table width='100%' id='MasterTable' height='100%' border='0' cellpadding='0' cellspacing='0'><tr><td valign='top'>");
			Response.Write("<div id='MainRegion' class='EditMain'>");

			Response.Write(@"
		<div style='display: none'>
			<form id='Form2' method='post' target='previewWindow' ACTION='Preview.aspx'>
				<textarea id='body' name='body'></textarea>
				<input  type='text' id='Text1' name='defaultNamespace' rawValue ='" + TheTopic.Namespace  + @"'>
				<input  type='text' id='Text2' name='topic' rawValue ='" + TheTopic.LocalName  + @"'>

			</form>
		</div>
		<div class='EditZone' id='EditZone' >
			<form id='Form1' method='post'>
			<textarea class='EditBox' onkeydown='if (document.all && event.keyCode == 9) {  event.returnValue= false; document.selection.createRange().text = String.fromCharCode(9)} ' rows='20' cols='50' name='Text1' onfocus='textArea_OnFocus(event)' onblur='textArea_OnBlur(event)'>");

			string defaultContent = @"
Check out the formatting tips on the right for help formatting and making links.

Use the template below:

----

Summary: add a one or paragraph summary or description of what's discussed here;  put yours after 'Summary:'

Add your wiki text here.

";
			string content = null;
			if (Federation.TopicExists(TheTopic))
				content = Federation.Read(TheTopic);
			if (IsPost && IsBanned)
				content = PostedTopicText;		// preserve what they asked for, even though we won't let them save

			#region Build up the list of templates and set the content accordingly
			string templateSelect = string.Empty;
			NamespaceManager currentContentBase = Federation.NamespaceManagerForNamespace(TheTopic.Namespace);

			// Process the topics looking for topics beginning with '_Template'.
			ArrayList templates = new ArrayList();
			foreach (TopicName topic in currentContentBase.AllTopics(ImportPolicy.DoNotIncludeImports))
			{
				if (topic.LocalName.StartsWith("_Template"))
				{
					templates.Add(topic);

					if ("_templatedefault" == topic.LocalName.ToLower())
					{
						defaultContent = Federation.Read(topic);
					}
				}
			}

			if (0 != templates.Count)
			{
				// Build up a combo box for selecting the template.
				StringBuilder builder = new StringBuilder("<select name=\"templateSelect\" id=\"templateSelect\">\n");
				foreach (NamespaceQualifiedTopicVersionKey topic in templates)
				{
					builder.Append("\t<optionrawValuee=\"");
					builder.Append(Formatter.EscapeHTML(Federation.Read(topic)));
					builder.Append("\">");
					string topicStart = "_ Template";
					string name = topic.FormattedName;
					builder.Append(name.Substring(topicStart.Length));
					builder.Append("</option>\n");
				}
				builder.Append("</select>\n");

				templateSelect = builder.ToString();

				// Check to see if a template was specified in the request.
				if (null != this.Request.QueryString["template"])
				{
					string templateName = this.Request["template"];
					NamespaceQualifiedTopicVersionKey topicVersionKey = new NamespaceQualifiedTopicVersionKey(templateName, currentContentBase.Namespace);
					if ((content == null) && (true == currentContentBase.TopicExists(topicVersionKey.LocalName, 
                        ImportPolicy.DoNotIncludeImports)))
					{
						content = Federation.Read(topicVersionKey);
					}
				}
			}
			#endregion

			if (content == null)
			{
				content = defaultContent;
			}

			Response.Write(Formatter.EscapeHTML(content));
			Response.Write(@"</textarea>");
			if (IsWritable)
			{
				Response.Write("<input type='text' style='display:none' name='UserSuppliedName' rawValue ='" + Formatter.EscapeHTML(UserPrefix == null ? "" : UserPrefix) + "'>");
				if (Federation.TopicExists(TheTopic))
					Response.Write("<input type='text' style='display:none' name='TopicLastWrite' rawValue ='" + Formatter.EscapeHTML(Federation.GetTopicModificationTime(TheTopic).ToString("s")) + "'>");
				Response.Write("<input type='text' style='display:none' name='Topic' rawValue ='" + Formatter.EscapeHTML(TheTopic.ToString()) + "'>");
				if (ReturnTopic != null)
				{
					Response.Write("<input type='text' style='display:none' name='ReturnTopic' rawValue ='" + Formatter.EscapeHTML(ReturnTopic) + "'>");
				}
			}

			Response.Write(@"</form></div>");

			Response.Write("</div></td>");
			Response.Write("<td valign='top' id='Sidebar' class='Sidebar'>");

			Response.Write("<table style='height: 100%'><tr><td height='100%' valign='top'>");

			/////////////////////////////
			OpenPane(Response.Output, "Edit&nbsp;" + BELString.MaxLengthString2(Formatter.EscapeHTML(TheTopic.ToString()), 20, "..."));
			if (IsWritable)
			{
				NamespaceManager storeManager = Federation.NamespaceManagerForTopic(TheTopic);
				if (storeManager.TopicExists(TheTopic.LocalName, ImportPolicy.DoNotIncludeImports))
				{
					Response.Write("Make your changes to the text on the left and then select Save.");
				}
				else
				{
					Response.Write(@"
			<div class='CreateTopicWarning'>
				You are about to create a new topic called <b>" + TheTopic.LocalName + @"</b> in the <b>" +
						storeManager.FriendlyTitle + @"</b> namespace.");
					Response.Write("<P>Please be sure you are creating this topic in the desired namespace.</p>");
					Response.Write(@"</div>");
				}
			}
			else
			{
				Response.Write("<span class='ReadOnlyStripe'>You do not have permission to change this topic.</span>");
			}

			ClosePane(Response.Output);


			if (IsConflictingChange)
			{
				OpenPane(Response.Output, "Conflicting Change");
				Response.Write("<div class='ConflictingChange'>Your change can not be saved.</div>");
				Response.Write("The topic has been changed since you started to edit it and if you saved your changes, the other changes would be lost.");
				Response.Write(" Please save your changes somewhere and edit again (no merge functionality yet).  You are now being shown the new version on the left.  To recover your previous edits, use the Back button.");
				ClosePane(Response.Output);
			}

			if (IsPost && IsBanned)
			{
				OpenPane(Response.Output, "Banned URLs");
				Response.Write("<div class='BannedChange'>Your change can not be saved.</div>");
				Response.Write("The changes you are trying to save include banned URLs.");
				ClosePane(Response.Output);
				LogBannedAttempt();
			}

			if (Federation.NoFollowExternalHyperlinks)
			{
				OpenPane(Response.Output, "External Hyperlinks");
				Response.Write("<img src='" + TheLinkMaker.LinkToImage("images/NoFollowNoSpam.gif") + "' align='right'>External hyperlinks will not be indexed by search engines.");
				ClosePane(Response.Output);
				LogBannedAttempt();
			}


			///////////////////////////////
			if (IsWritable)
			{
				OpenPane(Response.Output, "Attribution");
				if (User.Identity.IsAuthenticated)
				{
					Response.Write("Your edit will be attributed to: <b>" + Formatter.EscapeHTML(VisitorIdentityString) + "</b>.");
				}
				else
				{
					Response.Write("Your edit will be attributed to: <b>" + Formatter.EscapeHTML(VisitorIdentityString) + "</b>.<br>");

					Response.Write("<div id='ShowAttribution' style='display: block'><a onclick=\"javascript:Swap('ShowAttribution', 'HideAttribution')\">Change this...</a></div>");
					Response.Write("<div id='HideAttribution' style='display: none'>");
					Response.Write("<a onclick=\"javascript:Swap('ShowAttribution', 'HideAttribution')\">Hide this...</a><br>");

					Response.Write("You can change part of this by entering your preferred user identity here (e.g., an email address):<br>");
					Response.Write(@"<input style='font-size: x-small' type='text' id='UserNameEntryField' rawValue ='" +
						(UserPrefix == null ? "" : Formatter.EscapeHTML(UserPrefix)) + "'>");
					Response.Write("</div>");

				}
				ClosePane(Response.Output);
			}

			///////////////////////////////

			if (IsWritable)
			{
				OpenPane(Response.Output, "Formatting Tips");
				Response.Write("<div id='ShowTips' style='display: block'><a onclick=\"javascript:Swap('ShowTips', 'HideTips')\">Show tips...</a></div>");
				Response.Write("<div id='HideTips' style='display: none'>");
				Response.Write("<a onclick=\"javascript:Swap('ShowTips', 'HideTips')\">Hide tips...</a><br>");

				Response.Write("Click on a subject for more information about formatting rules: ");
				WriteTip("tip_boldtip", "Bold");
				WriteTip("tip_italicstip", "Italics");
				WriteTip("tip_headingtip", "Headings");
				WriteTip("tip_hypertip", "Hyperlinks");
				WriteTip("tip_linestip", "Lines");
				WriteTip("tip_liststip", "Lists");
				WriteTip("tip_tablestip", "Tables");
				WriteTip("tip_emoticonstip", "Emoticons");
				WriteTip("tip_pretip", "Preformatted");
				WriteTip("tip_imagetip", "Images");
				WriteTip("tip_proptip", "Properties");
				Response.Write("<br><div class='TipArea' id='TipArea'></div>");

				Response.Write(@"
<div style='display: none'>
		<div id='tip_proptip'>
			<div class='tipBody'>
				A line that starts with a wiki word and a colon identifies a property.
				The rawValue of the property is everything on the line after the colon.
				Multiline imports use PropertyName:[ and then multiple lines and then ] on a
				blank line to mark the end.
			</div>
		</div>
		<div id='tip_imagetip'>
			<div class='tipBody'>
				Any URL that ends with .gif, .jpeg, .jpg or .png will be turned into an image
				tag to display the actual image.
			</div>
		</div>
		<div id='tip_pretip'>
			<div class='tipBody'>
				Any line that starts with at least one space will be fixed-width formatted.
				Good for code and simple tables.
			</div>
		</div>
		<div id='tip_liststip'>
			<div class='tipBody'>
				Start a line with a tab (or 8 spaces) followed by a star '*'.
				Two tabs (or 16 spaces) indents to the next level, etc.
				For ordered lists, use '1.' instead of '*'
			</div>
		</div>
		<div id='tip_linestip'>
			<div class='tipBody'>
				Four hyphens makes a horizontal rule.
			</div>
		</div>
		<div id='tip_boldtip'>
			<div class='tipBody'>
				Surround the text with three ticks (''').
				For example, '''<b>this text will be bold</b>'''
			</div>
		</div>
		<div id='tip_italicstip'>
			<div class='tipBody'>
				Surround the text with two ticks ('').
				For example, ''<i>this text will be italic</i>''
			</div>
		</div>
		<div id='tip_headingtip'>
			<div class='tipBody'>
				Bang (!) at the start of a line for H1.
				Bang Bang (!!) at the start of a line for H2.
				And so on...
			</div>
		</div>
		<div id='tip_hypertip'>
			<div class='tipBody'>
				Any PascalCased word becomes a link.
				Surrounding a word with square brackets [ word ] will make non-pascalcased
				words into links; generally this is considered 'odd'.
				Any URL becomes a link (http://www.msn.com)
			</div>
		</div>
		<div id='tip_emoticonstip'>
			<div class='tipBody'>
				All the common emoticons like :-) and :-( are turned into the apprpriate
				graphical images (like in messenger).
			</div>
		</div>
		<div id='tip_tablestip'>
			<div class='tipBody'>
				A line that starts and ends with || is a table row.  Cells are divided by ||.
				For example: <br />
				||Region || Sales||<br />
				||East || $100||<br />
				||West || $100||<br />
			</div>
		</div>
</div>
");

				Response.Write("</div>");
				ClosePane(Response.Output);
			}

			//////////////////////////////

			if (IsWritable && templateSelect.Length > 0)
			{
				// Render the template selection dropdown.
				OpenPane(Response.Output, "Topic Templates");
				Response.Write("Select a new template:<br/>\n");
				Response.Write(templateSelect);
				Response.Write("&nbsp;<input type=\"image\" src=\"" + TheLinkMaker.LinkToImage("images/go-dark.gif") + "\" title=\"Change Template\" onclick=\"javascript:ChangeTemplate('templateSelect');\">");
				ClosePane(Response.Output);
			}

			//////////////////////////////

			if (IsWritable)
			{
				// generate cancel, save, search, preview, and Save&Return buttons
				Response.Write(@"
<div style='margin-top: 12px; text-align: center'><table>
<tr><td><button onclick='javascript:Cancel()' name='CancelButton'>Cancel</button></td>
<td><button onclick='javascript:Save()' name='SaveButton'>Save</button></td></tr>
<tr><td><Button OnClick='javascript:search()' ID='button3'>Search</Button></td>
<td><Button OnClick='javascript:preview()' ID='button1'>Preview</Button></td></tr>");

				if (ReturnTopic != null)
				{
					Response.Write("<tr><td colspan='2'><button onclick='javascript:SaveAndReturn()'  name='SaveButton'>Save and Back</button></td></tr>");
				}

				Response.Write("</table></div>");
			}

			Response.Write("</td></tr></table>");

			Response.Write("</td></tr></table>");

			Response.Write("</body>");

		}

		private void WriteTip(string id, string text)
		{
			Response.Write(@"<span onclick='javascript:ShowTip(""" + id + @""")'><b>" + text + "</b></span> ");
		}
	}
}
