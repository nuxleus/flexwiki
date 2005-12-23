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
using System.Xml.XPath; 

using NUnit.Framework; 

namespace FlexWiki.BuildVerificationTests
{
  [TestFixture]
	public class RssTests
	{
    private string _rssUrl; 
    private Federation _federation; 
    private WikiState _oldWikiState; 
    private TestContent _testContent = new TestContent(
      new TestNamespace("NamespaceOne", 
      new TestTopic("TopicOne", "This is some test content in NamespaceOne"),
      new TestTopic("TopicTwo", "This is some other test content in NamespaceTwo")
      ),
      new TestNamespace("NamespaceTwo",
      new TestTopic("TopicOne", "This is some test content in NamespaceTwo"),
      new TestTopic("TopicThree", "This is yet more content in NamespaceTwo"),
      new TestTopic("TopicOther", "This is some old test content in NamespaceTwo", 
      "This is some new test content in NamespaceTwo")
      )
      );      


    [SetUp]
    public void SetUp()
    {
      // Get the base URL from the config file and gen up the web service endpoint
      string baseUrl = TestUtilities.BaseUrl;
      _rssUrl = baseUrl + "rss.aspx"; 

      // Back up the wiki configuration
      _oldWikiState = TestUtilities.BackupWikiState(); 

      // Recreate the wiki each time so we start from a known state. Otherwise, tests
      // that (for example) retrieve old versions might not get what they expect.
      _federation = TestUtilities.CreateFederation("TestFederation", _testContent); 
    }

    [TearDown]
    public void TearDown()
    {
      TestUtilities.RestoreWikiState(_oldWikiState); 
    }

    [Test] public void RssValid()
    {
      XmlTextReader xmlReader = new XmlTextReader(_rssUrl); 
      XmlDocument doc = new XmlDocument(); 
      doc.Load(xmlReader); 

      XPathNavigator navigator = doc.CreateNavigator(); 
      Assert.AreEqual(1, navigator.Evaluate("count(/rss)"), "Checking that the root element is 'rss'."); 
      Assert.IsTrue(0 < Convert.ToInt32(navigator.Evaluate("count(//item)")), 
        "Checking that there is at least one item in the feed."); 
    }

  }
}
