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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using FlexWiki.Formatting;
using System.Xml; 
using System.Xml.Serialization; 

namespace FlexWiki.UnitTests
{
	[TestFixture] public class FederationTests : WikiTests
	{
		ContentBase	_base;

		[SetUp] public void Init()
		{
			TheFederation = new Federation(OutputFormat.HTML, new LinkMaker("/federationtests/"));
			string author = "tester-joebob";

			_base = CreateStore("FlexWiki.Base"); 

			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello there", author);
			WriteTestTopicAndNewVersion(_base, "Versioned", "v1", "tester-bob");
			WriteTestTopicAndNewVersion(_base, "Versioned", "v2", "tester-sally");
			WriteTestTopicAndNewVersion(_base, "TopicTwo", @"Something about TopicOne and more!", author);
			WriteTestTopicAndNewVersion(_base, "Props", @"First: one
Second: two
Third:[ lots
and

lots
]
more stuff
", author);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
		}

		[Test] public void TestFederationPropagationOfContentBaseFederationUpdates()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("EventTest", _base.Namespace);
			_base.WriteTopic(tn.LocalName, @"Stay1: hello
Stay2: foo
Go1: foo
Go2: foo
Change1: blag
Change2: blag
");

			FederationUpdate expected = new FederationUpdate();
			expected.RecordPropertyChange(tn, "_ModificationTime", FederationUpdate.PropertyChangeType.PropertyUpdate);
			expected.RecordPropertyChange(tn, "_Body", FederationUpdate.PropertyChangeType.PropertyUpdate);
			expected.RecordPropertyChange(tn, "Go1", FederationUpdate.PropertyChangeType.PropertyRemove);
			expected.RecordPropertyChange(tn, "Go2", FederationUpdate.PropertyChangeType.PropertyRemove);
			expected.RecordPropertyChange(tn, "Change1", FederationUpdate.PropertyChangeType.PropertyUpdate);
			expected.RecordPropertyChange(tn, "Change2", FederationUpdate.PropertyChangeType.PropertyUpdate);
			
			StartMonitoringFederationEvents();
			_base.WriteTopic(tn.LocalName, @"Stay1: hello
Stay2: foo
Change1: new value
Change2: new value
");
			StopMonitoringFederationEvents();
			ContentBaseTests.CompareFederationUpdates(expected, _Events, false, true);
		}

		[Test] public void TestFederationPropertiesChangedEventForAbout()
		{
			FederationUpdate expected = new FederationUpdate();
			expected.FederationPropertiesChanged = true;
			
			StartMonitoringFederationEvents();
			TheFederation.AboutWikiString = "Boo";
			StopMonitoringFederationEvents();
			ContentBaseTests.CompareFederationUpdates(expected, _Events, false, false);
		}

		void FederationUpdateMonitor(object sender, FederationUpdateEventArgs  e) 
		{
			_Events.Add(e.Updates);
		}

		ArrayList _Events;

		void StartMonitoringFederationEvents()
		{
			TheFederation.FederationUpdated += new Federation.FederationUpdateEventHandler(FederationUpdateMonitor);
			_Events = new ArrayList();
		}

		void StopMonitoringFederationEvents()
		{
			TheFederation.FederationUpdated -= new Federation.FederationUpdateEventHandler(FederationUpdateMonitor);
		}
	}
}
