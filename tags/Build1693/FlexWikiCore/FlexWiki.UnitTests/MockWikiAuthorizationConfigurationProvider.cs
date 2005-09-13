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

namespace FlexWiki.UnitTests
{
	public class MockAuthorizationConfigurationProvider : AuthorizationConfigurationProviderBase
	{
    private readonly AuthorizationConfiguration _wikiConfiguration = new AuthorizationConfiguration(); 
    private readonly Hashtable _namespaceConfigurations = new Hashtable(); 


		public MockAuthorizationConfigurationProvider()
		{
		}

    public override AuthorizationConfiguration WikiConfiguration
    {
      get { return _wikiConfiguration; }
    }

    public override AuthorizationConfiguration GetNamespaceConfiguration(string nmspc)
    {
      return (AuthorizationConfiguration) _namespaceConfigurations[nmspc]; 
    }

    internal void AddNamespacePermission(string principal, string nmspc, Permission permission)
    {
      AuthorizationConfiguration namespaceConfiguration = GetOrCreateNamespaceConfiguration(nmspc); 

      AddPermission(namespaceConfiguration, principal, permission); 
    }

    internal void AddWikiPermission(string principal, Permission permission)
    {
      AddPermission(_wikiConfiguration, principal, permission); 
    }

    private void AddPermission(AuthorizationConfiguration configuration, string principal, Permission permission)
    {
      if (permission == Permission.Administer)
      {
        configuration.Administrators.Add(principal); 
      }
      else if (permission == Permission.Edit)
      {
        configuration.Editors.Add(principal); 
      }
      else if (permission == Permission.Read)
      {
        configuration.Readers.Add(principal); 
      }
    }
    private AuthorizationConfiguration GetOrCreateNamespaceConfiguration(string nmspc)
    {
      if (_namespaceConfigurations.ContainsKey(nmspc))
      {
        return (AuthorizationConfiguration) _namespaceConfigurations[nmspc]; 
      }

      AuthorizationConfiguration namespaceConfiguration = new AuthorizationConfiguration(); 
      _namespaceConfigurations[nmspc] = namespaceConfiguration; 
      return namespaceConfiguration; 
    }


	}
}
