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
