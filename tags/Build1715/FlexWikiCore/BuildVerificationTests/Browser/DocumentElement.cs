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
	/// Summary description for DocumentElement.
	/// </summary>
	public class DocumentElement
	{
		IHTMLDocument3 _Element = null;

		private IHTMLDocument3 TypedElement
		{
			get
			{
				return _Element;
			}
		}

		private IHTMLDocument2 TypedElement2
		{
			get
			{
				return (IHTMLDocument2)_Element;
			}
		}

		public DocumentElement(IHTMLDocument3 d, Browser b)
		{
			_Element = d;
			_Browser = b;
		}

		public string Title
		{
			get
			{
				if (TypedElement == null)
					return null;
				return TypedElement2.title;
			}
		}

		public Uri URI
		{
			get
			{
				if (TypedElement == null)
					return null;
				return new Uri(TypedElement.baseUrl);
			}
		}

		protected IHTMLElement GetHTMLElementByName(string name)
		{
			if (TypedElement == null)
				return null;
			IHTMLElementCollection collection = TypedElement.getElementsByName(name) as IHTMLElementCollection;
			if (collection == null || collection.length == 0)
				return null;
			IHTMLElement element = collection.item(0, 0) as IHTMLElement;
			if (element == null)
				return null;
			return element;
		}

		public HTMLElement Body
		{
			get
			{
				return new HTMLElement(TypedElement2.body, this);
			}
		}

		/// <summary>
		/// Answer the given element found by name (or null if none)
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual HTMLElement GetElementByName(string name)
		{
			IHTMLElement e = GetHTMLElementByName(name);
			if (e == null)
				return null;
			return NewTypedElement(e);
		}

		protected IHTMLElement GetHTMLElementByID(string id)
		{
			if (TypedElement == null)
				return null;
			IHTMLElement element = TypedElement.getElementById(id) as IHTMLElement;
			if (element == null)
				return null;
			return element;
		}

		/// <summary>
		/// Answer the given element found by name (or null if none)
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual HTMLElement GetElementByID(string name)
		{
			IHTMLElement e = GetHTMLElementByID(name);
			if (e == null)
				return null;
			return NewTypedElement(e);
		}

		public HTMLElement NewTypedElement(IHTMLElement element)
		{
			if (element is IHTMLInputElement)
				return new InputElement(element as IHTMLInputElement, this);
			if (element is IHTMLButtonElement)
				return new ButtonElement(element as IHTMLButtonElement, this);
			if (element is IHTMLAnchorElement)
				return new HyperlinkElement(element as IHTMLAnchorElement, this);
			if (element is IHTMLOptionElement)
				return new OptionElement(element as IHTMLOptionElement, this);
			if (element is IHTMLSelectElement)
				return new SelectElement(element as IHTMLSelectElement, this);
			
			// fallback
			return new HTMLElement(element, this);
		}

		public void WaitForNavigationComplete()
		{
			_Browser.WaitForNavigationComplete();	
		}

		Browser _Browser = null;

	}
}
