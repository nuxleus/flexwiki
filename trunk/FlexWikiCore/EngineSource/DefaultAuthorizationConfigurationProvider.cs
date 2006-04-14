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
using System.IO; 
using System.Reflection;
using System.Xml; 
using System.Xml.Serialization; 

namespace FlexWiki
{
	public class DefaultAuthorizationConfigurationProvider : AuthorizationConfigurationProviderBase
	{
    private AuthorizationConfiguration _wikiConfiguration; 

		public DefaultAuthorizationConfigurationProvider()
		{
		}

    private AuthorizationConfiguration DefaultWikiConfiguration
    {
      get
      {
        AuthorizationConfiguration configuration = new AuthorizationConfiguration(); 
        configuration.Administrators.Add("*");
        configuration.Editors.Add("*"); 
        configuration.Readers.Add("*"); 

        return configuration; 
      }
    }

    public override AuthorizationConfiguration WikiConfiguration
    {
      get 
      { 
        if (_wikiConfiguration == null)
        {
          // Look for a file called security.config in the same place as the web.config
          // or app.config
          string path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, 
            "security.config"); 

          // If it doesn't exist, use the default configuration, which is just to allow
          // everyone to do everything. 
          if (!File.Exists(path))
          {
            _wikiConfiguration = DefaultWikiConfiguration; 
          }
          else
          {
            SecurityConfiguration securityConfiguration = null; 

            XmlSerializer ser = new XmlSerializer(typeof(SecurityConfiguration)); 
            XmlReader reader = new XmlTextReader(path); 
            try
            {
              securityConfiguration = (SecurityConfiguration) ser.Deserialize(reader); 
            }
            finally
            {
              reader.Close(); 
            }

            _wikiConfiguration = securityConfiguration.AuthorizationConfiguration; 
          }
        }

        return _wikiConfiguration; 
      }
    }

    public override AuthorizationConfiguration GetNamespaceConfiguration(string nmspc)
    {
      return null;
    }

	}
}
