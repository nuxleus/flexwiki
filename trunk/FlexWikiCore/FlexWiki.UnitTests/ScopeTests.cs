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
using NUnit.Framework;
using FlexWiki.Formatting;


namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for ScopeTests.
	/// </summary>
	[TestFixture] public class ScopeTests : WikiTests, IWikiToPresentation
	{
		ContentBase _cb, _cb2;
		const string _base = "http://boo/";
		LinkMaker _lm;
		string user = "joe";

		string Run(string input)
		{
			BehaviorParser parser = new BehaviorParser();
			ExposableParseTreeNode obj = parser.Parse(input);
			Assertion.AssertNotNull(obj);
			ExecutionContext ctx = new ExecutionContext();
			ctx.WikiTalkVersion = 1;
			IBELObject evaluated = obj.Expose(ctx);
			IOutputSequence seq = evaluated.ToOutputSequence();
			return OutputSequenceToString(seq);
		}

		public string WikiToPresentation(string s)
		{
			return "P(" + s + ")";
		}


		string OutputSequenceToString(IOutputSequence s)
		{
			WikiOutput output = WikiOutput.ForFormat(OutputFormat.Testing, null);
			s.ToPresentation(this).OutputTo(output);
			return output.ToString();
		}

		[SetUp] public void Init()
		{
			_lm = new LinkMaker(_base);
			TheFederation = new Federation(OutputFormat.HTML, _lm);
			TheFederation.WikiTalkVersion = 1;
			string root = "c:\\temp-wikibasex";
			string root2 = "c:\\temp-wikibasex2";
			string ns = "FlexWiki";
			string ns2 = "FlexWiki2";
			TheFederation.Register(ns, root);		
			TheFederation.Register(ns2, root2);		
			_cb = TheFederation.ContentBaseForRoot(root);
			_cb.Namespace = ns;

			_cb2 = TheFederation.ContentBaseForRoot(root2);
			_cb2.Namespace = ns2;

			WriteTestTopicAndNewVersion(_cb, "HomePage", "", user);
			WriteTestTopicAndNewVersion(_cb, _cb.DefinitionTopicName.Name, @"
Namespace: " + ns + @"
Import: FlexWiki2", user);
			WriteTestTopicAndNewVersion(_cb, "QualifiedLocalPropertyRef", @"
Color: green
color=@@topics.QualifiedLocalPropertyRef.Color@@", user);
			WriteTestTopicAndNewVersion(_cb, "UnqualifiedLocalPropertyRef", @"
Color: green
color=@@Color@@", user);
			WriteTestTopicAndNewVersion(_cb, "QualifiedLocalMethodRef", @"
len=@@topics.QualifiedLocalMethodRef.DirectStringLength(""hello"")@@
DirectStringLength: { str | str.Length }
", user);
			WriteTestTopicAndNewVersion(_cb, "UnqualifiedLocalMethodRef", @"
len=@@DirectStringLength(""hello"")@@
DirectStringLength: { str | str.Length }
", user);
			WriteTestTopicAndNewVersion(_cb, "LocalMethodIndirection", @"
len=@@StringLength(""hello"")@@
StringLength: { str | Len(str) }
Len: { str | str.Length }
", user);
			WriteTestTopicAndNewVersion(_cb, "LocalMethodIndirection2", @"
len=@@StringLength(""hello"")@@
StringLength: { str | Len(str) }
Len: { s | s.Length }
", user);
			WriteTestTopicAndNewVersion(_cb, "CallerBlockLocalsShouldBeInvisible", @"
len=@@StringLength(""hello"")@@
StringLength: { str | Len(str) }
Len: { s | str.Length }
", user);
			WriteTestTopicAndNewVersion(_cb2, "Profile", @"Color: puce", user);
			WriteTestTopicAndNewVersion(_cb, "ReferAcrossNamespaces", @"@@topics.Profile.Color@@", user);

			WriteTestTopicAndNewVersion(_cb, "TestChecker", @"
Test: { FearFactor }
Color: green", user);

			WriteTestTopicAndNewVersion(_cb, "CallTestChecker", @"
FearFactor: nighttime
test=@@topics.TestChecker.Test@@
", user);

			WriteTestTopicAndNewVersion(_cb, "Topic1", @"
Function: { arg1 | topics.Topic2.FunctionTwo( { arg1 } ) }
", user);

			
			WriteTestTopicAndNewVersion(_cb, "Topic2", @"
FunctionTwo: { someArg | 	someArg.Value }
", user);

			WriteTestTopicAndNewVersion(_cb, "Topic3", @"@@topics.Topic1.Function(100)@@", user);


			WriteTestTopicAndNewVersion(_cb, "BlockCanSeeLexicalScopeCaller", @"
result=@@ topics.BlockCanSeeLexicalScopeCallee.BlockValue( { Color } ) @@
Color: green", user);

			WriteTestTopicAndNewVersion(_cb, "BlockCanSeeLexicalScopeCallee", @"
BlockValue: {aBlock | aBlock.Value }", user);

			WriteTestTopicAndNewVersion(_cb, "ThisTests", @"
topic=@@topic.Name@@
namespace=@@namespace.Name@@
nscount=@@federation.Namespaces.Count@@
color=@@this.Color@@
Color: red
", user);

		}

		[TearDown] public void Deinit()
		{
			_cb.Delete();
			_cb2.Delete();
		}


		[Test] public void TestCanNotHideCoreLanguageElements()
		{
			Assertion.AssertEquals("P(null)", Run(" { null | null }.Value(100)"));
		}

		[Test] public void TestBlockCanSeeEnclosingLexicalBlock()
		{
			Assertion.AssertEquals("P(100)", Run(" { a | {a}.Value }.Value(100)"));
		}

		[Test] public void TestThis()
		{
			ConfirmTopicContains("FlexWiki.ThisTests", "ThisTests");
			ConfirmTopicContains("FlexWiki.ThisTests", "FlexWiki");
			ConfirmTopicContains("FlexWiki.ThisTests", "color=red");
			ConfirmTopicContains("FlexWiki.ThisTests", "nscount=2");
		}

		[Test] public void TestReferAcrossNamespaces()
		{
			ConfirmTopicContains("FlexWiki.ReferAcrossNamespaces", "puce");
		}

		

		[Test] public void TestPassedBlockCanSeeContainingArgBlock()
		{
			ConfirmTopicContains("FlexWiki.Topic3", "100");
		}

		
		[Test] public void TestBlockCanSeeLexicalScopeCaller()
		{
			ConfirmTopicContains("FlexWiki.BlockCanSeeLexicalScopeCaller", "result=green");
		}

		[Test] public void TestQualifiedLocalPropertyRef()
		{
			ConfirmTopicContains("FlexWiki.QualifiedLocalPropertyRef", "color=green");
		}

		[Test]public void TestUnqualifiedLocalPropertyRef()
		{
			ConfirmTopicContains("FlexWiki.UnqualifiedLocalPropertyRef", "color=green");
		}

		[Test] public void TestCallerCanNotSeeCallerVariables()
		{
			DenyTopicContains("FlexWiki.CallTestChecker", "test=nighttime");
		}

		[Test] public void TestQualifiedLocalMethodRef()
		{
			ConfirmTopicContains("FlexWiki.QualifiedLocalMethodRef", "len=5");
		}

		[Test] public void TestLocalMethodIndirection()
		{
			ConfirmTopicContains("FlexWiki.LocalMethodIndirection", "len=5");
		}

		[Test] public void TestLocalMethodIndirection2()
		{
			ConfirmTopicContains("FlexWiki.LocalMethodIndirection2", "len=5");
		}

		[Test] public void TestUnqualifiedLocalMethodRef()
		{
			ConfirmTopicContains("FlexWiki.UnqualifiedLocalMethodRef", "len=5");
		}

		[Test] public void TestCallerBlockLocalsShouldBeInvisible()
		{
			DenyTopicContains("FlexWiki.CallerBlockLocalsShouldBeInvisible", "len=5");
		}
		

		void ConfirmTopicContains(string topic, string find)
		{
			ValidateTopicContains(topic, find, true);
		}

		void DenyTopicContains(string topic, string find)
		{
			ValidateTopicContains(topic, find, false);
		}

		void ValidateTopicContains(string topic, string find, bool sense)
		{
			string fmt = FormattedTopic(topic);
			bool found = fmt.IndexOf(find) != -1;
			if (sense != found)
			{
				Console.Out.WriteLine("Can't find string: ");
				Console.Out.WriteLine(find);
				Console.Out.WriteLine(fmt);
			}
			Assertion.Assert("Searching for " + find, sense == found);
		}

		string FormattedTopic(string topic)
		{
			AbsoluteTopicName tn = new AbsoluteTopicName(topic);
			CompositeCacheRule rule = new CompositeCacheRule();
			return Formatter.FormattedTopic(tn, OutputFormat.Testing, false, TheFederation, _lm, rule);
		}

	}
}
