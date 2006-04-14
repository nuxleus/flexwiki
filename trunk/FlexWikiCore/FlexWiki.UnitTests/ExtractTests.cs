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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using FlexWiki.Formatting;
using System.Xml; 
using System.Xml.Serialization; 


namespace FlexWiki.UnitTests
{
  [TestFixture] public class ExtractTests 
  {
    string full = "HomePage(2003-11-24-20-31-20-WINGROUP-davidorn)";

    [Test] public void VersionTest()
    {
      Assert.AreEqual("2003-11-24-20-31-20-WINGROUP-davidorn", FlexWiki.FileSystemStore.ExtractVersionFromHistoricalFilename(full));
    }

    [Test] public void NameTest()
    {
      Assert.AreEqual("HomePage",  FlexWiki.FileSystemStore.ExtractTopicFromHistoricalFilename(full));
    }			
  }
}
