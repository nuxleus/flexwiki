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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class NamespaceManagerTests
    {
        [Test]
        public void NamespaceQualifiedTopicNameFor()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager storeManager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicName topicName = storeManager.NamespaceQualifiedTopicNameFor("TopicOne");
            Assert.AreEqual("NamespaceOne.TopicOne", topicName.QualifiedName,
                "Checking that NamespaceQualifiedTopicNameFor returns a fully qualified name.");

        }
        [Test]
        public void NamespaceQualifiedTopicNameForNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager storeManager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicName topicName = storeManager.NamespaceQualifiedTopicNameFor("NoSuchTopic");
            Assert.AreEqual("NamespaceOne.NoSuchTopic", topicName.QualifiedName,
                "Checking that NamespaceQualifiedNameFor returns a fully qualified name even for a nonexistent topic.");
        }
        [Test]
        public void AllNamespaceQualifiedNamesThatExistNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SingleTopicNoImports);

            string topicName = "TopicOne";
            NamespaceManager storeManager = federation.NamespaceManagerForNamespace("NamespaceOne");
            NamespaceQualifiedTopicNameCollection topicNames = storeManager.AllNamespaceQualifiedTopicNamesThatExist(topicName);

            Assert.AreEqual(1, topicNames.Count, "Checking that the correct number of topics were returned.");
        }
        [Test]
        public void AllNamespaceQualifiedNamesThatExistWithImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SimpleImport);

            string topicName = "TopicOne";
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");
            NamespaceQualifiedTopicNameCollection topicNames = manager.AllNamespaceQualifiedTopicNamesThatExist(topicName);

            Assert.AreEqual(2, topicNames.Count, "Checking that the correct number of topics was returned.");

            TopicName topicName1 = topicNames[0];
            TopicName topicName2 = topicNames[1];

            Assert.AreEqual("NamespaceOne", topicName1.Namespace, "Checking that the first namespace is correct.");
            Assert.AreEqual("NamespaceTwo", topicName2.Namespace, "Checking that the second namespace is correct.");
        }
        [Test]
        public void AllChangesForTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";
            manager.WriteTopicAndNewVersion(topicName, "Version one", "Author 1");
            manager.WriteTopicAndNewVersion(topicName, "Version two", "Author 2");
            manager.WriteTopicAndNewVersion(topicName, "Version three", "Author 3");

            IList<TopicChange> changes = manager.AllChangesForTopic(topicName);

            Assert.AreEqual(3, changes.Count, "Checking that three changes were registered.");

            AssertTopicChangeCorrect(changes[0], topicName, "Author 1", new DateTime(2004, 10, 28, 14, 11, 01), "Version one");
            AssertTopicChangeCorrect(changes[1], topicName, "Author 2", new DateTime(2004, 10, 28, 14, 11, 02), "Version two");
            AssertTopicChangeCorrect(changes[2], topicName, "Author 3", new DateTime(2004, 10, 28, 14, 11, 03), "Version three");
        }
        [Test]
        public void AllChangesForTopicSingleChange()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";
            manager.WriteTopicAndNewVersion(topicName, "Version one", "Author 1");

            IList<TopicChange> changes = manager.AllChangesForTopic(topicName);

            Assert.AreEqual(1, changes.Count, "Checking that one change was registered.");
            AssertTopicChangeCorrect(changes[0], topicName, "Author 1", new DateTime(2004, 10, 28, 14, 11, 01), "Version one");
        }
        [Test]
        public void AllChangesForTopicNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";

            IList<TopicChange> changes = manager.AllChangesForTopic(topicName);

            Assert.IsNull(changes, "Checking that a null changelist was returned.");
        }
        [Test]
        public void AllChangesForTopicSince()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";
            manager.WriteTopicAndNewVersion(topicName, "Version one", "Author 1");
            manager.WriteTopicAndNewVersion(topicName, "Version two", "Author 2");
            manager.WriteTopicAndNewVersion(topicName, "Version three", "Author 3");

            IList<TopicChange> changes = manager.AllChangesForTopicSince(topicName, new DateTime(2004, 10, 28, 14, 11, 02));

            Assert.AreEqual(2, changes.Count, "Checking that the correct number of changes were returned.");
            AssertTopicChangeCorrect(changes[0], topicName, "Author 2", new DateTime(2004, 10, 28, 14, 11, 02), "Version two");
            AssertTopicChangeCorrect(changes[1], topicName, "Author 3", new DateTime(2004, 10, 28, 14, 11, 03), "Version three");
        }
        [Test]
        public void AllChangesForTopicSinceLaterThanLatestChange()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";
            manager.WriteTopicAndNewVersion(topicName, "Version one", "Author 1");
            manager.WriteTopicAndNewVersion(topicName, "Version two", "Author 2");
            manager.WriteTopicAndNewVersion(topicName, "Version three", "Author 3");

            IList<TopicChange> changes = manager.AllChangesForTopicSince(topicName, new DateTime(2004, 10, 28, 14, 11, 05));

            Assert.AreEqual(0, changes.Count, "Checking that an empty change list was returned.");
        }
        [Test]
        public void AllChangesForTopicSinceNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";

            IList<TopicChange> changes = manager.AllChangesForTopicSince(topicName, new DateTime(2004, 10, 28, 14, 11, 00));

            Assert.IsNull(changes, "Checking that a null changelist was returned.");
        }
        [Test]
        public void AllReferencesByTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.NonImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "ReferencingTopic";
            NamespaceQualifiedTopicVersionKeyCollection referencedTopics = manager.AllReferencesByTopic(topicName, ExistencePolicy.All);

            AssertReferencesCorrect(referencedTopics,
              new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceOne"),
              new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceTwo"),
              new NamespaceQualifiedTopicVersionKey("NonExistentTopic", "NamespaceOne")
            );

        }
        [Test]
        public void AllReferencesByTopicExistingOnly()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.NonImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "ReferencingTopic";
            NamespaceQualifiedTopicVersionKeyCollection referencedTopics = manager.AllReferencesByTopic(topicName, ExistencePolicy.ExistingOnly);

            AssertReferencesCorrect(referencedTopics,
              new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceOne"),
              new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceTwo")
            );
        }
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AllReferencesByTopicNullArgument()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.NonImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            NamespaceQualifiedTopicVersionKeyCollection referencedTopics = manager.AllReferencesByTopic(null, ExistencePolicy.ExistingOnly);
        }
        [Test]
        public void AllReferencesByTopicWithImportExistingOnly()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "ReferencingTopic";
            NamespaceQualifiedTopicVersionKeyCollection referencedTopics = manager.AllReferencesByTopic(topicName, ExistencePolicy.ExistingOnly);

            AssertReferencesCorrect(referencedTopics,
                new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceOne"),
                new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceTwo"),
                new NamespaceQualifiedTopicVersionKey("ImportedTopic", "NamespaceTwo"),
                new NamespaceQualifiedTopicVersionKey("OtherTopic", "NamespaceTwo")
            );

        }
        [Test]
        public void AllReferencesByTopicWithImportNonExistent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "ReferencingTopic";
            NamespaceQualifiedTopicVersionKeyCollection referencedTopics = manager.AllReferencesByTopic(topicName, ExistencePolicy.All);

            AssertReferencesCorrect(referencedTopics,
                new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceOne"),
                new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceTwo"),
                new NamespaceQualifiedTopicVersionKey("NonexistentTopic", "NamespaceOne"),
                new NamespaceQualifiedTopicVersionKey("NonexistentTopic", "NamespaceTwo"),
                new NamespaceQualifiedTopicVersionKey("ImportedTopic", "NamespaceOne"),
                new NamespaceQualifiedTopicVersionKey("ImportedTopic", "NamespaceTwo"),
                new NamespaceQualifiedTopicVersionKey("OtherTopic", "NamespaceTwo")
            );
        }
        [Test]
        public void AllTopicsEmpty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicNameCollection topics = manager.AllTopics(ImportPolicy.DoNotIncludeImports);

            Assert.AreEqual(0, topics.Count, "Checking that no topics were returned.");

        }
        [Test]
        public void AllTopicsMultipleTopicsNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicNameCollection topics = manager.AllTopics(ImportPolicy.DoNotIncludeImports);

            AssertTopicsCorrectUnordered(topics,
                new TopicName("_ContentBaseDefinition", "NamespaceOne"),
                new TopicName("ReferencedTopic", "NamespaceOne"),
                new TopicName("ReferencingTopic", "NamespaceOne"));
        }
        [Test]
        public void AllTopicsMultipleTopicsWithImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicNameCollection topics = manager.AllTopics(ImportPolicy.IncludeImports);

            AssertTopicsCorrectUnordered(topics,
                new TopicName("_ContentBaseDefinition", "NamespaceOne"),
                new TopicName("ReferencingTopic", "NamespaceOne"),
                new TopicName("ReferencedTopic", "NamespaceOne"),
                new TopicName("ReferencedTopic", "NamespaceTwo"),
                new TopicName("ImportedTopic", "NamespaceTwo"),
                new TopicName("OtherTopic", "NamespaceTwo")
                );
        }
        [Test]
        public void AllTopicsSingleTopicNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicNameCollection topics = manager.AllTopics(ImportPolicy.DoNotIncludeImports);

            AssertTopicsCorrectUnordered(topics, new TopicName("TopicOne", "NamespaceOne"));
        }
        [Test]
        public void AllTopicsSortedNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicNameCollection topics = manager.AllTopics(ImportPolicy.DoNotIncludeImports, NameSort);

            AssertTopicsCorrectOrdered(topics,
                new TopicName("_ContentBaseDefinition", "NamespaceOne"),
                new TopicName("ReferencedTopic", "NamespaceOne"),
                new TopicName("ReferencingTopic", "NamespaceOne")
            );
        }
        [Test]
        public void AllTopicsSortedWithImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicNameCollection topics = manager.AllTopics(ImportPolicy.IncludeImports, NameSort);

            AssertTopicsCorrectOrdered(topics,
                new TopicName("_ContentBaseDefinition", "NamespaceOne"),
                new TopicName("ImportedTopic", "NamespaceTwo"),
                new TopicName("OtherTopic", "NamespaceTwo"),
                new TopicName("ReferencedTopic", "NamespaceOne"),
                new TopicName("ReferencedTopic", "NamespaceTwo"),
                new TopicName("ReferencingTopic", "NamespaceOne")
            );
        }
        [Test]
        public void AllTopicsSortedLastModifiedDescending()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicNameCollection topics = manager.AllTopicsSortedLastModifiedDescending();

            AssertTopicsCorrectOrdered(topics,
                new TopicName("ReferencedTopic", "NamespaceOne"),
                new TopicName("ReferencingTopic", "NamespaceOne"),
                new TopicName("_ContentBaseDefinition", "NamespaceOne")
            );
        }
        [Test]
        public void AllTopicsWithNegativeNoProperty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.MultipleTopicsWithProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicInfoArray topics = manager.AllTopicsWith(new ExecutionContext(), "NoSuchProperty", "Bar");

            Assert.AreEqual(0, topics.Count, "Checking that a nonexistent property returns no results.");
        }
        [Test]
        public void AllTopicsWithNegativeNoValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.MultipleTopicsWithProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicInfoArray topics = manager.AllTopicsWith(new ExecutionContext(), "PropertyOne", "Value two");

            Assert.AreEqual(0, topics.Count, "Checking that no results are returned when the value does not match.");
        }
        [Test]
        public void AllTopicsWithPositiveAnyValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.MultipleTopicsWithProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicInfoArray topics = manager.AllTopicsWith(new ExecutionContext(), "OtherProperty", null);

            Assert.AreEqual(2, topics.Count, "Checking that two topics were found.");
            Assert.AreEqual("TopicOne", ((TopicVersionInfo) topics.Array[0]).Name,
                "Checking that the first topic is correct.");
            Assert.AreEqual("TopicTwo", ((TopicVersionInfo) topics.Array[1]).Name,
                "Checking that the second topic is correct.");
        }
        [Test]
        public void AllTopicsWithPositiveParticularValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.MultipleTopicsWithProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicInfoArray topics = manager.AllTopicsWith(new ExecutionContext(), "PropertyTwo", "Value two");

            Assert.AreEqual(2, topics.Count, "Checking that a two topics were found.");
            Assert.AreEqual("TopicOne", ((TopicVersionInfo) topics.Array[0]).Name,
                "Checking that the first topic is correct.");
            Assert.AreEqual("TopicTwo", ((TopicVersionInfo) topics.Array[1]).Name,
                "Checking that the second topic is correct.");
        }
        [Test]
        public void AsString()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string output = manager.AsString;

            Assert.AreEqual("NamespaceOne", output,
                "Checking the the result of calling AsString is the namespace managed by this object.");
        }
        [Test]
        public void CompareTo()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.ImportingReferencingSet);
            NamespaceManager manager1 = federation.NamespaceManagerForNamespace("NamespaceOne");
            NamespaceManager manager2 = federation.NamespaceManagerForNamespace("NamespaceTwo");

            Assert.IsTrue(manager1.CompareTo(manager2) < 0, "Checking that less than comparison is correct.");
            Assert.AreEqual(0, manager1.CompareTo(manager1), "Checking that equivalance comparison is correct.");
            Assert.IsTrue(manager2.CompareTo(manager1) > 0, "Checking that greater than comparison is correct.");
        }
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CompareToExpectedException()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.ImportingReferencingSet);
            NamespaceManager manager1 = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager1.CompareTo(federation);
        }
        [Test]
        public void Contact()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("Craig Andera", manager.Contact, "Checking that contact is correct.");

        }
        [Test]
        public void ContactAbsent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsNull(manager.Contact, "Checking that contact is null when absent.");
        }
        [Test]
        public void DefinitionTopicLocalName()
        {
            Assert.AreEqual("_ContentBaseDefinition", NamespaceManager.DefinitionTopicName,
                "Checking that NamespaceManager.DefinitionTopicLocalName is correct.");
        }
        [Test]
        public void DefinitionTopicName()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicName expected = new TopicName(NamespaceManager.DefinitionTopicName,
                manager.Namespace);

            Assert.AreEqual(expected.QualifiedName, manager.DefinitionTopic.QualifiedName,
                "Checking that DefinitionTopicName is correct.");
        }
        [Test]
        public void DeleteAllTopicsAndHistory()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.DeleteAllTopicsAndHistory();

            Assert.AreEqual(0, manager.AllTopics(ImportPolicy.DoNotIncludeImports).Count,
                "Checking that the contents of the namespace were deleted.");
        }
        [Test]
        public void DeleteTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topic = "TopicOne";
            manager.DeleteTopic(topic);

            // The topic should be gone, and its history should be invisible. 
            Assert.AreEqual(0, manager.AllTopics(ImportPolicy.DoNotIncludeImports).Count, "Checking that no topics are present.");
            Assert.IsNull(manager.AllChangesForTopic(topic),
                "Checking that history for deleted topics is not visible.");

            // However, if the topic is rewritten, the history should still be there. 
            manager.WriteTopicAndNewVersion(topic, "restored content", "author");

            Assert.AreEqual(4, manager.AllChangesForTopic(topic).Count,
                "Checking that history was restored when the topic was rewritten.");

        }
        [Test]
        public void DescriptionDefault()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("", manager.Description, "Checking that description is correct when not specified.");
        }
        [Test]
        public void DescriptionWithValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("The description", manager.Description,
                "Checking that description is correct when specified.");
        }
        [Test]
        public void DisplaySpacesInWikiLinksDefault()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsFalse(manager.DisplaySpacesInWikiLinks,
                "Checking that DisplaySpacesInWikiLinks defaults to false");
        }
        [Test]
        public void DisplaySpacesInWikiLinksOverride()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsTrue(manager.DisplaySpacesInWikiLinks,
                "Checking that the namespace values are picked up.");
        }
        [Test]
        public void DisplaySpacesInWikiLinksInvalidValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.WriteTopicAndNewVersion("_ContentBaseDefinition",
                "DisplaySpacesInWikiLinks: blah", "author");

            Assert.IsFalse(manager.DisplaySpacesInWikiLinks,
                "Checking that an illegal value results is a false answer.");
        }
        [Test]
        public void DisplaySpacesInWikiLinksMissingValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.WriteTopicAndNewVersion("_ContentBaseDefinition",
                "DisplaySpacesInWikiLinks:", "author");

            Assert.IsFalse(manager.DisplaySpacesInWikiLinks,
                "Checking that a missing value results is a false answer.");
        }
        [Test]
        public void ExistsNegative()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace, MockSetupOptions.StoreDoesNotExist);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsFalse(manager.Exists,
                "Checking that Exists returns false when store does not exist.");
        }
        [Test]
        public void ExistsPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsTrue(manager.Exists,
                "Checking that Exists returns true when store exists.");
        }
        [Test]
        public void ExposedContact()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string contact = manager.ExposedContact(new ExecutionContext());

            Assert.AreEqual("Craig Andera", contact, "Checking that contact is correct.");
        }
        [Test]
        public void ExposedContactAbsent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string contact = manager.ExposedContact(new ExecutionContext());

            Assert.IsNull(contact, "Checking that contact is null when absent.");
        }
        [Test]
        public void ExposedDescription()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string description = manager.ExposedDescription(new ExecutionContext());

            Assert.AreEqual("", description, "Checking that description is correct when not specified.");
        }
        [Test]
        public void ExposedDescriptionWithValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string description = manager.ExposedDescription(new ExecutionContext());

            Assert.AreEqual("The description", description,
                "Checking that description is correct when specified.");
        }
        [Test]
        public void ExposedExistsNegative()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace, MockSetupOptions.StoreDoesNotExist);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsFalse(manager.ExposedExists,
                "Checking that Exists returns false when store does not exist.");
        }
        [Test]
        public void ExposedExistsPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsTrue(manager.ExposedExists,
                "Checking that Exists returns true when store exists.");
        }
        [Test]
        public void ExposedImageURL()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("http://server/vdir/", manager.ExposedImageURL(new ExecutionContext()),
                "Checking that ImageURL returns correct value.");
        }
        [Test]
        public void ExposedImageURLContentBaseDefinitionAbsent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsNull(manager.ExposedImageURL(new ExecutionContext()),
                "Checking that ImageURL returns null when _ContentBaseDefinition is not present.");
        }
        [Test]
        public void ExposedImageURLPropertyAbsent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.WriteTopicAndNewVersion(manager.DefinitionTopic.LocalName, "", "author");

            Assert.IsNull(manager.ExposedImageURL(new ExecutionContext()),
                "Checking that ImageURL returns null when _ContentBaseDefinition has no ImageURL property.");
        }
        [Test]
        public void ExposedImportsImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager storeManager = federation.NamespaceManagerForNamespace("NamespaceOne");

            ArrayList imports = storeManager.ExposedImports(new ExecutionContext());

            Assert.AreEqual(0, imports.Count, "Checking that no imported namespaces were reported.");
        }
        [Test]
        public void ExposedImportsWithImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SimpleImport);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            ArrayList imports = manager.ExposedImports(new ExecutionContext());

            Assert.AreEqual(1, imports.Count, "Checking that the right number of namespaces were imported.");
            Assert.AreSame(federation.NamespaceManagerForNamespace("NamespaceTwo"),
                imports[0], "Checking that the right manager was imported.");

        }
        [Test]
        public void ExposedIsReadOnlyNegative()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsFalse(manager.ExposedIsReadOnly, "Checking that ExposedIsReadOnly returns false from a writable store.");
        }
        [Test]
        public void ExposedIsReadOnlyPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleTopicNoImports, MockSetupOptions.ReadOnlyStore);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsTrue(manager.ExposedIsReadOnly, "Checking that ExposedIsReadOnly returns true from a read-only store.");
        }
        [Test]
        public void ExposedNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("NamespaceOne", manager.ExposedNamespace(new ExecutionContext()),
                "Checking that namespace is returned correctly.");
        }
        [Test]
        public void ExposedTitle()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("The title", manager.ExposedTitle(new ExecutionContext()),
                "Checking that title is correct.");

        }
        [Test]
        public void ExposedTitleAbsent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual(manager.Namespace, manager.ExposedTitle(new ExecutionContext()),
                "Checking that ExposedTitle is equal to namespace when not explicitly specified.");
        }
        [Test]
        public void ExternalReferencesEmptyExternalWikis()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topic = "ExternalWikis";
            manager.WriteTopicAndNewVersion(topic, "", "author");

            Assert.AreEqual(0, manager.ExternalReferences.Count,
                "Checking that ExternalReferences is valid but empty when ExternalWikis is empty.");
        }
        [Test]
        public void ExternalReferencesMultipleExternalWikis()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topic = "ExternalWikis";
            manager.WriteTopicAndNewVersion(topic, @"@foo=http://www.google.com/
@bar=http://msdn2.microsoft.com/library", "author");

            Assert.AreEqual(2, manager.ExternalReferences.Count,
                "Checking that ExternalReferences has a two entries.");
            Assert.AreEqual("http://www.google.com/", manager.ExternalReferences["foo"],
                "Checking that the first entry is correct.");
            Assert.AreEqual("http://msdn2.microsoft.com/library", manager.ExternalReferences["bar"],
                "Checking that the second entry is correct.");
            Assert.IsNull(manager.ExternalReferences["quux"],
                "Checking that a nonexistent key returns null.");
        }
        [Test]
        public void ExternalReferencesNoExternalWikis()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual(0, manager.ExternalReferences.Count,
                "Checking that ExternalReferences is valid but empty when ExternalWikis is absent.");
        }
        [Test]
        public void ExternalReferencesSingleExternalWikis()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topic = "ExternalWikis";
            manager.WriteTopicAndNewVersion(topic, "@foo=http://www.google.com/", "author");

            Assert.AreEqual(1, manager.ExternalReferences.Count,
                "Checking that ExternalReferences has a single entry.");
            Assert.AreEqual("http://www.google.com/", manager.ExternalReferences["foo"],
                "Checking that the first entry is correct.");
        }
        [Test]
        public void ExternalWikisTopic()
        {
            Assert.AreEqual("ExternalWikis", NamespaceManager.ExternalWikisTopic,
                "Checking that the correct name is returned for ExternalWikisTopic.");
        }
        [Test]
        public void FederationProperty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreSame(federation, manager.Federation, "Checking that the federation was properly set.");
        }
        [Test]
        public void FriendlyTitleFromNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string friendlyTitle = manager.FriendlyTitle;

            Assert.AreEqual(friendlyTitle, manager.Namespace,
                "Checking that friendly title is equal to namespace in absence of Title.");
        }
        [Test]
        public void FriendlyTitleFromTitle()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string friendlyTitle = manager.FriendlyTitle;

            Assert.AreEqual(friendlyTitle, manager.Title,
                "Checking that friendly title comes from Title property when present.");
        }
        [Test]
        public void GetReferenceMapAll()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.NonImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            ReferenceMap map = manager.GetReferenceMap(ExistencePolicy.All);

            Assert.AreEqual(2, map.Keys.Count, "Checking that two items are present in the map.");

            Assert.AreEqual(3, map["ReferencingTopic"].Count, "Checking that ReferencingTopic references three topics.");
            Assert.AreEqual(
                new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceOne"),
                map["ReferencingTopic"][0],
                "Checking that the first reference in ReferencingTopic is correct.");
            Assert.AreEqual(
                new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceTwo"),
                map["ReferencingTopic"][1],
                "Checking that the second reference in ReferencingTopic is correct.");
            Assert.AreEqual(
                new NamespaceQualifiedTopicVersionKey("NonExistentTopic", "NamespaceOne"),
                map["ReferencingTopic"][2],
                "Checking that the third reference in ReferencingTopic is correct.");

            Assert.AreEqual(0, map["ReferencedTopic"].Count,
                "Checking that ReferenedTopic does not reference any other topics.");
        }
        [Test]
        public void GetReferenceMapExistingOnly()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.NonImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            ReferenceMap map = manager.GetReferenceMap(ExistencePolicy.ExistingOnly);

            Assert.AreEqual(2, map.Keys.Count, "Checking that two items are present in the map.");

            Assert.AreEqual(2, map["ReferencingTopic"].Count, "Checking that ReferencingTopic references two topics.");
            Assert.AreEqual(
                new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceOne"),
                map["ReferencingTopic"][0],
                "Checking that the first reference in ReferencingTopic is correct.");
            Assert.AreEqual(
                new NamespaceQualifiedTopicVersionKey("ReferencedTopic", "NamespaceTwo"),
                map["ReferencingTopic"][1],
                "Checking that the second reference in ReferencingTopic is correct.");

            Assert.AreEqual(0, map["ReferencedTopic"].Count,
                "Checking that ReferenedTopic does not reference any other topics.");
        }
        [Test]
        public void GetTopicCreationTimeFirstVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            // Flip through the history and get the oldest version
            string topicName = "TopicOne";
            TopicChangeCollection changes = manager.AllChangesForTopic(topicName);

            // Now ask for the time for the oldest version
            DateTime creationTime = manager.GetTopicCreationTime(topicName, changes[0].Version);

            Assert.AreEqual(new DateTime(2004, 10, 28, 14, 11, 02), creationTime,
                "Checking that the creation time for the first version is correct.");
        }
        [Test]
        public void GetTopicCreationTimeLatestVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";
            DateTime creationTime = manager.GetTopicCreationTime(topicName, null);

            Assert.AreEqual(new DateTime(2004, 10, 28, 14, 11, 06), creationTime,
                "Checking that the creation time for the latest version is correct.");
        }
        [Test]
        public void GetTopicCreationTimeModifiedVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            // Flip through the history and get the oldest version
            string topicName = "TopicOne";
            TopicChangeCollection changes = manager.AllChangesForTopic(topicName);

            manager.WriteTopic(topicName, changes[0].Version, "Modified content");

            DateTime creationTime = manager.GetTopicCreationTime(topicName, changes[0].Version);

            Assert.AreEqual(new DateTime(2004, 10, 28, 14, 11, 02), creationTime,
                "Checking that the creation time for the first version doesn't change even when modified.");
        }
        [Test]
        [ExpectedException(typeof(TopicNotFoundException))]
        public void GetTopicCreationTimeNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "NoSuchTopic";
            DateTime creationTime = manager.GetTopicCreationTime(topicName, null);
        }
        [Test]
        [ExpectedException(typeof(TopicNotFoundException))]
        public void GetTopicCreationTimeNonExistentVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            DateTime creationTime = manager.GetTopicCreationTime("TopicOne", "nosuchversion");
        }
        [Test]
        public void GetTopicInfoNegative()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicVersionInfo topicInfo = manager.GetTopicInfo("NoSuchTopic");

            // Note that even if the topic does not exist, a properly set-up object should still
            // come back. 
            Assert.IsFalse(topicInfo.Exists, "Checking that Exists can be called correctly.");
        }
        [Test]
        public void GetTopicInfoPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicVersionInfo topicInfo = manager.GetTopicInfo("TopicOne");

            // Just do some basic tests - full tests belong with unit tests of TopicInfo. 
            Assert.AreSame(federation, topicInfo.Federation,
                "Checking that Federation returns the correct federation.");
            Assert.AreEqual(manager, topicInfo.NamespaceManager,
                "Checking that NamespaceManager returns the correct namespace manager.");
        }
        [Test]
        public void GetTopicLastAuthorMultipleVersions()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string author = manager.GetTopicLastAuthor("TopicOne");

            Assert.AreEqual("author3", author, "Checking that the last author is correct.");
        }
        [Test]
        public void GetTopicLastAuthorNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string author = manager.GetTopicLastAuthor("NoSuchTopic");

            Assert.AreEqual(Federation.AnonymousUserName, author,
                "Checking that the last author is anonymous when the topic does not exist.");
        }
        [Test]
        public void GetTopicLastAuthorSingleVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string author = manager.GetTopicLastAuthor("TopicOne");

            Assert.AreEqual("author", author, "Checking that the last author is correct.");
        }
        [Test]
        public void GetTopicLastModificationTimeLatestVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            DateTime modificationTime = manager.GetTopicLastModificationTime("TopicOne");

            DateTime expectedModificationTime = new DateTime(2004, 10, 28, 14, 11, 02);
            Assert.AreEqual(expectedModificationTime, modificationTime, "Checking that modification time was correct.");
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "Could not locate a topic named NamespaceOne.NoSuchTopic")]
        public void GetTopicLastModificationTimeNonExistent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            DateTime modificationTime = manager.GetTopicLastModificationTime("NoSuchTopic");
        }
        [Test]
        public void GetTopicProperties()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.MultipleTopicsWithProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topic = "TopicOne";

            TopicPropertyCollection properties = manager.GetTopicProperties(topic);

            Assert.AreEqual(3, properties.Count, "Checking that the correct number of properties were returned.");
            AssertTopicPropertyCorrect(properties[0], "PropertyOne", "Value one");
            AssertTopicPropertyCorrect(properties[1], "PropertyTwo", "Value two");
            AssertTopicPropertyCorrect(properties[2], "OtherProperty", "Some value", "Some other value"); 

        }
        [Test]
        public void GetTopicPropertiesNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleTopicWithProperty);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsNull(manager.GetTopicProperties("NoSuchTopic"),
                "Checking that null is returned for a nonexistent topic.");
        }
        [Test]
        public void GetTopicProperty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              new TestContentSet(
                new TestNamespace("NamespaceOne",
                  new TestTopic("TopicOne", "author", @"PropertyOne: ValueOne
PropertyTwo: Value Two
PropertyOne: List, of, values")
                  )
                )
              );

            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicProperty property = manager.GetTopicProperty("TopicOne", "PropertyOne");

            Assert.AreEqual(2, property.Values.Count, "Checking that the correct number of values of PropertyOne are reported.");
            Assert.AreEqual("ValueOne", property.Values[0].RawValue, "Checking that the first rawValue is correct.");
            Assert.AreEqual("List, of, values", property.Values[1].RawValue, "Checking that second rawValue is correct.");

            Assert.AreEqual(3, property.Values[1].AsList().Count, "Checking that the correct number of list values of PropertyOne are reported.");
            Assert.AreEqual("List", property.Values[1].AsList()[0], "Checking that the first list rawValue is correct.");
            Assert.AreEqual("of", property.Values[1].AsList()[1], "Checking that the second list rawValue is correct.");
            Assert.AreEqual("values", property.Values[1].AsList()[2], "Checking that the third list rawValue is correct.");

            Assert.AreEqual(4, property.AsList().Count,
                "Checking that the property as a whole has the right number of values.");
            Assert.AreEqual("ValueOne", property.AsList()[0], "Checking that the first rawValue in the merged list is correct.");
            Assert.AreEqual("List", property.AsList()[1], "Checking that the second rawValue in the merged list is correct.");
            Assert.AreEqual("of", property.AsList()[2], "Checking that the third rawValue in the merged list is correct.");
            Assert.AreEqual("values", property.AsList()[3], "Checking that the fourth rawValue in the merged list is correct.");
        }
        [Test]
        public void GetTopicPropertyNonExistentProperty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicProperty property = manager.GetTopicProperty("TopicOne", "NoSuchProperty");

            Assert.AreEqual(0, property.Values.Count,
                "Checking that zero property values are returned for nonexistent property.");

        }
        [Test]
        public void GetTopicPropertyNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicProperty property = manager.GetTopicProperty("NoSuchTopic", "NoSuchProperty");

            Assert.IsNull(property, "Checking that property comes back null when topic does not exist.");
        }
        [Test]
        public void HomePageDefault()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("HomePage", manager.HomePage,
                "Checking that HomePage returns correct default value.");

        }
        [Test]
        public void HomePageNoValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.WriteTopicAndNewVersion(NamespaceManager.DefinitionTopicName,
                "", "Namespace manager tests");

            Assert.AreEqual("HomePage", manager.HomePage,
                "Checking that the home page defaults to the correct value when no value is specified."); 

        }
        [Test]
        public void HomePageOverride()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("DifferentHomePage", manager.HomePage,
                "Checking that HomePage returns correct nondefault value.");

        }
        [Test]
        public void HomePageSetCreate()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.HomePage = "FooBar";

            Assert.AreEqual("FooBar", manager.HomePage, "Checking that the home page was changed."); 
        }
        [Test]
        public void HomePageSetOverwrite()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.HomePage = "FooBar";
        
            Assert.AreEqual("FooBar", manager.HomePage, "Checking that the home page was changed.");
        }
        [Test]
        public void HomePageTopicNameDefault()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("NamespaceOne.HomePage", manager.HomePageTopicName.QualifiedName,
                "Checking that HomePageTopicName returns correct default value.");

        }
        [Test]
        public void HomePageTopicNameOverride()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("NamespaceOne.DifferentHomePage", manager.HomePageTopicName.QualifiedName,
                "Checking that HomePage returns correct nondefault value.");

        }
        [Test]
        public void ImageURL()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("http://server/vdir/", manager.ImageURL,
                "Checking that ImageURL returns correct value.");
        }
        [Test]
        public void ImageURLContentBaseDefinitionAbsent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsNull(manager.ImageURL,
                "Checking that ImageURL returns null when _ContentBaseDefinition is not present.");
        }
        [Test]
        public void ImageURLPropertyAbsent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.WriteTopicAndNewVersion(manager.DefinitionTopic.LocalName, "", "author");

            Assert.IsNull(manager.ImageURL,
                "Checking that ImageURL returns null when _ContentBaseDefinition has no ImageURL property.");
        }
        [Test]
        public void ImportedNamespaceManagersNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager storeManager = federation.NamespaceManagerForNamespace("NamespaceOne");

            IList<NamespaceManager> importedManagers = storeManager.ImportedNamespaceManagers;

            Assert.AreEqual(0, importedManagers.Count, "Checking that no imported namespaces were reported.");
        }
        [Test]
        public void ImportedNamespaceManagersWithImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SimpleImport);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            IList<NamespaceManager> importedManagers = manager.ImportedNamespaceManagers;

            Assert.AreEqual(1, importedManagers.Count, "Checking that the right number of namespaces were imported.");
            Assert.AreSame(federation.NamespaceManagerForNamespace("NamespaceTwo"),
                importedManagers[0], "Checking that the right manager was imported.");

        }
        [Test]
        public void ImportedNamespacesNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager storeManager = federation.NamespaceManagerForNamespace("NamespaceOne");

            IList<string> importedNamespaces = storeManager.ImportedNamespaces;

            Assert.AreEqual(0, importedNamespaces.Count, "Checking that no imported namespaces were reported.");
        }
        [Test]
        public void ImportedNamespacesWithImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SimpleImport);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            IList<string> importedNamespaces = manager.ImportedNamespaces;

            Assert.AreEqual(1, importedNamespaces.Count, "Checking that the right number of namespaces were imported.");
            Assert.AreEqual("NamespaceTwo", importedNamespaces[0], "Checking that the right namespace was imported.");
        }
        [Test]
        public void IsExistingTopicWritable()
        {
            // Nothing in the default content provider chain should deny write permission.
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            bool isWritable = manager.IsExistingTopicWritable("TopicOne");

            Assert.IsTrue(isWritable, "Checking that existing topic is writable.");
        }
        [Test]
        public void IsExistingTopicWritableNonExistent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            bool isWritable = manager.IsExistingTopicWritable("NoSuchTopic");

            Assert.IsFalse(isWritable, "Checking that false is returned for nonexistent topicName.");

        }
        [Test]
        public void IsReadOnlyNegative()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsFalse(manager.IsReadOnly, "Checking that IsReadOnly returns false from a writable store.");
        }
        [Test]
        public void IsReadOnlyPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleTopicNoImports, MockSetupOptions.ReadOnlyStore);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsTrue(manager.IsReadOnly, "Checking that IsReadOnly returns true from a read-only store.");
        }
        [Test]
        public void LastModifiedNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            DateTime lastModified = manager.LastModified(ImportPolicy.DoNotIncludeImports);

            Assert.AreEqual(new DateTime(2004, 10, 28, 14, 11, 06), lastModified,
                "Checking that last modification time is correct.");
        }
        [Test]
        public void LastModifiedNoTopics()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            DateTime lastModified = manager.LastModified(ImportPolicy.DoNotIncludeImports);

            Assert.AreEqual(DateTime.MinValue, lastModified,
                "Checking that last modification time is correct.");
        }
        [Test]
        public void LastModifiedWithImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            DateTime lastModified = manager.LastModified(ImportPolicy.IncludeImports);

            Assert.AreEqual(new DateTime(2004, 10, 28, 14, 11, 13), lastModified,
                "Checking that last modification time is correct.");
        }
        [Test]
        public void LatestVersionForTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
            TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string version = manager.LatestVersionForTopic("TopicOne");

            Assert.AreEqual("2004-10-28-14-11-06.0000-author3", version, "Checking that version was reported correctly.");
        }
        [Test]
        public void LastestVersionForTopicNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string version = manager.LatestVersionForTopic("NoSuchTopic");

            Assert.IsNull(version, "Checking that version comes back null when the topic does not exist.");
        }
        [Test]
        public void Namespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("NamespaceOne", manager.Namespace, "Checking that namespace is returned correctly.");
        }
        [Test]
        public void Parameters()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleEmptyNamespaceWithParameters);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual(2, manager.Parameters.Count, "Checking that two parameters are present.");
            Assert.AreEqual("foo", manager.Parameters[0].Name, "Checking that the first name is correct.");
            Assert.AreEqual("bar", manager.Parameters[0].Value, "Checking that the first rawValue is correct.");
            Assert.AreEqual("quux", manager.Parameters[1].Name, "Checking that the second name is correct.");
            Assert.AreEqual("baaz", manager.Parameters[1].Value, "Checking that the second rawValue is correct.");
        }
        [Test]
        public void ParametersEmptyParameterList()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual(0, manager.Parameters.Count, "Checking that the parameter list is empty.");
        }
        [Test]
        public void Read()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string content = manager.Read("TopicOne");

            Assert.AreEqual("content", content, "Checking that content was read correctly.");
        }
        [Test]
        public void ReadNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string content = manager.Read("NoSuchTopic");

            Assert.IsNull(content, "Checking that content returns null for a nonexistent topicName.");
        }
        [Test]
        public void ReadNonExistentVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string content = manager.Read("TopicOne", "NoSuchVersion");

            Assert.IsNull(content, "Checking that content returns null for a nonexistent version.");
        }
        [Test]
        public void ReadSpecificVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topic = "TopicOne";
            TopicChangeCollection changes = manager.AllChangesForTopic(topic);

            string content = manager.Read(topic, changes[1].Version);

            Assert.AreEqual("content2", content, "Checking that the right content was retrieved.");

        }
        [Test]
        public void RenameNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string oldName = "NoSuchTopic";
            string newName = "TopicOneRenamed";
            RenameTopicDetails details = manager.RenameTopic(oldName, newName,
                ReferenceFixupPolicy.DoNotFixReferences, "rename");

            Assert.AreEqual(RenameTopicResult.SourceTopicDoesNotExist, details.Result,
                "Checking that the result of the operation was 'No such topicName'.");

        }
        [Test]
        public void RenameTopicNoFixup()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string oldName = "TopicOne";
            string newName = "TopicOneRenamed";
            RenameTopicDetails details = manager.RenameTopic(oldName, newName,
                ReferenceFixupPolicy.DoNotFixReferences, "rename");

            Assert.IsTrue(manager.TopicExists(newName, ImportPolicy.DoNotIncludeImports));
            Assert.IsTrue(manager.TopicExists(oldName, ImportPolicy.DoNotIncludeImports));

            string redirectTo = manager.GetTopicProperty(oldName, "Redirect").LastValue;

            Assert.AreEqual(newName, redirectTo, "Checking that the topic redirect was added.");

            Assert.AreEqual("rename", AuthorForLastChange(manager, oldName),
                "Checking that the attribution was correct for the change to the old topicName.");
            Assert.AreEqual("rename", AuthorForLastChange(manager, newName),
                "Checking that the attribution was correct for the change to the new topicName.");

            Assert.AreEqual(0, details.UpdatedReferenceTopics.Count,
                "Checking that no updated topics were reported.");
            Assert.AreEqual(RenameTopicResult.Success, details.Result,
                "Checking that the result of the operation was 'Success'.");
        }
        [Test]
        public void RenameTopicToExistingTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.NonImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string oldName = "ReferencingTopic";
            string newName = "ReferencedTopic";
            RenameTopicDetails details = manager.RenameTopic(oldName, newName,
                ReferenceFixupPolicy.DoNotFixReferences, "rename");

            Assert.AreEqual(RenameTopicResult.DestinationTopicExists, details.Result,
                "Checking that the result of the operation is 'DestinationTopicExists'.");
            Assert.AreEqual(0, details.UpdatedReferenceTopics.Count,
                "Checking that no topics were reported updated.");

        }
        [Test]
        public void RenameTopicWithFixups()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string oldName = "ReferencedTopic";
            string newName = "ReferencedTopicRenamed";
            RenameTopicDetails details = manager.RenameTopic(oldName, newName,
                ReferenceFixupPolicy.FixReferences, "rename");

            // Note that we don't fix up references from other namespaces. 
            Assert.AreEqual(1, details.UpdatedReferenceTopics.Count,
                "Checking that the right number of fixups were reported.");
            Assert.AreEqual("NamespaceOne.ReferencingTopic", details.UpdatedReferenceTopics[0].QualifiedName,
                "Checking that the right topic was fixed up.");
            string contents = manager.Read("ReferencingTopic");

            Assert.IsTrue(contents.Contains("ReferencedTopicRenamed"),
                "Checking that the topic fixup was done correctly.");
        }
        [Test]
        public void SetTopicPropertyValue()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleTopicWithProperty);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string author = "SetTopicPropertyValue";
            string propertyName = "Property";
            string newValue = "New rawValue";
            string topicName = "TopicOne";
            manager.SetTopicPropertyValue(topicName, propertyName, newValue, false, author);

            TopicProperty properties = manager.GetTopicProperty(topicName, propertyName);
            Assert.AreEqual(1, properties.Values.Count, "Checking that the property has only one value.");
            Assert.AreEqual(newValue, properties.Values[0].RawValue, "Checking that the property rawValue is correct.");

        }
        [Test]
        public void SetTopicPropertyValueDoNotWriteNewVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleTopicWithProperty);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string author = "SetTopicPropertyValue";
            string propertyName = "Property";
            string newValue = "New rawValue";
            string topicName = "TopicOne";
            manager.SetTopicPropertyValue(topicName, propertyName, newValue, false, author);

            IList<TopicChange> changes = manager.AllChangesForTopic(topicName);
            Assert.AreEqual(1, changes.Count, "Checking that no new changes were recorded.");
        }
        [Test]
        public void SetTopicPropertyValueWriteNewVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests/",
             TestContentSets.SingleTopicWithProperty);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string author = "SetTopicPropertyValue";
            string propertyName = "Property";
            string newValue = "New rawValue";
            string topicName = "TopicOne";
            manager.SetTopicPropertyValue(topicName, propertyName, newValue, true, author);

            IList<TopicChange> changes = manager.AllChangesForTopic(topicName);
            Assert.AreEqual(2, changes.Count, "Checking that a new change was recorded.");
            Assert.AreEqual(author, changes[1].Author,
                "Checking that the last change's author is correct.");
        }
        [Test]
        public void TextReaderForTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";

            Assert.AreEqual("content", manager.TextReaderForTopic(topicName).ReadToEnd(),
              "Checking that the supplied text reader had the correct content.");
        }
        [Test]
        public void TextReaderForTopicHistorical()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";

            manager.WriteTopicAndNewVersion(topicName, "new content", "author");

            string historicalVersion = manager.AllChangesForTopic(topicName)[0].Version;

            Assert.AreEqual("new content", manager.TextReaderForTopic(topicName).ReadToEnd(),
              "Checking that latest content is returned when no version is supplied.");
            Assert.AreEqual("content", manager.TextReaderForTopic(topicName, historicalVersion).ReadToEnd(),
              "Checking that historical content is returned when requested.");

        }
        [Test]
        public void TextReaderForTopicNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TextReader textReader = manager.TextReaderForTopic("NoSuchTopic");

            Assert.IsNull(textReader, "Checking that null text reader is returned for nonexistent topicName");
        }
        [Test]
        public void TextReaderForTopicNonexistentVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TextReader textReader = manager.TextReaderForTopic("TopicOne", "foo");

            Assert.IsNull(textReader, "Checking that null text reader is returned for nonexistent version");
        }
        [Test]
        public void Title()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.NamespaceWithInfoProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("The title", manager.Title, "Checking that title is correct.");

        }
        [Test]
        public void TitleAbsent()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
               TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsNull(manager.Title, "Checking that title is null when absent.");
        }
        [Test]
        public void TopicExistsLocalNegativeNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
              TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "OtherTopic";

            Assert.IsFalse(manager.TopicExists(topicName, ImportPolicy.DoNotIncludeImports),
                "Checking that an imported topic returns false from TopicExists.");
        }
        [Test]
        public void TopicExistsLocalNegativeWithImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "NoSuchTopic";

            Assert.IsFalse(manager.TopicExists(topicName, ImportPolicy.IncludeImports),
                "Checking that a nonexistent topic returns false from TopicExists.");
        }
        [Test]
        public void TopicExistsLocalPositiveNoImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "ReferencingTopic";

            Assert.IsTrue(manager.TopicExists(topicName, ImportPolicy.DoNotIncludeImports),
                "Checking that an existing topic returns true from TopicExists when not importing.");
        }
        [Test]
        public void TopicExistsLocalPositiveWithImportLocal()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "ReferencingTopic";

            Assert.IsTrue(manager.TopicExists(topicName, ImportPolicy.IncludeImports),
                "Checking that a local topic returns true from TopicExists when importing.");
        }
        [Test]
        public void TopicExistsLocalPositiveWithImportRemote()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "OtherTopic";

            Assert.IsTrue(manager.TopicExists(topicName, ImportPolicy.IncludeImports),
                "Checking that a remote topic returns true from TopicExists when importing.");
        }
        [Test]
        public void TopicExistsRelativeUnqualifiedNegative()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            RelativeTopicVersionKey topicName = new RelativeTopicVersionKey("NoSuchTopic");

            Assert.IsFalse(manager.TopicExists(topicName, ImportPolicy.DoNotIncludeImports),
                "Checking that a nonexistent topic returns false from TopicExists.");

        }
        [Test]
        public void TopicExistsRelativeUnqualifiedPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            RelativeTopicVersionKey topicName = new RelativeTopicVersionKey("ReferencingTopic");

            Assert.IsTrue(manager.TopicExists(topicName, ImportPolicy.DoNotIncludeImports),
                "Checking that an existing topic returns true from TopicExists.");

        }
        [Test]
        public void TopicExistsRelativeQualifiedNegative()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            RelativeTopicVersionKey topicName = new RelativeTopicVersionKey("NoSuchTopic", "SomeNamespace");

            Assert.IsFalse(manager.TopicExists(topicName, ImportPolicy.DoNotIncludeImports),
                "Checking that a nonexistent topic returns false from TopicExists.");
        }
        [Test]
        public void TopicExistsRelativeQualifiedPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.ImportingReferencingSet);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            RelativeTopicVersionKey topicName = new RelativeTopicVersionKey("OtherTopic", "NamespaceTwo");

            Assert.IsTrue(manager.TopicExists(topicName, ImportPolicy.DoNotIncludeImports),
                "Checking that an existing topic returns true from TopicExists.");
        }
        [Test]
        public void TopicNamespacesForImport()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SimpleImport);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            NamespaceCollection namespaces = manager.TopicNamespaces("TopicOne");

            Assert.AreEqual(2, namespaces.Count, "Checking that topic exists in two namespaces.");
            Assert.IsTrue(namespaces.Contains("NamespaceOne"),
                "Checking that the first namespace is reported correctly.");
            Assert.IsTrue(namespaces.Contains("NamespaceTwo"),
                "Checking that the second namespace is reported correctly.");
        }
        [Test]
        public void TopicNamespacesNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SimpleImport);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            NamespaceCollection namespaces = manager.TopicNamespaces("NoSuchTopic");

            Assert.AreEqual(0, namespaces.Count,
                "Checking that no namespaces were returned for a nonexistent topic.");
        }
        [Test]
        public void Topics()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SimpleImport);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            ArrayList topics = manager.Topics(new ExecutionContext());

            Assert.AreEqual(2, topics.Count, "Checking that two topics were returned.");
            Assert.AreEqual("NamespaceOne.TopicOne", ((TopicVersionInfo) topics[0]).ExposedFullname,
                "Checking that the first topic returned was correct.");
            Assert.AreEqual("NamespaceOne._ContentBaseDefinition", ((TopicVersionInfo) topics[1]).ExposedFullname,
                "Checking that the second topic returned was correct.");
        }
        [Test]
        public void TopicsWithNoValueSpecified()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicInfoArray topics = manager.TopicsWith(new ExecutionContext(), "OtherProperty", null);

            Assert.AreEqual(2, topics.Count, "Checking that two topics were returned.");
            Assert.AreEqual("NamespaceOne.TopicOne", ((TopicVersionInfo) (topics.Array[0])).ExposedFullname,
                "Checking that the first topic returned was correct.");
            Assert.AreEqual("NamespaceOne.TopicTwo", ((TopicVersionInfo) (topics.Array[1])).ExposedFullname,
                "Checking that the first topic returned was correct.");
        }
        [Test]
        public void TopicsWithValueSpecified()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithProperties);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicInfoArray topics = manager.TopicsWith(new ExecutionContext(), "OtherProperty", "Some value");

            Assert.AreEqual(1, topics.Count, "Checking that only one topic was returned.");
            Assert.AreEqual("NamespaceOne.TopicOne", ((TopicVersionInfo) (topics.Array[0])).ExposedFullname,
                "Checking that the right topic was returned.");
        }
        [Test]
        public void ToStringTest()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.AreEqual("NamespaceOne", manager.ToString(),
                "Checking that ToString returns the correct result.");
        }
        [Test]
        [ExpectedException(typeof(TopicIsAmbiguousException))]
        public void UnambiguousTopicNameForAmbiguous()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SimpleImport);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            manager.UnambiguousTopicNameFor("TopicOne");
        }
        [Test]
        public void UnambiguousTopicNameForNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SingleEmptyNamespace);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            Assert.IsNull(manager.UnambiguousTopicNameFor("NoSuchTopic"),
                "Checking that null is returned for a nonexistent topic.");
        }
        [Test]
        public void UnambiguousTopicNameForUnambiguous()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.SimpleImport);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicName topicName = manager.UnambiguousTopicNameFor("_ContentBaseDefinition");

            Assert.AreEqual("NamespaceOne._ContentBaseDefinition", topicName.QualifiedName,
                "Checking that the right topic was returned.");
        }
        [Test]
        public void VersionPreviousTo()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicChangeCollection changes = manager.AllChangesForTopic("TopicOne");

            NamespaceQualifiedTopicVersionKey key = manager.VersionPreviousTo("TopicOne", changes[1].Version);

            Assert.AreEqual("NamespaceOne.TopicOne", key.QualifiedName,
                "Checking that the right topic is returned.");
            Assert.AreEqual(changes[0].Version, key.Version,
                "Checking that the right version is returned.");
        }
        [Test]
        public void VersionPreviousToFirstVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicChangeCollection changes = manager.AllChangesForTopic("TopicOne");

            NamespaceQualifiedTopicVersionKey key = manager.VersionPreviousTo("TopicOne", changes[0].Version);

            Assert.IsNull(key, "Checking that null is returned when asking for the version previous to the first.");
        }
        [Test]
        public void VersionPreviousToNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            NamespaceQualifiedTopicVersionKey key = manager.VersionPreviousTo("NoSuchTopic", null);

            Assert.IsNull(key, "Checking that null key is returned when requesting a nonexistent topic.");
        }
        [Test]
        public void VersionPreviousToNonExistentVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            NamespaceQualifiedTopicVersionKey key = manager.VersionPreviousTo("TopicOne", "Foobar");

            Assert.IsNull(key, "Checking that null key is returned when requesting a nonexistent version.");
        }
        [Test]
        public void VersionPreviousToNullVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            TopicChangeCollection changes = manager.AllChangesForTopic("TopicOne");

            NamespaceQualifiedTopicVersionKey key = manager.VersionPreviousTo("TopicOne", null);

            TopicChange penultimate = changes[changes.Count - 2];

            Assert.AreEqual("NamespaceOne.TopicOne", key.QualifiedName,
                "Checking that correct topic was returned.");
            Assert.AreEqual(penultimate.Version, key.Version,
                "Checking that correct version was returned.");
        }
        [Test]
        public void WriteTopicLatestVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";

            int versionCount = manager.AllChangesForTopic(topicName).Count;

            string newContents = "New contents";
            manager.WriteTopic(topicName, newContents);

            string actualContents = manager.Read(topicName);

            Assert.AreEqual(versionCount, manager.AllChangesForTopic(topicName).Count,
                "Checking that no new versions were created.");
            Assert.AreEqual(newContents, actualContents,
                "Checking that content was updated correctly.");
        }
        [Test]
        public void WriteTopicNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "NewTopic";

            string newContents = "New contents";
            manager.WriteTopic(topicName, newContents);

            Assert.AreEqual(newContents, manager.Read(topicName),
                "Checking that topic was written successfully.");

        }
        [Test]
        [ExpectedException(typeof(FlexWikiException), "Topic NamespaceOne.TopicOne does not have a version NoSuchVersion.")]
        public void WriteTopicNonexistentVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
             TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";

            string newContents = "New contents";
            manager.WriteTopic(topicName, "NoSuchVersion", newContents);
        }
        [Test]
        public void WriteTopicSpecificVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topicName = "TopicOne";

            TopicChangeCollection changes = manager.AllChangesForTopic(topicName);

            string newContents = "New contents";
            manager.WriteTopic(topicName, changes[1].Version, newContents);

            string actualContents = manager.Read(topicName, changes[1].Version);

            Assert.AreEqual(changes.Count, manager.AllChangesForTopic(topicName).Count,
                "Checking that no new versions were created.");
            Assert.AreEqual(newContents, actualContents,
                "Checking that content was updated correctly.");

        }
        [Test]
        public void WriteTopicAndNewVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topic = "TopicOne";
            TopicChangeCollection changesBefore = manager.AllChangesForTopic(topic); 

            string content = "New content"; 
            string author = "New author";
            manager.WriteTopicAndNewVersion(topic, content, author);

            TopicChangeCollection changesAfter = manager.AllChangesForTopic(topic);

            Assert.AreEqual(changesBefore.Count + 1, changesAfter.Count,
                "Checking that one new version was created.");
            Assert.AreEqual(content, manager.Read(topic),
                "Checking that the content is correct.");
            Assert.AreEqual(author, changesAfter.Latest.Author,
                "Checking that the correct author was written."); 
        }

        [Test]
        public void WriteTopicAndNewVersionNonexistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);
            NamespaceManager manager = federation.NamespaceManagerForNamespace("NamespaceOne");

            string topic = "NewTopic";
                    string content = "New content";
            string author = "New author";
            manager.WriteTopicAndNewVersion(topic, content, author);

            TopicChangeCollection changesAfter = manager.AllChangesForTopic(topic);

            Assert.AreEqual(1, changesAfter.Count,
                "Checking that one version was created.");
            Assert.AreEqual(content, manager.Read(topic),
                "Checking that the content is correct.");
            Assert.AreEqual(author, changesAfter.Latest.Author,
                "Checking that the correct author was written.");
        }


        private void AssertReferencesCorrect(NamespaceQualifiedTopicVersionKeyCollection actualTopics,
          params NamespaceQualifiedTopicVersionKey[] expectedTopics)
        {
            Assert.AreEqual(expectedTopics.Length, actualTopics.Count, "Checking that the correct number of topics were returned.");

            for (int i = 0; i < actualTopics.Count; i++)
            {
                Assert.AreEqual(expectedTopics[i], actualTopics[i],
                  string.Format("Checking that topic {0} was correct", i));
            }
        }
        private void AssertTopicChangeCorrect(TopicChange actualChange, string expectedName,
          string expectedAuthor, DateTime expectedTimestamp, string message)
        {
            Assert.AreEqual(expectedName, actualChange.Topic.LocalName, "Checking that name was correct for " + message);
            Assert.AreEqual(expectedAuthor, actualChange.Author, "Checking that author was correct for " + message);
            Assert.AreEqual(expectedTimestamp, actualChange.Created, "Checking that timestamp was correct for " + message);
        }
        private void AssertTopicPropertyCorrect(TopicProperty topicProperty, string propertyName, 
            params string[] values)
        {
            Assert.AreEqual(propertyName, topicProperty.Name, "Checking that topic property name was correct.");
            Assert.AreEqual(values.Length, topicProperty.Values.Count,
                "Checking that the correct number of values are present.");

            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values[i], topicProperty.Values[i].RawValue,
                    string.Format("Checking that value {0} is correct.", i)); 
            }
        }
        private void AssertTopicsCorrectOrdered(TopicNameCollection actualTopics,
            params TopicName[] expectedTopics)
        {
            Assert.AreEqual(expectedTopics.Length, actualTopics.Count,
                "Checking that the right number of topics were returned.");

            for (int i = 0; i < expectedTopics.Length; i++)
            {
                string message = string.Format("Checking that topic {0} was present.", expectedTopics[i]);
                Assert.AreEqual(expectedTopics[i].QualifiedName, actualTopics[i].QualifiedName, message);
            }
        }
        private void AssertTopicsCorrectUnordered(TopicNameCollection actualTopics,
            params TopicName[] expectedTopics)
        {
            Assert.AreEqual(expectedTopics.Length, actualTopics.Count,
                "Checking that the right number of topics were returned.");

            for (int i = 0; i < expectedTopics.Length; i++)
            {
                TopicName expectedTopic = expectedTopics[i];
                string message = string.Format("Checking that topic {0} was present.", expectedTopic);

                bool found = false;
                foreach (TopicName actualTopic in actualTopics)
                {
                    if (expectedTopic.QualifiedName == actualTopic.QualifiedName)
                    {
                        found = true;
                        break;
                    }
                }

                Assert.IsTrue(found, message);
            }
        }
        private string AuthorForLastChange(NamespaceManager manager, string topic)
        {
            IList<TopicChange> changes = manager.AllChangesForTopic(topic);
            return changes[changes.Count - 1].Author;
        }
        private int NameSort(TopicName topicA, TopicName topicB)
        {
            if (topicA == null)
            {
                if (topicB == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (topicB == null)
                {
                    return 1;
                }
                else
                {
                    if (topicA.LocalName == topicB.LocalName)
                    {
                        return topicA.Namespace.CompareTo(topicB.Namespace);
                    }
                    else
                    {
                        return topicA.LocalName.CompareTo(topicB.LocalName);
                    }
                }
            }
        }

        #region Old ContentBase tests.
        /*
     * 
     * Old ContentBase tests. Review and reimplement those that are needed.
     * 
     *
    
    [Test] public virtual void TestSerialization()
    {
      MemoryStream ms = new MemoryStream(); 
      XmlWriter wtr = new XmlTextWriter(ms, System.Text.Encoding.UTF8); 
      XmlSerializer ser = new XmlSerializer(typeof(FileSystemStore)); 
      ser.Serialize(wtr, StoreManager); 

      wtr.Close(); 

      // If we got this far, there was no exception. More rigorous 
      // testing would assert XPath expressions against the XML 

    } 

    [Test] public void TestTopicNameSerialization()
    {
      AbsoluteTopicName name1 = StoreManager.TopicNameFor("TopicOne");

      MemoryStream ms = new MemoryStream(); 
      XmlWriter wtr = new XmlTextWriter(ms, System.Text.Encoding.UTF8); 
      XmlSerializer ser = new XmlSerializer(typeof(AbsoluteTopicName)); 
      ser.Serialize(wtr, name1); 
      wtr.Close(); 
    } 



    [Test] public void RenameTest()
    {
      AbsoluteTopicName name1 = StoreManager.TopicNameFor("TopicOne");
      AbsoluteTopicName name2 = StoreManager.TopicNameFor("TopicTwo");
      ArrayList log = StoreManager.RenameTopic(name1.LocalName, "TopicOneRenamed", true);
      Assert.AreEqual(log.Count, 1);
      string t2 = StoreManager.Read(name2.LocalName);
      Assert.AreEqual("Something about TopicOneRenamed and more!", t2);
    }

    [Test] public void RenameAndVerifyVersionsGetRenamedTest()
    {
      LocalTopicName name1 = new LocalTopicName("TopicOne");
      LocalTopicName name2 = new LocalTopicName("TopicOneRenamed");
      StoreManager.RenameTopic(name1, name2.Name, false);

      int c = 0;
      foreach (object x in StoreManager.AllVersionsForTopic(name2))
      {
        x.ToString();		// do something with x just to shut the compiler up
        c++;			
      }
      Assert.AreEqual(c, 4);	// should be 4 versions, even after rename
    }

		
    [Test] public void SimpleVersioningTests()
    {
      ArrayList list = new ArrayList();
      foreach (AbsoluteTopicName s in StoreManager.AllVersionsForTopic(new LocalTopicName("Versioned")))
      {
        list.Add(s);
      }
      Assert.AreEqual(list.Count, 2);
    }

    [Test] public void AllChangesForTopicSinceTests()
    {
      ArrayList list = new ArrayList();
      foreach (TopicChange c in StoreManager.AllChangesForTopicSince(new LocalTopicName("Versioned"), DateTime.MinValue))
      {
        list.Add(c);
      }
      Assert.AreEqual(list.Count, 2);

      list = new ArrayList();
      foreach (string s in StoreManager.AllChangesForTopicSince(new LocalTopicName("Versioned"), DateTime.MaxValue))
      {
        list.Add(s);
      }
      Assert.AreEqual(list.Count, 0);
    }


    [Test] public void TopicTimeTests()
    {
      Assert.IsTrue(StoreManager.GetTopicCreationTime(new LocalTopicName("TopicOlder")) < StoreManager.GetTopicCreationTime(new LocalTopicName("TopicNewer")));
      Assert.AreEqual(StoreManager.GetTopicLastModificationTime(new LocalTopicName("TopicNewer")), StoreManager.LastModified(true));
    }

    [Test] public void AuthorshipTests()
    {
      Assert.AreEqual(StoreManager.GetTopicLastAuthor(new LocalTopicName("TopicNewer")), StoreManager.GetTopicLastAuthor(new LocalTopicName("TopicOlder")));
    }

    [Test] public void ExternalWikisTests()
    {
      Hashtable t = StoreManager.ExternalWikiHash();
      Assert.AreEqual(t.Count, 2);
      Assert.AreEqual(t["wiki1"], "dozo$$$");
      Assert.AreEqual(t["wiki2"], "fat$$$");
    }

    [Test] public void GetFieldsTest()
    {

      Hashtable t = StoreManager.GetTopicProperties(new LocalTopicName("Props"));
      Assert.AreEqual(t["First"], "one");
      Assert.AreEqual(t["Second"], "two");
      Assert.AreEqual(t["Third"], @"lots
and

lots");
    }

    [Test] public void BasicEnumTest()
    {
      ArrayList expecting = new ArrayList();
      expecting.Add("TopicOne");
      expecting.Add("TopicTwo");
      expecting.Add("Props");
      expecting.Add("ExternalWikis");
      expecting.Add("TopicOlder");
      expecting.Add("Versioned");
      expecting.Add("TopicNewer");
      foreach (string backing in StoreManager.BackingTopics.Keys)
      {
        expecting.Add(backing);
      }			

      foreach (AbsoluteTopicName topicName in StoreManager.AllTopics(ImportPolicy.DoNotIncludeImports))
      {
        Assert.IsTrue(expecting.Contains(topicName.Name), "Looking for " + topicName.Name);
        expecting.Remove(topicName.Name);
      }
      Assert.AreEqual(expecting.Count, 0);
    }

		
    [Test] public void SetFieldsTest()
    {
      NamespaceManager namespaceManager = StoreManager;
      string author = "joe_author";
      AbsoluteTopicName wn = WriteTestTopicAndNewVersion(namespaceManager, "FieldsTesting", "", author);
			
      Hashtable t;
			
      t = namespaceManager.GetTopicProperties(wn.LocalName);
			
      namespaceManager.SetTopicPropertyValue(wn.LocalName, "First", "one", false);
      t = namespaceManager.GetTopicProperties(wn.LocalName);
      Assert.AreEqual(t["First"], "one");

      namespaceManager.SetTopicPropertyValue(wn.LocalName, "Second", "two", false);
      t = namespaceManager.GetTopicProperties(wn.LocalName);
      Assert.AreEqual(t["First"], "one");
      Assert.AreEqual(t["Second"], "two");
			
      namespaceManager.SetTopicPropertyValue(wn.LocalName, "Second", "change", false);
      t = namespaceManager.GetTopicProperties(wn.LocalName);
      Assert.AreEqual(t["First"], "one");
      Assert.AreEqual(t["Second"], "change");

      namespaceManager.SetTopicPropertyValue(wn.LocalName, "First", @"change
is
good", false);
      t = namespaceManager.GetTopicProperties(wn.LocalName);
      Assert.AreEqual(t["First"], @"change
is
good");
      Assert.AreEqual(t["Second"], "change");

      namespaceManager.SetTopicPropertyValue(wn.LocalName, "First", "one", false);
      namespaceManager.SetTopicPropertyValue(wn.LocalName, "Second", "change", false);
      t = namespaceManager.GetTopicProperties(wn.LocalName);
      Assert.AreEqual(t["First"], "one");
      Assert.AreEqual(t["Second"], "change");
    }

    [Test] public void SimpleReadingAndWritingTest()
    {
      LocalTopicName an = new LocalTopicName("SimpleReadingAndWritingTest");
      string c = @"Hello
There";
      StoreManager.WriteTopic(an, c);
      string ret;
      using (TextReader sr = StoreManager.TextReaderForTopic(an))
      {
        ret = sr.ReadToEnd();
      }
      Assert.AreEqual(c, ret);

      StoreManager.DeleteTopic(an);
      Assert.IsTrue(!StoreManager.TopicExistsLocally(an));
    }



    [Test] public void SimpleTopicExistsTest()
    {
      Assert.IsTrue(StoreManager.TopicExists(StoreManager.TopicNameFor("TopicOne")));
    }

    [Test] public void LatestVersionForTopic()
    {
      string author = "LatestVersionForTopicTest"; 
      WriteTestTopicAndNewVersion(StoreManager, "TopicOne", @"A Change", author);
      WriteTestTopicAndNewVersion(StoreManager, "TopicTwo", @"A Change", author);
      WriteTestTopicAndNewVersion(StoreManager, "TopicThree", @"A Change", author);

      NamespaceManager namespaceManager = StoreManager; 
	      
      LocalTopicName atn1 = new LocalTopicName("TopicOne"); 
      LocalTopicName atn2 = new LocalTopicName("TopicTwo"); 
      LocalTopicName atn3 = new LocalTopicName("TopicThree"); 

      IEnumerable versions1 = namespaceManager.AllVersionsForTopic(atn1); 
      IEnumerable versions2 = namespaceManager.AllVersionsForTopic(atn2); 
      IEnumerable versions3 = namespaceManager.AllVersionsForTopic(atn3); 

      string version1 = null; 
      string version2 = null; 
      string version3 = null; 

      foreach (AbsoluteTopicName atn in versions1)
      {
        version1 = atn.Version; 
        break;
      }
      foreach (AbsoluteTopicName atn in versions2)
      {
        version2 = atn.Version; 
        break;
      }
      foreach (AbsoluteTopicName atn in versions3)
      {
        version3 = atn.Version; 
        break;
      }

      Assert.AreEqual(version1, namespaceManager.LatestVersionForTopic(atn1), "Checking that latest version is calculated correctly"); 
      Assert.AreEqual(version2, namespaceManager.LatestVersionForTopic(atn2), "Checking that latest version is calculated correctly"); 
      Assert.AreEqual(version3, namespaceManager.LatestVersionForTopic(atn3), "Checking that latest version is calculated correctly"); 

    }

    [Test] public void CompareTwoTopicVersions()
    {
      AbsoluteTopicName newestTopicVersion = null;
      AbsoluteTopicName oldTopicVersion	 = null;

      oldTopicVersion = WriteTestTopicAndNewVersion(StoreManager, "Versioned", "Original version", "CompareTest");
      System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
      newestTopicVersion = WriteTestTopicAndNewVersion(StoreManager, "Versioned", "Compare this version with an other.", "CompareTest");
			
      Assert.IsNotNull(newestTopicVersion, "Have not found the newer version of topicName");
      Assert.IsNotNull(oldTopicVersion, "Have not found the older version of topicName");

      string[] outputMustContainList = new string[] { @"<p style=""background: palegreen"">Compare this version with an other.</p>", 
                                                      @"<p style=""color: silver; text-decoration: line-through"">Original version</p>" };

      foreach (string outputMustContain in outputMustContainList)
      {
        string o = Federation.GetTopicFormattedContent(newestTopicVersion, oldTopicVersion);
        bool pass = o.IndexOf(outputMustContain) >= 0;
        if (!pass)
        {
          Console.Error.WriteTopicProperty("Got     : " + o);
          Console.Error.WriteTopicProperty("But Couldn't Find: " + outputMustContain);
        }
        Assert.IsTrue(pass, "The result of the compare is not as expected.");
      }

    }


    private static void ComparePropertyUpdatesForVerb(AbsoluteTopicName t, IList expectedProperties, IList gotProperties, string verb)
    {
      foreach (string propertyName in expectedProperties)
      {
        bool found = gotProperties.Contains(propertyName);
        if (!found)
          System.Diagnostics.Debug.WriteTopicProperty("ACK!");
        Assert.IsTrue(found, "Missing " + verb + " propertyName (" + propertyName + " in " + t.FullnameWithVersion + ") from fired event(s)");
        gotProperties.Remove(propertyName);
      }
      if (gotProperties.Count == 0)
        return;	// good -- there should be none left
      string s = "";
      foreach (string p in gotProperties)
      {
        if (s != "")
          s += ", ";
        s += p;
      }
      Assert.Fail("Unexpected " + verb + " propertyName notifications for: " + s);
    }

    */
        #endregion

    }

}
