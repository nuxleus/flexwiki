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
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;

namespace FlexWiki
{
	[XmlRoot("flexWiki")]
	public class FlexWikiConfigurationSectionHandler : System.Configuration.IConfigurationSectionHandler
	{
		private StringCollection plugins = new StringCollection();

		public FlexWikiConfigurationSectionHandler()
		{}

		public static FlexWikiConfigurationSectionHandler GetConfig()
		{
			return ConfigurationSettings.GetConfig("flexWiki") 
				as FlexWikiConfigurationSectionHandler;
		}

		Object IConfigurationSectionHandler.Create(
			object parent,
			object configContext,
			XmlNode section
			)
		{
			XmlSerializer ser = new XmlSerializer (typeof(FlexWikiConfigurationSectionHandler)); 
			return ser.Deserialize (new XmlNodeReader(section)); 
		}

		[XmlArray("plugins")]
		[XmlArrayItem("plugin",typeof(string))]
		public StringCollection Plugins
		{
			get
			{
				return this.plugins;
			}
		}
	}
}
