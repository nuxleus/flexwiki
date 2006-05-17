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
using NUnit.Framework;

namespace FlexWiki.UnitTests
{
    [TestFixture]
    public class TopicRevisionTests
    {
        [Test]
        public void ConstructDefault()
        {
            TopicRevision revision = new TopicRevision();
            Assert.IsNull(revision.LocalName, "Checking that local name is null by default.");
            Assert.IsNull(revision.Namespace, "Checking that namespace is null by default.");
            Assert.IsNull(revision.Version, "Checking that version is null by default.");
        }

        [Test]
        public void ConstructByString()
        {
            // format is revision, name, version
            string[][] data = {
                new string[] { "Foo", "Foo", null } ,
                new string[] { "Foo.Bar", "Foo.Bar", null } ,
                new string[] { "Cat.Dog.Hello", "Cat.Dog.Hello", null } ,
                new string[] { "Hello()", "Hello", null } ,
                new string[] { "Hello(123-abc)", "Hello", "123-abc" } , 
            };
            string[] revisions = { "Foo", "Foo.Bar", "Cat.Dog.Hello", "Hello()" };
            string[] names = { "Foo", "Foo.Bar", "Cat.Dog.Hello", "Hello" };
            string[] versions = { null, null, null, null }; 

            for (int i = 0; i < data.Length; ++i)
            {
                string revisionName = data[i][0];
                string name = data[i][1];
                string version = data[i][2]; 

                TopicRevision revision = new TopicRevision(revisionName);
                Assert.AreEqual(revision.Name, new TopicName(name),
                    "Checking that name was correct during construction by name was successful for " + name); 
                Assert.AreEqual(revision.Version, version, 
                    "Checking that version was correct during construction by name was successful for " + name); 
            }
            
        }


    }
}
