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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class BehaviorParser
	{
		public BehaviorParser()
		{
		}

		Scanner _Scanner;

		public ExposableParseTreeNode Parse(string input)
		{
			_Scanner = new Scanner(input);
			ExposableParseTreeNode answer = Expression();
			if (answer == null)
				throw new ExpectedTokenParseException(ExpectedWhat, ButGotToken);
			if (!Scanner.AtEnd)
				throw new UnexpectedTokenParseException(Scanner.Next());
			return answer;
		}

		Scanner Scanner
		{
			get
			{
				return _Scanner;
			}
		}

		void Expected(string what)
		{
			ExpectedWhat = what;
			ButGotToken = Scanner.LatestToken;
		}

		public string ExpectedWhat;
		public Token ButGotToken;

/* Node functions:
 * if matches
 *	return treeNode
 *  scanner positioned on next
 * else
 *  return null
 *  log expected via Expected(...)
 *  scanner positioned on next (i.e., nothing consumed)
 *  -- if the scanner can't be properly positioned because we looked too far ahead, we throw a parse exception
 */
		/*
		 	expr := 
			literal |
			literal PERIOD ReferenceChain |
			ReferenceChain
		*/

		ExposableParseTreeNode Expression()
		{
			ExposableParseTreeNode answer;
			
			answer = Literal();
			if (answer != null)
			{
				Token next = Scanner.Next();
				if (next.Type != TokenType.TokenPeriod)
				{
					Scanner.Pushback(next);
					return answer;
				}
				else
				{
					ParseTreeNode chain = ReferenceChain();
					if (chain == null)
						return null;
					return new DereferencePTN(answer, chain);
				}
			}
			answer = ReferenceChain();
			return answer;
		}

		/*
		 
			ReferenceChain :=
			symbolReference |
			symbolReference PERIOD referenceChain
		*/

		ExposableParseTreeNode ReferenceChain()
		{
			ExposableParseTreeNode reference = SymbolReference();

			if (reference == null)
			{
				Expected("property or function reference");
				return null;
			}
 			
			Token t = Scanner.Next();
			if (t == null)
				return reference;

			if (t.Type != TokenType.TokenPeriod)
			{
				Scanner.Pushback(t);
				return reference;
			}

			ParseTreeNode rightSide = ReferenceChain();
			if (rightSide == null)
			{
				Expected("expression");
				return null;
			}

			return new DereferencePTN(reference, rightSide);
		}


/*
		symbolReference :=
			IDENTIFIER blockArguments |
			IDENTIFIER LEFTPAREN args RIGHTPAREN blockArguments
*/

		ExposableParseTreeNode SymbolReference()
		{
			string ex = "property or function name";
			Token t = Scanner.Next();
			if (t.Type != TokenType.TokenIdentifier)
			{
				Scanner.Pushback(t);
				Expected(ex);
				return null;
			}
			ArrayList args = null;
			Token paren = Scanner.Next();
			if (paren.Type == TokenType.TokenLeftParen)
			{
				args = Args();
				if (args == null)
					throw new UnexpectedTokenParseException(Scanner.LatestToken);
				paren = Scanner.Next();
				if (paren.Type != TokenType.TokenRightParen)
				{
					throw new ExpectedTokenParseException("right parenthesis", paren);
				}
			}
			else
			{
				Scanner.Pushback(paren);
			}

			// Check to see if we have block argument(s)
			BlockArgumentsPTN blockArgs = BlockArguments();
			return new MethodReferencePTN(t.Value, args, blockArgs);
		}

		/*
			blockArguments :=
				blockLiteral qualifiedBlockArguments
			
		  */
		BlockArgumentsPTN BlockArguments()
		{
			BlockPTN first = BlockLiteral();
			if (first == null)
				return null;	// doesn't look like a match

			// OK, we know we have at least the opening 
			QualifiedBlockArgumentsPTN qualifiedBlockArguments = QualifiedBlockArguments();
			return new BlockArgumentsPTN(first, qualifiedBlockArguments);
		}


		/*
			qualifiedBlockArguments :=
				identifier blockLiteral |
				identifier blockLiteral qualifiedBlockArguments
		*/

		QualifiedBlockArgumentsPTN QualifiedBlockArguments()
		{
			QualifiedBlockArgumentsPTN answer = null;
			Token next;

			while (true)
			{
				next = Scanner.Next();
				if (next.Type != TokenType.TokenIdentifier)
				{
					Scanner.Pushback(next);
					break;	// we're done!
				}

				// OK, we've got an identifier -- there better be a block
				BlockPTN block = BlockLiteral();
				if (block == null)
					throw new ExpectedTokenParseException("block {}", Scanner.LatestToken);

				if (answer == null)
					answer = new QualifiedBlockArgumentsPTN();
				answer.AddQualifiedBlock(new QualifiedBlockPTN(next.Value, block));
			}
			return answer;
		}

		/* 
		args := 
		| 
		arglist
			arglist :=
		arg |
		arg COMMA args
					  arg := expr
		*/

		ArrayList Args()
		{
			string ex = "argument or right parenthesis";
			ArrayList answer = new ArrayList();
			Token next;

			while (true)
			{
				next = Scanner.Next();
				Scanner.Pushback(next);
				if (next.Type == TokenType.TokenRightParen)
				{
					return answer;
				}

				ParseTreeNode arg = Expression();
				if (arg == null)
				{
					Expected(ex);
					return null;
				}

				answer.Add(arg);

				next = Scanner.Next();
				if (next.Type == TokenType.TokenRightParen)
				{
					Scanner.Pushback(next);
					return answer;
				}
				if (next.Type != TokenType.TokenComma)
					throw new ExpectedTokenParseException("comma ','", next);
			}
		}

		/*
	 		literal := 
				STRING | 
				INTEGER | 
				arrayLiteral |
				blockLiteral
		*/
		ExposableParseTreeNode Literal()
		{
			Token next = Scanner.Next();
			if (next.Type == TokenType.TokenString)
				return new StringPTN(next.Value);
			else if (next.Type == TokenType.TokenInteger)
				return new IntegerPTN(next.Value);
			Scanner.Pushback(next);
			ExposableParseTreeNode array = ArrayLiteral();
			if (array != null)
				return array;
			ExposableParseTreeNode block = BlockLiteral();
			if (block != null)
				return block;
			Expected("string, integer, array or block");
			return null;
		}

		ArrayPTN ArrayLiteral()
		{
			Token next = Scanner.Next();
			if (next.Type != TokenType.TokenLeftBracket)
			{
				Scanner.Pushback(next);
				return null;
			}

			string ex = "expression or right bracket";
			ArrayPTN answer = new ArrayPTN();
			while (true)
			{
				next = Scanner.Next();
				if (next.Type == TokenType.TokenRightBracket)
				{
					return answer;
				}
				Scanner.Pushback(next);

				ParseTreeNode arg = Expression();
				if (arg == null)
				{
					Expected(ex);
					return null;
				}

				answer.Add(arg);

				next = Scanner.Next();
				if (next.Type == TokenType.TokenRightBracket)
					return answer;
				if (next.Type != TokenType.TokenComma)
					throw new ExpectedTokenParseException("comma ','", next);
			}
		}


		// blockLiteral := '{'  [blockParameters] expr'}'
		BlockPTN BlockLiteral()
		{
			Token next = Scanner.Next();
			if (next.Type != TokenType.TokenLeftBrace)
			{
				Scanner.Pushback(next);
				return null;
			}

			string ex = "expression, block parameters or right brace";
			BlockPTN answer = new BlockPTN();

			next = Scanner.Next();
			if (next.Type == TokenType.TokenRightBrace)
			{
				return answer;
			}
			Scanner.Pushback(next);

			BlockParametersPTN parms = BlockParameters();
			answer.Parameters = parms;

			ExposableParseTreeNode arg = Expression();
			if (arg == null)
			{
				Expected(ex);
				return null;
			}

			answer.ParseTree = arg;

			next = Scanner.Next();
			if (next.Type == TokenType.TokenRightBrace)
				return answer;
			throw new ExpectedTokenParseException("right brace '}'", next);
		}

		BlockParametersPTN BlockParameters()
		{
			Token identifier = null;
			Token type ;
			BlockParametersPTN answer = new BlockParametersPTN();

			while (true)
			{
				Token next;

				next = Scanner.Next();
				if (next.Type != TokenType.TokenIdentifier)
				{
					if (answer.Parameters.Count == 0)
					{
						Scanner.Pushback(next);
						return null; // not block parms
					}
					else
						throw new ExpectedTokenParseException("type or parameter name", next); // if we've lready got one, we're committed
				}
				type = next;
				next = Scanner.Next();

				if (next.Type == TokenType.TokenBar ||  next.Type == TokenType.TokenComma)
				{
					// oops, it's the case where they omit the type, the token we thought was the type must be the identifier
					answer.AddParameter(new BlockParameterPTN(null, type.Value));
					if (next.Type == TokenType.TokenBar)
						return answer;
					continue;
				} 
				else if (next.Type != TokenType.TokenIdentifier)
				{
					if (answer.Parameters.Count == 0)
					{
						Scanner.Pushback(next);
						Scanner.Pushback(type);
						return null; // not block parms
					}
					else
						throw new ExpectedTokenParseException("parameter name", next);
				}

				identifier = next;

				answer.AddParameter(new BlockParameterPTN(type.Value, identifier.Value));

				// next should be either comma or bar

				next = Scanner.Next();
				if (next.Type == TokenType.TokenBar)
					return answer;
				
				if (next.Type != TokenType.TokenComma)
					throw new ExpectedTokenParseException("| or ,", next);
			}
		}

	}
}
