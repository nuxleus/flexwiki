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

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class NamespaceToRoot
	{
		/// <summary>
		/// Answer a mapping object between the given namespace and root
		/// </summary>
		/// <param name="ns">Namepsace</param>
		/// <param name="root">Path to root for the content base</param>
		public NamespaceToRoot(string ns, string root)
		{
			_Namespace = ns;
			_Root = root;
		}

		public NamespaceToRoot()
		{
		}

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
		string _Root;

		[XmlAttribute]
		public bool Secure
		{
			get
			{
				return _Secure;
			}
			set
			{
				_Secure = value;
			}
		}
		bool _Secure;

		public string AbsoluteRoot(string relativeDirectoryBase)
		{
			// See if we're dealing with a relative 
			if (!Root.StartsWith(".\\"))
				return Root;
			if (relativeDirectoryBase == null)
				throw new Exception("Relative directory path specified for namespace without relative directory supplied: " + Root);
			return relativeDirectoryBase + "\\" + Root.Substring(2);
		}


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
		string _Namespace;

	}
}
