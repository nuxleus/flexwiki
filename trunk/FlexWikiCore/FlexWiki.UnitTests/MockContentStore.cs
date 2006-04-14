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
using System.IO;
using System.Text.RegularExpressions;

using FlexWiki.Collections;

namespace FlexWiki.UnitTests
{
    /// <summary>
    /// A content store whose purpose is to provide storage during unit tests.
    /// </summary>
    internal class MockContentStore : IUnparsedContentProvider
    {
        private NamespaceManager _namespaceManager;
        private DateTime _created;
        private MockSetupOptions _options;
        private readonly MockTopicCollection _topics = new MockTopicCollection();

        internal MockContentStore()
            : this(MockSetupOptions.Default)
        {
        }

        internal MockContentStore(MockSetupOptions options)
        {
            _options = options;
        }

        public DateTime Created
        {
            get
            {
                return _created;
            }
        }

        public bool Exists
        {
            get
            {
                return (_options & MockSetupOptions.StoreDoesNotExist) == 0;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((_options & MockSetupOptions.ReadOnlyStore) != 0);
            }
        }

        public DateTime LastRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NamespaceManager NamespaceManager
        {
            get
            {
                return _namespaceManager;
            }
        }

        public IUnparsedContentProvider Next
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private Federation Federation
        {
            get { return NamespaceManager.Federation; }
        }

        private string Namespace
        {
            get { return NamespaceManager.Namespace; }
        }

        public TopicChangeCollection AllChangesForTopicSince(string topicName, DateTime stamp)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.ExistingOnly);

            // If the topicName does not exist, return a null list
            if (topic == null)
            {
                return null;
            }

            TopicChangeCollection changes = new TopicChangeCollection();

            foreach (MockTopicHistory topicHistory in topic.History)
            {
                if (topicHistory.Created >= stamp)
                {
                    NamespaceQualifiedTopicVersionKey namespaceQualifiedTopic = new NamespaceQualifiedTopicVersionKey(topicName, Namespace);
                    namespaceQualifiedTopic.Version = TopicVersionKey.NewVersionStringForUser(topicHistory.Author, topicHistory.Created);
                    changes.Add(new TopicChange(namespaceQualifiedTopic, topicHistory.Created, topicHistory.Author));
                }
            }

            return changes;
        }

        public TopicNameCollection AllTopics()
        {
            TopicNameCollection topics = new TopicNameCollection();

            foreach (MockTopic topic in AllTopics(ExistencePolicy.ExistingOnly))
            {
                topics.Add(new TopicName(topic.Name, this.Namespace));
            }

            return topics;
        }

        public void DeleteAllTopicsAndHistory()
        {
            _topics.Clear();
        }

        public void DeleteTopic(string topicName)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.All);

            topic.IsDeleted = true;
        }

        public DateTime GetTopicCreationTime(string topicName, string version)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.ExistingOnly);

            if (topic == null)
            {
                throw TopicNotFoundException.ForTopic(topicName, Namespace);
            }

            if (version != null)
            {
                foreach (MockTopicHistory history in topic.History)
                {
                    if (history.Version == version)
                    {
                        return history.Created;
                    }
                }
            }
            else
            {
                return topic.Latest.Created;
            }

            throw TopicNotFoundException.ForTopic(topicName, Namespace);

        }

        //public IList<TopicProperty> GetTopicProperty(LocalTopicName topicName, string propertyName)
        //{
        //  string content = GetTopicContents(topicName); 

        //  if (content != null)
        //  {
        //    TopicParser parser = new TopicParser();
        //    ParsedTopic parsedTopic = parser.Parse(content);

        //    return parsedTopic.GetProperty(propertyName);
        //  }

        //  return null;
        //}

        public void Initialize(NamespaceManager manager)
        {
            _namespaceManager = manager;
            _created = manager.Federation.TimeProvider.Now;
        }

        public bool IsExistingTopicWritable(string topic)
        {
            // We're not implementing security here, so existence is good enough
            // to allow a write. 
            return TopicExists(topic);
        }

        public DateTime LastModified(bool includeImports)
        {
            throw new NotImplementedException();
        }

        public string LatestVersionForTopic(string topic)
        {
            throw new NotImplementedException();
        }

        public bool TopicExists(string name)
        {
            return GetTopic(name, ExistencePolicy.ExistingOnly) != null;
        }

        public TextReader TextReaderForTopic(string topicName, string version)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.ExistingOnly);
            if (topic == null)
            {
                return null;
            }

            MockTopicHistory history = topic[version];

            if (history == null)
            {
                return null;
            }

            return new StringReader(history.Contents);
        }

        public void Validate()
        {
            throw new NotImplementedException();
        }

        public void WriteTopic(string topicName, string version, string content)
        {
            MockTopic topic = RetrieveOrCreateTopic(topicName);

            MockTopicHistory history = null;
            if (topic.History.Count == 0)
            {
                history = new MockTopicHistory(content, "", Federation.TimeProvider.Now);
                topic.History.Add(history);
            }
            else if (version != null)
            {
                foreach (MockTopicHistory h in topic.History)
                {
                    if (version == h.Version)
                    {
                        history = h;
                    }
                }
            }
            else
            {
                history = topic.Latest;
            }

            history.Contents = content;
            history.Modified = Federation.TimeProvider.Now;
            topic.IsDeleted = false;
        }

        public void WriteTopicAndNewVersion(string topicName, string content, string author)
        {
            MockTopic topic = RetrieveOrCreateTopic(topicName);

            topic.History.Add(new MockTopicHistory(content, author, Federation.TimeProvider.Now));

            topic.IsDeleted = false;
        }
#if false
        private IEnumerable<MockTopic> AllTopics(ExistencePolicy existencePolicy)
        {
            MockTopicCollection topics = new MockTopicCollection(); 
            foreach (MockTopic topic in _topics)
            {
                if (existencePolicy == ExistencePolicy.All || topic.IsDeleted == false)
                {
                    topics.Add(topic); 
                }
            }

            return topics; 
        }
#else
        private IEnumerable<MockTopic> AllTopics(ExistencePolicy existencePolicy)
        {
            foreach (MockTopic topic in _topics)
            {
                if (existencePolicy == ExistencePolicy.All || topic.IsDeleted == false)
                {
                    yield return topic;
                }
            }
        }
#endif

        private MockTopic GetTopic(string topicName, ExistencePolicy existencePolicy)
        {
            if (!_topics.Contains(topicName))
            {
                return null;
            }

            MockTopic topic = _topics[topicName];

            if (topic.IsDeleted && existencePolicy == ExistencePolicy.ExistingOnly)
            {
                return null;
            }

            return topic;
        }

        private MockTopic RetrieveOrCreateTopic(string topicName)
        {
            MockTopic topic = GetTopic(topicName, ExistencePolicy.All);

            if (topic == null)
            {
                topic = new MockTopic(topicName);
                _topics.Add(topic);
            }
            return topic;
        }
    }
}
