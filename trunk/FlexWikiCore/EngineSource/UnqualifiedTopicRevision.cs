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

namespace FlexWiki
{
    /// <summary>
    /// Summary description for LocalTopicName.
    /// </summary>
    public class UnqualifiedTopicRevision : TopicRevision
    {
        public UnqualifiedTopicRevision()
        {
        }

        public UnqualifiedTopicRevision(string name)
            : base(name)
        {
            if (Namespace != null)
            {
                throw new ArgumentException("An illegal local name was specified: the namespace separator is not allowed as part of a local name."); 
            }
        }

        public UnqualifiedTopicRevision(UnqualifiedTopicName name)
            : base(name, null)
        {
        }

        public UnqualifiedTopicRevision(UnqualifiedTopicName name, string version)
            : base(name, version)
        {
        }

        public UnqualifiedTopicRevision(string name, string version)
            : base(name, null, version)
        {
        }
        
        public UnqualifiedTopicName AsUnqualifiedTopicName()
        {
            return new UnqualifiedTopicName(LocalName);
        }

    }
}
