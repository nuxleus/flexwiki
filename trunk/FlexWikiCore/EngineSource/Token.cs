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

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class Token
	{
		public Token(TokenType type, string val, int l, int c)
		{
			Line = l;
			Column = c;
			Type = type;
			Value = val;
		}

		public Token(TokenType type, int l, int c)
		{
			Line = l;
			Column = c;
			Type = type;
			Value = null;
		}

		public override string ToString()
		{
			string answer = "";
			switch (Type)
			{
				case TokenType.TokenBar:
					answer = "vertical bar '|'";
					break;

				case TokenType.TokenLeftBrace:
					answer = "left brace '{'";
					break;

				case TokenType.TokenRightBrace:
					answer = "right brace '}'";
					break;

				case TokenType.TokenLeftParen:
					answer = "left parenthesis '('";
					break;

				case TokenType.TokenRightParen:
					answer = "right parenthesis ')'";
					break;

				case TokenType.TokenComma:
					answer = "comma ','";
					break;

				case TokenType.TokenIdentifier:
					answer = "identifier " + (Value == null ? "" : " (" + Value + ")");
					break;

				case TokenType.TokenLeftBracket:
					answer = "left bracket '['";
					break;

				case TokenType.TokenSemicolon:
					answer = "semi-colon ';'";
					break;

				case TokenType.TokenRightBracket:
					answer = "right bracket ']'";
					break;

				case TokenType.TokenString:
					answer = "string" + (Value == null ? "" : " (" + Value + ")");
					break;

				case TokenType.TokenInteger:
					answer = "integer" + (Value == null ? "" : " (" + Value + ")");
					break;

				case TokenType.TokenPeriod:
					break;

				case TokenType.TokenEndOfInput:
				case TokenType.TokenOther:
				default:
					answer = Type.ToString() + (Value == null ? "" : " (" + Value + ")");
					break;
			}

			return answer;
		}

		public TokenType Type;
		public string Value;
		public int Line;
		public int Column;
	}


}
