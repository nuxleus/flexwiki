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
	public class WithScope : IScope
	{
		public WithScope(IScope ContainingScope)
		{
			_ContainingScope = ContainingScope;
		}

		IScope _ContainingScope;
		public IScope ContainingScope
		{
			get
			{
				return _ContainingScope;
			}
		}

		ArrayList _With = new ArrayList();

		public IBELObject ValueOf(string symbol, ArrayList args, ExecutionContext ctx)
		{
			IBELObject answer;
			foreach (IValueSource each in _With)
			{
				answer = each.ValueOf(symbol, args,ctx);
				if (answer != null)
					return answer;
			}
			return null;
		}

		public void AddObject(IValueSource val)
		{
			_With.Add(val);
		}

		
	}
}
