using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework; 

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class UnqualifiedTopicNameTests
    {
        [Test]
        public void ConstructionByEmptyNamespace()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName(".FooBar");
            Assert.IsNull(topicName.Namespace, "Checking that the namespace is null.");
            Assert.AreEqual("FooBar", topicName.LocalName, "Checking that the local name is correct.");
        }
        [Test]
        public void ConstructionByLocalName()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName("LocalName");

            Assert.AreEqual("LocalName", topicName.LocalName);
            Assert.IsNull(topicName.Namespace);
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), 
           "An illegal local name was specified: the namespace separator is not allowed as part of a local name.")]
        public void ConstructionByQualifiedName()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName("Dotted.Namespace.LocalName");

        }
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void NamespaceSet()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName();
            topicName.Namespace = "Foo";    
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "A null topic name is not legal.")]
        public void NullLocalName()
        {
            UnqualifiedTopicName topicName = new UnqualifiedTopicName(null);
        }

    }
}
