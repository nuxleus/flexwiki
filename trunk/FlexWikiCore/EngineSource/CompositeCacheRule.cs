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
using System.Text;
using System.Web.Caching;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for CompositeCacheRule.
	/// </summary>
	public class CompositeCacheRule : CacheRule
	{
		public CompositeCacheRule()
		{
		}

		ArrayList _Children = new ArrayList();

		public void Add(CacheRule aRule)
		{
			_Children.Add(aRule);
		}

		public override bool IncludesNeverCacheRule 
		{
			get
			{
				foreach (CacheRule each in _Children)
					if (each.IncludesNeverCacheRule)
						return true;
				return false;
			}
		}

		public override ICollection AllLeafRules
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (CacheRule each in _Children)
					answer.AddRange(each.AllLeafRules);
				return answer;
			}
		}


		public override CacheDependency GetCacheDependency(CacheDependency inner)
		{
			CacheDependency answer = inner;
			foreach (CacheRule each in _Children)
			{
				answer = each.GetCacheDependency(answer);
			}
			return answer;
		}

		public override string Description
		{
			get
			{
				StringBuilder b = new StringBuilder();
				b.Append("CompositeCacheRule(");
				foreach (CacheRule each in _Children)
				{
					if (b.Length > 0)
						b.Append(", ");
					b.Append(each.Description);
				}
				b.Append(")");
				return b.ToString();
			}
		}
		
	}
}
