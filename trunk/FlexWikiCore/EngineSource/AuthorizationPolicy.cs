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
using System.Collections.Specialized; 
using System.Security.Principal; 
using System.Threading; 

namespace FlexWiki
{
	internal class AuthorizationPolicy
	{
    private AuthorizationConfigurationProviderBase _configurationProvider = new DefaultAuthorizationConfigurationProvider(); 
    private Federation _federation; 

    internal AuthorizationConfigurationProviderBase ConfigurationProvider
    {
      get { return _configurationProvider; } 
      set { _configurationProvider = value; }
    }

    internal bool HasPermission(string nmspc, Permission permission)
    {
      // If someone passes in a null namespace (this can easily happen when non-readers
      // call a method that attempts to retrieve the namespace name) be sure to indicate
      // that permission is not granted. 
      if (nmspc == null)
      {
        return false; 
      }

      IPrincipal principal = Thread.CurrentPrincipal; 
      
      AuthorizationConfiguration configuration = ConfigurationProvider.GetNamespaceConfiguration(nmspc); 

      // Default to wiki configuration if there's no configuration provided by the namespace
      if (configuration == null)
      {
        configuration = ConfigurationProvider.WikiConfiguration; 
      }

      if (permission == Permission.Read)
      {
        return IsReader(principal, configuration) || IsEditor(principal, configuration) || IsAdministrator(principal, configuration); 
      }
      else if (permission == Permission.Edit)
      {
        return IsEditor(principal, configuration) || IsAdministrator(principal, configuration); 
      }
      else if (permission == Permission.Administer)
      {
        return IsAdministrator(principal, configuration); 
      }
      else
      {
        throw new ArgumentException(string.Format("Unexpected permission {0}", permission), "permission"); 
      }
    }

    internal void Initialize(Federation federation)
    {
      _federation = federation; 
      _configurationProvider.Initialize(federation); 
    }

    private static bool CaseInsensitiveEquivalent(string a, string b)
    {
      return CaseInsensitiveComparer.DefaultInvariant.Compare(a, b) == 0; 
    }

    private static bool IsAdministrator(IPrincipal principal, AuthorizationConfiguration configuration)
    {
      return IsUserOrInRole(principal, configuration.Administrators); 
    }

    private static bool IsEditor(IPrincipal principal, AuthorizationConfiguration configuration)
    {
      return IsUserOrInRole(principal, configuration.Editors); 
    }

    private static bool IsReader(IPrincipal principal, AuthorizationConfiguration configuration)
    {
      return IsUserOrInRole(principal, configuration.Readers); 
    }

    private static bool IsUserOrInRole(IPrincipal principal, StringCollection allowedPrincipals)
    {
      foreach (string allowedPrincipal in allowedPrincipals)
      {
        if (allowedPrincipal == "*")
        {
          return true; 
        }

        if (allowedPrincipal == "?")
        {
          if (principal.Identity.IsAuthenticated)
          {
            return true; 
          }
        }

        if (CaseInsensitiveEquivalent(principal.Identity.Name, allowedPrincipal))
        {
          return true; 
        }

        if (principal.IsInRole(allowedPrincipal))
        {
          return true; 
        }
      }

      return false; 
    }




	}
}
