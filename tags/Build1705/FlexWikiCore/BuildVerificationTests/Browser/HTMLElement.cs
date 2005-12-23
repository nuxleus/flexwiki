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
	/// Summary description for DocumentElement.
	/// </summary>
	public class HTMLElement
	{
		protected IHTMLElement Element
		{
			get
			{
				return _Element;
			}
		}

		IHTMLElement _Element;

		public HTMLElement(IHTMLElement e, DocumentElement doc)
		{
			Initialize(e, doc);
		}

		protected HTMLElement()
		{
			
		}

		protected virtual void WaitForNavigationComplete()
		{
			Document.WaitForNavigationComplete();	
		}

		protected void Initialize(IHTMLElement e, DocumentElement doc)
		{
			_Document = doc;
			_Element = e;
		}

		DocumentElement _Document = null;

		protected DocumentElement Document
		{
			get
			{
				return _Document;
			}
		}

		public string OuterText
		{
			get
			{
				return Element.outerText;
			}
		}

		public string OuterHTML
		{
			get
			{
				return Element.outerHTML;
			}
		}


		public string InnerText
		{
			get
			{
				return Element.innerText;
			}
		}

		public string InnerHTML
		{
			get
			{
				return Element.innerHTML;
			}
		}


	}
}