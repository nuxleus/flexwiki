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
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class CompositeWikiSequence : IOutputSequence
 
	{
		public CompositeWikiSequence()
		{
		}

		ArrayList _Children = new ArrayList();

		public void Add(IOutputSequence s)
		{
			_Children.Add(s);
		}

//		public IPresentation ToPresentation(IWikiToPresentation p)
//		{
//			CompositePresentation answer = new CompositePresentation();
//			foreach (IOutputSequence each in _Children)
//				answer.Add(each.ToPresentation(p));
//			return answer;
//		}

		public IPresentation ToPresentation(IWikiToPresentation p)
		{
			/// First, we get a linearized list of IOutputSequences.  These will be either WikiSequences or already Presentations.
			/// We stick them together into a single string with markers for all of the presentations.  We then call the IWikiToPresentation 
			/// which converts everything to a presentation.  We then break it apart at the markers and build a new composite presentation, 
			/// stitching in the original presentations where they were.
			ArrayList linearization = new ArrayList();
			AddAllTo(linearization);
			string marker = "0" + Guid.NewGuid().ToString("N");
			ArrayList presentations = new ArrayList();
			StringBuilder builder = new StringBuilder();
			foreach (IOutputSequence each in linearization)
			{
				IPresentation ip = each as IPresentation;
				if (ip != null)
				{
					builder.Append(marker);
					presentations.Add(ip);
					continue;
				} 
				WikiSequence wiki = each as WikiSequence;
				if (wiki == null)
					throw new Exception("WikiTalk implementation error.  Output sequences encountered that is neither an IPresentation nor a WikiSequence.");
				builder.Append(wiki.Value);
			}

			string pres = p.WikiToPresentation(builder.ToString());

			CompositePresentation answer = new CompositePresentation();
			int pidx = 0;
			while (pres.Length > 0)
			{
				// Get the next segment up to the marker (or the rest if there are no more markers)
				string part;
				int mark = pres.IndexOf(marker);
				if (mark == -1)
				{
					part = pres;
					pres = "";
				}
				else
				{
					part = pres.Substring(0, mark);
					pres = pres.Substring(mark + marker.Length, pres.Length - mark - marker.Length);
				}
				answer.Add(new StringPresenation(part));
				if (pidx < presentations.Count)
					answer.Add((IPresentation)presentations[pidx++]);
			}
			return answer;
		}

		public void AddAllTo(ArrayList list)
		{
			foreach (IOutputSequence each in _Children)
				each.AddAllTo(list);
		}

	}
}
