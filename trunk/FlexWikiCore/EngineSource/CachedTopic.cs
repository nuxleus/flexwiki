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
using System.Text;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for CachedTopic.
	/// </summary>
	public class CachedTopic : IHTMLRenderable
	{
		public CachedTopic(AbsoluteTopicName name)
		{
			_Name = name;
		}

		AbsoluteTopicName _Name;
		AbsoluteTopicName Name
		{
			get
			{
				return _Name;
			}
		}

		public string UnformattedContent;
		public IEnumerable Changes;
		public DateTime LastModified;
		public string LastModifiedBy;
		public DateTime CreationTime;
		public Hashtable Properties;

		public override string ToString()
		{
			StringBuilder b = new StringBuilder();
			b.Append("Name: " + Name.ToString() + Environment.NewLine);
			b.Append("Created: " + CreationTime.ToString() + Environment.NewLine);
			b.Append("Modified: " + LastModified.ToString() + Environment.NewLine);
			b.Append("Modified By: " + LastModifiedBy.ToString() + Environment.NewLine);
			b.Append("Content: " + UnformattedContent.ToString() + Environment.NewLine);
			return b.ToString();
		}
		#region IHTMLRenderable Members

		public void RenderToHTML(System.IO.TextWriter output)
		{
			output.WriteLine("<table cellspacing=\"0\" cellpadding=\"2\" border=\"0\">");
			output.WriteLine("<tr><td valign=\"top\">Name</td><td valign=\"top\">" + Formatting.Formatter.EscapeHTML(Name.FullnameWithVersion) + "</td></tr>");
			output.WriteLine("<tr><td valign=\"top\">Created</td><td valign=\"top\">" + Formatting.Formatter.EscapeHTML(CreationTime.ToString()) + "</td></tr>");
			output.WriteLine("<tr><td valign=\"top\">Modified</td><td valign=\"top\">" + Formatting.Formatter.EscapeHTML(LastModified.ToString()) + "</td></tr>");
			output.WriteLine("<tr><td valign=\"top\">Modified by</td><td valign=\"top\">" + Formatting.Formatter.EscapeHTML(LastModifiedBy.ToString()) + "</td></tr>");
			output.WriteLine("<tr><td valign=\"top\">Content</td><td valign=\"top\">" + Formatting.Formatter.EscapeHTML(UnformattedContent) + "</td></tr>");
			output.WriteLine("</table>");
		}

		#endregion
	}
}
