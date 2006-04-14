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
    public class InfiniteRecursionTests
    {
        private NamespaceManager _base;
        private Federation _federation;
        private NamespaceManager _imp1;
        private NamespaceManager _imp2;

        public Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }

        [SetUp]
        public void SetUp()
        {
            MockWikiApplication application = new MockWikiApplication(null,
                new LinkMaker("http://boobar"), OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            Federation = new Federation(application);
            _base = WikiTestUtilities.CreateMockStore(Federation, "FlexWiki.Projects.Wiki");
            _imp1 = WikiTestUtilities.CreateMockStore(Federation, "FlexWiki.Projects.Wiki1");
            _imp2 = WikiTestUtilities.CreateMockStore(Federation, "FlexWiki.Projects.Wiki2");

            string author = "tester-joebob";
            WikiTestUtilities.WriteTestTopicAndNewVersion(
                _base,
                _base.DefinitionTopic.LocalName, 
@"
Description: Test description
Import: FlexWiki.Projects.Wiki1",
                author);

            WikiTestUtilities.WriteTestTopicAndNewVersion(
                _imp1, 
                _imp1.DefinitionTopic.LocalName, 
@"
Description: Test1 description
Import: FlexWiki.Projects.Wiki2", 
                author);

            WikiTestUtilities.WriteTestTopicAndNewVersion(
                _imp2, 
                _imp2.DefinitionTopic.LocalName, 
@"
Description: Test1 description
Import: FlexWiki.Projects.Wiki", 
                author);

        }

        [TearDown]
        public void TearDown()
        {
            _base.DeleteAllTopicsAndHistory();
            _imp1.DeleteAllTopicsAndHistory();
            _imp2.DeleteAllTopicsAndHistory();
        }


        [Test]
        public void TestRecurse()
        {
            Assert.AreEqual("FlexWiki.Projects.Wiki", _base.Namespace);
            Assert.AreEqual("FlexWiki.Projects.Wiki1", _imp1.Namespace);
            Assert.AreEqual("FlexWiki.Projects.Wiki2", _imp2.Namespace);
        }

    }
}
