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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using FlexWiki.Formatting;
using System.Diagnostics;
using System.Xml;
using System.Xml.Xsl;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("Utility", "Easy access to general utility functions")]
	public class Utility : BELObject
	{
		public Utility() : base()
		{
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

		public override string ToString()
		{
			return "global utility object";
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Present an error string")]
		public IBELObject ErrorMessage(string title, string body)
		{
			return new ErrorMessage(title, body);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer a newline")]
		public string Newline
		{
			get
			{
				return Environment.NewLine;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer a tab")]
		public string Tab
		{
			get
			{
				return "\t";
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer a space")]
		public string Space
		{
			get
			{
				return " ";
			}
		}
	}
}
				
		
