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
	/// Summary description for BlockParameter.
	/// </summary>
	[ExposedClass("BlockParameter", "Describes a parameter for a Block")]
	public class BlockParameter : BELObject
	{
		public BlockParameter(string type, string identifier)
		{
			TypeName = type;
			Identifier = identifier;
		}

		public string _TypeName;

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the name of the type of the parameter")]
		public string TypeName
		{
			get
			{
				return _TypeName;
			}
			set
			{
				_TypeName = value;
			}
		}

		public string _Identifier;

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the name of the parameter")]
		public string Identifier
		{
			get
			{
				return _Identifier;
			}
			set
			{
				_Identifier = value;
			}
		}

	}
}
