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
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("FormStartPresentation", "Presents the start of a form")]
	public class FormStartPresentation : FlexWiki.PresentationPrimitive
	{
		public FormStartPresentation(string URI, string method)
		{
			_Method = method;
			_URI = URI;
		}

		public string _Method;
		public string Method
		{
			get
			{
				return _Method;
			}
		}

		public string _URI;
		public string URI
		{
			get
			{
				return _URI;
			}
		}

		public override void OutputTo(WikiOutput output)
		{
			output.FormStart(Method, URI);
		}


	}
}
