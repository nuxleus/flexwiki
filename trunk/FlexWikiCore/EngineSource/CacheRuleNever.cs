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
using System.Web.Caching;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for CacheRuleNever.
	/// </summary>
	public class CacheRuleNever : CacheRule
	{
		public CacheRuleNever()
		{
		}

		public override string Description
		{
			get
			{
				return "never cache";
			}
		}

		public override CacheDependency GetCacheDependency(CacheDependency inner)
		{
			return null;
		}

		public override bool IncludesNeverCacheRule 
		{
			get
			{
				return true;
			}
		}

		public override ICollection AllLeafRules
		{
			get
			{
				return ArrayList.Repeat(this, 1);
			}
		}

	}
}
