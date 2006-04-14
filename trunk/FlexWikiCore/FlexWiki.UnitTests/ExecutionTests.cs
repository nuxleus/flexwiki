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

using NUnit.Framework;
using FlexWiki.Formatting;
using System.Collections;


namespace FlexWiki.UnitTests
{
    /// <summary>
    /// Summary description for ExecutionTests.
    /// </summary>
    [TestFixture]
    public class ExecutionTests : IWikiToPresentation
    {
        string Run(string input)
        {
            return Run(input, 1);
        }

        const string ContextString = "Unit Tests";

        string Run(string input, int wikiTalkVersion)
        {
            BehaviorParser parser = new BehaviorParser(ContextString);
            ExposableParseTreeNode obj = parser.Parse(input);
            Assert.IsNotNull(obj);
            ExecutionContext ctx = new ExecutionContext();
            ctx.WikiTalkVersion = wikiTalkVersion;
            IBELObject evaluated = obj.Expose(ctx);
            IOutputSequence seq = evaluated.ToOutputSequence();
            return OutputSequenceToString(seq);
        }

        string OutputSequenceToString(IOutputSequence s)
        {
            WikiOutput output = WikiOutput.ForFormat(OutputFormat.Testing, null);
            s.ToPresentation(this).OutputTo(output);
            return output.ToString();
        }

        bool InvExcept(string str)
        {
            try
            {
                Run(str);
            }
            catch (MemberInvocationException)
            {
                return true;
            }
            return false;
        }

        public string WikiToPresentation(string s)
        {
            return "P(" + s + ")";
        }

        [Test]
        public void TestSimpleLiterals()
        {
            Assert.AreEqual("P(100)", Run("100"));
            Assert.AreEqual("P(-100)", Run("-100"));
            Assert.AreEqual(@"P(mustard ""and"" greens)", Run(@"""mustard \""and\"" greens"""));
        }

        [Test]
        public void TestBlockTypeChecking()
        {
            Assert.AreEqual("P(100)", Run(" {Integer  x | x }.Value(100)"));
            Assert.AreEqual("P(100)", Run(" {String  x | x }.Value(\"100\")"));
            Assert.AreEqual("P(2)", Run(" {Block  x | x.Value }.Value( { 2 })"));
        }



        [Test]
        public void TestSimpleVariableReference()
        {
            Assert.AreEqual("P(null)", Run("null"));
        }

        [Test]
        public void TestWithMethod()
        {
            Assert.AreEqual("P(el)", Run("with(\"hello\", {Substring(1,2)})"));
        }

        [Test]
        public void TestNewline()
        {
            Assert.AreEqual(@"P(
)", Run("Newline"));
        }

        [Test]
        public void TestTab()
        {
            Assert.AreEqual(@"P(	)", Run("Tab"));
        }

        [Test]
        public void TestSpace()
        {
            Assert.AreEqual(@"P( )", Run("Space"));
        }

        [Test]
        public void TestWithProperty()
        {
            Assert.AreEqual("P(5)", Run("with(\"hello\", {Length})"));
            Assert.AreEqual("P(2)", Run("with([1,2], {Count})"));
            Assert.AreEqual("P(12)", Run("with([1,2], {Collect({e | e})})"));
        }

        [Test]
        public void TestWithFindLast()
        {
            Assert.AreEqual("P(5)", Run("with(100, null, \"hello\", {Length})"));
        }

        [Test]
        public void TestWithFindFirst()
        {
            Assert.AreEqual("P(5)", Run("with( \"hello\", 100, null, {Length})"));
        }

        [Test]
        public void TestLiteralProperty()
        {
            Assert.AreEqual("P(5)", Run(@"""hello"".Length"));
        }

        [Test]
        public void TestChain()
        {
            Assert.AreEqual("P(5)", Run(@"""hello"".Reverse.Length"));
        }


        [Test]
        public void TestExpressionChain()
        {
            Assert.AreEqual("P(5)", Run(@"100; 200; ""hello"".Reverse.Length"));
        }


        [Test]
        public void TestArgs()
        {
            Assert.AreEqual("P(llo)", Run(@"""hello"".Substring(2,3)"));
        }

        [Test]
        public void TestOptionalArgs()
        {
            Assert.AreEqual("P(llo)", Run(@"""hello"".Substring(2)"));
        }

        [Test]
        public void TestChainedArgs()
        {
            Assert.AreEqual("P(ll)", Run(@"""hello"".Substring(2,3).Substring(0,2)"));
        }

        [Test]
        public void TestToString()
        {
            Assert.AreEqual("P(5)", Run(@"10000.ToString.Length"));
            Assert.AreEqual("P(hello)", Run(@"""hello"".ToString"));
        }

        [Test]
        public void TestBadSignature()
        {
            Assert.IsTrue(InvExcept(@"""hello"".Substring"));
            Assert.IsTrue(InvExcept(@"""hello"".Substring(100, ""z"")"));
            Assert.IsTrue(InvExcept(@"""hello"".Length(100)"));
        }

        [Test]
        public void TestFunctionWithArgsThatMustBeEvaluated()
        {
            Assert.AreEqual("P(llo)", Run(@"""hello"".Substring(""xx"".Length,""zzz"".Length)"));
        }

        [Test]
        public void TestArray()
        {
            Assert.AreEqual("P(3)", Run(@"[100, 200, ""hello""].Count"));
            Assert.AreEqual("P(1002005)", Run(@"[100, 200, ""hello"".Length]"));
            Assert.AreEqual("P(1002004)", Run(@"[100, 200, [1,2,3,4].Count]"));
            Assert.AreEqual("P(1002001234)", Run(@"[100, 200, [1,2,3,4]]"));
        }

        [Test]
        public void TestArrayIndex()
        {
            Assert.AreEqual("P(100)", Run(@"[100, 200, 300].Item(0)"));
            Assert.AreEqual("P(300)", Run(@"[100, 200, 300].Item(2)"));
        }

        [Test]
        public void TestNull()
        {
            Assert.AreEqual("P(null)", Run(@"null"));
        }

        [Test]
        public void TestTypeName()
        {
            Assert.AreEqual("P(Integer)", Run(@"100.Type.Name"));
            Assert.AreEqual("P(Array)", Run(@"[100,200].Type.Name"));
        }

        [Test]
        public void TestType()
        {
            Assert.AreEqual("P(Integer)", Run(@"100.Type.Name"));
            Assert.AreEqual("P(Array)", Run(@"[100,200].Type.Name"));
        }

        bool ExpectIndexOutOfBounds(string str)
        {
            try
            {
                Run(str);
            }
            catch (IndexOutOfRangeException)
            {
                return true;
            }
            return false;
        }

        bool ExpectNoSuchMember(string str)
        {
            try
            {
                Run(str);
            }
            catch (NoSuchMemberException)
            {
                return true;
            }
            return false;
        }

        [Test]
        public void TestNoSuchMember()
        {
            Assert.IsTrue(ExpectNoSuchMember("fooCouldNotPossiblyExist"));
            Assert.IsTrue(ExpectNoSuchMember("null.fooCouldNotPossiblyExist"));
        }


        [Test]
        public void TestArrayIndexOutOfBounds()
        {
            Assert.IsTrue(ExpectIndexOutOfBounds("[100, 200, 300].Item(-1)"));
            Assert.IsTrue(ExpectIndexOutOfBounds("[100, 200, 300].Item(-2)"));
            Assert.IsTrue(ExpectIndexOutOfBounds("[100, 200, 300].Item(3)"));
            Assert.IsTrue(ExpectIndexOutOfBounds("[100, 200, 300].Item(4)"));
        }

        [Test]
        public void TestCollect()
        {
            Assert.AreEqual("P(358)",
                Run(@"[""abc"",""12345"",""12345678""].Collect({ Object each | each.Length})"));
        }


        [Test]
        public void TestBlockArgument()
        {
            Assert.AreEqual("P(358)", Run(@"[""abc"",""12345"",""12345678""].Collect {each | each.Length}"));
            Assert.AreEqual("P(358)", Run(@"[""abc"",""12345"",""12345678""].Collect {String each | each.Length}"));
        }

#if false
		[Test] public void TestMethodCacheNever()
		{
			IBELObject p = new Dummy();
			ExecutionContext ctx = new ExecutionContext();
			IBELObject v1 = p.ValueOf("NumberNever", null, ctx);
			Assert.IsTrue(FindRule(ctx, typeof(CacheRuleNever)));
		}
#endif
        [Test]
        public void TestNestedBlockArg()
        {
            Assert.AreEqual("P(6)",
                Run(@"
with(""short"")
{
	with(""longer"")
	{
		Length
	}
}
"));
        }

        [Test]
        public void TestNestedWith()
        {
            Assert.AreEqual("P(5)",
                Run(@"
with(""short"")
{
	with(null)
	{
		Length
	}
}
"));
        }

        [Test]
        public void TestTypes()
        {
            Assert.AreEqual("P(" + DateTime.MinValue.ToString() + ")", Run(@"with (types) {DateTime.MinValue}"));
            Assert.AreEqual("P(" + DateTime.MinValue.ToString() + ")", Run(@"types.DateTime.MinValue"));
        }

        [Test]
        public void TestEmpty()
        {
            Assert.AreEqual("P(0)", Run(@"empty.Length"));
        }


        [Test]
        public void TestMetaTypeBasics()
        {
            Assert.AreEqual("P(" + DateTime.MinValue.ToString() + ")", Run(@"Now.Type.MinValue"));
        }

        [Test]
        public void TestMetaTypeName()
        {
            Assert.AreEqual("P(DateTimeType)", Run(@"Now.Type.Type.Name"));
        }

        [Test]
        public void TestBoolTrue()
        {
            Assert.AreEqual("P(true)", Run(@"true"));
        }

        [Test]
        public void TestBoolFalse()
        {
            Assert.AreEqual("P(false)", Run(@"false"));
        }

        [Test]
        public void TestIfNull()
        {
            Assert.AreEqual("P(null)", Run(@"100.IfNull {""yuppo""}"));
            Assert.AreEqual("P(yuppo)", Run(@"null.IfNull {""yuppo""}"));
        }

        [Test]
        public void TestIfTrue()
        {
            Assert.AreEqual("P(5)", Run(@"true.IfTrue {5}"));
            Assert.AreEqual("P(null)", Run(@"false.IfTrue {5}"));
        }

        [Test]
        public void TestIfFalse()
        {
            Assert.AreEqual("P(5)", Run(@"false.IfFalse {5}"));
            Assert.AreEqual("P(null)", Run(@"true.IfFalse {5}"));
        }

        [Test]
        public void TestIfTrueIfFalse()
        {
            Assert.AreEqual("P(5)", Run(@"true.IfTrue {5} IfFalse {3}"));
            Assert.AreEqual("P(3)", Run(@"false.IfTrue {5} IfFalse {3}"));
        }

        [Test]
        public void TestIfFalseIfTrue()
        {
            Assert.AreEqual("P(5)", Run(@"false.IfFalse {5} IfTrue {3}"));
            Assert.AreEqual("P(3)", Run(@"true.IfFalse {5} IfTrue {3}"));
        }


        [Test]
        public void TestStringEquals()
        {
            Assert.AreEqual("P(true)", Run(@"""hello"".Equals(""hello"")"));
            Assert.AreEqual("P(false)", Run(@"""hello"".Equals(""goodbye"")"));
        }


        [Test]
        public void TestStringEqualsCaseInsensitive()
        {
            Assert.AreEqual("P(true)", Run(@"""hello"".EqualsCaseInsensitive(""hello"")"));
            Assert.AreEqual("P(true)", Run(@"""HELLO"".EqualsCaseInsensitive(""hello"")"));
            Assert.AreEqual("P(false)", Run(@"""hello"".EqualsCaseInsensitive(""goodbye"")"));
        }

        [Test]
        public void TestStringContains()
        {
            Assert.AreEqual("P(true)", Run(@"""hello"".Contains(""ell"")"));
            Assert.AreEqual("P(true)", Run(@"""hello"".Contains(""h"")"));
            Assert.AreEqual("P(false)", Run(@"""hello"".Contains(""xl"")"));
        }

        [Test]
        public void TestBooleanEquals()
        {
            Assert.AreEqual("P(true)", Run(@"false.Not.Equals(true)"));
            Assert.AreEqual("P(true)", Run(@"true.Equals(true)"));
            Assert.AreEqual("P(true)", Run(@"false.Equals(false)"));
            Assert.AreEqual("P(false)", Run(@"true.Equals(false)"));
        }

        [Test]
        public void TestIntegerEquals()
        {
            Assert.AreEqual("P(true)", Run(@"100.Equals(100)"));
            Assert.AreEqual("P(true)", Run(@"-100.Equals(-100)"));
            Assert.AreEqual("P(false)", Run(@"20.Equals(10)"));
            Assert.AreEqual("P(false)", Run(@"20.Equals(""blah"")"));
        }

        #region BELString integer conversion
        [Test]
        public void ConvertStringToInteger()
        {
            Assert.AreEqual(@"P(true)", Run(@"""123"".AsInteger.Equals(123)"));
        }
        #endregion
#if false
		bool FindRule(ExecutionContext ctx, Type type)
		{
			foreach (CacheRule each in ctx.CacheRules)
				if (each.GetType() == type)
					return true;
			return false;
		}
#endif
        [Test]
        public void TestVarArgsZero()
        {
            IBELObject p = new Dummy();
            ExecutionContext ctx = new ExecutionContext();
            StringPTN s1 = new StringPTN(new BELLocation(ContextString, 1, 1), @"string1");
            StringPTN s2 = new StringPTN(new BELLocation(ContextString, 1, 1), @"string2");
            ArrayList args = new ArrayList();
            args.Add(s1);
            args.Add(s2);
            IBELObject v = p.ValueOf("ArgCounterZero", args, ctx);
            IOutputSequence seq = v.ToOutputSequence();
            Assert.AreEqual("P(2)", OutputSequenceToString(seq));
        }

        [Test]
        public void TestVarArgsExtract()
        {
            IBELObject p = new Dummy();
            ExecutionContext ctx = new ExecutionContext();
            StringPTN s1 = new StringPTN(new BELLocation(ContextString, 1, 1), @"string1");
            StringPTN s2 = new StringPTN(new BELLocation(ContextString, 1, 1), @"string2");
            ArrayList args = new ArrayList();
            args.Add(new IntegerPTN(new BELLocation(ContextString, 1, 1), "1"));
            args.Add(s1);
            args.Add(s2);
            IBELObject v = p.ValueOf("ExtractExtraArg", args, ctx);
            IOutputSequence seq = v.ToOutputSequence();
            Assert.AreEqual("P(string2)", OutputSequenceToString(seq));
        }


        [Test]
        public void TestTypeForNameExists()
        {
            BELType belType = new BELType();
            BELType result = belType.TypeForName("NamespaceInfo");
            Assert.AreEqual("Type NamespaceInfo", result.ToString());
        }

        [Test]
        public void TestTypeForNameDoesNotExist()
        {
            BELType belType = new BELType();
            BELType result = belType.TypeForName("WardInfo");
            Assert.AreEqual(null, result);
        }




        /* Other tests to add:
         * unknown propertyName error
         * -0
         * too large numbers
         * 
         * 
         * 
         */
    }
}
