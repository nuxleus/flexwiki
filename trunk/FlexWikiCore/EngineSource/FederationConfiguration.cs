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
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.IO;

namespace FlexWiki
{
	/// <summary>
	/// A FederationConfiguration is a persistable representation of all the configuration information needed for a Federation
	/// </summary>
	public class FederationConfiguration
	{
		/// <summary>
		/// Create a configuration object that represents the configuration stored in the given file
		/// </summary>
		/// <param name="path">Path to the XML configuration file</param>
		public static FederationConfiguration FromFile(string path)
		{
			string lines = "";
			using (TextReader sr = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				lines = sr.ReadToEnd(); 
			}
			FederationConfiguration answer = FederationConfiguration.FromXMLString(lines);
			answer.FederationNamespaceMapFilename = path;
			answer.FederationNamespaceMapLastRead = File.GetLastWriteTime(path);
			return answer;
		}

		public static FederationConfiguration FromXMLString(string xml)
		{			
			XmlSerializer serializer = new XmlSerializer(typeof(FederationConfiguration));
			StringReader reader = new StringReader(xml); 
			FederationConfiguration answer = (FederationConfiguration)serializer.Deserialize(reader);
			return answer;
		}

		public FederationConfiguration()
		{ 
		}

		public void WriteToFile(string path)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(FederationConfiguration));
			TextWriter writer = new StreamWriter(path);
			serializer.Serialize(writer, this);
			writer.Close();
		}

		[XmlIgnore]
		public DateTime FederationNamespaceMapLastRead = DateTime.MinValue;

		static Regex namespaceDefinitionRegex = new Regex("^([a-zA-Z0-9\\.]+)=(.*)$");

		public string DefaultNamespace;

		[XmlElement(ElementName = "About")]
		public string AboutWikiString;

		[XmlElement(ElementName = "Borders")]
		public string Borders;

		[XmlElement(ElementName = "WikiTalkVersion")]
		public int WikiTalkVersion;

		[XmlElement(ElementName = "DefaultDirectoryForNewNamespaces")]
		public string DefaultDirectoryForNewNamespaces;

		ArrayList _Mappings = new ArrayList();

		[XmlArray(ElementName = "Namespaces"), 
		XmlArrayItem(ElementName= "Namespace", 
			Type = typeof(NamespaceToRoot))
		]
		public ArrayList NamespaceMappings
		{
			get
			{
				return _Mappings;
			}
			set
			{
				_Mappings = value;
			}
		}

		[XmlIgnore]
		public string FederationNamespaceMapFilename;

	}
}
