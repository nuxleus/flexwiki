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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for LostAndFound.
	/// </summary>
	public class LostAndFound : BasePage
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		protected void ShowPage()
		{
			Response.Write(@"
<fieldset><legend class='DialogTitle'>Lost And Found</legend>
<form id='Form'>");

			ArrayList uniqueNamespaces = new ArrayList(TheFederation.Namespaces);
			uniqueNamespaces.Sort();

			string preferredNamespace = Request.QueryString["namespace"];
			if (preferredNamespace == null)
				preferredNamespace  = DefaultNamespace;

			Response.Write("<p>Namespace:<br /><select title='to explore the list and found for another namespace, select it here' name='namespace' class='SearchColumnFilterBox' id='NamespaceFilter'>");
			foreach (string ns in uniqueNamespaces)
			{
				string sel = (ns == preferredNamespace) ? " selected " : "";
				Response.Write("<option " + sel + " value='"+ ns + "'>" + ns + "</option>");
			}
			Response.Write(@"</select> <input title='click here to explore the lost and found for the selected namespace' type='submit' ID='Go' Value='Change namespace' /></p></form>");		


			ContentBase cb = TheFederation.ContentBaseForNamespace(preferredNamespace);
			LinkMaker lm = TheLinkMaker;

			if (cb == null)
			{
				Response.Write(@"
<h1>Inaccessible namespace</h1>
<p>The namespace you have selected is not accessible.</p>");
			}
			else
			{
				Response.Write(@"
	<h1>Lost and Found</h1>
	<p>Below are listed pages that are not reachable from the home page of this namespace.</p>
	<p>Related pages (ones that link to each other) are listed together.  Bold topics are completely unreferenced.  Other topics are referenced, but only from within the related topic group.
	");
				ContentBaseAnalysis analysis = new ContentBaseAnalysis(cb);

				AbsoluteTopicName home = new AbsoluteTopicName(cb.HomePage, cb.Namespace);
				Response.Write("<ul>");
				foreach (Set eachIsland in analysis.Islands)
				{
					if (eachIsland.Contains(home))
						continue;		// skip the mainland!
					bool first = true;
					Response.Write("<li>");
					foreach (AbsoluteTopicName eachTopic in eachIsland)
					{
						ContentBaseAnalysis.TopicAnalysis tan = analysis.AnalysisFor(eachTopic);
						if (!first)
							Response.Write(", ");
						first = false;
						int refs = tan.RefCount;
						if (refs == 0)
							Response.Write("<b>");
						Response.Write("<a href='" + lm.LinkToTopic(eachTopic) + "'>" + eachTopic.Name + "</a>");
						if (refs == 0)
							Response.Write("</b>");
					}
					Response.Write("</li>");
				}
				Response.Write("</ul>");
			}
			Response.Write("</fieldset>");
		}
	}

	internal class ContentBaseAnalysis
	{
		ContentBase _ContentBase;

		public ContentBaseAnalysis(ContentBase aBase)
		{
			_ContentBase = aBase;
			Analyze();
		}

		Set ocean;
		Hashtable topicToTopicAnalysis;
		Hashtable referenceMap;

		public ContentBaseAnalysis.TopicAnalysis AnalysisFor(AbsoluteTopicName topic)
		{
			return (TopicAnalysis)(topicToTopicAnalysis[topic]);
		}

		public Set Islands
		{
			get
			{
				return ocean;
			}
		}

		void Analyze()
		{
			// Build the ocean
			//	- an ocean (set) of islands (set)
			//	- also a hash for TopicAnalysis (topic->{island set,refcount}) for quick check if already present
			ocean = new Set();
			topicToTopicAnalysis = new Hashtable();
			referenceMap = _ContentBase.AllReferencesByTopic(null, true);

			foreach (AbsoluteTopicName outerTopic in referenceMap.Keys)
			{
				// Response.Write("Consider: " + outerTopic + "<br>");
				Set islands = new Set();
				IList linkedTopics = (IList)(referenceMap[outerTopic]);
				// Response.Write("Linked topics count: " + linkedTopics.Count + "<br>");

				TopicAnalysis outerTopicAnalysis = (TopicAnalysis)(topicToTopicAnalysis[outerTopic]);
				if (outerTopicAnalysis == null)
				{
					outerTopicAnalysis = new TopicAnalysis();
					topicToTopicAnalysis[outerTopic] = outerTopicAnalysis;
					// Response.Write("Creating info for " + outerTopic.Name + "<br>");
				}
				else
				{
					// Response.Write("Found existing info for " + outerTopic.Name + "<br>");
					// Response.Write("[island = " + outerTopicAnalysis.Island + "<br>");
				}

				if (outerTopicAnalysis.Island != null)
					islands.Add(outerTopicAnalysis.Island);

				//	- foreach outer topic
				//		islands = new set
				//		foreach linked topic
				//			increment refcount for linked topic			
				//			if (linkedtopic is on an island)
				//				islands add that island
				Set inNamespaceLinks = new Set();
				foreach (AbsoluteTopicName linkedTopic in linkedTopics)
				{
					// Only analyze in this namespace
					if (linkedTopic.Namespace != outerTopic.Namespace)
					{
						// Response.Write("Skiping linked topic (" + linkedTopic.Name + ") because namespace doesn't match<br>");
						continue;
					}
					// Only do each topic once; have we seen this one?
					if (inNamespaceLinks.Contains(linkedTopic))
					{
						// Response.Write("Skiping linked topic (" + linkedTopic.Name + ") because seen before<br>");
						continue;
					}
					// Skip self-references
					if (linkedTopic.Equals(outerTopic))
					{
						continue;
					}

					inNamespaceLinks.Add(linkedTopic);
					TopicAnalysis linkedTopicAnalysis = (TopicAnalysis)(topicToTopicAnalysis[linkedTopic]);
					if (linkedTopicAnalysis == null)
					{
						linkedTopicAnalysis = new TopicAnalysis();
						topicToTopicAnalysis[linkedTopic] = linkedTopicAnalysis;
						// Response.Write("Creating info for " + linkedTopic.Name + "<br>");
					}
					else
					{
						// Response.Write("Found existing info for " + linkedTopic.Name + "<br>");
					}
					linkedTopicAnalysis.RefCount++;
					if (linkedTopicAnalysis.Island != null)
						islands.Add(linkedTopicAnalysis.Island);
				}

				//		if (islands is empty)
				//			create new island
				//			add outer topic and all linked topics
				//		else if (islands size == 1)
				//			add all links and the outer topic to that islands
				//		else
				//			// need to merge islands
				//			newset = merged set of all islands
				//			TopicAnalysiss and replace and of the old islands with the new island

				Set newIsland;
				if (islands.Count == 1)
					newIsland = (Set)(islands.First);	// if there's only one, we can just use that one
				else
				{
					newIsland = new Set();
					ocean.Add(newIsland);
				}
				// Add the island and the linkedTopics
				newIsland.Add(outerTopic);
				outerTopicAnalysis.Island = newIsland;
				foreach (AbsoluteTopicName linkedTopic in inNamespaceLinks)
				{
					newIsland.Add(linkedTopic);
					((TopicAnalysis)(topicToTopicAnalysis[linkedTopic])).Island = newIsland;
					// Response.Write("Placing " + linkedTopic.Name + "<br>");
				}
				// Now merge if there was originally more than one
				if (islands.Count > 1)
				{
					foreach (Set eachIsland in islands)
					{
						foreach (object o in eachIsland)
							newIsland.Add(o);
						ocean.Remove(eachIsland);
						// Now update all the pointers from the TopicAnalysiss
						foreach (AbsoluteTopicName eachTopic in eachIsland)
							((TopicAnalysis)(topicToTopicAnalysis[eachTopic])).Island = newIsland;
					}
				}
			}
		}

		public class TopicAnalysis
		{
			public TopicAnalysis()
			{
				RefCount = 0;
			}

			public int	RefCount;
			public Set Island;
		}
	}
}
