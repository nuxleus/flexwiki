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

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class StringPTN : ExposableParseTreeNode
	{
		public StringPTN(string val)
		{
			_Value = val;
		}

		string _Value;
		public string Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
			}
		}

		public override IBELObject Expose(ExecutionContext ctx)
		{
			return new BELString(Value);
		}

		public override string ToString()
		{
			return "String: '" + _Value + "'";
		}
	}
}
