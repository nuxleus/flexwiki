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
	[TestFixture] public class TopicNameTests : WikiTests
	{
		[Test] public void SimpleTests()
		{
			Assert.AreEqual("Hello", new AbsoluteTopicName("Hello").Name);
			Assert.AreEqual("Hello", new AbsoluteTopicName("Dog.Hello").Name);
			Assert.AreEqual("Dog", new AbsoluteTopicName("Dog.Hello").Namespace);
			Assert.AreEqual("Cat.Dog", new AbsoluteTopicName("Cat.Dog.Hello").Namespace);
			Assert.AreEqual("Hello", new AbsoluteTopicName("Cat.Dog.Hello").Name);

			Assert.AreEqual(null, new AbsoluteTopicName("Hello()").Version);
			Assert.AreEqual("123-abc", new AbsoluteTopicName("Hello(123-abc)").Version);
			Assert.AreEqual("Hello", new AbsoluteTopicName("Hello(123-abc)").Name);
			Assert.AreEqual(null, new AbsoluteTopicName("Hello(123-abc)").Namespace);
			Assert.AreEqual("Foo.Bar", new AbsoluteTopicName("Foo.Bar.Hello(123-abc)").Namespace);

			Assert.AreEqual("TEST That Acryonyms SPACE Correctly", new AbsoluteTopicName("TESTThatAcryonymsSPACECorrectly").FormattedName);
		}

	}
}
