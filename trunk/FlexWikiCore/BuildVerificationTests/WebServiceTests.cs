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
using System.Configuration; 
using System.IO; 

using NUnit.Framework; 

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
        new TestTopic("TopicOther", "This is some other test content in NamespaceTwo")
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

      // Recreate the wiki each time so we start from a known state
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
      ContentBaseWireFormat[] bases = proxy.GetAllNamespaces(); 
      Assert.AreEqual(testContent.Namespaces.Length, bases.Length, "Checking that the correct number of content bases was returned"); 
      
      for (int i = 0; i < testContent.Namespaces.Length; ++i)
      {
        Assert.IsTrue(HasNamespace(bases, testContent.Namespaces[i].Name), 
          string.Format("Checking that the namespace {0} was returned", i)); 
      }
    }

    [Test]
    public void GetDefaultNamespace()
    {
      ContentBaseWireFormat contentBase = proxy.GetDefaultNamespace(); 

      Assert.AreEqual(testContent.Namespaces[0].Name, contentBase.Namespace, 
        "Checking that the correct default namespace was returned"); 
    }

    private bool HasNamespace(ContentBaseWireFormat[] bases, string name)
    {
      foreach (ContentBaseWireFormat contentBase in bases)
      {
        if (contentBase.Namespace == name)
        {
          return true; 
        }
      }

      return false; 
    }

  }
}

