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
	public class DynamicTopic : DynamicObject
	{
		public DynamicTopic(Federation aFed, AbsoluteTopicName name)
		{
			_CurrentFederation = aFed;
			_Name = name;
		}


		AbsoluteTopicName _Name;
		Federation _CurrentFederation;

		public AbsoluteTopicName Name
		{
			get
			{
				return _Name;
			}
		}

		public Federation CurrentFederation
		{
			get
			{
				return _CurrentFederation;
			}
		}

		TopicInfo	_CurrentTopicInfo;
		TopicInfo  CurrentTopicInfo
		{
			get
			{
				if (_CurrentTopicInfo != null)	
					return _CurrentTopicInfo;
				_CurrentTopicInfo = new TopicInfo(CurrentFederation, Name);
				return _CurrentTopicInfo;
			}
		}

		public override IOutputSequence ToOutputSequence()
		{
			// BELTODO -- test case (ensure that single word topic names work reasonably)
			return new WikiSequence(Name.Namespace + ".[" + Name.Name + "]");
		}

		public override IBELObject ValueOf(string name, System.Collections.ArrayList arguments, ExecutionContext ctx)
		{
			Hashtable members = ctx.CurrentFederation.GetTopicProperties(Name);
			string val = (string)(members[name]);
			if (val == null)
				return null;
			val = val.Trim();
			bool isBlock = val.StartsWith("{");
			if (!isBlock)
				return new BELString(val);
			// It's a block, so fire up the interpreter
			if (!val.EndsWith("}"))
				throw new ExecutionException("Topic member " + name + " defined in " + Name.Fullname + " is not well-formed; missing closing '}' for code block.");
			ContentBase cb = CurrentFederation.ContentBaseForTopic(Name);
			TopicContext newContext = new TopicContext(ctx.CurrentFederation, cb, CurrentTopicInfo);
			BehaviorInterpreter interpreter = new BehaviorInterpreter(val, CurrentFederation, CurrentFederation.WikiTalkVersion, ctx.Presenter);
			if (!interpreter.Parse())
				throw new ExecutionException("Parsing error evaluating topic member " + name + " defined in " + Name.Fullname + ": " + interpreter.ErrorString);
			
			IBELObject b1 = interpreter.EvaluateToObject(newContext, ctx.ExternalWikiMap);
			if (b1 == null)
				throw new ExecutionException("Error while evaluating topic member " + name + " defined in " + Name.Fullname + ": " + interpreter.ErrorString);
			Block block = (Block)b1;
			ArrayList evaluatedArgs = new ArrayList();
			foreach (object each in arguments)
			{
				IBELObject add = null;
				if (each != null && each is IBELObject)
					add = each as IBELObject;
				else
				{
					ExposableParseTreeNode ptn = each as ExposableParseTreeNode;
					add = ptn.Expose(ctx);
				}
				evaluatedArgs.Add(add);
			}

			InvocationFrame invocationFrame = new InvocationFrame();
			ctx.PushFrame(invocationFrame);

			TopicScope topicScope = new TopicScope(null, this);
			ctx.PushScope(topicScope);		// make sure we can use local references
			IBELObject answer = block.Value(ctx, evaluatedArgs);
			ctx.PopScope();

			ctx.PopFrame();

			// make sure to transfer any new cache rules
			// BELTODO - want a test case for this 
			foreach (CacheRule r in interpreter.CacheRules)
				ctx.AddCacheRule(r);

			return answer;
		}

	}
}
