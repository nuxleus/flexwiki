using System;
using System.IO;

using NUnit.Framework;

using FlexWiki.Collections;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class BuiltinTopicsProviderTests
    {
        [Test]
        public void AllChangesForTopicSinceDefault()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://BuiltInTopicsProviderTests/",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            BuiltinTopicsProvider provider = (BuiltinTopicsProvider) manager.GetProvider(typeof(BuiltinTopicsProvider));

            manager.WriteTopicAndNewVersion("NewTopic", "Some content", "Test");

            TopicChangeCollection changes = provider.AllChangesForTopicSince(
                new UnqualifiedTopicName(NamespaceManager.DefinitionTopicLocalName), DateTime.MinValue);

            changes = provider.AllChangesForTopicSince(new UnqualifiedTopicName(manager.HomePage), DateTime.MinValue);
            Assert.AreEqual(1, changes.Count, "Checking that exactly one change was listed for the home page.");
            AssertChangeCorrect(changes[0], "FlexWiki", DateTime.MinValue, DateTime.MinValue,
                "NamespaceOne.HomePage(0001-01-01-00-00-00.0000-FlexWiki)", "(HomePage)");

            changes = provider.AllChangesForTopicSince(new UnqualifiedTopicName(NamespaceManager.BordersTopicLocalName), DateTime.MinValue);
            Assert.AreEqual(1, changes.Count, "Checking that exactly one change was listed for the borders.");
            AssertChangeCorrect(changes[0], "FlexWiki", DateTime.MinValue, DateTime.MinValue,
                "NamespaceOne._NormalBorders(0001-01-01-00-00-00.0000-FlexWiki)", "(HomePage)");

            // Ensure that non-built-in topics don't have anything added to them, and that their history is preserved.
            changes = provider.AllChangesForTopicSince(new UnqualifiedTopicName("NewTopic"), DateTime.MinValue);
            Assert.AreEqual(1, changes.Count, "Checking that exactly one change is listed for a non-built-in topic since forever.");
            AssertChangeCorrect(changes[0], "Test", new DateTime(2004, 10, 28, 14, 11, 01),
                new DateTime(2004, 10, 28, 14, 11, 01),
                "NamespaceOne.NewTopic(2004-10-28-14-11-01.0000-Test)", "(Non-built-in topic since forever)");

            DateTime recentTime = federation.TimeProvider.Now.Subtract(TimeSpan.FromDays(1));

            changes = provider.AllChangesForTopicSince(new UnqualifiedTopicName(manager.HomePage), recentTime);
            Assert.IsNull(changes, "Checking that no changes are returned for the home page when the 'since' is not DateTime.MinValue.");

            changes = provider.AllChangesForTopicSince(new UnqualifiedTopicName(NamespaceManager.BordersTopicLocalName), recentTime);
            Assert.IsNull(changes, "Checking that no changes are returned for the borders topic when the 'since' is not DateTime.MinValue.");

            changes = provider.AllChangesForTopicSince(new UnqualifiedTopicName("NewTopic"), recentTime);
            Assert.AreEqual(1, changes.Count, "Checking that exactly one change is listed for a non-built-in topic recently.");
            AssertChangeCorrect(changes[0], "Test", new DateTime(2004, 10, 28, 14, 11, 01),
                new DateTime(2004, 10, 28, 14, 11, 01),
                "NamespaceOne.NewTopic(2004-10-28-14-11-01.0000-Test)", "(Non-built-in topic since recent)");


        }
        [Test]
        public void AllChangesForTopicSinceWithHistory()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://BuiltInTopicsProviderTests/",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            BuiltinTopicsProvider provider = (BuiltinTopicsProvider) manager.GetProvider(typeof(BuiltinTopicsProvider));

            WriteBuiltInTopics(manager);

            TopicChangeCollection changes = provider.AllChangesForTopicSince(
                new UnqualifiedTopicName(NamespaceManager.DefinitionTopicLocalName), DateTime.MinValue);

            changes = provider.AllChangesForTopicSince(
                new UnqualifiedTopicName(manager.HomePage), DateTime.MinValue);

            Assert.AreEqual(2, changes.Count, "Checking that exactly two changes were listed for the home page.");
            AssertChangeCorrect(changes[0], "FlexWiki", DateTime.MinValue, DateTime.MinValue,
                "NamespaceOne.HomePage(0001-01-01-00-00-00.0000-FlexWiki)", "(HomePage 0)");
            AssertChangeCorrect(changes[1], "Test", new DateTime(2004, 10, 28, 14, 11, 01),
                new DateTime(2004, 10, 28, 14, 11, 01),
                "NamespaceOne.HomePage(2004-10-28-14-11-01.0000-Test)", "(HomePage 1)");

            changes = provider.AllChangesForTopicSince(
                new UnqualifiedTopicName(NamespaceManager.BordersTopicLocalName), DateTime.MinValue);

            Assert.AreEqual(2, changes.Count, "Checking that exactly two changes were listed for the borders topic.");
            AssertChangeCorrect(changes[0], "FlexWiki", DateTime.MinValue, DateTime.MinValue,
                "NamespaceOne._NormalBorders(0001-01-01-00-00-00.0000-FlexWiki)", "(Borders topic 0)");
            AssertChangeCorrect(changes[1], "Test", new DateTime(2004, 10, 28, 14, 11, 02),
                new DateTime(2004, 10, 28, 14, 11, 02),
                "NamespaceOne._NormalBorders(2004-10-28-14-11-02.0000-Test)", "(Borders topic 1)");

        }
        [Test]
        public void AllTopics()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://BuiltInTopicsProviderTests/",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            BuiltinTopicsProvider provider = (BuiltinTopicsProvider) manager.GetProvider(typeof(BuiltinTopicsProvider));

            QualifiedTopicNameCollection topics = provider.AllTopics();

            Assert.AreEqual(2, topics.Count, "Checking that two topics exist by default.");

            Assert.IsTrue(topics.Contains(manager.HomePageTopicName),
                "Checking that the HomePage is present by default.");
            Assert.IsTrue(topics.Contains(manager.BordersTopicName),
                "Checking that the borders topic is present by default.");

            WriteBuiltInTopics(manager);

            topics = provider.AllTopics();

            Assert.AreEqual(2, topics.Count, "Checking that two topics still exist.");
            Assert.IsTrue(topics.Contains(manager.HomePageTopicName),
                "Checking that the HomePage is still present.");
            Assert.IsTrue(topics.Contains(manager.BordersTopicName),
                "Checking that the borders topic is still present.");

            manager.WriteTopicAndNewVersion("NewTopic", "New Content", "Test");

            Assert.AreEqual(3, provider.AllTopics().Count, "Checking that default topics do not interfere with the topic count.");
        }
        [Test]
        public void TextReaderForTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://BuiltInTopicsProviderTests/",
               TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            BuiltinTopicsProvider provider = (BuiltinTopicsProvider) manager.GetProvider(typeof(BuiltinTopicsProvider));

            string content = provider.TextReaderForTopic(new UnqualifiedTopicRevision(manager.HomePage)).ReadToEnd();

            Assert.AreEqual(445, GetContent(provider, manager.HomePage).Length, 
                "Checking that home page has correct default content.");
            Assert.AreEqual(3925, GetContent(provider, NamespaceManager.BordersTopicLocalName).Length,
                "Checking that borders topic has correct default content.");

            WriteBuiltInTopics(manager);

            Assert.AreEqual(20, GetContent(provider, manager.HomePage).Length, 
                "Checking that home page has correct non-default content.");
            Assert.AreEqual(19, GetContent(provider, NamespaceManager.BordersTopicLocalName).Length,
                "Checking that borders topic has correct non-default content.");

            Assert.AreEqual(445, GetDefaultContent(provider, manager.HomePage).Length,
                "Checking that home page has correct default content.");
            Assert.AreEqual(3925, GetDefaultContent(provider, NamespaceManager.BordersTopicLocalName).Length,
                "Checking that borders topic has correct default content.");

            UnqualifiedTopicRevision homePageDefaultRevision = new UnqualifiedTopicRevision(
                manager.HomePage, TopicRevision.NewVersionStringForUser("FlexWiki", DateTime.MinValue)); 
            UnqualifiedTopicRevision bordersDefaultRevision = new UnqualifiedTopicRevision(
                manager.BordersTopicName.LocalName, TopicRevision.NewVersionStringForUser("FlexWiki", DateTime.MinValue)); 
            UnqualifiedTopicRevision homePageNewRevision = new UnqualifiedTopicRevision(
                manager.HomePage, TopicRevision.NewVersionStringForUser("Test", new DateTime(2004, 10, 28, 14, 11, 01))); 
            UnqualifiedTopicRevision bordersNewRevision = new UnqualifiedTopicRevision(
                manager.BordersTopicName.LocalName, TopicRevision.NewVersionStringForUser("Test", new DateTime(2004, 10, 28, 14, 11, 02))); 
            UnqualifiedTopicRevision homePageNoSuchRevison = new UnqualifiedTopicRevision(
                manager.HomePage, "NoSuchVersion"); 
            UnqualifiedTopicRevision bordersNoSuchRevision = new UnqualifiedTopicRevision(
                manager.BordersTopicName.LocalName, "NoSuchVersion");

            Assert.AreEqual(445, GetContent(provider, homePageDefaultRevision).Length,
                "Checking that home page has correct content when retrieving default content revision explicitly.");
            Assert.AreEqual(20, GetContent(provider, homePageNewRevision).Length,
                "Checking that home page has correct content when retrieiving new content revision explicitly.");
            Assert.IsNull(provider.TextReaderForTopic(homePageNoSuchRevison),
                "Checking that home page returns null when retrieving non existent revision.");

            Assert.AreEqual(3925, GetContent(provider, bordersDefaultRevision).Length,
                "Checking that borders topic has correct content when retrieving default content revision explicitly.");
            Assert.AreEqual(19, GetContent(provider, bordersNewRevision).Length,
                "Checking that borders topic has correct content when retrieiving new content revision explicitly.");
            Assert.IsNull(provider.TextReaderForTopic(bordersNoSuchRevision),
                "Checking that borders topic returns null when retrieving non existent revision.");

            manager.WriteTopicAndNewVersion("NewTopic", "Some new content", "Test");

            UnqualifiedTopicRevision newTopicRevision = new UnqualifiedTopicRevision("NewTopic",
                TopicRevision.NewVersionStringForUser("Test", new DateTime(2004, 10, 28, 14, 11, 03)));
            UnqualifiedTopicRevision newTopicNoSuchRevision = new UnqualifiedTopicRevision("NewTopic",
                TopicRevision.NewVersionStringForUser("Test", DateTime.MinValue));

            Assert.AreEqual(16, GetContent(provider, "NewTopic").Length,
                "Checking that the provider doesn't interfere with non-built-in topics.");
            Assert.AreEqual(16, GetContent(provider, newTopicRevision).Length, 
                "Checking that the provider doesn't interfere with non-built-in topics when retreived by explicit version."); 

            Assert.IsNull(provider.TextReaderForTopic(newTopicNoSuchRevision), 
                "Checking that the provider returns null when a nonexistent version of a non-built-in topic is requested."); 

        }
        [Test]
        public void TopicExists()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://BuiltInTopicsProviderTests/",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            BuiltinTopicsProvider provider = (BuiltinTopicsProvider) manager.GetProvider(typeof(BuiltinTopicsProvider));

            Assert.IsTrue(provider.TopicExists(new UnqualifiedTopicName(manager.HomePage)),
                "Checking that home page exists even in the default case.");
            Assert.IsTrue(provider.TopicExists(new UnqualifiedTopicName(NamespaceManager.BordersTopicLocalName)),
                "Checking that the borders topic exists even in the default case.");

            Assert.IsFalse(provider.TopicExists(new UnqualifiedTopicName("SomeTopic")),
                "Checking that the provider doesn't interfere with existence of non-existent topics.");
            manager.WriteTopicAndNewVersion("SomeTopic", "some content", "Test");
            Assert.IsTrue(provider.TopicExists(new UnqualifiedTopicName("SomeTopic")),
                "Checking that the provider doesn't interfere with existence of existing topics.");

            WriteBuiltInTopics(manager);

            Assert.IsTrue(provider.TopicExists(new UnqualifiedTopicName(manager.HomePage)),
                "Checking that home page exists when it lives in the content store.");
            Assert.IsTrue(provider.TopicExists(new UnqualifiedTopicName(NamespaceManager.BordersTopicLocalName)),
                "Checking that the borders topic exists when it lives in the content store.");

        }

        private static void AssertChangeCorrect(TopicChange change, string author, DateTime created, DateTime modified,
            string dottedNameWithVersion, string message)
        {
            Assert.AreEqual(author, change.Author, "Checking that author was correct " + message);
            Assert.AreEqual(created, change.Created, "Checking that the created time is assigned correctly " + message);
            Assert.AreEqual(modified, change.Modified, "Checking that the modification time is correct " + message);
            Assert.AreEqual(dottedNameWithVersion, change.TopicRevision.DottedNameWithVersion,
                "Checking that the dotted name with version is correct" + message);
        }
        private static string GetContent(BuiltinTopicsProvider provider, string topic)
        {
            return GetContent(provider, new UnqualifiedTopicRevision(topic)); 
        }
        private static string GetContent(BuiltinTopicsProvider provider, UnqualifiedTopicRevision revision)
        {
            using (TextReader reader = provider.TextReaderForTopic(revision))
            {
                if (reader == null)
                {
                    return null;
                }

                return reader.ReadToEnd();
            }
        }
        private static string GetDefaultContent(BuiltinTopicsProvider provider, string topic)
        {
            string version = TopicRevision.NewVersionStringForUser("FlexWiki", DateTime.MinValue);
            using (TextReader reader = provider.TextReaderForTopic(new UnqualifiedTopicRevision(topic, version)))
            {

                if (reader == null)
                {
                    return null;
                }

                return reader.ReadToEnd();
            }
        }
        private static void WriteBuiltInTopics(NamespaceManager manager)
        {
            manager.WriteTopicAndNewVersion(manager.HomePage, "New HomePage content", "Test");
            manager.WriteTopicAndNewVersion(NamespaceManager.BordersTopicLocalName, "New borders content", "Test");
        }
    }
}
