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
using System.Xml; 
using System.Xml.XPath; 

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


    internal static Federation CreateFederation(string root, TestContent content)
    {
      string contentDir = Path.Combine(Path.GetFullPath(root), "WikiBases"); 
      if (Directory.Exists(contentDir))
      {
        Directory.Delete(contentDir, true); 
      }

      Directory.CreateDirectory(contentDir); 

      FederationConfiguration configuration = new FederationConfiguration(); 

      FileSystemNamespaceProvider provider = new FileSystemNamespaceProvider(); 
      foreach (TestNamespace ns in content.Namespaces)
      {
        provider.Root = Path.Combine(contentDir, ns.Name); 
        provider.Namespace = ns.Name; 
        NamespaceProviderDefinition definition = new NamespaceProviderDefinition(); 
        provider.SavePersistentParametersToDefinition(definition); 
        definition.Type = provider.GetType().FullName; 
        definition.AssemblyName = provider.GetType().Assembly.FullName; 
        configuration.NamespaceMappings.Add(definition); 
      }

      string configFilePath = TestUtilities.ReadConfigFilePath(); 
      configuration.WriteToFile(configFilePath); 
    
      Federation federation = new Federation(OutputFormat.HTML, new LinkMaker(TestUtilities.BaseUrl)); 
      federation.LoadFromConfiguration(configuration); 
      
      foreach (TestNamespace ns in content.Namespaces)
      {
        ContentBase contentBase = federation.ContentBaseForNamespace(ns.Name); 
        foreach (TestTopic topic in ns.Topics)
        {
          LocalTopicName name = new LocalTopicName(topic.Name); 
          contentBase.WriteTopic(name, topic.Content); 
        }
      }

      return federation; 

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

    
  }
}
