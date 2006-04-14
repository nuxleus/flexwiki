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
using System.Reflection;
using System.Collections;
using System.Web.Caching;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("TopicContext", "Tracks the context in which WikiTalk execution is occurring")]
	public class TopicContext : BELObject
	{
    private NamespaceManager _currentNamespaceManager;
    private Federation _currentFederation;
    private TopicVersionInfo _currentTopic;
    private Hashtable _externalWikiMap;
    
    public TopicContext(Federation f, NamespaceManager namespaceManager, TopicVersionInfo topic)
		{
			CurrentFederation = f;
			CurrentNamespaceManager = namespaceManager;
			CurrentTopic = topic;
		}

		
    public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}


		[ExposedMethod(ExposedMethodFlags.Default, "Answer the federation")]
		public Federation CurrentFederation
		{
			get
			{
				return _currentFederation;
			}
			set
			{
				_currentFederation = value;
			}
		}

		[ExposedMethod("CurrentTopic", ExposedMethodFlags.Default, "Answer a TopicInfo object describing the current topic")]
		public TopicVersionInfo CurrentTopic
		{
			get
			{
				return _currentTopic;
			}
			set
			{
				_currentTopic = value;
			}
		}

		[ExposedMethod("CurrentNamespace", ExposedMethodFlags.Default, "Answer the current namespace")]
		public NamespaceManager CurrentNamespaceManager
		{
			get
			{
				return _currentNamespaceManager;
			}
			set
			{
				_currentNamespaceManager = value;
			}
		}

		public Hashtable ExternalWikiMap
		{
			get
			{
				return _externalWikiMap;
			}
			set
			{
				_externalWikiMap = value;
			}
		}
	}
}
