using System;
using System.Collections; 
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using FlexWiki.Collections; 

namespace FlexWiki.UnitTests
{
    internal class TraceContentProvider : UnparsedContentProviderBase
    {
        public DateTime Created
        {
            get
            {
                RegisterCall(MethodInfo.GetCurrentMethod());
                throw new NotImplementedException(); 
            }
        }
        public bool Exists
        {
            get 
            { 
                RegisterCall(MethodInfo.GetCurrentMethod());
                throw new NotImplementedException(); 
            }
        }
        public bool IsReadOnly
        {
            get
            {
                RegisterCall(MethodInfo.GetCurrentMethod());
                throw new NotImplementedException(); 
            }
        }
        public UnparsedContentProviderBase Next
        {
            get
            {
                RegisterCall(MethodInfo.GetCurrentMethod());
                throw new NotImplementedException(); 
            }
            set
            {
                RegisterCall(MethodInfo.GetCurrentMethod());
                throw new NotImplementedException();
            }
        }
        public DateTime LastRead 
        {
            get 
            {
                RegisterCall(MethodInfo.GetCurrentMethod());
                throw new NotImplementedException(); 
            }
        }

        public TopicChangeCollection AllChangesForTopicSince(string topic, DateTime stamp)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException(); 
        }
        public QualifiedTopicNameCollection AllTopics()
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public void DeleteAllTopicsAndHistory()
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public void DeleteTopic(string topic)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public RelativeTopicRevisionCollection GetReferences(string referencingTopic)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public DateTime GetTopicCreationTime(string topic)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public DateTime GetTopicLastModificationTime(string topic)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public string GetTopicLastAuthor(string topic)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public TopicPropertyCollection GetTopicProperties(string topic, string property)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public void Initialize(NamespaceManager manager)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public bool IsExistingTopicWritable(string topic)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public string LatestVersionForTopic(string topic)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public DateTime LastModified(bool includeImports)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public System.IO.TextReader TextReaderForTopic(string topic, string version)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public bool TopicExists(string name)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public void Validate()
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public void WriteTopic(string topic, string version, string content)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }
        public void WriteTopicAndNewVersion(string topic, string content, string author)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException();
        }

        private void RegisterCall(MethodBase method)
        {
            RegisterCall(MethodInfo.GetCurrentMethod());
            throw new NotImplementedException(); 
        }

    }
}
