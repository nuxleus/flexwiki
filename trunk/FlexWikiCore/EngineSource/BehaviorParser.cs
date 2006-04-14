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
		public BehaviorParser(string contextString)
		{
			_ContextString = contextString;
		}

		string _ContextString;
		public string ContextString
		{
			get
			{
				return _ContextString;
			}
		}

		private Scanner _Scanner;

		BELLocation LocationFromToken(Token t)
		{
			return new BELLocation(_ContextString, t.Line, t.Column);
		}

		public ExposableParseTreeNode Parse(string input)
		{
			_Scanner = new Scanner(input);
			ExposableParseTreeNode answer = ExpressionChain();
			if (answer == null)
				throw new ExpectedTokenParseException(LocationFromToken(ButGotToken), ExpectedWhat, ButGotToken);
			if (!Scanner.AtEnd)
			{
				Token x = Scanner.Next();
				throw new UnexpectedTokenParseException(LocationFromToken(x), x);
			}
			return answer;
		}

		private Scanner Scanner
		{
			get
			{
				return _Scanner;
			}
		}

		private void Expected(string what)
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

		/* expressionChain :=
				expr |
				expr SEMICOLON exprChain
		*/

		ExposableParseTreeNode ExpressionChain()		
		{
			ExposableParseTreeNode left = Expression();
			if (left == null)
				return null;	// Whoops!  If it doesn't start with an expression, it's not a chain
			Token next = Scanner.Next();
			if (next.Type != TokenType.TokenSemicolon)
			{
				Scanner.Pushback(next);
				return left;		// Just a single expression
			}
			// Looks like a chain
			ExposableParseTreeNode right = ExpressionChain();
			if (right == null)
				throw new ExpectedTokenParseException(LocationFromToken(next), "expression following ';'", Scanner.LatestToken);
			return new ExpressionChainPTN(LocationFromToken(next), left, right);
		}


		/*
		 	expr := 
			literal |
			literal PERIOD ReferenceChain |
			ReferenceChain
		*/

		private ExposableParseTreeNode Expression()
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
					return new DereferencePTN(LocationFromToken(next), answer, chain);
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

		private ExposableParseTreeNode ReferenceChain()
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

			return new DereferencePTN(LocationFromToken(t), reference, rightSide);
		}


/*
		symbolReference :=
			IDENTIFIER blockArguments |
			IDENTIFIER LEFTPAREN args RIGHTPAREN blockArguments
*/

		private ExposableParseTreeNode SymbolReference()
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
					throw new UnexpectedTokenParseException(LocationFromToken(Scanner.LatestToken), Scanner.LatestToken);
				paren = Scanner.Next();
				if (paren.Type != TokenType.TokenRightParen)
				{
					throw new ExpectedTokenParseException(LocationFromToken(paren), "right parenthesis", paren);
				}
			}
			else
			{
				Scanner.Pushback(paren);
			}

			// Check to see if we have block argument(s)
			BlockArgumentsPTN blockArgs = BlockArguments();
			return new MethodReferencePTN(LocationFromToken(t), t.Value, args, blockArgs);
		}

		/*
			blockArguments :=
				blockLiteral qualifiedBlockArguments
			
		  */
		private BlockArgumentsPTN BlockArguments()
		{
			BlockPTN first = BlockLiteral();
			if (first == null)
				return null;	// doesn't look like a match

			// OK, we know we have at least the opening 
			QualifiedBlockArgumentsPTN qualifiedBlockArguments = QualifiedBlockArguments();
			return new BlockArgumentsPTN(first.Location, first, qualifiedBlockArguments);
		}


		/*
			qualifiedBlockArguments :=
				identifier blockLiteral |
				identifier blockLiteral qualifiedBlockArguments
		*/

		private QualifiedBlockArgumentsPTN QualifiedBlockArguments()
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
					throw new ExpectedTokenParseException(LocationFromToken(next), "block {}", Scanner.LatestToken);

				if (answer == null)
					answer = new QualifiedBlockArgumentsPTN(LocationFromToken(next));
				answer.AddQualifiedBlock(new QualifiedBlockPTN(LocationFromToken(next), next.Value, block));
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

		private ArrayList Args()
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

				ParseTreeNode arg = ExpressionChain();
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
					throw new ExpectedTokenParseException(LocationFromToken(next), "right parenthesis ')' or comma ','", next);
			}
		}

		/*
	 		literal := 
				STRING | 
				INTEGER | 
				arrayLiteral |
				blockLiteral
		*/
		private ExposableParseTreeNode Literal()
		{
			Token next = Scanner.Next();
			if (next.Type == TokenType.TokenString)
				return new StringPTN(LocationFromToken(next), next.Value);
			else if (next.Type == TokenType.TokenInteger)
				return new IntegerPTN(LocationFromToken(next), next.Value);
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

		private ArrayPTN ArrayLiteral()
		{
			Token next = Scanner.Next();
			if (next.Type != TokenType.TokenLeftBracket)
			{
				Scanner.Pushback(next);
				return null;
			}

			string ex = "expression or right bracket";
			ArrayPTN answer = new ArrayPTN(LocationFromToken(next));
			while (true)
			{
				next = Scanner.Next();
				if (next.Type == TokenType.TokenRightBracket)
				{
					return answer;
				}
				Scanner.Pushback(next);

				ParseTreeNode arg = ExpressionChain();
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
					throw new ExpectedTokenParseException(LocationFromToken(next), "right bracket ']' or comma ','", next);
			}
		}


		// blockLiteral := '{'  [blockParameters] expr'}'
		private BlockPTN BlockLiteral()
		{
			Token next = Scanner.Next();
			if (next.Type != TokenType.TokenLeftBrace)
			{
				Scanner.Pushback(next);
				return null;
			}

			string ex = "expression, block parameters or right brace";
			BlockPTN answer = new BlockPTN(LocationFromToken(next));

			next = Scanner.Next();
			if (next.Type == TokenType.TokenRightBrace)
			{
				return answer;
			}
			Scanner.Pushback(next);

			BlockParametersPTN parms = BlockParameters();
			answer.Parameters = parms;

			ExposableParseTreeNode arg = ExpressionChain();
			if (arg == null)
			{
				Expected(ex);
				return null;
			}

			answer.ParseTree = arg;

			next = Scanner.Next();
			if (next.Type == TokenType.TokenRightBrace)
				return answer;
			throw new ExpectedTokenParseException(LocationFromToken(next), "right brace '}'", next);
		}

		private BlockParametersPTN BlockParameters()
		{
			Token identifier = null;
			Token type ;
			BlockParametersPTN answer = null;

			while (true)
			{
				Token next;

				next = Scanner.Next();

				if (answer == null)
					answer = new BlockParametersPTN(LocationFromToken(next));

				if (next.Type != TokenType.TokenIdentifier)
				{
					if (answer.Parameters.Count == 0)
					{
						Scanner.Pushback(next);
						return null; // not block parms
					}
					else
						throw new ExpectedTokenParseException(LocationFromToken(next), "type or parameter name", next); // if we've lready got one, we're committed
				}
				type = next;
				next = Scanner.Next();

				if (next.Type == TokenType.TokenBar ||  next.Type == TokenType.TokenComma)
				{
					// oops, it's the case where they omit the type, the token we thought was the type must be the identifier
					answer.AddParameter(new BlockParameterPTN(LocationFromToken(next), null, type.Value));
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
						throw new ExpectedTokenParseException(LocationFromToken(next), "parameter name", next);
				}

				identifier = next;

				answer.AddParameter(new BlockParameterPTN(LocationFromToken(next), type.Value, identifier.Value));

				// next should be either comma or bar

				next = Scanner.Next();
				if (next.Type == TokenType.TokenBar)
					return answer;
				
				if (next.Type != TokenType.TokenComma)
					throw new ExpectedTokenParseException(LocationFromToken(next), "| or ,", next);
			}
		}

	}
}
