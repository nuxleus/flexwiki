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
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Reflection; 

using FlexWiki.Formatting;
using FlexWiki;

using NUnit.Framework;


namespace FlexWiki.UnitTests
{
	[TestFixture] public class NameMatches
	{
		// Tests to make sure the wikiname extraction work correctly
		[Test] public void TestNames()
		{
			ExtractName("HelloThere029.1BigDog", "HelloThere029");
			ExtractName("HelloThere.BigDog", "HelloThere.BigDog");
			ExtractName("Hey.BigDog", "Hey.BigDog");
			ExtractName(".BigDog", ".BigDog");
			ExtractName("This.Long.NameHere.BigDog", "This.Long.NameHere.BigDog");
			ExtractName("Hello.Big", null);
			ExtractName("HelloT22here.BigD11og", "HelloT22here.BigD11og");
			ExtractName("Some VerySimpleLinksFatPig", "VerySimpleLinksFatPig");	
		}

		[Test] public void TestBracketNames()
		{
			ExtractName(".[name]", ".[name]");
			ExtractName("[name]", "[name]");
			ExtractName("Name.[name]", "Name.[name]");
			ExtractName("Name.[Hot]", "Name.[Hot]");
			ExtractName("Name.[HotDog]", "Name.[HotDog]");
		}

		void ExtractName(string input, string match)
		{
			Regex m = new Regex(Formatter.extractWikiNamesString);
			if (match != null)
			{
				Assert.AreEqual(1, m.Matches(input).Count, match);
				Assert.AreEqual(match, m.Matches(input)[0].Groups["topic"].Value, match);
			}
			else
			{
				Assert.AreEqual(0, m.Matches(input).Count, match);
			}
		}


	}

}
