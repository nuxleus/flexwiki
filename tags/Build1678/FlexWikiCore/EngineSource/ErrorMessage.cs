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
	/// Summary description for ErrorMessage.
	/// </summary>
	[ExposedClass("ErrorMessage", "A presentation of an error message")]	
	public class ErrorMessage : PresentationPrimitive
	{
		public ErrorMessage(string title, string body)
		{
			_Title = title;
			_Body = body;
		}

		public override void OutputTo(WikiOutput output)
		{
			output.WriteErrorMessage(Title, Body);
		}

		public string Title
		{
			get
			{
				return _Title;
			}
		}

		public string Body
		{
			get
			{
				return _Body;
			}
		}

		string _Title;
		string _Body;

	}
}
