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
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class DeprecatedNamespaceDefinition
	{
		public DeprecatedNamespaceDefinition()
		{
		}

		private string _Root;
		[XmlAttribute]
		public string Root
		{
			get
			{
				return _Root;
			}
			set
			{
				_Root = value;
			}
		}

		private string _Namespace;
		[XmlAttribute]
		public string Namespace
		{
			get
			{
				return _Namespace;
			}
			set
			{
				_Namespace = value;
			}
		}

		
	}
}
