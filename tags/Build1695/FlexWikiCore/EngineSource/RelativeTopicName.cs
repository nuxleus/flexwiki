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
	/// A RelativeTopicName is a TopicName which *might* not have a namespace specified.
	/// </summary>
	public class RelativeTopicName : TopicName
	{
		public RelativeTopicName(string topic) : base(topic)
		{
		}

		public override TopicName NewOfSameType(string topic)
		{
			return new RelativeTopicName(topic);
		}


		public RelativeTopicName(string topic, string theNamespace) : base(topic, theNamespace)
		{
		}

		public override AbsoluteTopicName AsAbsoluteTopicName(string defaultNamespace)
		{
			if (Namespace == null)
				return new AbsoluteTopicName(Name, defaultNamespace);
			else
				return new AbsoluteTopicName(Name, Namespace);
		}

		public override IList AllAbsoluteTopicNamesFor(ContentBase cb)
		{
			ArrayList answer = new ArrayList();
			if (Namespace != null)
				answer.Add(new AbsoluteTopicName(Fullname));
			if (Namespace == null)
			{	
				answer.Add(new AbsoluteTopicName(Name, cb.Namespace));
				foreach (ContentBase each in cb.ImportedContentBases)
					answer.Add(new AbsoluteTopicName(Name, each.Namespace));
			}
			return answer;
		}


	}
}
