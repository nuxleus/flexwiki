using System;

using NUnit.Framework;

using FlexWiki.Collections;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class TopicNameTests
    {
        [Test]
        public void AlternateForms()
        {
            AssertAlternatesCorrect("Checking that singular names have no alternates",
                "TestName");
            AssertAlternatesCorrect("Checking that 's' names return the singular",
                "TestNames",
                "TestName");
            AssertAlternatesCorrect("Checking that 'ies' names return the singular",
                "TestNamies",
                "TestNamie", "TestNamy");
            AssertAlternatesCorrect("Checking that 'sses' names return the singular",
                "TestNamesses",
                "TestNamesse", "TestNamess");
            AssertAlternatesCorrect("Checking that 'xes' names return the singular",
                "TestNamexes",
                "TestNamexe", "TestNamex");

        }
        [Test]
        public void ConstructionByEmptyNamespace()
        {
            TopicName topicName = new TopicName(".FooBar");
            Assert.IsNull(topicName.Namespace, "Checking that the namespace is null.");
            Assert.AreEqual("FooBar", topicName.LocalName, "Checking that the local name is correct."); 
        }
        [Test]
        [ExpectedException(typeof(ArgumentException),
           "An illegal local name was specified: the namespace separator is not allowed as part of a local name.")]
        public void ConstructionByIllegalLocalNameAndNamespace()
        {
            TopicName topicName = new TopicName("Dotted.LocalName", "Dotted.Namespace");
        }
        [Test]
        public void ConstructionByLocalName()
        {
            TopicName topicName = new TopicName("LocalName");

            Assert.AreEqual("LocalName", topicName.LocalName);
            Assert.IsNull(topicName.Namespace);
        }
        [Test]
        public void ConstructionByLocalNameAndNamespace()
        {
            TopicName topicName = new TopicName("LocalName", "Dotted.Namespace");

            Assert.AreEqual("LocalName", topicName.LocalName);
            Assert.AreEqual("Dotted.Namespace", topicName.Namespace);
        }
        [Test]
        public void ConstructionByQualifiedName()
        {
            TopicName topicName = new TopicName("Dotted.Namespace.LocalName");

            Assert.AreEqual("LocalName", topicName.LocalName);
            Assert.AreEqual("Dotted.Namespace", topicName.Namespace);
        }
        [Test]
        public void EqualsTests()
        {
            Assert.AreEqual(new TopicName("Foo"), new TopicName("Foo"),
                "Checking that unqualifed names are equivalent.");
            Assert.AreEqual(new TopicName("Foo.Bar"), new TopicName("Foo.Bar"),
                "Checking that qualified names are equivalent.");

            Assert.AreEqual(new TopicName("Foo.Bar"), new QualifiedTopicName("Foo.Bar"),
                "Checking that a TopicName can be equivalent to a QualifiedTopicName.");
            Assert.AreEqual(new TopicName("Foo"), new UnqualifiedTopicName("Foo"),
                "Checking that a TopicName can be equivalent to an UnqualifiedTopicName.");

            Assert.AreEqual(new TopicName(), new TopicName(), 
                "Checking that two default topic names are equivalent."); 
            Assert.IsFalse(new TopicName("Foo.Bar").Equals(new TopicName()), 
                "Checking that a nondefault topic name is not equivalent to a default topic name.");
            Assert.IsFalse(new TopicName().Equals(new TopicName("Foo.Bar")), 
                "Checking that a default topic name is not equivalent to a nondefault topic name.");             

            Assert.IsFalse(new TopicName("Foo").Equals(new TopicName("Bar")),
                "Checking that two topic names with different local names are not equivalent.");
            Assert.IsFalse(new TopicName("Foo").Equals(new TopicName("foo")),
                "Checking that two topic names that differ only by case are not equivalent."); 
            Assert.IsFalse(new TopicName("Foo.Bar").Equals(new TopicName("Foo.Baaz")), 
                "Checking that two topic names that differ only by local name are not equivalent."); 
            Assert.IsFalse(new TopicName("Foo.Bar").Equals(new TopicName("Boo.Bar")), 
                "Checking that two topic names that differ only by namespace are not equivalent."); 

            Assert.IsFalse(new TopicName("Foo.Bar").Equals(new object()), 
                "Checking that a topic name is not equivalent to something that is not a topic name."); 
            Assert.IsFalse(new TopicName("Foo.Bar").Equals(null), 
                "Checking that a topic name is not equivalent to null."); 

        }
        [Test]
        public void FormattedName()
        {
            Assert.AreEqual("TEST That Acryonyms SPACE Correctly", 
                new TopicName("TESTThatAcryonymsSPACECorrectly").FormattedName, 
                "Checking that FormattedName deals with acronyms correctly.");
        }
        [Test]
        public void GetHashCodeTests()
        {
            Assert.IsNotNull(new TopicName().GetHashCode(),
                "Checking that a default topic name still has a hash code.");
            Assert.IsNotNull(new TopicName("Foo").GetHashCode(),
                "Checking that an unqualified topic name has a hash code.");
            Assert.IsNotNull(new TopicName("Foo.Bar").GetHashCode(),
                "Checking that a qualified topic name has a hash code."); 
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "A null topic name is not legal.")]
        public void NullLocalName()
        {
            TopicName topicName = new TopicName(null);
        }
        [Test]
        public void QualifiedName()
        {
            TopicName topicName = new TopicName("TopicName", "Namespace");
            Assert.AreEqual("Namespace.TopicName", topicName.DottedName,
                "Checking that a topic name with a non-null namespace returns the correct QualifiedName.");
        }
        [Test]
        public void QualifiedNameNullNamepace()
        {
            TopicName topicName = new TopicName("TopicName");
            Assert.AreEqual("TopicName", topicName.DottedName,
                "Checking that a topic name with a non-null namespace returns the correct QualifiedName.");
        }
        [Test]
        public void ResolveRelativeToFromQualified()
        {
            TopicName topicName = new TopicName("Namespace.TopicName");

            QualifiedTopicName qualifiedName = topicName.ResolveRelativeTo("SomeNamespace");

            Assert.AreEqual("Namespace.TopicName", qualifiedName.DottedName,
                "Checking that the original namespace is kept when resolving an already-qualified name."); 
        }
        [Test]
        public void ResolveRelativeToFromUnqualified()
        {
            TopicName topicName = new TopicName("TopicName");

            QualifiedTopicName qualifiedName = topicName.ResolveRelativeTo("SomeNamespace");

            Assert.AreEqual("SomeNamespace.TopicName", qualifiedName.DottedName,
                "Checking that the new namespace is used when resolving an unqualified name.");
        }

        private void AssertAlternatesCorrect(string message, string localName,
            params string[] expectedAlternates)
        {
            TopicName topicName = new TopicName(localName, "TestNamespace");
            TopicNameCollection actualAlternates = topicName.AlternateForms();

            Assert.AreEqual(expectedAlternates.Length, actualAlternates.Count,
                message + " - incorrect number of alternates.");

            for (int i = 0; i < expectedAlternates.Length; i++)
            {
                TopicName expectedAlternate = new TopicName(expectedAlternates[i], "TestNamespace");
                Assert.AreEqual(expectedAlternate.DottedName, actualAlternates[i].DottedName,
                    message);
            }
        }

    }
}
