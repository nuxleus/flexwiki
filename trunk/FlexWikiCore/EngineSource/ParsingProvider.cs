using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using FlexWiki.Collections; 

namespace FlexWiki
{
    internal class ParsingProvider : IParsedContentProvider
    {
        internal ParsingProvider(IUnparsedContentProvider next)
        {
            _next = next;
        }

        private NamespaceManager _namespaceManager;
        private IUnparsedContentProvider _next;

        public DateTime Created
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        public bool Exists
        {
            get 
            {
                return ContentProviderChain.Exists; 
            }
        }
        /// <summary>
        /// Implements <see cref="IUnparsedContentProvider.IsReadOnly"/>.
        /// </summary>
        public bool IsReadOnly
        {
            get { return ContentProviderChain.IsReadOnly; }
        }
        public DateTime LastRead
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        public IParsedContentProvider Next
        {
            get
            {
                // Parsing provider is always at the end of the parsed content provider chain
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                // Parsing provider is always at the end of the parsed content provider chain
                throw new Exception("The method or operation is not implemented.");
            }
        }

        private IUnparsedContentProvider ContentProviderChain
        {
            get { return _next; }
        }

        public TopicChangeCollection AllChangesForTopicSince(string topic, DateTime stamp)
        {
            return ContentProviderChain.AllChangesForTopicSince(topic, stamp);
        }
        public NamespaceQualifiedTopicNameCollection AllTopics()
        {
            return ContentProviderChain.AllTopics();
        }
        public void DeleteAllTopicsAndHistory()
        {
            ContentProviderChain.DeleteAllTopicsAndHistory(); 
        }
        public void DeleteTopic(string topic)
        {
            ContentProviderChain.DeleteTopic(topic); 
        }
        public ParsedTopic GetParsedTopic(string topic)
        {
            TextReader textReader = TextReaderForTopic(topic, null);
            if (textReader == null)
            {
                return null;
            }
            string contents = null;
            try
            {
                contents = textReader.ReadToEnd();
            }
            finally
            {
                textReader.Close();
            }

            ParsedTopic parsedTopic = TopicParser.Parse(contents);

            return parsedTopic;
        }
        public void Initialize(NamespaceManager manager)
        {
            ContentProviderChain.Initialize(manager);
            _namespaceManager = manager;
        }
        public bool IsExistingTopicWritable(string topic)
        {
            return ContentProviderChain.IsExistingTopicWritable(topic); 
        }
        public TextReader TextReaderForTopic(string topic, string version)
        {
            return ContentProviderChain.TextReaderForTopic(topic, version);
        }
        public bool TopicExists(string name)
        {
            return ContentProviderChain.TopicExists(name);
        }
        public void Validate()
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public void WriteTopic(string topic, string version, string content)
        {
            ContentProviderChain.WriteTopic(topic, version, content); 
        }
        public void WriteTopicAndNewVersion(string topic, string content, string author)
        {
            ContentProviderChain.WriteTopicAndNewVersion(topic, content, author);
        }

    }
}
