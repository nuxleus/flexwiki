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
using System.Collections.Generic;

using FlexWiki.Collections;

namespace FlexWiki
{
    public class ParsedTopic
    {
        private readonly ExternalReferencesMap _externalReferences = new ExternalReferencesMap(); 
        private readonly TopicPropertyCollection _properties = new TopicPropertyCollection();
        private readonly TopicRevisionCollection _topicLinks = new TopicRevisionCollection(); 

        public ParsedTopic(TopicPropertyCollection properties, TopicRevisionCollection topicLinks, 
            ExternalReferencesMap externalReferences)
        {
            _properties.AddRange(properties);
            _topicLinks.AddRange(topicLinks);
            _externalReferences.AddRange(externalReferences); 
        }

        public ExternalReferencesMap ExternalReferences
        {
            get
            {
                return _externalReferences; 
            }
        }

        public TopicPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// Gets a list of all topics that this topic links to. 
        /// </summary>
        /// <remarks>The topics returned may or may not exist. Also, the list does not
        /// include anything but topic links - links to other URIs are not included.</remarks>
        public TopicRevisionCollection TopicLinks
        {
            get
            {
                return _topicLinks; 
            }
        }


    }
}
