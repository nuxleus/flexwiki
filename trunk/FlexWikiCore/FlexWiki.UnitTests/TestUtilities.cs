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
using System.IO; 

namespace FlexWiki.UnitTests
{
  public class TestUtilities
  {
    public static void WriteConfigFile()
    {
      WriteFromResource("FlexWiki.UnitTests.dll.config");
    }

    public static void WritePermissionsFile()
    {
      WriteFromResource("TestPermissions.xml", "PermissionsData.config");
    }

    public static void WriteUserFile()
    {
      WriteFromResource("TestUser.xml", "UserData.config"); 
    }

    private static void WriteFromResource(string resourceName)
    {
      WriteFromResource(resourceName, resourceName); 
    }

    private static void WriteFromResource(string resourceName, string filePath)
    {
      Stream stm = Assembly.GetExecutingAssembly().GetManifestResourceStream("FlexWiki.UnitTests." + 
        resourceName);
      try
      {
        FileStream fs = File.Create(filePath);
        try
        {
          int bufSize = 8000; 
          byte[] buf = new byte[bufSize];
          int read = 0; 
          while ((read = stm.Read(buf, 0, bufSize)) > 0)
          {
            fs.Write(buf, 0, read); 
          }
        }
        finally
        {
          fs.Close(); 
        }
      }
      finally
      {
        stm.Close(); 
      }
    }
  }
}