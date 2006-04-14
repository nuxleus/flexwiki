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
    private TopicVersionKey _Topic;
    private string _Version;
    
    public TopicNotFoundException() : base()
		{
		}
		public TopicNotFoundException(string message) : base(message)
		{
		}


		public TopicVersionKey Topic
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

		public static TopicNotFoundException ForTopic(TopicVersionKey topic)
		{
			TopicNotFoundException answer = new TopicNotFoundException("Topic not found: " + topic.ToString());
			answer.Topic = topic;
			return answer;
		}

		public static TopicNotFoundException ForTopic(LocalTopicVersionKey topic, string ns)
		{
			NamespaceQualifiedTopicVersionKey t = new NamespaceQualifiedTopicVersionKey(topic.NameWithVersion, ns);
			return ForTopic(t);
		}

        public static TopicNotFoundException ForTopic(string topic, string ns)
        {
            return ForTopic(new LocalTopicVersionKey(topic), ns); 
        }


	}
}
