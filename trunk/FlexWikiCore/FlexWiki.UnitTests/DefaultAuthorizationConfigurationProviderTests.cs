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
    [TestFixture]
    [Ignore("This test disabled during the 2.0 upgrade. Re-enable as functionality is implemented.")]
    public class DefaultAuthorizationConfigurationProviderTests
    {
        private const string _baseUri = "http://localhost/flexwiki";
        private Federation _federation;
        private LinkMaker _linkMaker;
        private DefaultAuthorizationConfigurationProvider _provider = new DefaultAuthorizationConfigurationProvider();

        private Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }

        [SetUp]
        public void SetUp()
        {
            _linkMaker = new LinkMaker(_baseUri);

            MockWikiApplication application = new MockWikiApplication(null,
                _linkMaker,
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));

            Federation = new Federation(application);
            Federation.WikiTalkVersion = 1;

            _provider.Initialize(Federation);
        }

        /// <summary>
        /// Test that even if no security information is present, the default still 
        /// makes sense. 
        /// </summary>
        [Test]
        public void DefaultConfiguration()
        {
            AuthorizationConfiguration configuration = _provider.WikiConfiguration;

            Assert.IsNotNull(configuration, "Checking that a non-null configuration was returned.");

            Assert.AreEqual(1, configuration.Administrators.Count, "Checking that exactly one administrator is defined.");
            Assert.AreEqual("*", configuration.Administrators[0], "Checking that universal administration is allowed.");

            Assert.AreEqual(1, configuration.Editors.Count, "Checking that exactly one editor is defined.");
            Assert.AreEqual("*", configuration.Editors[0], "Checking that universal edition is allowed.");

            Assert.AreEqual(1, configuration.Readers.Count, "Checking that exactly one reader is defined.");
            Assert.AreEqual("*", configuration.Readers[0], "Checking that universal reading is allowed.");

        }
    }
}
