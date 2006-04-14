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

using FlexWiki.Collections; 
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for DynamicNamespace.
	/// </summary>
	public class DynamicNamespace : DynamicObject
	{
        private Federation _currentFederation;
        private string _namespace;
        private Hashtable _topics;
        
        public DynamicNamespace(Federation aFed, string ns)
		{
			_currentFederation = aFed;
			_namespace = ns;
		}

		public string Namespace
		{
			get
			{
				return _namespace;
			}
		}

		public Federation CurrentFederation
		{
			get
			{
				return _currentFederation;
			}
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence("(namespace \"\'" + Namespace + "\"\")");
		}

		public DynamicTopic DynamicTopicFor(string topic)
		{
            if (_topics == null)
            {
                _topics = new Hashtable();
            }
			DynamicTopic answer = (DynamicTopic)(_topics[topic]);
            if (answer != null)
            {
                return answer;
            }

            TopicName topicName = new TopicName(topic); 

            NamespaceManager manager = CurrentFederation.NamespaceManagerForNamespace(Namespace);
            NamespaceQualifiedTopicNameCollection alternatives = manager.AllPossibleNamespaceQualifiedTopicNames(topicName); 

			foreach (NamespaceQualifiedTopicName tn in alternatives)
			{
				NamespaceManager namespaceManager = CurrentFederation.NamespaceManagerForTopic(tn);
				if (!namespaceManager.TopicExists(tn.LocalName, ImportPolicy.DoNotIncludeImports))
					continue;
				answer = new DynamicTopic(CurrentFederation, new NamespaceQualifiedTopicVersionKey(tn));
				_topics[topic] = answer;
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
