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
        public void FormattedName()
        {
            Assert.AreEqual("TEST That Acryonyms SPACE Correctly", 
                new TopicName("TESTThatAcryonymsSPACECorrectly").FormattedName, 
                "Checking that FormattedName deals with acronyms correctly.");
        }
        [Test]
        [Ignore("These are from the 1.8 code and need to be refactored.")]
        public void LegacyTests()
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
            Assert.AreEqual("Namespace.TopicName", topicName.QualifiedName,
                "Checking that a topic name with a non-null namespace returns the correct QualifiedName.");
        }
        [Test]
        public void QualifiedNameNullNamepace()
        {
            TopicName topicName = new TopicName("TopicName");
            Assert.AreEqual("TopicName", topicName.QualifiedName,
                "Checking that a topic name with a non-null namespace returns the correct QualifiedName.");
        }
        [Test]
        public void ResolveRelativeToFromQualified()
        {
            TopicName topicName = new TopicName("Namespace.TopicName");

            NamespaceQualifiedTopicName qualifiedName = topicName.ResolveRelativeTo("SomeNamespace");

            Assert.AreEqual("Namespace.TopicName", qualifiedName.QualifiedName,
                "Checking that the original namespace is kept when resolving an already-qualified name."); 
        }
        [Test]
        public void ResolveRelativeToFromUnqualified()
        {
            TopicName topicName = new TopicName("TopicName");

            NamespaceQualifiedTopicName qualifiedName = topicName.ResolveRelativeTo("SomeNamespace");

            Assert.AreEqual("SomeNamespace.TopicName", qualifiedName.QualifiedName,
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
                Assert.AreEqual(expectedAlternate.QualifiedName, actualAlternates[i].QualifiedName,
                    message);
            }
        }

    }
}
