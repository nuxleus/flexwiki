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
using System.Threading;
using FlexWiki.BuildVerificationTests;
using SHDocVw;
using mshtml;
using FlexWiki;

namespace FlexWiki.BuildVerificationTests
{
	/// <summary>
	/// Summary description for Browser.
	/// </summary>
	public class Browser
	{
		public Browser()
		{
		}

		InternetExplorer _IE;
		InternetExplorer IE
		{
			get
			{
				if (_IE != null)
					return _IE;
				_IE = new InternetExplorerClass();
				_IE.ProgressChange += new DWebBrowserEvents2_ProgressChangeEventHandler(ie_ProgressChange);
				_IE.DocumentComplete +=new DWebBrowserEvents2_DocumentCompleteEventHandler(ie_DocumentComplete);		
				return _IE;
			}
		}

		// Use the "missing" value.
		static object Missing = System.Reflection.Missing.Value;

		
		int _Timeout = 30000;	// 30 seconds
		/// <summary>
		/// Set or get the timeout for navigation operations (in milliseconds)
		/// </summary>
		public int Timeout
		{
			get
			{
				return _Timeout;
			}
			set
			{
				_Timeout = value;
			}
		}

		private AutoResetEvent NavigationComplete
		{
			get
			{
				return _NavigationComplete;
			}
		}

		AutoResetEvent _NavigationComplete = new AutoResetEvent(false);

		/// <summary>
		/// Navigate to a given URL; throw if timeout, return when navigation complete
		/// </summary>
		/// <param name="URL"></param>
		/// <param name="wait">true to wait for completion of navigate</param>
		/// <returns>Document if wait request AND navigation succeeded</returns>
		public DocumentElement Navigate(string URL, bool wait)
		{
			_Document = null;
			IE.Navigate(URL, ref Missing, ref Missing, ref Missing, ref Missing);
			if (wait)
			{
				WaitForNavigationComplete();
				return Document;
			}
			return null;
		}

		public void WaitForNavigationComplete()
		{
			if (!_NavigationComplete.WaitOne(Timeout, false))
				throw new Exception("Browser navigate timeout: " + ((HTMLDocument)(IE.Document)).url);
			_Document = new DocumentElement((IHTMLDocument3)(IE.Document), this);
		}

		DocumentElement _Document = null;

		public DocumentElement Document
		{
			get
			{
				return _Document;
			}
		}


//		public HyperlinkElement GetHyperlinkFromText(string text)
//		{
//			foreach (IHTMLAnchorElement each in Document.anchors)
//			{
//				if ((each as IHTMLElement).innerText == text)
//					return (HyperlinkElement) HTMLElement.For(each as IHTMLElement, this);
//			}
//			return null;
//		}
		
		private void ie_ProgressChange(int Progress, int ProgressMax)
		{
//			Console.Out.WriteLine("" + Progress + " of " + ProgressMax);
		}

		private void ie_DocumentComplete(object pDisp, ref object URL)
		{
			_NavigationComplete.Set();
		}

	}
}
