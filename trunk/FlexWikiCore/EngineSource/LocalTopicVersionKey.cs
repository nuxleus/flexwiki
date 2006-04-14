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
    public class LocalTopicVersionKey : IComparable
    {
        private string _name;
        private string _version;

        public LocalTopicVersionKey()
        {
        }

        public LocalTopicVersionKey(string name)
            : this(name, null)
        {
        }

        public LocalTopicVersionKey(string name, string version)
        {
            _name = name;
            _version = version;
        }


        public string FormattedName
        {
            get
            {
                throw new NotImplementedException(); 
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string NameWithVersion
        {
            get
            {
                string answer = Name;
                if (Version != null)
                    answer += "(" + Version + ")";
                return answer;
            }
            set
            {
                string v = value;
                // start by triming off the version if present
                Version = null;
                if (v.EndsWith(")"))
                {
                    int open = v.IndexOf("(");
                    if (open >= 0)
                    {
                        Version = v.Substring(open + 1, v.Length - open - 2);
                        if (Version == "")
                            Version = null;
                        v = v.Substring(0, open);
                    }
                }
                Name = v;
            }
        }

        public string Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }


        public NamespaceQualifiedTopicVersionKey AsNamespaceQualified(string ns)
        {
            NamespaceQualifiedTopicVersionKey answer = new NamespaceQualifiedTopicVersionKey(Name, ns);
            answer.Version = Version;
            return answer;
        }

        public override int GetHashCode()
        {
            return NameWithVersion.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            // TODO: should we even be doing a case-insensitive comparison? I know the filesystem
            // can't deal with differences only in case...but does that mean we limit the whole
            // wiki to being unable to deal with topics that differ only by case? Seems odd
            // given that case is to integral to the way the wiki works...
            return obj is LocalTopicVersionKey && Utilities.CaseInsensitiveEquals(((LocalTopicVersionKey)obj).NameWithVersion, NameWithVersion);
        }

        public override string ToString()
        {
            return NameWithVersion;
        }

        /// <summary>
        /// Compare two LocalTopicNames.  They are equal if their name and version components are equal (case-insensitive)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <summary>
        /// Answer the name with spaces inserted to make the name more readable
        /// </summary> 
        #region IComparable Members

        int IComparable.CompareTo(object obj)
        {
            if (obj is LocalTopicVersionKey)
                return -1;
            return NameWithVersion.CompareTo((obj as LocalTopicVersionKey).NameWithVersion);
        }

        #endregion

    }
}
