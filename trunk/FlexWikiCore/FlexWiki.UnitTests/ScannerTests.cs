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
			Assert.AreEqual(TokenType.TokenEndOfInput, t.Type); 
		}


		[Test] public void LastTokenTest()
		{
			Scanner scanner = new Scanner("1");
			Token t = scanner.Next();
			Assert.AreEqual(t, scanner.LatestToken); 
		}

		[Test] public void Pushbacktest()
		{
			Scanner scanner = new Scanner("100");
			Token t = scanner.Next();
			scanner.Pushback(t);
			Token t2 = scanner.Next();
			Assert.AreSame(t, t2); 
		}

		[Test] public void ScanTest()
		{
			Scanner scanner = new Scanner(@"|_ident{}abc_def(),abc123[]""he\""llo""100-100#");
			Assert.AreEqual(TokenType.TokenBar, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenIdentifier, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenLeftBrace, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenRightBrace, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenIdentifier, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenLeftParen, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenRightParen, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenComma, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenIdentifier, scanner.Next().Type);
			Assert.AreEqual("abc123", scanner.LatestToken.Value);
			Assert.AreEqual(TokenType.TokenLeftBracket, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenRightBracket, scanner.Next().Type);
			Assert.AreEqual(TokenType.TokenString, scanner.Next().Type);
			Assert.AreEqual(@"he""llo", scanner.LatestToken.Value);
			Assert.AreEqual(TokenType.TokenInteger, scanner.Next().Type);
			Assert.AreEqual("100", scanner.LatestToken.Value);
			Assert.AreEqual(TokenType.TokenInteger, scanner.Next().Type);
			Assert.AreEqual("-100", scanner.LatestToken.Value);
			Assert.AreEqual(TokenType.TokenOther, scanner.Next().Type);
		}


	}
}
