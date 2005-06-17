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

		
	[TestFixture] public class CachingTests : WikiTests
	{
		ContentBase _cb, _cb2;
		const string _base = "http://boo/";
		LinkMaker _lm;
		string user = "joe";

		[SetUp] public void Init()
		{
			_lm = new LinkMaker(_base);
			TheFederation = new Federation(OutputFormat.HTML, _lm);
			TheFederation.WikiTalkVersion = 1;
			string ns = "FlexWiki";
			string ns2 = "FlexWiki2";

			_cb = CreateStore(ns);
			_cb2 = CreateStore(ns2);

			WriteTestTopicAndNewVersion(_cb, _cb.DefinitionTopicName.Name, "Import: FlexWiki2", user);
			WriteTestTopicAndNewVersion(_cb, "HomePage", "This is a simple topic RefOne plus PluralWords reference to wiki://PresentIncluder wiki://TestLibrary/foo.gif", user);
			WriteTestTopicAndNewVersion(_cb2, "AbsentIncluder", "{{NoSuchTopic}}", user);
			WriteTestTopicAndNewVersion(_cb2, "PresentIncluder", "{{IncludePresent}}", user);
			WriteTestTopicAndNewVersion(_cb2, "IncludePresent", "hey! this is ReferencedFromIncludePresent", user);
			WriteTestTopicAndNewVersion(_cb2, "TestLibrary", "URI: whatever", user);
		}

		[Test] public void TestRulesForAllTopicsInNamespace()
		{
			ExecutionContext ctx = new ExecutionContext();
			_cb.AllTopicsInfo(ctx);

			// 	We should find cache rules for all the topics in the namespaces and for the definition (in case the imports change)
			ArrayList expected = new ArrayList();
			expected.Add(new AllTopicsInNamespaceCacheRule(TheFederation, _cb.Namespace));
			expected.Add(new AllTopicsInNamespaceCacheRule(TheFederation, _cb2.Namespace));
			expected.Add(_cb.CacheRuleForDefinition);

			VerifyCacheRules(ctx, expected);
		}

		[Test] public void TestRulesForTopicsInNamespace()
		{
			ExecutionContext ctx = new ExecutionContext();
			_cb.Topics(ctx);

			// 	We should find cache rules for all the topics in the namespaces and for the definition (in case the imports change)
			ArrayList expected = new ArrayList();
			expected.Add(new AllTopicsInNamespaceCacheRule(TheFederation, _cb.Namespace));

			VerifyCacheRules(ctx, expected);
		}

		[Test] public void TestRulesForNamespaceDescription()
		{
			ExecutionContext ctx = new ExecutionContext();
			_cb.ExposedDescription(ctx);
			ArrayList expected = new ArrayList();
			expected.Add(_cb.CacheRuleForDefinition);
			VerifyCacheRules(ctx, expected);
		}

		[Test] public void TestRulesForNamespaceContact()
		{
			ExecutionContext ctx = new ExecutionContext();
			_cb.ExposedContact(ctx);
			ArrayList expected = new ArrayList();
			expected.Add(_cb.CacheRuleForDefinition);
			VerifyCacheRules(ctx, expected);
		}

		[Test] public void TestRulesForNamespaceImage()
		{
			ExecutionContext ctx = new ExecutionContext();
			_cb.ExposedImageURL(ctx);
			ArrayList expected = new ArrayList();
			expected.Add(_cb.CacheRuleForDefinition);
			VerifyCacheRules(ctx, expected);
		}

		[Test] public void TestRulesForNamespaceImports()
		{
			ExecutionContext ctx = new ExecutionContext();
			_cb.ExposedImports(ctx);
			ArrayList expected = new ArrayList();
			expected.Add(_cb.CacheRuleForDefinition);
			VerifyCacheRules(ctx, expected);
		}

		[Test] public void TestRulesForNamespaceName()
		{
			ExecutionContext ctx = new ExecutionContext();
			_cb.ExposedName(ctx);
			ArrayList expected = new ArrayList();
			expected.Add(_cb.CacheRuleForDefinition);
			VerifyCacheRules(ctx, expected);
		}

		[Test] public void TestRulesForNamespaceTitle()
		{
			ExecutionContext ctx = new ExecutionContext();
			_cb.ExposedTitle(ctx);
			ArrayList expected = new ArrayList();
			expected.Add(_cb.CacheRuleForDefinition);
			VerifyCacheRules(ctx, expected);
		}

		[Test] public void TestRulesForFederationNamespaceInfo()
		{
			ExecutionContext ctx = new ExecutionContext();
			TheFederation.ExposedContentBaseForNamespace(ctx, _cb.Namespace);
			ArrayList expected = new ArrayList();
			expected.Add(_cb.CacheRuleForDefinition);
			expected.Add(TheFederation.CacheRuleForNamespaces);
			VerifyCacheRules(ctx, expected);
		}


		/// <summary>
		/// Confirm the set of cache rules in the given execution context includes a set of expected ones; assertion failure if not.
		/// </summary>
		/// <param name="ctx"></param>
		/// <param name="rules"></param>
		void VerifyCacheRules(ExecutionContext ctx, IList rules)
		{
			Set set = new Set();
			foreach (CacheRule rule in ctx.CacheRules)
				set.AddRange(rule.AllLeafRules);
			foreach (CacheRule r in rules)
			{
				Assert.IsTrue(set.Contains(r), "Unable to find expected CacheRule (" + r.Description + ")");
				set.Remove(r);
			}
		}

		[Test] public void TestIncludeForPresentTopic()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki2.PresentIncluder");
			AssertTopicRule(rule, "FlexWiki2.IncludePresent");
			AssertTopicRule(rule, "FlexWiki2.ReferencedFromIncludePresent");
		}

		[Test] public void TestIncludeForAbsentTopic()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki2.AbsentIncluder");
			AssertTopicRule(rule, "FlexWiki2.NoSuchTopic");
		}

		[Test] public void TestWikiURLResourceLibraryCaching()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki.HomePage");
			AssertTopicRule(rule, "FlexWiki.TestLibrary");
			AssertTopicRule(rule, "FlexWiki2.TestLibrary");
		}


		[Test] public void TestWikiURLTopicCaching()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki.HomePage");
			AssertTopicRule(rule, "FlexWiki.PresentIncluder");
			AssertTopicRule(rule, "FlexWiki2.PresentIncluder");
		}

		[Test] public void TestBasicTopicCaching()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki.HomePage");
			AssertTopicRule(rule, "FlexWiki.HomePage");
			AssertTopicRule(rule, _cb.DefinitionTopicName.Fullname);
		}

		[Test] public void TestBasicTopicCachingAllPossibleVersions()
		{
			CacheRule rule = GetCacheRuleForTopic("FlexWiki.HomePage");

			// We should see two cache rules, one for each of the two namespaces in which the referenced topic could appear
			AssertTopicRule(rule,  "FlexWiki.RefOne");
			AssertTopicRule(rule,  "FlexWiki2.RefOne");

			// We should see four refs for PluralWord(s) (because it's a plural form)
			AssertTopicRule(rule,  "FlexWiki.PluralWord");
			AssertTopicRule(rule,  "FlexWiki2.PluralWord");
			AssertTopicRule(rule,  "FlexWiki.PluralWords");
			AssertTopicRule(rule,  "FlexWiki2.PluralWords");
		}

		[TearDown] public void Deinit()
		{
			_cb.Delete();
			_cb2.Delete();
		}

		void AssertTopicRule(CacheRule rule, string absName)
		{
			int count = CountTopicRuleMatches(rule, absName);
			if (count == 0)
				System.Console.Error.WriteLine("Rule: " + rule.Description);
			Assert.IsTrue(count > 0, "Searching for topic (" + absName + ") in cache rule"); 
		}

		int CountTopicRuleMatches(CacheRule rule, string path)
		{
			int found = 0;
			foreach (CacheRule r in rule.AllLeafRules)
			{
				if (r is TopicsCacheRule)
				{
					TopicsCacheRule tcr = (TopicsCacheRule)r;
					foreach (AbsoluteTopicName p in tcr.Topics)
					{
						if (p.ToString().IndexOf(path) >= 0)
						{
							found++;
						}
					}
				}
			}
			return found;
		}

		CacheRule GetCacheRuleForTopic(string topic)
		{
			AbsoluteTopicName tn = new AbsoluteTopicName(topic);
			CompositeCacheRule rule = new CompositeCacheRule();
			Formatter.FormattedTopic(tn, OutputFormat.Testing, null, TheFederation, _lm, rule);
			return rule;
		}

	}

}
