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

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	[TestFixture] public class ScannerTests
	{
		[SetUp] public void Init()
		{
		}

		[TearDown] public void Deinit()
		{
		}

		[Test] public void Emptyness()
		{
			Scanner scanner = new Scanner("");
			Token t = scanner.Next();
			Assertion.AssertEquals(TokenType.TokenEndOfInput, t.Type); 
		}


		[Test] public void LastTokenTest()
		{
			Scanner scanner = new Scanner("1");
			Token t = scanner.Next();
			Assertion.AssertSame(t, scanner.LatestToken); 
		}

		[Test] public void Pushbacktest()
		{
			Scanner scanner = new Scanner("100");
			Token t = scanner.Next();
			scanner.Pushback(t);
			Token t2 = scanner.Next();
			Assertion.AssertSame(t, t2); 
		}

		[Test] public void ScanTest()
		{
			Scanner scanner = new Scanner(@"|_ident{}abc_def(),abc123[]""he\""llo""100-100#");
			Assertion.AssertEquals(TokenType.TokenBar, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenIdentifier, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenLeftBrace, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenRightBrace, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenIdentifier, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenLeftParen, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenRightParen, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenComma, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenIdentifier, scanner.Next().Type);
			Assertion.AssertEquals("abc123", scanner.LatestToken.Value);
			Assertion.AssertEquals(TokenType.TokenLeftBracket, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenRightBracket, scanner.Next().Type);
			Assertion.AssertEquals(TokenType.TokenString, scanner.Next().Type);
			Assertion.AssertEquals(@"he""llo", scanner.LatestToken.Value);
			Assertion.AssertEquals(TokenType.TokenInteger, scanner.Next().Type);
			Assertion.AssertEquals("100", scanner.LatestToken.Value);
			Assertion.AssertEquals(TokenType.TokenInteger, scanner.Next().Type);
			Assertion.AssertEquals("-100", scanner.LatestToken.Value);
			Assertion.AssertEquals(TokenType.TokenOther, scanner.Next().Type);
		}


	}
}
