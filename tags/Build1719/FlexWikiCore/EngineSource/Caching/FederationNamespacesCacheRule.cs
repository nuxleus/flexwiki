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
	/// Summary description for FederationNamespacesCacheRule.
	/// </summary>
	public class FederationNamespacesCacheRule : CacheRule, IHTMLRenderable
	{
		Federation _Federation;
		public FederationNamespacesCacheRule(Federation fed)
		{
			_Federation = fed;
		}

		public override System.Collections.ICollection AllLeafRules
		{
			get
			{
				return ArrayList.Repeat(this, 1);
			}
		}

		public override void SetupInvalidation(FederationCacheManager manager, string key)
		{
			manager.AddKeyToInvalidateOnFederationNamespacesChange(key);
		}


		public override string Description 
		{
			get
			{
				return "FEDERATION-NAMESPACES(" + _Federation.FederationNamespaceMapFilename + ")";
			}
		}

		public override bool Equals(object obj)
		{
			FederationNamespacesCacheRule other = obj as FederationNamespacesCacheRule;
			if (other == null)
				return false;
			if (!other._Federation.Equals(_Federation))
				return false;
			return true;
		}

		public override int GetHashCode()
		{
			int answer = 0;
			if (_Federation != null)
				answer ^= _Federation.GetHashCode();
			return answer;
		}
		#region IHTMLRenderable Members

		public void RenderToHTML(System.IO.TextWriter output)
		{
			output.WriteLine("<fieldset><legend>Federation namespace list</legend></fieldset>");
		}

		#endregion
	}
}
