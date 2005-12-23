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
		int _Line;
		int _Column;

		public Scanner(string input)
		{
			_Input = input;
			_Pos = 0;
			_Line = 1;
			_Column = 1;
		}

		ArrayList _Pushback = new ArrayList();
			
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

		void Track(char c)
		{
			if (c == '\n')
			{
				_Line++;
				_Column = 1;
			}
			else
			{
				_Column++;
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
			//identifier = 'A-Za-z_' '_A-Za-z0-9'*
			//LeftBracket = '['
			//RightBracker = ']'
			//StringLiteral = "........."
			//Integer '-'? [0-9]+
			//LeftBrace '{'
			//RightBrace '}'
			//Semicolon ';'

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
			{
				Track(_Input[_Pos]);
				_Pos++;
			}

			// Record where this token starts for position reporting
			int lineStart = _Line;
			int columnStart = _Column;

			if (AtEnd)
				return new Token(TokenType.TokenEndOfInput, lineStart, columnStart);		// at the end

			char c = _Input[_Pos++];
			Track(c);
			switch (c)
			{
				case '|':
					return new Token(TokenType.TokenBar, lineStart, columnStart);

				case ';':
					return new Token(TokenType.TokenSemicolon, lineStart, columnStart);

				case '{':
					return new Token(TokenType.TokenLeftBrace, lineStart, columnStart);

				case '}':
					return new Token(TokenType.TokenRightBrace, lineStart, columnStart);
				
				case '(':
					return new Token(TokenType.TokenLeftParen, lineStart, columnStart);

				case '.':
					return new Token(TokenType.TokenPeriod, lineStart, columnStart);

				case ')':
					return new Token(TokenType.TokenRightParen, lineStart, columnStart);

				case '[':
					return new Token(TokenType.TokenLeftBracket, lineStart, columnStart);

				case ']':
					return new Token(TokenType.TokenRightBracket, lineStart, columnStart);

				case ',':
					return new Token(TokenType.TokenComma, lineStart, columnStart);
			}

			// OK, maybe a number
			if (Char.IsDigit(c) || (c == '-' && (_Pos < _Input.Length) && Char.IsDigit(_Input[_Pos])))
			{
				bool isInt = true;
				StringBuilder s = new StringBuilder();
				s.Append(c);
				while (_Pos < _Input.Length)
				{
					char next = _Input[_Pos];
					if (!Char.IsDigit(next))
					{
						isInt = false;
						//maybe an identifier
						if  (!Char.IsLetter(next))
							break;
					}
					s.Append(next);
					_Pos++;
					Track(next);
				}
				if (isInt)
				{
					return new Token(TokenType.TokenInteger, s.ToString(), lineStart, columnStart);
				}
				else
				{
					try
					{
						int integer = int.Parse(s.ToString());
						//if ok, it should be an Integer
						return new Token(TokenType.TokenInteger, s.ToString(), lineStart, columnStart);
					}
					catch 
					{
						return new Token(TokenType.TokenIdentifier, s.ToString(), lineStart, columnStart);
					}
				}
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
						Track(next);
						_Pos++;
						if (_Pos < _Input.Length)
						{
							s.Append(_Input[_Pos]);
							Track(_Input[_Pos]);
							_Pos++;
						}
						continue;
					}
					if (next == '"')
					{
						Track(next);
						_Pos++;
						break;
					}
					s.Append(next);
					Track(next);
					_Pos++;
				}
				return new Token(TokenType.TokenString, s.ToString(), lineStart, columnStart);
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
					Track(next);
					_Pos++;
				}
				return new Token(TokenType.TokenIdentifier, s.ToString(), lineStart, columnStart);
			}

			return new Token(TokenType.TokenOther, lineStart, columnStart);
		}			
	}
}
