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
using System.Xml.Serialization;

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// Represents a topic name with a namespace.  The namespace must be specified, because this is the unambiguous name
    /// of a topic in a federation.
    /// </summary>
    public class NamespaceQualifiedTopicVersionKey : TopicVersionKey
    {
        ///// <summary>
        ///// Default contructor for XML Serialization.
        ///// </summary>
        //public NamespaceQualifiedTopicVersionKey()
        //{

        //}

        /// <summary>
        /// Create a new <see cref="NamespaceQualifiedTopicVersionKey"/> with a given fully-qualified name
        /// </summary>
        /// <param name="topic">Fully-qualified name (including '.'s)</param>
        public NamespaceQualifiedTopicVersionKey(string topic)
            : base(topic)
        {
            // TODO: throw exception if there's no namespace qualification? Does the base 
            // TODO: class do this already/ 
        }

        public NamespaceQualifiedTopicVersionKey(TopicName topic) : this(topic.LocalName, topic.Namespace)
        {
        }

        /// <summary>
        /// Create a new <see cref="NamespaceQualifiedTopicName" /> with a given unqualified name and namespace
        /// </summary>
        /// <param name="topic">Topic name (no namespace)</param>
        /// <param name="theNamespace">The namespace</param>
        public NamespaceQualifiedTopicVersionKey(string topic, string theNamespace)
            : base(topic, theNamespace)
        {
            throw new NotImplementedException(); 
            /*
            if (topic.IndexOf(TopicName.Separator) > 0)
                throw new ArgumentException("Qualified topic name can not be used when namespace is specified", topic);
             */
        }

        /// <summary>
        /// Create a new topic of the same type as this one with the given string
        /// </summary>
        /// <param name="topic">Fully-qualified name (including '.'s)</param>
        /// <returns></returns>
        public override TopicVersionKey NewOfSameType(string topic)
        {
            return new NamespaceQualifiedTopicVersionKey(topic);
        }


        public NamespaceQualifiedTopicName AsNamespaceQualifiedTopicName()
        {
            return new NamespaceQualifiedTopicName(this.LocalName, this.Namespace); 
        }

        /// <summary>
        /// Answer this object as an <see cref="NamespaceQualfiedTopicVersionKey"/> (which it already is :-))
        /// </summary>
        /// <param name="defaultNamespace">n/a (only useful in other overrides)</param>
        /// <returns></returns>
        public override NamespaceQualifiedTopicVersionKey AsAbsoluteTopicVersionKey(string defaultNamespace)
        {
            return this;
        }

    }
}
