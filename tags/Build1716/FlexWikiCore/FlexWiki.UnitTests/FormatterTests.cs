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
	[TestFixture] public class FormattingTests : WikiTests
	{
		private ContentBase _cb;
		private const string _base = "/formattingtestswiki/";
		private Hashtable _externals;
		private LinkMaker _lm;
		private string user = "joe";

		// At runtime we dump the contents of the embedded resources to a directory so 
		// we don't have to rely on an Internet connection or a hardcoded path to 
		// retrieve the XML when testing behaviors that pull it in. These variables hold
		// the paths to the XML that we write to disk at Init time. 
		private string testRssXmlPath; 
		private string testRssXslPath; 
		private string meerkatRssPath; 

		#region Init
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
			WriteTestTopicAndNewVersion(_cb, "_Underscore", "Underscore", user);
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
		#endregion

		#region Deinit
		[TearDown] public void Deinit()
		{
			_cb.Delete();
		}
		#endregion


		#region TableFormatting rules tests

		[Test] public void TableFormattingRulesTest1()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("!+");
			Assert.IsTrue(info.IsHighlighted);
			Assert.IsFalse(info.AllowBreaks);
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.None);
			Assert.AreEqual(info.CellWidth, TableCellInfo.UnspecifiedWidth);
			Assert.AreEqual(info.ColSpan, 1);
			Assert.AreEqual(info.RowSpan, 1);
			Assert.AreEqual(info.BackgroundColor, null);
		}

		[Test] public void TableFormattingRulesCellWidthTest()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("W10");
			Assert.AreEqual(info.CellWidth, 10);
		}

		[Test] public void TableFormattingRulesColor()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("!*red*+");
			Assert.IsTrue(info.IsHighlighted);
			Assert.AreEqual(info.BackgroundColor, "red");
			Assert.IsFalse(info.AllowBreaks);
		}

		[Test] public void TableFormattingRulesSpanTest()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("C10R3");
			Assert.AreEqual(info.ColSpan, 10);
			Assert.AreEqual(info.RowSpan, 3);
		}


		[Test] public void TableFormattingRulesTestLeftCell()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("[");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Left);
		}

		[Test] public void TableFormattingRulesTestRightCell()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("]");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Right);
		}

		[Test] public void TableFormattingRulesTestLeftCenter()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("^");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Center);
		}

		#endregion


		#region WikiPageProperty Tests
		[Test] public void SinglineLinePropertyTest()
		{
			FormatTest(
				@"Singleline: single line property",
				@"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Singleline</legend><span class=""PropertyValue""><a name=""Singleline"" class=""Anchor"">single line property</a></span>
</fieldset>

");
		}

		[Test] public void FormattedSingleLinePropertyTest()
		{
			FormatTest(
				@"Singleline: '''bold''' ''italics'' -deleted-",
				@"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Singleline</legend><span class=""PropertyValue""><a name=""Singleline"" class=""Anchor""><strong>bold</strong> <em>italics</em> <del>deleted</del></a></span>
</fieldset>

");
		}

		[Test] public void MultilinePropertyTest()
		{
			FormatTest(
				@"Multiline:[
first line
second line
]",
				@"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Multiline</legend><span class=""PropertyValue""><a name=""Multiline"" class=""Anchor""><p>first line</p>
<p>second line</p>
</a></span>
</fieldset>
");
		}

		[Test] public void FormattedMultilinePropertyTest()
		{
			FormatTest(
				@"Multiline:[
!Heading1
'''bold''' ''italics'' -deleted-
]",
				@"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Multiline</legend><span class=""PropertyValue""><a name=""Multiline"" class=""Anchor""><h1>Heading1</h1>

<p><strong>bold</strong> <em>italics</em> <del>deleted</del></p>
</a></span>
</fieldset>
");
		}
		#endregion

		#region Non-existent Namespace Tests
		[Test] public void NonExistentNamespaceTest()
		{
			FormatTest(
				@"FooBar.BlahBlah",
				@"<p>FooBar.BlahBlah</p>
");
		}

		[Test] public void NonExistentNamespaceLeadingTextTest()
		{
			FormatTest(
				@"Leading text FooBar.BlahBlah",
				@"<p>Leading text FooBar.BlahBlah</p>
");
		}

		[Test] public void NonExistentNamespaceTrailingTextTest()
		{
			FormatTest(
				@"FooBar.BlahBlah trailing text",
				@"<p>FooBar.BlahBlah trailing text</p>
");
		}

		[Test] public void NonExistentNamespaceLeadingAndTrailingTextTest()
		{
			FormatTest(
				@"Leading text FooBar.BlahBlah trailing text",
				@"<p>Leading text FooBar.BlahBlah trailing text</p>
");
		}

		[Test] public void NonExistentMultipleNamespaceTest()
		{
			FormatTest(
				@"System.Diagnostics.Debug.WriteLine",
				@"<p>System.Diagnostics.Debug.WriteLine</p>
");
		}
		#endregion

		#region TestInlineWikiTalk
		[Test] public void TestInlineWikiTalk()
		{
			WikiOutput output = WikiOutput.ForFormat(OutputFormat.HTML, null);
			AbsoluteTopicName top = new AbsoluteTopicName("InlineTestTopic", _cb.Namespace);
			Formatter.Format(top, TheFederation.ContentBaseForTopic(top).Read(top.LocalName), output,  _cb, _lm, _externals, 0, null);
			string result = output.ToString();
			Assert.IsTrue(result.IndexOf("aaa foo zzz") >= 0);
		}
		#endregion

		#region TopicBehaviorProperty
		[Test] public void TopicBehaviorProperty()
		{
			FormattedTopicContainsTest(new AbsoluteTopicName("TestTopicWithBehaviorProperties", _cb.Namespace), "len=5");
			FormattedTopicContainsTest(new AbsoluteTopicName("TestTopicWithBehaviorProperties", _cb.Namespace), "lenWith=6");
			FormattedTopicContainsTest(new AbsoluteTopicName("TestTopicWithBehaviorProperties", _cb.Namespace), "lenSpanning=20");
		}
		#endregion

		#region TestAllNamespacesBehavior
		[Test] public void TestAllNamespacesBehavior()
		{
			FormatTestContains("@@AllNamespacesWithDetails@@", "FlexWiki");
			FormatTestContains("@@AllNamespacesWithDetails@@", "Friendly Title");
		}
		#endregion

		#region NamespaceAsTopicPreceedsQualifiedNames
		[Test] public void NamespaceAsTopicPreceedsQualifiedNames()
		{
			string s = FormattedTestText(@"FlexWiki bad FlexWiki.OneMinuteWiki");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("FlexWiki")) + @""">FlexWiki</a> bad");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("OneMinuteWiki")) + @""">OneMinuteWiki</a>");
		}
		#endregion

		#region WikiURIForTopic
		[Test] public void WikiURIForTopic()
		{
			string s = FormattedTestText("wiki://IncludeNestURI");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("IncludeNestURI")) + @""">IncludeNestURI</a>");
		} 
		#endregion

		#region WikiURIForResource
		[Test] public void WikiURIForResource()
		{
			FormatTest(@"wiki://ResourceReference/Cookies", @"<p><a class=""externalLink"" href=""http://www.google.com/Cookies"">http://www.google.com/Cookies</a></p>
");
		}
		#endregion
		
		#region WikiURIForImageResource
		[Test] public void WikiURIForImageResource()
		{
			FormatTest(@"wiki://ResourceReference/Cookies.gif", @"<p><img src=""http://www.google.com/Cookies.gif""/></p>
");
		}
		#endregion

		#region PropertyBehavior
		[Test] public void PropertyBehavior()
		{
			FormatTest(@"@@Property(""TopicWithColor"", ""Color"")@@", @"<p>Yellow</p>
");
		}
		#endregion

		#region WikiURIForTopicProperty
		[Test] public void WikiURIForTopicProperty()
		{
			FormatTest(@"wiki://TopicWithColor/#Color", @"<p>Yellow</p>
");
		}
		#endregion

		#region NestedWikiSpec
		[Test] public void NestedWikiSpec()
		{
			FormatTest(@"		{{IncludeNest}}", @"<h5>hey there</h5>
<h6>black dog</h6>
");
		}
		#endregion
 
		#region IncludeFailure
		[Test] public void IncludeFailure()
		{
			FormatTest(@"{{NoSuchTopic}}", 
				@"<p>{{<a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("NoSuchTopic")) + @""">NoSuchTopic</a>}}</p>
");

		}
		#endregion

		#region SimpleWikiSpec
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
		#endregion

		#region PlainInOut
		[Test] public void PlainInOut()
		{
			FormatTest("Hello there", "<p>Hello there</p>\n");
		}
		#endregion
 
		#region RelabelTests
		[Test] public void RelabelTests()
		{
			string s = FormattedTestText(@"""tell me about dogs"":BigDog");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigDog")) + @""">tell me about dogs</a>");
		}
		#endregion

		#region TopicNameRegexTests
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
		#endregion

		#region NonAsciiTopicNameRegexTest
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
		#endregion

		#region TopicNameWithAnchorRegexTests
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
		#endregion

		#region NonAsciiTopicNameWithAnchorRegexTest
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
		#endregion

		#region RealTopicWithAnchorTest
		[Test] public void RealTopicWithAnchorTest()
		{
			FormatTestContains(
				"HomePage#Anchor",
				"href=\"" + _lm.LinkToTopic(_cb.TopicNameFor("HomePage")) + "#Anchor\">HomePage#Anchor</a>");
		}
		#endregion

		#region RealTopicWithAnchorRelabelTest
		[Test] public void RealTopicWithAnchorRelabelTest()
		{
			FormatTestContains(
				"\"Relabel\":HomePage#Anchor",
				"href=\"" + _lm.LinkToTopic(_cb.TopicNameFor("HomePage")) + "#Anchor\">Relabel</a>");
		}
		#endregion

		#region ComplexLinkTests
		[Test] public void ComplexLinkTests()
		{
			FormatTestContains(
				@"[_Underscore] +and+ *@@[""after""]@@*",
				_lm.LinkToTopic(_cb.TopicNameFor("_Underscore")));
			FormatTestContains(
				@"[_Underscore] +and+ after@Google",
				_lm.LinkToTopic(_cb.TopicNameFor("_Underscore")));
		}
		#endregion

		#region SimpleLinkTests
		[Test] public void SimpleLinkTests()
		{
			FormatTest(
				@"Some VerySimpleLinksFatPig
StartOfLineShouldLink blah blah blah
blah blah EndOfLineShouldLink",
				@"<p>Some <a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("VerySimpleLinksFatPig")) + @""">VerySimpleLinksFatPig</a></p>
<p><a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("StartOfLineShouldLink")) + @""">StartOfLineShouldLink</a> blah blah blah</p>
<p>blah blah <a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("EndOfLineShouldLink")) + @""">EndOfLineShouldLink</a></p>
");
		}
		#endregion

		#region SimpleEscapeTest
		[Test] public void SimpleEscapeTest()
		{
			FormatTest(@"-deleted """"HelloWorld"""" text-",
				@"<p><del>deleted HelloWorld text</del></p>
");
			FormatTest(@"'''Hello """"MrWorld"""" World'''", @"<p><strong>Hello MrWorld World</strong></p>
");
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
		#endregion

		#region LinkAfterBangTests
		[Test] public void LinkAfterBangTests()
		{
			FormatTest(
				@"!HelloWorld",
				@"<h1><a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("HelloWorld")) + @""">HelloWorld</a></h1>

");
		}
		#endregion

		#region ExternalLinks
		[Test] public void ExternalLinks()
		{
			FormatTest(
				@"A ""xxxx FooBar 0x(ToolTipText)"":http://localhost link.",
				@"<p>A <a class=""externalLink"" title=""ToolTipText"" href=""http://localhost"">xxxx FooBar 0x</a> link.</p>
");
			FormatTest(
				@"A ""xxxx FooBar 0x(ToolTipText) "":http://localhost link.",
				@"<p>A <a class=""externalLink"" href=""http://localhost"">xxxx FooBar 0x(ToolTipText)</a> link.</p>
");			FormatTest(
				@"A ""xxxx FooBar 0x"":http://localhost link.",
				@"<p>A <a class=""externalLink"" href=""http://localhost"">xxxx FooBar 0x</a> link.</p>
");

			FormatTestContains(
				@"A ""xxxx FooBar 01"":HomePage link",
				@"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("HomePage")) + @""">xxxx FooBar 01</a> link");

			FormatTestContains(
				@"A ""MyHomePage"":HomePage2 topic",
				@"href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("HomePage2")) + @""">MyHomePage</a> topic");

			FormatTest(
				@"A ""xxxx FooBar 1x"":[http://server1], ""xxxx FooBar 11"":[http://server2] link.",
				@"<p>A <a class=""externalLink"" href=""http://server1"">xxxx FooBar 1x</a>, <a class=""externalLink"" href=""http://server2"">xxxx FooBar 11</a> link.</p>
");

			FormatTest(
				@"My ""xxxx FooBar 2x"":[https://www.flexwiki.com] link",
				@"<p>My <a class=""externalLink"" href=""https://www.flexwiki.com"">xxxx FooBar 2x</a> link</p>
");
			
			FormatTest(
				@"""xxxx FooBar 21"":[notes:\\servername]",
				@"<p><a class=""externalLink"" href=""notes:\\servername"">xxxx FooBar 21</a></p>
");

			FormatTest(
				@"""xxxx FooBar 23"":[file:\\xyz\asdf.doc]",
				@"<p><a class=""externalLink"" href=""file:\\xyz\asdf.doc"">xxxx FooBar 23</a></p>
");

			FormatTest(
				@"""xxxx FooBar 24"":[mailto:info@flexwiki.com]",
				@"<p><a class=""externalLink"" href=""mailto:info@flexwiki.com"">xxxx FooBar 24</a></p>
");
		
			FormatTest(
				@"""xxxx FooBar 25"":mailto:info@flexwiki.com",
				@"<p><a class=""externalLink"" href=""mailto:info@flexwiki.com"">xxxx FooBar 25</a></p>
");

			FormatTest(
				@"mailto:info@flexwiki.com",
				@"<p><a class=""externalLink"" href=""mailto:info@flexwiki.com"">mailto:info@flexwiki.com</a></p>
");


			FormatTest(
				@"""xxxx FooBar 3x"":[notes://Server/file.nsf/viename?openView]",
				@"<p><a class=""externalLink"" href=""notes://Server/file.nsf/viename?openView"">xxxx FooBar 3x</a></p>
");

			FormatTest(
				@"""asfasdf asdasfd (asdf) asdf"":http://localhost",
				@"<p><a class=""externalLink"" href=""http://localhost"">asfasdf asdasfd (asdf) asdf</a></p>
");
		}

		#endregion
		#region SpaceInLinks
		[Test] public void SpaceInLinks()
		{
			FormatTestContains(
				@"A [test test] new topic",
				@"href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("test test")) + @""">test test</a> new topic");

			FormatTestContains(
				@"A ""NewTest"":[test test] new topic",
				@"href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("test test")) + @""">NewTest</a> new topic");

			//			FormatTest(
			//				@"""Spacelink"":http://localhost/space in link/test.htm",
			//				@"<p><a class=""externalLink"" href=""http://localhost/space%20in%20link/test.htm"">Spacelink</a></p>
			//");

			FormatTest(
				@"""Correct link"":[http://localhost/link to a page/test.htm]",
				@"<p><a class=""externalLink"" href=""http://localhost/link%20to%20a%20page/test.htm"">Correct link</a></p>
");
			FormatTest(
				@"[http://localhost/link to a page/test2.htm]",
				@"<p><a class=""externalLink"" href=""http://localhost/link%20to%20a%20page/test2.htm"">http://localhost/link to a page/test2.htm</a></p>
");
			FormatTest(
				@"[http://localhost/link to a page/test3.htm] and [http://localhost/link to a page/test4.htm]",
				@"<p><a class=""externalLink"" href=""http://localhost/link%20to%20a%20page/test3.htm"">http://localhost/link to a page/test3.htm</a> and <a class=""externalLink"" href=""http://localhost/link%20to%20a%20page/test4.htm"">http://localhost/link to a page/test4.htm</a></p>
");

		}
		#endregion
		#region NumberInTopicName
		[Test] public void NumberInTopicName()
		{
			FormatTest(
				@"This is Some2Link to a topic.",
				@"<p>This is <a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("Some2Link")) + @""">Some2Link</a> to a topic.</p>
");
		}
		#endregion

		#region PluralLinkTests
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
		#endregion

		#region Rule
		[Test] public void Rule()
		{
			FormatTestContains("----", "<div class=\"Rule\"></div>");
			FormatTestContains("------------------------", "<div class=\"Rule\"></div>");
		}
		#endregion

		#region HeadingTests
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
		#endregion

		#region ListTests
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
		#endregion

		#region UnderEmphasis
		[Test] public void UnderEmphasis()
		{
			FormatTest(
				@"This should be _emphasised_ however, id_Foo and id_Bar should not be.",
				@"<p>This should be <em>emphasised</em> however, id_Foo and id_Bar should not be.</p>
");
		}
		#endregion

		#region InlineExternalReference
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
		#endregion

		#region UnusualCharactersInLinkTest
		[Test] public void UnusualCharactersInLinkTest()
		{
			FormatTest(
				@"some leading text http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm some trailing text",
				@"<p>some leading text <a class=""externalLink"" href=""http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm"">http://www.lemonde.fr/web/article/0,1-0@2-3212,36-358279,0.htm</a> some trailing text</p>
");
		}
		#endregion

		#region HyphensInImageLinkTest
		[Test] public void HyphensInImageLinkTest()
		{
			FormatTest(
				@"some leading text http://msdn.microsoft.com/library/en-us/dnpag/html/ch01---engineering-for-perf.gif some trailing text",
				@"<p>some leading text <img src=""http://msdn.microsoft.com/library/en-us/dnpag/html/ch01---engineering-for-perf.gif""/> some trailing text</p>
");
		}
		#endregion

		#region TextileLinkTest
		[Test] public void TextileLinkTest()
		{
			FormatTest(@"RowDataGateway http://www.google.com", @"<p><a title=""Click here to create this topic"" class=""create"" href=""/formattingtestswiki/WikiEdit.aspx?topic=FlexWiki.RowDataGateway"">RowDataGateway</a> <a class=""externalLink"" href=""http://www.google.com"">http://www.google.com</a></p>
");
			FormatTest(@"RowDataGateway ""Google"":http://www.google.com", @"<p><a title=""Click here to create this topic"" class=""create"" href=""/formattingtestswiki/WikiEdit.aspx?topic=FlexWiki.RowDataGateway"">RowDataGateway</a> <a class=""externalLink"" href=""http://www.google.com"">Google</a></p>
");
		}
		#endregion

		#region TextileHyphensInLinkTest
		[Test] public void TextileHyphensInLinkTest()
		{
			FormatTest(
				@"some leading text ""textile"":http://www.amazon.com/exec/obidos/tg/detail/-/0735617228/qid=1080277573/sr=8-1/ref=pd_ka_1/102-9825269-6457731?v=glance&s=books&n=50784 some trailing text",
				@"<p>some leading text <a class=""externalLink"" href=""http://www.amazon.com/exec/obidos/tg/detail/-/0735617228/qid=1080277573/sr=8-1/ref=pd_ka_1/102-9825269-6457731?v=glance&s=books&n=50784"">textile</a> some trailing text</p>
");
		}
		#endregion

		#region SpaceInHyperlinkTextTest
		[Test] public void SpaceInHyperlinkTextTest()
		{
			FormatTest(
				@"My ""xxx FooBar xxx"":[http://www.flexwiki.com/FooBar/default.aspx] link.",
				@"<p>My <a class=""externalLink"" href=""http://www.flexwiki.com/FooBar/default.aspx"">xxx FooBar xxx</a> link.</p>
");
			FormatTest(
				@"My ""xxx FooBar xxx"":http://www.flexwiki.com/FooBar/default.aspx link.",
				@"<p>My <a class=""externalLink"" href=""http://www.flexwiki.com/FooBar/default.aspx"">xxx FooBar xxx</a> link.</p>
");
		}
		#endregion

		#region DollarInLinkTest
		[Test] public void DollarInLinkTest()
		{
			FormatTest(
				@"""Main directory"":file://servername/umuff$/folder%20name/file.txt
""Main directory"":[file://servername/umuff$/folder%20name/file.txt]
file://servername/umuff$/folder%20name/file.txt",
				@"<p><a class=""externalLink"" href=""file://servername/umuff$/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff$/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff$/folder%20name/file.txt"">file://servername/umuff$/folder%20name/file.txt</a></p>
");
		}
		#endregion

		#region TildeInLinkTest
		[Test] public void TildeInLinkTest()
		{
			FormatTest(
				@"""Main directory"":file://servername/umuff~/folder%20name/file.txt
""Main directory"":[file://servername/umuff~/folder%20name/file.txt]
file://servername/umuff~/folder%20name/file.txt",
				@"<p><a class=""externalLink"" href=""file://servername/umuff~/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff~/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff~/folder%20name/file.txt"">file://servername/umuff~/folder%20name/file.txt</a></p>
");
		}
		#endregion

		#region EmphasisInLinkTest
		//[Ignore("This test shows an bug in link. Should be fixed in next patch. --ChristianMetz")]
		[Test] public void EmphasisInLinkTest()
		{
			FormatTest(
				@"""_Main directory_"":[file://servername/_default/orange_futurism/file.txt]",
				@"<p><a class=""externalLink"" href=""file://servername/_default/orange_futurism/file.txt""><em>Main directory</em></a></p>
");
			FormatTest(
				@"http://localhost/_default/orange_futurism/portal.jpg",
				@"<p><img src=""http://localhost/_default/orange_futurism/portal.jpg""/></p>
");

			FormatTest(
				@"""Main directory"":file://servername/_default/orange_futurism/file.txt",
				@"<p><a class=""externalLink"" href=""file://servername/_default/orange_futurism/file.txt"">Main directory</a></p>
");
			FormatTest(
				@"""Main directory"":[file://servername/_default/orange_futurism/file.txt]",
				@"<p><a class=""externalLink"" href=""file://servername/_default/orange_futurism/file.txt"">Main directory</a></p>
");
			FormatTest(
				@"file://servername/_default/orange_futurism/file.txt",
				@"<p><a class=""externalLink"" href=""file://servername/_default/orange_futurism/file.txt"">file://servername/_default/orange_futurism/file.txt</a></p>
");
		}
		

		#endregion

		#region Hash126InLink
		[Test] public void Hash126InLink()
		{
			FormatTest(
				@"""Main directory"":file://servername/umuff&#126;/folder%20name/file.txt
""Main directory"":[file://servername/umuff&#126;/folder%20name/file.txt]
file://servername/umuff&#126;/folder%20name/file.txt",
				@"<p><a class=""externalLink"" href=""file://servername/umuff&#126;/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff&#126;/folder%20name/file.txt"">Main directory</a></p>
<p><a class=""externalLink"" href=""file://servername/umuff&#126;/folder%20name/file.txt"">file://servername/umuff&#126;/folder%20name/file.txt</a></p>
");
		}
		#endregion

		#region MailToLink
		[Test] public void MailToLink()
		{
			FormatTest(
				@"Please send mailto:person@domain.com some email!",
				@"<p>Please send <a class=""externalLink"" href=""mailto:person@domain.com"">mailto:person@domain.com</a> some email!</p>
");

			FormatTest(
				@"Please send ""person"":mailto:person@domain.com some email!",
				@"<p>Please send <a class=""externalLink"" href=""mailto:person@domain.com"">person</a> some email!</p>
");
			FormatTest(
				@"Please send ""person"":[mailto:person@domain.com] some email!",
				@"<p>Please send <a class=""externalLink"" href=""mailto:person@domain.com"">person</a> some email!</p>
");
		}
		#endregion

		#region BasicWikinames
		[Test] public void BasicWikinames()
		{
			FormatTest(
				@"LinkThis, AndLinkThis, dontLinkThis, (LinkThis), _LinkAndEmphasisThis_ *LinkAndBold* (LinkThisOneToo)",
				@"<p><a title=""Click here to create this topic"" class=""create"" href=""" 
					+ _lm.LinkToEditTopic(_cb.TopicNameFor("LinkThis")) 
					+ @""">LinkThis</a>, <a title=""Click here to create this topic"" class=""create"" href=""" 
					+ _lm.LinkToEditTopic(_cb.TopicNameFor("AndLinkThis")) 
					+ @""">AndLinkThis</a>, dontLinkThis, (<a title=""Click here to create this topic"" class=""create"" href=""" 
					+ _lm.LinkToEditTopic(_cb.TopicNameFor("LinkThis")) 
					+ @""">LinkThis</a>), <em><a title=""Click here to create this topic"" class=""create"" href="""
					+ _lm.LinkToEditTopic(_cb.TopicNameFor("LinkAndEmphasisThis")) 
					+ @""">LinkAndEmphasisThis</a></em> <strong><a title=""Click here to create this topic"" class=""create"" href="""
					+ _lm.LinkToEditTopic(_cb.TopicNameFor("LinkAndBold"))
					+ @""">LinkAndBold</a></strong> (<a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("LinkThisOneToo")) + @""">LinkThisOneToo</a>)</p>
");
		}
		#endregion

		#region PreNoformatting
		[Test] public void PreNoformatting()
		{
			FormatTest(
				@" :-) bold '''not''' BigDog    _foo_",
				@"<pre>
 :-) bold '''not''' BigDog    _foo_
</pre>
");			
		}
		#endregion

		#region ExtendedPreFormatting
		[Test]
		public void ExtendedPreFormatting()
		{
			FormatTest(
				@"{+
%blue%public void%% '''Foo'''(%blue%string%% bar);
}+",
				@"<pre>
<span style=""color:blue"">public void</span> <strong>Foo</strong>(<span style=""color:blue"">string</span> bar);
</pre>
");
		}
		#endregion

		#region TableTests
		[Test] public void TableTests()
		{
			FormatTest(
				@"||",
				@"<p>||</p>
");

			FormatTest(
				@"||t1||",
				@"<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell"">t1</td>
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
				@"<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell""><em>table</em></td>
<td  class=""TableCell""><strong>more</strong></td>
<td  class=""TableCell"">columns</td>
</tr>
<tr>
<td  class=""TableCell"">1</td>
<td  class=""TableCell"">2</td>
<td  class=""TableCell"">3</td>
</tr>
</table>
");
		}
		#endregion

		#region EmoticonInTableTest
		[Test] public void EmoticonInTableTest()
		{
			FormatTest(
				@"||:-)||",
				@"<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell""><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""/></td>
</tr>
</table>
");
		}
		#endregion

		#region WikinameInTableTest
		[Test] public void WikinameInTableTest()
		{
			string s = FormattedTestText(@"||BigDog||");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigDog")) + @""">BigDog</a>");
		}
		#endregion

		#region HyperlinkInTableTest
		[Test] public void HyperlinkInTableTest()
		{
			FormatTest(
				@"||http://www.yahoo.com/foo.html||",
				@"<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell""><a class=""externalLink"" href=""http://www.yahoo.com/foo.html"">http://www.yahoo.com/foo.html</a></td>
</tr>
</table>
");
		}
		#endregion

		#region EmoticonTest
		[Test] public void EmoticonTest()
		{
			FormatTest(
				@":-) :-(",
				@"<p><img src=""" + _lm.LinkToImage("emoticons/regular_smile.gif") + @"""/> <img src=""" + _lm.LinkToImage("emoticons/sad_smile.gif") + @"""/></p>
");
		}
		#endregion

		#region BracketedLinks
		[Test] public void BracketedLinks()
		{
			string s = FormattedTestText(@"[BigBox] summer [eDocuments] and [e] and [HelloWorld] and [aZero123]");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("BigBox")) + @""">BigBox</a>");
			AssertStringContains(s, @"<a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("eDocuments")) + @""">eDocuments</a> and <a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("e")) + @""">e</a> and <a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("HelloWorld")) + @""">HelloWorld</a> and <a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor("aZero123")) + @""">aZero123</a>");
		}
		#endregion

		#region CodeFormatting
		[Test] public void CodeFormatting()
		{
			FormatTest("@Namespace.Class@", @"<p><code>Namespace.Class</code></p>
");
			FormatTest("@Name.Space.Class@", @"<p><code>Name.Space.Class</code></p>
");
			FormatTest(
				@"These values should be code formatted, not external Wiki references @""""IObjectWithSite""""@ and @""""IViewObject""""@. This should also be (@code formatted@) inside of the parens, and @""""PropertyManager.RegisterProperty""""@ should work...",
				@"<p>These values should be code formatted, not external Wiki references <code>IObjectWithSite</code> and <code>IViewObject</code>. This should also be (<code>code formatted</code>) inside of the parens, and <code>PropertyManager.RegisterProperty</code> should work...</p>
");
			FormatTest(
				@"These values should be Wiki references and code formatted, @[IObjectWithSite]@ and @[IViewObject]@. This should also be (@code formatted@) inside of the parens, and @PropertyManager.RegisterProperty@ should work...",
				@"<p>These values should be Wiki references and code formatted, <code><a title=""Click here to create this topic"" class=""create"" href=""/formattingtestswiki/WikiEdit.aspx?topic=FlexWiki.IObjectWithSite"">IObjectWithSite</a></code> and <code><a title=""Click here to create this topic"" class=""create"" href=""/formattingtestswiki/WikiEdit.aspx?topic=FlexWiki.IViewObject"">IViewObject</a></code>. This should also be (<code>code formatted</code>) inside of the parens, and <code>PropertyManager.RegisterProperty</code> should work...</p>
");
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
		#endregion

		#region ImageLink
		[Test] public void ImageLink()
		{
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.jpg",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.jpg""/></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.JPG",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.JPG""/></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.png",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.png""/></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.PNG",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.PNG""/></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.gif",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.gif""/></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/SOFA-BILL.GIF",
				@"<p><img src=""http://www.microsoft.com/billgates/images/SOFA-BILL.GIF""/></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/sofa-bill.jpeg",
				@"<p><img src=""http://www.microsoft.com/billgates/images/sofa-bill.jpeg""/></p>
");
			FormatTest(
				@"http://www.microsoft.com/billgates/images/SOFA-BILL.JPEG",
				@"<p><img src=""http://www.microsoft.com/billgates/images/SOFA-BILL.JPEG""/></p>
");
			// Make sure we really need the period before the trigger extensions...
			// Look for a hyperlink, not an <img>
			FormatTestContains(
				@"http://www.microsoft.com/billgates/images/sofa-billjpeg",
				@"<a class=""externalLink"" href");
		}

		[Test] public void HttpsImageLink()
		{
			FormatTest(
				@"https://www.microsoft.com/billgates/images/sofa-bill.jpg",
				@"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.jpg""/></p>
");
			FormatTest(
				@"https://www.microsoft.com/billgates/images/sofa-bill.JPG",
				@"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.JPG""/></p>
");
			FormatTest(
				@"https://www.microsoft.com/billgates/images/sofa-bill.png",
				@"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.png""/></p>
");
			FormatTest(
				@"https://www.microsoft.com/billgates/images/sofa-bill.PNG",
				@"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.PNG""/></p>
");
			FormatTest(
				@"https://www.microsoft.com/billgates/images/sofa-bill.gif",
				@"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.gif""/></p>
");
			FormatTest(
				@"https://www.microsoft.com/billgates/images/SOFA-BILL.GIF",
				@"<p><img src=""https://www.microsoft.com/billgates/images/SOFA-BILL.GIF""/></p>
");
			FormatTest(
				@"https://www.microsoft.com/billgates/images/sofa-bill.jpeg",
				@"<p><img src=""https://www.microsoft.com/billgates/images/sofa-bill.jpeg""/></p>
");
			FormatTest(
				@"https://www.microsoft.com/billgates/images/SOFA-BILL.JPEG",
				@"<p><img src=""https://www.microsoft.com/billgates/images/SOFA-BILL.JPEG""/></p>
");
			// Make sure we really need the period before the trigger extensions...
			// Look for a hyperlink, not an <img>
			FormatTestContains(
				@"https://www.microsoft.com/billgates/images/sofa-billjpeg",
				@"<a class=""externalLink"" href");
		}

		#endregion

		#region FileLinks
		[Test] public void FileLinks()
		{
			FormatTest(
				@"http://www.microsoft.com/download/word.doc",
				@"<p><a class=""externalLink"" href=""http://www.microsoft.com/download/word.doc"">http://www.microsoft.com/download/word.doc</a></p>
");

			FormatTest(
				@"http://www.microsoft.com/download/excel.xls",
				@"<p><a class=""externalLink"" href=""http://www.microsoft.com/download/excel.xls"">http://www.microsoft.com/download/excel.xls</a></p>
");

			FormatTest(
				@"http://www.microsoft.com/download/powerpoint.ppt",
				@"<p><a class=""externalLink"" href=""http://www.microsoft.com/download/powerpoint.ppt"">http://www.microsoft.com/download/powerpoint.ppt</a></p>
");

			FormatTest(
				@"file:\\server\share\directory\mydoc.doc",
				@"<p><a class=""externalLink"" href=""file:\\server\share\directory\mydoc.doc"">file:\\server\share\directory\mydoc.doc</a></p>
");

			FormatTest(
				@"file://server\share\directory\mydoc1.jpg",
				@"<p><a class=""externalLink"" href=""file://server\share\directory\mydoc1.jpg"">file://server\share\directory\mydoc1.jpg</a></p>
");

 			FormatTest(
				@"test file:\\server\share\directory\mydoc.doc",
				@"<p>test <a class=""externalLink"" href=""file:\\server\share\directory\mydoc.doc"">file:\\server\share\directory\mydoc.doc</a></p>
");

			FormatTest(
				@"file:\\server\share\directory\mydoc2.doc",
				@"<p><a class=""externalLink"" href=""file:\\server\share\directory\mydoc2.doc"">file:\\server\share\directory\mydoc2.doc</a></p>
");

			FormatTest(
				@"file://server\share\directory\mydoc3.jpg",
				@"<p><a class=""externalLink"" href=""file://server\share\directory\mydoc3.jpg"">file://server\share\directory\mydoc3.jpg</a></p>
");

			FormatTest(
				@"notes://Server/file.nsf/viename?openView",
				@"<p><a class=""externalLink"" href=""notes://Server/file.nsf/viename?openView"">notes://Server/file.nsf/viename?openView</a></p>
");

			FormatTest(
				@"ms-help:\\Server\file.nsf\viename?openView",
				@"<p><a class=""externalLink"" href=""ms-help:\\Server\file.nsf\viename?openView"">ms-help:\\Server\file.nsf\viename?openView</a></p>
");
		}
		#endregion

		#region BehaviorTest
		[Test] public void BehaviorTest()
		{
			string s = FormattedTestText(@"@@ProductName@@");
			AssertStringContains(s, @"href=""" + _lm.LinkToTopic(_cb.TopicNameFor("FlexWiki")) + @""">FlexWiki</a>");
		}
		#endregion

		#region BehaviorWithLineBreak
		[Test] public void BehaviorWithLineBreak()
		{
			string s = FormattedTestText(@"@@[100, 200
, 
300]@@");
			AssertStringContains(s, @"100200300");
		}
		#endregion

		#region ImageBehaviorTwoParamTest
		[Test] public void ImageBehaviorTwoParamTest() 
		{
			FormatTest("@@Image(\"http://server/image.jpg\", \"Alternative text\")@@",
				"<p><img src=\"http://server/image.jpg\" alt=\"Alternative text\"/></p>\n");
		}
		#endregion

		#region ImageBehaviorFourParamTest
		[Test] public void ImageBehaviorFourParamTest() 
		{
			FormatTest("@@Image(\"http://server/image.jpg\", \"Alternative text\", \"500\", \"400\")@@",
				"<p><img src=\"http://server/image.jpg\" alt=\"Alternative text\" " +
				"width=\"500\" height=\"400\"/></p>\n");
		}
		#endregion

		#region ImageBehaviorEmbeddedQuotationMarks
		[Test] public void ImageBehaviorEmbeddedQuotationMarks() 
		{
			FormatTest(@"@@Image(""http://server/image.jpg"", ""Alt \""text\"""")@@",
				"<p><img src=\"http://server/image.jpg\" alt=\"Alt &quot;text&quot;\"/></p>\n");
		}
		#endregion

		#region ImageBehaviorTwoPerLineTest
		[Test] public void ImageBehaviorTwoPerLineTest()
		{
			FormatTest("@@Image(\"http://server/image.jpg\", \"alt\")@@ and @@Image(\"http://server/image2.jpg\", \"alt2\")@@",
				"<p><img src=\"http://server/image.jpg\" alt=\"alt\"/> and <img src=\"http://server/image2.jpg\" alt=\"alt2\"/></p>\n");
		}
		#endregion

		#region XmlTransformBehaviorTwoParamTest
		[Test] public void XmlTransformBehaviorTwoParamTest() 
		{
			// Need to escape all the backslashes in the path
			string xmlPath = testRssXmlPath.Replace(@"\", @"\\");
			string xslPath = testRssXslPath.Replace(@"\", @"\\");

			FormatTest("@@XmlTransform(\"" + xmlPath + "\", \"" + xslPath + "\")@@",
				@"<p><h1>Weblogs @ ASP.NET</h1>

<table cellpadding=""2"" cellspacing=""1"" class=""TableClass"">
<tr>
<td  class=""TableCell""><strong>Published Date</strong></td>
<td  class=""TableCell""><strong>Title</strong></td>
</tr>
<tr>
<td  class=""TableCell"">Wed, 07 Jan 2004 05:45:00 GMT</td>
<td  class=""TableCell""><a class=""externalLink"" href=""http://weblogs.asp.net/aconrad/archive/2004/01/06/48205.aspx"">Fast Chicken</a></td>
</tr>
<tr>
<td  class=""TableCell"">Wed, 07 Jan 2004 03:36:00 GMT</td>
<td  class=""TableCell""><a class=""externalLink"" href=""http://weblogs.asp.net/CSchittko/archive/2004/01/06/48178.aspx"">Are You Linked In?</a></td>
</tr>
<tr>
<td  class=""TableCell"">Wed, 07 Jan 2004 03:27:00 GMT</td>
<td  class=""TableCell""><a class=""externalLink"" href=""http://weblogs.asp.net/francip/archive/2004/01/06/48172.aspx"">Whidbey configuration APIs</a></td>
</tr>
</table></p>
");
		}
		#endregion

		#region XmlTransformBehaviorXmlParamNotFoundTest
		[Test] public void XmlTransformBehaviorXmlParamNotFoundTest() 
		{
			FormatTestContains("@@XmlTransform(\"file://noWayThisExists\", \"Alternative text\")@@",
				"Failed to load XML parameter");
		}
		#endregion

		#region XmlTransformBehaviorXslParamNotFoundTest
		[Ignore("This test fails on the build machine. Not sure why, but it's blocking deployment of CruiseControl, so I'll come back to it later. --CraigAndera")]
		[Test] public void XmlTransformBehaviorXslParamNotFoundTest() 
		{
			// Go against just the filename: the full path screws up the build machine
			string xmlPath = Path.GetFileName(meerkatRssPath);

			FormatTestContains("@@XmlTransform(\"" + xmlPath + "\", \"file://noWayThisExists\")@@",
				"Failed to load XSL parameter");
		}
		#endregion

		#region BracketedHyperLinks
		[Test] public void BracketedHyperLinks()
		{
			FormatTest(
				@"(http://www.msn.com) (http://www.yahoo.com) (http://www.yahoo.com)",
				@"<p>(<a class=""externalLink"" href=""http://www.msn.com"">http://www.msn.com</a>) (<a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a>) (<a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a>)</p>
");
			FormatTest(
				@"[http://www.msn.com] [http://www.yahoo.com] [http://www.yahoo.com]",
				@"<p><a class=""externalLink"" href=""http://www.msn.com"">http://www.msn.com</a> <a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a> <a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a></p>
");
			FormatTest(
				@"{http://www.msn.com} {http://www.yahoo.com} {http://www.yahoo.com}",
				@"<p>{<a class=""externalLink"" href=""http://www.msn.com"">http://www.msn.com</a>} {<a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a>} {<a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a>}</p>
");
		}
		#endregion

		#region BasicHyperLinks
		[Test] public void BasicHyperLinks()
		{
			FormatTest(
				@"http://www.msn.com http://www.yahoo.com",
				@"<p><a class=""externalLink"" href=""http://www.msn.com"">http://www.msn.com</a> <a class=""externalLink"" href=""http://www.yahoo.com"">http://www.yahoo.com</a></p>
");
			FormatTest(
				@"ftp://feeds.scripting.com",
				@"<p><a class=""externalLink"" href=""ftp://feeds.scripting.com"">ftp://feeds.scripting.com</a></p>
");
			FormatTest(
				@"gopher://feeds.scripting.com",
				@"<p><a class=""externalLink"" href=""gopher://feeds.scripting.com"">gopher://feeds.scripting.com</a></p>
");
			FormatTest(
				@"telnet://melvyl.ucop.edu/",
				@"<p><a class=""externalLink"" href=""telnet://melvyl.ucop.edu/"">telnet://melvyl.ucop.edu/</a></p>
");
			FormatTest(
				@"news:comp.infosystems.www.servers.unix",
				@"<p><a class=""externalLink"" href=""news:comp.infosystems.www.servers.unix"">news:comp.infosystems.www.servers.unix</a></p>
");
			FormatTest(
				@"https://server/directory",
				@"<p><a class=""externalLink"" href=""https://server/directory"">https://server/directory</a></p>
");
			FormatTest(
				@"http://www.msn:8080/ http://www.msn:8080",
				@"<p><a class=""externalLink"" href=""http://www.msn:8080/"">http://www.msn:8080/</a> <a class=""externalLink"" href=""http://www.msn:8080"">http://www.msn:8080</a></p>
");
			FormatTest(
				@"notes://server/directory",
				@"<p><a class=""externalLink"" href=""notes://server/directory"">notes://server/directory</a></p>
");
			FormatTest(
				@"ms-help://server/directory",
				@"<p><a class=""externalLink"" href=""ms-help://server/directory"">ms-help://server/directory</a></p>
");			
		}
		#endregion

		#region NamedHyperLinks
		[Test] public void NamedHyperLinks()
		{
			FormatTest(
				@"""msn"":http://www.msn.com ""yahoo"":http://www.yahoo.com",
				@"<p><a class=""externalLink"" href=""http://www.msn.com"">msn</a> <a class=""externalLink"" href=""http://www.yahoo.com"">yahoo</a></p>
");
			FormatTest(
				@"""ftp link"":ftp://feeds.scripting.com",
				@"<p><a class=""externalLink"" href=""ftp://feeds.scripting.com"">ftp link</a></p>
");
			FormatTest(
				@"""gopher link"":gopher://feeds.scripting.com",
				@"<p><a class=""externalLink"" href=""gopher://feeds.scripting.com"">gopher link</a></p>
");
			FormatTest(
				@"""telnet link"":telnet://melvyl.ucop.edu/",
				@"<p><a class=""externalLink"" href=""telnet://melvyl.ucop.edu/"">telnet link</a></p>
");
			FormatTest(
				@"""news group link"":news:comp.infosystems.www.servers.unix",
				@"<p><a class=""externalLink"" href=""news:comp.infosystems.www.servers.unix"">news group link</a></p>
");
			FormatTest(
				@"""secure link"":https://server/directory",
				@"<p><a class=""externalLink"" href=""https://server/directory"">secure link</a></p>
");
			FormatTest(
				@"""port link"":http://www.msn:8080/ ""port link"":http://www.msn:8080",
				@"<p><a class=""externalLink"" href=""http://www.msn:8080/"">port link</a> <a class=""externalLink"" href=""http://www.msn:8080"">port link</a></p>
");
			FormatTest(
				@"""notes link"":notes://server/directory",
				@"<p><a class=""externalLink"" href=""notes://server/directory"">notes link</a></p>
");
			FormatTest(
				@"""ms-help link"":ms-help://server/directory",
				@"<p><a class=""externalLink"" href=""ms-help://server/directory"">ms-help link</a></p>
");
		}
		#endregion

		#region NamedHyperLinksWithBrackets
		[Test] public void NamedHyperLinksWithBrackets()
		{
			FormatTest(
				@"""msn"":[http://www.msn.com] ""yahoo"":[http://www.yahoo.com]",
				@"<p><a class=""externalLink"" href=""http://www.msn.com"">msn</a> <a class=""externalLink"" href=""http://www.yahoo.com"">yahoo</a></p>
");
			FormatTest(
				@"""ftp link"":[ftp://feeds.scripting.com]",
				@"<p><a class=""externalLink"" href=""ftp://feeds.scripting.com"">ftp link</a></p>
");
			FormatTest(
				@"""gopher link"":[gopher://feeds.scripting.com]",
				@"<p><a class=""externalLink"" href=""gopher://feeds.scripting.com"">gopher link</a></p>
");
			FormatTest(
				@"""telnet link"":[telnet://melvyl.ucop.edu/]",
				@"<p><a class=""externalLink"" href=""telnet://melvyl.ucop.edu/"">telnet link</a></p>
");
			FormatTest(
				@"""news group link"":[news:comp.infosystems.www.servers.unix]",
				@"<p><a class=""externalLink"" href=""news:comp.infosystems.www.servers.unix"">news group link</a></p>
");
			FormatTest(
				@"""secure link"":[https://server/directory]",
				@"<p><a class=""externalLink"" href=""https://server/directory"">secure link</a></p>
");
			FormatTest(
				@"""port link"":[http://www.msn:8080/] ""port link"":[http://www.msn:8080]",
				@"<p><a class=""externalLink"" href=""http://www.msn:8080/"">port link</a> <a class=""externalLink"" href=""http://www.msn:8080"">port link</a></p>
");
			FormatTest(
				@"""notes link"":[notes://server/directory]",
				@"<p><a class=""externalLink"" href=""notes://server/directory"">notes link</a></p>
");
			FormatTest(
				@"""ms-help link"":[ms-help://server/directory]",
				@"<p><a class=""externalLink"" href=""ms-help://server/directory"">ms-help link</a></p>
");
		}
		#endregion

		#region PoundHyperLinks
		[Test] public void PoundHyperLinks()
		{
			FormatTest(
				@"http://www.msn.com#hello",
				@"<p><a class=""externalLink"" href=""http://www.msn.com#hello"">http://www.msn.com#hello</a></p>
");
			FormatTest(
				@"http://www.msn.com#hello",
				@"<p><a class=""externalLink"" href=""http://www.msn.com#hello"">http://www.msn.com#hello</a></p>
");
			FormatTest(
				@"ms-help://server/directory#hello",
				@"<p><a class=""externalLink"" href=""ms-help://server/directory#hello"">ms-help://server/directory#hello</a></p>
");
		}
		#endregion

		#region PlusSignHyperLinks
		[Test] public void PlusSignHyperLinks()
		{
			FormatTest(
				@"http://www.google.com/search?q=wiki+url+specification",
				@"<p><a class=""externalLink"" href=""http://www.google.com/search?q=wiki+url+specification"">http://www.google.com/search?q=wiki+url+specification</a></p>
");
		}
		#endregion

		#region PercentSignHyperLinks
		[Test] public void PercentSignHyperLinks()
		{
			FormatTest(
				@"file://server/directory/file%20GM%.doc",
				@"<p><a class=""externalLink"" href=""file://server/directory/file%20GM%.doc"">file://server/directory/file%20GM%.doc</a></p>
");
			FormatTest(
				@"""Sales 20% Markup"":file://server/directory/sales%2020%%20Markup.doc",
				@"<p><a class=""externalLink"" href=""file://server/directory/sales%2020%%20Markup.doc"">Sales 20% Markup</a></p>
");
		}
		#endregion

		#region DoNotConvertIntoLinks
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
			FormatTest(
				@"ms-help:",
				@"<p>ms-help:</p>
");
			FormatTest(
				@"ms-help",
				@"<p>ms-help</p>
");
			FormatTest(
				@"notes",
				@"<p>notes</p>
");
			FormatTest(
				@"notes:",
				@"<p>notes:</p>
");
		}
		#endregion

		#region ParensHyperLinks
		[Test] public void ParensHyperLinks()
		{
			FormatTest(
				@"file://servername/directory/File%20(1420).txt",
				@"<p><a class=""externalLink"" href=""file://servername/directory/File%20(1420).txt"">file://servername/directory/File%20(1420).txt</a></p>
");
		}
		#endregion

		#region SemicolonHyperLinks
		[Test] public void SemicolonHyperLinks()
		{
			FormatTest(
				@"http://servername/directory/File.html?test=1;test2=2",
				@"<p><a class=""externalLink"" href=""http://servername/directory/File.html?test=1;test2=2"">http://servername/directory/File.html?test=1;test2=2</a></p>
");
		}
		#endregion

		#region DollarSignHyperLinks
		[Test] public void DollarSignHyperLinks()
		{
			FormatTest(
				@"http://feeds.scripting.com/discuss/msgReader$4",
				@"<p><a class=""externalLink"" href=""http://feeds.scripting.com/discuss/msgReader$4"">http://feeds.scripting.com/discuss/msgReader$4</a></p>
");
			FormatTest(
				@"file://machine/user$/folder/file",
				@"<p><a class=""externalLink"" href=""file://machine/user$/folder/file"">file://machine/user$/folder/file</a></p>
");
		}
		#endregion

		#region TildeHyperLinks
		[Test] public void TildeHyperLinks()
		{
			// Collides with textile subscript markup
			FormatTest(
				@"""TildeLink"":http://servername/~mike",
				@"<p><a class=""externalLink"" href=""http://servername/~mike"">TildeLink</a></p>
");
			FormatTest(
				@"http://servername/~mike",
				@"<p><a class=""externalLink"" href=""http://servername/~mike"">http://servername/~mike</a></p>
");
		}
		#endregion
	
		#region TextileFormat
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
				@"AB -CD- -EF- GH -IJ-",
				@"<p>AB <del>CD</del> <del>EF</del> GH <del>IJ</del></p>
");
			FormatTest(
				@"-deleted text- undeleted text -more deleted text-",
				@"<p><del>deleted text</del> undeleted text <del>more deleted text</del></p>
");
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
		#endregion

		#region WikiSignatureWithHyphenDates
		[Test] public void WikiSignatureWithHyphenDates()
		{
			FormatTest(
				@"Test comment. -- Derek Lakin [28-Jan-2005]",
				@"<p>Test comment. -- Derek Lakin [28-Jan-2005]</p>
");
		}
		#endregion

		#region MultipleParametersHyperLinks
		[Test] public void MultipleParametersHyperLinks()
		{
			// This test verifies the & sign can work in a URL
			FormatTest(
				@"http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8",
				@"<p><a class=""externalLink"" href=""http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8"">http://www.google.com/search?sourceid=navclient&ie=UTF-8&oe=UTF-8</a></p>
");

		}
		#endregion

		#region Ambersand
		[Test] public void Ambersand()
		{
			// Since & sign is not a valid html character also veryify that the & sign is correct when it is not in a URL
			FormatTest(
				@"this test should make the & sign a freindly HTML element",
				@"<p>this test should make the &amp; sign a freindly HTML element</p>
");
		}
		#endregion

		#region ListAfterPreTest
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
		#endregion

		#region PreTest
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
		#endregion

		#region PreFormattedBlockTests
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
		#endregion 

		#region ColorAndTextSizeTests
		[Test] public void ColorAndTextSizeTests()
		{
			FormatTest(
				@"%% by itself %% and again %%",
				@"<p>%% by itself %% and again %%</p>
");
			FormatTest(
				@"%red% red %% and now %% by itself",
				@"<p><span style=""color:red""> red </span> and now %% by itself</p>
");
			FormatTest(
				@"%red%red %blue%blue%% but this text is normal.",
				@"<p><span style=""color:red"">red </span><span style=""color:blue"">blue</span> but this text is normal.</p>
");
			FormatTest(
				@"%red%Red and no %big%closing %small%percentpercent
Normal again
",
				@"<p><span style=""color:red"">Red and no </span><big>closing </big><small>percentpercent</small></p>
<p>Normal again</p>
");
			FormatTest(
				@"Strange %#13579B%Color %dima% non-color",
				@"<p>Strange <span style=""color:#13579b"">Color </span><span style=""color:dima""> non-color</span></p>
");
			FormatTest(
				@"'''%red big%Big bold red text''' %small green%''Small green italic''%%normal",
				@"<p><strong><span style=""color:red""><big>Big bold red text</strong> </big></span><small><span style=""color:green""><em>Small green italic</em></small></span>normal</p>
");
			FormatTest(
				@"normal %big big%Very big %% normal %small small% very small %blue% normal size blue",
				@"<p>normal <big><big>Very big </big></big> normal <small><small> very small </small></small><span style=""color:blue""> normal size blue</span></p>
");
		}
		#endregion 

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
				@"<p><span class=""ErrorMessage""><span class=""ErrorMessageBody"">The values array does not contain the same number of items as the options array
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

		#region Additional form element tests
		[Test] public void FormCheckboxTest()
		{
			FormatTest(
				@"@@Presentations.Checkbox(""checkboxTest2"", ""99"", false)@@",
				@"<p><input type=""checkbox"" name=""checkboxTest2"" value=""99""/></p>
");
			FormatTest(
				@"@@Presentations.Checkbox(""checkboxTest"", ""999"", true, ""class='myChxbx'"")@@",
				@"<p><input type=""checkbox"" name=""checkboxTest"" value=""999"" class='myChxbx' checked=""true""/></p>
");
		}
		[Test] public void FormRadioButtonTest()
		{
			FormatTest(
				@"@@Presentations.Radio(""radioTest1"", ""421"", false)@@",
				@"<p><input type=""radio"" name=""radioTest1"" value=""421""/></p>
");

			FormatTest(
				@"@@Presentations.Radio(""radioTest"", ""42"", true, ""class='myRdbx'"")@@",
				@"<p><input type=""radio"" name=""radioTest"" value=""42"" class='myRdbx' checked=""true"" /></p>
");
		}
		[Test] public void FormLabelTest()
		{
			FormatTest(
				@"@@Presentations.Label(""forInputXY"", ""This is the label text"")@@",
				@"<p><label for=""forInputXY"">This is the label text</label></p>
");
			FormatTest(
				@"@@Presentations.Label(""forInputXY2"", ""This is the label text"", ""class='myLabels'"")@@",
				@"<p><label class='myLabels' for=""forInputXY2"">This is the label text</label></p>
");
		}
		[Test] public void FormInputFieldTest()
		{
			FormatTest(
				@"@@Presentations.InputField(""myText"", ""This is the default text"", 100, ""class='mytxt'"")@@",
				@"<p><input type=""text"" name=""myText"" id=""myText"" size=""100"" class='mytxt' value=""This is the default text"" /></p>
");
			FormatTest(
				@"@@Presentations.InputField(""myText"", ""This is the default text"")@@",
				@"<p><input type=""text"" name=""myText"" id=""myText"" value=""This is the default text"" /></p>
");
		}
		[Test] public void FormHiddenFieldTest()
		{
			FormatTest(
				@"@@Presentations.HiddenField(""myHidden"", ""param"", ""class='mytxt'"")@@",
				@"<p><input style=""display: none"" type=""text"" name=""myHidden"" value=""param"" class='mytxt' /></p>
");
			FormatTest(
				@"@@Presentations.HiddenField(""myHidden"", ""param"")@@",
				@"<p><input style=""display: none"" type=""text"" name=""myHidden"" value=""param"" /></p>
");
		}
		[Test] public void FormResetButtonTest()
		{
			FormatTest(
				@"@@Presentations.ResetButton(""myCncl"", ""Cancel"", ""class='mytxt'"")@@",
				@"<p><input type=""reset"" name=""myCncl"" class='mytxt' value=""Cancel"" /></p>
");
			FormatTest(
				@"@@Presentations.ResetButton(""myCncl"", ""Cancel"")@@",
				@"<p><input type=""reset"" name=""myCncl"" value=""Cancel"" /></p>
");
		}
		[Test] public void FormTextareaTest()
		{
			FormatTest(
				@"@@Presentations.Textarea(""txtName"", ""This is the label text"", 10, 40, ""class='myTxtarea'"")@@",
				@"<p><textarea name=""txtName"" rows=""10"" cols=""40"" class='myTxtarea'>This is the label text</textarea></p>
");
			FormatTest(
				@"@@Presentations.Textarea(""txtName"", ""This is the label text"", 80)@@",
				@"<p><textarea name=""txtName"" rows=""80"">This is the label text</textarea></p>
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
				@"<p><span class=""ErrorMessage""><span class=""ErrorMessageBody"">The values array does not contain the same number of items as the options array
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

		#region PropertyAnchors
		[Test]
		public void PropertyAnchors()
		{
			FormatTest("Foo:Bar", 
			@"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Foo</legend><span class=""PropertyValue""><a name=""Foo"" class=""Anchor"">Bar</a></span>
</fieldset>

");
			FormatTest(":Foo:Bar", @"<a name=""Foo"" class=""Anchor""></a>
");
			FormatTest(@"Foo:[ Bar
Baz
]",
			@"<fieldset  class=""Property"" style=""width: auto""><legend class=""PropertyName"">Foo</legend><span class=""PropertyValue""><a name=""Foo"" class=""Anchor"">Bar<p>Baz</p>
</a></span>
</fieldset>
");
			FormatTest(@":Foo:[ Bar
Baz
]",
			@"<a name=""Foo"" class=""Anchor""></a>
");

		}
		#endregion

		#region ProcessLineElementRefactor
		[Test] public void ProcessLineElementRefactor()
		{
			// make sure that input string that contain the parameter tokens are properly escaped
			FormatTest(@"${USER}=${1} """"# comment"""" is """"here""""",
				@"<p>${USER}=${1} # comment is here</p>
");
			// original impl expected < 10 escaped parameters
			FormatTest(@"""""One"""" """"two"""" """"three"""" """"four"""" """"five"""" """"six"""" """"seven"""" """"eight"""" """"nine"""" """"ten"""" """"eleven"""" """"twelve"""" ",
				@"<p>One two three four five six seven eight nine ten eleven twelve </p>
");
            FormatTest(@"The ${0} is @@ProductVersion@@",
                @"<p>The ${0} is " + new ClassicBehaviors().ProductVersion + @"</p>
");
            FormatTest(@"The """"ProductVersion"""" is @@ProductVersion@@",
                @"<p>The ProductVersion is " + new ClassicBehaviors().ProductVersion + @"</p>
");
            FormatTest(@"The """"@@"""" is @@ProductVersion@@ and that is all there is to it",
                @"<p>The @@ is " + new ClassicBehaviors().ProductVersion + @" and that is all there is to it</p>
");
            FormatTest(@"The """"ProductVersion"""" is @@ProductVersion@@ and that is all there is to it",
                @"<p>The ProductVersion is " + new ClassicBehaviors().ProductVersion + @" and that is all there is to it</p>
");
		}
		#endregion

        #region SimpleLinkTo tests
        [Test] public void SimpleLinkToTest()
        {
            FormatTest("@@federation.LinkMaker.SimpleLinkTo(\"Foo.aspx\")@@",
                @"<p>" + _base + @"Foo.aspx</p>
"); 
        }
        #endregion


		#region Private Methodes
		
		#region ShouldBeTopicName
		private void ShouldBeTopicName(string s)
		{
			string topicName = s;
			int hashIndex = s.IndexOf("#");
			if (hashIndex > -1)
			{
				topicName = s.Substring(0, hashIndex);
			}
			FormatTest(
				@s,
				@"<p><a title=""Click here to create this topic"" class=""create"" href=""" + _lm.LinkToEditTopic(_cb.TopicNameFor(topicName)) + @""">" + topicName + @"</a></p>
");
		}
		#endregion

		#region ShouldBeTopicName
		private void ShouldNotBeTopicName(string s)
		{
			FormatTest(
				@s,
				@"<p>" + s + @"</p>
");
		}
		#endregion

		private void AssertStringContains(string container, string find)
		{
			Assert.IsTrue(container.IndexOf(find) != -1, "Searching for " + find + " in " + container);
		}
		private string FormattedTestText(string inputString)
		{
			return FormattedTestText(inputString, null);
		}

		private string FormattedTestText(string inputString, AbsoluteTopicName top)
		{
			WikiOutput output = WikiOutput.ForFormat(OutputFormat.Testing, null);
			Formatter.Format(top, inputString, output,  _cb, _lm, _externals, 0, null);
			string o = output.ToString();
			string o1 = o.Replace("\r", "");
			return o1;
		}

		private string FormattedTopic(AbsoluteTopicName top)
		{
			return FormattedTestText(TheFederation.ContentBaseForTopic(top).Read(top.LocalName), top);
		}

		private void FormattedTopicContainsTest(AbsoluteTopicName top, string find)
		{
			FormatTestContains(FormattedTopic(top), find);
		}

		private void FormatTestContains(string inputString, string find)
		{
			string s = FormattedTestText(inputString);
			AssertStringContains(s, find);
		}

		private void FormatTest(string inputString, string outputString)
		{
			FormatTest(inputString, outputString, null);
		}

		private void FormatTest(string inputString, string outputString, AbsoluteTopicName top)
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
		#endregion
	}
}
