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
using System.Text.RegularExpressions;

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// Class that holds topic revision information key (composed of three parts: local name, namespace and version).  
    /// There are two key subclasses (<see cref="NamespaceQualfiedTopicName" /> and 
    /// <see cref="LocalTopicName" />).
    /// </summary>
    public class TopicRevision : IComparable
    {
        // Fields 

        private TopicName _topicName;
        private string _version; 

        // Constructors

        public TopicRevision()
        {
        }
        public TopicRevision(string revision)
        {
            if (revision == null)
            {
                throw new ArgumentException("topic cannot be null"); 
            }

            // start by triming off the version if present
            _version = null;
            if (revision.EndsWith(")"))
            {
                int open = revision.IndexOf("(");
                if (open >= 0)
                {
                    _version = revision.Substring(open + 1, revision.Length - open - 2);
                    if (_version == "")
                    {
                        _version = null;
                    }
                    revision = revision.Substring(0, open);
                }
            }

            _topicName = new TopicName(revision); 

        }
        public TopicRevision(string localName, string ns)
        {
            _topicName = new TopicName(localName, ns); 
        }
        public TopicRevision(string localName, string ns, string version)
        {
            _topicName = new TopicName(localName, ns);
            _version = version; 
        }
        public TopicRevision(TopicName name, string version)
        {
            _topicName = name;
            _version = version; 
        }

        // Properties

        public string DottedName
        {
            get
            {
                return Name.DottedName;
            }
        }
        public string DottedNameWithVersion
        {
            get
            {
                string answer = DottedName;
                if (Version != null)
                {
                    answer += "(" + Version + ")";
                }
                return answer;
            }
        }
        /// <summary>
        /// Answer the name (without namespace) with spaces inserted to make the name more readable
        /// </summary> 
        public string FormattedName
        {
            get
            {
                return Name.FormattedName;
            }
        }
        public bool IsQualified
        {
            get { return Name.IsQualified; }
        }
        public string LocalName
        {
            get
            {
                return Name == null ? null : Name.LocalName;
            }
            set
            {
                Name.LocalName = value; 
            }
        }
        public TopicName Name
        {
            get { return _topicName; }
        }
        public string Namespace
        {
            get
            {
                return Name == null ? null : Name.Namespace;
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


        // Methods

        /// <summary>
        /// Answer this as a qualified topic name.  If this is a qualified name already, answer it.  If it isn't, 
        /// answer a qualified name, filling in any unspecified namespace with the supplied default.
        /// </summary>
        /// <param name="defaultNamespace"></param>
        /// <returns></returns>
        public virtual QualifiedTopicRevision ResolveRelativeTo(string defaultNamespace)
        {
            TopicName name = Name.ResolveRelativeTo(defaultNamespace);
            return new QualifiedTopicRevision(name.LocalName, name.Namespace, Version); 
        }
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1; 
            }

            if (!(obj is TopicRevision))
            {
                throw new ArgumentException("obj is not a TopicRevision"); 
            }
            return DottedNameWithVersion.CompareTo((obj as TopicRevision).DottedNameWithVersion);
        }
        /// <summary>
        /// Compare two TopicNames.  Topic names are equal if their name, namespace and version components are equal (case-insensitive)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is TopicRevision && ((TopicRevision)obj).DottedNameWithVersion.ToLower() == DottedNameWithVersion.ToLower();
        }
        public override int GetHashCode()
        {
            return DottedNameWithVersion.GetHashCode();
        }
        public virtual TopicRevision NewOfSameType(string revision)
        {
            return new TopicRevision(revision); 
        }
        /// <summary>
        /// Answer a version string that can be used to identify a topic version for the supplied user.
        /// </summary>
        /// <param name="user">A username string.</param>
        /// <returns>A new version string.</returns>
        /// <remarks>Note that calling this method very rapidly with the same user can result in duplicate
        /// version strings being returned, as DateTime only has a resolution of about 15ms. The fix for this
        /// is to sleep at least 30ms between calls to this method when specifying the same user, or to 
        /// specify different users.</remarks>
        public static string NewVersionStringForUser(string user, ITimeProvider timeProvider)
        {
            return NewVersionStringForUser(user, timeProvider.Now);
        }
        public static string NewVersionStringForUser(string user, DateTime timestamp)
        {
            string u = user;
            u = u.Replace('\\', '-');
            u = u.Replace('?', '-');
            u = u.Replace('/', '-');
            u = u.Replace(':', '-');
            return timestamp.ToString("yyyy-MM-dd-HH-mm-ss.ffff") + "-" + u;
        }
        public override string ToString()
        {
            return DottedNameWithVersion;
        }

    }
}
