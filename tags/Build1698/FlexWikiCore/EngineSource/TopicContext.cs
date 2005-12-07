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
		public TopicContext(Federation f, ContentBase cb, TopicInfo topic)
		{
			CurrentFederation = f;
			CurrentContentBase = cb;
			CurrentTopic = topic;
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}


		Federation _CurrentFederation;
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the federation")]
		public Federation CurrentFederation
		{
			get
			{
				return _CurrentFederation;
			}
			set
			{
				_CurrentFederation = value;
			}
		}

		TopicInfo _CurrentTopic;
		[ExposedMethod("CurrentTopic", ExposedMethodFlags.CachePolicyNone, "Answer a TopicInfo object describing the current topic")]
		public TopicInfo CurrentTopic
		{
			get
			{
				return _CurrentTopic;
			}
			set
			{
				_CurrentTopic = value;
			}
		}

		ContentBase _CurrentContentBase;
		[ExposedMethod("CurrentNamespace", ExposedMethodFlags.CachePolicyNone, "Answer the current namespace")]
		public ContentBase CurrentContentBase
		{
			get
			{
				return _CurrentContentBase;
			}
			set
			{
				_CurrentContentBase = value;
			}
		}

		Hashtable _ExternalWikiMap;
		public Hashtable ExternalWikiMap
		{
			get
			{
				return _ExternalWikiMap;
			}
			set
			{
				_ExternalWikiMap = value;
			}
		}
	}
}
