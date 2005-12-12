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

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class ParseException : System.ApplicationException
	{
		public ParseException(BELLocation loc) : base(loc.ToString())
		{
			Location = loc;
		}

		public ParseException(BELLocation loc, string message) : base(loc.ToString() + " : " + message)
		{
			Location = loc;
		}

		public ParseException(BELLocation loc, string message, Token token) : base(loc.ToString() + " : " + message + (token == null ? "" : ": " + token.ToString()))
		{
			Location = loc;
		}

		BELLocation Location;
	}
}
