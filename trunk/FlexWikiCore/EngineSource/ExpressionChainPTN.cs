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
	/// 
	/// </summary>
	public class ExpressionChainPTN : ExposableParseTreeNode
	{
		public ExpressionChainPTN(BELLocation loc, ExposableParseTreeNode left, ExposableParseTreeNode right)  : base(loc)
		{
			_Left = left;
			_Right = right;
		}

		ExposableParseTreeNode _Left;
		ExposableParseTreeNode _Right;

		public ExposableParseTreeNode Left
		{
			get
			{
				return _Left;
			}
		}

		public ExposableParseTreeNode Right
		{
			get
			{
				return _Right;
			}
		}

		public override string ToString()
		{
			return "Expression Chain";
		}

		public override System.Collections.IEnumerable Children
		{
			get
			{
				ArrayList answer = new ArrayList();
				answer.Add(Left);
				answer.Add(Right);
				return answer;
			}
		}

		public override IBELObject Expose(ExecutionContext ctx)
		{
			try
			{
				ctx.PushLocation(Left.Location);
				Left.Expose(ctx);
			}
			finally
			{
				ctx.PopLocation();
			}

			try
			{
				ctx.PushLocation(Right.Location);
				return Right.Expose(ctx);
			}
			finally
			{
				ctx.PopLocation();
			}

		}

	}
}
