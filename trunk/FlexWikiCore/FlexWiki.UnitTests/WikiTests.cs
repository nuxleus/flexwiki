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

        // Having weird intermittant problems trying to write the file to disk. Looks
        // like it might be a race condition with someone else trying to close the file. 
        // So if we can't open the file, we sleep and try again a few times. 
        Stream output = null;
        int errorCount = 0; 
        bool done = false;
        while (!done)
        {
          try
          {
            output = File.Open(path, FileMode.Create, FileAccess.Write); 
            done = true; 
          }
          catch (Exception e)
          {
            ++errorCount; 
            if (errorCount > 5)
            {
              throw e; 
            }

            System.Threading.Thread.Sleep(250);
          }

        }

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
