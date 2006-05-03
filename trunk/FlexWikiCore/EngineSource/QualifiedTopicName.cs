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

namespace FlexWiki
{
    public class QualifiedTopicName : TopicName
    {
        public QualifiedTopicName()
        {
        }

        public QualifiedTopicName(string name)
            : base(name)
        {
            if (Namespace == null)
            {
                throw new ArgumentException("'" + name + "' does not contain a namespace - a namespace must be specified.");
            }
        }

        public QualifiedTopicName(string localName, string ns)
            : base(localName, ns)
        {
            if (ns == null)
            {
                throw new ArgumentException("A namespace is required."); 
            }

            if (ns.Length == 0)
            {
                throw new ArgumentException("A namespace is required."); 
            }
        }
    }
}
