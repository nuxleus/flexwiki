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

using FlexWiki;
using FlexWiki.Formatting;

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for TopicInfoArrayTests.
	/// </summary>
	[TestFixture]
	public class TopicInfoArrayTests: WikiTests
	{
		const string _bh = "http://boo/";
		LinkMaker _lm;

		public TopicInfoArrayTests()
		{
		}

		[SetUp] public void Init()
		{
			_lm = new LinkMaker(_bh);
			TheFederation = new Federation(OutputFormat.HTML, _lm);
		}

		[Test]
		public void TestIntersect()
		{
			TopicInfoArray list = new TopicInfoArray();
			TopicInfoArray listToCompare = new TopicInfoArray();

			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic4", "testnamespace")));

			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic2", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic5", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic6", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic7", "testnamespace")));

			Assert.AreEqual(3, list.Intersect(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Intersect");

		}

		[Test]
		public void TestIntersectWithDuplicate()
		{
			TopicInfoArray list = new TopicInfoArray();
			TopicInfoArray listToCompare = new TopicInfoArray();

			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic4", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));

			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic2", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic5", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic6", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic7", "testnamespace")));

			Assert.AreEqual(3, list.Intersect(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Intersect");

		}

		[Test]
		public void TestUnion()
		{
			TopicInfoArray list = new TopicInfoArray();
			TopicInfoArray listToCompare = new TopicInfoArray();

			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic4", "testnamespace")));

			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic2", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic5", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic6", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic7", "testnamespace")));

			Assert.AreEqual(8, list.Union(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Union");

		}

		[Test]
		public void TestUnionWithDuplicate()
		{
			TopicInfoArray list = new TopicInfoArray();
			TopicInfoArray listToCompare = new TopicInfoArray();

			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic4", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));

			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic2", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic5", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic6", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic7", "testnamespace")));

			Assert.AreEqual(8, list.Union(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Intersect");

		}

		[Test]
		public void TestDifference()
		{
			TopicInfoArray list = new TopicInfoArray();
			TopicInfoArray listToCompare = new TopicInfoArray();

			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic4", "testnamespace")));

			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic2", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic5", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic6", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic7", "testnamespace")));

			Assert.AreEqual(1, list.Difference(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Difference");

		}

		[Test]
		public void TestDifferenceWithDuplicate()
		{
			TopicInfoArray list = new TopicInfoArray();
			TopicInfoArray listToCompare = new TopicInfoArray();

			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic4", "testnamespace")));
			list.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));

			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic3", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic1", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic2", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic5", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic6", "testnamespace")));
			listToCompare.Add(new TopicInfo(TheFederation, new AbsoluteTopicName("topic7", "testnamespace")));

			Assert.AreEqual(1, list.Difference(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Difference");

		}

	}
}
