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
using System.Configuration; 
using System.IO; 
using System.Text.RegularExpressions; 

using NUnit.Framework; 

using FlexWiki.BuildVerificationTests.EditService; 

namespace FlexWiki.BuildVerificationTests
{
  [TestFixture]
  public class WebServiceTests
  {
    private Federation federation; 
    private WikiState oldWikiState; 
    private EditServiceProxy proxy; 
    private TestContent testContent = new TestContent(
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
      proxy = new EditServiceProxy(); 

      // Get the base URL from the config file and gen up the web service endpoint
      string baseUrl = TestUtilities.BaseUrl;
      proxy.Url = baseUrl + "EditService.asmx"; 

      // Back up the wiki configuration
      oldWikiState = TestUtilities.BackupWikiState(); 

      // Recreate the wiki each time so we start from a known state. Otherwise, tests
      // that (for example) retrieve old versions might not get what they expect.
      federation = TestUtilities.CreateFederation("TestFederation", testContent); 
    }

    [TearDown]
    public void TearDown()
    {
      TestUtilities.RestoreWikiState(oldWikiState); 
    }

    [Test]
    public void CanEdit()
    {
      string visitorIdentityString = proxy.CanEdit(); 
      Assert.IsNotNull(visitorIdentityString, "Checking that CanEdit returns a non-null string"); 
    }

    [Test]
    public void GetAllNamespaces()
    {
      EditService.ContentBase[] bases = proxy.GetAllNamespaces(); 
      Assert.AreEqual(testContent.Namespaces.Length, bases.Length, "Checking that the correct number of content bases was returned"); 
      
      for (int i = 0; i < testContent.Namespaces.Length; ++i)
      {
        Assert.IsTrue(HasNamespace(bases, testContent.Namespaces[i].Name), 
          string.Format("Checking that the namespace {0} was returned", i)); 
      }
    }

    [Test]
    public void GetAllTopics()
    {
      TestNamespace testNamespace = testContent.Namespaces[1]; 
      EditService.ContentBase contentBase = new EditService.ContentBase(); 
      contentBase.Namespace = testNamespace.Name; 
      EditService.AbsoluteTopicName[] topics = proxy.GetAllTopics(contentBase); 

      // We might get some extra topics back, as FlexWiki will create things like 
      // HomePage and _ContentBaseDefinition for us. 
      Assert.IsTrue(testNamespace.Topics.Length <= topics.Length, 
        "Checking that the right number of topics were returned");

      // The list we get back is unordered, so we can't just walk the two arrays
      // and compare them. Plus, as mentioned above, there might be extras. So we're
      // happy as long as all the ones we expect to be there still are. 
      foreach (TestTopic topic in testNamespace.Topics)
      {
        Assert.IsTrue(HasTopic(topics, topic.Name), 
          string.Format("Checking that topic {0} was returned", topic.Name)); 
      }
    }

    [Test]
    public void GetDefaultNamespace()
    {
      EditService.ContentBase contentBase = proxy.GetDefaultNamespace(); 

      Assert.AreEqual(testContent.Namespaces[0].Name, contentBase.Namespace, 
        "Checking that the correct default namespace was returned"); 
    }

    [Test]
    public void GetHtmlForTopic()
    {
      TestNamespace testNamespace = testContent.Namespaces[1]; 
      EditService.AbsoluteTopicName topicName = new EditService.AbsoluteTopicName(); 
      topicName.Namespace = testNamespace.Name; 
      topicName.Name = testNamespace.Topics[1].Name; 
      string html = proxy.GetHtmlForTopic(topicName); 

      Assert.IsFalse(html.IndexOf("This is yet more content") == -1, 
        "Checking that plain wiki text is rendered to HTML correctly"); 
      Assert.IsTrue(Regex.IsMatch(html, @"\<a.*\>NamespaceTwo.*\</a\>"), 
        "Checking that a link was rendered"); 
    }

    [Test]
    public void GetHtmlForTopicVersion()
    {
      TestNamespace testNamespace = testContent.Namespaces[1]; 
      TestTopic testTopic = testNamespace.Topics[2];
      string oldVersion = GetVersions(testNamespace.Name, testTopic.Name)[0]; 

      EditService.AbsoluteTopicName topicName = new EditService.AbsoluteTopicName(); 
      topicName.Namespace = testNamespace.Name; 
      topicName.Name = testTopic.Name; 
      string html = proxy.GetHtmlForTopicVersion(topicName, oldVersion); 
    
      Assert.IsFalse(html.IndexOf("This is some old test content") == -1, 
        "Checking that old content was returned"); 
    }

    [Test]
    public void GetPreviewForTopic()
    {
      TestNamespace testNamespace = testContent.Namespaces[1]; 
      TestTopic testTopic = testNamespace.Topics[2];
      EditService.AbsoluteTopicName topicName = new EditService.AbsoluteTopicName(); 
      topicName.Namespace = testNamespace.Name; 
      topicName.Name = testTopic.Name; 

      string html = proxy.GetPreviewForTopic(topicName, "This is some unrelated text with a WikiWord in it."); 

      Assert.IsTrue(html.IndexOf("This is some unrelated text") != 0, 
        "Checking that plain text was rendered to HTML"); 
      Assert.IsTrue(Regex.IsMatch(html, @"\<a.*\>WikiWord.*\</a\>"), 
        "Checking that a link was rendered"); 

    } 

    [Test]
    public void GetTextForTopic()
    {
      TestNamespace testNamespace = testContent.Namespaces[1]; 
      TestTopic testTopic = testNamespace.Topics[2];
      EditService.AbsoluteTopicName topicName = new EditService.AbsoluteTopicName(); 
      topicName.Namespace = testNamespace.Name; 
      topicName.Name = testTopic.Name; 

      string text = proxy.GetTextForTopic(topicName); 

      Assert.AreEqual(testTopic.LatestContent, text, "Checking that correct text was returned"); 
    }

    [Test]
    public void GetVersionsForTopic()
    {
      TestNamespace testNamespace = testContent.Namespaces[1]; 
      TestTopic testTopic = testNamespace.Topics[2];
      EditService.AbsoluteTopicName topicName = new EditService.AbsoluteTopicName(); 
      topicName.Namespace = testNamespace.Name; 
      topicName.Name = testTopic.Name; 

      string[] versions = proxy.GetVersionsForTopic(topicName); 
      StringCollection expectedVersions = GetVersions(testNamespace.Name, testTopic.Name); 

      Assert.AreEqual(expectedVersions.Count, versions.Length, "Checking that right number of versions were returned"); 

      int n = expectedVersions.Count; 
      for (int i = 0; i < n; ++i)
      {
        // Versions are returned from the web service in reverse order
        Assert.AreEqual(expectedVersions[i], versions[n - i - 1], "Checking that correct version was returned"); 
      }

    }

    [Test]
    public void GetWikiVersion()
    {
      WikiVersion version = proxy.GetWikiVersion(); 

      bool nonzeroVersion = 
        version.Major != 0 ||
        version.Minor != 0 ||
        version.Build != 0 ||
        version.Revision != 0; 

      Assert.IsTrue(nonzeroVersion, "Checking that at least one element of the wiki version is nonzero"); 
    }

    [Test]
    public void RestoreTopic()
    {
      TestNamespace testNamespace = testContent.Namespaces[1]; 
      TestTopic testTopic = testNamespace.Topics[2];
      EditService.AbsoluteTopicName topicName = new EditService.AbsoluteTopicName(); 
      topicName.Namespace = testNamespace.Name; 
      topicName.Name = testTopic.Name; 
      StringCollection versions = GetVersions(testNamespace.Name, testTopic.Name); 

      proxy.RestoreTopic(topicName, "BuildVerificationTests", versions[0]); 

      string text = proxy.GetTextForTopic(topicName); 

      Assert.AreEqual(testTopic.ContentHistory[0], text, "Checking that topic text was restored"); 
    }

    [Test]
    public void SetTextForTopic()
    {
      TestNamespace testNamespace = testContent.Namespaces[1]; 
      TestTopic testTopic = testNamespace.Topics[2];
      string newContent = "This is the replacement text.";

      EditService.AbsoluteTopicName topicName = new EditService.AbsoluteTopicName(); 
      topicName.Namespace = testNamespace.Name; 
      topicName.Name = testTopic.Name; 

      proxy.SetTextForTopic(topicName, newContent, "BuildVerificationTests"); 

      string retrievedContent = proxy.GetTextForTopic(topicName); 

      Assert.AreEqual(newContent, retrievedContent, "Checking that topic text was updated on the remote wiki."); 

    }
    
    private StringCollection GetVersions(string ns, string topic)
    {
      StringCollection versions = new StringCollection(); 
      FlexWiki.AbsoluteTopicName atn = new FlexWiki.AbsoluteTopicName(topic, ns); 
      foreach (TopicChange change in federation.GetTopicChanges(atn))
      {
        // They appear to come in reverse chronological order, so we 
        // add them in reverse order. 
        versions.Insert(0, change.Version); 
      }

      return versions; 
    }

    private bool HasNamespace(EditService.ContentBase[] bases, string name)
    {
      foreach (EditService.ContentBase contentBase in bases)
      {
        if (contentBase.Namespace == name)
        {
          return true; 
        }
      }

      return false; 
    }

    private bool HasTopic(EditService.AbsoluteTopicName[] topics, string name)
    {
      foreach (EditService.AbsoluteTopicName topic in topics)
      {
        if (topic.Name == name)
        {
          return true; 
        }
      }

      return false; 
    }

  }
}

