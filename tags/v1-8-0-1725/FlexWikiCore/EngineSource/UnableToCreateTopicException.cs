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
	/// Summary description for UnableToCreateTopicException.
	/// </summary>
	public class UnableToCreateTopicException : ApplicationException
	{
		public UnableToCreateTopicException() : base()
		{
		}
		public UnableToCreateTopicException(string message) : base(message)
		{
		}

		string _Topic;

		public string Topic
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

		public static UnableToCreateTopicException ForTopic(string topic)
		{
			UnableToCreateTopicException answer = new UnableToCreateTopicException("Unable to create topic (bad namespace?): " + topic);
			answer.Topic = topic;
			return answer;
		}
		public static UnableToCreateTopicException ForTopic(string topic, string version)
		{
			if (version == null || version == "")
				return UnableToCreateTopicException.ForTopic(topic);
			UnableToCreateTopicException answer = new UnableToCreateTopicException("Unable to create topic (bad namespace?): " + topic + " (version " + version + ")");
			answer.Topic = topic;
			answer.Version = version;
			return answer;
		}

	}
}
