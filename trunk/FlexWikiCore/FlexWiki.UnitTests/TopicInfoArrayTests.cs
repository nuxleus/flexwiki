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
    [Ignore("This test disabled during the 2.0 upgrade. Re-enable as functionality is implemented.")]
    public class TopicInfoArrayTests
    {
        private const string _bh = "http://boo/";
        private Federation _federation;
        private LinkMaker _lm;

        private Federation Federation
        {
            get { return _federation; }
            set { _federation = value; }
        }

        [SetUp]
        public void SetUp()
        {
            _lm = new LinkMaker(_bh);
            MockWikiApplication application = new MockWikiApplication(null, _lm, OutputFormat.HTML, 
                new MockTimeProvider(TimeSpan.FromSeconds(1))); 
            Federation = new Federation(application);
        }


        [Test]
        public void TestIntersect()
        {
            TopicInfoArray list = new TopicInfoArray();
            TopicInfoArray listToCompare = new TopicInfoArray();

            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic4", "testnamespace")));

            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic2", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic5", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic6", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic7", "testnamespace")));

            Assert.AreEqual(3, list.Intersect(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Intersect");

        }

        [Test]
        public void TestIntersectWithDuplicate()
        {
            TopicInfoArray list = new TopicInfoArray();
            TopicInfoArray listToCompare = new TopicInfoArray();

            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic4", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));

            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic2", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic5", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic6", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic7", "testnamespace")));

            Assert.AreEqual(3, list.Intersect(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Intersect");

        }

        [Test]
        public void TestUnion()
        {
            TopicInfoArray list = new TopicInfoArray();
            TopicInfoArray listToCompare = new TopicInfoArray();

            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic4", "testnamespace")));

            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic2", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic5", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic6", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic7", "testnamespace")));

            Assert.AreEqual(8, list.Union(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Union");

        }

        [Test]
        public void TestUnionWithDuplicate()
        {
            TopicInfoArray list = new TopicInfoArray();
            TopicInfoArray listToCompare = new TopicInfoArray();

            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic4", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));

            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic2", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic5", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic6", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic7", "testnamespace")));

            Assert.AreEqual(8, list.Union(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Intersect");

        }

        [Test]
        public void TestDifference()
        {
            TopicInfoArray list = new TopicInfoArray();
            TopicInfoArray listToCompare = new TopicInfoArray();

            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic4", "testnamespace")));

            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic2", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic5", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic6", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic7", "testnamespace")));

            Assert.AreEqual(1, list.Difference(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Difference");

        }

        [Test]
        public void TestDifferenceWithDuplicate()
        {
            TopicInfoArray list = new TopicInfoArray();
            TopicInfoArray listToCompare = new TopicInfoArray();

            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic4", "testnamespace")));
            list.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));

            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topicName", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic3", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic1", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic2", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic5", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic6", "testnamespace")));
            listToCompare.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision("topic7", "testnamespace")));

            Assert.AreEqual(1, list.Difference(listToCompare.Array).Count, "Comparing number of elements in list and listToCompare after Difference");

        }

    }
}
