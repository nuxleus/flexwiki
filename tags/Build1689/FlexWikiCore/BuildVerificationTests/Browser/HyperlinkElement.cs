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
using SHDocVw;
using mshtml;


namespace FlexWiki.BuildVerificationTests
{
	/// <summary>
	/// Summary description for HyperlinkElement.
	/// </summary>
	public class HyperlinkElement : HTMLElement
	{
		private IHTMLAnchorElement TypedElement
		{
			get
			{
				return (IHTMLAnchorElement)Element;
			}
		}

		public HyperlinkElement(IHTMLAnchorElement e, DocumentElement d) : base(e as IHTMLElement, d)
		{
		}

		public void Click(bool wait)
		{
			Element.click();
			if (wait)
				WaitForNavigationComplete();
		}


	}
}
