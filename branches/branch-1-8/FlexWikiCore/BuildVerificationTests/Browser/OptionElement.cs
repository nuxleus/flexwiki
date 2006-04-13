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

using System.Collections;
using mshtml;

namespace FlexWiki.BuildVerificationTests
{
	/// <summary>
	/// Summary description for OptionElement.
	/// </summary>
	public class OptionElement : HTMLElement
	{
		private IHTMLOptionElement TypedElement
		{
			get
			{
				return (IHTMLOptionElement)Element;
			}
		}

		public OptionElement(IHTMLOptionElement e, DocumentElement d) : base(e as IHTMLElement, d)
		{
		}

		public string Text
		{
			get
			{
				return TypedElement.text;
			}
		}

		public string Value
		{
			get
			{
				return TypedElement.value;
			}
		}

		public bool IsSelected
		{
			get
			{
				return TypedElement.selected;
			}
		}

	}
}
