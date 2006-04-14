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
using System.Xml.Xsl; 
using System.Xml.XPath; 
using System.IO;
using System.ComponentModel; 
using System.Reflection; 

namespace wikidpad2flexwiki
{
	class App
	{
		static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: wikidpad2flexwiki input.xml outputdir"); 
				return 1; 
			}

			string input = args[0];
			string dir = args[1];

			if (Directory.Exists(dir))
			{
				Console.WriteLine("Output must be to a directory that does not exist - it will be created"); 
				return 2; 
			}

			Directory.CreateDirectory(dir); 

			dir = Path.GetFullPath(dir); 

			Stream xslt = Assembly.GetExecutingAssembly().GetManifestResourceStream("wikidpad2flexwiki.wikidpad2flexwiki.xslt"); 
			XmlReader xsltxml = new XmlTextReader(xslt); 
			XslTransform txfm = new XslTransform(); 

			txfm.Load(xsltxml, null, null); 

			XmlDocument wikid = new XmlDocument(); 
			wikid.Load(input); 

			XPathNavigator nav = wikid.CreateNavigator(); 
			XPathNodeIterator iter = nav.Select("/wiki/wikiword"); 

			XmlNodeList wikiwords = wikid.SelectNodes("/wiki/wikiword"); 

			Console.WriteLine(); 
			foreach (XmlNode wikiword in wikiwords)
			{
				CreateWikiFile(dir, wikiword, txfm); 
				Console.Write("."); 
			}
			Console.WriteLine(); 

			return 0; 
		}

		static void CreateWikiFile(string dir, XmlNode wikiword, XslTransform txfm)
		{
			string name = wikiword.SelectSingleNode("@name").Value; 
			string path = Path.Combine(dir, name + ".wiki"); 

			XmlDocument doc = new XmlDocument(); 
			doc.LoadXml(wikiword.OuterXml); 

			// Transform it into memory first so we can strip off the stupid BOM
			MemoryStream ms = new MemoryStream(); 

			txfm.Transform(doc.CreateNavigator(), null, ms, null); 

			// Rewind to just after the BOM
			ms.Position = 1; 

			FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write); 

			StreamReader sr = new StreamReader(ms); 
			StreamWriter sw = new StreamWriter(fs); 

			sw.Write(sr.ReadToEnd()); 

			sw.Close(); 
			sr.Close(); 
			
		}
	}
}
