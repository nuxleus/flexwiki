using System;
using System.Collections;

using FlexWiki;

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for MockFederationCache.
	/// </summary>
	public class MockFederationCache: IFederationCache
	{
		private Hashtable cache;

		public MockFederationCache()
		{
			cache = new Hashtable();
		}

		#region IFederationCache Members

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
		}

		void FlexWiki.IFederationCache.Put(string key, object val)
		{
			cache[key] = val;		
		}

		public CacheRule GetRuleForKey(string key)
		{
			return new FilesCacheRule();
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
		}

		#endregion
	}
}
