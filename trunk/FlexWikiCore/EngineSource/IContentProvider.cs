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
    public interface IContentProvider
    {
        /// <summary>
        /// Gets a rawValue indicating whether the namespace actually exists. 
        /// </summary>
        /// <remarks>Note that this rawValue could change dynamically, depending on 
        /// (for instance) the security context of the calling user.
        /// </remarks>
        bool Exists { get; }
        /// <summary>
        /// Returns true if the content store is read-only.
        /// </summary>
        /// <remarks>
        /// A read-only content store may not be written to. Note that due to security, 
        /// some users may find a content store to be read-only, while others 
        /// will find it writable.
        /// </remarks>
        bool IsReadOnly { get; }
        DateTime LastRead { get; }

        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="topic">A given date</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <param name="rule">A composite cache rule to fill with rules that represented accumulated dependencies (or null)</param>
        /// <returns>List of <see cref=TopicChange" /> objects.</returns>
        TopicChangeCollection AllChangesForTopicSince(string topic, DateTime stamp);
        /// <summary>
        /// Gets a list of all the <see cref="LocalTopicName"/>s in this content store.
        /// </summary>
        /// <returns>A list of all the <see cref="LocalTopicName"/>s in this content store</returns>
        NamespaceQualifiedTopicNameCollection AllTopics();
        /// <summary>
        /// Delete the contents of a namespace (kills everything inside recursively).  Note that this does *not* include unregistering
        /// the content base within the federation.
        /// </summary>
        void DeleteAllTopicsAndHistory();
        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        void DeleteTopic(string topic);
        /// <summary>
        /// Perform initial setup of the content store chain. 
        /// </summary>
        /// <param name="manager">The <see cref="NamespaceManager"/> that manages
        /// the content store chain.</param>
        void Initialize(NamespaceManager manager);
        /// <summary>
        /// Answer whether a topic exists and is writable
        /// </summary>
        /// <param name="topic">The topic (must directly be in this content base)</param>
        /// <returns>true if the topic exists AND is writable by the current user; else false</returns>
        bool IsExistingTopicWritable(string topic);
        /// <summary>
        /// Answer a TextReader for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
        /// <returns>TextReader</returns>
        TextReader TextReaderForTopic(string topic, string version);
        /// <summary>
        /// Answer true if a topic exists in this namespace
        /// </summary>
        /// <param name="name">Name of the topic</param>
        /// <returns>true if it exists</returns>
        bool TopicExists(string name);
        void WriteTopic(string topic, string version, string content);
        void WriteTopicAndNewVersion(string topic, string content, string author);

    }
}
