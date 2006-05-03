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
    public class TopicRevisionTests
    {
        [Test]
        public void ConstructDefault()
        {
            TopicRevision revision = new TopicRevision();
            Assert.IsNull(revision.LocalName, "Checking that local name is null by default.");
            Assert.IsNull(revision.Namespace, "Checking that namespace is null by default.");
            Assert.IsNull(revision.Version, "Checking that version is null by default."); 
        }

    }
}
