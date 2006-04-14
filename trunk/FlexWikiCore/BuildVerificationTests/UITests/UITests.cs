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

namespace FlexWiki.BuildVerificationTests
{
    public class UITests
    {
        protected Federation _federation;
        internal WikiState _oldWikiState;
        private string _root;

        /// <summary>
        /// Answer the namespaces and topics that the default setup and teardown will establish in the federation
        /// </summary>
        internal virtual TestContent FederationContent
        {
            get
            {
                return _testContent;
            }
        }

        internal TestContent _testContent = new TestContent(
            new TestNamespace("NamespaceOne",
                new TestTopic("TopicOne", "This is some test content in NamespaceOne"),
                new TestTopic("TopicTwo", "This is some other test content in NamespaceTwo"),
                new TestTopic("TitledTopic", @"
Hello

Title: This fat hen

"),
                new TestTopic("RenameableTopic", "This topic can be renamed.")
            ),
            new TestNamespace("NamespaceTwo",
                new TestTopic("TopicOne", "This is some test content in NamespaceTwo"),
                new TestTopic("TopicThree", "This is yet more content in NamespaceTwo"),
                new TestTopic("TopicOther", "This is some other test content in NamespaceTwo"),
                new TestTopic("TopicFour", "This is test content with a link to TopicOne")
            )
        );


        protected Federation Federation
        {
            get
            {
                return _federation;
            }
            set
            {
                _federation = value;
            }
        }


        internal WikiState OldWikiState
        {
            get
            {
                return _oldWikiState;
            }
            set
            {
                _oldWikiState = value;
            }
        }

        internal string Root
        {
            get { return _root; }
            set { _root = value; }
        }

        [SetUp]
        public virtual void Setup()
        {
            // Back up the wiki configuration
            OldWikiState = TestUtilities.BackupWikiState();

            // Recreate the wiki each time so we start from a known state
            Root = System.Guid.NewGuid().ToString();
            Federation = TestUtilities.CreateFederation(Root, FederationContent);

            // Establish a link maker
            TheLinkMaker = new LinkMaker(TestUtilities.BaseUrl);

            // And a browser
            TheBrowser = new Browser();
        }

        protected LinkMaker _LinkMaker;

        protected LinkMaker TheLinkMaker
        {
            get
            {
                return _LinkMaker;
            }
            set
            {
                _LinkMaker = value;
            }
        }

        protected Browser _Browser;

        protected Browser TheBrowser
        {
            get
            {
                return _Browser;
            }
            set
            {
                _Browser = value;
            }
        }

        [TearDown]
        public virtual void Teardown()
        {
            TestUtilities.RestoreWikiState(OldWikiState);
        }
    }
}
