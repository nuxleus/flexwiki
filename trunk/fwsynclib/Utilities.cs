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
using System.IO; 
using System.Collections;
using System.Collections.Specialized; 
using System.Security.Cryptography; 
using System.Net; 
using System.Text; 

namespace FlexWiki.FwSyncLib
{
	public class Utilities
	{

    public static long ComputeChecksumFromFile(string path)
    {
      StreamReader rdr = new StreamReader(path, Encoding.ASCII, true); 
      string content = null; 
      try
      {
        content = rdr.ReadToEnd(); 
      }
      finally
      {
        rdr.Close(); 
      }
      return ComputeChecksumFromString(content); 
    }
    
    public static long ComputeChecksumFromString(string content)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(content); 
      MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider(); 
      byte[] hash = md5.ComputeHash(bytes); 

      return hash[0] + (hash[1] << 8) + (hash[2] << 16) + (hash[3] << 24); 
    }

    public static string GetTopicContent(LocalTopic topic)
    {
      StreamReader rdr = new StreamReader(topic.Path, System.Text.Encoding.ASCII); 
      string content = rdr.ReadToEnd();
      rdr.Close(); 

      return content; 			
    }

    public static INamedObject LookupByName(ICollection c, string name)
    {
      foreach (INamedObject no in c)
      {
        if (no.Name == name)
        {
          return no;
        }
      }

      return null; 
    }

	}
}

