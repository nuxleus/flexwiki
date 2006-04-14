using System;
using System.Collections.Generic;
using System.Text;

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// Provides a useful base class for parsed content providers. 
    /// </summary>
    /// <remarks>It is not required to derive from this base class. The real
    /// requirement is to implement <see cref="IParsedContentProvider"/>. However, because
    /// many content providers will largely forward class to the next provider in the 
    /// chain, this class may be useful as a base.</remarks>
    public abstract class ForwardingParsedContentProvider : IParsedContentProvider
    {
        // Fields 

        private IParsedContentProvider _next; 

        public IParsedContentProvider Next
        {
            get { return _next; }
            set { _next = value; }
        }

        public virtual bool Exists
        {
            get { return Next.Exists; }
        }
        public virtual bool IsReadOnly
        {
            get { return Next.IsReadOnly; }
        }
        public virtual DateTime LastRead
        {
            get { return Next.LastRead; }
        }

        public virtual TopicChangeCollection AllChangesForTopicSince(string topic, DateTime stamp)
        {
            return Next.AllChangesForTopicSince(topic, stamp); 
        }
        public virtual TopicNameCollection AllTopics()
        {
            return Next.AllTopics(); 
        }
        public virtual void DeleteAllTopicsAndHistory()
        {
            Next.DeleteAllTopicsAndHistory();
        }
        public virtual void DeleteTopic(string topic)
        {
            Next.DeleteTopic(topic); 
        }
        public virtual ParsedTopic GetParsedTopic(string topic)
        {
            throw new NotImplementedException(); 
        }
        public virtual void Initialize(NamespaceManager manager)
        {
            Next.Initialize(manager); 
        }
        public virtual bool IsExistingTopicWritable(string topic)
        {
            return Next.IsExistingTopicWritable(topic); 
        }
        public virtual System.IO.TextReader TextReaderForTopic(string topic, string version)
        {
            return Next.TextReaderForTopic(topic, version);
        }
        public virtual bool TopicExists(string name)
        {
            return Next.TopicExists(name); 
        }
        public virtual void WriteTopic(string topic, string version, string content)
        {
            Next.WriteTopic(topic, version, content); 
        }
        public virtual void WriteTopicAndNewVersion(string topic, string content, string author)
        {
            Next.WriteTopicAndNewVersion(topic, content, author); 
        }

    }
}
