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
using System.IO; 

namespace FlexWiki.FwDocGen
{
  public class FileResolver : XmlResolver
  {
    private string dir; 

    public FileResolver(string dir)
    {
      this.dir = Path.GetFullPath(dir); 
    }

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
        string path = Path.Combine(dir, relativeUri); 
        Uri uri = new Uri(Uri.UriSchemeFile + Uri.SchemeDelimiter + path); 
        return uri; 
      }
      else
      {
        return null; 
      }
    }

    public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
    {
      if (absoluteUri.Scheme == Uri.UriSchemeFile)
      {
        return File.OpenRead(absoluteUri.AbsolutePath); 
      }
      else
      {
        return null; 
      }
    }

  }
}
  