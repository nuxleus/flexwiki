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
	/// Summary description for TopicNotFoundException.
	/// </summary>
	public class TopicNotFoundException : ApplicationException
	{
		public TopicNotFoundException() : base()
		{
		}
		public TopicNotFoundException(string message) : base(message)
		{
		}

		TopicName _Topic;

		public TopicName Topic
		{
			get
			{
				return _Topic;
			}
			set
			{
				_Topic = value;
			}
		}
		string _Version;

		public string Version
		{
			get
			{
				return _Version;
			}
			set
			{
				_Version = value;
			}
		}

		public static TopicNotFoundException ForTopic(TopicName topic)
		{
			TopicNotFoundException answer = new TopicNotFoundException("Topic not found: " + topic.ToString());
			answer.Topic = topic;
			return answer;
		}

	}
}
