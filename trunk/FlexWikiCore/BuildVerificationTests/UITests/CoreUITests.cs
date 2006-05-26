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
using System.IO;
using System.Net; 
using System.Threading;

using NUnit.Framework;

using FlexWiki;

namespace FlexWiki.BuildVerificationTests
{
	/// <summary>
	/// Basic tests that simulate the basic functions of visiting the site and changing a few pages
	/// </summary>
	[TestFixture]
	public class CoreUITests : UITests 
	{
		[Test]
		public void CheckHomePage()
		{
			string home = TheLinkMaker.LinkToTopic(HomePage);
			DocumentElement doc = TheBrowser.Navigate(home, true);
			Assert.IsTrue(doc.Body.OuterHTML.IndexOf("The two most important things ") > 0);
		}

		[Test]
		public void ControlPageTitle()
		{
			QualifiedTopicRevision top = new QualifiedTopicRevision("TitledTopic", Federation.DefaultNamespaceManager.Namespace);
			string t = TheLinkMaker.LinkToTopic(top);
			DocumentElement doc = TheBrowser.Navigate(t, true);
			
			Assert.AreEqual("This fat hen", doc.Title);
		}

		[Test]
		public void ControlTopicBar()
		{
			QualifiedTopicRevision top = new QualifiedTopicRevision("TitledTopic", Federation.DefaultNamespaceManager.Namespace);
			string t = TheLinkMaker.LinkToTopic(top);
			DocumentElement doc = TheBrowser.Navigate(t, true);
			HTMLElement staticTopicBar = (HTMLElement)doc.GetElementByName("StaticTopicBar");

			Assert.AreEqual("This fat hen", staticTopicBar.InnerText);
		}

		[Test]
		public void CreateTestPage()
		{
			QualifiedTopicRevision top = new QualifiedTopicRevision("DummyPage", Federation.DefaultNamespaceManager.Namespace);

			bool exists;
			
			exists = Federation.TopicExists(top);
			Assert.IsTrue(!exists);

			string home = TheLinkMaker.LinkToTopic(top);
			DocumentElement doc = TheBrowser.Navigate(home, true);
			Assert.IsTrue(doc.Body.OuterHTML.IndexOf("Formatting Tips") > 0);
			InputElement text = (InputElement) doc.GetElementByName("Text1");
			text.Value = "This is SoCool!";
			ButtonElement save = (ButtonElement) doc.GetElementByName("SaveButton");

			save.Click(true);
			// Make sure it actually got saved
			exists = Federation.TopicExists(top);
			Assert.IsTrue(exists);
		}

/// <summary>
    /// Checks for the presence of bug 1039466: Wiki links always point to localhost
    /// </summary>
    [Test]
    public void LinkMakerBug()
    {
      QualifiedTopicRevision topic = new QualifiedTopicRevision("TopicFour", "NamespaceTwo"); 
      string topicUri = TheLinkMaker.LinkToTopic(topic);
      Uri byName = new Uri(topicUri); 
      string ip = Dns.GetHostEntry(byName.Host).AddressList[0].ToString(); 
      Uri byIP   = new Uri(string.Format("{0}://{1}:{2}{3}", byName.Scheme, ip, byName.Port, byName.AbsolutePath));
      
      DocumentElement docByName = TheBrowser.Navigate(byName.ToString(), true);
      string expectedByName = new Uri(string.Format("{0}://{1}:{2}", byName.Scheme, byName.Host, byName.Port)).ToString();
      Assert.IsFalse(docByName.Body.InnerHTML.IndexOf(expectedByName) == -1, 
        "Checking that links by name were rendered correctly: " + expectedByName); 

      DocumentElement docByIP   = TheBrowser.Navigate(byIP.ToString(), true); 
      string expectedByIP = new Uri(string.Format("{0}://{1}:{2}", byIP.Scheme, byIP.Host, byIP.Port)).ToString();
      Assert.IsFalse(docByName.Body.InnerHTML.IndexOf(expectedByIP) == -1, 
        "Checking that links by IP were rendered correctly: " + expectedByIP); 
    }


     [Test]
    public void Rename()
    {
      QualifiedTopicRevision before = new QualifiedTopicRevision("RenameableTopic", "NamespaceOne"); 
      string renameUrl = TheLinkMaker.LinkToRename(before.DottedName); 
      DocumentElement doc = TheBrowser.Navigate(renameUrl, true); 

      InputElement newName = doc.GetElementByName("newName") as InputElement; 
      newName.Value = "RenamedTopic"; 

      InputElement fixup = doc.GetElementByName("fixup") as InputElement; 
      fixup.Checked = true; 

      fixup.Form.Submit(); 

      // Give the filesystem a chance to flush the changes, even though I hate having 
      // tests that are timing-dependent.
      Thread.Sleep(3000); 

      QualifiedTopicRevision after = new QualifiedTopicRevision("RenamedTopic", "NamespaceOne"); 

      Assert.IsTrue(Federation.TopicExists(after)); 
      string beforeContents = Federation.Read(before); 
      string afterContents = Federation.Read(after); 

      Assert.AreEqual("RenamedTopic", Federation.GetTopicPropertyValue(before, "Redirect"), 
        "Checking that the redirect was put in place"); 
      Assert.AreEqual("This topic can be renamed.", afterContents, 
        "Checking that the renamed topic has the correct contents."); 
      
    }

		[Test]
		public void TestNamespaceContents()
		{
//				System.Diagnostics.Debugger.Break();

			string rc = TheLinkMaker.LinkToRecentChanges(Federation.DefaultNamespace);
			DocumentElement doc = TheBrowser.Navigate(rc, true);
			SelectElement sel = (SelectElement)doc.GetElementByID("NamespaceFilter");
			Assert.IsNotNull(sel);
			IList list = sel.Options;
			Assert.AreEqual(FederationContent.Namespaces.Length, list.Count, "Matching number of namespaces");
			for (int i = 0; i < FederationContent.Namespaces.Length; i++)
			{
				string n = FederationContent.Namespaces[i].Name;
				bool found = false;
				foreach (OptionElement each in list)
				{
					if (each.Text == n)
					{
						found = true;
						break;
					}
				}
				Assert.IsTrue(found, "Finding namespace " + n);
			}
		}


    private QualifiedTopicRevision HomePage
    {
      get
      {
        return new QualifiedTopicRevision(Federation.DefaultNamespaceManager.HomePage, Federation.DefaultNamespaceManager.Namespace);
      }
    }

	}
}
