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
        private Federation _currentFederation;
        private TopicVersionInfo _currentTopicInfo;
        private QualifiedTopicRevision _key;

        public DynamicTopic(Federation aFed, QualifiedTopicRevision key)
        {
            _currentFederation = aFed;
            _key = key;
        }


        public QualifiedTopicRevision Name
        {
            get
            {
                return _key;
            }
        }

        public Federation CurrentFederation
        {
            get
            {
                return _currentFederation;
            }
        }

        TopicVersionInfo CurrentTopicInfo
        {
            get
            {
                if (_currentTopicInfo != null)
                    return _currentTopicInfo;
                _currentTopicInfo = new TopicVersionInfo(CurrentFederation, Name);
                return _currentTopicInfo;
            }
        }

        public override IOutputSequence ToOutputSequence()
        {
            // BELTODO -- test case (ensure that single word topic names work reasonably)
            return new WikiSequence(Name.Namespace + ".[" + Name.LocalName + "]");
        }

        public override IBELObject ValueOf(string name, System.Collections.ArrayList arguments, ExecutionContext ctx)
        {
            TopicPropertyCollection members = ctx.CurrentFederation.GetTopicProperties(Name);
            string val = members[name].LastValue;
            if (val == null)
                return null;
            val = val.Trim();
            bool isBlock = val.StartsWith("{");
            if (!isBlock)
                return new BELString(val);
            // It's a block, so fire up the interpreter
            if (!val.EndsWith("}"))
                throw new ExecutionException(ctx.CurrentLocation, "Topic member " + name + " defined in " + Name.QualifiedName + " is not well-formed; missing closing '}' for code block.");
            NamespaceManager cb = CurrentFederation.NamespaceManagerForTopic(Name);
            TopicContext newContext = new TopicContext(ctx.CurrentFederation, cb, CurrentTopicInfo);
            BehaviorInterpreter interpreter = new BehaviorInterpreter(Name.QualifiedName + "#" + name, val, CurrentFederation, CurrentFederation.WikiTalkVersion, ctx.Presenter);
            if (!interpreter.Parse())
                throw new ExecutionException(ctx.CurrentLocation, "Syntax error in " + interpreter.ErrorString);

            IBELObject b1 = interpreter.EvaluateToObject(newContext, ctx.ExternalWikiMap);
            if (b1 == null)
                throw new ExecutionException(ctx.CurrentLocation, "Execution error in " + interpreter.ErrorString);
            Block block = (Block) b1;
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

            return answer;
        }

    }
}
