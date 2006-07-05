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
    public class MoreNamespaceManagerTests
    {
        private NamespaceManager _base;
        private Federation _federation;
        private NamespaceManager _imp1;
        private NamespaceManager _imp2;

        private Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }

        [SetUp]
        public void Init()
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
                _base.DefinitionTopicName.LocalName, 
                @"
Description: Test description
Import: FlexWiki.Projects.Wiki1, FlexWiki.Projects.Wiki2", author);

        }

        [TearDown]
        public void DeInit()
        {
            _base.DeleteAllTopicsAndHistory();
            _imp1.DeleteAllTopicsAndHistory();
            _imp2.DeleteAllTopicsAndHistory();
        }

        NamespaceManager Base
        {
            get
            {
                return _base;
            }
        }

        [Test]
        public void SimpleReadingTest()
        {
            Assert.AreEqual("FlexWiki.Projects.Wiki", Base.Namespace);
            Assert.AreEqual("Test description", Base.Description);
            ArrayList rels = new ArrayList();
            rels.Add(_imp1.Namespace);
            rels.Add(_imp2.Namespace);
            foreach (NamespaceManager each in Base.ImportedNamespaceManagers)
            {
                Assert.IsTrue(rels.Contains(each.Namespace));
                rels.Remove(each.Namespace);
            }
            Assert.AreEqual(0, rels.Count);
        }
    }
}
