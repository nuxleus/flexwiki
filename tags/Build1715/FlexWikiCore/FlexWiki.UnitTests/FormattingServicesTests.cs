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
	[TestFixture] public class FormattingServicesTests : WikiTests
	{
		ContentBase	_base;
		ArrayList _versions;
		LinkMaker _lm;

		[SetUp] public void Init()
		{
			string author = "tester-joebob";
			_lm = new LinkMaker("http://bogusville");
			TheFederation = new Federation(OutputFormat.HTML, _lm);

			_versions = new ArrayList();
			_base = CreateStore("FlexWiki.Base");

			WriteTestTopicAndNewVersion(_base, "TopicOne", @"1
2
3
4
5
6
7
8
9", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"1
2
a
b
c
3
4
5
6
7
8
9", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"1
2
a
b
6
7
8
9", author);

			foreach (TopicChange change in _base.AllChangesForTopic(new LocalTopicName("TopicOne")))
				_versions.Add(change.Version);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
		}

		
		ContentBase ContentBase()
		{
			return _base;
		}

		[Test] public void OldestTest()
		{
			// Test the oldest; should have no markers
			VersionCompare("TopicOne", (string)_versions[_versions.Count - 1], @"<p>1</p>
<p>2</p>
<p>3</p>
<p>4</p>
<p>5</p>
<p>6</p>
<p>7</p>
<p>8</p>
<p>9</p>
");
		}

		[Test] public void InsertTest()
		{
			// Inserts oldest should have the 
			VersionCompare("TopicOne", (string)_versions[_versions.Count - 2], @"<p>1</p>
<p>2</p>
<p style=""background: palegreen"">a</p>
<p style=""background: palegreen"">b</p>
<p style=""background: palegreen"">c</p>
<p>3</p>
<p>4</p>
<p>5</p>
<p>6</p>
<p>7</p>
<p>8</p>
<p>9</p>
");
		}

		[Test] public void DeleteTest()
		{
			VersionCompare("TopicOne", (string)_versions[_versions.Count - 3], @"<p>1</p>
<p>2</p>
<p>a</p>
<p>b</p>
<p style=""color: silver; text-decoration: line-through"">c</p>
<p style=""color: silver; text-decoration: line-through"">3</p>
<p style=""color: silver; text-decoration: line-through"">4</p>
<p style=""color: silver; text-decoration: line-through"">5</p>
<p>6</p>
<p>7</p>
<p>8</p>
<p>9</p>
");
		}

		void VersionCompare(string topic, string version, string expecting)
		{
			AbsoluteTopicName abs = ContentBase().TopicNameFor(topic);
			abs.Version = version;
			AbsoluteTopicName oldTopic = ContentBase().VersionPreviousTo(abs.LocalName); 


			string got = Formatter.FormattedTopic(abs, OutputFormat.Testing, oldTopic, TheFederation, _lm, null);
			got = got.Replace("\r", "");
			string o2 = expecting.Replace("\r", "");

			if (got != o2)
			{
				Console.Error.WriteLine("Got     : " + got);
				Console.Error.WriteLine("Expected: " + o2);
			}
			Assert.AreEqual(o2, got);
		}
	}
}
