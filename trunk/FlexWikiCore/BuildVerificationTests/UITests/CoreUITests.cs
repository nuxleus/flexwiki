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
		public CoreUITests() : base()
		{
		}

		AbsoluteTopicName HomePage
		{
			get
			{
				return new AbsoluteTopicName(TheFederation.DefaultContentBase.HomePage, TheFederation.DefaultContentBase.Namespace);
			}
		}
		
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
			AbsoluteTopicName top = new AbsoluteTopicName("TitledTopic", TheFederation.DefaultContentBase.Namespace);
			string t = TheLinkMaker.LinkToTopic(top);
			DocumentElement doc = TheBrowser.Navigate(t, true);
			
			Assert.AreEqual("This fat hen", doc.Title);
		}

		
		[Test]
		public void ControlTopicBar()
		{
			AbsoluteTopicName top = new AbsoluteTopicName("TitledTopic", TheFederation.DefaultContentBase.Namespace);
			string t = TheLinkMaker.LinkToTopic(top);
			DocumentElement doc = TheBrowser.Navigate(t, true);
			HTMLElement staticTopicBar = (HTMLElement)doc.GetElementByName("StaticTopicBar");

			Assert.AreEqual("This fat hen", staticTopicBar.InnerText);
		}

		[Test]
		public void CreateTestPage()
		{
			AbsoluteTopicName top = new AbsoluteTopicName("DummyPage", TheFederation.DefaultContentBase.Namespace);

			bool exists;
			
			exists = TheFederation.TopicExists(top);
			Assert.IsTrue(!exists);

			string home = TheLinkMaker.LinkToTopic(top);
			DocumentElement doc = TheBrowser.Navigate(home, true);
			Assert.IsTrue(doc.Body.OuterHTML.IndexOf("Formatting Tips") > 0);
			InputElement text = (InputElement) doc.GetElementByName("Text1");
			text.Value = "This is SoCool!";
			ButtonElement save = (ButtonElement) doc.GetElementByName("SaveButton");

			save.Click(true);
			// Make sure it actually got saved
			exists = TheFederation.TopicExists(top);
			Assert.IsTrue(exists);
		}

		[Test]
		public void TestNamespaceContents()
		{
//				System.Diagnostics.Debugger.Break();

			string rc = TheLinkMaker.LinkToRecentChanges(TheFederation.DefaultNamespace);
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

	}
}
