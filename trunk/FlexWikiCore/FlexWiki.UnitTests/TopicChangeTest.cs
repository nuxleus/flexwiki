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
	[TestFixture] public class TopicChangeTests : WikiTests
	{
		[Test] public void VersionWithSimpleName()
		{
			TopicChange ch = new TopicChange(new AbsoluteTopicName("Foo.CodeImprovementIdeas(2003-11-23-14-34-03-Name)"));
			DateTime dt = new DateTime(2003, 11, 23, 14,34, 3, 0);
			Assertion.AssertEquals(dt, ch.Timestamp);
		}

		[Test] public void VersionWithSimpleNameWithMilliseconds()
		{
			TopicChange ch = new TopicChange(new AbsoluteTopicName("Foo.CodeImprovementIdeas(2003-11-23-14-34-03.123-Name)"));
			DateTime dt = new DateTime(2003, 11, 23, 14,34, 3, 123);
			Assertion.AssertEquals(dt, ch.Timestamp);
		}


		[Test] public void VersionWithLeadingDigitForName()
		{
			TopicChange ch = new TopicChange(new AbsoluteTopicName("Foo.CodeImprovementIdeas(2003-11-23-14-34-03-127.0.0.1)"));
			DateTime dt = new DateTime(2003, 11, 23, 14,34, 3, 0);
			Assertion.AssertEquals(dt, ch.Timestamp);
		}

		[Test] public void VersionWithMillisecondsWithLeadingDigitForName()
		{
			TopicChange ch = new TopicChange(new AbsoluteTopicName("Foo.CodeImprovementIdeas(2003-11-23-14-34-03.1-127.0.0.1)"));
			DateTime dt = new DateTime(2003, 11, 23, 14,34, 3, 100);
			Assertion.AssertEquals(dt, ch.Timestamp);
		}

		[Test] public void VersionWithMillisecondsWithExtraTail()
		{
			TopicChange ch = new TopicChange(new AbsoluteTopicName("Foo.CodeImprovementIdeas(2003-11-23-14-34-03.1000-127.0.0.1)"));
			DateTime dt = new DateTime(2003, 11, 23, 14,34, 3, 100);
			Assertion.AssertEquals(dt, ch.Timestamp);
		}

		[Test] public void VersionWithMillisecondsBiggie()
		{
			TopicChange ch = new TopicChange(new AbsoluteTopicName("Foo.CodeImprovementIdeas(2003-11-23-14-34-03.8890-127.0.0.1)"));
			DateTime dt = new DateTime(2003, 11, 23, 14,34, 3, 889);
			Assertion.AssertEquals(dt, ch.Timestamp);
		}
	}
}
