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
	public class CompositeCacheRule : CacheRule, IHTMLRenderable
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

		public override void SetupInvalidation(FederationCacheManager manager, string key)
		{
				foreach (CacheRule each in _Children)
					each.SetupInvalidation(manager, key);
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


		public override string Description
		{
			get
			{
				StringBuilder b = new StringBuilder();
				b.Append("CompositeCacheRule(");
				bool first = true;
				foreach (CacheRule each in _Children)
				{
					if (!first)
						b.Append(", ");
					first = false;
					b.Append(each.Description);
				}
				b.Append(")");
				return b.ToString();
			}
		}
		#region IHTMLRenderable Members

		public void RenderToHTML(System.IO.TextWriter output)
		{
			output.WriteLine("<fieldset><legend>Composite</legend>");
			output.WriteLine("<table width=\"100%\" border=\"0\" cellpadding=\"3\" cellspacing=\"0\">");
			foreach (CacheRule each in _Children)
			{
				output.WriteLine("<tr><td valign=\"top\" class=\"CompositeCacheRuleChild\">");
				IHTMLRenderable child = (IHTMLRenderable)each;
				child.RenderToHTML(output);
				output.WriteLine("</tr>");
			}
			output.WriteLine("</table>");
			output.WriteLine("</fieldset>");
		}

//		public void RenderToHTML(System.IO.TextWriter output)
//		{
//			output.WriteLine("<table width='100%' border=0 cellpadding=3 cellspacing=0>");
//			foreach (CacheRule each in _Children)
//			{
//				output.WriteLine("<tr><td valign='top'>");
//				IHTMLRenderable child = (IHTMLRenderable)each;
//				child.RenderToHTML(output);
//				output.WriteLine("</tr>");
//			}
//			output.WriteLine("</table>");
//		}

		#endregion
	}
}
