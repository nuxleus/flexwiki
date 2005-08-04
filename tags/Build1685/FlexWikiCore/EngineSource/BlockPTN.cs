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
	/// Summary description for BlockPTN.
	/// </summary>
	public class BlockPTN : ExposableParseTreeNode
	{
		public BlockPTN() : base()
		{
		}

		public ExposableParseTreeNode ParseTree;
		public BlockParametersPTN Parameters; 

		public override string ToString()
		{
			return "Block";
		}

		public override System.Collections.IEnumerable Children
		{
			get
			{
				ArrayList answer = new ArrayList();
				if (Parameters != null)
					answer.Add(Parameters);
				if (ParseTree != null)
					answer.Add(ParseTree);
				return answer;
			}
		}

		public override IBELObject Expose(ExecutionContext ctx)
		{
			ArrayList parameters = null;

			if (Parameters != null)
			{
				parameters = new ArrayList();
				foreach (BlockParameterPTN p in Parameters.Parameters)
				{
					parameters.Add(new BlockParameter(p.Type, p.Identifier));
				}
			}
			return new Block(ParseTree, parameters, ctx.CurrentScope);
		}
	}
}
