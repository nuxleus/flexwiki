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
using System.Xml.Serialization; 
using System.IO; 
using System.Collections;
using System.Collections.Specialized; 
using System.Text; 

namespace FlexWiki.FwSyncLib
{
	[XmlRoot("state")]
	public class SynchronizerState
	{
		private LocalNamespaceList namespaces; 
		private string proxyUri; 

		[XmlArray("namespaces")]
		public LocalNamespaceList Namespaces
		{
			get { return namespaces; }
			set { namespaces = value; }
		}

		[XmlAttribute("uri")]
		public string ProxyUri
		{
			get { return proxyUri; }
			set { proxyUri = value; }
		}

	}
}