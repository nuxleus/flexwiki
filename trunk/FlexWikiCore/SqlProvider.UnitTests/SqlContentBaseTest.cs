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
using System.Collections;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using FlexWiki;
using NUnit.Framework;
using FlexWiki.UnitTests;

namespace SqlProvider.UnitTests
{
	[TestFixture] public class SqlContentBaseTests: ContentBaseTests
	{
		public SqlContentBaseTests()
		{
			base.storeType = "sql";	
		}

		[Test] public override void TestSerialization()
		{
			MemoryStream ms = new MemoryStream(); 
			XmlWriter wtr = new XmlTextWriter(ms, System.Text.Encoding.UTF8); 
			XmlSerializer ser = new XmlSerializer(typeof(SqlStore)); 
			ser.Serialize(wtr, _base); 

			wtr.Close(); 

			// If we got this far, there was no exception. More rigorous 
			// testing would assert XPath expressions against the XML 

		} 

	}

	[TestFixture] public class MoreSqlContentBaseTests: MoreContentBaseTests
	{
		public MoreSqlContentBaseTests()
		{
			base.storeType = "sql";	
		}
	}
}
