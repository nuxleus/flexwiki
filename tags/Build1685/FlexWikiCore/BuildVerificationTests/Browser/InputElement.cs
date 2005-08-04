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
using SHDocVw;
using mshtml;


namespace FlexWiki.BuildVerificationTests
{
	/// <summary>
	/// Summary description for InputElement.
	/// </summary>
	public class InputElement : HTMLElement
	{
		private IHTMLInputElement TypedElement
		{
			get
			{
				return (IHTMLInputElement)Element;
			}
		}

		public InputElement(IHTMLInputElement e, DocumentElement d) : base(e as IHTMLElement, d)
		{
		}

		/// <summary>
		/// Get or set the value of the input field
		/// </summary>
		public string Value
		{
			get
			{
				return TypedElement.value;
			}
			set
			{
				TypedElement.value = value;
			}
		}


	}
}
