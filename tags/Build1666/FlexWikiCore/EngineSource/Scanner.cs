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
using System.Text;

namespace FlexWiki
{
	public class Scanner
	{
		string _Input;
		int _Pos;
		public Scanner(string input)
		{
			_Input = input;
			_Pos = 0;;
		}

		ArrayList _Pushback = new ArrayList();
			
		Token _EndOfInput = new Token(TokenType.TokenEndOfInput);

		public void Pushback(Token t)
		{
			_Pushback.Add(t);
		}

		public bool AtEnd
		{
			get
			{
				if (_Pushback.Count == 0)
					return _Pos >= _Input.Length;
				else
					return ((Token)(_Pushback[_Pushback.Count - 1])).Type == TokenType.TokenEndOfInput;
			}
		}

		public Token LatestToken = null;

		/// <summary>
		/// Answer the next token or null if no more
		/// </summary>
		/// <returns></returns>
		public Token Next()
		{
			LatestToken = GetNextToken();
			return LatestToken;
		}

		Token GetNextToken()
		{
			// Find the next valid token from the following tokens:
			//Bar = '|'
			//LeftParen = '('
			//RightParen = ')'
			//Comma = ','
			//identifir = 'A-Za-z_' '_A-Za-z0-9'*
			//LeftBracket = '['
			//RightBracker = ']'
			//StringLiteral = "........."
			//Integer '-'? [0-9]+
			//LeftBrace '{'
			//RightBrace '}'

			// NOT IMPLEMENTED YET (EVER?)
			//Hash='#'
			//Plus='+'
			//Minus='-'
			//Divide='/'
			//Multiply='*'
			//Equality='=='
			//Less= '<'
			//LessEq= '<='
			//greater= '>'
			//GreaterEq= '>='
			//Not='!'
			//Modulo='%'
			// END LIST


			//Other (anything else -- 1 char at a time -- probably an error)
			
			if (_Pushback.Count != 0)
			{
				Token answer = (Token)(_Pushback[_Pushback.Count - 1]);
				_Pushback.RemoveAt(_Pushback.Count - 1);
				return answer;
			}

			// OK, we need to figure it out

			// Skip whitespace
			while (_Pos < _Input.Length && Char.IsWhiteSpace(_Input[_Pos]))
				_Pos++;

			if (AtEnd)
				return _EndOfInput;		// at the end

			char c = _Input[_Pos++];
			switch (c)
			{
				case '|':
					return new Token(TokenType.TokenBar);

				case '{':
					return new Token(TokenType.TokenLeftBrace);

				case '}':
					return new Token(TokenType.TokenRightBrace);
				
				case '(':
					return new Token(TokenType.TokenLeftParen);

				case '.':
					return new Token(TokenType.TokenPeriod);

				case ')':
					return new Token(TokenType.TokenRightParen);

				case '[':
					return new Token(TokenType.TokenLeftBracket);

				case ']':
					return new Token(TokenType.TokenRightBracket);

				case ',':
					return new Token(TokenType.TokenComma);
			}

			// OK, maybe a number
			if (Char.IsDigit(c) || (c == '-' && (_Pos < _Input.Length) && Char.IsDigit(_Input[_Pos])))
			{
				StringBuilder s = new StringBuilder();
				s.Append(c);
				while (_Pos < _Input.Length)
				{
					char next = _Input[_Pos];
					if (!Char.IsDigit(next))
						break;
					s.Append(next);
					_Pos++;
				}
				return new Token(TokenType.TokenInteger, s.ToString());
			}

			// A string literal?
			if (c == '"')
			{
				StringBuilder s = new StringBuilder();
				while (_Pos < _Input.Length)
				{
					char next = _Input[_Pos];
					if (next == '\\')
					{
						_Pos++;
						if (_Pos < _Input.Length)
						{
							s.Append(_Input[_Pos]);
							_Pos++;
						}
						continue;
					}
					if (next == '"')
					{
						_Pos++;
						break;
					}
					s.Append(next);
					_Pos++;
				}
				return new Token(TokenType.TokenString, s.ToString());
			}

			// Identifier
			if (Char.IsLetter(c) || c == '_')
			{
				StringBuilder s = new StringBuilder();
				s.Append(c);
				while (_Pos < _Input.Length)
				{
					char next = _Input[_Pos];
					if (!Char.IsLetterOrDigit(next) && next != '_')
						break;
					s.Append(next);
					_Pos++;
				}
				return new Token(TokenType.TokenIdentifier, s.ToString());
			}

			return new Token(TokenType.TokenOther);
		}			
	}
}
