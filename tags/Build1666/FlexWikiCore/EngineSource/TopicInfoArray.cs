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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for TopicInfoArray.
	/// </summary>
	[ExposedClass("TopicInfoArray", "Provides an array of topic info")]
	public class TopicInfoArray: BELArray, IHTMLRenderable
	{
		public TopicInfoArray(): base()
		{
		}

		public TopicInfoArray(ArrayList list): base(list)
		{
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer with the intersection of topics with the supplied array of topics (no duplicates).")]
		public TopicInfoArray Intersect(ArrayList topicInfoArrayToIntersect)
		{
			TopicInfoArray answer = new TopicInfoArray();
			this.Array.Sort(null);
			topicInfoArrayToIntersect.Sort(null);

			foreach(TopicInfo topicInfo in this.Array)
			{
				if(topicInfoArrayToIntersect.BinarySearch(topicInfo) >= 0)
				{
					if( answer.Array.BinarySearch(topicInfo) < 0 )
					{
						answer.Add(topicInfo);
					}
				}
			}

			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer with the union of topics with the supplied array of topics (no duplicates).")]
		public TopicInfoArray Union(ArrayList topicInfoArrayToIntersect)
		{
			TopicInfoArray answer = new TopicInfoArray();
			this.Array.Sort(null);
			topicInfoArrayToIntersect.Sort(null);

			foreach(TopicInfo topicInfo in this.Array)
			{
				if( answer.Array.BinarySearch(topicInfo) < 0 )
				{
					answer.Add(topicInfo);
				}
			}

			foreach(TopicInfo topicInfo in topicInfoArrayToIntersect)
			{
				if( answer.Array.BinarySearch(topicInfo) < 0 )
				{
					answer.Add(topicInfo);
				}
			}

			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer with the difference of topics with the supplied array of topics (no duplicates).")]
		public TopicInfoArray Difference(ArrayList topicInfoArrayToIntersect)
		{
			TopicInfoArray answer = new TopicInfoArray();
			this.Array.Sort(null);
			topicInfoArrayToIntersect.Sort(null);

			foreach(TopicInfo topicInfo in this.Array)
			{
				if(topicInfoArrayToIntersect.BinarySearch(topicInfo) < 0)
				{
					if( answer.Array.BinarySearch(topicInfo) < 0 )
					{
						answer.Add(topicInfo);
					}
				}
			}

			return answer;
		}
		
		#region IHTMLRenderable Members

		public void RenderToHTML(System.IO.TextWriter output)
		{
			output.WriteLine("<table width='100%' border=1 cellpadding=3 cellspacing=0>");
			foreach(TopicInfo each in Array)
			{
				output.WriteLine("<tr><td valign='top'>");
				IHTMLRenderable child = each as IHTMLRenderable;
				if (child == null)
					output.Write(FlexWiki.Formatting.Formatter.EscapeHTML(each.ToString()) + "<br />");
				else
					child.RenderToHTML(output);
				output.WriteLine("</tr>");
			}
			output.WriteLine("</table>");
		}

		#endregion
	}
}
