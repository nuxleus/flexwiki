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
using System.IO;

using FlexWiki.Collections;

namespace FlexWiki
{
    public abstract class ContentProviderBase
    {
        private NamespaceManager _namespaceManager; 
        private ContentProviderBase _next;

        public ContentProviderBase(ContentProviderBase next)
        {
            _next = next; 
        }

        /// <summary>
        /// Gets a rawValue indicating whether the namespace actually exists. 
        /// </summary>
        /// <remarks>Note that this rawValue could change dynamically, depending on 
        /// (for instance) the security context of the calling user.
        /// </remarks>
        public virtual bool Exists
        {
            get
            {
                return _next.Exists;
            }
        }
        /// <summary>
        /// Returns true if the content store is read-only.
        /// </summary>
        /// <remarks>
        /// A read-only content store may not be written to. Note that due to security, 
        /// some users may find a content store to be read-only, while others 
        /// will find it writable.
        /// </remarks>
        public virtual bool IsReadOnly
        {
            get
            {
                return _next.IsReadOnly;
            }
        }
        public virtual DateTime LastRead
        {
            get
            {
                return _next.LastRead;
            }
        }
        public NamespaceManager NamespaceManager
        {
            get { return _namespaceManager; }
        }

        protected virtual ContentProviderBase Next
        {
            get { return _next; }
            set { _next = value; }
        }

        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="topic">A given date</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <param name="rule">A composite cache rule to fill with rules that represented accumulated dependencies (or null)</param>
        /// <returns>List of <see cref=TopicChange" /> objects.</returns>
        public virtual TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            return _next.AllChangesForTopicSince(topic, stamp);
        }
        /// <summary>
        /// Gets a list of all the <see cref="LocalTopicName"/>s in this content store.
        /// </summary>
        /// <returns>A list of all the <see cref="LocalTopicName"/>s in this content store</returns>
        public virtual QualifiedTopicNameCollection AllTopics()
        {
            return _next.AllTopics();
        }
        /// <summary>
        /// Delete the contents of a namespace (kills everything inside recursively).  Note that this does *not* include unregistering
        /// the content base within the federation.
        /// </summary>
        public virtual void DeleteAllTopicsAndHistory()
        {
            _next.DeleteAllTopicsAndHistory();
        }
        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public virtual void DeleteTopic(UnqualifiedTopicName topic)
        {
            _next.DeleteTopic(topic);
        }
        /// <summary>
        /// Returns the parsed representation of the specified topic. 
        /// </summary>
        /// <param name="topic">The topic for which to return the parsed representation.</param>
        /// <returns>A <see cref="ParsedTopic"/> object containing the parsed representation of the topic.</returns>
        public virtual ParsedTopic GetParsedTopic(UnqualifiedTopicRevision topicRevision)
        {
            return _next.GetParsedTopic(topicRevision);
        }
        public virtual void Initialize(NamespaceManager namespaceManager)
        {
            _namespaceManager = namespaceManager; 
        }
        /// <summary>
        /// Answer whether a topic exists and is writable
        /// </summary>
        /// <param name="topic">The topic (must directly be in this content base)</param>
        /// <returns>true if the topic exists AND is writable by the current user; else false</returns>
        public virtual bool IsExistingTopicWritable(UnqualifiedTopicName topic)
        {
            return _next.IsExistingTopicWritable(topic); 
        }
        /// <summary>
        /// Answer a TextReader for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
        /// <returns>TextReader</returns>
        public virtual TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            return _next.TextReaderForTopic(topicRevision); 
        }
        /// <summary>
        /// Answer true if a topic exists in this namespace
        /// </summary>
        /// <param name="name">Name of the topic</param>
        /// <returns>true if it exists</returns>
        public virtual bool TopicExists(UnqualifiedTopicName name)
        {
            return _next.TopicExists(name); 
        }
        public virtual void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            _next.WriteTopic(topicRevision, content); 
        }
        public virtual void WriteTopicAndNewVersion(UnqualifiedTopicName topic, string content, string author)
        {
            _next.WriteTopicAndNewVersion(topic, content, author); 
        }

    }
}
