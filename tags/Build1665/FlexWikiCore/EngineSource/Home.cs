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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using FlexWiki.Formatting;
using System.Diagnostics;
using System.Xml;
using System.Xml.Xsl;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("Home", "Hold the global objects available everywhere")]
	public class Home : BELObject
	{
		public Home() : base()
		{
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

		public override string ToString()
		{
			return "home object";
		}


		Request _Request;

		[ExposedMethod("request", ExposedMethodFlags.CachePolicyNever, "Answer the Request object describing the current user's request for this topic")]
		public Request CurrentRequest
		{
			get
			{
				if (_Request == null)
					_Request = new Request();
				return _Request;
			}
		}
		

		TopicContext _TopicContext;
		TopicContext TopicContext
		{
			get
			{
				return _TopicContext;
			}
			set
			{
				_TopicContext = value;
			}
		}

		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer an object whose properties are all of the different types in WikiTalk")]
		public TypeRegistry types(ExecutionContext ctx)
		{
			return ctx.TypeRegistry;
		}

		[ExposedMethod("this", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer an object whose properties are those of the current topic")]
		public DynamicTopic This(ExecutionContext ctx)
		{
			return ctx.CurrentTopic;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer an object whose properties are all of the topics in this namespace")]
		public DynamicNamespace topics(ExecutionContext ctx)
		{
			if (ctx.CurrentContentBase == null)
				return null;
			return new DynamicNamespace(ctx.CurrentFederation, ctx.CurrentContentBase.Namespace);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer a TopicInfo describing the current topic")]
		public TopicInfo topic(ExecutionContext ctx)
		{
			if (ctx.CurrentTopicName == null)
				return null;
			return new TopicInfo(ctx.CurrentFederation, ctx.CurrentTopicName);
		}

		[ExposedMethod("namespace", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer a Namespace describing the current namespace")]
		public ContentBase Namespace(ExecutionContext ctx)
		{
			return ctx.CurrentContentBase;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer the active Federation")]
		public Federation federation(ExecutionContext ctx)
		{
			return ctx.CurrentFederation;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever | ExposedMethodFlags.NeedContext, "Answer an Array of all of the types supported by WikiTalk")]
		public ArrayList allTypes(ExecutionContext ctx)
		{
			return ctx.TypeRegistry.AllTypes;
		}


		[ExposedMethod("null", ExposedMethodFlags.CachePolicyForever, "Answer the null object")]
		public string Null
		{
			get
			{
				return null;
			}
		}

		[ExposedMethod("true", ExposedMethodFlags.CachePolicyForever, "Answer the true object")]
		public bool True
		{
			get
			{
				return true;
			}
		}

		[ExposedMethod("false", ExposedMethodFlags.CachePolicyForever, "Answer the false object")]
		public bool False
		{
			get
			{
				return false;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer the empty string")]
		public BELString empty
		{
			get
			{
				return BELString.Empty;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.IsCustomArgumentProcessor | ExposedMethodFlags.AllowsVariableArguments, "Evaluate the given block with the supplied list of objects as implicit context for any member references")]
		public IBELObject with(ExecutionContext ctx)
		{
			InvocationFrame frame = ctx.TopFrame;
			WithScope with = new WithScope(ctx.CurrentScope);
			if (frame.ExtraArguments.Count < 1)
				throw new ArgumentException("With requires at least a block to execute; zero arguments supplied.");
			int pushCount = frame.ExtraArguments.Count - 1;
			for (int i = pushCount - 1; i >= 0; i--)
			{
				ExposableParseTreeNode e = (ExposableParseTreeNode)frame.ExtraArguments[i];
				IBELObject ex = e.Expose(ctx);
				with.AddObject(ex);
			}
			ctx.PushScope(with);

			// OK, now we have the new scope on the stack -- now it's safe to create the Block (since it'll "live in" the top scope
			ExposableParseTreeNode blockTree = (ExposableParseTreeNode)(frame.ExtraArguments[frame.ExtraArguments.Count - 1]);
			Block block = (blockTree.Expose(ctx)) as Block;
			if (block == null)
				throw new ArgumentException("With requires last argument to be a block; it isn't.");

			IBELObject answer = block.Value(ctx);
			ctx.PopScope();
			return answer;
		}

	}
}
				
		
