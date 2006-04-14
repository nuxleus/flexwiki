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
using System.IO;

using FlexWiki;
using FlexWiki.Collections;

namespace FlexWiki.CalendarProvider
{
    /// <summary>
    /// Summary description for CalendarStore.
    /// </summary>
    public class CalendarStore : IUnparsedContentProvider
    {
        public CalendarStore(int year, int month)
        {
            _year = year;
            _month = month;
        }

        private static DateTime s_creationConst = DateTime.Now;

        private DateTime _lastRead = DateTime.Now;
        private int _month;
        private readonly Hashtable _topics = new Hashtable();	// keys = names (double entry, both local and abs), values = DateTimes
        private int _year;

        public DateTime Created
        {
            get
            {
                return DateTime.MinValue;
            }
        }
        public bool Exists
        {
            get
            {
                return true;
            }
        }
        public bool IsReadOnly
        {
            get { return true; }
        }
        public string HomePage
        {
            get
            {
                return TopicNameForDate(FirstDate).LocalName;
            }
            set
            {
            }
        }
        public DateTime LastRead
        {
            get
            {
                return _lastRead;
            }
        }
        public IUnparsedContentProvider Next
        {
            get { throw new NotSupportedException(); }
        }

        protected IEnumerable Dates
        {
            get
            {
                ArrayList answer = new ArrayList();
                for (int i = 1; i < DateTime.DaysInMonth(_year, _month) + 1; i++)
                {
                    answer.Add(new DateTime(_year, _month, i));
                }
                return answer;
            }
        }

        private DateTime FirstDate
        {
            get
            {
                foreach (DateTime each in Dates)
                {
                    return each;
                }
                throw new InvalidOperationException();	// should never happen
            }
        }

        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="topic">A given date</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <returns>Enumeration of TopicChanges</returns>
        TopicChangeCollection IContentProvider.AllChangesForTopicSince(string topic, DateTime stamp)
        {
            throw new NotImplementedException();
            /*
            ArrayList answer = new ArrayList();
            foreach (AbsoluteTopicName each in AllVersionsForTopic(topic))
            {
                DateTime when = GetTopicLastModificationTime(topic);
                if (when >= stamp)
                    answer.Add(new TopicChange(each, when, GetTopicLastAuthor(each.LocalName)));
            }
            return answer;
             */
        }

        TopicNameCollection IContentProvider.AllTopics()
        {
            throw new NotImplementedException(); 
        }

        void IContentProvider.DeleteAllTopicsAndHistory()
        {
            throw new NotImplementedException();
        }

        void IContentProvider.DeleteTopic(string topicName)
        {
            throw new NotImplementedException();
        }

        void IContentProvider.Initialize(NamespaceManager namespaceManager)
        {
            throw new NotImplementedException();

            //foreach (DateTime each in Dates)
            //{
            //    AbsoluteTopicName abs = TopicNameForDate(each);
            //    _Topics[abs] = each;
            //    _Topics[abs.LocalName] = each;
            //}

            //AbsoluteTopicName a = new AbsoluteTopicName("_NormalBorders", namespaceManager.Namespace);
            //BackingTopic top = new BackingTopic(a, DefaultNormalBordersContent, true);
            //BackingTopics[a.Name] = top;
            //_Topics[a] = DateTime.MinValue;
        }

        bool IContentProvider.IsExistingTopicWritable(string topicName)
        {
            throw new NotImplementedException();
        }

        TextReader IContentProvider.TextReaderForTopic(string topic, string version)
        {
            throw new NotImplementedException();

            //if (!TopicExistsLocally(topic))
            //    throw TopicNotFoundException.ForTopic(topic, Namespace);
            //BackingTopic back = GetBackingTopicNamed(topic);
            //if (back != null)
            //    return new StringReader(back.Body);

            //StringBuilder b = new StringBuilder();
            //DateTime dt = DateTimeFromTopicName(topic);
            //b.Append("This page contains information about '''" + dt.ToLongDateString() + "'''.");
            //return new StringReader(b.ToString());
        }
        
        bool IContentProvider.TopicExists(string name)
        {
            throw new NotImplementedException();

            //foreach (AbsoluteTopicName each in AllTopicsUnsorted())
            //{
            //    if (each.LocalName.Equals(name))

            //        return true;
            //}
            //return false;
        }
        
        void IContentProvider.WriteTopic(string topic, string version, string content)
        {
            throw new NotImplementedException();
        }

        void IContentProvider.WriteTopicAndNewVersion(string topic, string content, string author)
        {
            throw new NotImplementedException();
        }


        private DateTime DateTimeFromTopicName(LocalTopicVersionKey topic)
        {
            return (DateTime) (_topics[topic]);
        }

        private NamespaceQualifiedTopicVersionKey TopicNameForDate(DateTime stamp)
        {
            throw new NotImplementedException();
            //return new AbsoluteTopicName("About" + stamp.ToString("MMMMdd"), Namespace);
        }




        
        

    }
}
