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
using System.Text;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for PropertyCacheRule.
	/// </summary>
	public class PropertyCacheRule : CacheRule, IHTMLRenderable
	{
		public PropertyCacheRule(Federation fed)
		{
			_Federation = fed;
		}

		public override ICollection AllLeafRules
		{
			get
			{
				ArrayList answer = new ArrayList();
				answer.Add(this);
				return answer;
			}
		}


		public PropertyCacheRule(Federation fed, string propertyName)
		{
			_Federation = fed;
			_PropertyName = propertyName;
		}

		string _PropertyName;
		public string PropertyName
		{
			get
			{
				return _PropertyName;
			}
		}

		Federation _Federation;

		public override void SetupInvalidation(FederationCacheManager manager, string key)
		{
			manager.AddKeyToInvalidateOnPropertyChange(key, PropertyName);

			/// For now we take a very aggressive invalidation approach that says that whenever
			/// the list of namespaces changes, we invalidate all cached properties.  This is because
			/// we dont' yet do the work to be more granular (e.g., figure out when a namespace is added 
			/// or removed exactly which properties are impacted).  This will likely cause a near flushing of
			/// the cache when namespaces are added or removed, but that should be pretty rare.  if it 
			/// becomes a serious problem, we can enhance this to be more granular.
			
			manager.AddKeyToInvalidateOnFederationNamespacesChange(key);
		}

		public override string Description 
		{
			get
			{
				return PropertyName;
			}
		}

		public override bool Equals(object obj)
		{
			PropertyCacheRule other = obj as PropertyCacheRule;
			if (other == null)
				return false;
			if (!other._Federation.Equals(_Federation))
				return false;
			if (other._PropertyName != _PropertyName)
				return false;
			return true;
		}

		public override int GetHashCode()
		{
			int answer = 0;
			if (_Federation != null)
				answer ^= _Federation.GetHashCode();
			if (_PropertyName != null)
				answer ^= _PropertyName.GetHashCode();
			return answer;
		}
		#region IHTMLRenderable Members

		public void RenderToHTML(System.IO.TextWriter output)
		{
			output.WriteLine("<fieldset><legend>Property</legend>" + Formatting.Formatter.EscapeHTML(_PropertyName) +"</fieldset>");
		}

		#endregion
	}
}
