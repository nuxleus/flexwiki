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
	public class DereferencePTN : ExposableParseTreeNode
	{
		ExposableParseTreeNode Left;
		ParseTreeNode Right;

		public DereferencePTN(ExposableParseTreeNode left, ParseTreeNode right)
		{
			Left = left;
			Right = right;
		}

		public override IEnumerable Children
		{
			get
			{
				ArrayList answer = new ArrayList();
				answer.Add(Left);
				answer.Add(Right);
				return answer;
			}
		}

		public override string ToString()
		{
			return "Dereference";
		}

		
		public override IBELObject Expose(ExecutionContext ctx)
		{
			// The object on the left is evaluated relative to the global context and temps
			IBELObject leftObject = Left.Expose(ctx);
			// The object on the right is either a MethodReferencePTN or another DereferencePTN
			if (Right is MethodReferencePTN)
				return ((MethodReferencePTN)Right).EvaluateWithReceiver(ctx, leftObject);
			else if (Right is DereferencePTN)
				return ((DereferencePTN)Right).EvaluateWithReceiver(ctx, leftObject);
			throw new Exception("Corrupt parse tree -- DereferencePTN with right side that's neither a MethodReferencePTN nor a DereferencePTN");
		}

		public IBELObject EvaluateWithReceiver(ExecutionContext ctx, IBELObject receiver)
		{
			// The object on the left is evaluated aginst the receiver
			MethodReferencePTN leftRef = (MethodReferencePTN)Left;	
			IBELObject newReceiver = leftRef.EvaluateWithReceiver(ctx, receiver);
			// The object on the right is either a MethodReferencePTN or another DereferencePTN
			if (Right is MethodReferencePTN)
				return ((MethodReferencePTN)Right).EvaluateWithReceiver(ctx, newReceiver);
			else if (Right is DereferencePTN)
				return ((DereferencePTN)Right).EvaluateWithReceiver(ctx, newReceiver);
			throw new Exception("Corrupt parse tree -- DereferencePTN with right side that's neither a MethodReferencePTN nor a DereferencePTN");
		}

	}

}
