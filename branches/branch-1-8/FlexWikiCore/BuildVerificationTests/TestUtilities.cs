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
using System.Configuration; 
using System.IO; 
using System.Runtime.InteropServices; 
using System.Xml; 
using System.Xml.XPath; 

using Microsoft.Win32.Security; 

using FlexWiki.Formatting;

namespace FlexWiki.BuildVerificationTests
{
  internal sealed class TestUtilities
  {
    private TestUtilities()
    {
    }

    
    internal static string BaseUrl
    {
      get 
      {
        return ConfigurationSettings.AppSettings["InstallationUri"];
      }
    }


    internal static WikiState BackupWikiState()
    {
      string configFilePath = TestUtilities.ReadConfigFilePath();
      string configFileDir = Path.GetDirectoryName(configFilePath); 
      string backupFile = Path.GetFileName(configFilePath); 
      backupFile = string.Format("{0}.{1}.backup", backupFile, DateTime.Now.ToString("yyyyMMddhhmmss")); 
      string backupPath = Path.Combine(configFileDir, backupFile);

      if (File.Exists(configFilePath))
      {
        File.Move(configFilePath, backupPath); 
      }

      return new WikiState(backupPath); 
    }

    internal static Federation CreateFederation(string root, TestContent content)
    {
      string contentDir = Path.Combine(Path.GetFullPath(root), "WikiBases"); 
      if (Directory.Exists(contentDir))
      {
        Directory.Delete(contentDir, true); 
      }

      Directory.CreateDirectory(contentDir); 
      
      TestUtilities.SetDirectoryWebWritable(contentDir); 

      FederationConfiguration configuration = new FederationConfiguration(); 

      FileSystemNamespaceProvider provider = new FileSystemNamespaceProvider(); 
      bool defaultNamespaceSpecified = false; 
      foreach (TestNamespace ns in content.Namespaces)
      {
        provider.Root = Path.Combine(contentDir, ns.Name); 
        provider.Namespace = ns.Name; 
        NamespaceProviderDefinition definition = new NamespaceProviderDefinition();
        definition.Id = Guid.NewGuid().ToString();
        provider.SavePersistentParametersToDefinition(definition); 
        definition.Type = provider.GetType().FullName; 
        definition.AssemblyName = provider.GetType().Assembly.FullName; 
        configuration.NamespaceMappings.Add(definition); 

        // We always set the first namespace to be the default one
        if (defaultNamespaceSpecified == false)
        {
          configuration.DefaultNamespace = ns.Name; 
          defaultNamespaceSpecified = true; 
        }
      }

      string configFilePath = TestUtilities.ReadConfigFilePath(); 
      configuration.WriteToFile(configFilePath); 
    
      Federation federation = new Federation(OutputFormat.HTML, new LinkMaker(TestUtilities.BaseUrl)); 
      federation.LoadFromConfiguration(configuration); 
      
      foreach (TestNamespace ns in content.Namespaces)
      {
        FlexWiki.ContentBase contentBase = federation.ContentBaseForNamespace(ns.Name); 
        foreach (TestTopic topic in ns.Topics)
        {
          LocalTopicName name = new LocalTopicName(topic.Name); 
          foreach (string text in topic.ContentHistory)
          {
            name.Version = FlexWiki.AbsoluteTopicName.NewVersionStringForUser("BuildVerificationTests");
            contentBase.WriteTopicAndNewVersion(name, text); 
            //CA We need to sleep a little while so we don't try to write two topics 
            //CA within the same time slice, or they get the same version name. 
            System.Threading.Thread.Sleep(30); 
          }
        }
      }

	
    	RestartWebApplication();

    	return federation; 

    }

  	// Touch the web.config file to force the web app to restart and get back to a known state
	 private static void RestartWebApplication()
  	{
  		StreamReader i = File.OpenText(WebConfigPath);
  		string text = i.ReadToEnd();
  		i.Close();
  		StreamWriter o = new StreamWriter(WebConfigPath, false);
  		o.Write(text);
  		o.Close();
  	}

  	internal static string WebConfigPath
  {
	  get
	  {
		  string installationPath = ConfigurationSettings.AppSettings["InstallationPath"];
		  installationPath = Path.GetFullPath(installationPath);        
		  return Path.Combine(installationPath, "web.config");
	  }
  }

    internal static string ReadConfigFilePath()
    {
      string installationPath = ConfigurationSettings.AppSettings["InstallationPath"];
      installationPath = Path.GetFullPath(installationPath); 
        
      string webConfigPath = Path.Combine(installationPath, "web.config"); 

      XPathDocument doc = new XPathDocument(webConfigPath); 
      XPathNavigator nav = doc.CreateNavigator(); 

      string configFilePath = (string) nav.Evaluate("string(/configuration/appSettings/add[@key='FederationNamespaceMapFile']/@value)"); 

      if (configFilePath.StartsWith("~/"))
      {
        configFilePath = configFilePath.Substring(2); 
      }

      configFilePath = configFilePath.Replace("/", Path.DirectorySeparatorChar.ToString()); 

      configFilePath = Path.Combine(installationPath, configFilePath); 

      return configFilePath; 

    }

    internal static void RestoreWikiState(WikiState state)
    {
      if (File.Exists(state.ConfigPath))
      {
        string configFilePath = TestUtilities.ReadConfigFilePath(); 
        if (File.Exists(configFilePath))
        {
          File.Delete(configFilePath);
        }

        File.Move(state.ConfigPath, configFilePath); 
      }
    }


    internal static void SetDirectoryWebWritable(string path)
    {
      SecurityDescriptor sd = SecurityDescriptor.GetFileSecurity(path, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION); 
      Dacl dacl = sd.Dacl;

      AddAceForAccount(dacl, "NETWORK SERVICE"); // IIS6 process identity
      AddAceForAccount(dacl, "ASPNET");          // IIS5 process identity

      sd.SetDacl(dacl); 
      sd.SetFileSecurity(path, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION);
    }

    private static void AddAceForAccount(Dacl dacl, string account)
    {
      bool accountExists = true;

      Sid sid = null; 
      try
      {
        sid = new Sid(account); 
      }
      catch (COMException)
      {
        accountExists = false; 
      }

      if (accountExists)
      {
        AceAccessAllowed netAce = new AceAccessAllowed(sid, AccessType.GENERIC_ALL, AceFlags.CONTAINER_INHERIT_ACE | AceFlags.OBJECT_INHERIT_ACE); 
        dacl.AddAce(netAce); 
      }

    }
    
  }
}
