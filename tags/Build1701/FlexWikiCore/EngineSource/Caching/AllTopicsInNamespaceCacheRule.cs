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


namespace FlexWiki
{
	/// <summary>
	/// Summary description for AllTopicsInNamespaceCacheRule.
	/// </summary>
	public class AllTopicsInNamespaceCacheRule : CacheRule, IHTMLRenderable
	{
		Federation _Federation;
		public AllTopicsInNamespaceCacheRule(Federation fed, string ns)
		{
			_Namespace = ns;
			_Federation = fed;
		}

		public override void SetupInvalidation(FederationCacheManager manager, string key)
		{
			manager.AddKeyToInvalidateOnTopicsInNamespaceChange(key, Namespace);

			/// For now we take a very aggressive invalidation approach that says that whenever
			/// the list of namespaces changes, we invalidate all cached topic lists.  This is because
			/// we dont' yet do the work to be more granular (e.g., figure out when a namespace is added 
			/// or removed exactly which namespaces are impacted).  This will likely cause a near flushing of
			/// the cache when namespaces are added or removed, but that should be pretty rare.  if it 
			/// becomes a serious problem, we can enhance this to be more granular.
			
			manager.AddKeyToInvalidateOnFederationNamespacesChange(key);
		}


		string _Namespace;
		public string Namespace
		{
			get
			{
				return _Namespace;
			}
		}

		public Federation Federation
		{
			get
			{
				return _Federation;
			}
		}

		public override System.Collections.ICollection AllLeafRules
		{
			get
			{
				return ArrayList.Repeat(this, 1);
			}
		}

		public override string Description 
		{
			get
			{
				return "THE-FEDERATION(" + _Federation.FederationNamespaceMapFilename + ")";
			}
		}

		public override bool Equals(object obj)
		{
			AllTopicsInNamespaceCacheRule other = obj as AllTopicsInNamespaceCacheRule;
			if (other == null)
				return false;
			if (!other.Federation.Equals(Federation))
				return false;
			if (other.Namespace != Namespace)
				return false;
			return true;
		}

		public override int GetHashCode()
		{
			int answer = 0;
			if (Federation != null)
				answer ^= Federation.GetHashCode();
			if (Namespace != null)
				answer ^= Namespace.GetHashCode();
			return answer;
		}
		#region IHTMLRenderable Members

		public void RenderToHTML(System.IO.TextWriter output)
		{
			output.WriteLine("<fieldset><legend>All topics in namespace</legend>" + Formatting.Formatter.EscapeHTML(Namespace) +"</fieldset>");
		}

		#endregion
	}
}
