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
using System.Text; 

using NUnit.Framework; 

using FlexWiki.FwSyncLib; 

namespace FlexWiki.FwSyncLib.Tests
{
	[TestFixture]
	public class UtilitiesTests
	{
    [Test]
    public void ComputeChecksum()
    {
      string path = "testfile.wiki"; 
      
      ComputeChecksumInternal(path, Encoding.ASCII, "Test content", -504213429); 
      ComputeChecksumInternal(path, Encoding.UTF8, "Test content", -504213429); 
      ComputeChecksumInternal(path, Encoding.Unicode, "Test content", -504213429); 

    }


    private void ComputeChecksumInternal(string path, System.Text.Encoding encoding, 
      string content, long expectedChecksum)
    {
      if (File.Exists(path))
      {
        File.Delete(path); 
      }
      
      FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write); 
      StreamWriter sw = new StreamWriter(fs, encoding); 

      sw.WriteLine(content); 

      sw.Close(); 

      long correct = expectedChecksum; 

      long checksum = Utilities.ComputeChecksumFromFile(path); 
      Assert.AreEqual(correct, checksum, "Checking for correct checksum with encoding {0}", encoding); 

      checksum = Utilities.ComputeChecksumFromFile(path); 
      Assert.AreEqual(correct, checksum, "Checking that checksum didn't change  with encoding {0}", encoding); 

      File.SetLastWriteTime(path, DateTime.Now + TimeSpan.FromDays(42)); 
      Assert.AreEqual(correct, checksum, "Checking that timestamp does not affect checksum with encoding {0}", encoding); 

      fs = new FileStream(path, FileMode.Append); 
      sw = new StreamWriter(fs, encoding); 

      sw.WriteLine("More content"); 
      sw.Close(); 

      checksum = Utilities.ComputeChecksumFromFile(path); 
      Assert.IsFalse(checksum == correct, "Checking that checksum changes when file contents do with encoding {0}", encoding); 

      File.Delete(path); 

    }


  }
}
