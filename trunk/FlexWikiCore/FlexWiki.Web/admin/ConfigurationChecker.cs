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
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Configuration;
using FlexWiki.Web;
using FlexWiki;


namespace FlexWiki.Web.Admin
{
	/// <summary>
	/// Summary description for ConfigurationChecker.
	/// </summary>
	public class ConfigurationChecker
	{
		string FederationNamespaceMapLogicalPath;
		string FederationNamespaceMap;

		public ConfigurationChecker(string logicalPath, string physicalPath)
		{
			FederationNamespaceMapLogicalPath = logicalPath;
			FederationNamespaceMap = physicalPath;
		}

		public class Result
		{
			public enum Level
			{
				Error,
				Warning,
				OK,
				Info
			};

			public Level ResultLevel;
			public HTMLStringWriter Writer = new HTMLStringWriter();
			public string Title;

			public Result(string title, Level aLevel)
			{
				ResultLevel = aLevel;
				Title = title;
			}

		};
		
		ArrayList _Results = new ArrayList();

		public IList Results
		{
			get
			{
				return _Results;
			}
		}

		public void WriteTo(HTMLWriter writer)
		{
			UITable t = new UITable();
			UIColumn color = new UIColumn();
			t.AddColumn(color);
			UIColumn text = new UIColumn("Description");

			writer.WriteStartTable(t);
			
			foreach (Result each in Results)
			{
				string bg = null;

				switch (each.ResultLevel)
				{
					case ConfigurationChecker.Result.Level.OK:
						bg = "lightgreen";
						break;

					case ConfigurationChecker.Result.Level.Info:
						bg = "lightgreen";
						break;

					case ConfigurationChecker.Result.Level.Warning:
						bg = "yellow";
						break;

					case ConfigurationChecker.Result.Level.Error:
						bg = "red";
						break;
				}

				writer.WriteStartRow();
				writer.Write("<td valign='top' bgcolor="+ bg + ">&nbsp;&nbsp;</td>");
				writer.Write("<td valign='top' class='GenericCell' bgcolor='#e0e0e0'>" + writer.Bold(HTMLStringWriter.Escape(each.Title)) + "</td>");
				writer.WriteEndRow();
				
				writer.WriteStartRow();
				writer.Write("<td valign='top' bgcolor="+ bg + ">&nbsp;&nbsp;</td>");
				writer.WriteCell(text, each.Writer.ToString());
				writer.WriteEndRow();
			}

			writer.WriteEndTable();
		}

		void AddResult(Result aRes)
		{
			Results.Add(aRes);
		}
		
		public IList ResultsOfLevel(Result.Level aLevel)
		{
			ArrayList answer = new ArrayList();
			foreach (Result each in Results)
				if (each.ResultLevel == aLevel)
					answer.Add(each);
			return answer;
		}

		public ConfigurationChecker.Result.Level AggregateResultLevel
		{
			get
			{
				Result.Level answer = Result.Level.OK;
				foreach (Result each in Results)
				{
					if ((answer == Result.Level.OK || answer == Result.Level.Warning) && each.ResultLevel == Result.Level.Error)
						answer = each.ResultLevel;
					if ((answer == Result.Level.OK) && each.ResultLevel == Result.Level.Warning)
						answer = each.ResultLevel;
				}
				return answer;
			}
		}

		void Info(string title)
		{
			Info(title, null);
		}

		void Info(string title, string htmlBody)
		{
			Result r = new Result(title, Result.Level.Info);
			if (htmlBody != null)
				r.Writer.Write(htmlBody);
			AddResult(r);
		}

		void OK(string title)
		{
			OK(title, null);
		}

		void OK(string title, string htmlBody)
		{
			Result r = new Result(title, Result.Level.OK);
			if (htmlBody != null)
				r.Writer.Write(htmlBody);
			AddResult(r);
		}

		void Error(string title)
		{
			Error(title, null);	
		}
		
		void Error(string title, string htmlBody)
		{
			Result r = new Result(title, Result.Level.Error);
			if (htmlBody != null)
				r.Writer.Write(htmlBody);
			AddResult(r);
		}

		void Warning(string title)
		{
			Warning(title, null);
		}

		void Warning(string title, string htmlBody)
		{
			Result r = new Result(title, Result.Level.Warning);
			if (htmlBody != null)
				r.Writer.Write(htmlBody);
			AddResult(r);
		}
		
		public void Check()
		{
			_Results = new ArrayList();
			FederationConfiguration config = null;

			CheckPluginTypes();

			if (CheckConfigurationFileSetting())
				if (CheckConfigurationFileExists())
					config = CheckConfigurationFileCanBeRead();

			if (config != null)
			{
				CheckProviders(config);
				try
				{
					Federation aFed = new Federation(FederationNamespaceMap, Formatting.OutputFormat.Testing, new LinkMaker("http://dummy"));
					ValidateDefaultNamespace(aFed);
					ValidateWritableNamespaces(aFed);
				}
				catch (Exception e)
				{
					Result r = new Result("Error loading federation", Result.Level.Error);
					r.Writer.WritePara("Error loading federation: " + HTMLWriter.Escape(e.Message));
					AddResult(r);
				}
			}
		}

		void ValidateWritableNamespaces(Federation aFed)
		{
			/// Make sure we can write data (correct ASP.NET permissions)
			ArrayList exceptions = new ArrayList();
			ArrayList paths = new ArrayList();

			foreach (ContentBase cb in aFed.ContentBases)
			{
				if (!(cb is FileSystemStore))
					continue;

				FileSystemStore store = cb as FileSystemStore;
				string directoryToWriteTo = store.Root;

				try
				{
					System.IO.StreamWriter sw = File.CreateText(Path.Combine(directoryToWriteTo, "testFile.txt"));
					sw.Close();

					Result r = new Result("Namespace (" + HTMLWriter.Escape(cb.Namespace) + ") confirmed as writable.", Result.Level.OK);
					r.Writer.WritePara(String.Format("Path\"{0}\" is writable", directoryToWriteTo));
					AddResult(r);
				}
				catch(Exception ex)
				{
					Result r = new Result("Namespace (" + HTMLWriter.Escape(cb.Namespace) + ") not writable.", Result.Level.Error);
					r.Writer.WritePara(String.Format("Path\"{0}\" is not writable", directoryToWriteTo));
					r.Writer.WritePara("Error was: <br>" + HTMLWriter.Escape(ex.ToString(), true));
					AddResult(r);
				}

				if (File.Exists(Path.Combine(directoryToWriteTo, "testFile.txt")))
				{
					File.Delete(Path.Combine(directoryToWriteTo, "testFile.txt"));
				}
			}
		}

		void ValidateDefaultNamespace(Federation aFed)
		{
			///////////
			///Make sure the default namespace is configured
			string defaultNamespace = aFed.CurrentConfiguration.DefaultNamespace;
			if (defaultNamespace == null)
			{
				Result r = new Result("Default namespace not specified", Result.Level.Error);
				r.Writer.Write(@"<p>You have not specified the default namespace for your federation in the federation configuration file (<b>" + HTMLWriter.Escape(FederationNamespaceMap) + @"</b>).
This setting must be present and must name a namespace listed in your configuration file. <p>Here is an example of a valid federation configuration file:");
				r.Writer.Write(ExampleConfig());
				AddResult(r);
				return;
			}

			if (aFed.ContentBaseForNamespace(defaultNamespace) == null)
			{
				Result r = new Result("Default namespace not found", Result.Level.Error);
				r.Writer.Write(@"<p>You have specified the default namespace for your federation in the federation configuration file (<b>" + HTMLWriter.Escape(FederationNamespaceMap) + @"</b>),
but the namespace you specified can not be found (either because it's not listed in the configuration file or it's not valid).
<p>Here is an example of a valid federation configuration file:");
				r.Writer.Write(ExampleConfig());
				AddResult(r);
				return;
			}
			
			OK("Valid default namespace setting detected: " + HTMLWriter.Escape(defaultNamespace));
		}

		void CheckPluginTypes()
		{
			FlexWikiConfigurationSectionHandler config = FlexWikiConfigurationSectionHandler.GetConfig();
			if (config==null)
				return;

			foreach(string plugin in config.Plugins)
			{
				Assembly assembly = null;
				string assemblyError = null;
				try
				{
					assembly = Assembly.Load(plugin);
				}
				catch (FileNotFoundException)
				{
					assemblyError = "Assembly not found.";
				}
				catch (BadImageFormatException)
				{
					assemblyError = "Assembly is not a valid managed assembly.";
				}
				if (assemblyError == null)
				{
					Result r = new Result("Plugin found and loaded successfully", Result.Level.OK);
					r.Writer.Write(@"<p>The plugin <b>" + HTMLWriter.Escape(plugin) + "</b> has been successfully loaded.</p>");
					AddResult(r);
				}
				else
				{
					Result r = new Result("Plugin not found", Result.Level.Error);
					r.Writer.Write(@"<p>The plugin <b>" + HTMLWriter.Escape(plugin) + "</b> could not be loaded.</p>");
					r.Writer.Write(@"<p>" + HTMLWriter.Escape(assemblyError) + "</p>");
					AddResult(r);
				}
			}

		}

		
		void CheckProviders(FederationConfiguration config )
		{
			ArrayList uniqueProviders = new ArrayList();

			// Now make sure each provider is well-defined
			foreach (NamespaceProviderDefinition provider in config.NamespaceMappings)
			{
				if (provider.Id == null || (provider.Id != null && provider.Id.Length == 0))
				{
					Result r = new Result("Missing Id attribute in <Namespace> element", Result.Level.Error);
					r.Writer.Write(@"<p>You did not specify the a unique Id for the NamespaceProvider in the in the &lt;Namespaces&gt; section of the configuration file.
	<p>Here is an example of a valid federation configuration file:");			
					r.Writer.Write(ExampleConfig());
					AddResult(r);
				}
				if( provider.Id != null )
				{
					if( uniqueProviders.Contains(provider.Id) )
					{
						Result r = new Result("Id attribute value in <Namespace> element must be unique", Result.Level.Error);
						r.Writer.Write(@"<p>You specified a non-unique Id:&quot;" + provider.Id + @"&quot; for the NamespaceProvider in the in the &lt;Namespaces&gt; section of the configuration file.
	<p>Here is an example of a valid federation configuration file:");			
						r.Writer.Write(ExampleConfig());
						AddResult(r);		
					}
					else
					{
						uniqueProviders.Add(provider.Id);
					}
				}
				if (provider.Type == null)
				{
					Result r = new Result("Missing Type attribute in &lt;Namespace&gt; element", Result.Level.Error);
					r.Writer.Write(@"<p>You did not specify the Type of a NamespaceProvider in the in the &lt;Namespaces&gt; section of the configuration file.
	<p>Here is an example of a valid federation configuration file:");			
					r.Writer.Write(ExampleConfig());
					AddResult(r);
				}
				// If an assembly is specified, make sure it can be found
				if (provider.AssemblyName != null)
				{
					try
					{
						System.Reflection.Assembly.Load(provider.AssemblyName);
						OK("Successfully loaded assembly " + HTMLWriter.Escape(provider.AssemblyName));
					}
					catch (Exception e)
					{
						Result r = new Result("Error loading namespace provider assembly", Result.Level.Error);
						r.Writer.Write(@"<p>The assembly <b>" +HTMLWriter.Escape(provider.AssemblyName) + @"</b> you specified could not be loaded.  The following error occurred: <p>" + HTMLWriter.Escape(e.ToString(), true));			
						AddResult(r);
					}
				}
				// Make sure the type is a valid type and can be created
				if (provider.Type != null)
				{
					try
					{
						Type x = provider.ProviderType;	// see if it'll resolve, but catch exceptions
						if (x == null)
							throw new Exception("Unable to find type.");
						OK("Successfully loaded type " + HTMLWriter.Escape(provider.Type));
					}
					catch (Exception e)
					{
						Result r = new Result("Error loading namespace provider type", Result.Level.Error);
						r.Writer.Write(@"<p>The type <b>" +HTMLWriter.Escape(provider.Type) + @"</b> you specified could not be loaded.");			
						if (provider.AssemblyName == null)
							r.Writer.Write(@"The assembly used was the base FlexWiki engine.");
						else
							r.Writer.Write(@"The assembly used was: " + HTMLWriter.Escape(provider.AssemblyName));
						r.Writer.Write("<p>The following error occurred: <p>" + HTMLWriter.Escape(e.Message, true));
						AddResult(r);
					}
				}

				// TODO: Check for per-provider parms
			}
		}
		

		bool CheckConfigurationFileSetting()
		{
			///////////
			///Make sure there is a namespace file pointed to by the configuration settings
			if (FederationNamespaceMapLogicalPath == null)
			{
				Result r = new Result("Missing configuration file setting", Result.Level.Error);
				r.Writer.Write(@"<p>You are missing the setting for <b>FederationNamespaceMapFile</b> in your <b>web.config</b> file.
	<p>Here is an example of a valid web.config file:");
				WriteExampleWebConfig();
				r.Writer.Write(@"<p>Note that some of the authentication settings may vary for your site.  This example is configured for Windows network authentication.  
	<p></b>FederationNamespaceMapFile</b> must be set to the logical web path of a valid FlexWiki federation configuration file.  Here 
	is an example of a valid federation configuration file:");			
				r.Writer.Write(ExampleConfig());
				AddResult(r);
				return false;
			}
			OK("Federation configuration file identified in web.config", UIResponse.Escape(FederationNamespaceMapLogicalPath));
			return true;
		}

		public void WriteStoplightTo(HTMLWriter w)
		{
			string image = null;
			string text = null;
			switch (AggregateResultLevel)
			{
				case ConfigurationChecker.Result.Level.OK:
					image = "../images/green-light";
					text = "FlexWiki is correctly configured.";
					break;

				case ConfigurationChecker.Result.Level.Warning:
					image = "../images/yellow-light";
					text = "FlexWiki is probably correctly configured, but you should review configuration warnings.";
					break;

				case ConfigurationChecker.Result.Level.Error:
					image = "../images/red-light";
					text = "FlexWiki is not configured correctly.";
					break;
			}

			w.WriteCenter(
				w.Link("Config.aspx", w.Image(image + ".gif"), "Review configuration details") + 
				w.Para(w.Bold(HTMLWriter.Escape(text)) + "<br/>" + "For more information, click on the stoplight."));
		}

		bool CheckConfigurationFileExists()
		{
			if (!File.Exists(FederationNamespaceMap))
			{
				Result r = new Result("Missing configuration file", Result.Level.Error);
				r.Writer.Write(@"<p>You specified a federation configuration file, but it can't be found.  Given your current settings,
this file must be present at <b>" + HTMLWriter.Escape(FederationNamespaceMap) + @"</b> and be a valid federation configuration file.
<p>Here is an example of a valid federation configuration file:");
				r.Writer.Write(ExampleConfig());
				AddResult(r);
				return false;
			}
			OK("Federation configuration file found at: " + HTMLWriter.Escape(FederationNamespaceMap));
			
			Result r2 = new Result("Federation configuration", Result.Level.Info);
			r2.Writer.Write("<pre>");
			using (TextReader sr = new StreamReader(FederationNamespaceMap))
			{
				string s = sr.ReadToEnd();
				r2.Writer.Write(HTMLWriter.Escape(s));
			}
			r2.Writer.Write("</pre>");
			AddResult(r2);
			return true;
		}

		//////////
		///Make sure we can read the configuration file in
		///
		FederationConfiguration CheckConfigurationFileCanBeRead()
		{
			FederationConfiguration config = null;
			try
			{
				config = FederationConfiguration.FromFile(FederationNamespaceMap);
			}
			catch (Exception ex)
			{
				Result r = new Result("Missing configuration file", Result.Level.Error);
				r.Writer.Write(@"<p>You specified a federation configuration file, but an error occurred while reading it.  The federation
configuration file is stored here: <b>" + HTMLWriter.Escape(FederationNamespaceMap) + @"</b> and and the following error occurred while reading the file:");
				r.Writer.Write("<p><blockquote>" + HTMLWriter.Escape(ex.ToString()) + "</blockquote>");
				r.Writer.Write(@"<p>Here is an example of a valid federation configuration file:");
				r.Writer.Write(ExampleConfig());
				AddResult(r);
				return null;
			}
			OK("Federation configuration file successfully read");
			return config;
		}


		
		static string ExampleConfig()
		{
			HTMLStringWriter w = new HTMLStringWriter();
			w.Write("<blockquote><pre>");
			w.Write(HTMLStringWriter.Escape(@"<?xml version=""1.0"" encoding=""utf-8""?>
<FederationConfiguration>
  <DefaultNamespace>FlexWiki</DefaultNamespace>
  <Namespaces>
		<Namespace Id=""238f8774-a470-4f2e-93a0-07252e99fcb9"" Type=""FlexWiki.FileSystemStore"" Connection="".\wikibases\FlexWiki"" Namespace=""FlexWiki"" />
		<Namespace Id=""890e8874-a470-4f2e-93a0-07252e99ed19"" Type=""FlexWiki.FileSystemStore"" Connection="".\wikibases\Some.Other.Namespace"" Namespace=""Some.Other.Namespace"" />
  </Namespaces>
  <About>This site is the home of FlexWiki, an experimental collaboration tool.</About>
</FederationConfiguration>"));
			w.Write("</pre></blockquote>");
			return w.ToString();
		}

		static string WriteExampleWebConfig()
		{
			HTMLStringWriter w = new HTMLStringWriter();
			w.Write("<blockquote><pre>");
			w.Write(HTMLStringWriter.Escape(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
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
			w.Write("</pre></blockquote>");
			return w.ToString();
		}

	}
}
