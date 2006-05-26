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
        public void CompareTo()
        {
            TopicRevision aa11 = new TopicRevision("A", "A", "1");
            TopicRevision aa12 = new TopicRevision("A", "A", "1");
            TopicRevision aa21 = new TopicRevision("A", "A", "2");
            TopicRevision ab11 = new TopicRevision("A", "B", "1");
            TopicRevision ba11 = new TopicRevision("B", "A", "1");

            Assert.AreEqual(0, aa11.CompareTo(aa11), "Checking that an object compares equal to itself.");
            Assert.AreEqual(0, aa11.CompareTo(aa12), "Checking that an object compares equal to an equivalent object.");
            Assert.AreEqual(-1, aa11.CompareTo(aa21), "Checking that an object with a higher version compares higher.");
            Assert.AreEqual(1, aa21.CompareTo(aa11), "Checking that an object with a lower version compares lower.");
            Assert.AreEqual(-1, aa11.CompareTo(ab11), "Checking that an object with a higher namespace compares higher.");
            Assert.AreEqual(1, ab11.CompareTo(aa11), "Checking that an object with a lower namespace compares lower.");
            Assert.AreEqual(-1, aa11.CompareTo(ba11), "Checking that an object with a higher local name compares higher.");
            Assert.AreEqual(1, ba11.CompareTo(aa11), "Checking that an object with a lower local name compares lower.");
            Assert.AreEqual(1, aa11.CompareTo(null), "Checking that an object compares greater than null.");

            QualifiedTopicRevision qualifiedRevision = new QualifiedTopicRevision("A", "A", "1");
            Assert.AreEqual(0, aa11.CompareTo(qualifiedRevision), "Checking that a QualifiedTopicRevision can compare successfully to a TopicRevision"); 
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "obj is not a TopicRevision")]
        public void CompareToIncompatibleType()
        {
            TopicRevision revision = new TopicRevision("Foo");
            revision.CompareTo("Foo"); 
        }
        [Test]
        public void ConstructDefault()
        {
            TopicRevision revision = new TopicRevision();
            Assert.IsNull(revision.LocalName, "Checking that local name is null by default.");
            Assert.IsNull(revision.Namespace, "Checking that namespace is null by default.");
            Assert.IsNull(revision.Version, "Checking that version is null by default.");
        }
        [Test]
        public void ConstructByLocalNameAndNamespace()
        {
            TopicRevision revision = new TopicRevision("Foo", "Bar");
            Assert.AreEqual("Foo", revision.LocalName, "Checking that local name is correct.");
            Assert.AreEqual("Bar", revision.Namespace, "Checking that namespace is correct.");
            Assert.IsNull(revision.Version, "Checking that version is null when not specified."); 
        }
        [Test]
        public void ConstructByLocalNameNamespaceAndVersion()
        {
            TopicRevision revision = new TopicRevision("Foo", "Bar", "1.0");
            Assert.AreEqual("Foo", revision.LocalName, "Checking that local name is correct.");
            Assert.AreEqual("Bar", revision.Namespace, "Checking that namespace is correct.");
            Assert.AreEqual("1.0", revision.Version, "Checking that version is correct.");
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "topic cannot be null")]
        public void ConstructByNull()
        {
            TopicRevision revision = new TopicRevision(null);
        }

        [Test]
        public void ConstructByString()
        {
            // format is revision, name, version
            string[][] data = {
                new string[] { "Foo", "Foo", null } ,
                new string[] { "Foo.Bar", "Foo.Bar", null } ,
                new string[] { "Cat.Dog.Hello", "Cat.Dog.Hello", null } ,
                new string[] { "Hello()", "Hello", null } ,
                new string[] { "Hello(123-abc)", "Hello", "123-abc" } , 
            };
            string[] revisions = { "Foo", "Foo.Bar", "Cat.Dog.Hello", "Hello()" };
            string[] names = { "Foo", "Foo.Bar", "Cat.Dog.Hello", "Hello" };
            string[] versions = { null, null, null, null }; 

            for (int i = 0; i < data.Length; ++i)
            {
                string revisionName = data[i][0];
                string name = data[i][1];
                string version = data[i][2]; 

                TopicRevision revision = new TopicRevision(revisionName);
                Assert.AreEqual(revision.Name, new TopicName(name),
                    "Checking that name was correct during construction by name was successful for " + name); 
                Assert.AreEqual(revision.Version, version, 
                    "Checking that version was correct during construction by name was successful for " + name); 
            }
            
        }
        [Test]
        public void ConstructByTopicNameAndVersion()
        {
            TopicName name = new TopicName("Foo", "Bar");
            TopicRevision revision = new TopicRevision(name, "1.0");
            Assert.AreEqual("Foo", revision.LocalName, "Checking that local name is correct.");
            Assert.AreEqual("Bar", revision.Namespace, "Checking that namespace is correct.");
            Assert.AreEqual("1.0", revision.Version, "Checking that version is correct.");

        }
        [Test]
        public void EqualsTest()
        {
            TopicRevision aa11 = new TopicRevision("A", "A", "1");
            TopicRevision aa12 = new TopicRevision("A", "A", "1");
            TopicRevision aa21 = new TopicRevision("A", "A", "2");
            TopicRevision ab11 = new TopicRevision("A", "B", "1");
            TopicRevision ba11 = new TopicRevision("B", "A", "1");

            Assert.IsTrue(aa11.Equals(aa11), "Checking that an object compares equal to itself.");
            Assert.IsTrue(aa11.Equals(aa12), "Checking that an object compares equal to an equivalent object.");
            Assert.IsFalse(aa11.Equals(aa21), "Checking that an object is not equal to something with a different version.");
            Assert.IsFalse(aa11.Equals(ab11), "Checking that an object is not equal to something with a different namespace.");
            Assert.IsFalse(aa11.Equals(ba11), "Checking that an object is not equal to something with a different local name.");
            Assert.IsFalse(aa11.Equals(null), "Checking that an object is not equal to null."); 
        }
        [Test]
        public void GetHashCodeTests()
        {
            TopicRevision aa11 = new TopicRevision("A", "A", "1");
            TopicRevision aa12 = new TopicRevision("A", "A", "1");
            TopicRevision aa21 = new TopicRevision("A", "A", "2");
            TopicRevision ab11 = new TopicRevision("A", "B", "1");
            TopicRevision ba11 = new TopicRevision("B", "A", "1");

            Assert.AreEqual(aa11.GetHashCode(), aa11.GetHashCode(), 
                "Checking that an object always returns the same hash code.");
            Assert.AreEqual(aa12.GetHashCode(), aa11.GetHashCode(), 
                "Checking that an object returns the same hash code as an equivalent object.");
            Assert.IsFalse(aa11.GetHashCode() == aa21.GetHashCode(), 
                "Checking that an object with a different version returns a different hash code.");
            Assert.IsFalse(aa11.GetHashCode() == ab11.GetHashCode(), 
                "Checking that an object with a different namespace returns a different hash code.");
            Assert.IsFalse(aa11.GetHashCode() == ba11.GetHashCode(),
                "Checking that an object with a different local name returns a different hash code.");

        }
        [Test]
        public void LocalNameForNullName()
        {
            TopicRevision revision = new TopicRevision();

            Assert.IsNull(revision.Name); 
            Assert.IsNull(revision.LocalName, "Checking that LocalName doesn't blow up even when Name is null."); 
        }
        [Test]
        public void NewOfSameType()
        {
            TopicRevision revision = new TopicRevision().NewOfSameType("A.B(C)");

            Assert.AreEqual(typeof(TopicRevision), revision.GetType(), "Checking that the newly created type is a TopicRevision.");
            Assert.AreEqual("A.B(C)", revision.DottedNameWithVersion, "Checking that the correct properties were parsed."); 
        }
        [Test]
        public void NewVersionStringForUserFromTimeProvider()
        {
            MockTimeProvider timeProvider = new MockTimeProvider(TimeSpan.FromSeconds(1));

            string versionString = TopicRevision.NewVersionStringForUser("candera", timeProvider);

            Assert.AreEqual("2004-10-28-14-11-00.0000-candera", versionString, 
                "Checking that the correct version string was returned."); 
        }
        [Test]
        public void NewVersionStringForUserFromTimestamp()
        {
            string versionString = TopicRevision.NewVersionStringForUser("candera", new DateTime(2004, 10, 28, 14, 11, 00));
            Assert.AreEqual("2004-10-28-14-11-00.0000-candera", versionString, 
                "Checking that the correct version string was returned."); 
        }
        [Test]
        public void ResolveRelativeToFromQualified()
        {
            TopicRevision revision = new TopicRevision("TopicName", "Namespace", "version");

            QualifiedTopicRevision qualifiedRevision = revision.ResolveRelativeTo("SomeNamespace");

            Assert.AreEqual("Namespace.TopicName(version)", qualifiedRevision.DottedNameWithVersion,
                "Checking that the original namespace is kept when resolving an already-qualified revision.");
        }
        [Test]
        public void ResolveRelativeToFromUnqualified()
        {
            TopicRevision revision = new TopicRevision(new TopicName("TopicName"), "version");

            QualifiedTopicRevision qualifiedRevision = revision.ResolveRelativeTo("SomeNamespace");

            Assert.AreEqual("SomeNamespace.TopicName(version)", qualifiedRevision.DottedNameWithVersion,
                "Checking that the new namespace is used when resolving an unqualified name.");
        }

    }
}
