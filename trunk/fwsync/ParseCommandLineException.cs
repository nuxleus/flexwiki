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
using System.Xml; 
using System.IO; 
using System.Collections;
using System.Collections.Specialized; 

namespace FlexWiki.FwSync
{
	public class ParseCommandLineException : ApplicationException
	{
		#region Fields
		#endregion Fields

		#region Constructors
		public ParseCommandLineException(string message) : this(message, null)
		{
		}

		public ParseCommandLineException(string message, Exception innerException) : 
			base(message, innerException)
		{
		}
		#endregion Constructors

		#region Properties
		#endregion Properties

		#region Methods
		#endregion Methods
	}
}

