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
using System.IO;
using System.Text.RegularExpressions;
using System.Configuration;
using FlexWiki.Web;

namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for Config.
	/// </summary>
	public class Config : System.Web.UI.Page
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

		void ErrorMissingConfigurationFileSetting()
		{
			ErrorStart("Missing configuration file setting");
			Response.Write(@"<p>You are missing the setting for <b>FederationNamespaceMapFile</b> in your <b>web.config</b> file.
<p>Here is an example of a valid web.config file:");
			WriteExampleWebConfig();
			Response.Write(@"<p>Note that some of the authentication settings may vary for your site.  This example is configured for Windows network authentication.  <p></b>FederationNamespaceMapFile</b> must be set to the logical web path of a valid FlexWiki federation configuration file.  Here is an example of a valid federation configuration file:");
			WriteExampleConfig();
			ErrorEnd();
		}

		void WriteExampleConfig()
		{
			Response.Write("<blockquote><pre>");
			Response.Write(esc(@"<?xml version=""1.0"" encoding=""utf-8""?>
<FederationConfiguration>
  <DefaultNamespace>FlexWiki</DefaultNamespace>
  <Namespaces>
		<Namespace Root="".\wikibases\FlexWiki"" Namespace=""FlexWiki"" />
		<Namespace Root="".\wikibases\Some.Other.Namespace"" Namespace=""Some.Other.Namespace"" />
  </Namespaces>
  <About>This site is the home of FlexWiki, an experimental collaboration tool.</About>
</FederationConfiguration>"));
			Response.Write("</pre></blockquote>");
		}

		void WriteExampleWebConfig()
		{
			Response.Write("<blockquote><pre>");
			Response.Write(esc(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
	<appSettings>

		<!-- PUT THE LOGICAL PATH TO THE FEDERATION CONFIGURATION FILE HERE -->
		<add key=""FederationNamespaceMapFile"" value=""/NamespaceMap.xml"" />

		<!-- SET THE FOLLOWING KEY TO AN SMTP SERVER THAT WILL DELIVER MAIL.
                NEEDED IF YOU WANT YOUR WIKI SITE TO BE ABLE TO DELIVER NEWSLETTERS -->
		<add key=""SMTPServer"" value=""mail.my-server.com"" />

		<!-- SET THE FOLLOWING KEY TO THE FULL FULLY-QUALIFIED ADDRESS OF THE USER NAME 
                TO USE TO AUTHENTICATE AGAINST THE SMTP SERVER -->
		<add key=""SMTPUser"" value=""user@domain.com"" />

		<!-- SET THE FOLLOWING KEY IF THE SMTP SERVER NEEDS LOGIN AUTHENTICATION -->
		<add key=""SMTPPassword"" value=""password goes here"" />

		<!-- SET THE FOLLOWING KEY TO THE DESIRED FROM ADDRESS FOR NEWSLETTERS -->
		<add key=""NewslettersFrom"" value=""newsletters@mysite.com"" />

	</appSettings>
	<system.web> 
		<authentication mode=""Windows"" />
		<authorization> 
			<deny users=""?""/> 
		</authorization> 
		<pages validateRequest = ""false"" />
	</system.web>
</configuration>"));
			Response.Write("</pre></blockquote>");
		}

		void ErrorMissingConfigurationFile(string fn)
		{
			ErrorStart("Missing configuration file");
			Response.Write(@"<p>You specified a federation configuration file, but it can't be found.  Given your current settings,
this file must be present at <b>" + esc(fn) + @"</b> and be a valid federation configuration file.
<p>Here is an example of a valid federation configuration file:");
			WriteExampleConfig();
			ErrorEnd();
		}

		void ErrorReadingConfigurationFile(string fn, Exception ex)
		{
			ErrorStart("Error reading configuration file");
			Response.Write(@"<p>You specified a federation configuration file, but an error occurred while it was being read.  
The federation configuration file is stored here (<b>" + esc(fn) + @"</b>) and the following error occurred while reading the file:");
			Response.Write("<p><blockquote>" + esc(ex.ToString()) + "</blockquote>");
			Response.Write(@"<p>Here is an example of a valid federation configuration file:");
			WriteExampleConfig();
			ErrorEnd();
		}

		void WarningMissingContentBaseDefinition(NamespaceToRoot map, string defTopicPath)
		{
			WarningStart("Content base definition topic missing for namespace");
			Response.Write(@"<p>The namespace <b>" + map.Namespace + @"</b> is listed in the federation configuration, but it's definition topic is missing.
The definition topic is the wiki topic that contains configuration information about the namespace and its ContentBase.
The system looked for this topic file (<b>" + esc(defTopicPath) + @"</b>), but couldn't find it.
");
			Response.Write(@"<p>Here is an example of a valid content base definition topic you could put in this file.");
			WriteExampleContentBaseDefinition(map.Namespace);
			Response.Write(@"<p>Note that there are other properties that you will likely want to specify in this topic, but the above should get the topic up and running.");
			WarningEnd();
		}

		void WarningMissingSMTPServer()
		{
			WarningStart("SMTP Server not listed");
			Response.Write(@"<p>No SMTP server is configured, so no newsletters will be delivered.  This will not impact the functioning of your wiki except that newsletters will not be delivered.");
			Response.Write(@"<p>If you wish to set this server, set the SMTPServer and SMTPUser keys in your web.config file.");
			Response.Write(@"<p>Here is an example of a valid web.config file with these entries:");
			WriteExampleWebConfig();
			WarningEnd();
		}

		void WarningMissingNewslettersFrom()
		{
			WarningStart("NewslettersFrom address is not listed");
			Response.Write(@"<p>No newsletter <i>From:</i> address is configured, so no newsletters will be delivered.  This will not impact the functioning of your wiki except that newsletters will not be delivered.");
			Response.Write(@"<p>If you wish to set this address, set the NewslettersFrom key in your web.config file.  And remember that, depending on how the SMTP server you're using is configured, you might need to put what will be considered a non-fradulent From address here!");
			Response.Write(@"<p>Here is an example of a valid web.config file with this entry:");
			WriteExampleWebConfig();
			WarningEnd();
		}
		
		void WriteExampleContentBaseDefinition(string ns)
		{
			Response.Write("<blockquote><pre>");
			Response.Write(@"Namespace: " + esc(ns) + @"<br />
Description: place a description of the namespace here
Title: " + ns + @"
Contact: a person to contact
");
			Response.Write("</pre></blockquote>");
		}

		void ErrorWritingToNamepsaceFolderFailed(Exception ex)
		{
			if (ex.GetType() == typeof(UnauthorizedAccessException))
			{
				ErrorStart("Cannot write to the namespace folder. Access was denied. Your permissions are incorrect.");
				Response.Write(ex.Message);
				ErrorEnd();
			}
			else
			{
				ErrorStart("Cannot write to the namespace folder.");
				Response.Write(ex.Message);
				ErrorEnd();
			}
			
		}
		
		void ErrorMissingDefaultNamespace(string configFile)
		{
			ErrorStart("Default namespace not specified");
			Response.Write(@"<p>You have not specified the default namespace for your federation in the federation configuration file (<b>" + esc(configFile) + @"</b>).
This setting must be present and must name a namespace listed in your configuration file. <p>Here is an example of a valid federation configuration file:");
			WriteExampleConfig();
			ErrorEnd();
		}
		
		void ErrorDefaultNamespaceSpecifiedButNotFound(string configFile, string proposedNS)
		{
			ErrorStart("Default namespace not found");
			Response.Write(@"<p>You have specified a default namespace <b>" + esc(proposedNS) + @"</b> in the federation configuration file (<b>" + esc(configFile) + @"</b>),
but the namespace is not listed in the &lt;namespaces&gt; section of the configuration file.
<p>Here is an example of a valid federation configuration file:");
			WriteExampleConfig();
			ErrorEnd();
		}

		void ErrorStart(string heading)
		{
			Error(heading);
			DetailsStart();
		}

		void ErrorEnd()
		{
			DetailsEnd();
		}

		void WarningStart(string heading)
		{
			Warning(heading);
			DetailsStart();
		}

		void WarningEnd()
		{
			DetailsEnd();
		}

		void DetailsStart()
		{
			Response.Write("<div style='margin-left: .25in; font-size: 8pt'>\n");
		}

		void DetailsEnd()
		{
			Response.Write("</div>");
		}

		protected string RootUrl(HttpRequest req)
		{
			string full = req.Url.ToString();
			if (req.Url.Query != null && req.Url.Query.Length > 0)
			{
				full = full.Substring(0, full.Length - req.Url.Query.Length);
			}
			if (req.PathInfo != null && req.PathInfo.Length > 0)
			{
				full = full.Substring(0, full.Length - (req.PathInfo.Length + 1));
			}
			full = full.Substring(0, full.LastIndexOf('/') + 1);
			return full;
		}

		protected void Configure()
		{

			int errors = 0;

			Response.Write("<h1>Federation Configuration Report</h1>");
			Response.Write("<blockquote>");

			// Verify everything in order

			///////////
			///Make sure there is a namespace file pointed to by the configuration settings
			string federationNamespaceMapLogicalPath = ConfigurationSettings.AppSettings["FederationNamespaceMapFile"];
			if (federationNamespaceMapLogicalPath == null)
			{
				ErrorMissingConfigurationFileSetting();
				return;
			}
			Proof("Federation configuration file identified in web.config: " + esc(federationNamespaceMapLogicalPath));

			///////////
			///Make sure the federation file exists
			string federationNamespaceMap = MapPath(federationNamespaceMapLogicalPath);
			if (!File.Exists(federationNamespaceMap))
			{
				ErrorMissingConfigurationFile(federationNamespaceMap);
				return;
			}
			Proof("Federation configuration file found at: " + esc(federationNamespaceMap));

			using (TextReader sr = new StreamReader(federationNamespaceMap))
			{
				DetailsStart();
				string s = sr.ReadToEnd();
				s = Regex.Replace (esc(s), "\n", "<br />\n") ;
				Response.Write(s);
				DetailsEnd();
			}

			//////////
			///Make sure we can read the configuration file in
			///
			FederationConfiguration config = null;
			try
			{
				config = FederationConfiguration.FromFile(federationNamespaceMap);
			}
			catch (Exception ex)
			{
				ErrorReadingConfigurationFile(federationNamespaceMap, ex);
				return;
			}
			Proof("Federation configuration successfully read");
			DetailsStart();
			Proof2(config.NamespaceMappings.Count + " mapping(s) found");

			FileInfo info = new FileInfo(federationNamespaceMap);
			foreach (NamespaceToRoot map in config.NamespaceMappings)
			{
				string abs = map.AbsoluteRoot(info.DirectoryName);
				Proof2("Namespace '" + (map.Namespace) + "' maps to '" + esc(abs) + "'");
				string defTopicPath = abs + "\\" + ContentBase.DefinitionTopicLocalName + ".wiki";
				if (File.Exists(defTopicPath))
					Proof("ContentBase definition present for namespace '" + esc(map.Namespace) + "'");
				else
				{
					WarningMissingContentBaseDefinition(map, defTopicPath);
					errors++;
				}
			}
			DetailsEnd();

			//			///////////
			//			///Make sure the log file is specified
			//			string logFile = ConfigurationSettings.AppSettings["LogPath"];
			//			if (!File.Exists(logFile))
			//			{
			//				ErrorMissingLogFilePath();
			//				return;
			//			}
			//			Proof("Log file location configured: " + esc(logFile));


			///////////
			///Make sure the default namespace is configured and present
			string defaultNamespace = config.DefaultNamespace;
			if (defaultNamespace == null)
			{
				ErrorMissingDefaultNamespace(federationNamespaceMap);
				return;
			}
			bool found = false;
			foreach (NamespaceToRoot m in config.NamespaceMappings)
			{
				if (m.Namespace == defaultNamespace)
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				ErrorDefaultNamespaceSpecifiedButNotFound(federationNamespaceMap, defaultNamespace);
				return;
			}
			Proof("Valid default namespace setting detected: " + esc(defaultNamespace));

			/// Make sure we can write data (correct ASP.NET permissions)
			bool isWritable = true;
			ArrayList exceptions = new ArrayList();
			ArrayList paths = new ArrayList();

			foreach (NamespaceToRoot m in config.NamespaceMappings)
			{
				string directoryToWriteTo = m.AbsoluteRoot(info.DirectoryName);
//				string directoryToWriteTo = Server.MapPath(m.Root);

				try
				{
					System.IO.StreamWriter sw = File.CreateText(Path.Combine(directoryToWriteTo, "testFile.txt"));
					sw.Close();
					paths.Add(String.Format("{0} Path\"{1}\" is writable {2}", "<p>", directoryToWriteTo, "</p>"));
				}
				catch(Exception ex)
				{
					errors++;
					isWritable = false;
					exceptions.Add(ex);
				}

				if (File.Exists(Path.Combine(directoryToWriteTo, "testFile.txt")))
				{
					File.Delete(Path.Combine(directoryToWriteTo, "testFile.txt"));
				}
			}
			DetailsEnd();

			if (isWritable)
			{
				Proof("All folders for namespaces are writable");
				DetailsStart();
				foreach (string s in paths)
				{
					Response.Write(s);
				}
				DetailsEnd();
			}
			else
			{
				ErrorStart("All folders for namespaces are not writable");
				ErrorEnd();
				DetailsStart();
				foreach(Exception ex in exceptions)
				{
					ErrorWritingToNamepsaceFolderFailed(ex);
				}
				DetailsEnd();
			}


			/// Check to see if the SMTP server is configured
			string SMTPServer = ConfigurationSettings.AppSettings["SMTPServer"]; 
			string SMTPUser = ConfigurationSettings.AppSettings["SMTPUser"];
			string SMTPPassword = ConfigurationSettings.AppSettings["SMTPPassword"];
			string NewslettersFrom = ConfigurationSettings.AppSettings["NewslettersFrom"];

			if (SMTPServer == null)
			{
				errors++;
				WarningMissingSMTPServer();
			}
			else
			{
				Proof(String.Format("SMTP server listed: {0}", SMTPServer));
			}

			if (NewslettersFrom == null)
			{
				errors++;
				WarningMissingNewslettersFrom();
			}
			else
			{
				Proof(String.Format("Newsletter From address listed: {0}", NewslettersFrom));
			}

			if (errors == 0)
				Proof("ALL SETTINGS CONFIGURED CORRECTLY");
			else
				Error("SOME SETTINGS NOT CONFIGURED CORRECTLY");

			Response.Write("</blockquote>");

		}

		void Proof(string s)
		{
			Response.Write("<p><font color='#00c000'>" + s + "</font></p>");
		}

		void Proof2(string s)
		{
			Response.Write("<p><font size='2'>" + s + "</font></p>");
		}

		new void Error(string s)
		{
			Response.Write("<p><font color='#c00000'>" + s + "</font></p>");
		}

		void Warning(string s)
		{
			Response.Write("<p><font color='orange'>" + s + "</font></p>");
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
	}
}
