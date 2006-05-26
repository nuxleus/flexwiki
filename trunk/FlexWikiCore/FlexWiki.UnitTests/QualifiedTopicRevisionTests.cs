using System;

using NUnit.Framework; 

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class QualifiedTopicRevisionTests
    {
        [Test]
        public void AsQualifiedTopicName()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalName", "Namespace", "Version");
            QualifiedTopicName qualifiedName = revision.AsQualifiedTopicName();

            Assert.AreEqual("Namespace.LocalName", qualifiedName.DottedName,
                "Checking that the namespace and local name were set correctly."); 
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructByStringNoNamespace()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalNameOnly");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructByTopicNameNoNamespace()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision(new TopicName("LocalNameOnly"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructByLocalNameAndNullNamespace()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalNameOnly", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), "A namespace is required.")]
        public void ConstructByLocalNameAndVersionNullNamespace()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("LocalNameOnly", null, "version");
        }

        [Test]
        public void NewOfSameType()
        {
            QualifiedTopicRevision revision = new QualifiedTopicRevision("Foo.Bar").NewOfSameType("Namespace.LocalName(Version)")
                as QualifiedTopicRevision;

            Assert.AreEqual(typeof(QualifiedTopicRevision), revision.GetType(), 
                "Checking that the newly created type is a QualifiedTopicRevision.");
            Assert.AreEqual("Namespace.LocalName(Version)", revision.DottedNameWithVersion, "Checking that the correct properties were parsed."); 

        }
    }
}
