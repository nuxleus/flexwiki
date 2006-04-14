using System;
using System.Collections.Generic;
using System.Text;

using NUnit.Framework; 

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class BuiltinTopicsProviderTests
    {
        [Test]
        public void AllChangesForTopicSinceBuiltin()
        {
            MockContentStore store = new MockContentStore();
            IUnparsedContentProvider provider = new BuiltinTopicsProvider(store);

            IList<TopicChange> changes = provider.AllChangesForTopicSince(
                NamespaceManager.DefinitionTopicName, DateTime.MinValue);

            Assert.AreEqual(1, changes.Count, "Checking that exactly one change was listed.");
            Assert.AreEqual(string.Empty, changes[0].Author,
                "Checking that author was empty.");
            Assert.AreEqual(DateTime.MinValue, changes[0].Created,
                "Checking that the timestamp is assigned correctly.");
            Assert.Fail("Need to finish implementing."); 
            //Assert.AreEqual(new AbsoluteTopicName("", NamespaceManager.DefinitionTopicLocalName, changes[0].Topic, 
            //    "Checking that the topic name is correct."); 

        }
    }
}
