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
using System.Web.Caching;
using System.Collections;
using System.Text;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for TopicsCacheRule.
	/// </summary>
	public class TopicsCacheRule : CacheRule, IHTMLRenderable
	{
		public TopicsCacheRule(Federation fed)
		{
			_Federation = fed;
		}

		public TopicsCacheRule(Federation fed, AbsoluteTopicName topic)
		{
			_Federation = fed;
			AddTopic(topic);
		}

		Federation _Federation;

		public TopicsCacheRule(IEnumerable list)
		{
			foreach (AbsoluteTopicName topic in list)
			{
				AddTopic(topic);
			}
		}

		public override void SetupInvalidation(FederationCacheManager manager, string key)
		{
			foreach (AbsoluteTopicName topic in Topics)
			{
				manager.AddKeyToInvalidateOnTopicChange(key, topic);

				/// For now we take a very aggressive invalidation approach that says that whenever
				/// the list of namespaces changes, we invalidate all cached topics.  This is because
				/// we dont' yet do the work to be more granular (e.g., figure out when a namespace is added 
				/// or removed exactly which topics are impacted).  This will likely cause a near flushing of
				/// the cache when namespaces are added or removed, but that should be pretty rare.  if it 
				/// becomes a serious problem, we can enhance this to be more granular.
			
				manager.AddKeyToInvalidateOnFederationNamespacesChange(key);

			}
		}


		ArrayList _Topics = new ArrayList();

		public IList Topics
		{
			get
			{
				return _Topics;
			}
		}

		public override ICollection AllLeafRules
		{
			get
			{
				return ArrayList.Repeat(this, 1);
			}
		}

		public void AddTopic(AbsoluteTopicName topic)
		{
			// prevent dups (the GetHashCode and Equal implementations depend on this)
			if (_Topics.Contains(topic))
				return;
			_Topics.Add(topic);
		}

		public override string Description 
		{
			get
			{
				StringBuilder answer = new StringBuilder();
				foreach (AbsoluteTopicName top in _Topics)
				{
					if (answer.Length > 0)
						answer.Append(", ");
					answer.Append(top.FullnameWithVersion);
				}
				return answer.ToString();
			}
		}

		public override bool Equals(object obj)
		{
			TopicsCacheRule other = obj as TopicsCacheRule;
			if (other == null)
				return false;
			if (!other._Federation.Equals(_Federation))
				return false;
			if (other._Topics.Count != _Topics.Count)
				return false;
			foreach (AbsoluteTopicName tn in _Topics)
				if (!other._Topics.Contains(tn))
					return false;
			return true;
		}

		public override int GetHashCode()
		{
			int answer = 0;
			if (_Federation != null)
				answer ^= _Federation.GetHashCode();
			foreach (AbsoluteTopicName tn in _Topics)
				answer ^= tn.GetHashCode();
			return answer;
		}
		#region IHTMLRenderable Members

		public void RenderToHTML(System.IO.TextWriter output)
		{
			output.WriteLine("<fieldset><legend>Topics</legend>");
			foreach (AbsoluteTopicName top in _Topics)
			{
				output.WriteLine(Formatting.Formatter.EscapeHTML(top.FullnameWithVersion) + "<br />");
			}
			output.WriteLine("</fieldset>");
		}

		#endregion
	}
}
