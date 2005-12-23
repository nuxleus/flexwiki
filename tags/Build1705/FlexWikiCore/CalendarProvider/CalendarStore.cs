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
using System.Collections;
using FlexWiki;
using System.IO;


namespace CalendarProvider
{
	/// <summary>
	/// Summary description for CalendarStore.
	/// </summary>
	public class CalendarStore : ReadOnlyStore
	{
		public CalendarStore(Federation fed, string ns, int year, int month)
		{
			SetFederation(fed);
			Namespace = ns;
			Year = year;
			Month = month;
			foreach (DateTime each in Dates)
			{
				AbsoluteTopicName abs = TopicNameForDate(each);
				_Topics[abs] = each;
				_Topics[abs.LocalName] = each;
			}

			AbsoluteTopicName a = new AbsoluteTopicName("_NormalBorders", Namespace);
			BackingTopic top = new BackingTopic(a, DefaultNormalBordersContent, true);
			BackingTopics[a.Name] = top;
			_Topics[a] = DateTime.MinValue;
		}

		public override string HomePage
		{
			get
			{
				return TopicNameForDate(FirstDate).LocalName.Name;
			}
			set
			{
			}
		}

		DateTime FirstDate
		{
			get
			{
				foreach (DateTime each in Dates)
					return each;
				return DateTime.Now;	// should never happen
			}
		}



		Hashtable _Topics = new Hashtable();	// keys = names (double entry, both local and abs), values = DateTimes

		int Year;
		int Month;

		public override bool Exists
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// A list of TopicChanges to a topic since a given date [sorted by date]
		/// </summary>
		/// <param name="topic">A given date</param>
		/// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
		/// <returns>Enumeration of TopicChanges</returns>
		public override System.Collections.IEnumerable AllChangesForTopicSince(LocalTopicName topic, DateTime stamp, CompositeCacheRule rule)
		{
			ArrayList answer = new ArrayList();
			foreach (AbsoluteTopicName each in AllVersionsForTopic(topic))
			{
				DateTime when = GetTopicLastWriteTime(topic);
				if (when >= stamp)
					answer.Add(new TopicChange(each, when, GetTopicLastAuthor(each.LocalName)));
			}
			return answer;
		}

		public override IEnumerable AllVersionsForTopic(LocalTopicName topic)
		{
			ArrayList answer = new ArrayList();
			AbsoluteTopicName only = topic.AsAbsoluteTopicName(Namespace);
			only.Version = VersionConst;
			answer.Add(only);
			return answer;
		}

		public override string LatestVersionForTopic(LocalTopicName topic)
		{
			return VersionConst;
		}


		static string VersionConst = "1";
		static DateTime CreationConst = DateTime.Now;
		static string AuthorConst = "Sample User";

		protected override IEnumerable AllTopicsUnsorted()
		{
			return AllTopicsSortedLastModifiedDescending();
		}

		public override IEnumerable AllTopicsSortedLastModifiedDescending()
		{
			ArrayList answer = new ArrayList();
			foreach (object each in _Topics.Keys)
			{
				if (each is AbsoluteTopicName)
					answer.Add(each);
			}
			return answer;

		}

		public override DateTime GetTopicCreationTime(LocalTopicName topic)
		{
			return CreationConst;
		}

		public override DateTime GetTopicLastWriteTime(LocalTopicName topic)
		{
			return CreationConst;
		}

		public override string GetTopicLastAuthor(LocalTopicName topic)
		{
			return AuthorConst;
		}

		AbsoluteTopicName TopicNameForDate(DateTime stamp)
		{
			return new AbsoluteTopicName("About" + stamp.ToString("MMMMdd"), Namespace);
		}

		DateTime DateTimeFromTopicName(LocalTopicName topic)
		{
			return (DateTime)(_Topics[topic]);
		}

		protected IEnumerable Dates
		{
			get
			{
				ArrayList answer = new ArrayList();
				for (int i = 1; i < DateTime.DaysInMonth(Year, Month) + 1; i++)
				{
					answer.Add(new DateTime(Year, Month, i));
				}
				return answer;
			}
		}

		public override DateTime Created
		{
			get
			{
				return DateTime.MinValue;
			}
		}

		DateTime _LastRead;

		public override DateTime LastRead
		{
			get
			{
				return _LastRead;
			}
		}

		public override void Validate()
		{
			_LastRead = DateTime.Now;
		}

		public override bool TopicExistsLocally(LocalTopicName name)
		{
			foreach (AbsoluteTopicName each in AllTopicsUnsorted())
			{
				if (each.LocalName.Equals(name))

					return true;
			}
			return false;
		}

		public override TextReader TextReaderForTopic(LocalTopicName topic)
		{
			if (!TopicExistsLocally(topic))
				throw TopicNotFoundException.ForTopic(topic, Namespace);
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				return new StringReader(back.Body);

			StringBuilder b = new StringBuilder();
			DateTime dt = DateTimeFromTopicName(topic);
			b.Append("This page contains information about '''" + dt.ToLongDateString() + "'''.");
			return new StringReader(b.ToString());
		}


	}
}
