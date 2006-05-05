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
using System.Collections.Generic;

using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class TopicParserTests
    {
        [Test]
        public void ClosingDelimiterForOpeningMultilinePropertyDelimiter()
        {
            Assert.AreEqual("]", TopicParser.ClosingDelimiterForOpeningMultilinePropertyDelimiter("["));
            Assert.AreEqual("}", TopicParser.ClosingDelimiterForOpeningMultilinePropertyDelimiter("{"));
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void ClosingDelimiterForOpeningMultilinePropertyDelimiterIllegalCharacter()
        {
            TopicParser.ClosingDelimiterForOpeningMultilinePropertyDelimiter(".");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ClosingDelimiterForOpeningMultilinePropertyDelimiterNullCharacter()
        {
            TopicParser.ClosingDelimiterForOpeningMultilinePropertyDelimiter(null); 
        }

        [Test]
        public void IsBehaviorPropertyDelimiter()
        {
            Assert.IsTrue(TopicParser.IsBehaviorPropertyDelimiter("{"),
                "Checking that open brace is a behavior property delimiter."); 
            Assert.IsTrue(TopicParser.IsBehaviorPropertyDelimiter("}"), 
                "Checking that close brace is a behavior property delimiter.");
            Assert.IsFalse(TopicParser.IsBehaviorPropertyDelimiter("{a"),
                "Checking that something that starts with open brace is not a behavior property delimiter.");
            Assert.IsFalse(TopicParser.IsBehaviorPropertyDelimiter("a}"),
                "Checking that something that ends with close brace is not a behavior property delimiter.");
            Assert.IsFalse(TopicParser.IsBehaviorPropertyDelimiter("a{"),
                "Checking that something that ends with open brace is not a behavior property delimiter.");
            Assert.IsFalse(TopicParser.IsBehaviorPropertyDelimiter("}a"),
                "Checking that something that starts with close brace is not a behavior property delimiter.");
        }

        [Test]
        public void ParseExternalReferencesNoReferences()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"");

            Assert.AreEqual(0, parsedTopic.ExternalReferences.Count,
                "Checking that an empty topic results in zero external references."); 
        }

        [Test]
        public void ParseExternalReferencesMultipleReferences()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"@foo=http://www.google.com
@bar=http://microsoft.com/
fake=http://should.not.be/recognized");

            Assert.AreEqual(2, parsedTopic.ExternalReferences.Count,
                "Checking that two external references were parsed.");
            Assert.AreEqual("http://www.google.com", parsedTopic.ExternalReferences["foo"],
                "Checking that the 'foo' reference is correct.");
            Assert.AreEqual("http://microsoft.com/", parsedTopic.ExternalReferences["bar"],
                "Checking that the 'bar' reference is correct.");          
        }

        [Test]
        public void ParseExternalReferencesSingleReference()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"@foo=http://www.google.com");
            Assert.AreEqual(1, parsedTopic.ExternalReferences.Count,
                "Checking that a single external reference is parsed.");
            Assert.AreEqual("http://www.google.com", parsedTopic.ExternalReferences["foo"],
                "Checking that the reference was parsed correctly."); 
        }

        [Test]
        public void ParseMultilineProperty()
        {
            Assert.Fail("Not yet implemented.");
        }

        [Test]
        public void ParseMultipleProperties()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"PropertyOne: ValueA
PropertyTwo: ValueB
PropertyOne: ValueC");

            Assert.AreEqual(2, parsedTopic.Properties.Count,
              "Checking that the right number of property were returned.");
            AssertPropertyCorrect(parsedTopic.Properties[0], "PropertyOne", "ValueA", "ValueC");
            AssertPropertyCorrect(parsedTopic.Properties[1], "PropertyTwo", "ValueB");
        }

        [Test]
        public void ParseSimpleProperty()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"Property: Value");

            Assert.AreEqual(1, parsedTopic.Properties.Count, "Checking that the right number of property were parsed.");
            AssertPropertyCorrect(parsedTopic.Properties[0], "Property", "Value");
        }

        [Test]
        public void ParseTopicLinksForeignNamespace()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"This is a topic with 
a link to WikiTopicOne and WikiTopicTwo and ForeignNamespace.WikiTopicOne");

            AssertTopicLinksCorrect(parsedTopic.TopicLinks,
                new TopicRevision("WikiTopicOne"),
                new TopicRevision("WikiTopicTwo"),
                new TopicRevision("WikiTopicOne", "ForeignNamespace"));
        }
        
        [Test]
        public void ParseTopicLinksNoLinks()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"This is a page with no links.");

            Assert.AreEqual(0, parsedTopic.TopicLinks.Count, "Checking that an empty list of links was returned.");
        }

        [Test]
        public void ParseTopicLinksNoRepeats()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"This is a topic with 
a link to WikiTopicOne, another link to WikiTopicOne, and a link to WikiTopicTwo.");

            AssertTopicLinksCorrect(parsedTopic.TopicLinks,
                new TopicRevision("WikiTopicOne"),
                new TopicRevision("WikiTopicTwo"));
        }

        [Test]
        public void ParseTopicLinksSimple()
        {
            ParsedTopic parsedTopic = TopicParser.Parse(@"This is a page that links
to WikiTopicOne and WikiTopicTwo");

            AssertTopicLinksCorrect(parsedTopic.TopicLinks,
                new TopicRevision("WikiTopicOne"),
                new TopicRevision("WikiTopicTwo")
                );
        }

    
        private static void AssertPropertyCorrect(TopicProperty property, string expectedName, params string[] expectedValues)
        {
            Assert.AreEqual(expectedName, property.Name, "Checking that property name was correct.");

            Assert.AreEqual(expectedValues.Length, property.Values.Count, 
                "Checking that property had the right number of values.");

            for (int i = 0; i < expectedValues.Length; i++)
			{
                string message = string.Format("Checking that rawValue {0} of property {1} is correct.", 
                    i, property.Name); 
			    Assert.AreEqual(expectedValues[i], property.Values[i].RawValue, message); 
			}
        }

        private static void AssertTopicLinksCorrect(IList<TopicRevision> actualLinks,
            params TopicRevision[] expectedLinks)
        {
            Assert.AreEqual(expectedLinks.Length, actualLinks.Count,
                "Checking that the right number of links were returned.");

            for (int i = 0; i < actualLinks.Count; i++)
            {
                Assert.AreEqual(expectedLinks[i], actualLinks[i],
                    string.Format("Checking that link {0} is correct.", i));
            }
        }
    }
}
