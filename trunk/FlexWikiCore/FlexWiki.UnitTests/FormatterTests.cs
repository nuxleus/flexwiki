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
using FlexWiki;

using NUnit.Framework;


namespace FlexWiki.UnitTests
{
	[TestFixture] public class TableFormattingOptionsTests 
	{
		public void TestDefaults()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("");
			Assert.IsTrue(info.HasBorder);
			Assert.IsTrue(!info.IsHighlighted);
		}

		public void TestError()
		{
			TableCellInfo info = new TableCellInfo();
			Assert.IsTrue(info.Parse("T%") != null);
		}


		public void TestTable()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("T-T]");
			Assert.IsTrue(!info.HasBorder);
			Assert.AreEqual(info.TableAlignment, TableCellInfo.AlignOption.Right);
			info.Parse("T[");
			Assert.AreEqual(info.TableAlignment, TableCellInfo.AlignOption.Left);
			info.Parse("T^");
			Assert.AreEqual(info.TableAlignment, TableCellInfo.AlignOption.Center);
		}

		public void TestCellAlignment()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("]");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Right);
			info.Parse("[");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Left);
			info.Parse("^");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Center);
		}

		public void TestHighlight()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("!");
			Assert.IsTrue(info.IsHighlighted);
		}

		public void TestSpans()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("C10!R5T-");
			Assert.IsTrue(info.IsHighlighted);
			Assert.AreEqual(info.ColSpan, 10);
			Assert.AreEqual(info.RowSpan, 5);
		}

		public void TestWidths()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("");
			Assert.IsTrue(info.TableWidth == TableCellInfo.UnspecifiedWidth);
			Assert.IsTrue(info.CellWidth == TableCellInfo.UnspecifiedWidth);
			info.Parse("TW1");
			Assert.AreEqual(1, info.TableWidth);
			info.Parse("TW100");
			Assert.AreEqual(100, info.TableWidth);
			info.Parse("TW100C2");
			Assert.AreEqual(100, info.TableWidth);
			info.Parse("W1");
			Assert.AreEqual(1, info.CellWidth);
			info.Parse("W100");
			Assert.AreEqual(100, info.CellWidth);
			info.Parse("W100C2");
			Assert.AreEqual(100, info.CellWidth);
			info.Parse("W100TW200C2");
			Assert.AreEqual(100, info.CellWidth);
			Assert.AreEqual(200, info.TableWidth);


		}

		public void TestErrorOnMissingIntegers()
		{
			TableCellInfo info = new TableCellInfo();
			string missing = "Missing";
			Assert.IsTrue(info.Parse("TW").StartsWith(missing));
			Assert.IsTrue(info.Parse("R").StartsWith(missing));
			Assert.IsTrue(info.Parse("C").StartsWith(missing));
			Assert.IsTrue(info.Parse("W").StartsWith(missing));
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

			_cb = CreateStore("FlexWiki");
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
			WriteTestTopicAndNewVersion(_cb, "InlineTestTopic", @"aaa @@""foo""@@ zzz", user);
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

		#region WikiPageProperty Tests
		[Test] public void SinglineLinePropertyTest()
		{
			FormatTest(
				@"Singleline: single line property",
				@"<a name=""Singleline"" class=""Anchor"">
<fieldset  class='Property' style='width: auto'>
<legend class='PropertyName'>Singleline</legend> <span class='PropertyValue'>
single line property</span>
</fieldset>
</a>

");
		}

		[Test] public void FormattedSingleLinePropertyTest()
		{
			FormatTest(
				@"Singleline: '''bold''' ''italics'' -deleted-",
				@"<a name=""Singleline"" class=""Anchor"">
<fieldset  class='Property' style='width: auto'>
<legend class='PropertyName'>Singleline</legend> <span class='PropertyValue'>
<strong>bold</strong> <em>italics</em> <del>deleted</del></span>
</fieldset>
</a>

");
		}

		[Test] public void MultilinePropertyTest()
		{
			FormatTest(
				@"Multiline:[
first line
second line
]",
				@"<a name=""Multiline"" class=""Anchor"">
<fieldset  class='Property' style='width: auto'>
<legend class='PropertyName'>Multiline</legend> <span class='PropertyValue'>
<p>first line</p>
<p>second line</p>
</span>
</fieldset>
</a>
");
		}

		[Test] public void FormattedMultilinePropertyTest()
		{
			FormatTest(
				@"Multiline:[
!Heading1
'''bold''' ''italics'' -deleted-
]",
				@"<a name=""Multiline"" class=""Anchor"">
<fieldset  class='Property' style='width: auto'>
<legend class='PropertyName'>Multiline</legend> <span class='PropertyValue'>
<h1>Heading1</h1>

<p><strong>bold</strong> <em>italics</em> <del>deleted</del></p>
</span>
</fieldset>
</a>
");
		}
		#endregion

		[Test] public void TestInlineWikiTalk()
		{
			WikiOutput output = WikiOutput.ForFormat(OutputFormat.HTML, null);
			AbsoluteTopicName top = new AbsoluteTopicName("InlineTestTopic", _cb.Namespace);
			Formatter.Format(top, TheFederation.ContentBaseForTopic(top).Read(top.LocalName), output,  _cb, _lm, _externals, 0, null);
			string result = output.ToString();
			Assert.IsTrue(result.IndexOf("aaa foo zzz") >= 0);
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
			Assert.IsTrue(container.IndexOf(find) != -1, "Searching for " + find + " in " + container);
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

		[Test] public void TopicNameWithAnchorRegexTests()
		{
			ShouldBeTopicName("AaA#Anchor");
			ShouldBeTopicName("AaA#anchor");
			ShouldBeTopicName("AaA#TestAnchor");
			ShouldBeTopicName("AaA#testAnchor");
			ShouldBeTopicName("AAa#Anchor");
			ShouldBeTopicName("AAa#anchor");
			ShouldBeTopicName("AAa#TestAnchor");
			ShouldBeTopicName("AAa#testAnchor");
			ShouldBeTopicName("AaAA#Anchor");
			ShouldBeTopicName("AaAA#anchor");
			ShouldBeTopicName("AaAA#TestAnchor");
			ShouldBeTopicName("AaAA#aestAnchor");
			ShouldBeTopicName("AaAa#Anchor");
			ShouldBeTopicName("AaAa#anchor");
			ShouldBeTopicName("AaAa#TestAnchor");
			ShouldBeTopicName("AaAa#testAnchor");
			ShouldBeTopicName("ZAAbAA#Anchor");
			ShouldBeTopicName("ZAAbAA#anchor");
			ShouldBeTopicName("ZAAbAA#TestAnchor");
			ShouldBeTopicName("ZAAbAA#testAnchor");
			ShouldBeTopicName("ZAAb#Anchor");
			ShouldBeTopicName("ZAAb#anchor");
			ShouldBeTopicName("ZAAb#TestAnchor");
			ShouldBeTopicName("ZAAb#testAnchor");
			ShouldBeTopicName("ZaAA#Anchor");
			ShouldBeTopicName("ZaAA#anchor");
			ShouldBeTopicName("ZaAA#TestAnchor");
			ShouldBeTopicName("ZaAA#testAnchor");
			ShouldBeTopicName("CSharp#Anchor");
			ShouldBeTopicName("CSharp#anchor");
			ShouldBeTopicName("CSharp#TestAnchor");
			ShouldBeTopicName("CSharp#testAnchor");
			ShouldBeTopicName("Meeting25Dec#Anchor");
			ShouldBeTopicName("Meeting25Dec#anchor");
			ShouldBeTopicName("Meeting25Dec#TestAnchor");
			ShouldBeTopicName("Meeting25Dec#testAnchor");
			ShouldBeTopicName("KeyOfficeIRMFunctionality#Anchor");
			ShouldBeTopicName("KeyOfficeIRMFunctionality#anchor");
			ShouldBeTopicName("KeyOfficeIRMFunctionality#TestAnchor");
			ShouldBeTopicName("KeyOfficeIRMFunctionality#testAnchor");
			ShouldBeTopicName("IBMMainframe#Anchor");
			ShouldBeTopicName("IBMMainframe#anchor");
			ShouldBeTopicName("IBMMainframe#TestAnchor");
			ShouldBeTopicName("IBMMainframe#testAnchor");

			ShouldNotBeTopicName("AAA#Anchor");
			ShouldNotBeTopicName("AAA#anchor");
			ShouldNotBeTopicName("AAA#TestAnchor");
			ShouldNotBeTopicName("AAA#testAnchor");
			ShouldNotBeTopicName("AA#Anchor");
			ShouldNotBeTopicName("AA#anchor");
			ShouldNotBeTopicName("AA#TestAnchor");
			ShouldNotBeTopicName("AA#testAnchor");
			ShouldNotBeTopicName("A42#Anchor");
			ShouldNotBeTopicName("A42#anchor");
			ShouldNotBeTopicName("A42#TestAnchor");
			ShouldNotBeTopicName("A42#testAnchor");
			ShouldNotBeTopicName("A#Anchor");
			ShouldNotBeTopicName("A#anchor");
			ShouldNotBeTopicName("A#TestAnchor");
			ShouldNotBeTopicName("A#testAnchor");
			ShouldNotBeTopicName("a#Anchor");
			ShouldNotBeTopicName("a#anchor");
			ShouldNotBeTopicName("a#TestAnchor");
			ShouldNotBeTopicName("a#testAnchor");
			ShouldNotBeTopicName("about#Anchor");
			ShouldNotBeTopicName("about#anchor");
			ShouldNotBeTopicName("about#TestAnchor");
			ShouldNotBeTopicName("about#testAnchor");
			ShouldNotBeTopicName("Hello#Anchor");
			ShouldNotBeTopicName("Hello#anchor");
			ShouldNotBeTopicName("Hello#TestAnchor");
			ShouldNotBeTopicName("Hello#testAnchor");
		}

		[Test] public void NonAsciiTopicNameWithAnchorRegexTest() 
		{
			ShouldBeTopicName("DistribuciónTécnica#Distribución");
			ShouldBeTopicName("DistribuciónTécnica#distribución");
			ShouldBeTopicName("DistribuciónTécnica#DistribuciónTécnica");
			ShouldBeTopicName("DistribuciónTécnica#distribuciónTécnica");
			ShouldBeTopicName("ProblemasProgramación#Problemas");
			ShouldBeTopicName("ProblemasProgramación#problemas");
			ShouldBeTopicName("ProblemasProgramación#ProblemasProgramación");
			ShouldBeTopicName("ProblemasProgramación#problemasProgramación");
			ShouldBeTopicName("SmørgåsBord#Smørgås");
			ShouldBeTopicName("SmørgåsBord#smørgås");
			ShouldBeTopicName("SmørgåsBord#SmørgåsBord");
			ShouldBeTopicName("SmørgåsBord#smørgåsBord");
			ShouldBeTopicName("ØrneRede#Ørne");
			ShouldBeTopicName("ØrneRede#ØrneRede");
			ShouldBeTopicName("HöchstMaß#Höchst");
			ShouldBeTopicName("HöchstMaß#höchst");
			ShouldBeTopicName("HöchstMaß#HöchstMaß");
			ShouldBeTopicName("HöchstMaß#höchstMaß");
			ShouldBeTopicName("FaçadePattern#FaçadePattern");
			ShouldBeTopicName("FaçadePattern#façadePattern");
			ShouldBeTopicName("FaçadePattern#Façade");
			ShouldBeTopicName("FaçadePattern#façade");
			ShouldBeTopicName("ReykjavíkCity#Reykjavík");
			ShouldBeTopicName("ReykjavíkCity#reykjavík");
			ShouldBeTopicName("ReykjavíkCity#ReykjavíkCity");
			ShouldBeTopicName("ReykjavíkCity#reykjavíkCity");

			// string russian = "\u1044\u1086\u1093\u1083\u1072\u1103\u1056\u1099\u1073\u1072";
			// ShouldBeTopicName(russian);

			ShouldNotBeTopicName("æøå#æøå");
			ShouldNotBeTopicName("ÆØÅ#ÆØÅ");
			ShouldNotBeTopicName("Ølle#Ølle");
			ShouldNotBeTopicName("ølle#ølle");
		}

		void ShouldBeTopicName(string s)
		{
			string topicName = s;
			int hashIndex = s.IndexOf("#");
			if (hashIndex > -1)
			{
				topicName = s.Substring(0, hashIndex);
			}
			FormatTest(
				@s,
				@"<p><a title='Click here to create this topic' class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor(topicName)) + @""">" + topicName + @"</a></p>
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

		[Test] public void RealTopicWithAnchorTest()
		{
			FormatTestContains(
				"HomePage#Anchor",
				"href=\"" + _lm.LinkToTopic(_cb.TopicNameFor("HomePage")) + "#Anchor\">HomePage#Anchor</a>");
		}

		[Test] public void RealTopicWithAnchorRelabelTest()
		{
			FormatTestContains(
				"\"Relabel\":HomePage#Anchor",
				"href=\"" + _lm.LinkToTopic(_cb.TopicNameFor("HomePage")) + "#Anchor\">Relabel</a>");
		}

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

		[Test] public void UnusualCharactersInLinkTest()
		{
			FormatTest(
				@"some leading text http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm some trailing text",
				@"<p>some leading text <a href=""http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm"">http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm</a> some trailing text</p>
");
		}

		[Test] public void HyphensInImageLinkTest()
		{
			FormatTest(
				@"some leading text http://msdn.microsoft.com/library/en-us/dnpag/html/ch01---engineering-for-perf.gif some trailing text",
				@"<p>some leading text <img src=""http://msdn.microsoft.com/library/en-us/dnpag/html/ch01---engineering-for-perf.gif""> some trailing text</p>
");
		}

		[Test] public void TextileHyphensInLinkTest()
		{
			FormatTest(
				@"some leading text ""textile"":http://www.amazon.com/exec/obidos/tg/detail/-/0735617228/qid=1080277573/sr=8-1/ref=pd_ka_1/102-9825269-6457731?v=glance&s=books&n=50784 some trailing text",
				@"<p>some leading text <a href=""http://www.amazon.com/exec/obidos/tg/detail/-/0735617228/qid=1080277573/sr=8-1/ref=pd_ka_1/102-9825269-6457731?v=glance&s=books&n=50784"">textile</a> some trailing text</p>
");
		}

		[Test] public void DollarInLinkTest()
		{
			FormatTest(
				@"""Main directory"":file://servername/umuff$/folder%20name/file.txt
file://servername/umuff$/folder%20name/file.txt",
				@"<p><a href=""file://servername/umuff$/folder%20name/file.txt"">Main directory</a></p>
<p><a href=""file://servername/umuff$/folder%20name/file.txt"">file://servername/umuff$/folder%20name/file.txt</a></p>
");
		}

		[Test] public void TildeInLinkTest()
		{
			FormatTest(
				@"""Main directory"":file://servername/umuff~/folder%20name/file.txt
file://servername/umuff~/folder%20name/file.txt",
				@"<p><a href=""file://servername/umuff~/folder%20name/file.txt"">Main directory</a></p>
<p><a href=""file://servername/umuff~/folder%20name/file.txt"">file://servername/umuff~/folder%20name/file.txt</a></p>
");
		}

		[Test] public void Hash126InLink()
		{
			FormatTest(
				@"""Main directory"":file://servername/umuff&#126;/folder%20name/file.txt
file://servername/umuff&#126;/folder%20name/file.txt",
				@"<p><a href=""file://servername/umuff&#126;/folder%20name/file.txt"">Main directory</a></p>
<p><a href=""file://servername/umuff&#126;/folder%20name/file.txt"">file://servername/umuff&#126;/folder%20name/file.txt</a></p>
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
				@"<p>Please send <a href=""mailto:person@domain.com"">person</a> some email!</p>
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
				"<p><h1>Weblogs @ ASP.NET</h1>\n\n<table cellpadding='2' cellspacing='1' class='TableClass'>\n<tr>\n<td  class='TableCell'><strong>Published Date</strong></td>\n<td  class='TableCell'><strong>Title</strong></td>\n</tr>\n<tr>\n<td  class='TableCell'>Wed, 07 Jan 2004 05:45:00 GMT</td>\n<td  class='TableCell'><a href=\"http://weblogs.asp.net/aconrad/archive/2004/01/06/48205.aspx\">Fast Chicken</a></td>\n</tr>\n<tr>\n<td  class='TableCell'>Wed, 07 Jan 2004 03:36:00 GMT</td>\n<td  class='TableCell'><a href=\"http://weblogs.asp.net/CSchittko/archive/2004/01/06/48178.aspx\">Are You Linked In?</a></td>\n</tr>\n<tr>\n<td  class='TableCell'>Wed, 07 Jan 2004 03:27:00 GMT</td>\n<td  class='TableCell'><a href=\"http://weblogs.asp.net/francip/archive/2004/01/06/48172.aspx\">Whidbey configuration APIs</a></td>\n</tr>\n</table></p>\n");
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
				@"<p><a href=""http://www.msn.com"">msn</a> <a href=""http://www.yahoo.com"">yahoo</a></p>
");
			FormatTest(
				@"""ftp link"":ftp://feeds.scripting.com",
				@"<p><a href=""ftp://feeds.scripting.com"">ftp link</a></p>
");
			FormatTest(
				@"""gopher link"":gopher://feeds.scripting.com",
				@"<p><a href=""gopher://feeds.scripting.com"">gopher link</a></p>
");
			FormatTest(
				@"""telnet link"":telnet://melvyl.ucop.edu/",
				@"<p><a href=""telnet://melvyl.ucop.edu/"">telnet link</a></p>
");
			FormatTest(
				@"""news group link"":news:comp.infosystems.www.servers.unix",
				@"<p><a href=""news:comp.infosystems.www.servers.unix"">news group link</a></p>
");
			FormatTest(
				@"""secure link"":https://server/directory",
				@"<p><a href=""https://server/directory"">secure link</a></p>
");
			FormatTest(
				@"""port link"":http://www.msn:8080/ ""port link"":http://www.msn:8080",
				@"<p><a href=""http://www.msn:8080/"">port link</a> <a href=""http://www.msn:8080"">port link</a></p>
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
				@"<p><a href=""file://server/directory/sales%2020%%20Markup.doc"">Sales 20% Markup</a></p>
");
		}
		[Test] public void DoNotConvertIntoLinks()
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
		[Test] public void TildeHyperLinks()
		{
			// Collides with textile subscript markup
			FormatTest(
				@"""TildeLink"":http://servername/~mike",
				@"<p><a href=""http://servername/~mike"">TildeLink</a></p>
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
		[Test] public void WikiSignatureWithHyphenDates()
		{
			FormatTest(
				@"Test comment. -- Derek Lakin [28-Jan-2005]",
				@"<p>Test comment. -- Derek Lakin [28-Jan-2005]</p>
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
			return FormattedTestText(TheFederation.ContentBaseForTopic(top).Read(top.LocalName), top);
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
			Assert.AreEqual(o2, o1);
		}			

		[Test] public void PreFormattedBlockTests()
		{
			FormatTest(
				@"
{@KeyWord
Some pre
forma-
ted
 text
  goes
   here
*non bold* ''etc''
} is allowed
}@ is allowed
}@non-key is allowed
}@ KeyWord 
{@
No key case
}@WrongKey
}@
Normal text here
 Regular preformated text
Normal again
",
				@"

<pre>
Some pre
forma-
ted
 text
  goes
   here
*non bold* ''etc''
} is allowed
}@ is allowed
}@non-key is allowed
</pre>
<pre>
No key case
}@WrongKey
</pre>
<p>Normal text here</p>
<pre>
 Regular preformated text
</pre>
<p>Normal again</p>
");
		}

		[Test] public void ColorAndTextSizeTests()
		{
			FormatTest(
				@"%% by itself %% and again %%",
				@"<p>%% by itself %% and again %%</p>
");
			FormatTest(
				@"%red% red %% and now %% by itself",
				@"<p><span style='color:red'> red </span> and now %% by itself</p>
");
			FormatTest(
				@"%red%red %blue%blue%% but this text is normal.",
				@"<p><span style='color:red'>red </span><span style='color:blue'>blue</span> but this text is normal.</p>
");
			FormatTest(
				@"%red%Red and no %big%closing %small%percentpercent
Normal again
",
				@"<p><span style='color:red'>Red and no </span><big>closing </big><small>percentpercent</small></p>
<p>Normal again</p>
");
			FormatTest(
				@"Strange %#13579B%Color %dima% non-color",
				@"<p>Strange <span style='color:#13579b'>Color </span><span style='color:dima'> non-color</span></p>
");
			FormatTest(
				@"'''%red big%Big bold red text''' %small green%''Small green italic''%%normal",
				@"<p><strong><span style='color:red'><big>Big bold red text</strong> </big></span><small><span style='color:green'><em>Small green italic</em></small></span>normal</p>
");
			FormatTest(
				@"normal %big big%Very big %% normal %small small% very small %blue% normal size blue",
				@"<p>normal <big><big>Very big </big></big> normal <small><small> very small </small></small><span style='color:blue'> normal size blue</span></p>
");
		}

		#region FormSelectFieldPresentation tests
		#region Single-line (combo) tests
		[Test] public void SelectFieldTest()
		{
			FormatTest(
				@"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""])@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""1""><option>one</option><option>two</option><option>three</option></select></p>
");
		}
		[Test] public void SelectFieldWithValuesTest()
		{
			FormatTest(
				@"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2, 3])@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""1""><option value=""1"">one</option><option value=""2"">two</option><option value=""3"">three</option></select></p>
");
		}
		[Test] public void SelectFieldWithMismatchedValuesTest()
		{
			FormatTest(
				@"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2])@@",
				@"<p><span class='ErrorMessage'><span class='ErrorMessageBody'>Error evaluating expression: The values array does not contain the same number of items as the options array
Parameter name: values</span></span></p>
");
		}
		[Test] public void SelectFieldEmptyOptionsTest()
		{
			FormatTest(
				@"@@Presentations.ComboSelectField(""selectTest"", [])@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""1""></select></p>
");
		}
		[Test] public void SelectFieldSelectedOptionTest()
		{
			FormatTest(
				@"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], ""two"")@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""1""><option>one</option><option selected=""selected"">two</option><option>three</option></select></p>
");
		}
		[Test] public void SelectFieldSelectedValueTest()
		{
			FormatTest(
				@"@@Presentations.ComboSelectField(""selectTest"", [""one"", ""two"", ""three""], null, [1, 2, 3], 2)@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""1""><option value=""1"">one</option><option value=""2"" selected=""selected"">two</option><option value=""3"">three</option></select></p>
");
		}
		#endregion
		#region Multi-line (listbox) tests
		[Test] public void SelectFieldMultilineTest()
		{
			FormatTest(
				@"@@Presentations.ListSelectField(""selectTest"", 2, false, [""one"", ""two"", ""three""])@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""2""><option>one</option><option>two</option><option>three</option></select></p>
");
		}
		[Test] public void SelectFieldMultilineMultiSelectTest()
		{
			FormatTest(
				@"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""])@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option>one</option><option>two</option><option>three</option></select></p>
");
		}
		[Test] public void SelectFieldMultilineWithValuesTest()
		{
			FormatTest(
				@"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2, 3])@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option value=""1"">one</option><option value=""2"">two</option><option value=""3"">three</option></select></p>
");
		}
		[Test] public void SelectFieldMultilineWithMismatchedValuesTest()
		{
			FormatTest(
				@"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2])@@",
				@"<p><span class='ErrorMessage'><span class='ErrorMessageBody'>Error evaluating expression: The values array does not contain the same number of items as the options array
Parameter name: values</span></span></p>
");
		}
		[Test] public void SelectFieldMultilineEmptyOptionsTest()
		{
			FormatTest(
				@"@@Presentations.ListSelectField(""selectTest"", 2, true, [])@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""></select></p>
");
		}
		[Test] public void SelectFieldMultilineSelectedOptionTest()
		{
			FormatTest(
				@"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], ""two"")@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option>one</option><option selected=""selected"">two</option><option>three</option></select></p>
");
		}
		[Test] public void SelectFieldMultilineSelectedValueTest()
		{
			FormatTest(
				@"@@Presentations.ListSelectField(""selectTest"", 2, true, [""one"", ""two"", ""three""], null, [1, 2, 3], 2)@@",
				@"<p><select name=""selectTest"" id=""selectTest"" size=""2"" multiple=""multiple""><option value=""1"">one</option><option value=""2"" selected=""selected"">two</option><option value=""3"">three</option></select></p>
");
		}
		#endregion
		#endregion
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
				Assert.AreEqual(1, m.Matches(input).Count, match);
				Assert.AreEqual(match, m.Matches(input)[0].Groups["topic"].Value, match);
			}
			else
			{
				Assert.AreEqual(0, m.Matches(input).Count, match);
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
			string author = "tester-joebob";
			_lm = new LinkMaker("http://bogusville");
			TheFederation = new Federation(OutputFormat.HTML, _lm);

			_versions = new ArrayList();
			_base = CreateStore("FlexWiki.Base");

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

			foreach (TopicChange change in _base.AllChangesForTopic(new LocalTopicName("TopicOne")))
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
			Assert.AreEqual(o2, got);
		}
	}
}
