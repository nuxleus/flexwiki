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
using System.IO;
using FlexWiki.Web;
using System.Web.Mail;


namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Admin.
	/// </summary>
	public class RequestNamespace : BasePage
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
			if (SendRequestsTo == null)
			{
				Response.Write(@"<h3>This feature is not enabled on this installation of FlexWiki.</h3>");
				return;
			}


			Response.Write(@"<fieldset><legend class='DialogTitle'>Request Namespace</legend>");

			if (IsPost)
				ProcessPost();
			else
				ProcessFirstVisit();
 
			Response.Write(@"</fieldset>");
		}

		void ProcessPost()
		{
			FormValues values = ReadValuesFromPost();
            string validationFailure = Validate(values);
			if (validationFailure != null)
			{
				Response.Write("<p><font color='red'><b>Error:</b> " + EscapeHTML(validationFailure) + "</font></p>");
				WriteForm(values);
				return;
			}
			// Looks good -- make the change
			SaveChanges(values);
		}

		string Validate(FormValues values)
		{
			if (values.Namespace == null || values.Namespace == "")
				return "Namespace must be specified";
			if (values.Title == null || values.Title == "")
				return "Title must be specified";
			if (TheFederation.ContentBaseForNamespace(values.Namespace) != null)
				return "The namespace you selected already exists.  Please select another name.";
			if (values.Description == null || values.Description == "")
				return "Description must be specified";
			if (values.Contact == null || values.Contact == "")
				return "You must supply your contact information (which should be a valid email address)";
			return null;		
		}

		void SaveChanges(FormValues values)
		{
			DoCreate(values);
		}

		void DoCreate(FormValues values)
		{
			MailMessage msg = new MailMessage();
			string adminMail = SendRequestsTo;
			Uri uri = Request.Url;
			UriBuilder b = new UriBuilder(uri.Scheme, uri.Host, uri.Port, "/admin/EditNamespace.aspx", 
				"?contact=" +  HttpUtility.UrlEncode(values.Contact) + 
				"&ns=" +  HttpUtility.UrlEncode(values.Namespace) + 
				"&title=" +  HttpUtility.UrlEncode(values.Title) + 
				"&description=" +  HttpUtility.UrlEncode(values.Description));
			string link = b.ToString();

			msg.To = adminMail;
			msg.BodyFormat = MailFormat.Html;
			msg.From = values.Contact;
			msg.Subject = "FlexWiki namespace request - " + values.Namespace;
			msg.Body = @"<p>You have received a request to create a FlexWiki namespace.

<p><b>Contact:</b> " + values.Contact + @"<br />
<b>Namespace:</b> " + values.Namespace + @"<br />
<b>Title:</b> " + values.Title + @"<br />
<b>Description:</b> " + values.Description + @"<br />

<p>To processed with creation of this namespace, you can use <a href='" + link + "'>this link</a>.";
			string fail = SendMail(msg);
			if (fail == null)
				Response.Write(@"<p>Your request to create a FlexWiki namespace (" + EscapeHTML(values.Namespace) + @") has been forwarded to the administration staff for this site (" + EscapeHTML(adminMail) + ").");
			else
			{
				Response.Write(@"<p>Your request to create a FlexWiki namespace (" + EscapeHTML(values.Namespace) + @") can not be forwarded to the administration staff at this time.  An error occurred trying to send them mail with your request.  You can try again later or send the information in mail manually to: " + EscapeHTML(adminMail) + ".");
				Response.Write(@"<p>The error that occurred is: <pre>
" + EscapeHTML(fail) 
+ "</pre>");
			}

			// Also send confirmation mail
			MailMessage msg2 = new MailMessage();

			msg2.To = values.Contact;
			msg2.From = values.Contact;
			msg2.Subject = "FlexWiki namespace request - " + values.Namespace;
			msg2.BodyFormat = MailFormat.Html;
			msg2.Body = @"<p>This message is confirmation of your request to create a FlexWiki namespace.  

<p><b>Contact:</b> " + values.Contact + @"<br />
<b>Namespace:</b> " + values.Namespace + @"<br />
<b>Title:</b> " + values.Title + @"<br />
<b>Description:</b> " + values.Description + @"<br />

";
			fail = SendMail(msg2);
			if (fail == null)
				Response.Write(@"<p>A confirmation mail describing your request to create a FlexWiki namespace (" + EscapeHTML(values.Namespace) + @") has been sent to your email address.");
			else
			{
				Response.Write(@"<p>A confirmation mail describing your request could not be forwarded to your email address.  An error occurred trying to send the email.");
				Response.Write(@"<p>The error that occurred is: <pre>
" + EscapeHTML(fail) 
					+ "</pre>");
			}
		}

		FormValues ReadValuesFromPost()
		{
			FormValues answer = new FormValues();
			answer.Namespace = Request.Form["ns"];
			answer.Title = Request.Form["title"];
			answer.Description = Request.Form["description"];
			answer.Contact = Request.Form["contact"];
			return answer;
		}

		void ProcessFirstVisit()
		{
			FormValues values;
			values = NewDefaultValues();
			WriteForm(values);
		}

		FormValues NewDefaultValues()
		{
			FormValues answer = new FormValues();
			return answer;
		}
		
		class FormValues
		{
			public string Namespace;
			public string Contact;
			public string Title;
			public string Description;
		}

		void WriteForm(FormValues values)
		{
			// Write the form
			Response.Write("<form method='post' ACTION='RequestNamespace.aspx'>");
			StartFields();
			WriteInputField("ns", "Namespace", "The full identifier for the namespace (e.g., Microsoft.Projects.Wiki or FlexWiki.Dev.Testing)", values.Namespace);
			WriteInputField("title", "Title", "A short title for this namespace", values.Title);
			WriteTextAreaField("description", "Description", "A description for the namespace (can use Wiki formatting)", values.Description);
			WriteInputField("contact", "Contact", "Specify a contact for this namespace (a valid email address)", values.Contact);
			EndFields();
			Response.Write("<input  type='submit'  name='OK' value ='Submit Request'>");
			Response.Write("</form>");
		}

		void WriteInputField(string fieldName, string fieldLabel, string help, string value)
		{	
			WriteFieldHTML(fieldLabel, help, 
				"<input type='text' size='50' value='" + EscapeHTML(value) + "' name='" + fieldName + "'>");
		}

		void WriteTextAreaField(string fieldName, string fieldLabel, string help, string value)
		{	
			string html = "<textarea onkeydown='if (document.all && event.keyCode == 9) {  event.returnValue= false; document.selection.createRange().text = String.fromCharCode(9)} ' rows='5' cols='40' name='" + fieldName +"'>";
			html += EscapeHTML(value);
			html += "</textarea>";
			WriteFieldHTML(fieldLabel, help, html);
		}

		void WriteFieldHTML(string fieldLabel, string help, string html)
		{
			Response.Write("<tr>");
			Response.Write("<td valign='top' class='FieldName'>" + EscapeHTML(fieldLabel) + ":</td>");
			Response.Write("<td valign='top' class='FieldValue'>" + html + "</td>");
			Response.Write("</tr>");
			Response.Write("<tr>");
			Response.Write("<td><td class='FieldHelp'>" + (help) + "</td>");
			Response.Write("</tr>");
		}

		bool IsPost
		{
			get
			{
				return Request.HttpMethod == "POST";
			}
		}

		void EndFields()
		{
			Response.Write("</table>");
		}

		void StartFields()
		{
			Response.Write("<table class='FieldTable' cellpadding=2 cellspacing=0>");
		}

	}
}
