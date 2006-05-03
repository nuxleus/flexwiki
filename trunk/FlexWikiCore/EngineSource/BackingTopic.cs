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
    /// Summary description for BackingTopic.
    /// </summary>
    public class BackingTopic
    {
        private string _Body = "";
        private bool _CanOverride;
        private DateTime _CreationTime = DateTime.MinValue;
        private QualifiedTopicRevision _FullName;
        private string _LastAuthor = Federation.AnonymousUserName;
        private DateTime _LastModificationTime = DateTime.MinValue;

        public BackingTopic(QualifiedTopicRevision name)
        {
            _FullName = name;
        }

        public BackingTopic(QualifiedTopicRevision name, string body, bool canOverride)
        {
            _FullName = name;
            Body = body;
            CanOverride = canOverride;
        }


        public QualifiedTopicRevision FullName
        {
            get
            {
                return _FullName;
            }
        }

        public string Body
        {
            get
            {
                return _Body;
            }
            set
            {
                _Body = value;
            }
        }

        public bool CanOverride
        {
            get
            {
                return _CanOverride;
            }
            set
            {
                _CanOverride = value;
            }
        }

        public string LastAuthor
        {
            get
            {
                return _LastAuthor;
            }
            set
            {
                _LastAuthor = value;
            }
        }

        public DateTime CreationTime
        {
            get
            {
                return _CreationTime;
            }
            set
            {
                _CreationTime = value;
            }
        }

        public DateTime LastModificationTime
        {
            get
            {
                return _LastModificationTime;
            }
            set
            {
                _LastModificationTime = value;
            }
        }



    }
}
