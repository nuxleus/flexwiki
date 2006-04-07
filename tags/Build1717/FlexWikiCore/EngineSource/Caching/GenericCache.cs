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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for GenericCache.
	/// </summary>
	public class GenericCache: IFederationCache
	{
		private Hashtable cache;
		private Hashtable rules;

		public GenericCache()
		{
			Clear();
		}

		#region IFederationCache Members

		public void Clear()
		{
			cache = new Hashtable();
			rules = new Hashtable();
		}

		public object this[string key]
		{
			get
			{
				return cache[key];
			}
			set
			{
				cache[key] = value;
			}
		}

		public void Put(string key, object val, CacheRule rule)
		{
			cache[key] = val;
			rules[key] = rule;
		}

		void FlexWiki.IFederationCache.Put(string key, object val)
		{
			cache[key] = val;		
		}

		public CacheRule GetRuleForKey(string key)
		{
			return (CacheRule)(rules[key]);
		}

		public object Get(string key)
		{
			return cache[key];
		}

		public System.Collections.ICollection Keys
		{
			get
			{
				return cache.Keys;
			}
		}

		public void Remove(string key)
		{
			this.cache.Remove(key);
			if (rules.ContainsKey(key))
				rules.Remove(key);
		}

		#endregion
	}
}
