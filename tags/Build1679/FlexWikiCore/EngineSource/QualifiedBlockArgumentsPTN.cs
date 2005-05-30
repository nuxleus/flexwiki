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
	/// Summary description for QualifiedBlockArgumentsPTN.
	/// </summary>
	public class QualifiedBlockArgumentsPTN  : ParseTreeNode
	{
		public QualifiedBlockArgumentsPTN() : base()
		{
		}

		public ArrayList _QualifiedArguments = new ArrayList();

		public override string ToString()
		{
			return "Qualified Block Arguments";
		}

		public void AddQualifiedBlock(QualifiedBlockPTN qb)
		{
			_QualifiedArguments.Add(qb);
		}

		public IList QualifiedBlocks
		{
			get
			{
				return _QualifiedArguments;
			}
		}

		public override System.Collections.IEnumerable Children
		{
			get
			{
				return _QualifiedArguments;
			}
		}

	}
}
