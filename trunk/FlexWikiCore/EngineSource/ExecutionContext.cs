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
	public class ExecutionContext
	{

		public ExecutionContext(TopicContext ctx)
		{
			_CurrentTopicContext = ctx;
			Initialize();
		}

		void Initialize()
		{
			_Home = new Home();
			PushFrame(new InvocationFrame());	// lay down a base frame (so we push scopes even before a member invocation)						
		}
		
		public ExecutionContext()
		{
			Initialize();
		}

		TopicContext _CurrentTopicContext;
		public TopicContext CurrentTopicContext
		{
			get
			{
				return _CurrentTopicContext;
			}
		}

		ArrayList	_CacheRules = new ArrayList();

		public ICollection CacheRules
		{
			get
			{
				return _CacheRules;
			}
		}

		public void AddCacheRule(CacheRule rule)
		{
			_CacheRules.Add(rule);
		}

		ArrayList _FrameStack = new ArrayList();
		public InvocationFrame TopFrame
		{
			get
			{
				if (_FrameStack.Count == 0)
					return null;
				return (InvocationFrame)(_FrameStack[_FrameStack.Count - 1]);
			}
		}

		public void PushFrame(InvocationFrame f)
		{
			_FrameStack.Add(f);
		}

		public void PopFrame()
		{
			if (_FrameStack.Count == 0)
				throw new Exception("Invocation frame stack has underflowed.");
			_FrameStack.RemoveAt(_FrameStack.Count - 1);
		}		
		
		
		public Federation CurrentFederation
		{
			get
			{
				if (CurrentTopicContext == null)
					return null;
				return CurrentTopicContext.CurrentFederation;
			}
		}

		public AbsoluteTopicName CurrentTopicName
		{
			get
			{
				if (CurrentTopicContext == null)
					return null;
				return CurrentTopicContext.CurrentTopic.Fullname;
			}
		}

		public DynamicTopic CurrentTopic
		{
			get
			{
				if (CurrentTopicName == null)
					return null;
				return new DynamicTopic(CurrentFederation, CurrentTopicName);
			}
		}

		public ContentBase CurrentContentBase
		{
			get
			{
				if (CurrentTopicContext == null)
					return null;
				return CurrentTopicContext.CurrentContentBase;
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



		Home _Home;
		Home Home
		{
			get
			{
				return _Home;
			}
		}

		/// <summary>
		/// Answer the value of the given property or function.  
		/// Consider temporary variables followed by globals.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="args"></param>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public IBELObject FindAndInvoke(string name, ArrayList args)
		{
			IBELObject answer = null;

			// Before we check anything else, we check the language builtin
			answer = Home.ValueOf(name, args, this);
			if (answer != null)
				return answer;

			// OK, not a builtin -- look into the current scope (and the containing scopes)
			if (TopFrame != null)
			{
				IScope scope = TopFrame.CurrentScope;
				while (scope != null)
				{
					answer = scope.ValueOf(name, args, this);
					if (answer != null)
						return answer;
					scope = scope.ContainingScope;
				}
			}

			// OK, didn't find it there -- let's try the global scope
			return GlobalScope.ValueOf(name, args, this);
		}


		int _WikiTalkVersion = 1;	
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

		IScope _GlobalScope;
		IScope GlobalScope
		{
			get
			{
				if (_GlobalScope != null)
					return _GlobalScope;
				WithScope with = new WithScope(null);
				with.AddObject(TypeRegistry);
				with.AddObject(new Utility());
				with.AddObject(new ClassicBehaviors());
				_GlobalScope = with;
				return _GlobalScope;
			}
		}

		TypeRegistry _TypeRegistry = new TypeRegistry();
		public TypeRegistry TypeRegistry
		{
			get
			{
				return _TypeRegistry;
			}
		}

		public void PushScope(IScope src)
		{
			InvocationFrame frame = TopFrame;
			if (frame == null)
				throw new Exception("Can't push scope; no top frame.");
			frame.PushScope(src);
		}

		public void PopScope()
		{
			InvocationFrame frame = TopFrame;
			if (frame == null)
				throw new Exception("Can't pop scope; no top frame.");
			frame.PopScope();
		}

		public IScope CurrentScope
		{
			get
			{
				InvocationFrame frame = TopFrame;
				if (frame == null)
					return null;
				return frame.CurrentScope;
			}
		}


	}
}
