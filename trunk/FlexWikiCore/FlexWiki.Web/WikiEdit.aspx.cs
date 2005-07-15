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
using System.Web.Mail;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using System.IO;
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

		string PostedTopicText
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

		void ProcessPost()
		{
			ProcessSave(true);
		}

		bool IsConflictingChange
		{
			get
			{
				if (!IsPost)
					return false;
				string lastEdit = Request.Form["TopicLastWrite"];
				if (lastEdit == "" || lastEdit == null)
					return false;	// it's probably new
				DateTime currentStamp;
        
				return TheFederation.TopicExists(TheTopic) && 
					!(currentStamp = TheFederation.GetTopicModificationTime(TheTopic)).ToString("s").Equals(lastEdit);
			}
		}

		void LogBannedAttempt()
		{
			string to = System.Configuration.ConfigurationSettings.AppSettings["SendBanNotificationsToMailAddress"];
			if (to == null || to == "")
				return;

			HTMLStringWriter w = new HTMLStringWriter();
			w.WritePara(String.Format("{0} attempted to post a change with banned content to the topic {1} on the FlexWiki site at {2}.", 
				HTMLStringWriter.Escape(VisitorIdentityString), HTMLStringWriter.Escape(TheTopic.Fullname), HTMLStringWriter.Escape((Request.Url.Host))));
			w.WritePara("Banned content includes:");
			w.WriteStartUnorderedList();
			string proposed = PostedTopicText;
			foreach (string each in TheFederation.BlacklistedExternalLinkPrefixes)
			{
				if (proposed.ToUpper().IndexOf(each.ToUpper()) >= 0)
					w.WriteListItem(HTMLStringWriter.Escape(each));
			}
			w.WriteEndUnorderedList();

			MailMessage msg = new MailMessage();
			msg.To = to;
			msg.From = "noreply_spam_report@" + Request.Url.Host;
			msg.Subject = "Banned content post attempt from " + VisitorIdentityString;
			msg.BodyFormat = MailFormat.Html;
			msg.Body = w.ToString();
			SendMail(msg);
		}

		bool IsBanned
		{
			get
			{
				string proposed = PostedTopicText;
				foreach (string each in TheFederation.BlacklistedExternalLinkPrefixes)
				{
					if (proposed.ToUpper().IndexOf(each.ToUpper()) >= 0)
						return true;
				}
				return false;
			}
		}

		protected void ProcessSave(bool back)
		{
			AbsoluteTopicName returnTo = null;

			//Check Null edits
			string oldContent = string.Empty;
			if (TheFederation.TopicExists(TheTopic))
				oldContent = TheFederation.Read(TheTopic);

			if (string.Compare(oldContent, PostedTopicText)!=0)
			{
				bool isDelete = Regex.IsMatch(PostedTopicText, "^delete$", RegexOptions.IgnoreCase);
				LogEvent ev;
				LogEvent.LogEventType evType = isDelete ? LogEvent.LogEventType.DeleteTopic : LogEvent.LogEventType.WriteTopic;
				ev = TheFederation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, TheTopic.ToString(), evType);
				
				try
				{
					AbsoluteTopicName newVersionName = new AbsoluteTopicName(TheTopic.Name, TheTopic.Namespace);
					newVersionName.Version = TopicName.NewVersionStringForUser(VisitorIdentityString);
					ContentBase cb = TheFederation.ContentBaseForNamespace(TheTopic.Namespace);
					cb.WriteTopicAndNewVersion(newVersionName.LocalName, PostedTopicText);		
					returnTo = TheTopic;

					if (isDelete)
					{
						returnTo = null;	// we won't be able to go back here because we're deleting it!
						TheFederation.DeleteTopic(TheTopic);
					}

					if (back && ReturnTopic != null)
						returnTo = new AbsoluteTopicName(ReturnTopic);
				}
				finally
				{
					if (Federation.GetPerformanceCounter(Federation.PerformanceCounterNames.TopicWrite) != null)
						Federation.GetPerformanceCounter(Federation.PerformanceCounterNames.TopicWrite).Increment();
					ev.Record();
				}
			}

			if (returnTo == null)
				Response.Redirect(RootUrl(Request));
			else
				Response.Redirect(TheLinkMaker.LinkToTopic(returnTo));

		}

		AbsoluteTopicName _TheTopic;
		protected AbsoluteTopicName TheTopic
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
				_TheTopic = new AbsoluteTopicName(topic);
				return _TheTopic;
			}
		}

		bool IsWritable
		{
			get
			{
				if (!TheFederation.TopicExists(TheTopic))
					return true;	// assume we can create
				return TheFederation.IsExistingTopicWritable(TheTopic);
			}
		}

		protected void DoPage()
		{
			if (IsPost && !IsConflictingChange && !IsBanned)
				ProcessPost();
			else
				ShowEditPage();
		}

		void ShowEditPage()
		{
			Response.Write("<body class='EditBody' width='100%' height='100%' scroll='no'>");

			Response.Write("<table width='100%' id='MasterTable' height='100%' border='0' cellpadding='0' cellspacing='0'><tr><td valign='top'>");
			Response.Write("<div id='MainRegion' class='EditMain'>");

			Response.Write(@"
		<div style='display: none'>
			<form id='Form2' method='post' target='previewWindow' ACTION='preview.aspx'>
				<textarea id='body' name='body'></textarea>
				<input  type='text' id='Text1' name='defaultNamespace' value ='" + TheTopic.Namespace  + @"'>
				<input  type='text' id='Text2' name='topic' value ='" + TheTopic.Name  + @"'>

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
			if (TheFederation.TopicExists(TheTopic))
				content = TheFederation.Read(TheTopic);
			if (IsPost && IsBanned)
				content = PostedTopicText;		// preserve what they asked for, even though we won't let them save

			#region Build up the list of templates and set the content accordingly
			string templateSelect = string.Empty;
			ContentBase currentContentBase = TheFederation.ContentBaseForNamespace(TheTopic.Namespace);

			// Process the topics looking for topics beginning with '_Template'.
			ArrayList templates = new ArrayList();
			foreach (AbsoluteTopicName topic in currentContentBase.AllTopics(false))
			{
				if (topic.Name.StartsWith("_Template"))
				{
					templates.Add(topic);

					if ("_templatedefault" == topic.Name.ToLower())
					{
						defaultContent = TheFederation.Read(topic);
					}
				}
			}

			if (0 != templates.Count)
			{
				// Build up a combo box for selecting the template.
				StringBuilder builder = new StringBuilder("<select name=\"templateSelect\" id=\"templateSelect\">\n");
				foreach (AbsoluteTopicName topic in templates)
				{
					builder.Append("\t<option value=\"");
					builder.Append(Formatter.EscapeHTML(TheFederation.Read(topic)));
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
					AbsoluteTopicName topicName = new AbsoluteTopicName(templateName, currentContentBase.Namespace);
					if ((content == null) && (true == currentContentBase.TopicExists(topicName)))
					{
						content = TheFederation.Read(topicName);
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
				Response.Write("<input type='text' style='display:none' name='UserSuppliedName' value ='" + Formatter.EscapeHTML(UserPrefix == null ? "" : UserPrefix) + "'>");
				if (TheFederation.TopicExists(TheTopic))
					Response.Write("<input type='text' style='display:none' name='TopicLastWrite' value ='" + Formatter.EscapeHTML(TheFederation.GetTopicModificationTime(TheTopic).ToString("s")) + "'>");
				Response.Write("<input type='text' style='display:none' name='Topic' value ='" + Formatter.EscapeHTML(TheTopic.ToString()) + "'>");
				if (ReturnTopic != null)
				{
					Response.Write("<input type='text' style='display:none' name='ReturnTopic' value ='" + Formatter.EscapeHTML(ReturnTopic) + "'>");
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
				ContentBase cb = TheFederation.ContentBaseForTopic(TheTopic);
				if (cb.TopicExists(TheTopic))
				{
					Response.Write("Make your changes to the text on the left and then select Save.");
				}
				else
				{
					Response.Write(@"
			<div class='CreateTopicWarning'>
				You are about to create a new topic called <b>" + TheTopic.Name + @"</b> in the <b>" +
						cb.FriendlyTitle + @"</b> namespace.");
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

			if (TheFederation.NoFollowExternalHyperlinks)
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
					Response.Write(@"<input style='font-size: x-small' type='text' id='UserNameEntryField' value ='" +
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
				The value of the property is everything on the line after the colon.
				Multiline properties use PropertyName:[ and then multiple lines and then ] on a
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

		void WriteTip(string id, string text)
		{
			Response.Write(@"<span onclick='javascript:ShowTip(""" + id + @""")'><b>" + text + "</b></span> ");
		}
	}
}
