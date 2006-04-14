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
using System.IO;
using System.Text.RegularExpressions;

using FlexWiki;
using FlexWiki.Collections; 

namespace FlexWiki.SqlProvider
{
    /// <summary>
    /// Summary description for SqlStore.
    /// </summary>
    public class SqlStore : IUnparsedContentProvider
    {
        // Nested Types

        /// <summary>
        /// IComparer for FileInfos
        /// </summary>
        protected class TimeSort : IComparer
        {
            public int Compare(object left, object right)
            {
                return ((TopicData)right).LastModificationTime.CompareTo(((TopicData)left).LastModificationTime);
            }
        }

        // Fields

        private DateTime _created = DateTime.Now;
        private string _connectionString;
        private DateTime _definitionTopicLastRead = DateTime.MinValue;
        private DateTime _lastRead;
        private NamespaceManager _storeManager;
        private SqlStoreState _state = SqlStoreState.New;

        public static Regex TopicNameRegex = new Regex("[^(]+\\((?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?\\)");

        // Constructors
        
        public SqlStore()
        {

        }

        // Public properties

        /// <summary>
        /// Sql Connection String for the namespace corresponding to this store.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
        }
        public NamespaceManager StoreManager
        {
            get { return _storeManager; }
            set { _storeManager = value; }
        }

        // Private properties
        
        private Federation Federation
        {
            get { return StoreManager.Federation; }
        }
        bool IContentProvider.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }
        private string Namespace
        {
            get { return StoreManager.Namespace; }
        }
        //
        // Public methods
        // 

        // 
        // Private methods
        // 
        DateTime IContentProvider.LastRead
        {
            get { return _lastRead; }
        }
        IUnparsedContentProvider IUnparsedContentProvider.Next
        {
            // This provider is always at the end of the chain.
            get { throw new NotSupportedException(); }
        }

        private static string MakeTopicName(string name)
        {
            return MakeTopicName(new LocalTopicVersionKey(name)); 
        }
        private static string MakeTopicName(LocalTopicVersionKey name)
        {
            if (name.Version == null || name.Version.Length == 0)
                return name.Name;
            else
                return name.Name + "(" + name.Version + ")";
        }
        /// <summary>
        /// Read the Namespace details from the store.
        /// </summary>
        private void Read()
        {
            if (_state == SqlStoreState.Loading)
            {
                throw new Exception("Recursion problem: already loading ContentProviderChain for " + Namespace);
            }

            StoreManager.ImportedNamespaces.Clear();
            //StoreManager.Description = null;
            //StoreManager.Title = null;
            //StoreManager.ImageURL = null;
            //StoreManager.Contact = null;

            _state = SqlStoreState.Loading;
            _lastRead = DateTime.Now;

            //StoreManager.DisplaySpacesInWikiLinks = false; // the default, applied if *.config key is missing and _ContentBaseDefinition propertyName is missing			
            // TODO: Pull the configuration out of this module and into the surrounding app
            if (System.Configuration.ConfigurationManager.AppSettings["DisplaySpacesInWikiLinks"] != null)
            {
                // TODO: Pull the configuration out of this module and into the surrounding app
                //StoreManager.DisplaySpacesInWikiLinks = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["DisplaySpacesInWikiLinks"]);
            }

            if (SqlHelper.TopicExists(Namespace, NamespaceManager.DefinitionTopicName, ConnectionString))
            {
                _definitionTopicLastRead = SqlHelper.GetTopicLastWriteTime(Namespace, NamespaceManager.DefinitionTopicName, ConnectionString);

                // TODO: Deal with the way properties are extracted now. 
                Hashtable hash = null; 
                //Hashtable hash = NamespaceManager.ExtractExplicitFieldsFromTopicBody(SqlHelper.GetTopicBody(Namespace, NamespaceManager.DefinitionTopicLocalName, ConnectionString));

                //StoreManager.Title = (string)hash["Title"];
                string homePage = (string)hash["HomePage"];
                if (homePage != null)
                {
                    StoreManager.HomePage = homePage;
                }
                //StoreManager.Description = (string)hash["Description"];
                //StoreManager.ImageURL = (string)hash["ImageURL"];
                //StoreManager.Contact = (string)hash["Contact"];

                if (hash.ContainsKey("DisplaySpacesInWikiLinks")) // _ContentBaseDefinition propertyName overrides *.config setting
                {
                    //StoreManager.DisplaySpacesInWikiLinks = Convert.ToBoolean(hash["DisplaySpacesInWikiLinks"]);
                }

                string importList = (string)hash["Import"];
                if (importList != null)
                {
                    foreach (string each in Federation.ParseListPropertyValue(importList))
                    {
                        StoreManager.ImportedNamespaces.Add(each);
                    }
                }
            }

            _state = SqlStoreState.Loaded;
        }





        void IContentProvider.Initialize(NamespaceManager storeManager)
        {
            _storeManager = storeManager;
            _connectionString = StoreManager.Parameters["Connection String"].Value;
            throw new NotImplementedException();

            //      SetFederation(aFed);
            //
            //      Namespace = ns;
            //
            //      _connectionString = connectionString;
            //
            //      _state = SqlStoreState.New;
            //      // Check if the namespace exists in the database
            //      // If does not exist then create the database.
            //      if( SqlNamespaceProvider.Exists(Namespace, ConnectionString) )
            //      {
            //        Read();
            //      }
            //      else
            //      {
            //        SqlHelper.CreateNamespace(Namespace, ConnectionString);
            //      }

        }

        bool IContentProvider.Exists
        {
            get { return SqlHelper.NamespaceExists(Namespace, _connectionString); }
        }

        /// <summary>
        /// Answer true if a topic exists in this ContentProviderChain
        /// </summary>
        /// <param name="name">Name of the topic</param>
        /// <returns>true if it exists</returns>
        bool IContentProvider.TopicExists(string name)
        {
            return SqlHelper.TopicExists(Namespace, MakeTopicName(name), _connectionString);
        }

        /// <summary>
        /// Answer whether a topic exists and is writable
        /// </summary>
        /// <param name="topic">The topic (must directly be in this content base)</param>
        /// <returns>true if the topic exists AND is writable by the current user; else false</returns>
        bool IContentProvider.IsExistingTopicWritable(string topic)
        {
            if (!SqlHelper.TopicExists(Namespace, topic, _connectionString))
            {
                return false;
            }

            return SqlHelper.IsExistingTopicWritable(Namespace, topic, _connectionString);
        }

        private SqlInfoForTopic[] SqlTopicInfosForTopic(LocalTopicVersionKey topic)
        {
            return SqlHelper.GetSqlTopicInfosForTopic(Namespace, topic.Name, _connectionString);
        }


        /// <summary>
        /// Answer a TextReader for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
        /// <returns>TextReader</returns>
        TextReader IContentProvider.TextReaderForTopic(string topic, string version)
        {
            string topicName = MakeTopicName(new LocalTopicVersionKey(topic, version));
            if (topicName == null || !SqlHelper.TopicExists(Namespace, topicName, _connectionString))
            {
                throw TopicNotFoundException.ForTopic(topic, Namespace);
            }
            return new StringReader(SqlHelper.GetTopicBody(Namespace, topicName, _connectionString));
        }

        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        void IContentProvider.DeleteTopic(string topic)
        {
            if (!SqlHelper.TopicExists(Namespace, topic, _connectionString))
            {
                return;
            }

            SqlHelper.DeleteTopic(Namespace, topic, _connectionString);

            //      // Fire the event
            //      FederationUpdate update = new FederationUpdate();
            //      update.RecordDeletedTopic(topic.AsAbsoluteTopicName(Namespace));
            //      OnFederationUpdated(new FederationUpdateEventArgs(update));

        }
        /// <summary>
        /// Answer an (unsorted) enumeration of all topic in the ContentProviderChain (doesn't include imports)
        /// </summary>
        /// <returns>Enumeration of AbsoluteTopicNames</returns>
        TopicNameCollection IContentProvider.AllTopics()
        {
            return AllTopicsSorted(null);
        }

        /// <summary>
        /// Answer an enumeration of all topic in the ContentProviderChain, sorted using the supplied IComparer (does not include imports)
        /// </summary>
        /// <param name="comparer">Used to sort the topics in the answer</param>
        /// <returns>Enumeration of AbsoluteTopicNames</returns>
        protected TopicNameCollection AllTopicsSorted(IComparer comparer)
        {
            throw new NotImplementedException();

            //ArrayList answer = new ArrayList();
            //ArrayList all = new ArrayList();
            //Set present = new Set();
            //foreach (SqlInfoForTopic each in SqlHelper.GetSqlTopicInfoForNonArchiveTopics(Namespace, comparer != null, _connectionString))
            //{
            //    SqlInfoTopicData td = new SqlInfoTopicData(each, Namespace);
            //    all.Add(td);
            //    present.Add(td.Name);
            //}
            //bool sortAgain = false;
            //foreach (BackingTopic each in StoreManager.BackingTopics.Values)
            //{
            //    BackingTopicData td = new BackingTopicData(each);
            //    if (present.Contains(td.Name))
            //        continue;
            //    sortAgain = true;
            //    all.Add(td);
            //}
            //if (comparer != null && sortAgain)
            //    all.Sort(comparer);
            //foreach (TopicData each in all)
            //{
            //    AbsoluteTopicName name = new AbsoluteTopicName(each.Name, each.Namespace);
            //    answer.Add(name);
            //}
            //return answer;
        }

        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="topic">A given date</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <param name="rule">A composite cache rule to fill with rules that represented accumulated dependencies (or null)</param>
        /// <returns>Enumeration of TopicChanges</returns>
        TopicChangeCollection IContentProvider.AllChangesForTopicSince(string topic, DateTime stamp)
        {
            TopicChangeCollection answer = new TopicChangeCollection();
            SqlInfoForTopic[] infos = SqlHelper.GetSqlTopicInfosForTopicSince(Namespace, topic, stamp, ConnectionString);
            ArrayList sortable = new ArrayList();
            foreach (SqlInfoForTopic each in infos)
            {
                sortable.Add(new SqlInfoTopicData(each, Namespace));
            }
            sortable.Sort(new TimeSort());

            foreach (TopicData each in sortable)
            {
                if (each.LastModificationTime < stamp)
                    continue;
                NamespaceQualifiedTopicVersionKey name = new NamespaceQualifiedTopicVersionKey(topic, Namespace);
                name.Version = each.Version;
                TopicChange change = TopicChangeFromName(name);
                answer.Add(change);
            }
            return answer;

        }

        static public TopicChange TopicChangeFromName(NamespaceQualifiedTopicVersionKey topic)
        {
            try
            {
                Regex re = new Regex("(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?");
                if (!re.IsMatch(topic.Version))
                    throw new FormatException("Illegal wiki archive filename: " + topic.Version);
                Match match = re.Match(topic.Version);
                // Format into: "2/16/1992 12:15:12";
                int frac = 0;
                if (match.Groups["fraction"] != null)
                {
                    string fracs = "0." + match.Groups["fraction"].Value;
                    try
                    {
                        Decimal f = Decimal.Parse(fracs, new System.Globalization.CultureInfo("en-US"));
                        frac = (int)(1000 * f);
                    }
                    catch (FormatException ex)
                    {
                        ex.ToString();

                        // shut up compiler;
                    }
                }

                DateTime ts = new DateTime(
                    SafeIntegerParse(match.Groups["year"].Value), // month
                    SafeIntegerParse(match.Groups["month"].Value), // day
                    SafeIntegerParse(match.Groups["day"].Value), // year
                    SafeIntegerParse(match.Groups["hour"].Value), // hour
                    SafeIntegerParse(match.Groups["minute"].Value), // minutes
                    SafeIntegerParse(match.Groups["second"].Value), // seconds
                    frac);
                return new TopicChange(topic, ts, match.Groups["name"].Value);
            }
            catch (Exception ex)
            {
                throw new Exception("Exception processing topic change " + topic.Version + " - " + ex.ToString());
            }
        }

        static int SafeIntegerParse(string input)
        {
            try
            {
                return Int32.Parse(input);
            }
            catch (FormatException)
            {
                return 0;
            }
        }

        /// <summary>
        /// Write a new version of the topic (doesn't write a new version).  Generate all needed federation update changes via the supplied generator.
        /// </summary>
        /// <param name="topic">Topic to write</param>
        /// <param name="content">New content</param>
        /// <param name="gen">Object to recieve change info about the topic</param>
        void IContentProvider.WriteTopic(string topic, string version, string content)
        {
            string topicName = MakeTopicName(topic);
            bool isNew = !(SqlHelper.TopicExists(Namespace, topicName, _connectionString));

            // Get old topic so we can analyze it for imports to compare with the new one
            string oldText = null;
            Hashtable oldProperties = null;
            if (!isNew)
            {
                oldText = SqlHelper.GetTopicBody(Namespace, topicName, _connectionString);
                // TODO: Deal with how this API has changed
                //oldProperties = NamespaceManager.ExtractExplicitFieldsFromTopicBody(oldText);
                throw new NotImplementedException(); 
            }

            string nameWithVersion = new LocalTopicVersionKey(topic, version).NameWithVersion; 
            SqlHelper.WriteTopic(Namespace, topicName, LastWriteTime(nameWithVersion), _connectionString, content, ((version != null && version.Length > 0) ? true : false));

            // Record changes
            RecordTopicChanges(new LocalTopicVersionKey(topic, version), isNew, content, oldText, oldProperties);
        }

        void IContentProvider.WriteTopicAndNewVersion(string topic, string content, string author)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// We need to extract the time from the Topic Name so as to do everything
        /// to avoid descrepancy between the time stamp in the topic name and Sql 
        /// the time stamp. The reason is that Sql supports only 1/300th of a second
        /// accuracy as opposed to 1/10000th of second accuracy in the topic name.
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        private DateTime LastWriteTime(string topicName)
        {
            int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;

            Match m = TopicNameRegex.Match(topicName);
            if (!m.Success)
                return DateTime.Now;
            if (m.Groups["year"].Captures.Count == 0)
                year = DateTime.Now.Year;
            else
                year = Int32.Parse(m.Groups["year"].Value);
            if (m.Groups["month"].Captures.Count == 0)
                month = DateTime.Now.Year;
            else
                month = Int32.Parse(m.Groups["month"].Value);
            if (m.Groups["day"].Captures.Count == 0)
                day = DateTime.Now.Year;
            else
                day = Int32.Parse(m.Groups["day"].Value);
            if (m.Groups["hour"].Captures.Count == 0)
                hour = DateTime.Now.Year;
            else
                if (m.Groups["minute"].Captures.Count == 0)
                    minute = DateTime.Now.Year;
                else
                    minute = Int32.Parse(m.Groups["minute"].Value);
            hour = Int32.Parse(m.Groups["hour"].Value);

            if (m.Groups["second"].Captures.Count == 0)
                second = DateTime.Now.Year;
            else
                second = Int32.Parse(m.Groups["second"].Value);
            if (m.Groups["fraction"].Captures.Count == 0)
                millisecond = DateTime.Now.Year;
            else
                millisecond = Int32.Parse(m.Groups["fraction"].Value.Substring(0, 3));

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        private void RecordTopicChanges(LocalTopicVersionKey topic, bool isNew, string content, string oldText, Hashtable oldProperties)
        {
            throw new NotImplementedException("Reenable if we figure out how to deal with updates.");
            /*
                  try
                  {
                      AbsoluteTopicName absTopic = topic.AsAbsoluteTopicName(Namespace);

                      gen.Push();

                      // Record the topic-level change
                      if (isNew)
                          gen.RecordCreatedTopic(absTopic);
                      else
                          gen.RecordUpdatedTopic(absTopic);

                      //	Now process the imports
                      Hashtable newProperties = NamespaceManager.ExtractExplicitFieldsFromTopicBody(content);
                      if (isNew)
                      {
                          foreach (string pName in newProperties.Keys)
                              gen.RecordPropertyChange(absTopic, pName, FederationUpdate.PropertyChangeType.PropertyAdd);
                          gen.RecordPropertyChange(absTopic, "_Body", FederationUpdate.PropertyChangeType.PropertyAdd);
                          gen.RecordPropertyChange(absTopic, "_TopicName", FederationUpdate.PropertyChangeType.PropertyAdd);
                          gen.RecordPropertyChange(absTopic, "_TopicFullName", FederationUpdate.PropertyChangeType.PropertyAdd);
                          gen.RecordPropertyChange(absTopic, "_LastModifiedBy", FederationUpdate.PropertyChangeType.PropertyAdd);
                          gen.RecordPropertyChange(absTopic, "_CreationTime", FederationUpdate.PropertyChangeType.PropertyAdd);
                          gen.RecordPropertyChange(absTopic, "_ModificationTime", FederationUpdate.PropertyChangeType.PropertyAdd);
                      }
                      else
                      {
                          if (content != oldText)
                          {
                              FillFederationUpdateByComparingPropertyHashes(gen, absTopic, oldProperties, newProperties);
                              gen.RecordPropertyChange(absTopic, "_Body", FederationUpdate.PropertyChangeType.PropertyUpdate);
                          }
                          gen.RecordPropertyChange(absTopic, "_ModificationTime", FederationUpdate.PropertyChangeType.PropertyUpdate);				
                      }
                  }
                  finally
                  {
                      gen.Pop();
                  }
            */
        }

        ///// <summary>
        ///// Rename the given topic.  If requested, find references and fix them up.  Answer a report of what was fixed up.  Throw a DuplicationTopicException
        ///// if the new name is the name of a topic that already exists.
        ///// </summary>
        ///// <param name="oldName">Old topic name</param>
        ///// <param name="newName">The new name</param>
        ///// <param name="fixup">true to fixup referenced topic *in this namespace*; false to do no fixups</param>
        ///// <returns>ArrayList of strings that can be reported back to the user of what happened during the fixup process</returns>
        //ArrayList IContentProvider.RenameTopic(LocalTopicName oldName, string newName, bool fixup, string author)
        //{
        //    //FederationUpdateGenerator gen = CreateFederationUpdateGenerator();

        //    // TRIGGER
        //    ArrayList answer = new ArrayList();
        //    string currentTopicName = oldName.Name;
        //    string newTopicName = newName;
        //    AbsoluteTopicName newFullName = new AbsoluteTopicName(newName, Namespace);

        //    // Make sure it's not goign to overwrite an existing topic
        //    if (StoreManager.TopicExists(new LocalTopicName(newName), ImportPolicy.DoNotIncludeImports))
        //    {
        //        throw DuplicateTopicException.ForTopic(newFullName);
        //    }

        //    // If the topic does not exist (e.g., it's a backing topic), don't bother...
        //    if (!TipTopicRecordExists(oldName.Name))
        //    {
        //        answer.Add("This topic can not be renamed (it is probably a backing topic).");
        //        return answer;
        //    }

        //    try
        //    {
        //        // TODO: Renable when updates get figured out
        //        //gen.Push();

        //        // Rename the archive files, too
        //        foreach (SqlInfoForTopic each in SqlTopicInfosForTopic(oldName))
        //        {
        //            AbsoluteTopicName newNameForThisVersion = new AbsoluteTopicName(newName, Namespace);
        //            newNameForThisVersion.Version = ExtractVersionFromTopicName(each.Name);

        //            AbsoluteTopicName oldNameForThisVersion = new AbsoluteTopicName(oldName.Name, Namespace);
        //            oldNameForThisVersion.Version = newNameForThisVersion.Version;

        //            SqlHelper.RenameTopic(Namespace, each.Name, MakeTopicName(newNameForThisVersion.LocalName), _connectionString);

        //            // record changes (a delete for the old one and an add for the new one)
        //            // TODO: Renable when updates get figured out
        //            //gen.RecordCreatedTopic(newNameForThisVersion);
        //            //gen.RecordDeletedTopic(oldNameForThisVersion);
        //        }

        //        // Rename the topic file
        //        SqlHelper.RenameTopic(Namespace, currentTopicName, newTopicName, _connectionString);

        //        // Record changes (a delete for the old one and an add for the new one)
        //        // TODO: Renable when updates get figured out
        //        //        gen.RecordCreatedTopic(newFullName);
        //        //				gen.RecordDeletedTopic(oldName.AsAbsoluteTopicName(Namespace));

        //        // Now get ready to do fixups
        //        if (!fixup)
        //            return answer;

        //        // OK, we need to do the hard work
        //        AbsoluteTopicName oldabs = oldName.AsAbsoluteTopicName(Namespace);
        //        AbsoluteTopicName newabs = new AbsoluteTopicName(newName, oldabs.Namespace);

        //        // Now the master loop
        //        foreach (AbsoluteTopicName topic in StoreManager.AllTopics(ImportPolicy.DoNotIncludeImports))
        //        {
        //            if (StoreManager.RenameTopicReferences(topic.LocalName, oldabs, newabs, author))
        //            {
        //                answer.Add("Found and replaced references in " + topic);
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        // TODO: Renable when updates get figured out
        //        //gen.Pop();
        //    }

        //    return answer;
        //}

        /// Delete a content base (kills everything inside recursively).  Note that this does *not* include unregistering
        /// the content base within the federation.
        /// </summary>
        void IContentProvider.DeleteAllTopicsAndHistory()
        {
            SqlHelper.DeleteNamespace(Namespace, _connectionString);
            //OnFederationUpdated(new FederationUpdateEventArgs(update));
        }

        private bool TipTopicRecordExists(string topicName)
        {
            return SqlHelper.TopicExists(Namespace, MakeTopicName(new LocalTopicVersionKey(topicName)), _connectionString);
        }

        public static string ExtractNameFromTopicName(string topicName)
        {
            int p = topicName.IndexOf("(");
            if (p == -1)
                return topicName;
            return topicName.Substring(0, p);
        }

        public static string ExtractVersionFromTopicName(string topicName)
        {
            // ab(xyz)
            // 
            int p = topicName.IndexOf("(");
            if (p == -1)
                return topicName;
            int close = topicName.LastIndexOf(")");
            return topicName.Substring(p + 1, close - p - 1);
        }

        

    }



}
