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
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Web;
using System.Net.Mail;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using FlexWiki;
using FlexWiki.Web;


namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Admin.
	/// </summary>
	public class EditProvider : AdminPage
	{
	
		private void Page_Load(object sender, System.EventArgs e)
		{
			DefaultPageLoad();
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
			UIResponse.ShowPage("Edit Provider", new UIResponse.MenuWriter(ShowAdminMenu), new UIResponse.BodyWriter(Body));
		}

		private void Body()
		{
			string providerId = ProviderIDParm;
			if (ProviderIDParm == null)
			{
				// CREATE
				Type type = ProviderType;
				if (type == null)
					ShowTypeSelectionForm();
				else
					ShowTypeDetailsForm(null);
			}
			else
			{
				// EDIT
				ShowTypeDetailsForm(providerId);
			}
		}

		private bool IsSave
		{
			get
			{
				return SaveNowParm != null && SaveNowParm != "";
			}
		}

		private void ShowTypeDetailsForm(string providerID)
		{
			bool isCreate = providerID == null;
			INamespaceProvider provider = null;

			// If we're creating, fill default values, else fill values from the specified instance (or form propertyName if the user already submitted)
			NamespaceProviderDefinition def = null;
			if (providerID == null)
			{
				// We're creating one
				provider = (INamespaceProvider)(Activator.CreateInstance(ProviderType));
			}
			else
			{
				foreach (NamespaceProviderDefinition each in Federation.CurrentConfiguration.NamespaceMappings)
				{
					if (each.Id == providerID)
					{
						def = each;
						break;
					}
				}
				if (def != null)
					provider = (INamespaceProvider)(Activator.CreateInstance(def.ProviderType));
				else
				{
					UIResponse.WritePara("Error: unknown namespace provider - " + UIResponse.Escape(providerID));
					return;
				}
			}


			// Decide if we're trying to save and have everything set
			IList errors = null;
			if (IsSave)
			{
				bool err = false;
				foreach (NamespaceProviderParameterDescriptor each in provider.ParameterDescriptors)
				{
					if (!isCreate && !each.IsPersistent)
						continue;	// skip create only parms for non-create scenario
					if (provider.ValidateParameter(Federation, each.ID, GetParm(each.ID), isCreate) != null)
					{
						err = true;
						break;
					}
				}

				if (!err)
				{
					errors = provider.ValidateAggregate(Federation, isCreate);
					if (errors == null)
					{
						// Great!  All's OK.  Create/modify the provider
						if (def == null)
						{
							def = new NamespaceProviderDefinition(AssemblyNamePart, TypeNamePart, Guid.NewGuid().ToString());
						}
						foreach (NamespaceProviderParameterDescriptor each in provider.ParameterDescriptors)
						{
							provider.SetParameter(each.ID, GetParm(each.ID));
							provider.SavePersistentParametersToDefinition(def);
						}
						IList namespaces = null;
						if (isCreate)
						{
							Federation.CurrentConfiguration.NamespaceMappings.Add(def);
							namespaces = provider.CreateNamespaces(Federation);
						}
						else
						{
							// If we're editing, we will have modified the definition in place, but we'll need to rescind and recreate
							// the namespaces
							provider.UpdateNamespaces(Federation);
						}

						Federation.Application.WriteFederationConfiguration();
						if (isCreate)
						{
							UIResponse.WritePara("The provider has been created.");
							if (provider.OwnerMailingAddress != null)
								NotifyOwnerOfCreation(provider.OwnerMailingAddress, namespaces);
							UIResponse.WritePara("Namespaces created:");
							UIResponse.WriteStartUnorderedList();
							foreach (string ns in namespaces)
							{
								UIResponse.WriteListItem(HtmlWriter.Escape(ns));
							}
							UIResponse.WriteEndUnorderedList();
						}
						else
						{
							UIResponse.WritePara("The provider has been updated.");
						}
						UIResponse.WritePara(UIResponse.Link("Providers.aspx", "View provider list"));
						return;
					}
				}
			}

			UIResponse.WriteStartForm("EditProvider.aspx");
			UIResponse.WriteStartFields();
  
			if (errors != null)
			{
				foreach (string s in errors)
				{
					UIResponse.WriteFieldError(UIResponse.Escape(s));
				}
			}

			foreach (NamespaceProviderParameterDescriptor each in provider.ParameterDescriptors)
			{
				if (!isCreate && !each.IsPersistent)
					continue;	// skip create only parms for non-create scenario
				string val = null;
				if (IsSave)
					val = GetParm(each.ID);
				else
				{
					if (isCreate)
					{
						string fromQuery = GetParm(each.ID);
						val = fromQuery == null ? each.DefaultValue : fromQuery;
					}
					else
						val = (string)(def.ParametersAsHash[each.ID]);
				}
				bool readOnly = !isCreate && !provider.CanParameterBeEdited(each.ID);
				UIResponse.WriteInputField(each.ID, each.Title, each.Description, val, readOnly);
				string error = provider.ValidateParameter(Federation, each.ID, val, isCreate);
				if (error != null)
					UIResponse.WriteFieldError(UIResponse.Escape(error));
			}
			UIResponse.WriteHiddenField(ParmNameSaveNow, "yup");
			UIResponse.WriteHiddenField(ParmNameTypeName, TypeNameParm);
			if (!isCreate)
				UIResponse.WriteHiddenField(ParmNameProviderID, providerID);

			UIResponse.WriteEndFields();

			UIResponse.WriteStartButtons();
			UIResponse.WriteSubmitButton("next2", (isCreate ? "Create >>" : "Save >>"));
			UIResponse.WriteEndButtons();

			UIResponse.WriteEndForm();				

		}

		private void NotifyOwnerOfCreation(string ownerMailingAddress, IList namespaces)
		{
			MailMessage msg = new MailMessage();
			string adminMail = (string)(ConfigurationManager.AppSettings["SendNamespaceCreationMailFrom"]);
			if (adminMail == null || adminMail == "")
			{
				Response.Write(@"<p>FlexWiki is not configured to automatically send mail notifying the contact of the namespace creation.");
				return;
			}
			msg.To.Add(ownerMailingAddress);

			string cc = (string)(ConfigurationManager.AppSettings["SendNamespaceCreationMailToCC"]);
            if (cc != null && cc != "")
            {
                msg.CC.Add(cc);
            }
			msg.IsBodyHtml = true;
			msg.From = new MailAddress(adminMail);
			msg.Subject = "Wiki namespace creation request completed";
			string body = @"<p>Your request has been completed.</p>";
			foreach (string ns in namespaces)
			{
				string url = TheLinkMaker.LinkToTopic(Federation.NamespaceManagerForNamespace(ns).HomePageTopicName);
				Uri u = System.Web.HttpContext.Current.Request.Url;
				url = new UriBuilder(u.Scheme, u.Host, u.Port, url).ToString();
				body += @"<p>The namespace " + ns + " has been created.  You can visit the home page at <a href='" + url + "'>" + HtmlWriter.Escape(url) + "</a>.</p>";
			}
			bool signed = false; 
			try
			{
				signed = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["SignNamespaceCreationMail"]); 
			}
			catch
			{
			}
			if (signed)
				body += @"<p>--- " + HtmlWriter.Escape(VisitorIdentityString) + "</p>";
			msg.Body = body;
			string fail = SendMail(msg);
			if (fail == null)
				UIResponse.Write(@"<p>Mail has been sent notifying the namespace owner of creation of the namespace(s).");
			else
			{
				Response.Write(@"<p>Mail could not be sent to the namespace oner notifying them of creation..");
				Response.Write(@"<p>The error that occurred is: <pre>
" + EscapeHTML(fail) 
					+ "</pre>");
			}
		}


		private static string ParmNameSaveNow = "SaveNow";
		private string SaveNowParm
		{
			get
			{
				return GetParm(ParmNameSaveNow);
			}
		}

		public static string ParmNameProviderID = "Provider";
		private string ProviderIDParm
		{
			get
			{
				return GetParm(ParmNameProviderID);
			}
		}

		private static string ParmNameTypeName = "TypeName";
		private string TypeNameParm
		{
			get
			{
				return GetParm(ParmNameTypeName);
			}
		}

		private string GetParm(string name)
		{
			if (IsPost)
				return Request.Form[name];
			else
				return Request.QueryString[name];
		}


		private Type ProviderType
		{
			get
			{
				Type answer = null;
				NamespaceProviderDefinition def = new NamespaceProviderDefinition(AssemblyNamePart, TypeNamePart, Guid.NewGuid().ToString());
				try
				{
					answer = def.ProviderType;
				}
				catch (Exception)
				{
				}
				return answer;
			}
		}

		private string AssemblyNamePart
		{
			get
			{
				string s = TypeNameParm;
				if (s == null)
					return null;
				int hash = s.IndexOf("#");
				if (hash < 0)
					return null;
				return s.Substring(0, hash);
			}
		}

		private string TypeNamePart
		{
			get
			{
				string s = TypeNameParm;
				if (s == null)
					return null;
				int hash = s.IndexOf("#");
				if (hash < 0)
					return null;
				return s.Substring(hash + 1);
			}
		}

		private void ShowTypeSelectionForm()
		{
			UIResponse.WriteStartForm("EditProvider.aspx");
			UIResponse.WritePara(@"To add a new provider, you must first identify the type of provider you want.  Once you have selected a valid one, you may be prompted for additional information, depending on the type of the provider.");

			UIResponse.WriteStartFields();
			// Pick the right default
			string defaultProvider;
			if (TypeNameParm != null && TypeNameParm != "")
				defaultProvider = TypeNameParm;
			else
				defaultProvider = (string)(ConfigurationManager.AppSettings["DefaultNamespaceProviderForNamespaceCreation"]);

			string def = null;
			// Get the list of types in the loaded assemblies that implement INamespaceProvider
			ChoiceSet choices = new ChoiceSet();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (Type each in assembly.GetTypes())
				{
					if (each.IsClass)
					{
						foreach (Type maybe in each.GetInterfaces())
						{
							if (maybe == typeof(INamespaceProvider))
							{
								INamespaceProvider worker = (INamespaceProvider)(Activator.CreateInstance(each));
								string displayString = each.FullName + " (" + worker.Description + ")";
								string val = assembly.GetName() + "#" + each.FullName;
								if (each.FullName == defaultProvider)
									def = val;

								choices.Add(displayString, val);
								break;
							}
						}
					}
				}
			}



			UIResponse.WriteCombobox(ParmNameTypeName, "Type", "The type of provider to to use", choices, def);

			// If there were any proposed values for the parms provided in the query string, pass them along
			foreach (string pName in Request.QueryString)
				UIResponse.WriteHiddenField(pName, Request.QueryString[pName]);
			
			UIResponse.WriteEndFields();

			UIResponse.WriteStartButtons();
			UIResponse.WriteSubmitButton("next1", "Next >>");
			UIResponse.WriteEndButtons();

			UIResponse.WriteEndForm();
		}


	}
}
