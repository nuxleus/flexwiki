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
			Assertion.AssertEquals("Hello", new AbsoluteTopicName("Hello").Name);
			Assertion.AssertEquals("Hello", new AbsoluteTopicName("Dog.Hello").Name);
			Assertion.AssertEquals("Dog", new AbsoluteTopicName("Dog.Hello").Namespace);
			Assertion.AssertEquals("Cat.Dog", new AbsoluteTopicName("Cat.Dog.Hello").Namespace);
			Assertion.AssertEquals("Hello", new AbsoluteTopicName("Cat.Dog.Hello").Name);

			Assertion.AssertEquals(null, new AbsoluteTopicName("Hello()").Version);
			Assertion.AssertEquals("123-abc", new AbsoluteTopicName("Hello(123-abc)").Version);
			Assertion.AssertEquals("Hello", new AbsoluteTopicName("Hello(123-abc)").Name);
			Assertion.AssertEquals(null, new AbsoluteTopicName("Hello(123-abc)").Namespace);
			Assertion.AssertEquals("Foo.Bar", new AbsoluteTopicName("Foo.Bar.Hello(123-abc)").Namespace);

			Assertion.AssertEquals("TEST That Acryonyms SPACE Correctly", new AbsoluteTopicName("TESTThatAcryonymsSPACECorrectly").FormattedName);
		}

	}
}
