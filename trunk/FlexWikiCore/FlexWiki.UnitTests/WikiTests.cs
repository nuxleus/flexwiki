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

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for WikiTests.
	/// </summary>
	public abstract class WikiTests
	{
		public WikiTests()
		{
		}

		protected Federation TheFederation;

    protected void WriteResourceToFile(Assembly a, string resourceName, string path)
    {
      Stream input = a.GetManifestResourceStream(resourceName); 
      try
      {
        int bufsize = 8000; 
        byte[] buf = new byte[bufsize]; 
        int read = 0; 

        Stream output = File.Open(path, FileMode.Create, FileAccess.Write); 
        try
        {
          while ((read = input.Read(buf, 0, bufsize)) > 0)
          {
            output.Write(buf, 0, read); 
          }
        }
        finally
        {
          output.Close(); 
        }
      }
      finally
      {
        input.Close(); 
      }
    }

		static protected AbsoluteTopicName WriteTestTopicAndNewVersion(ContentBase cb, string localName, string content, string author)
		{
			AbsoluteTopicName name = new AbsoluteTopicName(localName, cb.Namespace);
			name.Version = AbsoluteTopicName.NewVersionStringForUser(author);
			cb.WriteTopicAndNewVersion(name, content);
			return new AbsoluteTopicName(localName, cb.Namespace);
		}


	}
}
