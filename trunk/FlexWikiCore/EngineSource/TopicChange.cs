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
using System.Text;
using System.Text.RegularExpressions;


namespace FlexWiki
{
    /// <summary>
    /// A TopicChange describes a topic change (which topic, who changed it and when)
    /// </summary>
    [ExposedClass("TopicChange", "Describes a single change to a topic")]
    public class TopicChange : BELObject, IComparable
    {
        // Fields

        private string _author;
        private DateTime _created;
        private DateTime _modified;
        private QualifiedTopicRevision _topic;

        // Constructors

        public TopicChange(QualifiedTopicRevision topic, DateTime created, string author) :
            this(topic, created, created, author)
        {
        }

        public TopicChange(QualifiedTopicRevision topic, DateTime created, DateTime modified, string author)
        {
            _topic = topic;
            _author = author;
            _created = created;
            _modified = modified; 
        }

        // Properties

        [ExposedMethod(ExposedMethodFlags.Default, "Answer the name of the author of this change")]
        public string Author
        {
            get
            {
                return _author;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a DateTime that indicates when this version was created.")]
        public DateTime Created
        {
            get
            {
                return _created;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the full name of the topic whose change is described by this TopicChange")]
        public string DottedName
        {
            get
            {
                return TopicRevision.DottedName;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer a DateTime that indicates when this version was last modified.")]
        public DateTime Modified
        {
            get
            {
                return _modified; 
            }
        }
        public QualifiedTopicRevision TopicRevision
        {
            get
            {
                return _topic;
            }
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Answer the version stamp for this change")]
        public string Version
        {
            get
            {
                return TopicRevision.Version;
            }
        }

        // Methods

        public int CompareTo(object obj)
        {
            if (!(obj is TopicChange))
                return -1;
            TopicChange other = (TopicChange)obj;
            int answer;
            answer = TopicRevision.CompareTo(other.TopicRevision);
            if (answer != 0)
                return answer;
            return Created.CompareTo(other.Created);
        }

    }
}
