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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for Topic.
	/// </summary>
	[ExposedClass("VisitorEvent", "Provides information about an event for a visitor to the site")]
	public class VisitorEvent : BELObject, IComparable
	{
		public static string Read = "read";
		public static string Write = "write";

		AbsoluteTopicName _Topic;
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a the (full)name of the topic")]
		public AbsoluteTopicName Topic
		{
			get
			{
				return _Topic;
			}
		}

		string _Verb;
		[ExposedMethod("Verb", ExposedMethodFlags.CachePolicyNone, "Answer the verb (e.g., 'read', 'write')")]
		public string Verb
		{
			get
			{
				return _Verb;
			}
		}

		DateTime _When;
		[ExposedMethod("When", ExposedMethodFlags.CachePolicyNone, "Answer when the event happened")]
		public DateTime When
		{
			get
			{
				return _When;
			}
		}

		public VisitorEvent(AbsoluteTopicName topic, string verb, DateTime when)
		{
			_Topic = topic;
			_Verb = verb;
			_When = when;
		}
		
		[ExposedMethod("Fullname", ExposedMethodFlags.CachePolicyNone, "Answer the complete name of the topic (including namespace and version, if present)")]
		public string ExposedFullname
		{
			get
			{
				return Topic.ToString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the name of the topic (without namespace)")]
		public string Name
		{
			get
			{
				return Topic.Name;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the version stamp for the topic")]
		public string Version
		{
			get
			{
				return Topic.Version;
			}
		}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			return When.CompareTo(obj);
		}

		#endregion
	}
}
