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
    public class TopicPropertyTests
    {
        [Test]
        public void AsListSingleValue()
        {
            TopicProperty property = new TopicProperty("Name");
            property.Values.Add(new TopicPropertyValue("Single rawValue"));
            Assert.AreEqual(1, property.AsList().Count);
            Assert.AreEqual("Single rawValue", property.AsList()[0], "Checking that single rawValue is returned intact.");
        }

        [Test]
        public void AsListMultipleValues()
        {
            TopicProperty property = new TopicProperty("Name");
            property.Values.Add(new TopicPropertyValue("List, of multiple, values"));
            AssertValueListCorrect(property.AsList(), "List", "of multiple", "values");
        }

        [Test]
        public void HasValueNegative()
        {
            TopicProperty property = new TopicProperty("Name");

            Assert.IsFalse(property.HasValue, "Checking that a property with no value returns the correct result."); 
        }

        [Test]
        public void HasValuePositive()
        {
            TopicProperty property = new TopicProperty("Name"); 
            property.Values.Add(new TopicPropertyValue("Single rawVaue")); 

            Assert.IsTrue(property.HasValue, "Checking that a property with a value returns the correct result."); 
        }

        [Test]
        public void LastValueMultipleValues()
        {
            TopicProperty property = new TopicProperty("Name");
            property.Values.Add(new TopicPropertyValue("Value1"));
            property.Values.Add(new TopicPropertyValue("Value2"));
            property.Values.Add(new TopicPropertyValue("Value3"));

            Assert.AreEqual("Value3", property.LastValue, "Checking that the last value is correct."); 
        }

        [Test]
        public void LastValueNoValues()
        {
            TopicProperty property = new TopicProperty("Name");

            Assert.IsNull(property.LastValue, "Checking that null is returned when the property has no values."); 
        }

        [Test]
        public void LastValueSingleValue()
        {
            TopicProperty property = new TopicProperty("Name");
            property.Values.Add(new TopicPropertyValue("Value"));

            Assert.AreEqual("Value", property.LastValue, "Checking that the last value is correct."); 
        }

        private static void AssertValueListCorrect(IList<string> actual, params string[] expected)
        {
            Assert.AreEqual(expected.Length, actual.Count, "Checking that the correct number of values were returned.");
            for (int i = 0; i < actual.Count; ++i)
            {
                Assert.AreEqual(expected[i], actual[i],
                  string.Format("Checking that rawValue {0} is correct.", i));
            }
        }
    }
}
