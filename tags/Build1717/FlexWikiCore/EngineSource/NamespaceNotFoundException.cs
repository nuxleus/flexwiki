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
	/// Summary description for NamespaceNotFoundException.
	/// </summary>
	public class NamespaceNotFoundException : ApplicationException
	{
		public NamespaceNotFoundException() : base()
		{
		}
		public NamespaceNotFoundException(string message) : base(message)
		{
		}

		string _Namespace;

		public string Namespace
		{
			get
			{
				return _Namespace;
			}
			set
			{
				_Namespace = value;
			}
		}

		public static NamespaceNotFoundException ForNamespace(string ns)
		{
			NamespaceNotFoundException answer = new NamespaceNotFoundException("Namespace not found: " + ns);
			answer.Namespace = ns;
			return answer;
		}
		
	}
}
