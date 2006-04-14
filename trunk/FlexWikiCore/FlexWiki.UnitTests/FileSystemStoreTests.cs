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

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class FileSystemStoreTests
    {
        private Federation _federation;

        private Federation Federation
        {
            get { return _federation; }
        }

        [SetUp]
        public void SetUp()
        {
            MockWikiApplication application = new MockWikiApplication(null,
                new LinkMaker("test://FileSystemStoreTests/"), OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            _federation = new Federation(application);
        }

        [Test]
        public void EnsureContentDirectoryIsMadeIfAbsent()
        {

            Assert.Fail("Needs reimplementation.");

            //      string ns = "Test.Foo";
            //      string p = @"\temp-wikitests-creation";
            //      FileSystemStore store = new FileSystemStore(Federation, ns, p);
            //      Assert.IsTrue(Directory.Exists(p));
            //      store.DeleteNamespace();
        }
    }
}
