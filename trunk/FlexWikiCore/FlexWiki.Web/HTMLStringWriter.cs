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
using System.Text;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for HtmlStringWriter.
	/// </summary>
	public class HtmlStringWriter : HtmlWriter
	{
		StringBuilder _Builder = new StringBuilder();

		public HtmlStringWriter()
		{
		}

		public override void Write(string s)
		{
			_Builder.Append(s);
		}

		public override string ToString()
		{
			return _Builder.ToString();
		}

	}
}
