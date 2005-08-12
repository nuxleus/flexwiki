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
	public class NamespaceScope : IScope
	{
		public NamespaceScope(IScope scope, DynamicNamespace ns)
		{
			_ContainingScope = scope;
			_Namespace = ns;
		}

		IScope _ContainingScope;
		public IScope ContainingScope
		{
			get
			{
				return _ContainingScope;
			}
		}

		DynamicNamespace _Namespace;
		public DynamicNamespace Namespace 
		{
			get
			{
				return _Namespace;
			}
		}

		public IBELObject ValueOf(string symbol, ArrayList args, ExecutionContext ctx)
		{
			return Namespace.ValueOf(symbol, args, ctx);
		}
		
	}
}
