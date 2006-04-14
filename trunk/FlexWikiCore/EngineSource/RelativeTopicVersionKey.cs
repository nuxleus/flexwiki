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

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// A RelativeTopicVersionKey is a <see cref="TopicVersionKey" /> which <b>might</b> not have a namespace specified.
    /// </summary>
    public class RelativeTopicVersionKey : TopicVersionKey
    {
        public RelativeTopicVersionKey(string topic)
            : base(topic)
        {
        }

        public bool IsNamespaceQualified
        {
            get { return Namespace != null; }
        }

        public override TopicVersionKey NewOfSameType(string topic)
        {
            return new RelativeTopicVersionKey(topic);
        }


        public RelativeTopicVersionKey(string topic, string theNamespace)
            : base(topic, theNamespace)
        {
        }

        public override NamespaceQualifiedTopicVersionKey AsAbsoluteTopicVersionKey(string baseNamespace)
        {
            if (Namespace == null)
            {
                return new NamespaceQualifiedTopicVersionKey(LocalName, baseNamespace);
            }
            else
            {
                return new NamespaceQualifiedTopicVersionKey(LocalName, Namespace);
            }
        }

    }
}
