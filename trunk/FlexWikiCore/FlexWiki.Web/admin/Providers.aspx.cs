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
using System.Configuration;
using FlexWiki.Web;


namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Admin.
	/// </summary>
	public class Providers : AdminPage
	{
	
		private void Page_Load(object sender, System.EventArgs e)
		{
		}

		override protected void DefaultPageLoad()  
		{
			MinimalPageLoad();
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
			UIResponse.ShowPage("Namespace Providers", new UIResponse.MenuWriter(ShowMenu), new UIResponse.BodyWriter(ShowProviders));
		}

		void ShowMenu()
		{
			UIResponse.WriteStartMenu("Providers");
			UIResponse.WriteMenuItem("EditProvider.aspx", "Add provider", "Add a new provider instance");
			UIResponse.WriteEndMenu();
			UIResponse.WritePara("&nbsp;");

			ShowAdminMenu();
		}

		// Answer an array of arrays.  Each inner array has a collection of NamespaceProviderDefinitions with the same type
		ArrayList ProvidersByType
		{
			get
			{
				Hashtable collector = new Hashtable();	
				FederationConfiguration fc = FederationConfigurationFromFile;
				if (fc != null)
				{
					foreach (NamespaceProviderDefinition each in fc.NamespaceMappings)
					{
						string key = each.Type;
						ArrayList list = (ArrayList)(collector[key]);
						if (list == null)
						{
							list = new ArrayList();
							collector[key] = list;
						}
						list.Add(each);
					}
				}
				ArrayList answer = new ArrayList();
				ArrayList keys = new ArrayList();
				keys.AddRange(collector.Keys);
				keys.Sort();
				foreach (string k in keys)
					answer.Add(collector[k]);

				return answer;
			}
		}

		void ShowProviders()
		{

			foreach (ArrayList each in ProvidersByType)
			{
				// Lay down the header
				NamespaceProviderDefinition first = (NamespaceProviderDefinition)(each[0]);
				string s = first.Type;
				if (first.AssemblyName != null)
					s += " (in " + first.AssemblyName + ")";
				UIResponse.WriteHeading(UIResponse.Escape(s), 1);

				UITable table = new UITable();
				table.AddColumn(new UIColumn()); // edit link
				foreach (NamespaceProviderParameter parm in first.Parameters)
					table.AddColumn(new UIColumn(parm.Name));

				UIResponse.WriteStartTable(table);
				foreach (NamespaceProviderDefinition inner in each)
				{
					UIResponse.WriteStartRow();
					UIResponse.WriteCell(UIResponse.CommandLink("EditProvider.aspx?Provider=" + inner.Id, UIResponse.Command.Edit, "edit this provider's information"));
					foreach (NamespaceProviderParameter parm in inner.Parameters)
						UIResponse.WriteCell(UIResponse.Escape(parm.Value));
					UIResponse.WriteEndRow();
				}
				UIResponse.WriteEndTable();

			}
		}
	}
}
