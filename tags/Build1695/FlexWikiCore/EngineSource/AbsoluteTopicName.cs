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

namespace FlexWiki
{
	/// <summary>
	/// Represents a topic name with a namespace.  The namespace must be specified, because this is the unambiguous name
	/// of a topic in a federation.
	/// </summary>
	public class AbsoluteTopicName : TopicName
	{
		/// <summary>
		/// Default contructor for XML Serialization.
		/// </summary>
		public AbsoluteTopicName()
		{
			
		}

		/// <summary>
		/// Create a new AbsoluteTopicName with a given fully-qualified name
		/// </summary>
		/// <param name="topic">Fully-qualified name (including '.'s)</param>
		public AbsoluteTopicName(string topic) : base(topic)
		{
		}

		/// <summary>
		/// Create a new topic of the same type as this one (AbsoluteTopicName) with the given string
		/// </summary>
		/// <param name="topic">Fully-qualified name (including '.'s)</param>
		/// <returns></returns>
		public override TopicName NewOfSameType(string topic)
		{
			return new AbsoluteTopicName(topic);
		}


		/// <summary>
		/// Create a new AbsoluteTopicName with a given unqualified name and namespace
		/// </summary>
		/// <param name="topic">Topic name (no namespace)</param>
		/// <param name="theNamespace">The namespace</param>
		public AbsoluteTopicName(string topic, string theNamespace) : base(topic, theNamespace)
		{
			if (topic.IndexOf(TopicName.Separator) > 0)
				throw new ArgumentException("Qualified topic name can not be used when namespace is specified", topic);
		}

		/// <summary>
		/// Answer this object as an AbsoluteTopicName (which it already is :-))
		/// </summary>
		/// <param name="defaultNamespace">n/a (only useful in other overrides)</param>
		/// <returns></returns>
		public override AbsoluteTopicName AsAbsoluteTopicName(string defaultNamespace)
		{
			return this;
		}

		/// <summary>
		/// Answer a collection of possible AbsoluteTopicNames for this topic that are known in a given ContentBase.
		/// Mostly useful in other overrides. Since this name is already absolute, we always answer a collection of one.
		/// </summary>
		/// <param name="info">Identifies the content base relative to whose reachable namespaces the topic name should be resolved</param>
		/// <returns></returns>
		public override IList AllAbsoluteTopicNamesFor(ContentBase info)
		{
			ArrayList answer = new ArrayList();
			answer.Add(this);
			return answer;
		}
	}
}
