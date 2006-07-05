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
    [TestFixture]
    [Ignore("This test disabled during the 2.0 upgrade. Re-enable as functionality is implemented.")]
    public class RegistryTests
    {
        private NamespaceManager _base;
        private const string _bh = "http://boo/";
        private NamespaceManager _namespaceManager5;
        private Federation _federation;
        private LinkMaker _lm;
        private NamespaceManager _other1;
        private NamespaceManager _other2;
        private NamespaceManager _other3;

        private Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }

        [SetUp]
        public void SetUp()
        {
            string author = "tester-joebob";
            _lm = new LinkMaker(_bh);
            MockWikiApplication application = new MockWikiApplication(null,
                _lm,
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            Federation = new Federation(application);

            _base = WikiTestUtilities.CreateMockStore(Federation, "FlexWiki.Base");
            _other1 = WikiTestUtilities.CreateMockStore(Federation, "FlexWiki.Other1");
            _other2 = WikiTestUtilities.CreateMockStore(Federation, "Other2");
            _other3 = WikiTestUtilities.CreateMockStore(Federation, "Other3");
            _namespaceManager5 = WikiTestUtilities.CreateMockStore(Federation, "Space5");

            WikiTestUtilities.WriteTestTopicAndNewVersion(_base, _base.DefinitionTopicName.LocalName, @"Import: FlexWiki.Other1, Other2", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_base, "TopicOne", @"OtherOneHello", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_base, "TopicTwo", @"FlexWiki.Other1.OtherOneGoodbye", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_base, "TopicThree", @"No.Such.Namespace.FooBar", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_base, "TopicFour", @".TopicOne", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_base, "TopicFive", @"FooBar
Role:Designer", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_base, "TopicSix", @".GooBar
Role:Developer", author);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, _other1.DefinitionTopicName.LocalName, @"Import: Other3,Other2", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneHello", @"hello
Role:Developer", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneGoodbye", @"goodbye", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneRefThree", @"OtherThreeTest", author);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneTopicOne", @"OtherTwoHello", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneTopicTwo", @"Other2.OtherTwoGoodbye", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneTopicThree", @"No.Such.Namespace.FooBar", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneTopicFour", @".OtherOneTopicOne", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneTopicFive", @"FooBar", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other1, "OtherOneTopicSix", @".GooBar", author);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_other2, "OtherTwoHello", @"hello", author);
            WikiTestUtilities.WriteTestTopicAndNewVersion(_other2, "OtherTwoGoodbye", @"goodbye", author);

            WikiTestUtilities.WriteTestTopicAndNewVersion(_other3, "OtherThreeTest", @"yo", author);



            WikiTestUtilities.WriteTestTopicAndNewVersion(_namespaceManager5, "AbsRef", @"Other2.OtherTwoHello", author);

        }

        [TearDown]
        public void TearDown()
        {
            _base.DeleteAllTopicsAndHistory();
            _other1.DeleteAllTopicsAndHistory();
            _other2.DeleteAllTopicsAndHistory();
            _other3.DeleteAllTopicsAndHistory();
            _namespaceManager5.DeleteAllTopicsAndHistory();
        }


        [Test]
        public void AllTopicsInfo()
        {
            //Assert.AreEqual(23,_base.AllTopicsInfo(new ExecutionContext()).Count);
            throw new NotImplementedException();
        }

        [Test]
        public void DoubleHopImportTest()
        {
            _lm.ReturnToTopicForEditLinks = new QualifiedTopicRevision("FlexWiki.Other1.OtherOneRefThree");
            CompareTopic("OtherOneRefThree", @"href=""" + _lm.LinkToTopic(new QualifiedTopicRevision("Other3.OtherThreeTest")) + @""">OtherThreeTest</a>");
        }

        [Test]
        public void BaseToForcedLocal()
        {
            CompareTopic("TopicFour", @"href=""" + _lm.LinkToTopic(new QualifiedTopicRevision("FlexWiki.Base.TopicOne")) + @""">TopicOne</a>");
        }

        [Test]
        public void BaseToForeignUnqualified()
        {
            CompareTopic("TopicOne", @"href=""" + _lm.LinkToTopic(new QualifiedTopicRevision("FlexWiki.Other1.OtherOneHello")) + @""">OtherOneHello</a>");
        }

        [Test]
        public void BaseToForeignQualified()
        {
            CompareTopic("TopicTwo", @"href=""" + _lm.LinkToTopic(new QualifiedTopicRevision("FlexWiki.Other1.OtherOneGoodbye")) + @""">OtherOneGoodbye</a>");
        }

        [Test]
        public void BaseToQualifiedAbsent()
        {
            _lm.ReturnToTopicForEditLinks = new QualifiedTopicRevision("FlexWiki.Base.TopicThree");
            CompareTopic("TopicThree", @"<p>No.Such.Namespace.FooBar</p>");
        }

        [Test]
        public void BaseToUnqualifiedAbsent()
        {
            _lm.ReturnToTopicForEditLinks = new QualifiedTopicRevision("FlexWiki.Base.TopicFive");
            CompareTopic("TopicFive", @"class=""create"" href=""" + _lm.LinkToEditTopic(new TopicName("FlexWiki.Base.FooBar")) + @""">FooBar</a>");
        }

        [Test]
        public void BaseToForcedLocalAbsent()
        {
            _lm.ReturnToTopicForEditLinks = new QualifiedTopicRevision("FlexWiki.Base.TopicSix");
            CompareTopic("TopicSix", @"class=""create"" href=""" + _lm.LinkToEditTopic(new TopicName("FlexWiki.Base.GooBar")) + @""">GooBar</a>");
        }

        [Test]
        public void EnumIncludingImportsTest()
        {

            //ArrayList expecting = new ArrayList();
            //expecting.Add("FlexWiki.Base._ContentBaseDefinition");
            //expecting.Add("FlexWiki.Base.TopicOne");
            //expecting.Add("FlexWiki.Base.TopicTwo");
            //expecting.Add("FlexWiki.Base.TopicThree");
            //expecting.Add("FlexWiki.Base.TopicFour");
            //expecting.Add("FlexWiki.Base.TopicFive");
            //expecting.Add("FlexWiki.Base.TopicSix");
            //foreach (string backing in _base.BackingTopics.Keys)
            //{
            //    expecting.Add(_base.Namespace + "." + backing);
            //}

            //expecting.Add("FlexWiki.Other1._ContentBaseDefinition");
            //expecting.Add("FlexWiki.Other1.OtherOneHello");
            //expecting.Add("FlexWiki.Other1.OtherOneGoodbye");
            //expecting.Add("FlexWiki.Other1.OtherOneRefThree");
            //expecting.Add("FlexWiki.Other1.OtherOneTopicOne");
            //expecting.Add("FlexWiki.Other1.OtherOneTopicTwo");
            //expecting.Add("FlexWiki.Other1.OtherOneTopicThree");
            //expecting.Add("FlexWiki.Other1.OtherOneTopicFour");
            //expecting.Add("FlexWiki.Other1.OtherOneTopicFive");
            //expecting.Add("FlexWiki.Other1.OtherOneTopicSix");
            //foreach (string backing in _other1.BackingTopics.Keys)
            //{
            //    expecting.Add(_other1.Namespace + "." + backing);
            //}

            //expecting.Add("Other2.OtherTwoHello");
            //expecting.Add("Other2.OtherTwoGoodbye");
            //foreach (string backing in _other2.BackingTopics.Keys)
            //{
            //    expecting.Add(_other2.Namespace + "." + backing);
            //}


            //foreach (AbsoluteTopicName topicName in _base.AllTopics(ImportPolicy.IncludeImports))
            //{
            //    Assert.IsTrue(expecting.Contains(topicName.Fullname), "Looking for " + topicName.ToString());
            //    expecting.Remove(topicName.Fullname);
            //}
            //Assert.AreEqual(expecting.Count, 0);
        }

        [Test]
        public void ForeignToForeignUnqualified()
        {
            CompareTopic("OtherOneTopicOne", @"href=""" + _lm.LinkToTopic(new QualifiedTopicRevision("Other2.OtherTwoHello")) + @""">OtherTwoHello</a>");
        }

        [Test]
        public void ForeignToForeignQualified()
        {
            CompareTopic("OtherOneTopicTwo", @"href=""" + _lm.LinkToTopic(new QualifiedTopicRevision("Other2.OtherTwoGoodbye")) + @""">OtherTwoGoodbye</a>");
        }

        [Test]
        public void ForeignToQualifiedAbsent()
        {
            _lm.ReturnToTopicForEditLinks = new QualifiedTopicRevision("FlexWiki.Other1.OtherOneTopicThree");
            CompareTopic("OtherOneTopicThree", @"<p>No.Such.Namespace.FooBar</p>");
        }

        [Test]
        public void ForeignToForcedLocal()
        {
            CompareTopic("OtherOneTopicFour", @"href=""" + _lm.LinkToTopic(new TopicName("FlexWiki.Other1.OtherOneTopicOne")) + @""">OtherOneTopicOne</a>");
        }

        [Test]
        public void ForeignToUnqualifiedAbsent()
        {
            _lm.ReturnToTopicForEditLinks = new QualifiedTopicRevision("FlexWiki.Other1.OtherOneTopicFive");
            CompareTopic("OtherOneTopicFive", @"class=""create"" href=""" + _lm.LinkToEditTopic(new TopicName("FlexWiki.Other1.FooBar")) + @""">FooBar</a>");
        }

        [Test]
        public void ForeignToForcedLocalAbsent()
        {
            _lm.ReturnToTopicForEditLinks = new QualifiedTopicRevision("FlexWiki.Other1.OtherOneTopicSix");
            CompareTopic("OtherOneTopicSix", @"class=""create"" href=""" + _lm.LinkToEditTopic(new TopicName("FlexWiki.Other1.GooBar")) + @""">GooBar</a>");
        }

        [Test]
        public void ReferenceTopicInNonImportedNamespace()
        {
            CompareTopic("Space5.AbsRef", @"href=""" + _lm.LinkToTopic(new QualifiedTopicRevision("Other2.OtherTwoHello")) + @""">OtherTwoHello</a>");
        }

        [Test]
        public void TopicsWithTest()
        {
            Assert.AreEqual(1, _base.TopicsWith(new ExecutionContext(), "Role", "Developer").Count, "TopicsWith Role:Developer");
            Assert.AreEqual(1, _base.TopicsWith(new ExecutionContext(), "Role", "Designer").Count, "TopicsWith Role:Designer");
            Assert.AreEqual(2, _base.AllTopicsWith(new ExecutionContext(), "Role", "Developer").Count, "AllTopicsWith Role:Developer");
            Assert.AreEqual(1, _base.AllTopicsWith(new ExecutionContext(), "Role", "Designer").Count, "AllTopicsWith Role:Designer");
        }


        private void CompareTopic(string topic, string outputMustContain)
        {
            TopicName abs = _base.UnambiguousTopicNameFor(topic);
            string o = Formatter.FormattedTopic(new QualifiedTopicRevision(abs), OutputFormat.HTML, null, Federation, _lm);
            string o1 = o.Replace("\r", "");
            string o2 = outputMustContain.Replace("\r", "");
            bool pass = o1.IndexOf(o2) >= 0;
            if (!pass)
            {
                Console.Error.WriteLine("Got     : " + o1);
                Console.Error.WriteLine("But Couldn't Find: " + o2);
            }
            Assert.IsTrue(pass);
        }


    }

}
