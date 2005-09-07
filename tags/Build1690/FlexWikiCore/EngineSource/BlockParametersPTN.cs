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
	/// Summary description for BlockParametersPTN.
	/// </summary>
	public class BlockParametersPTN  : ParseTreeNode
	{
		public BlockParametersPTN() : base()
		{
		}

		public ArrayList _Parameters = new ArrayList();

		public override string ToString()
		{
			return "Block Parameters";
		}

		public void AddParameter(BlockParameterPTN p)
		{
			_Parameters.Add(p);
		}

		public IList Parameters
		{
			get
			{
				return _Parameters;
			}
		}

		public override System.Collections.IEnumerable Children
		{
			get
			{
				return _Parameters;
			}
		}

	}
}

