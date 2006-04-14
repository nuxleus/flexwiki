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
using System.IO;
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
	/// Summary description for UpgradeConfigFile.
	/// </summary>
	public class UpgradeConfigFile : AdminPage
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		protected override void PageLoad()
		{

		}

		protected void ShowUpdateInfo()
		{
			if (IsPost)
				ProcessUpgradeRequest();
			else
				OfferUpgrade();
		}

		private void OfferUpgrade()
		{
			UIResponse.WritePara("Your configuration file uses old style namespace definitions (see below for more details).");
			UIResponse.WritePara("FlexWiki can automatically convert your old-style namespace definitions to the new format.  This won't change the configuration of your namespaces, but will just upgrade the configuration file to the new format.");
			UIResponse.WriteStartForm("UpgradeConfigFile.aspx");
			UIResponse.WriteSubmitButton("Upgrade", "Upgrade my configuration");
			UIResponse.WriteEndForm();
		}

		private void ProcessUpgradeRequest()
		{
            throw new NotImplementedException(); 
            //UIResponse.WritePara("Upgrading...");

            //// Get the existing one
            //FederationConfiguration config = Federation.Application.FederationConfiguration;
            //string backupFilename = config.FederationNamespaceMapFilename + ".bak";

            //UIResponse.WritePara("Writing backup configuration file: " + HtmlWriter.Escape(backupFilename));
            //File.Copy(config.FederationNamespaceMapFilename, backupFilename, true);

            //// Convert each of the old ones to a new one
            //foreach (DeprecatedNamespaceDefinition each in config.DeprecatedNamespaceDefinitions)
            //{
            //    UIResponse.WritePara("Converting namespace " + HtmlWriter.Escape(each.Namespace) + "...");

            //    NamespaceProviderDefinition def = new NamespaceProviderDefinition(typeof(FileSystemNamespaceProvider).Assembly.FullName, typeof(FileSystemNamespaceProvider).FullName, Guid.NewGuid().ToString());
            //    def.SetParameter("Root", each.Root);
            //    def.SetParameter("Namespace", each.Namespace);
            //    config.NamespaceMappings.Add(def);
            //}
            //config.DeprecatedNamespaceDefinitions.Clear();

            //foreach(NamespaceProviderDefinition providerDefinition in config.NamespaceMappings)
            //{
            //    // Upgrade the id if it is null or empty.
            //    if(providerDefinition.Id == null || (providerDefinition.Id != null && providerDefinition.Id.Length == 0))
            //    {
            //        providerDefinition.Id = Guid.NewGuid().ToString();					
            //    }
            //}

            //UIResponse.WritePara("Writing updated configuration file: " + HtmlWriter.Escape(config.FederationNamespaceMapFilename));
            //config.WriteToFile(config.FederationNamespaceMapFilename);

            //UIResponse.WritePara("Upgrade complete.");

            //UIResponse.WritePara("Visit <a href='default.aspx'>Administration Home</a>.");

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

		}
		#endregion
	}
}
