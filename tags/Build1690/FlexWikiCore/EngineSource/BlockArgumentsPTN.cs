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
	/// Summary description for BlockArgumentsPTN.
	/// </summary>
	public class BlockArgumentsPTN : ParseTreeNode
	{
		public BlockArgumentsPTN(BlockPTN block, QualifiedBlockArgumentsPTN qualifiedBlocks) : base()
		{
			Block = block;
			QualifiedBlocks = qualifiedBlocks;
		}

		public BlockPTN Block;
		public QualifiedBlockArgumentsPTN QualifiedBlocks;

		public override string ToString()
		{
			return "Block Arguments";
		}

		public override System.Collections.IEnumerable Children
		{
			get
			{
				ArrayList answer = new ArrayList();
				answer.Add(Block);
				if (QualifiedBlocks != null)
					answer.Add(QualifiedBlocks);
				return answer;
			}
		}

	}
}
