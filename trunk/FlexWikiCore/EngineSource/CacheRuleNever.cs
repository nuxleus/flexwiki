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
	public class CacheRuleNever : CacheRule, IHTMLRenderable
	{
		public CacheRuleNever()
		{
		}

		public override void SetupInvalidation(FederationCacheManager manager, string key)
		{

		}

		public override string Description
		{
			get
			{
				return "never cache";
			}
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

		public override bool Equals(object obj)
		{
			return obj is CacheRuleNever;
		}

		public override int GetHashCode()
		{
			return 0;	// all instances have the same hash
		}
		#region IHTMLRenderable Members

		public void RenderToHTML(System.IO.TextWriter output)
		{
			output.WriteLine("<fieldset><legend>Federation Properties</legend></fieldset>");
		}

		#endregion
	}
}
