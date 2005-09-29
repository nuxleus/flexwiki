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
	public class BlockScope : IScope
	{
		public BlockScope(IScope ContainingScope)
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

		Hashtable _Table = new Hashtable();

		public IBELObject ValueOf(string symbol, ArrayList args, ExecutionContext ctx)
		{
			IBELObject answer = (IBELObject)_Table[symbol];
			if (answer == null)
				return null;		// not found
			if (args != null && args.Count > 0)
				throw new ExecutionException(symbol + " is a temporary variable, not a function; no arguments allowed");
			return answer;
		}

		public void AddParameter(string symbol, IBELObject val)
		{
			_Table[symbol] = val;
		}

		
	}
}
