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
using System.Web.Mail;
using System.IO;
using System.Collections;
using System.Collections.Specialized; 
using System.Text;
using FlexWiki;
using FlexWiki.Formatting;
using FlexWiki.Web;

namespace FlexWiki.Newsletters
{
	/// <summary>
	/// Summary description for NewsletterManager.
	/// </summary>
	public class NewsletterManager
	{
		IDeliveryBoy _DeliveryBoy;
		LinkMaker _LinkMaker;
		Federation _Federation;
		string _NewslettersFrom;
		string _HeadInsert;
		
		public NewsletterManager(Federation aFed, LinkMaker lm, IDeliveryBoy boy, string newslettersFrom, string headInsert)
		{
			_LinkMaker = lm;
			_NewslettersFrom = newslettersFrom;
			_Federation = aFed;
			_DeliveryBoy = boy;
			_HeadInsert = headInsert;
		}

		public NewsletterManager(Federation aFed, LinkMaker lm)
		{
			_LinkMaker = lm;
			_Federation = aFed;
			_HeadInsert = "";
		}

		LinkMaker LinkMaker()
		{
			return _LinkMaker;
		}

		public IDeliveryBoy DeliveryBoy()
		{
			return _DeliveryBoy;
		}

		TextWriter _Log;

		void LogLine(string s)
		{
			if (_Log == null)
				return;
			_Log.WriteLine(s);
		}

		void Log(string s)
		{
			if (_Log == null)
				return;
			_Log.Write(s);
		}

		public void Notify(TextWriter log)
		{
			_Log = log;
			// Troll through all the newsletters and see if any of them need an update
			foreach (AbsoluteTopicName each in GetAllNewsletterNames())
			{
				LogLine("Checking newsletter: " + each);
				LogLine("\tLast newsletter update - " + GetLastUpdateForNewsletter(each));
				if (!IsNewsletterDueForUpdate(each))
				{
					LogLine("\tnot due for update");
					continue;
				}
				LogLine("\tdue for update");
				LogLine("\tcollecting changes");
				IEnumerable changes = AllChangesForNewsletterSince(each, GetLastUpdateForNewsletter(each));
				IEnumerator e = changes.GetEnumerator();
				if (!e.MoveNext())
				{
					LogLine("\tno changes; skipping");
					continue;	// no changes
				}
				LogLine("\tchanges found; sending newsletter");
				SendNewsletterUpdate(each, changes, log);				
			}
		}

		public string GetDescriptionForNewsletter(AbsoluteTopicName newsletter)
		{
			return TheFederation.GetTopicProperty(newsletter, "Description");
		}

		void GenerateAndDeliverNewsletter(AbsoluteTopicName newsletter, IEnumerable changes, TextWriter log)
		{
			string description = GetDescriptionForNewsletter(newsletter);
			string news = BuildArbitraryNewsletter(newsletter.Name, LinkMaker().LinkToTopic(newsletter), 
				AllTopicsForNewsletter(newsletter), GetLastUpdateForNewsletter(newsletter), _HeadInsert, description, newsletter.Namespace);
			DeliverNewsletterToAllSubscribers(newsletter, news, TheFederation.GetTopicProperty(newsletter, "Subscribers"), log);
		}

		public string BuildArbitraryNewsletter(string newsletterTitle, string newsletterLink, IEnumerable topics, DateTime since, string headInsert, string description, string homeNamespace)
		{
			LogLine("\tBuild newsletter: " + newsletterTitle);
			StringBuilder builder = new StringBuilder();
			builder.Append(@"<html>
<head>" + headInsert + @"
        <meta name='Robots' content='NOINDEX, NOFOLLOW'>
</head>
<body class='NewsletterBody'>");

			LinkMaker lm = LinkMaker();

			if (newsletterTitle != null)
			{
				builder.Append("<div class='NewsletterName'>");
				if (newsletterLink != null)
					builder.Append("<a href='" + newsletterLink + "'>");
				builder.Append(newsletterTitle);
				builder.Append("</a>");
				builder.Append("</div>\n");
			}

			builder.Append("<div class='NewsletterInterior'>\n");
			if (description != null)
				builder.Append("<div class='NewsletterDescription'>" +  Formatter.EscapeHTML(description) + "</div>\n");

			// Organize all changes based on topic
			Hashtable changeMap = new Hashtable();
			foreach (TopicChange each in AllChangesForTopicsSince(topics, DateTime.MinValue))
			{
				ArrayList list;
				AbsoluteTopicName nameWithoutVersion = new AbsoluteTopicName(each.Topic.Fullname);
				if (!changeMap.ContainsKey(nameWithoutVersion))
					changeMap[nameWithoutVersion] = new ArrayList();
				list = (ArrayList)(changeMap[nameWithoutVersion]);
				list.Add(each);
			}

			Hashtable immediatelyPreviousVersions = new Hashtable();

			foreach (AbsoluteTopicName each in topics)
			{
				if (!changeMap.ContainsKey(each))
					continue;
				ArrayList changesForThisTopic = (ArrayList)(changeMap[each]);

				// Go through the changes and (1) find the version immediately prior 
				// to the cutoff time for each topic [so we can diff against it] and (2) remove 
				// all versions before the cutoff date
				for (int i = 0; i < changesForThisTopic.Count; )
				{
					TopicChange c = (TopicChange)changesForThisTopic[i];
					if (c.Timestamp > since)
					{
						i++;
						continue;
					}
					if (immediatelyPreviousVersions[each] == null)
						immediatelyPreviousVersions[each] = c;
					changesForThisTopic.RemoveAt(i);
				}
				
				if (changesForThisTopic.Count == 0)
				{
					// no changes in range -- remove it
					changeMap.Remove(each);
					continue;
				}
				string appearAs = (homeNamespace == each.Namespace) ? each.Name : each.Fullname;
				TopicChange newestChange = (TopicChange)(changesForThisTopic[0]);

				builder.Append("<div class='NewsletterTableOfContentsChangedTopicName'><a href='#" + each.Fullname + "'>" + appearAs + "</a></div>");
				builder.AppendFormat("<div  class='NewsletterTableOfContentsChangedTopicDetails'>Changed {0} time(s).  Latest change at {1}.</div>", changesForThisTopic.Count, newestChange.Timestamp.ToString());
			}

			foreach (AbsoluteTopicName each in topics)
			{
				if (!changeMap.ContainsKey(each))
					continue;

				ArrayList changesForThisTopic = (ArrayList)(changeMap[each]);
				string appearAs = (homeNamespace == each.Namespace) ? each.Name : each.Fullname;
				
				builder.AppendFormat("<div class='NewsletterTopicName'><a name='#" + each.Fullname + "'>{0}</a></div>", "<a href='" + lm.LinkToTopic(each) + "'>" + appearAs + "</a>");
				TopicChange newestChange = (TopicChange)(changesForThisTopic[0]);
				TopicChange oldestChange = (TopicChange)immediatelyPreviousVersions[each];
				if (oldestChange == null)
					oldestChange = newestChange;	// nothing prior
				
				string changedBy = null;
				ArrayList changers = new ArrayList();
				foreach (TopicChange c in changesForThisTopic)
				{
					if (changers.Contains(c.Author))
						continue;
					changers.Add(c.Author);
					if (changedBy == null)
						changedBy = "Changes by: " + c.Author;
					else
						changedBy += ", " + c.Author;
				}			
				
				builder.AppendFormat("<div class='NewsletterTopicChangers'>{0}</div>",  Formatter.EscapeHTML(changedBy));
				builder.Append("<div class='NewsletterTopicBody'>");
				builder.Append(Formatter.FormattedTopicWithSpecificDiffs(newestChange.Topic, OutputFormat.HTML, oldestChange.Topic, TheFederation, lm, null));
				builder.Append("</div>");
			}


			builder.Append("<div class='NewsletterInformationHeader'>Newsletter Information</div>\n");
			builder.AppendFormat("<div class='NewsletterInfoTopics'><b>Topics</b>: ");
			bool first = true;
			foreach (AbsoluteTopicName each in topics)
			{
				string appearAs = (homeNamespace == each.Namespace) ? each.Name : each.Fullname;
				if (!first)
					builder.Append(", ");
				builder.Append("<a href='" + lm.LinkToTopic(each) + "'>" + appearAs + "</a>");
				first = false;
			}
			builder.AppendFormat("</div>");
			AbsoluteTopicName homeTopic = new AbsoluteTopicName(TheFederation.ContentBaseForNamespace(homeNamespace).HomePage, homeNamespace);
			builder.AppendFormat("<div class='NewsletterDeliveredBy'>Newsletter generated by FlexWiki - see <a href='{0}'>{0}</a></div>", LinkMaker().LinkToTopic(homeTopic));

			builder.Append("</div>\n");

			builder.Append(@"</body>
</html>");

			return builder.ToString();
		}
		
    // Updated 2004-01-29 by CraigAndera
    public IEnumerable AllTopicsForNewsletter(AbsoluteTopicName newsletter)
    {
      // HybridDictionary switches between using a ListDictionary and a Hashtable 
      // depending on the size of the collection - should be a good choice since we don't
      // know how big the collection of topic names will be 
      HybridDictionary answer = new HybridDictionary(); 

      ContentBase cb = ContentBaseForNewsletter(newsletter);

      foreach (string s in TheFederation.GetTopicListPropertyValue(newsletter, "Topics"))
      {
        // If the wildcard appears, ignore all the other topics listed - include every topic
        if (s == "*")
        {
          answer.Clear(); 
          foreach (AbsoluteTopicName atn in cb.AllTopics(false))
          {
            answer.Add(atn.Fullname, atn); 
          }
          // No need to continue iterating after we find the wildcard 
          break; 
        }
        else
        {
          RelativeTopicName rel = new RelativeTopicName(s);
          foreach (AbsoluteTopicName atn in cb.AllAbsoluteTopicNamesThatExist(rel))
          {
            answer.Add(atn.Fullname, atn); 
          }
        }
      }

      // Now we need to remove any topics that appear in the Exclude field
      foreach (string s in TheFederation.GetTopicListPropertyValue(newsletter, "Exclude"))
      {
        RelativeTopicName rel = new RelativeTopicName(s);
        foreach (AbsoluteTopicName atn in cb.AllAbsoluteTopicNamesThatExist(rel))
        {
          answer.Remove(atn.Fullname); 
        }
      }

      // Do the same for "Excludes", since it's hard to remember which one to use
      foreach (string s in TheFederation.GetTopicListPropertyValue(newsletter, "Excludes"))
      {
        RelativeTopicName rel = new RelativeTopicName(s);
        foreach (AbsoluteTopicName atn in cb.AllAbsoluteTopicNamesThatExist(rel))
        {
          answer.Remove(atn.Fullname); 
        }
      }

      return answer.Values;
    }


		ContentBase ContentBaseForNewsletter(AbsoluteTopicName newsletter)
		{
			return TheFederation.ContentBaseForNamespace(newsletter.Namespace);
		}

		public IEnumerable AllChangesForNewsletterSince(AbsoluteTopicName newsletter, DateTime since)
		{
			LogLine("\tAllChangesForNewsletterSince since(" + since.ToString() + ")");
			return AllChangesForTopicsSince(AllTopicsForNewsletter(newsletter), since);
		}

		public IEnumerable AllChangesForTopicsSince(IEnumerable topics, DateTime since)
		{
			ArrayList answer = new ArrayList();
			foreach (AbsoluteTopicName each in topics)
			{
				ContentBase cb = TheFederation.DefaultContentBase;
				IEnumerable e = cb.AllChangesForTopicSince(each, since);   
				int changeCount = 0;
				foreach (object obj in e)
				{
					changeCount++;
					answer.Add(obj);
				}
				LogLine("\tAllChangesForTopicsSince topic(" + each.Fullname + ") since(" + since.ToString() + ") - " + changeCount + " change(s)");
			}
			return answer;
		}

		public IEnumerable GetAllNewsletterNames()
		{
			return GetAllNewsletterNames(null);
		}
		
		public IEnumerable GetAllNewsletterNames(string namespaceFilter)
		{
			ArrayList answer = new ArrayList();
			foreach (ContentBase cb in TheFederation.ContentBases)
			{
				if (namespaceFilter != null && cb.Namespace != namespaceFilter)
					continue;
				foreach (string s in TheFederation.GetTopicListPropertyValue(cb.TopicNameFor("WikiNewsletterIndex"), "Newsletters"))
				{
					RelativeTopicName rel = new RelativeTopicName(s);
					answer.AddRange(cb.AllAbsoluteTopicNamesThatExist(rel));
				}
			}
			return answer;
		}

		bool IsNewsletterDueForUpdate(AbsoluteTopicName newsletter)
		{
			DateTime lastUpdate = GetLastUpdateForNewsletter(newsletter);
			int updateFrequency = GetUpdateFrequencyForNewsletter(newsletter);
			DateTime updateDue = lastUpdate.AddMinutes(updateFrequency);
			return updateDue <= DateTime.Now;
		}

		// History for a topic is in the same namespace, called _NewsletterHistory
		AbsoluteTopicName NewsletterHistoryTopicFor(AbsoluteTopicName newsletter)
		{
			AbsoluteTopicName answer = new AbsoluteTopicName(newsletter.Fullname);
			answer.Name = "_NewsletterHistory";
			return answer;
		}

		public DateTime GetLastUpdateForNewsletter(AbsoluteTopicName newsletter)
		{
			string f = TheFederation.GetTopicProperty(NewsletterHistoryTopicFor(newsletter), newsletter.Name + "_LastUpdate");
			try
			{
				return DateTime.Parse(f);
			}
			catch (FormatException e)
			{
				e.ToString();
			}
			return DateTime.MinValue;	// The beginning of time(ish)
		}

		void SetLastUpdateForNewsletter(AbsoluteTopicName newsletter, DateTime dt)
		{
			TheFederation.SetTopicProperty(NewsletterHistoryTopicFor(newsletter), newsletter.Name + "_LastUpdate", dt.ToString(), false);
		}

		int GetUpdateFrequencyForNewsletter(AbsoluteTopicName newsletter)
		{
			string f = TheFederation.GetTopicProperty(newsletter, "UpdateFrequency");
			try
			{
				return Int32.Parse(f);
			}
			catch (FormatException e)
			{
				e.ToString();
			}
			return 60 * 3;	// Default to once per three hours (60 * 3 minutes)
		}

		void SendNewsletterUpdate(AbsoluteTopicName newsletter, IEnumerable changes, TextWriter log)
		{
			GenerateAndDeliverNewsletter(newsletter, changes, log);
			SetLastUpdateForNewsletter(newsletter, DateTime.Now);
		}

		Federation TheFederation
		{
			get
			{
				return _Federation;
			}
		}

		void DeliverNewsletterToAllSubscribers(AbsoluteTopicName newsletter, string body, string subscribers, TextWriter log)
		{
			foreach (string each in Federation.ParseListPropertyValue(subscribers))
			{
				DeliverNewsletterToSubscriber(newsletter, body, each, log);
			}
		}

		void DeliverNewsletterToSubscriber(AbsoluteTopicName newsletter, string body, string address, TextWriter log)
		{
			string from = _NewslettersFrom;
			string to = address;
			string subject = newsletter + " update";
			DeliveryBoy().Deliver(to, from, subject, body);
		}

	}
}
