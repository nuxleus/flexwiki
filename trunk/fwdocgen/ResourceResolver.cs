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
using System.Xml;
using System.Xml.Xsl; 
using System.Reflection; 

namespace FlexWiki.FwDocGen
{
  public class ResourceResolver : XmlResolver
  {
    public override System.Net.ICredentials Credentials
    {
      set
      {
      }
    }


    public override Uri ResolveUri(Uri baseUri, string relativeUri)
    {
      if (baseUri == null)
      {
        Uri uri = new Uri("resource://fwdocgen.exe/" + relativeUri); 
        return uri; 
      }
      else
      {
        return null; 
      }
    }

    public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
    {
      if (absoluteUri.Scheme == "resource")
      {
        // Strip the leading "/"
        string name = absoluteUri.LocalPath.Substring(1); 
        string resource = "FlexWiki.FwDocGen." + name; 
        return Assembly.GetExecutingAssembly().GetManifestResourceStream(resource); 
      }
      else
      {
        return null; 
      }
    }

  }
}
  