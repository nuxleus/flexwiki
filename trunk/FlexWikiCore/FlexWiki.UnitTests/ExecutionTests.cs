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
	[TestFixture] public class ExecutionTests : IWikiToPresentation
	{
		string Run(string input)
		{
			return Run(input, 1);
		}

		string Run(string input, int wikiTalkVersion)
		{
			BehaviorParser parser = new BehaviorParser();
			ExposableParseTreeNode obj = parser.Parse(input);
			Assertion.AssertNotNull(obj);
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

		[Test] public void TestSimpleLiterals()
		{
			Assertion.AssertEquals("P(100)", Run("100")); 
			Assertion.AssertEquals("P(-100)", Run("-100")); 
			Assertion.AssertEquals(@"P(mustard ""and"" greens)", Run(@"""mustard \""and\"" greens""")); 
		}

		[Test] public void TestBlockTypeChecking()
		{
			Assertion.AssertEquals("P(100)", Run(" {Integer  x | x }.Value(100)"));
			Assertion.AssertEquals("P(100)", Run(" {String  x | x }.Value(\"100\")"));
			Assertion.AssertEquals("P(2)", Run(" {Block  x | x.Value }.Value( { 2 })"));
		}



		[Test] public void TestSimpleVariableReference()
		{
			Assertion.AssertEquals("P(null)", Run("null"));
		}

		[Test] public void TestWithMethod()
		{
			Assertion.AssertEquals("P(el)", Run("with(\"hello\", {Substring(1,2)})"));
		}

		[Test] public void TestNewline()
		{ 
			Assertion.AssertEquals(@"P(
)", Run("Newline"));
		}

		[Test] public void TestTab()
		{ 
			Assertion.AssertEquals(@"P(	)", Run("Tab"));
		}

		[Test] public void TestSpace()
		{ 
			Assertion.AssertEquals(@"P( )", Run("Space"));
		}

		[Test] public void TestWithProperty()
		{
			Assertion.AssertEquals("P(5)", Run("with(\"hello\", {Length})"));
			Assertion.AssertEquals("P(2)", Run("with([1,2], {Count})"));
			Assertion.AssertEquals("P(12)", Run("with([1,2], {Collect({e | e})})"));
		}

		[Test] public void TestWithFindLast()
		{
			Assertion.AssertEquals("P(5)", Run("with(100, null, \"hello\", {Length})"));
		}

		[Test] public void TestWithFindFirst()
		{
			Assertion.AssertEquals("P(5)", Run("with( \"hello\", 100, null, {Length})"));
		}

		[Test] public void TestLiteralProperty()
		{
			Assertion.AssertEquals("P(5)", Run(@"""hello"".Length"));
		}

		[Test] public void TestChain()
		{
			Assertion.AssertEquals("P(5)", Run(@"""hello"".Reverse.Length"));
		}

		[Test] public void TestArgs()
		{
			Assertion.AssertEquals("P(llo)", Run(@"""hello"".Substring(2,3)"));
		}

		[Test] public void TestOptionalArgs()
		{
			Assertion.AssertEquals("P(llo)", Run(@"""hello"".Substring(2)"));
		}

		[Test] public void TestChainedArgs()
		{
			Assertion.AssertEquals("P(ll)", Run(@"""hello"".Substring(2,3).Substring(0,2)"));
		}

		[Test] public void TestToString()
		{
			Assertion.AssertEquals("P(5)", Run(@"10000.ToString.Length"));
			Assertion.AssertEquals("P(hello)", Run(@"""hello"".ToString"));
		}

		[Test] public void TestBadSignature()
		{
			Assertion.Assert(InvExcept(@"""hello"".Substring"));
			Assertion.Assert(InvExcept(@"""hello"".Substring(100, ""z"")"));
			Assertion.Assert(InvExcept(@"""hello"".Length(100)"));
		}

		[Test] public void TestFunctionWithArgsThatMustBeEvaluated()
		{
			Assertion.AssertEquals("P(llo)", Run(@"""hello"".Substring(""xx"".Length,""zzz"".Length)"));
		}

		[Test] public void TestArray()
		{
			Assertion.AssertEquals("P(3)", Run(@"[100, 200, ""hello""].Count"));
			Assertion.AssertEquals("P(1002005)", Run(@"[100, 200, ""hello"".Length]"));
			Assertion.AssertEquals("P(1002004)", Run(@"[100, 200, [1,2,3,4].Count]"));
			Assertion.AssertEquals("P(1002001234)", Run(@"[100, 200, [1,2,3,4]]"));
		}

		[Test] public void TestArrayIndex()
		{
			Assertion.AssertEquals("P(100)", Run(@"[100, 200, 300].Item(0)"));
			Assertion.AssertEquals("P(300)", Run(@"[100, 200, 300].Item(2)"));
		}

		[Test] public void TestNull()
		{
			Assertion.AssertEquals("P(null)", Run(@"null"));
		}

		[Test] public void TestTypeName()
		{
			Assertion.AssertEquals("P(Integer)", Run(@"100.Type.Name"));
			Assertion.AssertEquals("P(Array)", Run(@"[100,200].Type.Name"));
		}

		[Test] public void TestType()
		{
			Assertion.AssertEquals("P(Integer)", Run(@"100.Type.Name"));
			Assertion.AssertEquals("P(Array)", Run(@"[100,200].Type.Name"));
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

		[Test] public void TestNoSuchMember()
		{
			Assertion.Assert(ExpectNoSuchMember("fooCouldNotPossiblyExist"));
			Assertion.Assert(ExpectNoSuchMember("null.fooCouldNotPossiblyExist"));
		}


		[Test] public void TestArrayIndexOutOfBounds()
		{
			Assertion.Assert(ExpectIndexOutOfBounds("[100, 200, 300].Item(-1)"));
			Assertion.Assert(ExpectIndexOutOfBounds("[100, 200, 300].Item(-2)"));
			Assertion.Assert(ExpectIndexOutOfBounds("[100, 200, 300].Item(3)"));
			Assertion.Assert(ExpectIndexOutOfBounds("[100, 200, 300].Item(4)"));
		}

		[Test] public void TestCollect()
		{
			Assertion.AssertEquals("P(358)", 
				Run(@"[""abc"",""12345"",""12345678""].Collect({ Object each | each.Length})"));
		}


		[Test] public void TestBlockArgument()
		{
			Assertion.AssertEquals("P(358)", Run(@"[""abc"",""12345"",""12345678""].Collect {each | each.Length}"));
			Assertion.AssertEquals("P(358)", Run(@"[""abc"",""12345"",""12345678""].Collect {String each | each.Length}"));
		}

		[Test] public void TestMethodCacheNever()
		{
			IBELObject p = new Dummy();
			ExecutionContext ctx = new ExecutionContext();
			IBELObject v1 = p.ValueOf("NumberNever", null, ctx);
			Assertion.Assert(FindRule(ctx, typeof(CacheRuleNever)));
		}

		[Test] public void TestNestedBlockArg()
		{
			Assertion.AssertEquals("P(6)", 
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

		[Test] public void TestNestedWith()
		{
			Assertion.AssertEquals("P(5)", 
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

		[Test] public void TestTypes()
		{
			Assertion.AssertEquals("P("+ DateTime.MinValue.ToString() + ")", Run(@"with (types) {DateTime.MinValue}"));
			Assertion.AssertEquals("P("+ DateTime.MinValue.ToString() + ")", Run(@"types.DateTime.MinValue"));
		}

		[Test] public void TestEmpty()
		{
			Assertion.AssertEquals("P(0)", Run(@"empty.Length"));
		}


		[Test] public void TestMetaTypeBasics()
		{
			Assertion.AssertEquals("P("+ DateTime.MinValue.ToString() + ")", Run(@"Now.Type.MinValue"));
		}

		[Test] public void TestMetaTypeName()
		{
			Assertion.AssertEquals("P(DateTimeType)", Run(@"Now.Type.Type.Name"));
		}

		[Test] public void TestBoolTrue()
		{
			Assertion.AssertEquals("P(true)", Run(@"true"));
		}

		[Test] public void TestBoolFalse()
		{
			Assertion.AssertEquals("P(false)", Run(@"false"));
		}

		[Test] public void TestIfNull()
		{
			Assertion.AssertEquals("P(null)", Run(@"100.IfNull {""yuppo""}"));
			Assertion.AssertEquals("P(yuppo)", Run(@"null.IfNull {""yuppo""}"));
		}

		[Test] public void TestIfTrue()
		{
			Assertion.AssertEquals("P(5)", Run(@"true.IfTrue {5}"));
			Assertion.AssertEquals("P(null)", Run(@"false.IfTrue {5}"));
		}

		[Test] public void TestIfFalse()
		{
			Assertion.AssertEquals("P(5)", Run(@"false.IfFalse {5}"));
			Assertion.AssertEquals("P(null)", Run(@"true.IfFalse {5}"));
		}

		[Test] public void TestIfTrueIfFalse()
		{
			Assertion.AssertEquals("P(5)", Run(@"true.IfTrue {5} IfFalse {3}"));
			Assertion.AssertEquals("P(3)", Run(@"false.IfTrue {5} IfFalse {3}"));
		}

		[Test] public void TestIfFalseIfTrue()
		{
			Assertion.AssertEquals("P(5)", Run(@"false.IfFalse {5} IfTrue {3}"));
			Assertion.AssertEquals("P(3)", Run(@"true.IfFalse {5} IfTrue {3}"));
		}


		[Test] public void TestStringEquals()
		{
			Assertion.AssertEquals("P(true)", Run(@"""hello"".Equals(""hello"")"));
			Assertion.AssertEquals("P(false)", Run(@"""hello"".Equals(""goodbye"")"));
		}

		
		[Test] public void TestStringEqualsCaseInsensitive()
		{
			Assertion.AssertEquals("P(true)", Run(@"""hello"".EqualsCaseInsensitive(""hello"")"));
			Assertion.AssertEquals("P(true)", Run(@"""HELLO"".EqualsCaseInsensitive(""hello"")"));
			Assertion.AssertEquals("P(false)", Run(@"""hello"".EqualsCaseInsensitive(""goodbye"")"));
		}

		[Test] public void TestStringContains()
		{
			Assertion.AssertEquals("P(true)", Run(@"""hello"".Contains(""ell"")"));
			Assertion.AssertEquals("P(true)", Run(@"""hello"".Contains(""h"")"));
			Assertion.AssertEquals("P(false)", Run(@"""hello"".Contains(""xl"")"));
		}

		[Test] public void TestBooleanEquals()
		{
			Assertion.AssertEquals("P(true)", Run(@"false.Not.Equals(true)"));
			Assertion.AssertEquals("P(true)", Run(@"true.Equals(true)"));
			Assertion.AssertEquals("P(true)", Run(@"false.Equals(false)"));
			Assertion.AssertEquals("P(false)", Run(@"true.Equals(false)"));
		}
		
		[Test] public void TestIntegerEquals()
		{
			Assertion.AssertEquals("P(true)", Run(@"100.Equals(100)"));
			Assertion.AssertEquals("P(true)", Run(@"-100.Equals(-100)"));
			Assertion.AssertEquals("P(false)", Run(@"20.Equals(10)"));
			Assertion.AssertEquals("P(false)", Run(@"20.Equals(""blah"")"));
		}



		bool FindRule(ExecutionContext ctx, Type type)
		{
			foreach (CacheRule each in ctx.CacheRules)
				if (each.GetType() == type)
					return true;
			return false;
		}

		[Test] public void TestComplexCache()
		{
			IBELObject p = new Dummy();
			StringPTN s1 = new StringPTN(@"c:\boot.ini");
			ExecutionContext ctx = new ExecutionContext();
			ArrayList args = new ArrayList();
			args.Add(s1);

			IBELObject v = p.ValueOf("FileTime", args, ctx);

			Assertion.AssertEquals(1, ctx.CacheRules.Count);
			foreach (CacheRule r in ctx.CacheRules)
			{
				Assertion.AssertEquals(r.GetType(), typeof(FilesCacheRule));
				FilesCacheRule fcr = (FilesCacheRule)r;
				Assertion.AssertEquals(1, fcr.Files.Count);
				Assertion.AssertEquals(s1.Value, fcr.Files[0]);
			}
		}

		[Test] public void TestVarArgsZero()
		{
			IBELObject p = new Dummy();
			ExecutionContext ctx = new ExecutionContext();
			StringPTN s1 = new StringPTN(@"string1");
			StringPTN s2 = new StringPTN(@"string2");
			ArrayList args = new ArrayList();
			args.Add(s1);
			args.Add(s2);
			IBELObject v = p.ValueOf("ArgCounterZero", args, ctx);
			IOutputSequence seq = v.ToOutputSequence();
			Assertion.AssertEquals("P(2)", OutputSequenceToString(seq));			
		}

		[Test] public void TestVarArgsExtract()
		{
			IBELObject p = new Dummy();
			ExecutionContext ctx = new ExecutionContext();
			StringPTN s1 = new StringPTN(@"string1");
			StringPTN s2 = new StringPTN(@"string2");
			ArrayList args = new ArrayList();
			args.Add(new IntegerPTN("1"));
			args.Add(s1);
			args.Add(s2);
			IBELObject v = p.ValueOf("ExtractExtraArg", args, ctx);
			IOutputSequence seq = v.ToOutputSequence();
			Assertion.AssertEquals("P(string2)", OutputSequenceToString(seq));			
		}


		[Test] public void TestTypeForNameExists()
		{
			BELType belType = new BELType();
			BELType result = belType.TypeForName("NamespaceInfo");
			Assertion.AssertEquals("Type NamespaceInfo", result.ToString());
		}

		[Test] public void TestTypeForNameDoesNotExist()
		{
			BELType belType = new BELType();
			BELType result = belType.TypeForName("WardInfo");
			Assertion.AssertEquals(null, result);
		}


		class Dummy : BELObject
		{
			static int _NeverCounter = 0;

			[ExposedMethod(ExposedMethodFlags.CachePolicyNever, "")]
			public int NumberNever
			{
				get
				{
					return _NeverCounter++;
				}
			}

			static int _ForeverCounter = 10;
			[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "")]
			public int NumberForever
			{
				get
				{
					return _ForeverCounter++;
				}
			}

			[ExposedMethod(ExposedMethodFlags.CachePolicyComplex, "")]
			public DateTime FileTime(ExecutionContext ctx, string path)
			{
				ctx.AddCacheRule(new FilesCacheRule(path));
				return System.IO.File.GetLastWriteTime(path);
			}

			public override IOutputSequence ToOutputSequence()
			{
				return null;
			}

			[ExposedMethod(ExposedMethodFlags.AllowsVariableArguments, "")]
			public int ArgCounterZero(ExecutionContext ctx)
			{
				return ctx.TopFrame.ExtraArguments.Count;
			}

			[ExposedMethod(ExposedMethodFlags.AllowsVariableArguments, "")]
			public string ExtractExtraArg(ExecutionContext ctx, int which)
			{
				return (string)(ctx.TopFrame.ExtraArguments[which]);
			}

		}



		/* Other tests to add:
		 * unknown property error
		 * -0
		 * too large numbers
		 * 
		 * 
		 * 
		 */
	}
}
