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
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Reflection; 

using FlexWiki.Formatting;

using NUnit.Framework;


namespace FlexWiki.UnitTests
{
	[TestFixture] public class TableFormattingOptionsTests 
	{
		public void TestDefaults()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("");
			Assertion.Assert(info.HasBorder);
			Assertion.Assert(!info.IsHighlighted);
		}

		public void TestError()
		{
			TableCellInfo info = new TableCellInfo();
			Assertion.Assert(info.Parse("T%") != null);
		}


		public void TestTable()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("T-T]");
			Assertion.Assert(!info.HasBorder);
			Assertion.AssertEquals(info.TableAlignment, TableCellInfo.AlignOption.Right);
			info.Parse("T[");
			Assertion.AssertEquals(info.TableAlignment, TableCellInfo.AlignOption.Left);
			info.Parse("T^");
			Assertion.AssertEquals(info.TableAlignment, TableCellInfo.AlignOption.Center);
		}

		public void TestCellAlignment()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("]");
			Assertion.AssertEquals(info.CellAlignment, TableCellInfo.AlignOption.Right);
			info.Parse("[");
			Assertion.AssertEquals(info.CellAlignment, TableCellInfo.AlignOption.Left);
			info.Parse("^");
			Assertion.AssertEquals(info.CellAlignment, TableCellInfo.AlignOption.Center);
		}

		public void TestHighlight()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("!");
			Assertion.Assert(info.IsHighlighted);
		}

		public void TestSpans()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("C10!R5T-");
			Assertion.Assert(info.IsHighlighted);
			Assertion.AssertEquals(info.ColSpan, 10);
			Assertion.AssertEquals(info.RowSpan, 5);
		}

	}

	
	
	[TestFixture] public class CachingTests : WikiTests
	{
		ContentBase _cb, _cb2;
		const string _base = "http://boo/";
		LinkMaker _lm;
		string user = "joe";

		[SetUp] public void Init()
		{
			_lm = new LinkMaker(_base);
			TheFederation = new Federation(OutputFormat.HTML, _lm);
			TheFederation.WikiTalkVersion = 1;
			string root = Path.GetFullPath("temp-wikibasex");
			string root2 = Path.GetFullPath("temp-wikibasex2");
			string ns = "FlexWiki";
			string ns2 = "FlexWiki2";
			TheFederation.Register(ns, root);		
			TheFederation.Register(ns2, root2);		
			_cb = TheFederation.ContentBaseForRoot(root);
			_cb.Namespace = ns;

			_cb2 = TheFederation.ContentBaseForRoot(root2);
			_cb2.Namespace = ns2;

			WriteTestTopicAndNewVersion(_cb, _cb.DefinitionTopicName.Name, @"Import: FlexWiki2
Namespace: " + ns + @"
", user);
			WriteTestTopicAndNewVersion(_cb2, _cb2.DefinitionTopicName.Name, @"
Namespace: " + ns2 + @"
", user);
			WriteTestTopicAndNewVersion(_cb, "HomePage", "This is a simple topic RefOne plus PluralWords reference to wiki://PresentIncluder wiki://TestLibrary/foo.gif", user);
			WriteTestTopicAndNewVersion(_cb2, "AbsentIncluder", "{{NoSuchTopic}}", user);
			WriteTestTopicAndNewVersion(_cb2, "PresentIncluder", "{{IncludePresent}}", user);
			WriteTestTopicAndNewVersion(_cb2, "IncludePresent", "hey! this is ReferencedFromIncludePresent", user);
			WriteTestTopicAndNewVersion(_cb2, "TestLibrary", "URI: whatever", user);
		}

		[Test] public void TestIncludeForPresentTopic()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki2.PresentIncluder");
			AssertFileRule(rule, "IncludePresent.wiki");
			AssertFileRule(rule, "ReferencedFromIncludePresent.wiki");
		}

		[Test] public void TestIncludeForAbsentTopic()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki2.AbsentIncluder");
			AssertFileRule(rule, "NoSuchTopic.wiki");
		}

		[Test] public void TestWikiURLResourceLibraryCaching()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki.HomePage");
			AssertFileRule(rule, "TestLibrary.wiki");
		}


		[Test] public void TestWikiURLTopicCaching()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki.HomePage");
			AssertFileRule(rule, "PresentIncluder.wiki");
		}

		[Test] public void TestBasicTopicCaching()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki.HomePage");
			AssertFileRule(rule, "HomePage.wiki");
			AssertFileRule(rule, _cb.DefinitionTopicName.Name);	// getting the whole path is not convenient, but this should be enough
		}

		[Test] public void TestBasicTopicCachingAllPossibleVersions()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki.HomePage");
			Assertion.AssertEquals("Want 2 refs to RefOne", 2, CountFileRuleMatches(rule, "RefOne.wiki"));	// We should see two cache rules, one for each of the two namespaces in which the referenced topic could appear
			Assertion.AssertEquals("Want 2 refs to PluralWord", 2, CountFileRuleMatches(rule, "PluralWord.wiki"));	// We should see two refs to this (because it's a plural form)
			Assertion.AssertEquals("Want 2 refs to PluralWords", 2, CountFileRuleMatches(rule, "PluralWords.wiki"));	// We should see two refs to this (because it's the natural form)
		}

		[TearDown] public void Deinit()
		{
			_cb.Delete();
			_cb2.Delete();
		}

		void AssertFileRule(CacheRule rule, string path)
		{
			int count = CountFileRuleMatches(rule, path);
			if (count == 0)
				System.Console.Error.WriteLine("Rule: " + rule.Description);
			Assertion.Assert("Searching for path (" + path + ") in cache rule", count > 0); 
		}

		int CountFileRuleMatches(CacheRule rule, string path)
		{
			int found = 0;
			foreach (CacheRule r in rule.AllLeafRules)
			{
				if (r is FilesCacheRule)
				{
					FilesCacheRule fcr = (FilesCacheRule)r;
					foreach (string p in fcr.Files)
					{
						if (p.IndexOf(path) >= 0)
						{
							found++;
						}
					}
				}
			}
			return found;
		}

		CacheRule GetCacheRuleForTopic(string topic)
		{
			AbsoluteTopicName tn = new AbsoluteTopicName(topic);
			CompositeCacheRule rule = new CompositeCacheRule();
			Formatter.FormattedTopic(tn, OutputFormat.Testing, false, TheFederation, _lm, rule);
			return rule;
		}

	}

	[TestFixture] public class FormattingTests : WikiTests
	{
		ContentBase _cb;
		const string _base = "http://boo/";
		Hashtable _externals;
		LinkMaker _lm;
		string user = "joe";

    // At runtime we dump the contents of the embedded resources to a directory so 
    // we don't have to rely on an Internet connection or a hardcoded path to 
    // retrieve the XML when testing behaviors that pull it in. These variables hold
    // the paths to the XML that we write to disk at Init time. 
    string testRssXmlPath; 
    string testRssXslPath; 
    string meerkatRssPath; 

		[SetUp] public void Init()
		{
      // Dump the contents of the embedded resources to a file so we can read them 
      // in during the tests. 
      meerkatRssPath = Path.GetFullPath("meerkat.rss.xml"); 
      testRssXmlPath = Path.GetFullPath("rsstest.xml"); 
      testRssXslPath = Path.GetFullPath("rsstest.xsl"); 
      Assembly a = Assembly.GetExecutingAssembly(); 
      WriteResourceToFile(a, "FlexWiki.UnitTests.TestContent.meerkat.rss.xml", meerkatRssPath); 
      WriteResourceToFile(a, "FlexWiki.UnitTests.TestContent.rsstest.xml", testRssXmlPath); 
      WriteResourceToFile(a, "FlexWiki.UnitTests.TestContent.rsstest.xsl", testRssXslPath); 

			_lm = new LinkMaker(_base);
			TheFederation = new Federation(OutputFormat.HTML, _lm);
			TheFederation.WikiTalkVersion = 1;
			string root = Path.GetFullPath("temp-wikibasex");
			string ns = "FlexWiki";
			TheFederation.Register(ns, root);		
			_cb = TheFederation.ContentBaseForRoot(root);
			_cb.Namespace = ns;
			_cb.Title  = "Friendly Title";

			WriteTestTopicAndNewVersion(_cb, "HomePage", "Home is where the heart is", user);
			WriteTestTopicAndNewVersion(_cb, "BigPolicy", "This is ", user);
			WriteTestTopicAndNewVersion(_cb, "BigDog", "This is ", user);
			WriteTestTopicAndNewVersion(_cb, "BigAddress", "This is ", user);
			WriteTestTopicAndNewVersion(_cb, "BigBox", "This is ", user);
			WriteTestTopicAndNewVersion(_cb, "IncludeOne", "inc1", user);
			WriteTestTopicAndNewVersion(_cb, "IncludeTwo", "!inc2", user);
			WriteTestTopicAndNewVersion(_cb, "IncludeThree", "!!inc3", user);
			WriteTestTopicAndNewVersion(_cb, "IncludeFour", "!inc4", user);
			WriteTestTopicAndNewVersion(_cb, "IncludeNest", @"		{{IncludeNest1}}
			{{IncludeNest2}}", user);
			WriteTestTopicAndNewVersion(_cb, "TopicWithColor", "Color: Yellow", user);
			WriteTestTopicAndNewVersion(_cb, "IncludeNest1", "!hey there", user);
			WriteTestTopicAndNewVersion(_cb, "IncludeNest2", "!black dog", user);
			WriteTestTopicAndNewVersion(_cb, "IncludeNestURI", @"wiki://IncludeNest1 wiki://IncludeNest2 ", user);
			WriteTestTopicAndNewVersion(_cb, "ResourceReference", @"URI: http://www.google.com/$$$", user);
			WriteTestTopicAndNewVersion(_cb, "FlexWiki", "flex ", user);
			WriteTestTopicAndNewVersion(_cb, "OneMinuteWiki", "one ", user);
			WriteTestTopicAndNewVersion(_cb, "TestIncludesBehaviors", "@@ProductName@@ somthing @@Now@@ then @@Now@@", user);
			WriteTestTopicAndNewVersion(_cb, "TopicWithBehaviorProperties", @"
Face: {""hello"".Length}
one 
FaceWithArg: {arg | arg.Length }
FaceSpanningLines:{ arg |

arg.Length 

}

", user);
			WriteTestTopicAndNewVersion(_cb, "TestTopicWithBehaviorProperties", @"
len=@@topics.TopicWithBehaviorProperties.Face@@
lenWith=@@topics.TopicWithBehaviorProperties.FaceWithArg(""superb"")@@
lenSpanning=@@topics.TopicWithBehaviorProperties.FaceSpanningLines(""parsing is wonderful"")@@
", user);


			_externals = new Hashtable();
		}

		[Test] public void TopicBehaviorProperty()
		{
			FormattedTopicContainsTest(new AbsoluteTopicName("TestTopicWithBehaviorProperties", _cb.Namespace), "len=5");
			FormattedTopicContainsTest(new AbsoluteTopicName("TestTopicWithBehaviorProperties", _cb.Namespace), "lenWith=6");
			FormattedTopicContainsTest(new AbsoluteTopicName("TestTopicWithBehaviorProperties", _cb.Namespace), "lenSpanning=20");
		}

		[Test] public void TestAllNamespacesBehavior()
		{
			FormatTestContains("@@AllNamespacesWithDetails@@", "FlexWiki");
			FormatTestContains("@@AllNamespacesWithDetails@@", "Friendly Title");
		}

		[TearDown] public void Deinit()
		{
			_cb.Delete();
		}

		[Test] public void NamespaceAsTopicPreceedsQualifiedNames()
		{
			string s = FormattedTestText(@"FlexWiki bad FlexWiki.OneMinuteWiki");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("FlexWiki")) + @""">FlexWiki</a> bad");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("OneMinuteWiki")) + @""">OneMinuteWiki</a>");
		}

		void AssertStringContains(string container, string find)
		{
			Assertion.Assert("Searching for " + find + " in " + container, container.IndexOf(find) != -1);
		}

		[Test] public void WikiURIForTopic()
		{
			string s = FormattedTestText("wiki://IncludeNestURI");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("IncludeNestURI")) + @""">IncludeNestURI</a>");
		} 

		[Test] public void WikiURIForResource()
		{
			FormatTest(@"wiki://ResourceReference/Cookies", @"<p><a href=""http://www.google.com/Cookies"">http://www.google.com/Cookies</a></p>
");
		}
		
		[Test] public void WikiURIForImageResource()
		{
			FormatTest(@"wiki://ResourceReference/Cookies.gif", @"<p><img src=""http://www.google.com/Cookies.gif""></p>
");
		}

		[Test] public void PropertyBehavior()
		{
			FormatTest(@"@@Property(""TopicWithColor"", ""Color"")@@", @"<p>Yellow</p>
");
		}

		[Test] public void WikiURIForTopicProperty()
		{
			FormatTest(@"wiki://TopicWithColor/#Color", @"<p>Yellow</p>
");
		}

		[Test] public void NestedWikiSpec()
		{
			FormatTest(@"		{{IncludeNest}}", @"<h5>hey there</h5>
<h6>black dog</h6>
");
		}



		[Test] public void IncludeFailure()
		{
			FormatTest(@"{{NoSuchTopic}}", 
				@"<p>{{<a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("NoSuchTopic")) + @""">NoSuchTopic</a>}}</p>
");

		}

		[Test] public void SimpleWikiSpec()
		{
			FormatTest(@"!Head1
	{{IncludeOne}}
	{{IncludeTwo}}
		{{IncludeThree}}
	{{IncludeFour}}
", @"<h1>Head1</h1>

<p>inc1</p>
<h2>inc2</h2>
<h4>inc3</h4>
<h2>inc4</h2>
");
		}

		[Test] public void PlainInOut()
		{
			FormatTest("Hello there", "<p>Hello there</p>\n");
		}

		[Test] public void RelabelTests()
		{
			string s = FormattedTestText(@"""tell me about dogs"":BigDog");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigDog")) + @""">tell me about dogs</a>");
		}

		[Test] public void TopicNameRegexTests()
		{
			ShouldBeTopicName("AaA");
			ShouldBeTopicName("AAa");
			ShouldBeTopicName("AaAA");
			ShouldBeTopicName("AaAa");
			ShouldBeTopicName("ZAAbAA");
			ShouldBeTopicName("ZAAb");
			ShouldBeTopicName("ZaAA");
			ShouldBeTopicName("CSharp");
			ShouldBeTopicName("Meeting25Dec");
			ShouldBeTopicName("KeyOfficeIRMFunctionality");
			ShouldBeTopicName("IBMMainframe");

			ShouldNotBeTopicName("AAA");
			ShouldNotBeTopicName("AA");
			ShouldNotBeTopicName("A42");
			ShouldNotBeTopicName("A");
			ShouldNotBeTopicName("a");
			ShouldNotBeTopicName("about");
			ShouldNotBeTopicName("Hello");
		}

		[Test] public void NonAsciiTopicNameRegexTest() 
		{
			ShouldBeTopicName("DistribuciónTécnica");
			ShouldBeTopicName("ProblemasProgramación");
			ShouldBeTopicName("SmørgåsBord");
			ShouldBeTopicName("ØrneRede");
			ShouldBeTopicName("HöchstMaß");
			ShouldBeTopicName("FaçadePattern");
			ShouldBeTopicName("ReykjavíkCity");

			// string russian = "\u1044\u1086\u1093\u1083\u1072\u1103\u1056\u1099\u1073\u1072";
			// ShouldBeTopicName(russian);

			ShouldNotBeTopicName("æøå");
			ShouldNotBeTopicName("ÆØÅ");
			ShouldNotBeTopicName("Ølle");
			ShouldNotBeTopicName("ølle");
		}

		void ShouldBeTopicName(string s)
		{
			FormatTest(
				@s,
				@"<p><a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor(s)) + @""">" + s + @"</a></p>
");
		}

		void ShouldNotBeTopicName(string s)
		{
			FormatTest(
				@s,
				@"<p>" + s + @"</p>
");
		}

//  commented out - david ornstein; 2/24/2004 need to figure out a better way to make this work that doesn't cause massive strikethrough bug
//		[Test] public void FormattedLinkTests()
//		{
//			FormatTestContains(
//				@"''BigBox''",
//				@"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigBox")) + @""">BigBox</a>");
//		}


		[Test] public void SimpleLinkTests()
		{
			FormatTest(
				@"Some VerySimpleLinksFatPig
StartOfLineShouldLink blah blah blah
blah blah EndOfLineShouldLink",
				@"<p>Some <a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("VerySimpleLinksFatPig")) + @""">VerySimpleLinksFatPig</a></p>
<p><a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("StartOfLineShouldLink")) + @""">StartOfLineShouldLink</a> blah blah blah</p>
<p>blah blah <a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("EndOfLineShouldLink")) + @""">EndOfLineShouldLink</a></p>
");
		}

		[Test] public void SimpleEscapeTest()
		{
			FormatTest(@"Hello """" World", @"<p>Hello """" World</p>
");
			FormatTest(@"Hello """"World""""", @"<p>Hello World</p>
");
			FormatTest(@"Hello """"WouldBeLink""""", @"<p>Hello WouldBeLink</p>
");
			FormatTest(@"""""WouldBeLink""""", @"<p>WouldBeLink</p>
");
			FormatTest(@"""""WouldBeLink"""" and more ''ital''", @"<p>WouldBeLink and more <em>ital</em></p>
");
			FormatTest(@"""""WouldBeLink and more ''ital''""""", @"<p>WouldBeLink and more ''ital''</p>
");
			FormatTest(@"""""WouldBeLink"""" and """"----"""" line", @"<p>WouldBeLink and ---- line</p>
");
			FormatTest(@"""""", @"<p>""""</p>
");
			FormatTest(@"("""""""""""")", @"<p>("""")</p>
");
			FormatTest(@"("""")", @"<p>("""")</p>
");
		}

		[Test] public void LinkAfterBangTests()
		{
			FormatTest(
				@"!HelloWorld",
				@"<h1><a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("HelloWorld")) + @""">HelloWorld</a></h1>

");
		}

		[Test] public void PluralLinkTests()
		{
			FormatTestContains(
				@"BigDogs",
				@"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigDog")) + @""">BigDogs</a>");
			FormatTestContains(
				@"BigPolicies",
				@"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigPolicy")) + @""">BigPolicies</a>");
			FormatTestContains(
				@"BigAddresses",
				@"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigAddress")) + @""">BigAddresses</a>");
			FormatTestContains(
				@"BigBoxes",
				@"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigBox")) + @""">BigBoxes</a>");

			// Test for plural before singular
			string s = FormattedTestText(@"See what happens when I mention BigBoxes; the topic is called BigBox.");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigBox")) + @""">BigBoxes</a>");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigBox")) + @""">BigBox</a>");
		}

		[Test] public void Rule()
		{
			FormatTestContains("----", "<div class='Rule'></div>");
			FormatTestContains("------------------------", "<div class='Rule'></div>");
		}

		[Test] public void HeadingTests()
		{
			FormatTest("!Hey Dog", @"<h1>Hey Dog</h1>

");
			FormatTest("!!Hey Dog", @"<h2>Hey Dog</h2>

");
			FormatTest("!!!Hey Dog", @"<h3>Hey Dog</h3>

");
			FormatTest("!!!!Hey Dog", @"<h4>Hey Dog</h4>

");
			FormatTest("!!!!!Hey Dog", @"<h5>Hey Dog</h5>

");
			FormatTest("!!!!!!Hey Dog", @"<h6>Hey Dog</h6>

");
			FormatTest("!!!!!!!Hey Dog", @"<h7>Hey Dog</h7>

");
		}


		[Test] public void ListTests()
		{
			FormatTest(
				@"        1. item 1
        1. item 2",
				@"<ol>
<li> item 1</li>

<li> item 2</li>

</ol>
");	
            
			FormatTest(
				@"	* level 1
		* level 2
		* level 2
	* level 1
	* level 1 (again!)",
				@"<ul>
<li> level 1</li>

<ul>
<li> level 2</li>

<li> level 2</li>

</ul>
<li> level 1</li>

<li> level 1 (again!)</li>

</ul>
");
		}

		[Test] public void UnderEmphasis()
		{
			FormatTest(
				@"This should be _emphasised_ however, id_Foo and id_Bar should not be.",
				@"<p>This should be <em>emphasised</em> however, id_Foo and id_Bar should not be.</p>
");
		}

		[Test] public void InlineExternalReference()
		{
			FormatTest(
				@"@baf=http://www.baf.com/$$$
Again, TestIt@baf should be an external link along with TestItAgain@baf, however @this should be code formatted@.",
				@"<p>Again, <a class=ExternalLink title=""External link to baf"" target=""ExternalLinks"" href=""http://www.baf.com/TestIt"">TestIt</a> should be an external link along with <a class=ExternalLink title=""External link to baf"" target=""ExternalLinks"" href=""http://www.baf.com/TestItAgain"">TestItAgain</a>, however <code>this should be code formatted</code>.</p>
");
			FormatTest(
				@"@google=http://www.google.com/search?hl=en&ie=UTF-8&oe=UTF-8&q=$$$
ExternalTopic@google - verify the casing is correct.",
				@"<p><a class=ExternalLink title=""External link to google"" target=""ExternalLinks"" href=""http://www.google.com/search?hl=en&ie=UTF-8&oe=UTF-8&q=ExternalTopic"">ExternalTopic</a> - verify the casing is correct.</p>
");
			FormatTest(
				@"@baf=http://www.baf.com/$$$
Let's test one that comes at the end of a sentence, such as EOSTest@baf.",
				@"<p>Let's test one that comes at the end of a sentence, such as <a class=ExternalLink title=""External link to baf"" target=""ExternalLinks"" href=""http://www.baf.com/EOSTest"">EOSTest</a>.</p>
");
			FormatTest(
				@"@baf=http://www.baf.com/$$$
Test for case-insensitivity, such as CAPS@BAF, or some such nonsense.",
				@"<p>Test for case-insensitivity, such as <a class=ExternalLink title=""External link to BAF"" target=""ExternalLinks"" href=""http://www.baf.com/CAPS"">CAPS</a>, or some such nonsense.</p>
");
		}

		[Test] public void MailToLink()
		{
			FormatTest(
				@"Please send mailto:person@domain.com some email!",
				@"<p>Please send <a href=""mailto:person@domain.com"">mailto:person@domain.com</a> some email!</p>
");

			FormatTest(
				@"Please send ""person"":mailto:person@domain.com some email!",
				@"<p>Please send <a href=""mailto:person@domain.com"" title="""">person</a> some email!</p>
");
		}

		[Test] public void BasicWikinames()
		{
			FormatTest(
				@"LinkThis, AndLinkThis, dontLinkThis, (LinkThis), _LinkAndEmphasisThis_ *LinkAndBold* (LinkThisOneToo)",
				@"<p><a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("LinkThis")) + @""">LinkThis</a>, <a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("AndLinkThis")) + @""">AndLinkThis</a>, dontLinkThis, (<a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("LinkThis")) + @""">LinkThis</a>), <em>LinkAndEmphasisThis</em> <strong>LinkAndBold</strong> (<a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("LinkThisOneToo")) + @""">LinkThisOneToo</a>)</p>
");
		}

		[Test] public void PreNoformatting()
		{
			FormatTest(
				@" :-) bold '''not''' BigDog    _foo_",
				@"<pre>
 :-) bold '''not''' BigDog    _foo_
</pre>
");			
		}

		[Test] public void TableTests()
		{
			FormatTest(
				@"||",
				@"<p>||</p>
");

			FormatTest(
				@"||t1||",
				@"<table cellpadding='2' cellspacing='1' class='TableClass'>
<tr>
<td  class='TableCell'>t1</td>
</tr>
</table>
");
			
			FormatTest(
				@"not a table||",
				@"<p>not a table||</p>
");
			
			FormatTest(
				@"||not a table",
				@"<p>||not a table</p>
");

			FormatTest(
				@"||''table''||'''more'''||columns||
||1||2||3||
",
				@"<table cellpadding='2' cellspacing='1' class='TableClass'>
<tr>
<td  class='TableCell'><em>table</em></td>
<td  class='TableCell'><strong>more</strong></td>
<td  class='TableCell'>columns</td>
</tr>
<tr>
<td  class='TableCell'>1</td>
<td  class='TableCell'>2</td>
<td  class='TableCell'>3</td>
</tr>
</table>
");
		}

		[Test] public void EmoticonInTableTest()
		{
			FormatTest(
				@"||:-)||",
				@"<table cellpadding='2' cellspacing='1' class='TableClass'>
<tr>
<td  class='TableCell'><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""></td>
</tr>
</table>
");
		}

		[Test] public void WikinameInTableTest()
		{
			string s = FormattedTestText(@"||BigDog||");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigDog")) + @""">BigDog</a>");
		}

		[Test] public void HyperlinkInTableTest()
		{
			FormatTest(
				@"||http://www.yahoo.com/foo.html||",
				@"<table cellpadding='2' cellspacing='1' class='TableClass'>
<tr>
<td  class='TableCell'><a href=""http://www.yahoo.com/foo.html"">http://www.yahoo.com/foo.html</a></td>
</tr>
</table>
");
		}

		[Test] public void EmoticonTest()
		{
			FormatTest(
				@":-) :-(",
				@"<p><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""> <img src=""" + _lm.LinkToImage("emoticons/sad_smile.gif") + @"""></p>
");
		}



		[Test] public void BracketedLinks()
		{
			string s = FormattedTestText(@"[BigBox] summer [eDocuments] and [e] and [HelloWorld] and [aZero123]");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigBox")) + @""">BigBox</a>");
			AssertStringContains(s, @"<a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("eDocuments")) + @""">eDocuments</a> and <a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("e")) + @""">e</a> and <a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("HelloWorld")) + @""">HelloWorld</a> and <a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("aZero123")) + @""">aZero123</a>");
		}

		[Test] public void CodeFormatting()
		{
			FormatTest(
				@"These values should be code formatted, not external Wiki references @IObjectWithSite@ and @IViewObject@. This should also be (@code formatted@) inside of the parens, and @PropertyManager.RegisterProperty@ should work...",
				@"<p>These values should be code formatted, not external Wiki references <code>IObjectWithSite</code> and <code>IViewObject</code>. This should also be (<code>code formatted</code>) inside of the parens, and <code>PropertyManager.RegisterProperty</code> should work...</p>
");
			FormatTest(
				@"The text in the parens and brackets should be bold:
(*hello*) [*world*] [*hello world*]
And the text in the parens and brackets should be code formatted:
(@hello@) [@world@] [@hello world@]",
				@"<p>The text in the parens and brackets should be bold:</p>
<p>(<strong>hello</strong>) [<strong>world</strong>] [<strong>hello world</strong>]</p>
<p>And the text in the parens and brackets should be code formatted:</p>
<p>(<code>hello</code>) [<code>world</code>] [<code>hello world</code>]</p>
");
		}

		[Test] public void ImageLink()
		{
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.jpg",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.jpg""></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.png",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.png""></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.gif",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.gif""></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.jpeg",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.jpeg""></p>
");
			// Make sure we really need the period before the trigger extensions...
			// Look for a hyperlink, not an <img>
			FormatTestContains(
				@"http://www.microsoft.com/billgates/images/sofa-billjpeg",
				@"<a href");
		}

		[Test] public void BehaviorTest()
		{
			string s = FormattedTestText(@"@@ProductName@@");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("FlexWiki")) + @""">FlexWiki</a>");
		}

		[Test] public void BehaviorWithLineBreak()
		{
			string s = FormattedTestText(@"@@[100, 200
, 
300]@@");
			AssertStringContains(s, @"100200300");
		}

		[Test] public void ImageBehaviorTwoParamTest() 
		{
			FormatTest("@@Image(\"http://server/image.jpg\", \"Alternative text\")@@",
				"<p><img src=\"http://server/image.jpg\" alt=\"Alternative text\"></p>\n");
		}

		[Test] public void ImageBehaviorFourParamTest() 
		{
			FormatTest("@@Image(\"http://server/image.jpg\", \"Alternative text\", \"500\", \"400\")@@",
				"<p><img src=\"http://server/image.jpg\" alt=\"Alternative text\" " +
				"width=\"500\" height=\"400\"></p>\n");
		}

		[Test] public void ImageBehaviorEmbeddedQuotationMarks() 
		{
			FormatTest(@"@@Image(""http://server/image.jpg"", ""Alt \""text\"""")@@",
				"<p><img src=\"http://server/image.jpg\" alt=\"Alt &quot;text&quot;\"></p>\n");
		}

		[Test] public void ImageBehaviorTwoPerLineTest()
		{
			FormatTest("@@Image(\"http://server/image.jpg\", \"alt\")@@ and @@Image(\"http://server/image2.jpg\", \"alt2\")@@",
				"<p><img src=\"http://server/image.jpg\" alt=\"alt\"> and <img src=\"http://server/image2.jpg\" alt=\"alt2\"></p>\n");
		}

		[Test] public void XmlTransformBehaviorTwoParamTest() 
		{
      // Need to escape all the backslashes in the path
      string xmlPath = testRssXmlPath.Replace(@"\", @"\\");
      string xslPath = testRssXslPath.Replace(@"\", @"\\");

			FormatTest("@@XmlTransform(\"" + xmlPath + "\", \"" + xslPath + "\")@@",
				"<p><h1>Weblogs @ ASP.NET</h1>\n\n<table cellpadding='2' cellspacing='1' class='TableClass'>\n<tr>\n<td  class='TableCell'><strong>Published Date</strong></td>\n<td  class='TableCell'><strong>Title</strong></td>\n</tr>\n<tr>\n<td  class='TableCell'>Wed, 07 Jan 2004 05:45:00 GMT</td>\n<td  class='TableCell'><a href=\"http://weblogs.asp.net/aconrad/archive/2004/01/06/48205.aspx\" title=\"\">Fast Chicken</a></td>\n</tr>\n<tr>\n<td  class='TableCell'>Wed, 07 Jan 2004 03:36:00 GMT</td>\n<td  class='TableCell'><a href=\"http://weblogs.asp.net/CSchittko/archive/2004/01/06/48178.aspx\" title=\"\">Are You Linked In?</a></td>\n</tr>\n<tr>\n<td  class='TableCell'>Wed, 07 Jan 2004 03:27:00 GMT</td>\n<td  class='TableCell'><a href=\"http://weblogs.asp.net/francip/archive/2004/01/06/48172.aspx\" title=\"\">Whidbey configuration APIs</a></td>\n</tr>\n</table></p>\n");
		}

		[Test] public void XmlTransformBehaviorXmlParamNotFoundTest() 
		{
			FormatTestContains("@@XmlTransform(\"file://noWayThisExists\", \"Alternative text\")@@",
				"Failed to load XML parameter");
		}
    [Ignore("This test fails on the build machine. Not sure why, but it's blocking deployment of CruiseControl, so I'll come back to it later. --CraigAndera")]
		[Test] public void XmlTransformBehaviorXslParamNotFoundTest() 
		{
      // Go against just the filename: the full path screws up the build machine
      string xmlPath = Path.GetFileName(meerkatRssPath);

      FormatTestContains("@@XmlTransform(\"" + xmlPath + "\", \"file://noWayThisExists\")@@",
				"Failed to load XSL parameter");
		}

		[Test] public void BracketedHyperLinks()
		{
			FormatTest(
				@"(http://www.msn.com) (http://www.yahoo.com) (http://www.yahoo.com)",
				@"<p>(<a href=""http://www.msn.com"">http://www.msn.com</a>) (<a href=""http://www.yahoo.com"">http://www.yahoo.com</a>) (<a href=""http://www.yahoo.com"">http://www.yahoo.com</a>)</p>
");
			FormatTest(
				@"[http://www.msn.com] [http://www.yahoo.com] [http://www.yahoo.com]",
				@"<p>[<a href=""http://www.msn.com"">http://www.msn.com</a>] [<a href=""http://www.yahoo.com"">http://www.yahoo.com</a>] [<a href=""http://www.yahoo.com"">http://www.yahoo.com</a>]</p>
");
			FormatTest(
				@"{http://www.msn.com} {http://www.yahoo.com} {http://www.yahoo.com}",
				@"<p>{<a href=""http://www.msn.com"">http://www.msn.com</a>} {<a href=""http://www.yahoo.com"">http://www.yahoo.com</a>} {<a href=""http://www.yahoo.com"">http://www.yahoo.com</a>}</p>
");
		}

		[Test] public void BasicHyperLinks()
		{
			FormatTest(
				@"http://www.msn.com http://www.yahoo.com",
				@"<p><a href=""http://www.msn.com"">http://www.msn.com</a> <a href=""http://www.yahoo.com"">http://www.yahoo.com</a></p>
");
			FormatTest(
				@"ftp://feeds.scripting.com",
				@"<p><a href=""ftp://feeds.scripting.com"">ftp://feeds.scripting.com</a></p>
");
			FormatTest(
				@"gopher://feeds.scripting.com",
				@"<p><a href=""gopher://feeds.scripting.com"">gopher://feeds.scripting.com</a></p>
");
			FormatTest(
				@"telnet://melvyl.ucop.edu/",
				@"<p><a href=""telnet://melvyl.ucop.edu/"">telnet://melvyl.ucop.edu/</a></p>
");
			FormatTest(
				@"news:comp.infosystems.www.servers.unix",
				@"<p><a href=""news:comp.infosystems.www.servers.unix"">news:comp.infosystems.www.servers.unix</a></p>
");
			FormatTest(
				@"https://server/directory",
				@"<p><a href=""https://server/directory"">https://server/directory</a></p>
");
			FormatTest(
				@"http://www.msn:8080/ http://www.msn:8080",
				@"<p><a href=""http://www.msn:8080/"">http://www.msn:8080/</a> <a href=""http://www.msn:8080"">http://www.msn:8080</a></p>
");
			
		}
		[Test] public void NamedHyperLinks()
		{
			FormatTest(
				@"""msn"":http://www.msn.com ""yahoo"":http://www.yahoo.com",
				@"<p><a href=""http://www.msn.com"" title="""">msn</a> <a href=""http://www.yahoo.com"" title="""">yahoo</a></p>
");
			FormatTest(
				@"""ftp link"":ftp://feeds.scripting.com",
				@"<p><a href=""ftp://feeds.scripting.com"" title="""">ftp link</a></p>
");
			FormatTest(
				@"""gopher link"":gopher://feeds.scripting.com",
				@"<p><a href=""gopher://feeds.scripting.com"" title="""">gopher link</a></p>
");
			FormatTest(
				@"""telnet link"":telnet://melvyl.ucop.edu/",
				@"<p><a href=""telnet://melvyl.ucop.edu/"" title="""">telnet link</a></p>
");
			FormatTest(
				@"""news group link"":news:comp.infosystems.www.servers.unix",
				@"<p><a href=""news:comp.infosystems.www.servers.unix"" title="""">news group link</a></p>
");
			FormatTest(
				@"""secure link"":https://server/directory",
				@"<p><a href=""https://server/directory"" title="""">secure link</a></p>
");
			FormatTest(
				@"""port link"":http://www.msn:8080/ ""port link"":http://www.msn:8080",
				@"<p><a href=""http://www.msn:8080/"" title="""">port link</a> <a href=""http://www.msn:8080"" title="""">port link</a></p>
");
		}
		[Test] public void PoundHyperLinks()
		{
			FormatTest(
				@"http://www.msn.com#hello",
				@"<p><a href=""http://www.msn.com#hello"">http://www.msn.com#hello</a></p>
");
			FormatTest(
				@"http://www.msn.com#hello",
				@"<p><a href=""http://www.msn.com#hello"">http://www.msn.com#hello</a></p>
");
		}
		[Test] public void PlusSignHyperLinks()
		{
			FormatTest(
				@"http://www.google.com/search?q=wiki+url+specification",
				@"<p><a href=""http://www.google.com/search?q=wiki+url+specification"">http://www.google.com/search?q=wiki+url+specification</a></p>
");
		}
		[Test] public void PercentSignHyperLinks()
		{
			FormatTest(
				@"file://server/directory/file%20GM%.doc",
				@"<p><a href=""file://server/directory/file%20GM%.doc"">file://server/directory/file%20GM%.doc</a></p>
");
			FormatTest(
				@"""Sales 20% Markup"":file://server/directory/sales%2020%%20Markup.doc",
				@"<p><a href=""file://server/directory/sales%2020%%20Markup.doc"" title="""">Sales 20% Markup</a></p>
");
		}
		[Test][Ignore("Not Functional yet")] public void DoNotConvertIntoLinks()
		{
			FormatTest(
				@":",
				@"<p>:</p>
");
			FormatTest(
				@"http",
				@"<p>http</p>
");
			FormatTest(
				@"http:",
				@"<p>http:</p>
");
			FormatTest(
				@"https",
				@"<p>https</p>
");
			FormatTest(
				@"https:",
				@"<p>https:</p>
");
			FormatTest(
				@"ftp",
				@"<p>ftp</p>
");
			FormatTest(
				@"ftp:",
				@"<p>ftp:</p>
");
			FormatTest(
				@"gopher",
				@"<p>gopher</p>
");
			FormatTest(
				@"gopher:",
				@"<p>gopher:</p>
");
			FormatTest(
				@"news",
				@"<p>news</p>
");
			FormatTest(
				@"news:",
				@"<p>news:</p>
");
			FormatTest(
				@"telnet",
				@"<p>telnet</p>
");
			FormatTest(
				@"telnet:",
				@"<p>telnet:</p>
");
		}
		[Test] public void ParensHyperLinks()
		{
			FormatTest(
				@"file://servername/directory/File%20(1420).txt",
				@"<p><a href=""file://servername/directory/File%20(1420).txt"">file://servername/directory/File%20(1420).txt</a></p>
");
		}
		[Test] public void SemicolonHyperLinks()
		{
			FormatTest(
				@"http://servername/directory/File.html?test=1;test2=2",
				@"<p><a href=""http://servername/directory/File.html?test=1;test2=2"">http://servername/directory/File.html?test=1;test2=2</a></p>
");
		}
		[Test] public void DollarSignHyperLinks()
		{
			FormatTest(
				@"http://feeds.scripting.com/discuss/msgReader$4",
				@"<p><a href=""http://feeds.scripting.com/discuss/msgReader$4"">http://feeds.scripting.com/discuss/msgReader$4</a></p>
");
			FormatTest(
				@"file://machine/user$/folder/file",
				@"<p><a href=""file://machine/user$/folder/file"">file://machine/user$/folder/file</a></p>
");
		}
		[Test][Ignore("Not functional yet")] public void TildeHyperLinks()
		{
			// Collides with textile subscript markup
			FormatTest(
				@"""TildeLink"":http://servername/~mike",
				@"<p><a href=""http://servername/~mike"" title="""">TildeLink</a></p>
");
			FormatTest(
				@"http://servername/~mike",
				@"<p><a href=""http://servername/~mike"">http://servername/~mike</a></p>
");
		}			
		[Test] public void TextileFormat()
		{
			// _emphasis_
			FormatTest(
				@"_emphasis_",
				@"<p><em>emphasis</em></p>
");
			// *strong* 
			FormatTest(
				@"*strong*",
				@"<p><strong>strong</strong></p>
");
			// ??citation?? 
			FormatTest(
				@"??citation??",
				@"<p><cite>citation</cite></p>
");
			// -deleted text- 
			FormatTest(
				@"-deleted text-",
				@"<p><del>deleted text</del></p>
");
			// +inserted text+ 
			FormatTest(
				@"+inserted text+",
				@"<p><ins>inserted text</ins></p>
");
			// ^superscript^ 
			FormatTest(
				@"^superscript^",
				@"<p><sup>superscript</sup></p>
");
			// ~subscript~ 
			FormatTest(
				@"~1/2~",
				@"<p><sub>1/2</sub></p>
");

		}
		[Test] public void MultipleParametersHyperLinks()
		{
			// This test verifies the & sign can work in a URL
			FormatTest(
				@"http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8",
				@"<p><a href=""http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8"">http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8</a></p>
");

		}
		[Test] public void Ambersand()
		{
			// Since & sign is not a valid html character also veryify that the & sign is correct when it is not in a URL
			FormatTest(
				@"this test should make the & sign a freindly HTML element",
				@"<p>this test should make the &amp; sign a freindly HTML element</p>
");
		}
		[Test] public void ListAfterPreTest()
		{
			FormatTest(
				@" pre
	* hello
	* goodbye",
				@"<pre>
 pre
</pre>
<ul>
<li> hello</li>

<li> goodbye</li>

</ul>
");
		}


		[Test] public void PreTest()
		{
			FormatTest(
				@" pre
 pre

",
				@"<pre>
 pre
 pre
</pre>
");
			FormatTest(
				@" pre
 pre",
				@"<pre>
 pre
 pre
</pre>
");
		}

		string FormattedTestText(string inputString)
		{
			return FormattedTestText(inputString, null);
		}

		string FormattedTestText(string inputString, AbsoluteTopicName top)
		{
			WikiOutput output = WikiOutput.ForFormat(OutputFormat.Testing, null);
			Formatter.Format(top, inputString, output,  _cb, _lm, _externals, 0, null);
			string o = output.ToString();
			string o1 = o.Replace("\r", "");
			return o1;
		}

		string FormattedTopic(AbsoluteTopicName top)
		{
			return FormattedTestText(TheFederation.ContentBaseForTopic(top).Read(top), top);
		}

		void FormattedTopicContainsTest(AbsoluteTopicName top, string find)
		{
			FormatTestContains(FormattedTopic(top), find);
		}

		void FormatTestContains(string inputString, string find)
		{
			string s = FormattedTestText(inputString);
			AssertStringContains(s, find);
		}

		void FormatTest(string inputString, string outputString)
		{
			FormatTest(inputString, outputString, null);
		}

		void FormatTest(string inputString, string outputString, AbsoluteTopicName top)
		{
			string o1 = FormattedTestText(inputString, top);
			string o2 = outputString.Replace("\r", "");
			if (o1 != o2)
			{
				Console.Error.WriteLine("Got     : " + o1);
				Console.Error.WriteLine("Expected: " + o2);
			}
			Assertion.AssertEquals(o2, o1);
		}			
	}

	[TestFixture] public class NameMatches
	{
		// Tests to make sure the wikiname extraction work correctly
		[Test] public void TestNames()
		{
			ExtractName("HelloThere029.1BigDog", "HelloThere029");
			ExtractName("HelloThere.BigDog", "HelloThere.BigDog");
			ExtractName("Hey.BigDog", "Hey.BigDog");
			ExtractName(".BigDog", ".BigDog");
			ExtractName("This.Long.NameHere.BigDog", "This.Long.NameHere.BigDog");
			ExtractName("Hello.Big", null);
			ExtractName("HelloT22here.BigD11og", "HelloT22here.BigD11og");
			ExtractName("Some VerySimpleLinksFatPig", "VerySimpleLinksFatPig");	
		}

		[Test] public void TestBracketNames()
		{
			ExtractName(".[name]", ".[name]");
			ExtractName("[name]", "[name]");
			ExtractName("Name.[name]", "Name.[name]");
			ExtractName("Name.[Hot]", "Name.[Hot]");
			ExtractName("Name.[HotDog]", "Name.[HotDog]");
		}

		void ExtractName(string input, string match)
		{
			Regex m = new Regex(Formatter.extractWikiNamesString);
			if (match != null)
			{
				Assertion.AssertEquals(match, 1, m.Matches(input).Count);
				Assertion.AssertEquals(match, match, m.Matches(input)[0].Groups["topic"].Value);
			}
			else
			{
				Assertion.AssertEquals(match, 0, m.Matches(input).Count);
			}
		}


	}

	[TestFixture] public class FormattingServicesTests : WikiTests
	{
		ContentBase	_base;
		ArrayList _versions;
		LinkMaker _lm;

		[SetUp] public void Init()
		{
			string _tempPath = @"\temp-wikitests";
			string author = "tester-joebob";
			_lm = new LinkMaker("http://bogusville");
			TheFederation = new Federation(OutputFormat.HTML, _lm);

			_versions = new ArrayList();
			TheFederation.Register("FlexWiki.Base", _tempPath);
			_base = TheFederation.ContentBaseForRoot(_tempPath);
			_base.Namespace = "FlexWiki.Base";

			WriteTestTopicAndNewVersion(_base, "TopicOne", @"1
2
3
4
5
6
7
8
9", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"1
2
a
b
c
3
4
5
6
7
8
9", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"1
2
a
b
6
7
8
9", author);

			foreach (TopicChange change in _base.AllChangesForTopic(_base.TopicNameFor("TopicOne")))
				_versions.Add(change.Version);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
		}

		
		ContentBase ContentBase()
		{
			return _base;
		}

		[Test] public void OldestTest()
		{
			// Test the oldest; should have no markers
			VersionCompare("TopicOne", (string)_versions[_versions.Count - 1], @"<p>1</p>
<p>2</p>
<p>3</p>
<p>4</p>
<p>5</p>
<p>6</p>
<p>7</p>
<p>8</p>
<p>9</p>
");
		}

		[Test] public void InsertTest()
		{
			// Inserts oldest should have the 
			VersionCompare("TopicOne", (string)_versions[_versions.Count - 2], @"<p>1</p>
<p>2</p>
<p style='background: palegreen'>a</p>
<p style='background: palegreen'>b</p>
<p style='background: palegreen'>c</p>
<p>3</p>
<p>4</p>
<p>5</p>
<p>6</p>
<p>7</p>
<p>8</p>
<p>9</p>
");
		}

		[Test] public void DeleteTest()
		{
			VersionCompare("TopicOne", (string)_versions[_versions.Count - 3], @"<p>1</p>
<p>2</p>
<p>a</p>
<p>b</p>
<p style='color: silver; text-decoration: line-through'>c</p>
<p style='color: silver; text-decoration: line-through'>3</p>
<p style='color: silver; text-decoration: line-through'>4</p>
<p style='color: silver; text-decoration: line-through'>5</p>
<p>6</p>
<p>7</p>
<p>8</p>
<p>9</p>
");
		}

		void VersionCompare(string topic, string version, string expecting)
		{
			AbsoluteTopicName abs = ContentBase().TopicNameFor(topic);
			abs.Version = version;
			string got = Formatter.FormattedTopic(abs, OutputFormat.Testing, true, TheFederation, _lm, null);
			got = got.Replace("\r", "");
			string o2 = expecting.Replace("\r", "");

			if (got != o2)
			{
				Console.Error.WriteLine("Got     : " + got);
				Console.Error.WriteLine("Expected: " + o2);
			}
			Assertion.AssertEquals(o2, got);
		}
	}
}
