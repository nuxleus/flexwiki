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
	[TestFixture] public class RegistryTests : WikiTests
	{
		ContentBase	_base;
		ContentBase	_other1;
		ContentBase	_other2;
		ContentBase	_other3;
		ContentBase	_cb5;
		const string _bh = "http://boo/";
		LinkMaker _lm;
		
		[SetUp] public void Init()
		{
			string author = "tester-joebob";
			_lm = new LinkMaker(_bh);
			TheFederation = new Federation(OutputFormat.HTML, _lm);

      string testroot = Path.GetFullPath(Path.Combine("TestContent", "RegistryTests")); 

      if (Directory.Exists(testroot))
      {
        Directory.Delete(testroot, true); 
      }

      Directory.CreateDirectory(testroot); 

			string _tempPath = Path.Combine(testroot, @"temp-wikitests");
			string _other1Path = Path.Combine(testroot, @"temp-wikitests1");
			string _other2Path = Path.Combine(testroot, @"temp-wikitests2");
			string _other3Path = Path.Combine(testroot, @"temp-wikitests3");
			string _cb5Path = Path.Combine(testroot, @"temp-wikitests5");

			TheFederation.Register("FlexWiki.Base", _tempPath);
			TheFederation.Register("FlexWiki.Other1", _other1Path);
			TheFederation.Register("Other2", _other2Path);
			TheFederation.Register("Other3", _other3Path);
			TheFederation.Register("Space5", _cb5Path);

			_base = TheFederation.ContentBaseForRoot(_tempPath);
      _base.Namespace = "FlexWiki.Base"; 
      
      _other1 = TheFederation.ContentBaseForRoot(_other1Path);
      _other1.Namespace = "FlexWiki.Other1"; 

      _other2 = TheFederation.ContentBaseForRoot(_other2Path);
      _other2.Namespace = "Other2"; 

      _other3 = TheFederation.ContentBaseForRoot(_other3Path);
      _other3.Namespace = "Other3"; 

      _cb5 = TheFederation.ContentBaseForRoot(_cb5Path);
      _cb5.Namespace = "Space5"; 

			WriteTestTopicAndNewVersion(_base, _base.DefinitionTopicName.Name, @"Namespace: FlexWiki.Base
Import: FlexWiki.Other1, Other2", author);
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"OtherOneHello", author);
			WriteTestTopicAndNewVersion(_base, "TopicTwo", @"FlexWiki.Other1.OtherOneGoodbye", author);
			WriteTestTopicAndNewVersion(_base, "TopicThree", @"No.Such.Namespace.FooBar", author);
			WriteTestTopicAndNewVersion(_base, "TopicFour", @".TopicOne", author);
			WriteTestTopicAndNewVersion(_base, "TopicFive", @"FooBar
Role:Designer", author);
			WriteTestTopicAndNewVersion(_base, "TopicSix", @".GooBar
Role:Developer", author);

			WriteTestTopicAndNewVersion(_other1, _other1.DefinitionTopicName.Name, @"Namespace: FlexWiki.Other1
Import: Other3,Other2", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneHello", @"hello
Role:Developer", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneGoodbye", @"goodbye", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneRefThree", @"OtherThreeTest", author);

			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicOne", @"OtherTwoHello", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicTwo", @"Other2.OtherTwoGoodbye", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicThree", @"No.Such.Namespace.FooBar", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicFour", @".OtherOneTopicOne", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicFive", @"FooBar", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicSix", @".GooBar", author);

			WriteTestTopicAndNewVersion(_other2, _other2.DefinitionTopicName.Name, @"Namespace: Other2", author);
			WriteTestTopicAndNewVersion(_other2, "OtherTwoHello", @"hello", author);
			WriteTestTopicAndNewVersion(_other2, "OtherTwoGoodbye", @"goodbye", author);

			WriteTestTopicAndNewVersion(_other3, _other3.DefinitionTopicName.Name, @"Namespace: Other3", author);
			WriteTestTopicAndNewVersion(_other3, "OtherThreeTest", @"yo", author);

			WriteTestTopicAndNewVersion(_cb5, _cb5.DefinitionTopicName.Name, @"Namespace: Space5
", author);
			WriteTestTopicAndNewVersion(_cb5, "AbsRef", @"Other2.OtherTwoHello", author);

		}

		[Test] public void EnumIncludingImportsTest()
		{
			ArrayList expecting = new ArrayList();
			expecting.Add("FlexWiki.Base._ContentBaseDefinition");
			expecting.Add("FlexWiki.Base.TopicOne");
			expecting.Add("FlexWiki.Base.TopicTwo");
			expecting.Add("FlexWiki.Base.TopicThree");
			expecting.Add("FlexWiki.Base.TopicFour");
			expecting.Add("FlexWiki.Base.TopicFive");
			expecting.Add("FlexWiki.Base.TopicSix");
			foreach (string backing in _base.BackingTopics.Keys)
			{
				expecting.Add(_base.Namespace + "." + backing);
			}			

			expecting.Add("FlexWiki.Other1._ContentBaseDefinition");
			expecting.Add("FlexWiki.Other1.OtherOneHello");
			expecting.Add("FlexWiki.Other1.OtherOneGoodbye");
			expecting.Add("FlexWiki.Other1.OtherOneRefThree");
			expecting.Add("FlexWiki.Other1.OtherOneTopicOne");
			expecting.Add("FlexWiki.Other1.OtherOneTopicTwo");
			expecting.Add("FlexWiki.Other1.OtherOneTopicThree");
			expecting.Add("FlexWiki.Other1.OtherOneTopicFour");
			expecting.Add("FlexWiki.Other1.OtherOneTopicFive");
			expecting.Add("FlexWiki.Other1.OtherOneTopicSix");
			foreach (string backing in _other1.BackingTopics.Keys)
			{
				expecting.Add(_other1.Namespace + "." + backing);
			}			

			expecting.Add("Other2._ContentBaseDefinition");
			expecting.Add("Other2.OtherTwoHello");
			expecting.Add("Other2.OtherTwoGoodbye");
			foreach (string backing in _other2.BackingTopics.Keys)
			{
				expecting.Add(_other2.Namespace + "." + backing);
			}			


			foreach (AbsoluteTopicName topic in _base.AllTopics(true))
			{
				Assertion.Assert("Looking for " + topic.ToString(), expecting.Contains(topic.Fullname));
				expecting.Remove(topic.Fullname); 
			}
			Assertion.AssertEquals(expecting.Count, 0);
		}

		[Test] public void TopicsWithTest()
		{
			TheFederation.FederationCache = new MockFederationCache();

			Assert.AreEqual(1,_base.TopicsWith("Role","Developer").Count,"TopicsWith Role:Developer");
			Assert.AreEqual(1,_base.TopicsWith("Role","Designer").Count,"TopicsWith Role:Designer");
			Assert.AreEqual(2,_base.AllTopicsWith("Role","Developer").Count,"AllTopicsWith Role:Developer");
			Assert.AreEqual(1,_base.AllTopicsWith("Role","Designer").Count,"AllTopicsWith Role:Designer");
		}

		[Test] public void AllTopicsInfo()
		{
			Assert.AreEqual(26,_base.AllTopicsInfo.Count);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
			_other1.Delete();
			_other2.Delete();
			_other3.Delete();
			_cb5.Delete();
		}

		void CompareTopic(string topic, string outputMustContain)
		{	
			AbsoluteTopicName abs = _base.UnambiguousTopicNameFor(new RelativeTopicName(topic));
			string o = Formatter.FormattedTopic(abs, OutputFormat.HTML, false, TheFederation, _lm, null);
			string o1 = o.Replace("\r", "");
			string o2 = outputMustContain.Replace("\r", "");
			bool pass = o1.IndexOf(o2) >= 0;
			if (!pass)
			{
				Console.Error.WriteLine("Got     : " + o1);
				Console.Error.WriteLine("But Couldn't Find: " + o2);
			}
			Assertion.Assert(pass);
		}			

		[Test] public void ReferenceTopicInNonImportedNamespace()
		{
			CompareTopic("Space5.AbsRef", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("Other2.OtherTwoHello")) + @""">OtherTwoHello</a>");
		}

		[Test] public void DoubleHopImportTest()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Other1.OtherOneRefThree");
			CompareTopic("OtherOneRefThree", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("Other3.OtherThreeTest")) + @""">OtherThreeTest</a>");
		}

		[Test] public void BaseToForeignUnqualified()
		{
			CompareTopic("TopicOne", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("FlexWiki.Other1.OtherOneHello")) + @""">OtherOneHello</a>");
		}

		[Test] public void BaseToForeignQualified()
		{
			CompareTopic("TopicTwo", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("FlexWiki.Other1.OtherOneGoodbye")) + @""">OtherOneGoodbye</a>");
		}

		[Test] public void BaseToQualifiedAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Base.TopicThree");
			CompareTopic("TopicThree", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("No.Such.Namespace.FooBar")) + @""">FooBar</a>");
		}

		[Test] public void BaseToForcedLocal()
		{
			CompareTopic("TopicFour", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("FlexWiki.Base.TopicOne")) + @""">TopicOne</a>");
		}

		[Test] public void BaseToUnqualifiedAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Base.TopicFive");
			CompareTopic("TopicFive", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("FlexWiki.Base.FooBar")) + @""">FooBar</a>");
		}
		
		[Test] public void BaseToForcedLocalAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Base.TopicSix");
			CompareTopic("TopicSix", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("FlexWiki.Base.GooBar")) + @""">GooBar</a>");
		}

		[Test] public void ForeignToForeignUnqualified()
		{
			CompareTopic("OtherOneTopicOne", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("Other2.OtherTwoHello")) + @""">OtherTwoHello</a>");
		}

		[Test] public void ForeignToForeignQualified()
		{
			CompareTopic("OtherOneTopicTwo", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("Other2.OtherTwoGoodbye")) + @""">OtherTwoGoodbye</a>");
		}

		[Test] public void ForeignToQualifiedAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Other1.OtherOneTopicThree");
			CompareTopic("OtherOneTopicThree", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("No.Such.Namespace.FooBar")) + @""">FooBar</a>");
		}

		[Test] public void ForeignToForcedLocal()
		{
			CompareTopic("OtherOneTopicFour", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("FlexWiki.Other1.OtherOneTopicOne")) + @""">OtherOneTopicOne</a>");
		}

		[Test] public void ForeignToUnqualifiedAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Other1.OtherOneTopicFive");
			CompareTopic("OtherOneTopicFive", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("FlexWiki.Other1.FooBar")) + @""">FooBar</a>");
		}
		
		[Test] public void ForeignToForcedLocalAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Other1.OtherOneTopicSix");
			CompareTopic("OtherOneTopicSix", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("FlexWiki.Other1.GooBar")) + @""">GooBar</a>");
		}
	}


	[TestFixture] public class ContentBaseTests : WikiTests
	{
		ContentBase	_base;

		[SetUp] public void Init()
		{
			TheFederation = new Federation(OutputFormat.HTML, new LinkMaker("http://boobar"));
			string _tempPath = @"\temp-wikitests";
			string author = "tester-joebob";


			TheFederation.Register("FlexWiki.Base", _tempPath);
			_base = TheFederation.ContentBaseForRoot(_tempPath);
      _base.Namespace = "FlexWiki.Base"; 

			WriteTestTopicAndNewVersion(_base, _base.DefinitionTopicName.Name, @"Namespace: FlexWiki.Base", author);

			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello there", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello a", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello b", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello c", author);

			WriteTestTopicAndNewVersion(_base, "Versioned", "v1", "tester-bob");
			WriteTestTopicAndNewVersion(_base, "Versioned", "v2", "tester-sally");

			WriteTestTopicAndNewVersion(_base, "TopicTwo", @"Something about TopicOne and more!", author);
			WriteTestTopicAndNewVersion(_base, "Props", @"First: one
Second: two
Third:[ lots
and

lots
]
more stuff
", author);
			WriteTestTopicAndNewVersion(_base, "TopicOlder", @"write first", author);
			WriteTestTopicAndNewVersion(_base, "ExternalWikis", @"@wiki1=dozo$$$
@wiki2=fat$$$", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!

			// THIS ONE (TopicNewer) MUST BE WRITTEN LAST!!!!
			WriteTestTopicAndNewVersion(_base, "TopicNewer", @"write last", author);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
		}

		[Test] public void TestSerialization()
		{
			MemoryStream ms = new MemoryStream(); 
			XmlWriter wtr = new XmlTextWriter(ms, System.Text.Encoding.UTF8); 
			XmlSerializer ser = new XmlSerializer(typeof(ContentBase)); 
			ser.Serialize(wtr, _base); 

			wtr.Close(); 

			// If we got this far, there was no exception. More rigorous 
			// testing would assert XPath expressions against the XML 

		} 

		[Test] public void TestTopicNameSerialization()
		{
			AbsoluteTopicName name1 = ContentBase().TopicNameFor("TopicOne");

			MemoryStream ms = new MemoryStream(); 
			XmlWriter wtr = new XmlTextWriter(ms, System.Text.Encoding.UTF8); 
			XmlSerializer ser = new XmlSerializer(typeof(AbsoluteTopicName)); 
			ser.Serialize(wtr, name1); 
			wtr.Close(); 
		} 



		[Test] public void RenameTest()
		{
			AbsoluteTopicName name1 = ContentBase().TopicNameFor("TopicOne");
			AbsoluteTopicName name2 = ContentBase().TopicNameFor("TopicTwo");
			ArrayList log = ContentBase().RenameTopic(name1, "TopicOneRenamed", true);
			Assertion.AssertEquals(log.Count, 1);
			string t2 = ContentBase().Read(name2);
			Assertion.AssertEquals("Something about TopicOneRenamed and more!", t2);
		}

		[Test] public void RenameAndVerifyVersionsGetRenamedTest()
		{
			AbsoluteTopicName name1 = ContentBase().TopicNameFor("TopicOne");
			AbsoluteTopicName name2 = ContentBase().TopicNameFor("TopicOneRenamed");
			ContentBase().RenameTopic(name1, name2.Name, false);

			int c = 0;
			foreach (object x in ContentBase().AllVersionsForTopic(name2))
			{
				x.ToString();		// do something with x just to shut the compiler up
				c++;			
			}
			Assertion.AssertEquals(c, 4);	// should be 4 versions, even after rename
		}

		
		ContentBase ContentBase()
		{
			return _base;
		}

		[Test] public void SimpleVersioningTests()
		{
			ArrayList list = new ArrayList();
			foreach (AbsoluteTopicName s in ContentBase().AllVersionsForTopic(ContentBase().TopicNameFor("Versioned")))
			{
				list.Add(s);
			}
			Assertion.AssertEquals(list.Count, 2);
		}

		[Test] public void AllChangesForTopicSinceTests()
		{
			ArrayList list = new ArrayList();
			foreach (TopicChange c in ContentBase().AllChangesForTopicSince(ContentBase().TopicNameFor("Versioned"), DateTime.MinValue))
			{
				list.Add(c);
			}
			Assertion.AssertEquals(list.Count, 2);

			list = new ArrayList();
			foreach (string s in ContentBase().AllChangesForTopicSince(ContentBase().TopicNameFor("Versioned"), DateTime.MaxValue))
			{
				list.Add(s);
			}
			Assertion.AssertEquals(list.Count, 0);
		}

		[Test] public void EnsureContentBaseIsMadeIfAbsent()
		{
			string p = @"\temp-wikitests-creation";
			ContentBase b2 = TheFederation.ContentBaseForRoot(p);
			Assertion.Assert(Directory.Exists(p));
			b2.Delete();
		}

		[Test] public void TopicTimeTests()
		{
			Assertion.Assert(ContentBase().GetTopicCreationTime(ContentBase().TopicNameFor("TopicOlder")) < ContentBase().GetTopicCreationTime(ContentBase().TopicNameFor("TopicNewer")));
			Assertion.AssertEquals(ContentBase().GetTopicLastWriteTime(ContentBase().TopicNameFor("TopicNewer")), ContentBase().LastModified(true));
		}

		[Test] public void AuthorshipTests()
		{
			Assertion.AssertEquals(ContentBase().GetTopicLastAuthor(ContentBase().TopicNameFor("TopicNewer")), ContentBase().GetTopicLastAuthor(ContentBase().TopicNameFor("TopicOlder")));
		}

		[Test] public void ExternalWikisTests()
		{
			Hashtable t = ContentBase().ExternalWikiHash();
			Assertion.AssertEquals(t.Count, 2);
			Assertion.AssertEquals(t["wiki1"], "dozo$$$");
			Assertion.AssertEquals(t["wiki2"], "fat$$$");
		}

		[Test] public void GetFieldsTest()
		{
			Hashtable t = ContentBase().GetFieldsForTopic(ContentBase().TopicNameFor("Props"));
			Assertion.AssertEquals(t["First"], "one");
			Assertion.AssertEquals(t["Second"], "two");
			Assertion.AssertEquals(t["Third"], @"lots
and

lots");
		}

		[Test] public void BasicEnumTest()
		{
			ArrayList expecting = new ArrayList();
			expecting.Add("TopicOne");
			expecting.Add("TopicTwo");
			expecting.Add(_base.DefinitionTopicName.Name);
			expecting.Add("Props");
			expecting.Add("ExternalWikis");
			expecting.Add("TopicOlder");
			expecting.Add("Versioned");
			expecting.Add("TopicNewer");
			foreach (string backing in ContentBase().BackingTopics.Keys)
			{
				expecting.Add(backing);
			}			

			foreach (AbsoluteTopicName topic in ContentBase().AllTopics(false))
			{
				Assertion.Assert("Looking for " + topic.Name, expecting.Contains(topic.Name));
				expecting.Remove(topic.Name);
			}
			Assertion.AssertEquals(expecting.Count, 0);
		}

		
		[Test] public void SetFieldsTest()
		{
			ContentBase cb = ContentBase();
			string author = "joe_author";
			AbsoluteTopicName wn = WriteTestTopicAndNewVersion(cb, "FieldsTesting", "", author);
			
			Hashtable t;
			
			t = ContentBase().GetFieldsForTopic(wn);
			
			cb.SetFieldValue(wn, "First", "one", false);
			t = ContentBase().GetFieldsForTopic(wn);
			Assertion.AssertEquals(t["First"], "one");

			cb.SetFieldValue(wn, "Second", "two", false);
			t = ContentBase().GetFieldsForTopic(wn);
			Assertion.AssertEquals(t["First"], "one");
			Assertion.AssertEquals(t["Second"], "two");
			
			cb.SetFieldValue(wn, "Second", "change", false);
			t = ContentBase().GetFieldsForTopic(wn);
			Assertion.AssertEquals(t["First"], "one");
			Assertion.AssertEquals(t["Second"], "change");

			cb.SetFieldValue(wn, "First", @"change
is
good", false);
			t = ContentBase().GetFieldsForTopic(wn);
			Assertion.AssertEquals(t["First"], @"change
is
good");
			Assertion.AssertEquals(t["Second"], "change");

			cb.SetFieldValue(wn, "First", "one", false);
			cb.SetFieldValue(wn, "Second", "change", false);
			t = ContentBase().GetFieldsForTopic(wn);
			Assertion.AssertEquals(t["First"], "one");
			Assertion.AssertEquals(t["Second"], "change");
		}

		[Test] public void SimpleReadingAndWritingTest()
		{
			AbsoluteTopicName an = ContentBase().TopicNameFor("SimpleReadingAndWritingTest");
			string c = @"Hello
There";
			ContentBase().WriteTopic(an, c);
			string ret;
			using (TextReader sr = ContentBase().TextReaderForTopic(an))
			{
				ret = sr.ReadToEnd();
			}
			Assertion.AssertEquals(c, ret);

			ContentBase().DeleteTopic(an);
			Assertion.Assert(!ContentBase().TopicExists(an));
		}



		[Test] public void SimpleTopicExistsTest()
		{
			Assertion.Assert(ContentBase().TopicExists(ContentBase().TopicNameFor("TopicOne")));
		}

    [Test] public void LatestVersionForTopic()
    {
      string author = "LatestVersionForTopicTest"; 
      WriteTestTopicAndNewVersion(_base, "TopicOne", @"A Change", author);
      WriteTestTopicAndNewVersion(_base, "TopicTwo", @"A Change", author);
      WriteTestTopicAndNewVersion(_base, "TopicThree", @"A Change", author);

      ContentBase cb = ContentBase(); 
      
      AbsoluteTopicName atn1 = cb.TopicNameFor("TopicOne"); 
      AbsoluteTopicName atn2 = cb.TopicNameFor("TopicTwo"); 
      AbsoluteTopicName atn3 = cb.TopicNameFor("TopicThree"); 

      IEnumerable versions1 = cb.AllVersionsForTopic(atn1); 
      IEnumerable versions2 = cb.AllVersionsForTopic(atn2); 
      IEnumerable versions3 = cb.AllVersionsForTopic(atn3); 

      string version1 = null; 
      string version2 = null; 
      string version3 = null; 

      foreach (AbsoluteTopicName atn in versions1)
      {
        version1 = atn.Version; 
        break;
      }
      foreach (AbsoluteTopicName atn in versions2)
      {
        version2 = atn.Version; 
        break;
      }
      foreach (AbsoluteTopicName atn in versions3)
      {
        version3 = atn.Version; 
        break;
      }

      Assertion.AssertEquals("Checking that latest version is calculated correctly", 
        version1, cb.LatestVersionForTopic(atn1)); 
      Assertion.AssertEquals("Checking that latest version is calculated correctly", 
        version2, cb.LatestVersionForTopic(atn2)); 
      Assertion.AssertEquals("Checking that latest version is calculated correctly", 
        version3, cb.LatestVersionForTopic(atn3)); 

    }


	}

	[TestFixture] public class InfiniteRecursionTests : WikiTests
	{
		ContentBase	_base, _imp1, _imp2;

		[SetUp] public void Init()
		{
			TheFederation = new Federation(OutputFormat.HTML, new LinkMaker("http://boobar"));
			_base = TheFederation.ContentBaseForRoot(@"\temp-wikitests");
			_imp1 = TheFederation.ContentBaseForRoot(@"\temp-wikitests-imp1");
			_imp2 = TheFederation.ContentBaseForRoot(@"\temp-wikitests-imp2");
			TheFederation.Register("FlexWiki.Projects.Wiki", _base.Root);
      _base.Namespace = "FlexWiki.Projects.Wiki"; 
			TheFederation.Register("FlexWiki.Projects.Wiki1", _imp1.Root);
      _imp1.Namespace = "FlexWiki.Projects.Wiki1"; 
			TheFederation.Register("FlexWiki.Projects.Wiki2", _imp2.Root);
      _imp2.Namespace = "FlexWiki.Projects.Wiki2"; 

			string author = "tester-joebob";
			WriteTestTopicAndNewVersion(_base, _base.DefinitionTopicName.Name, @"

Namespace: FlexWiki.Projects.Wiki
Description: Test description
Import: FlexWiki.Projects.Wiki1", author);

			WriteTestTopicAndNewVersion(_imp1, _imp1.DefinitionTopicName.Name, @"

Namespace: FlexWiki.Projects.Wiki1
Description: Test1 description
Import: FlexWiki.Projects.Wiki2", author);

			WriteTestTopicAndNewVersion(_imp2, _imp2.DefinitionTopicName.Name, @"

Namespace: FlexWiki.Projects.Wiki2
Description: Test1 description
Import: FlexWiki.Projects.Wiki", author);

		}

		[Test] public void TestRecurse()
		{
			Assertion.AssertEquals("FlexWiki.Projects.Wiki", _base.Namespace);
			Assertion.AssertEquals("FlexWiki.Projects.Wiki1", _imp1.Namespace);
			Assertion.AssertEquals("FlexWiki.Projects.Wiki2", _imp2.Namespace);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
			_imp1.Delete();
			_imp2.Delete();
		}
		
	}

	[TestFixture] public class MoreContentBaseTests : WikiTests
	{
		ContentBase	_base, _imp1, _imp2;

		[SetUp] public void Init()
		{
			TheFederation = new Federation(OutputFormat.HTML, new LinkMaker("http://boobar"));
			_base = TheFederation.ContentBaseForRoot(@"\temp-wikitests");
			_imp1 = TheFederation.ContentBaseForRoot(@"\temp-wikitests-imp1");
			_imp2 = TheFederation.ContentBaseForRoot(@"\temp-wikitests-imp2");

			TheFederation.Register("FlexWiki.Projects.Wiki", _base.Root);
      _base.Namespace = "FlexWiki.Projects.Wiki";
			TheFederation.Register("FlexWiki.Projects.Wiki1", _imp1.Root);
      _imp1.Namespace = "FlexWiki.Projects.Wiki1";
			TheFederation.Register("FlexWiki.Projects.Wiki2", _imp2.Root);
      _imp2.Namespace = "FlexWiki.Projects.Wiki2";

			string author = "tester-joebob";
			WriteTestTopicAndNewVersion(_base, _base.DefinitionTopicName.Name, @"

Namespace: FlexWiki.Projects.Wiki
Description: Test description
Import: FlexWiki.Projects.Wiki1, FlexWiki.Projects.Wiki2", author);

		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
			_imp1.Delete();
			_imp2.Delete();
		}
		
		ContentBase Base
		{
			get
			{
				return _base;
			}
		}

		[Test] public void SimpleReadingTest()
		{
			Assertion.AssertEquals("FlexWiki.Projects.Wiki", Base.Namespace);
			Assertion.AssertEquals("Test description", Base.Description);
			ArrayList rels = new ArrayList();
			rels.Add(_imp1.Root);
			rels.Add(_imp2.Root);
			foreach (ContentBase each in  Base.ImportedContentBases)
			{
				Assertion.Assert(rels.Contains(each.Root));
				rels.Remove(each.Root);
			}
			Assertion.AssertEquals(0, rels.Count);					
		}
	}
	[TestFixture] public class ExtractTests : WikiTests
	{
		string full = "HomePage(2003-11-24-20-31-20-WINGROUP-davidorn)";

		[Test] public void VersionTest()
		{
			Assertion.AssertEquals("2003-11-24-20-31-20-WINGROUP-davidorn", FlexWiki.ContentBase.ExtractVersionFromHistoricalFilename(full));
		}

		[Test] public void NameTest()
		{
			Assertion.AssertEquals("HomePage", FlexWiki.ContentBase.ExtractTopicFromHistoricalFilename(full));
		}			
	}

}
