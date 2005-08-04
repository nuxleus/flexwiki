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
	public class ArrayPTN : ExposableParseTreeNode
	{
		public ArrayPTN()
		{
		}

		public void Add(ParseTreeNode arg)
		{
			_Array.Add(arg);
		}

		ArrayList _Array = new ArrayList();

		public override string ToString()
		{
			return "Array";
		}

		public override System.Collections.IEnumerable Children
		{
			get
			{
				return _Array;
			}
		}

		public override IBELObject Expose(ExecutionContext ctx)
		{
			BELArray a = new BELArray();
			foreach (ExposableParseTreeNode each in _Array)
				a.Add(each.Expose(ctx));
			return a;
		}

	}
}
