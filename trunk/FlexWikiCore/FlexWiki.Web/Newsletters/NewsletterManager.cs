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
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.Mail;

using FlexWiki;
using FlexWiki.Formatting;
using FlexWiki.Web;

namespace FlexWiki.Web.Newsletters
{
    /// <summary>
    /// Summary description for NewsletterManager.
    /// </summary>



    public class NewsletterManager
    {
        private IDeliveryBoy _DeliveryBoy;
        private LinkMaker _LinkMaker;
        private Federation _Federation;
        private string _NewslettersFrom;
        private string _HeadInsert;

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

        private LinkMaker LinkMaker()
        {
            return _LinkMaker;
        }

        public IDeliveryBoy DeliveryBoy()
        {
            return _DeliveryBoy;
        }

        private TextWriter _Log;

        private void LogLine(string s)
        {
            if (_Log == null)
                return;
            _Log.WriteLine(s);
        }

        private void Log(string s)
        {
            if (_Log == null)
                return;
            _Log.Write(s);
        }

        public void Notify(TextWriter log)
        {
            _Log = log;
            // Troll through all the newsletters and see if any of them need an update
            foreach (QualifiedTopicRevision each in GetAllNewsletterNames())
            {
                LogLine("Checking newsletter: " + each);
                DateTime nextUpdate = DateTime.MaxValue;
                DateTime lastUpdate = this.GetLastUpdateForNewsletter(each);
                LogLine("\tLast newsletter update - " + lastUpdate);
                if (!IsNewsletterDueForUpdate(each, out nextUpdate))
                {
                    LogLine("\tnot due for update - " + nextUpdate);
                    continue;
                }
                LogLine("\tdue for update");
                LogLine("\tcollecting changes");
                IEnumerable changes = AllChangesForNewsletterSince(each, lastUpdate);
                IEnumerator e = changes.GetEnumerator();
                if (!e.MoveNext())
                {
                    LogLine("\tno changes; skipping");
                    SetLastUpdateForNewsletter(each, DateTime.Now);
                    continue;	// no changes
                }
                LogLine("\tchanges found; sending newsletter");
                SendNewsletterUpdate(each, changes, log);
                SetLastUpdateForNewsletter(each, DateTime.Now);
            }
        }

        public string GetDescriptionForNewsletter(QualifiedTopicRevision newsletter)
        {
            return Federation.GetTopicPropertyValue(newsletter, "Description");
        }

        private void GenerateAndDeliverNewsletter(QualifiedTopicRevision newsletter, IEnumerable changes, TextWriter log)
        {
            string description = GetDescriptionForNewsletter(newsletter);
            string news = BuildArbitraryNewsletter(newsletter.LocalName, LinkMaker().LinkToTopic(newsletter),
                AllTopicsForNewsletter(newsletter), GetLastUpdateForNewsletter(newsletter), _HeadInsert, description, newsletter.Namespace);
            DeliverNewsletterToAllSubscribers(newsletter, news, Federation.GetTopicPropertyValue(newsletter, "Subscribers"), log);
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
                builder.Append("<div class='NewsletterDescription'>" + Formatter.EscapeHTML(description) + "</div>\n");

            // Organize all changes based on topic
            Hashtable changeMap = new Hashtable();
            foreach (TopicChange each in AllChangesForTopicsSince(topics, DateTime.MinValue))
            {
                ArrayList list;
                QualifiedTopicRevision nameWithoutVersion = new QualifiedTopicRevision(each.TopicRevision.DottedName);
                if (!changeMap.ContainsKey(nameWithoutVersion))
                    changeMap[nameWithoutVersion] = new ArrayList();
                list = (ArrayList)(changeMap[nameWithoutVersion]);
                list.Add(each);
            }

            Hashtable immediatelyPreviousVersions = new Hashtable();

            builder.Append("<div class='NewsletterTOCHeader'>Table of Contents</div>");

            builder.Append("<table class='NewsletterTOCTable' border=0 cellpadding=2 cellspacing=0>");
            builder.Append("<tr>");
            builder.Append("<td class='NewsletterTOCHeaderCell'>" + "Topic" + "</td>");
            builder.Append("<td class='NewsletterTOCHeaderCell'>" + "Changes" + "</td>");
            builder.Append("<td class='NewsletterTOCHeaderCell'>" + "Most Recent" + "</td>");
            builder.Append("</tr>");


            foreach (QualifiedTopicRevision each in topics)
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
                    if (c.Created > since)
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
                string appearAs = (homeNamespace == each.Namespace) ? each.LocalName : each.DottedName;
                TopicChange newestChange = (TopicChange)(changesForThisTopic[0]);

                builder.Append("<tr>");
                builder.Append("<td class='NewsletterTOCBodyCell'>" + "<div class='NewsletterTableOfContentsChangedTopicName'><a href='#" + each.DottedName + "'>" + appearAs + "</a></div>" + "</td>");
                builder.Append("<td class='NewsletterTOCBodyCell'>" + changesForThisTopic.Count + "</td>");
                builder.Append("<td class='NewsletterTOCBodyCell'>" + newestChange.Created.ToString() + "</td>");
                builder.Append("</tr>");

            }
            builder.Append("</table>");

            builder.Append("<br /><div class='NewsletterTOCFinsher'>&nbsp;</div>");

            foreach (QualifiedTopicRevision each in topics)
            {
                if (!changeMap.ContainsKey(each))
                    continue;

                ArrayList changesForThisTopic = (ArrayList)(changeMap[each]);
                string appearAs = (homeNamespace == each.Namespace) ? each.LocalName : each.DottedName;

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
                        changedBy = "changed by: " + c.Author;
                    else
                        changedBy += ", " + c.Author;
                }


                builder.AppendFormat("<div class='NewsletterTopicName'>");
                builder.AppendFormat("<a name='#" + each.DottedName + "'>{0}</a>", "<a href='" + lm.LinkToTopic(each) + "'>" + appearAs + "</a>");
                builder.AppendFormat(" (");
                builder.AppendFormat("<span class='NewsletterTopicChangers'>{0}</span>", Formatter.EscapeHTML(changedBy));
                builder.AppendFormat(" )");
                builder.AppendFormat("</div>");

                builder.Append("<div class='NewsletterTopicBody'>");
                try
                {
                    builder.Append(Formatter.FormattedTopicWithSpecificDiffs(newestChange.TopicRevision, OutputFormat.HTML, oldestChange.TopicRevision, Federation, lm));
                }
                catch (Exception ex)
                {
                    builder.Append(@"<p><b>An exception occurred while formatting this topic:</b> " + HtmlWriter.Escape(ex.ToString(), true) + "</p>");
                }
                builder.Append("</div>");
            }


            //			builder.Append("<div class='NewsletterInformationHeader'>Newsletter Information</div>\n");
            QualifiedTopicRevision homeTopic = new QualifiedTopicRevision(Federation.NamespaceManagerForNamespace(homeNamespace).HomePage, homeNamespace);
            builder.AppendFormat("<div class='NewsletterDeliveredBy'>Newsletter generated by <a href='{0}'>FlexWiki</a></div>", LinkMaker().LinkToTopic(homeTopic));

            builder.Append("</div>\n");

            builder.Append(@"</body>
</html>");

            return builder.ToString();
        }

        // Updated 2004-01-29 by CraigAndera
        public IEnumerable AllTopicsForNewsletter(QualifiedTopicRevision newsletter)
        {
            // HybridDictionary switches between using a ListDictionary and a Hashtable 
            // depending on the size of the collection - should be a good choice since we don't
            // know how big the collection of topic names will be 
            Dictionary<string, TopicName> answer = new Dictionary<string, TopicName>();

            NamespaceManager namespaceManager = NamespaceManagerForNewsletter(newsletter);

            foreach (string s in Federation.GetTopicListPropertyValue(newsletter, "Topics"))
            {
                // If the wildcard appears, ignore all the other topics listed - include every topic
                if (s == "*")
                {
                    answer.Clear();
                    foreach (TopicName topic in namespaceManager.AllTopics(ImportPolicy.DoNotIncludeImports))
                    {
                        answer.Add(topic.DottedName, topic);
                    }
                    // No need to continue iterating after we find the wildcard 
                    break;
                }
                else
                {
                    foreach (TopicName topic in namespaceManager.AllQualifiedTopicNamesThatExist(s))
                    {
                        answer.Add(topic.DottedName, topic);
                    }
                }
            }

            // Now we need to remove any topics that appear in the Exclude propertyName
            foreach (string s in Federation.GetTopicListPropertyValue(newsletter, "Exclude"))
            {
                foreach (TopicName topic in namespaceManager.AllQualifiedTopicNamesThatExist(s))
                {
                    answer.Remove(topic.DottedName);
                }
            }

            // Do the same for "Excludes", since it's hard to remember which one to use
            foreach (string s in Federation.GetTopicListPropertyValue(newsletter, "Excludes"))
            {
                foreach (TopicName topic in namespaceManager.AllQualifiedTopicNamesThatExist(s))
                {
                    answer.Remove(topic.DottedName);
                }
            }

            return answer.Values;
        }


        private NamespaceManager NamespaceManagerForNewsletter(QualifiedTopicRevision newsletter)
        {
            return Federation.NamespaceManagerForNamespace(newsletter.Namespace);
        }

        public IEnumerable AllChangesForNewsletterSince(QualifiedTopicRevision newsletter, DateTime since)
        {
            LogLine("\tAllChangesForNewsletterSince since(" + since.ToString() + ")");
            return AllChangesForTopicsSince(AllTopicsForNewsletter(newsletter), since);
        }

        public IEnumerable AllChangesForTopicsSince(IEnumerable topics, DateTime since)
        {
            ArrayList answer = new ArrayList();
            foreach (QualifiedTopicRevision each in topics)
            {
                NamespaceManager namespaceManager = Federation.NamespaceManagerForTopic(each);
                IEnumerable e = namespaceManager.AllChangesForTopicSince(each.LocalName, since);
                int changeCount = 0;
                foreach (object obj in e)
                {
                    changeCount++;
                    answer.Add(obj);
                }
                LogLine("\tAllChangesForTopicsSince topic(" + each.DottedName + ") since(" + since.ToString() + ") - " + changeCount + " change(s)");
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
            foreach (NamespaceManager namespaceManager in Federation.NamespaceManagers)
            {
                if (namespaceFilter != null && namespaceManager.Namespace != namespaceFilter)
                    continue;
                foreach (string s in Federation.GetTopicListPropertyValue(namespaceManager.QualifiedTopicNameFor("WikiNewsletterIndex"), "Newsletters"))
                {
                    answer.AddRange(namespaceManager.AllQualifiedTopicNamesThatExist(s));
                }
            }
            return answer;
        }

        private bool IsNewsletterDueForUpdate(QualifiedTopicRevision newsletter, out DateTime nextUpdate)
        {
            DateTime lastUpdate = GetLastUpdateForNewsletter(newsletter);
            int updateFrequency = GetUpdateFrequencyForNewsletter(newsletter);
            DateTime updateDue = lastUpdate.AddMinutes(updateFrequency);
            nextUpdate = updateDue;
            return updateDue <= DateTime.Now;
        }
        private bool IsNewsletterDueForUpdate(QualifiedTopicRevision newsletter)
        {
            DateTime throwAway;
            return IsNewsletterDueForUpdate(newsletter, out throwAway);
        }

        // History for a topic is in the same namespace, called _NewsletterHistory
        private QualifiedTopicRevision NewsletterHistoryTopicFor(QualifiedTopicRevision newsletter)
        {
            QualifiedTopicRevision answer = new QualifiedTopicRevision(newsletter.DottedName);
            answer.LocalName = "_NewsletterHistory";
            return answer;
        }

        public DateTime GetLastUpdateForNewsletter(QualifiedTopicRevision newsletter)
        {
            string f = Federation.GetTopicPropertyValue(NewsletterHistoryTopicFor(newsletter), newsletter.LocalName + "_LastUpdate");
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

        private void SetLastUpdateForNewsletter(QualifiedTopicRevision newsletter, DateTime dt)
        {
            Federation.SetTopicProperty(NewsletterHistoryTopicFor(newsletter), newsletter.LocalName + "_LastUpdate", dt.ToString(), false, "NewsletterManager");
        }

        private int GetUpdateFrequencyForNewsletter(QualifiedTopicRevision newsletter)
        {
            string f = Federation.GetTopicPropertyValue(newsletter, "UpdateFrequency");
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

        private void SendNewsletterUpdate(QualifiedTopicRevision newsletter, IEnumerable changes, TextWriter log)
        {
            GenerateAndDeliverNewsletter(newsletter, changes, log);
        }

        private Federation Federation
        {
            get
            {
                return _Federation;
            }
        }

        private void DeliverNewsletterToAllSubscribers(QualifiedTopicRevision newsletter, string body, string subscribers, TextWriter log)
        {
            foreach (string each in Federation.ParseListPropertyValue(subscribers))
            {
                DeliverNewsletterToSubscriber(newsletter, body, each, log);
            }
        }

        private void DeliverNewsletterToSubscriber(QualifiedTopicRevision newsletter, string body, string address, TextWriter log)
        {
            string from = _NewslettersFrom;
            string to = address;
            string subject = newsletter + " update";
            DeliveryBoy().Deliver(to, from, subject, body);
        }

    }
}
