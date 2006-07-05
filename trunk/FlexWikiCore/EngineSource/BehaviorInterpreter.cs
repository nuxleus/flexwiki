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
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

using FlexWiki;
using FlexWiki.Collections; 

namespace FlexWiki.Formatting
{
	/// <summary>
	/// This class is responsible for parsing and "running" a behavior expression
	/// </summary>
	public class BehaviorInterpreter
	{
    private ICollection	_CacheRules = new ArrayList();
    private Federation _Federation;
    private string _Input = null;
    private IWikiToPresentation _Presenter;
    private InterpreterState _State;
    private IPresentation _Value = null;
    private int _WikiTalkVersion = 1;	
		
		private InterpreterState State
		{
			get
			{
				return _State;
			}
			set
			{
				_State = value;
			}
		}

		private string Input
		{
			get
			{
				return _Input;
			}
			set
			{
				_Input = value;
			}
		}

		private string _ErrorString;
		public string ErrorString
		{
			get
			{
				return _ErrorString;
			}
			set
			{
				_ErrorString = value;
			}
		}

		private Federation Federation
		{
			get
			{
				return _Federation;
			}
		}

		public IWikiToPresentation Presenter
		{
			get
			{
				return _Presenter;
			}
			set
			{
				_Presenter = value;
			}
		}

		string _ContextString;

		public BehaviorInterpreter(string contextString, string input, Federation aFed, int wikiTalkVersion, IWikiToPresentation presenter)
		{
			_Federation = aFed;
			_ContextString = contextString == null ? "(unknown location)" : contextString;
			WikiTalkVersion = wikiTalkVersion;
			Presenter = presenter;

			Input	= input;
			State = InterpreterState.ReadyToParse;
		}

		public int WikiTalkVersion
		{
			get
			{
				return _WikiTalkVersion;
			}
			set
			{
				_WikiTalkVersion = value;
			}
		}

		public ExposableParseTreeNode ParseTree;
		/// <summary>
		/// Parse
		/// </summary>
		/// <returns>true if success, false if failure</returns>
		public bool Parse()
		{
			ParseTree = null;
			BehaviorParser parser = new BehaviorParser(_ContextString);
			string error = null;
			try
			{
				ParseTree = parser.Parse(Input);
			}
			catch (ExpectedTokenParseException e)
			{
				error = e.Message;
			}
			catch (UnexpectedTokenParseException e)
			{
				error = e.Message;
			}
			if (error != null)
			{
				State = InterpreterState.ParseFailure;
				ErrorString = error;
				return false;
			}
			State = InterpreterState.ParseSuccess;
			return true;
		}

		public ICollection CacheRules
		{
			get
			{
				return _CacheRules;
			}
		}

		/// <summary>
		/// Evaluate and translate into a presentation
		/// </summary>
		/// <returns>true if success, false if failure</returns>
		public bool EvaluateToPresentation(TopicContext topicContext, ExternalReferencesMap externalWikimap)
		{
			IBELObject evaluated = EvaluateToObject(topicContext, externalWikimap);
			if (evaluated == null)
				return false;
			IOutputSequence seq = evaluated.ToOutputSequence();
			// output sequence -> pure presentation tree
			_Value = seq.ToPresentation(Presenter);
			return true;
		}

		public IBELObject EvaluateToObject(TopicContext topicContext,  ExternalReferencesMap externalWikimap)
		{
			_CacheRules = new ArrayList();
			if (ParseTree == null)
			{
				ErrorString = "Expression can not be evaluated; parse failed.";
				State = InterpreterState.EvaluationFailure;
				return null;
			}
			ExecutionContext ctx = new ExecutionContext(topicContext);
			ctx.WikiTalkVersion = WikiTalkVersion;
			IBELObject answer = null;
			try 
			{
				ctx.ExternalWikiMap = externalWikimap;
				IScope theScope = null;

				ctx.Presenter = Presenter;

				TopicVersionInfo topic = topicContext != null ? topicContext.CurrentTopic : null;
				if (topic != null && topic.TopicRevision != null)
				{
					// Locate any topics via the NamespaceWith propertyName to see 
                    // if there's anybody else we should import (for all topics in the namespace)
					ArrayList nswith = topicContext.CurrentFederation.GetTopicInfo(ctx, 
                        topicContext.CurrentTopic.NamespaceManager.DefinitionTopicName.DottedName).GetListProperty("NamespaceWith");
					if (nswith != null)
					{
						nswith.Reverse();
						foreach (string top in nswith)
						{
							QualifiedTopicRevision abs = Federation.UnambiguousTopicNameFor(new TopicRevision(top), 
                                topic.NamespaceManager.Namespace);
							if (abs == null)
							{
								throw new Exception("No such topic: " + top + " (as specifed in NamespaceWith: property for " + 
                                    topicContext.CurrentTopic.NamespaceManager.DefinitionTopicName.DottedName + ")");
							}
							theScope = new TopicScope(theScope, new DynamicTopic(topic.Federation, abs));
						}
					}
					
					// Locate any topics via the with propertyName to see if there's anybody else we should import
					ArrayList with = topicContext.CurrentTopic.GetListProperty("With");
					if (with != null)
					{
						with.Reverse();
						foreach (string top in with)
						{
							QualifiedTopicRevision abs = Federation.UnambiguousTopicNameFor(new TopicRevision(top), 
                                topic.NamespaceManager.Namespace);
							if (abs == null)
							{
								throw new Exception("No such topic: " + top + " (as specifed in With: property for " + topicContext.CurrentTopic + ")");
							}
							theScope = new TopicScope(theScope, new DynamicTopic(topic.Federation, abs));
						}
					}
					// add the topic to the current scope (this guy goes at the front of the queue!)
					theScope = new TopicScope(theScope, new DynamicTopic(topic.Federation, topic.TopicRevision));

				}
				if (theScope != null)
					ctx.PushScope(theScope); // make sure we can use local references
				// parse tree -> live objects
				answer = ParseTree.Expose(ctx);
				if (theScope != null)
					ctx.PopScope();

				_CacheRules = ctx.CacheRules;
			}
			catch (Exception e)
			{
				_CacheRules = ctx.CacheRules;
				ErrorString = e.Message;
				State = InterpreterState.EvaluationFailure;
				return  null;
			}
			State = InterpreterState.EvaluationSuccess;
			return answer;
		}

		public IPresentation Value
		{
			get
			{
				return _Value;
			}
		}

	}
}
