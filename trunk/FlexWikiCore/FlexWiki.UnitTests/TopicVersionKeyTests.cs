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
    public class TopicVersionKeyTests
    {
        [Test]
        public void ConstructDefault()
        {
            //TopicVersionKey key = new TopicVersionKey();
            //Assert.IsNull(key.LocalName, "Checking that local name is null by default.");
            //Assert.IsNull(key.Namespace, "Checking that namespace is null by default."); 
        }

        [Test]
        public void RelativeTopicNameIsNamespaceQualifiedNegative()
        {
            RelativeTopicVersionKey topicName = new RelativeTopicVersionKey("Foo");
            Assert.IsFalse(topicName.IsNamespaceQualified);
        }

        [Test]
        public void RelativeTopicNameIsNamespaceQualifiedPositive()
        {
            RelativeTopicVersionKey topicName = new RelativeTopicVersionKey("Foo", "Bar");
            Assert.IsTrue(topicName.IsNamespaceQualified);
        }

        [Test]
        public void SimpleTests()
        {
            Assert.AreEqual("Hello", new NamespaceQualifiedTopicVersionKey("Hello").LocalName);
            Assert.AreEqual("Hello", new NamespaceQualifiedTopicVersionKey("Dog.Hello").LocalName);
            Assert.AreEqual("Dog", new NamespaceQualifiedTopicVersionKey("Dog.Hello").Namespace);
            Assert.AreEqual("Cat.Dog", new NamespaceQualifiedTopicVersionKey("Cat.Dog.Hello").Namespace);
            Assert.AreEqual("Hello", new NamespaceQualifiedTopicVersionKey("Cat.Dog.Hello").LocalName);

            Assert.AreEqual(null, new NamespaceQualifiedTopicVersionKey("Hello()").Version);
            Assert.AreEqual("123-abc", new NamespaceQualifiedTopicVersionKey("Hello(123-abc)").Version);
            Assert.AreEqual("Hello", new NamespaceQualifiedTopicVersionKey("Hello(123-abc)").LocalName);
            Assert.AreEqual(null, new NamespaceQualifiedTopicVersionKey("Hello(123-abc)").Namespace);
            Assert.AreEqual("Foo.Bar", new NamespaceQualifiedTopicVersionKey("Foo.Bar.Hello(123-abc)").Namespace);

            Assert.AreEqual("TEST That Acryonyms SPACE Correctly", new NamespaceQualifiedTopicVersionKey("TESTThatAcryonymsSPACECorrectly").FormattedName);
        }

    }
}
