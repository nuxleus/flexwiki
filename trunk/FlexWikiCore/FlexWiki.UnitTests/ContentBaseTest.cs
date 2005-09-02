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
	[TestFixture] public class RegistryTests : WikiTests
	{
		ContentBase	_base;
		ContentBase	_other1;
		ContentBase	_other2;
		ContentBase	_other3;
		ContentBase	_cb5;
		const string _bh = "/registrytests/";
		LinkMaker _lm;
		
		
		[SetUp] public void Init()
		{
			string author = "tester-joebob";
			_lm = new LinkMaker(_bh);
			TheFederation = new Federation(OutputFormat.HTML, _lm);

			_base = CreateStore("FlexWiki.Base");
			_other1 = CreateStore("FlexWiki.Other1");
			_other2 = CreateStore("Other2");
			_other3 = CreateStore("Other3");
			_cb5 = CreateStore("Space5");

			WriteTestTopicAndNewVersion(_base, _base.DefinitionTopicName.Name, @"Import: FlexWiki.Other1, Other2", author);
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"OtherOneHello", author);
			WriteTestTopicAndNewVersion(_base, "TopicTwo", @"FlexWiki.Other1.OtherOneGoodbye", author);
			WriteTestTopicAndNewVersion(_base, "TopicThree", @"No.Such.Namespace.FooBar", author);
			WriteTestTopicAndNewVersion(_base, "TopicFour", @".TopicOne", author);
			WriteTestTopicAndNewVersion(_base, "TopicFive", @"FooBar
Role:Designer", author);
			WriteTestTopicAndNewVersion(_base, "TopicSix", @".GooBar
Role:Developer", author);

			WriteTestTopicAndNewVersion(_other1, _other1.DefinitionTopicName.Name, @"Import: Other3,Other2", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneHello", @"hello
Role:Developer", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneGoodbye", @"goodbye", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneRefThree", @"OtherThreeTest", author);

			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicOne", @"OtherTwoHello", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicTwo", @"Other2.OtherTwoGoodbye", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicThree", @"No.Such.Namespace.FooBar", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicFour", @".OtherOneTopicOne", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicFive", @"FooBar", author);
			WriteTestTopicAndNewVersion(_other1, "OtherOneTopicSix", @".GooBar", author);

			WriteTestTopicAndNewVersion(_other2, "OtherTwoHello", @"hello", author);
			WriteTestTopicAndNewVersion(_other2, "OtherTwoGoodbye", @"goodbye", author);

			WriteTestTopicAndNewVersion(_other3, "OtherThreeTest", @"yo", author);

			

			WriteTestTopicAndNewVersion(_cb5, "AbsRef", @"Other2.OtherTwoHello", author);

		}

		[Test] public void EnumIncludingImportsTest()
		{
			ArrayList expecting = new ArrayList();
			expecting.Add("FlexWiki.Base._ContentBaseDefinition");
			expecting.Add("FlexWiki.Base.TopicOne");
			expecting.Add("FlexWiki.Base.TopicTwo");
			expecting.Add("FlexWiki.Base.TopicThree");
			expecting.Add("FlexWiki.Base.TopicFour");
			expecting.Add("FlexWiki.Base.TopicFive");
			expecting.Add("FlexWiki.Base.TopicSix");
			foreach (string backing in _base.BackingTopics.Keys)
			{
				expecting.Add(_base.Namespace + "." + backing);
			}			

			expecting.Add("FlexWiki.Other1._ContentBaseDefinition");
			expecting.Add("FlexWiki.Other1.OtherOneHello");
			expecting.Add("FlexWiki.Other1.OtherOneGoodbye");
			expecting.Add("FlexWiki.Other1.OtherOneRefThree");
			expecting.Add("FlexWiki.Other1.OtherOneTopicOne");
			expecting.Add("FlexWiki.Other1.OtherOneTopicTwo");
			expecting.Add("FlexWiki.Other1.OtherOneTopicThree");
			expecting.Add("FlexWiki.Other1.OtherOneTopicFour");
			expecting.Add("FlexWiki.Other1.OtherOneTopicFive");
			expecting.Add("FlexWiki.Other1.OtherOneTopicSix");
			foreach (string backing in _other1.BackingTopics.Keys)
			{
				expecting.Add(_other1.Namespace + "." + backing);
			}			

			expecting.Add("Other2.OtherTwoHello");
			expecting.Add("Other2.OtherTwoGoodbye");
			foreach (string backing in _other2.BackingTopics.Keys)
			{
				expecting.Add(_other2.Namespace + "." + backing);
			}			


			foreach (AbsoluteTopicName topic in _base.AllTopics(true))
			{
				Assert.IsTrue(expecting.Contains(topic.Fullname), "Looking for " + topic.ToString());
				expecting.Remove(topic.Fullname); 
			}
			Assert.AreEqual(expecting.Count, 0);
		}

		[Test] public void TopicsWithTest()
		{
			TheFederation.EnableCaching();

			Assert.AreEqual(1,_base.TopicsWith(new ExecutionContext(), "Role","Developer").Count,"TopicsWith Role:Developer");
			Assert.AreEqual(1,_base.TopicsWith(new ExecutionContext(), "Role","Designer").Count,"TopicsWith Role:Designer");
			Assert.AreEqual(2,_base.AllTopicsWith(new ExecutionContext(), "Role","Developer").Count,"AllTopicsWith Role:Developer");
			Assert.AreEqual(1,_base.AllTopicsWith(new ExecutionContext(), "Role","Designer").Count,"AllTopicsWith Role:Designer");
		}

		[Test] public void AllTopicsInfo()
		{
			Assert.AreEqual(23,_base.AllTopicsInfo(new ExecutionContext()).Count);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
			_other1.Delete();
			_other2.Delete();
			_other3.Delete();
			_cb5.Delete();
		}


		void CompareTopic(string topic, string outputMustContain)
		{	
			AbsoluteTopicName abs = _base.UnambiguousTopicNameFor(new RelativeTopicName(topic));
			string o = Formatter.FormattedTopic(abs, OutputFormat.HTML, null, TheFederation, _lm, null);
			string o1 = o.Replace("\r", "");
			string o2 = outputMustContain.Replace("\r", "");
			bool pass = o1.IndexOf(o2) >= 0;
			if (!pass)
			{
				Console.Error.WriteLine("Got     : " + o1);
				Console.Error.WriteLine("But Couldn't Find: " + o2);
			}
			Assert.IsTrue(pass);
		}			

		[Test] public void ReferenceTopicInNonImportedNamespace()
		{
			CompareTopic("Space5.AbsRef", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("Other2.OtherTwoHello")) + @""">OtherTwoHello</a>");
		}

		[Test] public void DoubleHopImportTest()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Other1.OtherOneRefThree");
			CompareTopic("OtherOneRefThree", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("Other3.OtherThreeTest")) + @""">OtherThreeTest</a>");
		}

		[Test] public void BaseToForeignUnqualified()
		{
			CompareTopic("TopicOne", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("FlexWiki.Other1.OtherOneHello")) + @""">OtherOneHello</a>");
		}

		[Test] public void BaseToForeignQualified()
		{
			CompareTopic("TopicTwo", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("FlexWiki.Other1.OtherOneGoodbye")) + @""">OtherOneGoodbye</a>");
		}

		[Test] public void BaseToQualifiedAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Base.TopicThree");
			CompareTopic("TopicThree", @"<p>No.Such.Namespace.FooBar</p>");
		}

		[Test] public void BaseToForcedLocal()
		{
			CompareTopic("TopicFour", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("FlexWiki.Base.TopicOne")) + @""">TopicOne</a>");
		}

		[Test] public void BaseToUnqualifiedAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Base.TopicFive");
			CompareTopic("TopicFive", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("FlexWiki.Base.FooBar")) + @""">FooBar</a>");
		}
		
		[Test] public void BaseToForcedLocalAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Base.TopicSix");
			CompareTopic("TopicSix", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("FlexWiki.Base.GooBar")) + @""">GooBar</a>");
		}

		[Test] public void ForeignToForeignUnqualified()
		{
			CompareTopic("OtherOneTopicOne", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("Other2.OtherTwoHello")) + @""">OtherTwoHello</a>");
		}

		[Test] public void ForeignToForeignQualified()
		{
			CompareTopic("OtherOneTopicTwo", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("Other2.OtherTwoGoodbye")) + @""">OtherTwoGoodbye</a>");
		}

		[Test] public void ForeignToQualifiedAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Other1.OtherOneTopicThree");
			CompareTopic("OtherOneTopicThree", @"<p>No.Such.Namespace.FooBar</p>");
		}

		[Test] public void ForeignToForcedLocal()
		{
			CompareTopic("OtherOneTopicFour", @"href=""" + _lm.LinkToTopic(new AbsoluteTopicName("FlexWiki.Other1.OtherOneTopicOne")) + @""">OtherOneTopicOne</a>");
		}

		[Test] public void ForeignToUnqualifiedAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Other1.OtherOneTopicFive");
			CompareTopic("OtherOneTopicFive", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("FlexWiki.Other1.FooBar")) + @""">FooBar</a>");
		}
		
		[Test] public void ForeignToForcedLocalAbsent()
		{
			_lm.ReturnToTopicForEditLinks = new AbsoluteTopicName("FlexWiki.Other1.OtherOneTopicSix");
			CompareTopic("OtherOneTopicSix", @"class=""create"" href=""" + _lm.LinkToEditTopic(new AbsoluteTopicName("FlexWiki.Other1.GooBar")) + @""">GooBar</a>");
		}
	}


	[TestFixture] public class ContentBaseTests : WikiTests
	{
		protected ContentBase	_base;
		private LinkMaker _lm;

		[SetUp] public void Init()
		{
			string author = "tester-joebob";
			_lm = new LinkMaker("/contentbasetests/");

			TheFederation = new Federation(OutputFormat.HTML, _lm);

			_base = CreateStore("FlexWiki.Base");

			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello there", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello a", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello b", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			WriteTestTopicAndNewVersion(_base, "TopicOne", @"Hello c", author);

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
			WriteTestTopicAndNewVersion(_base, "TopicOlder", @"write first", author);
			WriteTestTopicAndNewVersion(_base, "ExternalWikis", @"@wiki1=dozo$$$
@wiki2=fat$$$", author);
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!

			// THIS ONE (TopicNewer) MUST BE WRITTEN LAST!!!!
			WriteTestTopicAndNewVersion(_base, "TopicNewer", @"write last", author);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
		}

		[Test] public void TestTopicCreateEvent()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("EventTest", _base.Namespace);

			FederationUpdate expected = new FederationUpdate();
			expected.RecordCreatedTopic(tn);
			
			StartMonitoringEvents(_base);
			_base.WriteTopic(tn.LocalName, "hello");
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, true, false);
		}


		[Test] public void TestTopicRenameEventWithoutReferenceUpdating()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("EventTest", _base.Namespace);
			_base.WriteTopic(tn.LocalName, "hello");

			AbsoluteTopicName newName = new AbsoluteTopicName("EventTestAfter", _base.Namespace);

			FederationUpdate expected = new FederationUpdate();
			expected.RecordDeletedTopic(tn);
			expected.RecordCreatedTopic(newName);
			
			StartMonitoringEvents(_base);
			_base.RenameTopic(tn.LocalName, newName.Name, false);
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, true, false);
		}

		[Test] public void TestTopicRenameEventWithReferenceUpdating()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("EventTest", _base.Namespace);
			_base.WriteTopic(tn.LocalName, "hello");

			AbsoluteTopicName tn2 = new AbsoluteTopicName("EventTest2", _base.Namespace);
			_base.WriteTopic(tn2.LocalName, "hello and reference to EventTest");
			AbsoluteTopicName tn3 = new AbsoluteTopicName("EventTest3", _base.Namespace);
			_base.WriteTopic(tn3.LocalName, "hello and reference to EventTest");

			AbsoluteTopicName newName = new AbsoluteTopicName("EventTestAfter", _base.Namespace);

			FederationUpdate expected = new FederationUpdate();
			expected.RecordDeletedTopic(tn);
			expected.RecordCreatedTopic(newName);
			expected.RecordUpdatedTopic(tn2);
			expected.RecordUpdatedTopic(tn3);
			
			StartMonitoringEvents(_base);
			_base.RenameTopic(tn.LocalName, newName.Name, true);
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, true, false);
		}


		[Test] public void TestTopicDeleteEventsForNamespaceDelete()
		{
			ContentBase cb = CreateStore("FlexWiki.Delete");
			AbsoluteTopicName tn1 = new AbsoluteTopicName("EventTest1", cb.Namespace);
			cb.WriteTopicAndNewVersion(tn1.LocalName, "topic 1");
			AbsoluteTopicName tn2 = new AbsoluteTopicName("EventTest2", cb.Namespace);
			cb.WriteTopicAndNewVersion(tn2.LocalName, "topic 2");
			AbsoluteTopicName tn3 = new AbsoluteTopicName("EventTest3", cb.Namespace);
			cb.WriteTopicAndNewVersion(tn3.LocalName, "topic 3");

			FederationUpdate expected = new FederationUpdate();
			expected.RecordDeletedTopic(tn1);
			expected.RecordDeletedTopic(tn2);
			expected.RecordDeletedTopic(tn3);			

			StartMonitoringEvents(cb);
			cb.Delete();
			StopMonitoringEvents(cb);

			CompareFederationUpdates(expected, _Events, true, false);

		}

		[Test] public void TestTopicCreateEventWhenCreatingWithNewVersion()
		{
			AbsoluteTopicName tnWithoutVersion = new AbsoluteTopicName("EventTest", _base.Namespace);
			AbsoluteTopicName tnWithVersion = new AbsoluteTopicName(tnWithoutVersion.Name,  tnWithoutVersion.Namespace);
			tnWithVersion.Version = "1.2.3.4.5.6";

			FederationUpdate expected = new FederationUpdate();
			expected.RecordCreatedTopic(tnWithVersion);
			expected.RecordCreatedTopic(tnWithoutVersion);
			
			StartMonitoringEvents(_base);
			_base.WriteTopicAndNewVersion(tnWithVersion.LocalName, "hello");
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, true, false);
		}

		[Test] public void TestTopicUpdateEvent()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("EventTest", _base.Namespace);

			FederationUpdate expected = new FederationUpdate();
			expected.RecordUpdatedTopic(tn);
			
			_base.WriteTopic(tn.LocalName, "hello");

			StartMonitoringEvents(_base);
			_base.WriteTopic(tn.LocalName, "second should be an update");
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, true, false);
		}

    [Ignore("Test fails intermittantly on some machines. Needs further analysis as to why.")]
		[Test] public void TestTopicUpdateWhenAuthorChanges()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("AuthorEventTest", _base.Namespace);

			FederationUpdate expected = new FederationUpdate();
			expected.RecordPropertyChange(tn, "_LastModifiedBy", FederationUpdate.PropertyChangeType.PropertyUpdate);
			expected.RecordPropertyChange(tn, "_Body", FederationUpdate.PropertyChangeType.PropertyUpdate);
			expected.RecordPropertyChange(tn, "_ModificationTime", FederationUpdate.PropertyChangeType.PropertyUpdate);
			
			WriteTestTopicAndNewVersion(_base, tn.Name, "this is the original text", "pre modern man");
			
			StartMonitoringEvents(_base);
			AbsoluteTopicName versioned = WriteTestTopicAndNewVersion(_base, tn.Name, "this is a change", "post modern man");
			StopMonitoringEvents(_base);

			expected.RecordPropertyChange(versioned, "_LastModifiedBy", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(versioned, "_Body", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(versioned, "_ModificationTime", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(versioned, "_TopicName", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(versioned, "_TopicFullName", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(versioned, "_CreationTime", FederationUpdate.PropertyChangeType.PropertyAdd);

			CompareFederationUpdates(expected, _Events, false, true);
		}

		[Test] public void TestTopicDeleteEvent()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("EventTest", _base.Namespace);

			FederationUpdate expected = new FederationUpdate();
			expected.RecordDeletedTopic(tn);
			
			_base.WriteTopic(tn.LocalName, "hello");
			StartMonitoringEvents(_base);
			_base.DeleteTopic(tn.LocalName);
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, true, false);
		}

		[Test] public void TestTopicPropertyCreateEvent()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("EventTest", _base.Namespace);

			FederationUpdate expected = new FederationUpdate();
			expected.RecordPropertyChange(tn, "Prop1", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(tn, "Prop2", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(tn, "Prop3", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(tn, "_Body", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(tn, "_TopicName", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(tn, "_TopicFullName", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(tn, "_LastModifiedBy", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(tn, "_CreationTime", FederationUpdate.PropertyChangeType.PropertyAdd);
			expected.RecordPropertyChange(tn, "_ModificationTime", FederationUpdate.PropertyChangeType.PropertyAdd);
			
			StartMonitoringEvents(_base);
			_base.WriteTopic(tn.LocalName, @"Prop1: hello
Prop2: goobye
Prop3: yellow submarine
");
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, false, true);
		}

		[Test] public void TestTopicPropertyBodyPropertyDoesNotChangeIfBodyNotChanged()
		{
			AbsoluteTopicName tn = new AbsoluteTopicName("EventTest", _base.Namespace);
			string body = @"Prop1: hello
Prop2: goobye
Prop3: yellow submarine
";
			_base.WriteTopic(tn.LocalName, body);

			FederationUpdate expected = new FederationUpdate();
			expected.RecordPropertyChange(tn, "_ModificationTime", FederationUpdate.PropertyChangeType.PropertyUpdate);
			
			StartMonitoringEvents(_base);
			_base.WriteTopic(tn.LocalName, body);
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, false, true);
		}

		[Test] public void TestTopicPropertyUpdatesAndRemoves()
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
			
			StartMonitoringEvents(_base);
			_base.WriteTopic(tn.LocalName, @"Stay1: hello
Stay2: foo
Change1: new value
Change2: new value
");
			StopMonitoringEvents(_base);

			CompareFederationUpdates(expected, _Events, false, true);
		}


		public static void CompareFederationUpdates(FederationUpdate expected, IList events, bool considerTopicChanges, bool considerPropertyChanges)
		{
			Assert.IsTrue(events.Count > 0, "Did not get any federation update events");

			FederationUpdate got = null;

			for (int i = 0; i < events.Count; i++)
			{
				if (i == 0)
					got = (FederationUpdate)(events[i]);
				else 
					got.AddUpdatesFrom((FederationUpdate)(events[i]));
			}

			Assert.AreEqual(expected.FederationPropertiesChanged, got.FederationPropertiesChanged, "FederationPropertiesChanged  doesn't match");
			Assert.AreEqual(expected.NamespaceListChanged, got.NamespaceListChanged, "NamespaceListChanged doesn't match");

			if (considerTopicChanges)
			{
				// OK, look to see if/where got and expected topics are different; assert that they're the same
				foreach (AbsoluteTopicName t in expected.CreatedTopics)
					Assert.IsTrue(got.CreatedTopics.Contains(t), "Missing created topic (" + t.FullnameWithVersion + ") from fired event(s)");
				Assert.AreEqual(expected.CreatedTopics.Count, got.CreatedTopics.Count, "Non-matching number of created topics");

				foreach (AbsoluteTopicName t in expected.UpdatedTopics)
					Assert.IsTrue(got.UpdatedTopics.Contains(t), "Missing updated topic (" + t.FullnameWithVersion + ") from fired event(s)");
				Assert.AreEqual(expected.UpdatedTopics.Count, got.UpdatedTopics.Count, "Non-matching number of updated topics");

				foreach (AbsoluteTopicName t in expected.DeletedTopics)
					Assert.IsTrue(got.DeletedTopics.Contains(t),"Missing deleted topic (" + t.FullnameWithVersion + ") from fired event(s)");
				Assert.AreEqual(expected.DeletedTopics.Count, got.DeletedTopics.Count,"Non-matching number of deleted topics");
			}

			if (considerPropertyChanges)
			{
				// Start by looking through the topics to be sure it's the expected set
				foreach (AbsoluteTopicName t in expected.AllTopicsWithChangedProperties)
					Assert.IsTrue(got.AllTopicsWithChangedProperties.Contains(t), "Missing topic with property update(s) (" + t.FullnameWithVersion + ")");
				Assert.AreEqual(expected.AllTopicsWithChangedProperties.Count, got.AllTopicsWithChangedProperties.Count, "Non-matching number of topics with property updates");

				// OK, we have the right topics; let's check each one for the right set of properties and the right kind of changes
				foreach (AbsoluteTopicName t in expected.AllTopicsWithChangedProperties)
				{
					// check added properties for the topic
					ComparePropertyUpdatesForVerb(t, expected.AddedPropertiesForTopic(t), got.AddedPropertiesForTopic(t), "added");
					ComparePropertyUpdatesForVerb(t, expected.ChangedPropertiesForTopic(t), got.ChangedPropertiesForTopic(t), "changed");
					ComparePropertyUpdatesForVerb(t, expected.RemovedPropertiesForTopic(t), got.RemovedPropertiesForTopic(t), "removed");
				}
			}
		}

		static void ComparePropertyUpdatesForVerb(AbsoluteTopicName t, IList expectedProperties, IList gotProperties, string verb)
		{
			foreach (string propertyName in expectedProperties)
			{
				bool found = gotProperties.Contains(propertyName);
				if (!found)
					System.Diagnostics.Debug.WriteLine("ACK!");
				Assert.IsTrue(found, "Missing " + verb + " property (" + propertyName + " in " + t.FullnameWithVersion + ") from fired event(s)");
				gotProperties.Remove(propertyName);
			}
			if (gotProperties.Count == 0)
				return;	// good -- there should be none left
			string s = "";
			foreach (string p in gotProperties)
			{
				if (s != "")
					s += ", ";
				s += p;
			}
			Assert.Fail("Unexpected " + verb + " property notifications for: " + s);
		}

		void FederationUpdateMonitor(object sender, FederationUpdateEventArgs  e) 
		{
			_Events.Add(e.Updates);
		}

		void StartMonitoringEvents(ContentBase cb)
		{
			cb.FederationUpdated += new ContentBase.FederationUpdateEventHandler(FederationUpdateMonitor);
			_Events = new ArrayList();
		}

		void StopMonitoringEvents(ContentBase cb)
		{
			cb.FederationUpdated -= new ContentBase.FederationUpdateEventHandler(FederationUpdateMonitor);
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


		[Test] public virtual void TestSerialization()
		{
			MemoryStream ms = new MemoryStream(); 
			XmlWriter wtr = new XmlTextWriter(ms, System.Text.Encoding.UTF8); 
			XmlSerializer ser = new XmlSerializer(typeof(FileSystemStore)); 
			ser.Serialize(wtr, _base); 

			wtr.Close(); 

			// If we got this far, there was no exception. More rigorous 
			// testing would assert XPath expressions against the XML 

		} 

		[Test] public void TestTopicNameSerialization()
		{
			AbsoluteTopicName name1 = ContentBase().TopicNameFor("TopicOne");

			MemoryStream ms = new MemoryStream(); 
			XmlWriter wtr = new XmlTextWriter(ms, System.Text.Encoding.UTF8); 
			XmlSerializer ser = new XmlSerializer(typeof(AbsoluteTopicName)); 
			ser.Serialize(wtr, name1); 
			wtr.Close(); 
		} 



		[Test] public void RenameTest()
		{
			AbsoluteTopicName name1 = ContentBase().TopicNameFor("TopicOne");
			AbsoluteTopicName name2 = ContentBase().TopicNameFor("TopicTwo");
			ArrayList log = ContentBase().RenameTopic(name1.LocalName, "TopicOneRenamed", true);
			Assert.AreEqual(log.Count, 1);
			string t2 = ContentBase().Read(name2.LocalName);
			Assert.AreEqual("Something about TopicOneRenamed and more!", t2);
		}

		[Test] public void RenameAndVerifyVersionsGetRenamedTest()
		{
			LocalTopicName name1 = new LocalTopicName("TopicOne");
			LocalTopicName name2 = new LocalTopicName("TopicOneRenamed");
			ContentBase().RenameTopic(name1, name2.Name, false);

			int c = 0;
			foreach (object x in ContentBase().AllVersionsForTopic(name2))
			{
				x.ToString();		// do something with x just to shut the compiler up
				c++;			
			}
			Assert.AreEqual(c, 4);	// should be 4 versions, even after rename
		}

		
		ContentBase ContentBase()
		{
			return _base;
		}

		[Test] public void SimpleVersioningTests()
		{
			ArrayList list = new ArrayList();
			foreach (AbsoluteTopicName s in ContentBase().AllVersionsForTopic(new LocalTopicName("Versioned")))
			{
				list.Add(s);
			}
			Assert.AreEqual(list.Count, 2);
		}

		[Test] public void AllChangesForTopicSinceTests()
		{
			ArrayList list = new ArrayList();
			foreach (TopicChange c in ContentBase().AllChangesForTopicSince(new LocalTopicName("Versioned"), DateTime.MinValue))
			{
				list.Add(c);
			}
			Assert.AreEqual(list.Count, 2);

			list = new ArrayList();
			foreach (string s in ContentBase().AllChangesForTopicSince(new LocalTopicName("Versioned"), DateTime.MaxValue))
			{
				list.Add(s);
			}
			Assert.AreEqual(list.Count, 0);
		}

		[Test] public void EnsureContentBaseDirectoryIsMadeIfAbsent()
		{
			string ns = "Test.Foo";
			string p = @"\temp-wikitests-creation";

			FileSystemStore store = new FileSystemStore(TheFederation, ns, p);
			Assert.IsTrue(Directory.Exists(p));
			store.Delete();
		}

		[Test] public void TopicTimeTests()
		{
			Assert.IsTrue(ContentBase().GetTopicCreationTime(new LocalTopicName("TopicOlder")) < ContentBase().GetTopicCreationTime(new LocalTopicName("TopicNewer")));
			Assert.AreEqual(ContentBase().GetTopicLastWriteTime(new LocalTopicName("TopicNewer")), ContentBase().LastModified(true));
		}

		[Test] public void AuthorshipTests()
		{
			Assert.AreEqual(ContentBase().GetTopicLastAuthor(new LocalTopicName("TopicNewer")), ContentBase().GetTopicLastAuthor(new LocalTopicName("TopicOlder")));
		}

		[Test] public void ExternalWikisTests()
		{
			Hashtable t = ContentBase().ExternalWikiHash();
			Assert.AreEqual(t.Count, 2);
			Assert.AreEqual(t["wiki1"], "dozo$$$");
			Assert.AreEqual(t["wiki2"], "fat$$$");
		}

		[Test] public void GetFieldsTest()
		{

			Hashtable t = ContentBase().GetFieldsForTopic(new LocalTopicName("Props"));
			Assert.AreEqual(t["First"], "one");
			Assert.AreEqual(t["Second"], "two");
			Assert.AreEqual(t["Third"], @"lots
and

lots");
		}

		[Test] public void BasicEnumTest()
		{
			ArrayList expecting = new ArrayList();
			expecting.Add("TopicOne");
			expecting.Add("TopicTwo");
			expecting.Add("Props");
			expecting.Add("ExternalWikis");
			expecting.Add("TopicOlder");
			expecting.Add("Versioned");
			expecting.Add("TopicNewer");
			foreach (string backing in ContentBase().BackingTopics.Keys)
			{
				expecting.Add(backing);
			}			

			foreach (AbsoluteTopicName topic in ContentBase().AllTopics(false))
			{
				Assert.IsTrue(expecting.Contains(topic.Name), "Looking for " + topic.Name);
				expecting.Remove(topic.Name);
			}
			Assert.AreEqual(expecting.Count, 0);
		}

		
		[Test] public void SetFieldsTest()
		{
			ContentBase cb = ContentBase();
			string author = "joe_author";
			AbsoluteTopicName wn = WriteTestTopicAndNewVersion(cb, "FieldsTesting", "", author);
			
			Hashtable t;
			
			t = cb.GetFieldsForTopic(wn.LocalName);
			
			cb.SetFieldValue(wn.LocalName, "First", "one", false);
			t = cb.GetFieldsForTopic(wn.LocalName);
			Assert.AreEqual(t["First"], "one");

			cb.SetFieldValue(wn.LocalName, "Second", "two", false);
			t = cb.GetFieldsForTopic(wn.LocalName);
			Assert.AreEqual(t["First"], "one");
			Assert.AreEqual(t["Second"], "two");
			
			cb.SetFieldValue(wn.LocalName, "Second", "change", false);
			t = cb.GetFieldsForTopic(wn.LocalName);
			Assert.AreEqual(t["First"], "one");
			Assert.AreEqual(t["Second"], "change");

			cb.SetFieldValue(wn.LocalName, "First", @"change
is
good", false);
			t = cb.GetFieldsForTopic(wn.LocalName);
			Assert.AreEqual(t["First"], @"change
is
good");
			Assert.AreEqual(t["Second"], "change");

			cb.SetFieldValue(wn.LocalName, "First", "one", false);
			cb.SetFieldValue(wn.LocalName, "Second", "change", false);
			t = cb.GetFieldsForTopic(wn.LocalName);
			Assert.AreEqual(t["First"], "one");
			Assert.AreEqual(t["Second"], "change");
		}

		[Test] public void SimpleReadingAndWritingTest()
		{
			LocalTopicName an = new LocalTopicName("SimpleReadingAndWritingTest");
			string c = @"Hello
There";
			ContentBase().WriteTopic(an, c);
			string ret;
			using (TextReader sr = ContentBase().TextReaderForTopic(an))
			{
				ret = sr.ReadToEnd();
			}
			Assert.AreEqual(c, ret);

			ContentBase().DeleteTopic(an);
			Assert.IsTrue(!ContentBase().TopicExistsLocally(an));
		}



		[Test] public void SimpleTopicExistsTest()
		{
			Assert.IsTrue(ContentBase().TopicExists(ContentBase().TopicNameFor("TopicOne")));
		}

		[Test] public void LatestVersionForTopic()
		{
		string author = "LatestVersionForTopicTest"; 
		WriteTestTopicAndNewVersion(_base, "TopicOne", @"A Change", author);
		WriteTestTopicAndNewVersion(_base, "TopicTwo", @"A Change", author);
		WriteTestTopicAndNewVersion(_base, "TopicThree", @"A Change", author);

		ContentBase cb = ContentBase(); 
	      
		LocalTopicName atn1 = new LocalTopicName("TopicOne"); 
		LocalTopicName atn2 = new LocalTopicName("TopicTwo"); 
		LocalTopicName atn3 = new LocalTopicName("TopicThree"); 

		IEnumerable versions1 = cb.AllVersionsForTopic(atn1); 
		IEnumerable versions2 = cb.AllVersionsForTopic(atn2); 
		IEnumerable versions3 = cb.AllVersionsForTopic(atn3); 

		string version1 = null; 
		string version2 = null; 
		string version3 = null; 

		foreach (AbsoluteTopicName atn in versions1)
		{
			version1 = atn.Version; 
			break;
		}
		foreach (AbsoluteTopicName atn in versions2)
		{
			version2 = atn.Version; 
			break;
		}
		foreach (AbsoluteTopicName atn in versions3)
		{
			version3 = atn.Version; 
			break;
		}

		Assert.AreEqual(version1, cb.LatestVersionForTopic(atn1), "Checking that latest version is calculated correctly"); 
		Assert.AreEqual(version2, cb.LatestVersionForTopic(atn2), "Checking that latest version is calculated correctly"); 
		Assert.AreEqual(version3, cb.LatestVersionForTopic(atn3), "Checking that latest version is calculated correctly"); 

		}

		[Test] public void CompareTwoTopicVersions()
		{
			AbsoluteTopicName newestTopicVersion = null;
			AbsoluteTopicName oldTopicVersion	 = null;

			oldTopicVersion = WriteTestTopicAndNewVersion(_base, "Versioned", "Original version", "CompareTest");
			System.Threading.Thread.Sleep(100); // need the newer one to be newer enough!
			newestTopicVersion = WriteTestTopicAndNewVersion(_base, "Versioned", "Compare this version with an other.", "CompareTest");
			
			Assert.IsNotNull(newestTopicVersion, "Have not found the newer version of topic");
			Assert.IsNotNull(oldTopicVersion, "Have not found the older version of topic");

			string[] outputMustContainList = new string[] { @"<p style=""background: palegreen"">Compare this version with an other.</p>", 
                                                      @"<p style=""color: silver; text-decoration: line-through"">Original version</p>" };

      foreach (string outputMustContain in outputMustContainList)
      {
        string o = TheFederation.GetTopicFormattedContent(newestTopicVersion, oldTopicVersion);
        bool pass = o.IndexOf(outputMustContain) >= 0;
        if (!pass)
        {
          Console.Error.WriteLine("Got     : " + o);
          Console.Error.WriteLine("But Couldn't Find: " + outputMustContain);
        }
        Assert.IsTrue(pass, "The result of the compare is not as expected.");
      }

		}


	}

	[TestFixture] public class InfiniteRecursionTests : WikiTests
	{
		ContentBase	_base, _imp1, _imp2;

		[SetUp] public void Init()
		{
			TheFederation = new Federation(OutputFormat.HTML, new LinkMaker("/infiniterecursiontests/"));
			_base = CreateStore("FlexWiki.Projects.Wiki");
			_imp1 = CreateStore("FlexWiki.Projects.Wiki1");
			_imp2 = CreateStore("FlexWiki.Projects.Wiki2");

			string author = "tester-joebob";
			WriteTestTopicAndNewVersion(_base, _base.DefinitionTopicName.Name, @"
Description: Test description
Import: FlexWiki.Projects.Wiki1", author);

			WriteTestTopicAndNewVersion(_imp1, _imp1.DefinitionTopicName.Name, @"
Description: Test1 description
Import: FlexWiki.Projects.Wiki2", author);

			WriteTestTopicAndNewVersion(_imp2, _imp2.DefinitionTopicName.Name, @"
Description: Test1 description
Import: FlexWiki.Projects.Wiki", author);

		}

		[Test] public void TestRecurse()
		{
			Assert.AreEqual("FlexWiki.Projects.Wiki", _base.Namespace);
			Assert.AreEqual("FlexWiki.Projects.Wiki1", _imp1.Namespace);
			Assert.AreEqual("FlexWiki.Projects.Wiki2", _imp2.Namespace);
		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
			_imp1.Delete();
			_imp2.Delete();
		}
		
	}

	[TestFixture] public class MoreContentBaseTests : WikiTests
	{
		ContentBase	_base, _imp1, _imp2;

		[SetUp] public void Init()
		{
			TheFederation = new Federation(OutputFormat.HTML, new LinkMaker("/morecontentbasetests/"));
			_base = CreateStore("FlexWiki.Projects.Wiki");
			_imp1 = CreateStore("FlexWiki.Projects.Wiki1");
			_imp2 = CreateStore("FlexWiki.Projects.Wiki2");

			string author = "tester-joebob";
			WriteTestTopicAndNewVersion(_base, _base.DefinitionTopicName.Name, @"
Description: Test description
Import: FlexWiki.Projects.Wiki1, FlexWiki.Projects.Wiki2", author);

		}

		[TearDown] public void DeInit()
		{
			_base.Delete();
			_imp1.Delete();
			_imp2.Delete();
		}
		
		ContentBase Base
		{
			get
			{
				return _base;
			}
		}

		[Test] public void SimpleReadingTest()
		{
			Assert.AreEqual("FlexWiki.Projects.Wiki", Base.Namespace);
			Assert.AreEqual("Test description", Base.Description);
			ArrayList rels = new ArrayList();
			rels.Add(_imp1.Namespace);
			rels.Add(_imp2.Namespace);
			foreach (ContentBase each in  Base.ImportedContentBases)
			{
				Assert.IsTrue(rels.Contains(each.Namespace));
				rels.Remove(each.Namespace);
			}
			Assert.AreEqual(0, rels.Count);					
		}
	}
	[TestFixture] public class ExtractTests : WikiTests
	{
		string full = "HomePage(2003-11-24-20-31-20-WINGROUP-davidorn)";

		[Test] public void VersionTest()
		{
			Assert.AreEqual("2003-11-24-20-31-20-WINGROUP-davidorn", FlexWiki.FileSystemStore.ExtractVersionFromHistoricalFilename(full));
		}

		[Test] public void NameTest()
		{
			Assert.AreEqual("HomePage",  FlexWiki.FileSystemStore.ExtractTopicFromHistoricalFilename(full));
		}			
	}

}
