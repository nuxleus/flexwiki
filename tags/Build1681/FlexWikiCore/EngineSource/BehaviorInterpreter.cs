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


namespace FlexWiki.Formatting
{
	/// <summary>
	/// This class is responsible for parsing and "running" a behavior expression
	/// </summary>
	public class BehaviorInterpreter
	{
		string _Input = null;
		IPresentation _Value = null;

		enum InterpreterState
		{
			ReadyToParse,
			ParseSuccess,
			ParseFailure,
			EvaluationSuccess,
			EvaluationFailure
		};

		InterpreterState _State;
		
		InterpreterState State
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

		string Input
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

		string _ErrorString;
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

		Federation _TheFederation;
		Federation TheFederation
		{
			get
			{
				return _TheFederation;
			}
		}

		IWikiToPresentation _Presenter;
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

		public BehaviorInterpreter(string input, Federation aFed, int wikiTalkVersion, IWikiToPresentation presenter)
		{
			_TheFederation = aFed;
			WikiTalkVersion = wikiTalkVersion;
			Presenter = presenter;

			Input	= input;
			State = InterpreterState.ReadyToParse;
		}

		public int _WikiTalkVersion = 1;	
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
			BehaviorParser parser = new BehaviorParser();
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

		ICollection	_CacheRules = new ArrayList();
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
		public bool EvaluateToPresentation(TopicContext topicContext, Hashtable externalWikimap)
		{
			IBELObject evaluated = EvaluateToObject(topicContext, externalWikimap);
			if (evaluated == null)
				return false;
			IOutputSequence seq = evaluated.ToOutputSequence();
			// output sequence -> pure presentation tree
			_Value = seq.ToPresentation(Presenter);
			return true;
		}

		public IBELObject EvaluateToObject(TopicContext topicContext,  Hashtable externalWikimap)
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

				TopicInfo topic = topicContext.CurrentTopic;
				if (topic != null && topic.Fullname != null)
				{
					// Locate any topics via the with property to see if there's anybody else we should import
					ArrayList with = topicContext.CurrentTopic.GetListProperty("With");
					if (with != null)
					{
						with.Reverse();
						foreach (string top in with)
						{
							AbsoluteTopicName abs = topic.ContentBase.UnambiguousTopicNameFor(new RelativeTopicName(top));
							if (abs == null)
							{
								throw new Exception("No such topic: " + top + " (as specifed in with: property for topic)");
							}
							theScope = new TopicScope(theScope, new DynamicTopic(topic.Federation, abs));
						}
					}
					// add the topic to the current scope (this guy goes at the front of the queue!)
					theScope = new TopicScope(theScope, new DynamicTopic(topic.Federation, topic.Fullname));

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
				ErrorString = "Error evaluating expression: " + e.Message;
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
