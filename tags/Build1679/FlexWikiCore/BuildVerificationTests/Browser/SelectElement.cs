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
	/// Summary description for SelectElement.
	/// </summary>
	public class SelectElement : HTMLElement
	{
		private IHTMLSelectElement TypedElement
		{
			get
			{
				return (IHTMLSelectElement)Element;
			}
		}

		public SelectElement(IHTMLSelectElement e, DocumentElement d) : base(e as IHTMLElement, d)
		{
		}

		public void Select(string label)
		{
			int index = Options.IndexOf(label);
			if (index >= 0)
				TypedElement.selectedIndex = index;
		}

		public IList Options
		{
			get
			{
				ArrayList answer = new ArrayList();
				for (int i = 0; i < TypedElement.length; i++)
				{
					answer.Add(Document.NewTypedElement((IHTMLElement) TypedElement.item(i, i)));
				}
					
				return ArrayList.ReadOnly(answer);
			}
		}


	}
}
