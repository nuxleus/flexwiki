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
using System.Web.Caching;
using FlexWiki;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for CacheManager.
	/// </summary>
	public class CacheManager : IFederationCache
	{
		Cache	_Cache;
		Cache TheCache
		{
			get
			{
				return _Cache;
			}
		}

		const string trackerKey = "___CacheRuleTrackerKey";
		Hashtable Tracker
		{
			get
			{
				object answer = _Cache[trackerKey];
				if (answer == null)
				{
					answer = new Hashtable();
					_Cache[trackerKey] = answer;
				}
				return (Hashtable)answer;
			}
		}

		public ICollection Keys
		{
			get
			{
				ArrayList answer = new ArrayList();
				IEnumerator en = TheCache.GetEnumerator();
				while (en.MoveNext())
					answer.Add(((DictionaryEntry)(en.Current)).Key);
				return answer;
			}
		}

		public object Get(string key)
		{
			return TheCache[key];
		}

		public CacheRule GetRuleForKey(string key)
		{
			return (CacheRule)Tracker[key];
		}

		public CacheManager(Cache aCache)
		{
			_Cache = aCache;
		}

		public object this[string key]
		{
			get
			{
				return Get(key);
			}
			set
			{
				Put(key, value);
			}
		}

		public void Put(string key, object val)
		{
			Put(key, val, null);
		}
	
		public void Put(string key, object val, CacheRule rule)
		{
			if (rule.IncludesNeverCacheRule)
				return;
			if (rule != null)
				TheCache.Insert(key, val, rule.GetCacheDependency());
			else
				TheCache.Insert(key, val);
			Tracker[key] = rule;
		}
	
	}
}
