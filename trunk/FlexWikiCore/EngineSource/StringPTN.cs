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
        private string _Value; 

		public StringPTN(BELLocation loc, string val) : base(loc)
		{
			_Value = val;
		}

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
			try
			{
				ctx.PushLocation(Location);
				return new BELString(Value);
			}
			finally
			{
				ctx.PopLocation();
			}

		}

		public override string ToString()
		{
			return "String: '" + _Value + "'";
		}
	}
}
