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
    public class UnqualifiedTopicName : TopicName
    {
        public UnqualifiedTopicName()
        {
        }

        public UnqualifiedTopicName(string localName)
            : base(localName)
        {
            if (base.Namespace != null)
            {
                throw new ArgumentException("An illegal local name was specified: the namespace separator is not allowed as part of a local name.");
            }
        }

        public override string Namespace
        {
            get
            {
                return null; 
            }
            set
            {
                throw new NotSupportedException("Namespace may not be set on a local name.");
            }
        }
    }
}
