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


namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Admin.
	/// </summary>
	public class EditNamespace : AdminPage
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
			string ns = Namespace;
			string tail = (ns == null ? "" :  " (" + EscapeHTML(ns) + ")");
			string title;
			if (IsCreate)
				title = "Create Namespace";
			else
				title = "Edit Namespace";
			title += tail;
			Response.Write(@"<fieldset><legend class='DialogTitle'>" + title + "</legend>");

			if (IsPost)
				ProcessPost();
			else
				ProcessFirstVisit();
 
			Response.Write(@"</fieldset>");
		}

		bool IsCreate
		{
			get
			{
				string ns = Namespace;
				if (ns == null)
					return true;
				if (TheFederation.ContentBaseForNamespace(ns) == null)
					return true;
				return false;
			}
		}

		void ProcessPost()
		{
			FormValues values = ReadValuesFromPost();
            string validationFailure = Validate(values);
			if (validationFailure != null)
			{
				Response.Write("<p><b>Error:</b> " + EscapeHTML(validationFailure) + "</p>");
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
			return null;		
		}

		void SaveChanges(FormValues values)
		{
			// Three possible operations: rename, create, edit
			bool isRename = values.OriginalNamespace != null && values.OriginalNamespace != "" && (values.OriginalNamespace != values.Namespace) && 
				TheFederation.ContentBaseForNamespace(values.OriginalNamespace) != null;
			if (isRename)
				DoRename(values);
			else 
			{
				if (TheFederation.ContentBaseForNamespace(values.Namespace) == null)
					DoCreate(values);
				else
					DoEdit(values);
			}
		}

		void DoRename(FormValues values)
		{
			Response.Write("<p>Renaming namespaces is not yet supported.  You need to do this manually for now.</p>");
		}

		void DoCreate(FormValues values)
		{
			Response.Write("<p>Creating namespace: " + EscapeHTML(values.Namespace) + "</p>");

			string dir = values.Directory;
			if (dir == null || dir == "")
				dir = TheFederation.DefaultDirectoryForNewNamespaces + "\\" + values.Namespace;

			ContentBase cb = TheFederation.ContentBaseForRoot(dir, true);
			cb.Namespace = values.Namespace;
			TheFederation.Register(values.Namespace, cb.Root);
			AbsoluteTopicName defTopic = cb.DefinitionTopicName;
			cb.WriteTopic(defTopic, "Namespace: " + values.Namespace + Environment.NewLine);
			TheFederation.SetTopicProperty(defTopic, "Contact", values.Contact, false);
			TheFederation.SetTopicProperty(defTopic, "Title", values.Title, false);
			TheFederation.SetTopicProperty(defTopic, "Description", values.Description, false);

			// Set imported namespaces, if any
			string defaultImportedNamespaces = System.Configuration.ConfigurationSettings.AppSettings["DefaultImportedNamespaces"];
			if (defaultImportedNamespaces != null)
				TheFederation.SetTopicProperty(defTopic, "Import", defaultImportedNamespaces, false);

			SetConfiguredDirectoryForNamespace(values.Namespace, dir);
			
			cb.WriteTopic(new AbsoluteTopicName("HomePage", values.Namespace), "!Welcome to Wiki..." + Environment.NewLine);
			
			Response.Write("<p>Namespace created (content stored in " + EscapeHTML(dir) + ").</p>");
			Response.Write("<p>Back to <a href='namespaces.aspx'>namespace list</a>.</p>");

			string link = TheLinkMaker.LinkToTopic(new AbsoluteTopicName(cb.HomePage, cb.Namespace));
			Uri uri = new Uri(link);
			link = uri.ToString();

			System.Web.Mail.MailMessage msg = new System.Web.Mail.MailMessage();
			msg.To = values.Contact;
			if (SendRequestsTo != null)
				msg.Cc = SendRequestsTo;
			msg.BodyFormat = System.Web.Mail.MailFormat.Html;
			msg.From = SendRequestsTo;
			msg.Subject = "FlexWiki namespace created - " + values.Namespace;
			msg.Body = @"<p>Your FlexWiki namespace (" + EscapeHTML(values.Namespace) + @") has been created.
<p>You may visit the home page for your namespace at <a href='" + link + "'>" + EscapeHTML(link) + "</a>";

			string fail = SendMail(msg);
			if (fail == null)
				Response.Write(@"<p>Mail has been sent notifying the contact that their namespace has been created.</p>");
			else
			{
				Response.Write(@"<p>Mail could not be sent notifying the contact about the creation of their namespace.</p>");
				Response.Write(@"<p>The error that occurred is: <pre>
" + EscapeHTML(fail) 
					+ "</pre>");
			}

			Response.Write(@"<p>Your FlexWiki namespace (" + EscapeHTML(values.Namespace) + @") has been created.
<p>You may visit the home page for your namespace at <a href='" + link + "'>" + EscapeHTML(link) + "</a>");

		}

		void DoEdit(FormValues values)
		{
			Response.Write("<p>Updating namespace: " + EscapeHTML(values.Namespace) + "</p>");
			// It exists -- we should be able to just edit it in place
			ContentBase cb = TheFederation.ContentBaseForNamespace(values.Namespace);
			AbsoluteTopicName defTopic = cb.DefinitionTopicName;

			TheFederation.SetTopicProperty(defTopic, "Contact", values.Contact, false);
			TheFederation.SetTopicProperty(defTopic, "Title", values.Title, false);
			TheFederation.SetTopicProperty(defTopic, "Description", values.Description, false);
			// if dir changed
			if (values.Directory!= ConfiguredDirectoryForNamespace(values.Namespace))
			{
				SetConfiguredDirectoryForNamespace(values.Namespace, values.Directory);
				Response.Write("<p><b>Warning:</b> you have change the location where content for this namespace is stored.  You must now <b>manually</b> move content from the old location to the new location.</p>");
			}
			Response.Write("<p>Namespace updated.</p>");
			Response.Write("<p>Back to <a href='namespaces.aspx'>namespace list</a>.</p>");
		}

		FormValues ReadValuesFromPost()
		{
			FormValues answer = new FormValues();
			answer.Namespace = Request.Form["ns"];
			answer.OriginalNamespace = Request.Form["originalNamespace"];
			answer.Title = Request.Form["title"];
			answer.Description = Request.Form["description"];
			answer.Contact = Request.Form["contact"];
			answer.Directory = Request.Form["directory"];
			return answer;
		}

		void ProcessFirstVisit()
		{
			string ns = Namespace;
			bool isCreate = ns == null;
			FormValues values;
			if (isCreate)
			{
				if (ns != null)
					Response.Write("<p><b>The namespace <i>" + EscapeHTML(ns) + "</i> does not exist.</b></p>");
				values = NewDefaultValues();
			}
			else
			{
				values = new FormValues();

				// Fill in any supplied values from the query string if we're creating one fresh
				if (TheFederation.ContentBaseForNamespace(ns) == null)
				{
					values.Namespace = Namespace;
					values.Title = Request.QueryString["title"];
					values.Description = Request.QueryString["description"];
					values.Contact = Request.QueryString["contact"];
				}
				else
					values = ReadValuesForNamespace(ns);
			}
			WriteForm(values);
		}

		FormValues NewDefaultValues()
		{
			FormValues answer = new FormValues();
			answer.Namespace = Namespace;

			return answer;
		}
		
		FormValues ReadValuesForNamespace(string ns)
		{
			FormValues answer = new FormValues();
			answer.Namespace = ns;
			ContentBase cb = TheFederation.ContentBaseForNamespace(ns);
			if (cb == null)
				return answer;
			answer.Contact = cb.Contact;
			answer.Title = cb.Title;
			answer.Description = cb.Description;
			answer.Directory = ConfiguredDirectoryForNamespace(ns);
			return answer;
		}

		string ConfiguredDirectoryForNamespace(string ns)
		{
			foreach (NamespaceToRoot each in Config.NamespaceMappings)
			{
				if (each.Namespace == ns)
				{
					return each.Root;
				}
			}
			return null;
		}

		void SetConfiguredDirectoryForNamespace(string ns, string dir)
		{
			NamespaceToRoot it = null;
			foreach (NamespaceToRoot each in Config.NamespaceMappings)
			{
				if (each.Namespace == ns)
				{
					it = each;
					break;
				}
			}
			if (it == null)
			{
				it = new NamespaceToRoot(ns, dir);
				Config.NamespaceMappings.Add(it);
			}
			else
			{
				it.Root = dir;
			}
			Config.WriteToFile(TheFederation.FederationNamespaceMapFilename);
		}

		class FormValues
		{
			public string Namespace;
			public string Contact;
			public string Title;
			public string Description;
			public string Directory;
			public string OriginalNamespace;
		}

		void WriteForm(FormValues values)
		{
			// Write the form
			Response.Write("<form method='post' ACTION='EditNamespace.aspx'>");
			StartFields();
			WriteHiddenField("originalNamespace", Namespace);
			WriteInputField("ns", "Namespace", "The full identifier for the namespace (e.g., FlexWiki.Dev.Testing)", values.Namespace);
			WriteInputField("title", "Title", "A short title for this namespace", values.Title);
			WriteTextAreaField("description", "Description", "A description for the namespace (use Wiki formatting)", values.Description);
			WriteInputField("contact", "Contact", "Specify a contact for this namespace (usually an email address, but not necessarily)", values.Contact);

			string def = "";
			if (IsCreate)
			{
				if (DefaultPath != null)
					def = " Leave blank to accept the default (" + EscapeHTML(DefaultPath) + @"\[NamespaceName]).";
			}
			else
				def =  "If you change this, you will need to <b>manually</b> move content from the old location to the new location.";
			WriteInputField("directory", "Directory", "The directory in which the namespace's content will be stored." + def, values.Directory);
			EndFields();
			string saveTitle = IsCreate ? "Create" : "Update";
			Response.Write("<input  type='submit'  name='OK' value ='" + saveTitle + "'>");
			Response.Write("</form>");
		}


		string DefaultPath
		{
			get
			{
				string answer = TheFederation.DefaultDirectoryForNewNamespaces;
				return answer;
			}
		}

			
		FederationConfiguration _Config;
		FederationConfiguration Config
		{
			get
			{
				if (_Config != null)
					return _Config;
				_Config = FederationConfiguration.FromFile(TheFederation.FederationNamespaceMapFilename);
				return _Config;
			}
		}
				

		void WriteHiddenField(string fieldName, string value)
		{	
			Response.Write("<span style='display: none'><input type='text' value='" + EscapeHTML(value) + "' name='" + fieldName + "'></span>");
		}
		
		void WriteInputField(string fieldName, string fieldLabel, string help, string value)
		{	
			WriteFieldHTML(fieldLabel, help, 
				"<input type='text' size='50' value='" + EscapeHTML(value) + "' name='" + fieldName + "'>");
		}

		void WriteCheckbox(string fieldName, string fieldLabel, string help, bool value)
		{	
			WriteFieldHTML(fieldLabel, help, 
				"<input type='checkbox' value='yes' " + (value ? "checked" : "") + " name='" + fieldName + "'>");
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

		string Namespace
		{
			get
			{
				if (IsPost)
					return Request.Form["ns"];
				else
					return Request.QueryString["ns"];
			}
		}


	}
}
