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
		public TopicChange(AbsoluteTopicName topic, DateTime changeStamp, string author)
		{
			_Topic = topic;
			_Author = author;
			_Timestamp = changeStamp;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the version stamp for this change")]
		public string Version
		{
			get
			{
				return Topic.Version;
			}
		}

                                                     
		public int CompareTo(object obj)
		{
			if (!(obj is TopicChange))
				return -1;
			TopicChange other = (TopicChange)obj;
			int answer;
			answer = Topic.CompareTo(other.Topic);
			if (answer != 0)
				return answer;
			return Timestamp.CompareTo(other.Timestamp);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the full name of the topic whose change is described by this TopicChange")]
		public string Fullname
		{
			get
			{
				return Topic.ToString();
			}
		}


			public AbsoluteTopicName Topic
		{
			get
			{
				return _Topic;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the name of the author of this change")]
		public string Author
		{
			get
			{
				return _Author;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a DateTime that indicates when the change was made")]
		public DateTime Timestamp
		{
			get
			{
				return _Timestamp;
			}
		}

		private AbsoluteTopicName _Topic;
		private string _Author;
		private DateTime _Timestamp;	
	}
}
