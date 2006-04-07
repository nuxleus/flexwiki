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
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for DynamicNamespace.
	/// </summary>
	public class DynamicNamespace : DynamicObject
	{
		public DynamicNamespace(Federation aFed, string ns)
		{
			_CurrentFederation = aFed;
			_Name = ns;
		}

		public string Name
		{
			get
			{
				return _Name;
			}
		}

		string _Name;

		Federation _CurrentFederation;
		public Federation CurrentFederation
		{
			get
			{
				return _CurrentFederation;
			}
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence("(namespace \"\'" + Name + "\"\")");
		}

		Hashtable _Topics;
		public DynamicTopic DynamicTopicFor(string topic)
		{
			if (_Topics == null)
				_Topics = new Hashtable();
			DynamicTopic answer = (DynamicTopic)(_Topics[topic]);
			if (answer != null)
				return answer;

			RelativeTopicName rel = new RelativeTopicName(topic);
			ArrayList alternatives = new ArrayList();
			if (rel.Namespace != null)	// if they left the namespace unspec'd
			{			
				alternatives.Add(new AbsoluteTopicName(topic));	
			}
			else
			{
				alternatives.Add(new AbsoluteTopicName(topic, Name));	// always try this one first
				alternatives.AddRange(rel.AllAbsoluteTopicNamesFor(CurrentFederation.ContentBaseForNamespace(Name)));
			}

			foreach (AbsoluteTopicName tn in alternatives)
			{
				ContentBase cb = CurrentFederation.ContentBaseForTopic(tn);
				if (!cb.TopicExists(tn))
					continue;
				answer = new DynamicTopic(CurrentFederation, tn);
				_Topics[topic] = answer;
				return answer;
			}
			return null;
		}

		public override IBELObject ValueOf(string name, System.Collections.ArrayList arguments, ExecutionContext ctx)
		{
			IBELObject answer = DynamicTopicFor(name);
			if (answer == null)
				return null;
			if (arguments != null && arguments.Count > 0)
				throw new ArgumentException("Arguments not allowed for topic names in namespaces");
			return answer;
		}

	}
}
