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

using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for IFederationCache.
	/// </summary>
	public interface IFederationCache
	{
		ICollection Keys {get;}
		object Get(string key);
		CacheRule GetRuleForKey(string key);
		object this[string key] {get; set;}
		void Put(string key, object val);
		void Put(string key, object val, CacheRule rule);
	}
}
