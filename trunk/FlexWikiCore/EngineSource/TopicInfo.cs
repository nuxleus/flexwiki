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

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// Summary description for Topic.
    /// </summary>
    // TODO: Should we update WikiTalk to reflect the new name of this object at some point?
    [ExposedClass("TopicInfo", "Provides information about a topic")]
    public class TopicVersionInfo : BELObject, IComparable
    {
        // Fields

        private NamespaceManager _namespaceManager;
        private Federation _federation;
        private NamespaceQualifiedTopicVersionKey _topicVersionKey;

        // Constructors

        public TopicVersionInfo(Federation aFed, NamespaceQualifiedTopicVersionKey name)
        {
            _topicVersionKey = name;
            _federation = aFed;
        }

        // Properties

        [ExposedMethod(ExposedMethodFlags.Default, "Answer a list of TopicChanges describing this topic's history")]
        public TopicChangeCollection Changes
        {
            get
            {
                TopicChangeCollection answer = new TopicChangeCollection();
                foreach (TopicChange each in Federation.GetTopicChanges(new TopicName(TopicVersionKey.QualifiedName)))
                {
                    answer.Add(each);
                }
                return answer;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a DateTime inndicating when the topic was created")]
        public DateTime Created
        {
            get
            {
                return Federation.GetTopicCreationTime(TopicVersionKey);
            }
        }
        public bool Exists
        {
            get
            {
                return NamespaceManager.TopicExists(TopicVersionKey.LocalName, ImportPolicy.DoNotIncludeImports);
            }
        }
        [ExposedMethod("Fullname", ExposedMethodFlags.Default, "Answer the complete name of the topic (including namespace and version, if present)")]
        public string ExposedFullname
        {
            get
            {
                return _topicVersionKey.ToString();
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the Federation containing the topic")]
        public Federation Federation
        {
            get
            {
                return _federation;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the Keywords property's rawValue")]
        public string Keywords
        {
            get
            {
                return GetProperty("Keywords");
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer an array containgin all the keywords listed in the Keywords property")]
        public ArrayList KeywordsList
        {
            get
            {
                return GetListProperty("Keywords");
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a DateTime inndicating when the topic was last modified")]
        public DateTime LastModified
        {
            get
            {
                return Federation.GetTopicModificationTime(TopicVersionKey);
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a string indicating who last modified the topic")]
        public string LastModifiedBy
        {
            get
            {
                return Federation.GetTopicLastModifiedBy(TopicName);
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a the (full)name of the topic")]
        public string Name
        {
            get
            {
                return TopicVersionKey.LocalName;
            }
        }
        [ExposedMethod("Namespace", ExposedMethodFlags.Default, "Answer the Namespace for this topic")]
        public NamespaceManager NamespaceManager
        {
            get
            {
                if (_namespaceManager != null)
                    return _namespaceManager;
                _namespaceManager = Federation.NamespaceManagerForTopic(TopicVersionKey);
                return _namespaceManager;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the list of property names for this topic")]
        public IList<string> PropertyNames
        {
            get
            {
                return Federation.GetTopicProperties(TopicVersionKey).Names;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the Summary property's rawValue")]
        public string Summary
        {
            get
            {
                return GetProperty("Summary");
            }
        }
        public TopicName TopicName
        {
            get
            {
                return new TopicName(TopicVersionKey.LocalName, TopicVersionKey.Namespace); 
            }
        }
        public NamespaceQualifiedTopicVersionKey TopicVersionKey
        {
            get
            {
                return _topicVersionKey;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the version stamp for this version of the topic")]
        public string Version
        {
            get
            {
                return TopicVersionKey.Version;
            }
        }

        // Methods

        public int CompareTo(object obj)
        {
            return this.TopicVersionKey.QualifiedName.CompareTo(((TopicVersionInfo)obj).TopicVersionKey.QualifiedName);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer an array of the values in the given list property")]
        public ArrayList GetListProperty(string propertyName)
        {
            return Federation.GetTopicListPropertyValue(TopicVersionKey, propertyName);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the given property from the topic")]
        public string GetProperty(string propertyName)
        {
            return Federation.GetTopicPropertyValue(TopicVersionKey, propertyName);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer true if the given toipic has the given property; else false")]
        public bool HasProperty(string propertyName)
        {
            TopicPropertyCollection properties = Federation.GetTopicProperties(TopicVersionKey);
            if (properties == null)
                return false;
            return properties.Contains(propertyName);
        }
        public override string ToString()
        {
            string answer = "TopicInfo for ";
            if (TopicVersionKey != null)
            {
                answer += TopicVersionKey;
            }
            return answer;
        }
    }
}
