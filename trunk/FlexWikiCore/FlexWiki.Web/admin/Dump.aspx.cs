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
using System.Text.RegularExpressions;
using FlexWiki.Web;

namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Dump.
	/// </summary>
	public class Dump : AdminPage
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

		protected void DoDump()
		{
			LinkMaker lm = TheLinkMaker;
			DumpFederation(TheFederation);
		}

		void DumpFederation(Federation aFederation)
		{
			WriteLine("<div style='margin: .25in'>");

			push("Federation");
			pushkv();
			kv("Created", aFederation.Created.ToString());
			kv("Default Namespace", aFederation.DefaultNamespace.ToString());
			kv("Configuration File", (aFederation.FederationNamespaceMapFilename == null ? "" :  aFederation.FederationNamespaceMapFilename));

			popkv();

			push("ContentBases");
			foreach (string root in aFederation.Roots)
			{
				ContentBase cb = aFederation.ContentBaseForRoot(root);
				string ns = aFederation.NamespaceForRoot(root);
				push(ns + " [root: " + root + "]");
				if (cb == null)
					WriteLine("[no content base definition available]<br>");
				else
				{
					pushkv();
					kv("Created", cb.Created.ToString());
					kv("LastRead", cb.LastRead.ToString());
					kv("HomePage", cb.HomePage);
					kv("Import", cb.ImportedNamespaces);
					kv("Contact", cb.Contact);
					kv("Exists", cb.Exists.ToString());
					kv("Title", cb.Title);
					popkv();
				}
				pop();
			}
			pop();


			pop();

			WriteLine("</div>");
		}

		int _Depth = 0;

		void push(string s)
		{
			WriteLine("<div style='margin-left: " + (0.2 * _Depth) + "in'>");
			WriteLine("<h" + (_Depth + 1) + ">" + esc(s) + "</h" + (_Depth + 1) + ">");
			_Depth++;
		}

		void pop()
		{
			_Depth--;
			WriteLine("</div>");
		}

		void pushkv()
		{
			WriteLine("<table margin='.2in' cellspacing='0' cellpadding='2' style='border: 1px solid blue'>");
		}

		void kv(string key, string val)
		{
			WriteLine("<tr>");
			WriteLine("<td style='background: #d0d0ff'>" + esc(key) + "</td>");
			WriteLine("<td>");
			if (val != null)
				Write(esc(val.ToString()));
			WriteLine("</td>");
			WriteLine("</tr>");
		}

		void kv(string key, ICollection val)
		{
			WriteLine("<tr>");
			WriteLine("<td style='background: #d0d0ff'>" + esc(key) + "</td>");
			WriteLine("<td>");
			if (val != null)
			{
				bool first = true;
				foreach (object obj in val)
				{
					if (!first)
						Write(", ");
					first = false;
					Write(esc(obj.ToString()));
				}
			}
			WriteLine("</td>");
			WriteLine("</tr>");
		}

		void popkv()
		{
			WriteLine("</table>");
		}

		static string esc(string input)
		{
			// replace HTML special characters with character entities
			// this has the side-effect of stripping all markup from text
			string str = input;
			str = Regex.Replace (str, "&", "&amp;") ;
			str = Regex.Replace (str, "<", "&lt;") ;
			str = Regex.Replace (str, ">", "&gt;") ;
			return str;
		}

		protected void Write(string s)
		{
			Response.Write(s);
		}

		protected void WriteLine(string s)
		{
			Response.Write(s + "\n");
		}

		protected void WriteHeading(string s, int level)
		{
			Response.Write("<h" + level + ">" + s + "</h" + level + ">");
		}
	}
}
