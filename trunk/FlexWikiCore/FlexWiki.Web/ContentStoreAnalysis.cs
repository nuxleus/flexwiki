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
using System.Collections.Generic;

using FlexWiki.Collections; 

namespace FlexWiki.Web
{
  internal class ContentStoreAnalysis
  {
    private NamespaceManager _namespaceManager;

    public ContentStoreAnalysis(NamespaceManager aNamespaceManager)
    {
      _namespaceManager = aNamespaceManager;
      Analyze();
    }

    Set ocean;
    Hashtable topicToTopicAnalysis;
    ReferenceMap referenceMap;

    public TopicAnalysis AnalysisFor(QualifiedTopicRevision topic)
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
      referenceMap = _namespaceManager.GetReferenceMap(ExistencePolicy.ExistingOnly);

      foreach (string outerTopic in referenceMap.Keys)
      {
        // Response.Write("Consider: " + outerTopic + "<br>");
        Set islands = new Set();
        QualifiedTopicRevisionCollection linkedTopics = referenceMap[outerTopic];
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
        foreach (QualifiedTopicRevision linkedTopic in linkedTopics)
        {
          // Only analyze in this namespace
          if (linkedTopic.Namespace != _namespaceManager.Namespace)
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
        foreach (QualifiedTopicRevision linkedTopic in inNamespaceLinks)
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
            foreach (QualifiedTopicRevision eachTopic in eachIsland)
              ((TopicAnalysis)(topicToTopicAnalysis[eachTopic])).Island = newIsland;
          }
        }
      }
    }

  }
}
