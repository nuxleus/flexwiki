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
using System.Web.Caching;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for InvocationFrame.
	/// </summary>
	public class InvocationFrame
	{
		ArrayList _ExtraArguments;
		/// <summary>
		/// Read-only list of extra arguments beyond those in the signature of the method
		/// </summary>
		public ArrayList ExtraArguments
		{
			get
			{
				if (_ExtraArguments == null)
					return _ExtraArguments = ArrayList.ReadOnly(new ArrayList());
				return _ExtraArguments;
			}
			set
			{
				_ExtraArguments = ArrayList.ReadOnly(value);
			}
		}

		IList _WasParameterSuppliedFlags = null;

		public IList WasParameterSuppliedFlags
		{
			get
			{
				return _WasParameterSuppliedFlags;
			}
			set
			{
				_WasParameterSuppliedFlags = value;
			}
		}

		public bool WasParameterSupplied(int index)
		{
			if (_WasParameterSuppliedFlags == null)
				return false;
			if (index < 0 || index >= _WasParameterSuppliedFlags.Count)
				throw new ArgumentException("Illegal index: " + index);
			return (bool)_WasParameterSuppliedFlags[index];
		}

		ArrayList _Scopes = null;
		
		ArrayList Scopes
		{
			get
			{
				if (_Scopes == null)
					_Scopes = new ArrayList();
				return _Scopes;
			}
		}

		public IScope CurrentScope
		{
			get
			{
				if (Scopes.Count == 0)
					return null;
				return (IScope)(Scopes[Scopes.Count - 1]);
			}
		}

		public void PushScope(IScope src)
		{
			Scopes.Add(src);
		}

		public void PopScope()
		{
			if (Scopes.Count <= 0)
				throw new Exception("Can't pop scope from empty scope stack");
			_Scopes.RemoveAt(Scopes.Count - 1);
		}


	}
}
