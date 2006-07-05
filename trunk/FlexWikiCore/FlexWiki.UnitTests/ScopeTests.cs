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
    [TestFixture]
    [Ignore("This test disabled during the 2.0 upgrade. Re-enable as functionality is implemented.")]
    public class ScopeTests : IWikiToPresentation
    {
        private const string _base = "http://boo/";
        private NamespaceManager _namespaceManager;
        private NamespaceManager _namespaceManager2;
        private Federation _federation;
        private LinkMaker _lm;
        private string _user = "joe";

        private Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }

        [SetUp]
        public void Init()
        {
            _lm = new LinkMaker(_base);
            MockWikiApplication application = new MockWikiApplication(null,
                _lm,
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            Federation = new Federation(application);
            Federation.WikiTalkVersion = 1;

            string ns = "FlexWiki";
            string ns2 = "FlexWiki2";
            _namespaceManager = WikiTestUtilities.CreateMockStore(Federation, ns);
            _namespaceManager2 = WikiTestUtilities.CreateMockStore(Federation, ns2);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "HomePage", "", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, _namespaceManager.DefinitionTopicName.LocalName, @"Import: FlexWiki2", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "QualifiedLocalPropertyRef", @"
Color: green
color=@@topics.QualifiedLocalPropertyRef.Color@@", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "UnqualifiedLocalPropertyRef", @"
Color: green
color=@@Color@@", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "QualifiedLocalMethodRef", @"
len=@@topics.QualifiedLocalMethodRef.DirectStringLength(""hello"")@@
DirectStringLength: { str | str.Length }
", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "UnqualifiedLocalMethodRef", @"
len=@@DirectStringLength(""hello"")@@
DirectStringLength: { str | str.Length }
", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "LocalMethodIndirection", @"
len=@@StringLength(""hello"")@@
StringLength: { str | Len(str) }
Len: { str | str.Length }
", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "LocalMethodIndirection2", @"
len=@@StringLength(""hello"")@@
StringLength: { str | Len(str) }
Len: { s | s.Length }
", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "CallerBlockLocalsShouldBeInvisible", @"
len=@@StringLength(""hello"")@@
StringLength: { str | Len(str) }
Len: { s | str.Length }
", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager2, "Profile", @"Color: puce", _user);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "ReferAcrossNamespaces", @"@@topics.Profile.Color@@", _user);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "TestChecker", @"
Test: { FearFactor }
Color: green", _user);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "CallTestChecker", @"
FearFactor: nighttime
test=@@topics.TestChecker.Test@@
", _user);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "Topic1", @"
Function: { arg1 | topics.Topic2.FunctionTwo( { arg1 } ) }
", _user);


            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "Topic2", @"
FunctionTwo: { someArg | 	someArg.Value }
", _user);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "Topic3", @"@@topics.Topic1.Function(100)@@", _user);


            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "BlockCanSeeLexicalScopeCaller", @"
result=@@ topics.BlockCanSeeLexicalScopeCallee.BlockValue( { Color } ) @@
Color: green", _user);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "BlockCanSeeLexicalScopeCallee", @"
BlockValue: {aBlock | aBlock.Value }", _user);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager, "ThisTests", @"
topicName=@@topicName.Name@@
namespace=@@namespace.Name@@
nscount=@@federation.Namespaces.Count@@
color=@@this.Color@@
Color: red
", _user);

        }

        [TearDown]
        public void Deinit()
        {
            _namespaceManager.DeleteAllTopicsAndHistory();
            _namespaceManager2.DeleteAllTopicsAndHistory();
        }


        [Test]
        public void TestCanNotHideCoreLanguageElements()
        {
            Assert.AreEqual("P(null)", Run(" { null | null }.Value(100)"));
        }

        [Test]
        public void TestBlockCanSeeEnclosingLexicalBlock()
        {
            Assert.AreEqual("P(100)", Run(" { a | {a}.Value }.Value(100)"));
        }

        [Test]
        public void TestThis()
        {
            ConfirmTopicContains("FlexWiki.ThisTests", "ThisTests");
            ConfirmTopicContains("FlexWiki.ThisTests", "FlexWiki");
            ConfirmTopicContains("FlexWiki.ThisTests", "color=red");
            ConfirmTopicContains("FlexWiki.ThisTests", "nscount=2");
        }

        [Test]
        public void TestReferAcrossNamespaces()
        {
            ConfirmTopicContains("FlexWiki.ReferAcrossNamespaces", "puce");
        }

        [Test]
        public void TestPassedBlockCanSeeContainingArgBlock()
        {
            ConfirmTopicContains("FlexWiki.Topic3", "100");
        }

        [Test]
        public void TestBlockCanSeeLexicalScopeCaller()
        {
            ConfirmTopicContains("FlexWiki.BlockCanSeeLexicalScopeCaller", "result=green");
        }

        [Test]
        public void TestQualifiedLocalPropertyRef()
        {
            ConfirmTopicContains("FlexWiki.QualifiedLocalPropertyRef", "color=green");
        }

        [Test]
        public void TestUnqualifiedLocalPropertyRef()
        {
            ConfirmTopicContains("FlexWiki.UnqualifiedLocalPropertyRef", "color=green");
        }

        [Test]
        public void TestCallerCanNotSeeCallerVariables()
        {
            DenyTopicContains("FlexWiki.CallTestChecker", "test=nighttime");
        }

        [Test]
        public void TestQualifiedLocalMethodRef()
        {
            ConfirmTopicContains("FlexWiki.QualifiedLocalMethodRef", "len=5");
        }

        [Test]
        public void TestLocalMethodIndirection()
        {
            ConfirmTopicContains("FlexWiki.LocalMethodIndirection", "len=5");
        }

        [Test]
        public void TestLocalMethodIndirection2()
        {
            ConfirmTopicContains("FlexWiki.LocalMethodIndirection2", "len=5");
        }

        [Test]
        public void TestUnqualifiedLocalMethodRef()
        {
            ConfirmTopicContains("FlexWiki.UnqualifiedLocalMethodRef", "len=5");
        }

        [Test]
        public void TestCallerBlockLocalsShouldBeInvisible()
        {
            DenyTopicContains("FlexWiki.CallerBlockLocalsShouldBeInvisible", "len=5");
        }


        private void ConfirmTopicContains(string topic, string find)
        {
            ValidateTopicContains(topic, find, true);
        }

        private void DenyTopicContains(string topic, string find)
        {
            ValidateTopicContains(topic, find, false);
        }

        private void ValidateTopicContains(string topic, string find, bool sense)
        {
            string fmt = FormattedTopic(topic);
            bool found = fmt.IndexOf(find) != -1;
            if (sense != found)
            {
                Console.Out.WriteLine("Can't find string: ");
                Console.Out.WriteLine(find);
                Console.Out.WriteLine(fmt);
            }
            Assert.IsTrue(sense == found, "Searching for " + find);
        }

        private string FormattedTopic(string topic)
        {
            QualifiedTopicRevision tn = new QualifiedTopicRevision(topic);
            return Formatter.FormattedTopic(tn, OutputFormat.Testing, null, Federation, _lm);
        }

        private string Run(string input)
        {
            BehaviorParser parser = new BehaviorParser("Scope Tests");
            ExposableParseTreeNode obj = parser.Parse(input);
            Assert.IsNotNull(obj);
            ExecutionContext ctx = new ExecutionContext();
            ctx.WikiTalkVersion = 1;
            IBELObject evaluated = obj.Expose(ctx);
            IOutputSequence seq = evaluated.ToOutputSequence();
            return OutputSequenceToString(seq);
        }

        private string OutputSequenceToString(IOutputSequence s)
        {
            WikiOutput output = WikiOutput.ForFormat(OutputFormat.Testing, null);
            s.ToPresentation(this).OutputTo(output);
            return output.ToString();
        }

        string IWikiToPresentation.WikiToPresentation(string s)
        {
            return "P(" + s + ")";
        }

    }
}
