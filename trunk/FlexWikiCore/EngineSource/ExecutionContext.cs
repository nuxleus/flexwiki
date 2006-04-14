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
using System.Reflection;
using System.Web.Caching;

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// 
    /// </summary>
    public class ExecutionContext
    {
        // Fields

        private ArrayList _cacheRules = new ArrayList();
        private TopicContext _currentTopicContext;
        private ExternalReferencesMap _externalWikiMap;
        private ArrayList _frameStack = new ArrayList();
        private IScope _globalScope;
        private Home _home;
        private ArrayList _locationStack = new ArrayList();
        private IWikiToPresentation _presenter;
        private TypeRegistry _typeRegistry = new TypeRegistry();
        private int _wikiTalkVersion = 1;

        // Constructors

        public ExecutionContext()
        {
            Initialize();
        }
        public ExecutionContext(TopicContext ctx)
        {
            _currentTopicContext = ctx;
            Initialize();
        }

        // Properties

        public ICollection CacheRules
        {
            get
            {
                return _cacheRules;
            }
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
        public BELLocation CurrentLocation
        {
            get
            {
                if (_locationStack.Count == 0)
                    return null;
                return (BELLocation)(_locationStack[_locationStack.Count - 1]);
            }
        }
        public NamespaceManager CurrentNamespaceManager
        {
            get
            {
                if (CurrentTopicContext == null)
                    return null;
                return CurrentTopicContext.CurrentNamespaceManager;
            }
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
        public DynamicTopic CurrentTopic
        {
            get
            {
                if (CurrentTopicName == null)
                    return null;
                return new DynamicTopic(CurrentFederation, CurrentTopicName);
            }
        }
        public TopicContext CurrentTopicContext
        {
            get
            {
                return _currentTopicContext;
            }
        }
        public NamespaceQualifiedTopicVersionKey CurrentTopicName
        {
            get
            {
                if (CurrentTopicContext == null)
                    return null;
                return CurrentTopicContext.CurrentTopic.TopicVersionKey;
            }
        }
        public ExternalReferencesMap ExternalWikiMap
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
        public Home Home
        {
            get
            {
                return _home;
            }
        }
        public IWikiToPresentation Presenter
        {
            get
            {
                return _presenter;
            }
            set
            {
                _presenter = value;
            }
        }
        public int StackDepth
        {
            get
            {
                return _frameStack.Count;
            }
        }
        public InvocationFrame TopFrame
        {
            get
            {
                if (_frameStack.Count == 0)
                    return null;
                return (InvocationFrame)(_frameStack[_frameStack.Count - 1]);
            }
        }
        public TypeRegistry TypeRegistry
        {
            get
            {
                return _typeRegistry;
            }
        }
        public int WikiTalkVersion
        {
            get
            {
                return _wikiTalkVersion;
            }
            set
            {
                _wikiTalkVersion = value;
            }
        }

        private IScope GlobalScope
        {
            get
            {
                if (_globalScope != null)
                {
                    return _globalScope;
                }
                WithScope with = new WithScope(null);
                with.AddObject(TypeRegistry);
                with.AddObject(new Utility());
                with.AddObject(new ClassicBehaviors());
                _globalScope = with;
                return _globalScope;
            }
        }

        // Methods

        /// <summary>
        /// Answer the rawValue of the given propertyName or function.  
        /// Consider temporary variables followed by globals.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public IBELObject FindAndInvoke(string name, ArrayList args)
        {
            IBELObject answer = null;

            if (name == Home.ExternalTypeName)
            {
                return Home;
            }
            // Before we check anything else, we check the language builtin
            answer = Home.ValueOf(name, args, this);
            if (answer != null)
            {
                return answer;
            }

            // OK, not a builtin -- look into the current scope (and the containing scopes)
            if (TopFrame != null)
            {
                IScope scope = TopFrame.CurrentScope;
                while (scope != null)
                {
                    answer = scope.ValueOf(name, args, this);
                    if (answer != null)
                    {
                        return answer;
                    }
                    scope = scope.ContainingScope;
                }
            }

            // OK, didn't find it there -- let's try the global scope
            return GlobalScope.ValueOf(name, args, this);
        }
        public void PopFrame()
        {
            if (_frameStack.Count == 0)
                throw new Exception("Invocation frame stack has underflowed.");
            _frameStack.RemoveAt(_frameStack.Count - 1);
        }
        public void PopLocation()
        {
            if (_locationStack.Count == 0)
                throw new Exception("Location stack has underflowed.");
            _locationStack.RemoveAt(_locationStack.Count - 1);
        }
        public void PopScope()
        {
            InvocationFrame frame = TopFrame;
            if (frame == null)
                throw new Exception("Can't pop scope; no top frame.");
            frame.PopScope();
        }
        public void PushFrame(InvocationFrame f)
        {
            _frameStack.Add(f);
        }
        public void PushLocation(BELLocation loc)
        {
            _locationStack.Add(loc);
        }
        public void PushScope(IScope src)
        {
            InvocationFrame frame = TopFrame;
            if (frame == null)
                throw new Exception("Can't push scope; no top frame.");
            frame.PushScope(src);
        }

        private void Initialize()
        {
            _home = new Home();
            PushFrame(new InvocationFrame());	// lay down a base frame (so we push scopes even before a member invocation)						
        }

    }
}
