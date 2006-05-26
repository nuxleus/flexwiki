using System;

using NUnit.Framework; 

namespace FlexWiki.UnitTests
{
    [TestFixture] 
    public class UnqualifiedTopicRevisionTests
    {
        [Test]
        public void AsUnqualifiedTopicName()
        {
            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision("LocalName(Revision)");

            Assert.AreEqual("LocalName", revision.AsUnqualifiedTopicName().DottedName,
                "Checking that the right name was returned."); 
            
        }
        [Test]
        public void ConstructDefault()
        {
            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision();

            Assert.IsNull(revision.LocalName, "Checking that local name is null on a default UnqualifiedTopicRevision");
            Assert.IsNull(revision.Namespace, "Checking that namespace is null on a default UnqualifiedTopicRevision");
            Assert.IsNull(revision.Version, "Checking that version is null on a default UnqualifiedTopicRevision");
        }

        [Test]
        public void ConstructByStringFromLocalName()
        {
            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision("LocalName(Version)");

            Assert.AreEqual("LocalName", revision.LocalName, "Checking that local name is correct.");
            Assert.AreEqual("Version", revision.Version, "Checking that version is correct.");
            Assert.IsNull(revision.Namespace, "Checking that namespace is correct."); 
        }

        [Test]
        [ExpectedException(typeof(ArgumentException),
            "An illegal local name was specified: the namespace separator is not allowed as part of a local name.")]
        public void ConstructByQualifiedNameFromName()
        {
            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision("Namespace.LocalName(Version)"); 
        }

        [Test]
        [ExpectedException(typeof(ArgumentException),
            "An illegal local name was specified: the namespace separator is not allowed as part of a local name.")]
        public void ConstructByQualifiedNameFromNameAndVersion()
        {
            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision("Namespace.LocalName", "Version");
        }

        [Test]
        public void ConstructByUnqualifiedTopicNameAndVersion()
        {
            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision(new UnqualifiedTopicName("LocalName"),
               "Version");

            Assert.AreEqual("LocalName(Version)", revision.DottedNameWithVersion,
                "Checking that name and version were set properly"); 
        }

        [Test]
        public void Name()
        {
            UnqualifiedTopicName name = new UnqualifiedTopicName("LocalName");
            UnqualifiedTopicRevision revision = new UnqualifiedTopicRevision(name);

            Assert.AreEqual("LocalName", revision.Name.DottedName, 
                "Checking that name is recorded correctly."); 
        }

    }
}
