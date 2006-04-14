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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using FlexWiki.Formatting;
using System.Xml;
using System.Xml.Serialization;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class FederationTests
    {
        [Test]
        public void GetTopicLastModificationTimeLatestVersion()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);

            DateTime modificationTime = federation.GetTopicLastModificationTime(
                new TopicName("TopicOne", "NamespaceOne"));

            DateTime expectedModificationTime = new DateTime(2004, 10, 28, 14, 11, 06);
            Assert.AreEqual(expectedModificationTime, modificationTime, "Checking that modification time was correct.");
        }
        [Test]
        [ExpectedException(typeof(FlexWikiException), "The namespace NamespaceTwo does not exist.")]
        public void GetTopicLastModificationTimeNonExistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);

            DateTime modificationTime = federation.GetTopicLastModificationTime(
                new TopicName("NoSuchTopic", "NamespaceTwo"));
        }
        [Test]
        [ExpectedException(typeof(ArgumentException), "Could not locate a topic named NamespaceOne.NoSuchTopic")]
        public void GetTopicLastModificationTimeNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);

            DateTime modificationTime = federation.GetTopicLastModificationTime(
                new TopicName("NoSuchTopic", "NamespaceOne"));
        }
        [Test]
        [ExpectedException(typeof(FlexWikiException))]
        public void GetTopicLastModificationTimeRelativeName()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleVersions);

            DateTime modificationTime = federation.GetTopicLastModificationTime(
                new TopicName("TopicOne"));

        }
        [Test]
        public void GetTopicProperty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("TopicOne", "NamespaceOne"),
                "PropertyOne");

            Assert.AreEqual(1, property.Values.Count, "Checking that the property has one value.");
            Assert.AreEqual("Value one", property.LastValue, "Checking that the value is correct."); 
        }
        [Test]
        [ExpectedException(typeof(FlexWikiException), "The namespace NoSuchNamespace does not exist.")]
        public void GetTopicPropertyNonExistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("TopicOne", "NoSuchNamespace"),
                "PropertyOne");
        }
        [Test]
        public void GetTopicPropertyNonExistentProperty()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("TopicOne", "NamespaceOne"), 
                "NoSuchProperty");

            Assert.AreEqual(0, property.Values.Count,
                "Checking that zero property values are returned for nonexistent property.");

        }
        [Test]
        public void GetTopicPropertyNonExistentTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("NoSuchTopic", "NamespaceOne"), 
                "PropertyOne");

            Assert.IsNull(property, "Checking that property comes back null when topic does not exist.");
        }
        [Test]
        [ExpectedException(typeof(FlexWikiException), "A topic name without a namespace was specified where a fully-qualified topic name was expected. The topic name was TopicOne")]
        public void GetTopicPropertyRelativeName()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://NamespaceManagerTests",
                TestContentSets.MultipleTopicsWithProperties);

            TopicProperty property = federation.GetTopicProperty(new TopicName("TopicOne"),
                "PropertyOne");
        }
        [Test]
        public void NamespaceManagerForTopicNegativeNonExistentNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForTopic(
                new NamespaceQualifiedTopicVersionKey("TopicOne", "NoSuchNamespace"));

            Assert.IsNull(manager, "Checking that a null manager is returned when namespace does not exist.");
        }

        [Test]
        public void NamespaceManagerForTopicNegativeNullTopic()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleTopicNoImports);

            NamespaceManager manager = federation.NamespaceManagerForTopic((NamespaceQualifiedTopicVersionKey) null);
            Assert.IsNull(manager, "Checking that a null manager is returned when topic key is null.");

            manager = federation.NamespaceManagerForTopic((TopicName) null);
            Assert.IsNull(manager, "Checking that a null manager is returned when topic name is null.");
        }

        [Test]
        public void NamespaceManagerForTopicPositive()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleTopicNoImports);
            NamespaceManager manager = federation.NamespaceManagerForTopic(
                new NamespaceQualifiedTopicVersionKey("TopicOne", "NamespaceOne"));

            Assert.IsNotNull(manager, "Checking that a non-null manager was returned.");
            Assert.AreEqual("NamespaceOne", manager.Namespace, "Checking that the correct manager was returned."); 
        }

        [Test]
        public void NamespaceManagerForNullNamespace()
        {
            Federation federation = WikiTestUtilities.SetupFederation("test://federationtests",
                TestContentSets.SingleEmptyNamespace); 
            Assert.IsNull(federation.NamespaceManagerForNamespace(null),
              "Checking that NamespaceManagerForNamespace returns null rather than throwing an exception when passed a null namespace.");
        }

        [Test]
        public void RegisterNamespace()
        {
            MockWikiApplication application = new MockWikiApplication(
                new FederationConfiguration(),
                new LinkMaker("test://federationtests"),
                OutputFormat.HTML,
                new MockTimeProvider(TimeSpan.FromSeconds(1))); 

            Federation federation = new Federation(application);
            MockContentStore store = new MockContentStore();
            NamespaceManager storeManager = federation.RegisterNamespace(store, "MockStore");

            Assert.IsNotNull(storeManager, "Checking that a NamespaceManager was created.");
            Assert.AreSame(storeManager, store.NamespaceManager,
              "Checking that the MockContentStore is at the end of the content store chain.");
        }



    }
}
