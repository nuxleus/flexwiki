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
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.Threading;

using FlexWiki.Collections; 
using FlexWiki.Formatting;

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    [Ignore("These features not ready yet")]
    public class SecurityTests
    {
        private const string _baseUri = "http://localhost/flexwiki";
        private Federation _federation;
        private LinkMaker _linkMaker;
        private NamespaceManager _storeOne;
        private string _storeOneId = "SecurityTestsOne";
        private NamespaceManager _storeTwo;
        private string _storeTwoId = "SecurityTestsTwo";

        private Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // Make sure we do all the setup as administrator, or we'll be refused
            // permission to do some things
            EstablishRoles("WikiAdministrators", "NamespaceOneAdministrators");

            _linkMaker = new LinkMaker(_baseUri);
            MockWikiApplication application = new MockWikiApplication(null,
                _linkMaker,
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1)));

            Federation = new Federation(application);
            Federation.WikiTalkVersion = 1;

            // Don't read from the config files on disk - the ability to do that gets tested separately. Using the 
            // mock provider keeps everything right here in the test, without the need to worry about files on 
            // disk or the content of topics.
            MockAuthorizationConfigurationProvider configProvider = new MockAuthorizationConfigurationProvider();

            configProvider.AddWikiPermission("WikiReaders", Permission.Read);
            configProvider.AddWikiPermission("WikiEditors", Permission.Edit);
            configProvider.AddWikiPermission("WikiAdministrators", Permission.Administer);

            // NamespaceOne has explicit permissions
            configProvider.AddNamespacePermission("NamespaceOneReaders", _storeOneId, Permission.Read);
            configProvider.AddNamespacePermission("NamespaceOneEditors", _storeOneId, Permission.Edit);
            configProvider.AddNamespacePermission("NamespaceOneAdministrators", _storeOneId, Permission.Administer);

            // NamespaceTwo has default permissions that come from the wiki itself      

            // NamespaceThree is like NamespaceOne
            configProvider.AddNamespacePermission("NamespaceThreeReaders", "NamespaceThree", Permission.Read);
            configProvider.AddNamespacePermission("NamespaceThreeEditors", "NamespaceThree", Permission.Edit);
            configProvider.AddNamespacePermission("NamespaceThreeAdministrators", "NamespaceThree", Permission.Administer);

            Federation.AuthorizationConfigurationProvider = configProvider;

            _storeOne = WikiTestUtilities.CreateMockStore(Federation, _storeOneId);
            _storeTwo = WikiTestUtilities.CreateMockStore(Federation, _storeTwoId);

            // Create the _ContentBaseDefinition topics so the wiki will actually work
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, _storeOne.DefinitionTopicName.LocalName,
      @"Import: SecurityTestsTwo
Contact: CraigAndera
Description: A test namespace
ImageURL: http://localhost/wiki/images/",
              "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, "TopicOne", "NamespaceOne.TopicOne test content", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, "NamespaceOneTopic", "Foo: Bar\nNamespaceOne.TopicOne test content", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, "ReferencingTopic", "References ReferencedTopicOne and ReferencedTopicTwo and SecurityTestsTwo.TopicOne and NonExistentTopic", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, "ReferencedTopicOne", "Referenced by another topicName", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, "ReferencedTopicTwo", "Referenced by another topicName", "SecurityTests");

            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeTwo, _storeTwo.DefinitionTopicName.LocalName, "Test content", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeTwo, "TopicOne", "NamespaceTwo.TopicOne test content", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeTwo, "NamespaceTwoTopic", "Foo: Bar\nNamespaceTwo.TopicOne test content", "SecurityTests");

        }

        /// <summary>
        /// Test that an empty list is returned when there is no access and no imports.
        /// </summary>
        [Test]
        public void AllNamespaceQualifiedNamesThatExistNegativeNoImport()
        {
            EstablishRoles("NoAccess");
            IList topics = _storeTwo.AllQualifiedTopicNamesThatExist("TopicOne");

            Assert.AreEqual(0, topics.Count, "Checking that no topics were returned");
        }

        /// <summary>
        /// Test that an empty list is returned when there is no access with an import.
        /// </summary>
        [Test]
        public void AllNamespaceQualifiedNamesThatExistNegativeWithImport()
        {
            EstablishRoles("NoAccess");
            IList topics = _storeOne.AllQualifiedTopicNamesThatExist("TopicOne");

            Assert.AreEqual(0, topics.Count, "Checking that no topics were returned");
        }

        [Test]
        public void AllNamespaceQualifiedNamesThatExistFullPositive()
        {
            EstablishRoles("NamespaceOneReaders", "WikiReaders");
            IList topics = _storeOne.AllQualifiedTopicNamesThatExist("TopicOne");

            Assert.AreEqual(2, topics.Count, "Checking that two topics were returned");
        }

        [Test]
        public void AllNamespaceQualifiedNamesThatExistPartialPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            IList topics = _storeOne.AllQualifiedTopicNamesThatExist("TopicOne");

            Assert.AreEqual(1, topics.Count, "Checking that only one topic was returned");
        }

        [Test]
        public void AllChangesForTopicNegative()
        {
            EstablishRoles("NoAccess");
            string topic = "NamespaceOneTopic";
            int count = CountEnumerable(_storeOne.AllChangesForTopic(topic));

            Assert.AreEqual(0, count, "Checking that no details were returned");
        }

        [Test]
        public void AllChangesForTopicPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            string topic = "NamespaceOneTopic";
            int count = CountEnumerable(_storeOne.AllChangesForTopic(topic));

            Assert.IsTrue(count > 0, "Checking that at least one change was returned");
        }

        [Test]
        public void AllChangesForTopicSinceNegative()
        {
            string topicName = "NamespaceOneTopic";

            // Temporarily establish elevated privileges so we can make a change
            EstablishRoles("NamespaceOneAdministrators");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, topicName, "Changes", "SecurityTests");

            // Drop back to having no access
            EstablishRoles();
            DateTime since = DateTime.MinValue;
            int count = CountEnumerable(_storeOne.AllChangesForTopicSince(topicName, since));
            Assert.AreEqual(0, count, "Checking that no changes were returned.");
        }
        [Test]
        public void AllChangesForTopicSincePositive()
        {
            EstablishRoles("NamespaceOneReaders");
            string topicName = "NamespaceOneTopic";
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, topicName, "Changes", "SecurityTests");
            DateTime since = DateTime.MinValue;
            int count = CountEnumerable(_storeOne.AllChangesForTopicSince(topicName, since));
            Assert.IsTrue(count > 0, "Checking that at least one change was returned.");
        }
        [Test]
        public void AllReferencesByTopicCanReadOtherNamespace()
        {
            // Check that when we have access to the other namespace, we can see all the existing 
            // references
            EstablishRoles("NamespaceOneReaders", "WikiReaders");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("ReferencingTopic", _storeOne.Namespace);
            QualifiedTopicRevisionCollection references = _storeOne.AllReferencesByTopic(topic.LocalName, 
                ExistencePolicy.ExistingOnly);
            Assert.AreEqual(3, references.Count, "Checking that all extant references were returned");
        }

        [Test]
        public void AllReferencesByTopicCannotReadOtherNamespace()
        {
            // Check that when we have no access to the other namespace, we can't see all the existing 
            // references
            EstablishRoles("NamespaceOneReaders");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("ReferencingTopic", _storeOne.Namespace);
            QualifiedTopicRevisionCollection references = _storeOne.AllReferencesByTopic(topic.LocalName, 
                ExistencePolicy.ExistingOnly);
            Assert.AreEqual(2, references.Count, "Checking that all extant references were returned");
        }

        [Test]
        public void AllReferencesByTopicNoAccess()
        {
            // Check that if we don't have read access to the topicName, then we shouldn't even
            // be able to find out what references it might have. 
            EstablishRoles();
            QualifiedTopicRevision topic = new QualifiedTopicRevision("ReferencingTopic", _storeOne.Namespace);
            QualifiedTopicRevisionCollection references = _storeOne.AllReferencesByTopic(topic.LocalName, 
                ExistencePolicy.All);
            Assert.AreEqual(0, references.Count, "Checking that no references were retrieved");
        }

        [Test]
        public void AllReferencesByTopicNonValidating()
        {
            // Check that even when we have no read access to foreign namespaces, we can still
            // know that a potential reference exists
            EstablishRoles("NamespaceOneReaders");
            QualifiedTopicRevision topic = new QualifiedTopicRevision("ReferencingTopic", _storeOne.Namespace);
            QualifiedTopicRevisionCollection references = _storeOne.AllReferencesByTopic(topic.LocalName, 
                ExistencePolicy.All);
            Assert.AreEqual(4, references.Count, "Checking that all references were retrieved");
        }

        [Test]
        public void AllTopicsNegative()
        {
            // Check that nothing is returned when there is no read access to the 
            // namespace, even when there is access to an imported namespace.
            EstablishRoles("WikiReaders");
            int count = CountEnumerable(_storeOne.AllTopics(ImportPolicy.IncludeImports));
            Assert.AreEqual(0, count, "Checking that no topics were returned");
        }
        [Test]
        public void AllTopicsPartial()
        {
            // Check that imported topics are not returned if no read access is allowed
            EstablishRoles("NamespaceOneReaders");
            int countWithImport = CountEnumerable(_storeOne.AllTopics(ImportPolicy.IncludeImports));
            int countWithoutImport = CountEnumerable(_storeOne.AllTopics(ImportPolicy.DoNotIncludeImports));

            Assert.AreEqual(countWithoutImport, countWithImport, "Checking that correct number of topics was returned");
        }

        [Test]
        public void AllTopicsPositive()
        {
            // Check that all topics, including the ones in the imported namespace, 
            // are returned when access is granted.
            EstablishRoles("NamespaceOneReaders", "WikiReaders");
            int countOneOnly = CountEnumerable(_storeOne.AllTopics(ImportPolicy.DoNotIncludeImports));
            int countTwoOnly = CountEnumerable(_storeTwo.AllTopics(ImportPolicy.DoNotIncludeImports));
            int countWithImport = CountEnumerable(_storeOne.AllTopics(ImportPolicy.IncludeImports));

            Assert.AreEqual(countOneOnly + countTwoOnly, countWithImport,
              "Checking that all imported topics were included");
        }

        [Test]
        public void AllTopicsInfoNegative()
        {
            // Check that nothing is returned when there is no read access to the 
            // namespace, even when there is access to an imported namespace.
            //EstablishRoles("WikiReaders"); 
            //int count = CountEnumerable(_storeOne.AllTopicsInfo(new ExecutionContext())); 
            //Assert.AreEqual(0, count, "Checking that no topics were returned");
            throw new NotImplementedException();
        }

        [Test]
        public void AllTopicsInfoPartial()
        {
            // Check that imported topics are not returned if no read access is allowed
            //EstablishRoles("NamespaceOneReaders");
            //int countOneOnly = CountEnumerable(_storeOne.AllTopics(ImportPolicy.DoNotIncludeImports));
            //int countWithImport = CountEnumerable(_storeOne.AllTopicsInfo(new ExecutionContext()));

            //Assert.AreEqual(countOneOnly, countWithImport, "Checking that correct number of topics was returned");
            throw new NotImplementedException(); 
        }

        [Test]
        public void AllTopicsInfoPositive()
        {
            // Check that all topics, including the ones in the imported namespace, 
            // are returned when access is granted.
            //EstablishRoles("NamespaceOneReaders", "WikiReaders");
            //int countOneOnly = CountEnumerable(_storeOne.AllTopics(ImportPolicy.DoNotIncludeImports));
            //int countTwoOnly = CountEnumerable(_storeTwo.AllTopics(ImportPolicy.DoNotIncludeImports));
            //int countWithImport = CountEnumerable(_storeOne.AllTopicsInfo(new ExecutionContext()));

            //Assert.AreEqual(countOneOnly + countTwoOnly, countWithImport,
            //  "Checking that all imported topics were included");
            throw new NotImplementedException(); 
        }

        [Test]
        public void AllTopicsSortedLastModifiedDescendingNegative()
        {
            // Check that no topics are returned when no access is granted.
            EstablishRoles();
            int count = CountEnumerable(_storeOne.AllTopicsSortedLastModifiedDescending());

            Assert.AreEqual(0, count, "Checking that no topics were returned.");
        }

        [Test]
        public void AllTopicsSortedLastModifiedDescendingPositive()
        {
            // Check that all topics in the target namespace (but no imports) are returned
            EstablishRoles("NamespaceOneReaders");
            int count = CountEnumerable(_storeOne.AllTopicsSortedLastModifiedDescending());
            int expected = CountEnumerable(_storeOne.AllTopics(ImportPolicy.DoNotIncludeImports));

            Assert.AreEqual(expected, count, "Checking that all non-imported topics were returned.");
        }

        [Test]
        public void AllTopicsWithNegative()
        {
            // Check that when no read access is granted, no topics are returned
            EstablishRoles();
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            TopicInfoArray topics = _storeOne.AllTopicsWith(new ExecutionContext(topicContext), "Foo", "Bar");
            Assert.AreEqual(0, topics.Count, "Checking that no topics were returned");
        }

        [Test]
        public void AllTopicsWithPartial()
        {
            // Check that when read access is granted to one namespace but not to an 
            // imported namespace, no topics are returned from the imported namespace
            EstablishRoles("NamespaceOneReaders");
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            TopicInfoArray topics = _storeOne.AllTopicsWith(new ExecutionContext(topicContext), "Foo", "Bar");
            Assert.AreEqual(1, topics.Count, "Checking that no topics were returned");
        }

        [Test]
        public void AllTopicsWithPositive()
        {
            // Check that when read access is granted, topics are returned even from 
            // imported namespaces
            EstablishRoles("NamespaceOneReaders", "WikiReaders");
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            TopicInfoArray topics = _storeOne.AllTopicsWith(new ExecutionContext(topicContext), "Foo", "Bar");
            Assert.AreEqual(2, topics.Count, "Checking that no topics were returned");
        }

        [Test]
        public void ContactReadNegative()
        {
            EstablishRoles();
            Assert.AreEqual(string.Empty, _storeOne.Contact,
              "Checking that contact is emtpy when no access is granted");
        }

        [Test]
        public void ContactReadPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            Assert.IsTrue(_storeOne.Contact.Length > 0,
              "Checking that contact is not emtpy when access is granted");
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void ContactWriteNegative()
        {
            //// Since this is equivalent to writing to _ContentBaseDefinition, it requires
            //// administrative privilege, and therefore should fail for writers
            //EstablishRoles("NamespaceOneWriters");
            //_storeOne.Contact = "ShouldThrow";
            Assert.Fail(); 
        }

        [Test]
        public void ContactWritePositive()
        {
            //EstablishRoles("NamespaceOneAdministrators");
            //_storeOne.Contact = "ShouldNotThrow";
            Assert.Fail(); 
        }

        [Test]
        public void DefinitionTopicNameNegative()
        {
            EstablishRoles();
            Assert.IsNull(_storeOne.DefinitionTopicName,
              "Checking that nothing is returned when no access is granted");
        }

        [Test]
        public void DefinitionTopicNamePositive()
        {
            EstablishRoles("NamespaceOneReaders");
            Assert.IsNotNull(_storeOne.DefinitionTopicName,
              "Checking that a non-null topic name is returned when no access is granted");
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void DeleteNegative()
        {
            // Bump up our security so we can actually create the namespace to delete
            EstablishRoles("WikiAdministrators", "NamespaceThreeAdministrators");
            NamespaceManager storeThree = CreateStoreThree();

            // This shouldn't be allowed even if you have read access: edit access is required
            EstablishRoles("NamespaceThreeReaders");
            storeThree.DeleteAllTopicsAndHistory();
        }

        [Test]
        public void DeletePositive()
        {
            // Bump up our security so we can actually create the namespace to delete
            EstablishRoles("WikiAdministrators", "NamespaceThreeAdministrators");
            NamespaceManager storeThree = CreateStoreThree();

            EstablishRoles("NamespaceThreeEditors");
            storeThree.DeleteAllTopicsAndHistory();
            Assert.IsFalse(storeThree.Exists, "Checking that the namespace was deleted.");
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void DeleteTopicNegative()
        {
            string topicName = "DeleteTopicNegative";

            // Temporarily elevate privileges so we can write a topicName
            EstablishRoles("NamespaceOneEditors");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, topicName, "content", "SecurityTests");

            // This should fail even if we have read access - edit access is required
            EstablishRoles("NamespaceOneReaders");
            _storeOne.DeleteTopic(topicName);
        }
        [Test]
        public void DeleteTopicPositive()
        {
            string topicName = "DeleteTopicPositive";
            EstablishRoles("NamespaceOneEditors");
            WikiTestUtilities.WriteTestTopicAndNewVersion(_storeOne, topicName, "content", "SecurityTests");
            _storeOne.DeleteTopic(topicName);
            Assert.IsFalse(_storeOne.TopicExists(topicName, ImportPolicy.DoNotIncludeImports),
              "Checking that topic was deleted.");
        }

        [Test]
        public void DescriptionReadNegative()
        {
            EstablishRoles();
            Assert.IsNull(_storeOne.Description,
              "Checking that no description is returned when no access is granted.");
        }
        [Test]
        public void DescriptionReadPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            Assert.IsNotNull(_storeOne.Description,
              "Checking that a non-null description is returned when access is granted.");
        }

        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void DisplaySpacesInWikiLinksReadNegative()
        {
            EstablishRoles();
            // Doesn't actually matter what assertion we use here - it should throw
            Assert.IsNotNull(_storeOne.DisplaySpacesInWikiLinks,
              "Checking that DisplaySpacesInWikiLinks is not returned when no access is granted.");
        }
        [Test]
        public void DisplaySpacesInWikiLinksReadPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            Assert.IsNotNull(_storeOne.DisplaySpacesInWikiLinks,
              "Checking that DisplaySpacesInWikiLinks is returned when access is granted.");
        }
        [Test]
        public void ExistsNegative()
        {
            // If we don't have permission to read this namespace, it should look like
            // it doesn't exist
            EstablishRoles();
            Assert.IsFalse(_storeOne.Exists, "Checking that Exists returns false");
        }
        [Test]
        public void ExistsPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            Assert.IsTrue(_storeOne.Exists, "Checking that Exists returns true");
        }
        [Test]
        public void ExposedContactNegative()
        {
            EstablishRoles();
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.AreEqual(string.Empty, _storeOne.ExposedContact(new ExecutionContext(topicContext)),
              "Checking that ExposedContact is emtpy when no access is granted");
        }

        [Test]
        public void ExposedContactPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.IsTrue(_storeOne.ExposedContact(new ExecutionContext(topicContext)).Length > 0,
              "Checking that ExposedContact is not emtpy when access is granted");
        }

        [Test]
        public void ExposedDescriptionNegative()
        {
            EstablishRoles();
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.IsNull(_storeOne.ExposedDescription(new ExecutionContext(topicContext)),
              "Checking that no ExposedDescription is returned when no access is granted.");
        }
        [Test]
        public void ExposedDescriptionPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.IsNotNull(_storeOne.ExposedDescription(new ExecutionContext(topicContext)),
              "Checking that a non-null ExposedDescription is returned when access is granted.");
        }
        [Test]
        public void ExposedExistsNegative()
        {
            // If we don't have permission to read this namespace, it should look like
            // it doesn't exist
            EstablishRoles();
            Assert.IsFalse(_storeOne.ExposedExists, "Checking that ExposedExists returns false");
        }
        [Test]
        public void ExposedExistsPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            Assert.IsTrue(_storeOne.ExposedExists, "Checking that ExposedExists returns true");
        }
        [Test]
        public void ExposedImageURLNegative()
        {
            EstablishRoles();
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.IsNull(_storeOne.ExposedImageURL(new ExecutionContext(topicContext)),
              "Checking that ExposedImageURL is null when no access is granted.");
        }
        [Test]
        public void ExposedImageURLPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.IsNotNull(_storeOne.ExposedImageURL(new ExecutionContext(topicContext)),
              "Checking that ExposedImageURL is not null when access is granted.");
        }
        [Test]
        public void ExposedImportsNegative()
        {
            EstablishRoles();
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.AreEqual(0, _storeOne.ExposedImports(new ExecutionContext(topicContext)).Count,
              "Checking that no imports are returned when no access is granted.");
        }
        [Test]
        public void ExposedImportsPositive()
        {
            // Check that we can see imports - even for namespaces we don't have 
            // access to - when we're given read access to this namespace
            EstablishRoles("NamespaceOneReaders");
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.AreEqual(1, _storeOne.ExposedImports(new ExecutionContext(topicContext)).Count,
              "Checking that one import is returned when read access is granted.");
        }
        [Test]
        [ExpectedException(typeof(SecurityException))]
        public void ExposedIsReadOnlyNegative()
        {
            EstablishRoles();
            // Doesn't really matter what assertion we use here - this should throw
            bool isReadOnly = _storeOne.ExposedIsReadOnly;
        }
        [Test]
        public void ExposedIsReadOnlyPositive()
        {
            EstablishRoles("NamespaceOneReaders");
            bool isReadOnly = _storeOne.ExposedIsReadOnly;
            // This test succeeds no matter what the result is, as long as it doesn't throw
        }
        [Test]
        public void ExposedNameNegative()
        {
            EstablishRoles();
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.IsNull(_storeOne.ExposedNamespace(new ExecutionContext(topicContext)),
              "Checking that ExposedName returns null when no access is granted.");
        }
        [Test]
        public void ExposedNamePositive()
        {
            EstablishRoles("NamespaceOneReaders");
            TopicContext topicContext = CreateTopicContext(Federation, _storeOne, "NamespaceOneTopic");
            Assert.AreEqual(_storeOneId, _storeOne.ExposedNamespace(new ExecutionContext(topicContext)),
              "Checking that ExposedName returns namespace anem when access is granted.");
        }


        private int CountEnumerable(IEnumerable enumerable)
        {
            int count = 0;
            IEnumerator enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ++count;
            }
            return count;
        }
        private NamespaceManager CreateStoreThree()
        {
            NamespaceManager storeThree = WikiTestUtilities.CreateMockStore(Federation, "NamespaceThree");

            // Create the _ContentBaseDefinition topics so the wiki will actually work
            WikiTestUtilities.WriteTestTopicAndNewVersion(storeThree, storeThree.DefinitionTopicName.LocalName,
              @"Import: SecurityTestsTwo
Contact: CraigAndera
Description: A test namespace
ImageURL: http://localhost/wiki/images/",
              "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(storeThree, "TopicOne", "NamespaceOne.TopicOne test content", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(storeThree, "NamespaceOneTopic", "Foo: Bar\nNamespaceOne.TopicOne test content", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(storeThree, "ReferencingTopic", "References ReferencedTopicOne and ReferencedTopicTwo and SecurityTestsTwo.TopicOne and NonExistentTopic", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(storeThree, "ReferencedTopicOne", "Referenced by another topicName", "SecurityTests");
            WikiTestUtilities.WriteTestTopicAndNewVersion(storeThree, "ReferencedTopicTwo", "Referenced by another topicName", "SecurityTests");

            return storeThree;

        }
        private TopicContext CreateTopicContext(Federation federation, NamespaceManager storeManager, 
      string topicName)
        {
            return new TopicContext(federation, storeManager, new TopicVersionInfo(federation,
              new QualifiedTopicRevision(topicName, storeManager.Namespace)));
        }
        private void EstablishRoles(params string[] roles)
        {
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("Alice"), roles);
        }

    }
}
