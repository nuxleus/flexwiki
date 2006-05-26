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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki
{
    /// <summary>
    /// 
    /// </summary>
    public class FileSystemStore : ContentProviderBase
    {
        // Fields

        private static Regex s_historicalFileNameRegex = new Regex("[^(]+\\((?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?\\)");

        private DateTime _created = DateTime.Now;
        private DateTime _definitionTopicLastRead = DateTime.MinValue;
        private DateTime _lastRead;
        /// <summary>
        /// The file system path to the folder that contains the content
        /// </summary>
        private string _root;
        private FileSystemStoreState _fileSystemStoreState = FileSystemStoreState.New;

        // Constructors

        public FileSystemStore() : base(null)
        {
        }

        // Properties

        public string Root
        {
            get { return _root; }
        }

        /// <summary>
        /// Answer the file system path to the definition topic file
        /// </summary>
        private string DefinitionTopicFilePath
        {
            get
            {
                return Path.Combine(Root, DefinitionTopicFilename);
            }
        }
        /// <summary>
        /// Answer the file name for the definition topic file (without the path)
        /// </summary>
        private static string DefinitionTopicFilename
        {
            get
            {
                return NamespaceManager.DefinitionTopicName + ".wiki";
            }
        }
        private Federation Federation
        {
            get { return NamespaceManager.Federation; }
        }
        public override bool Exists
        {
            get
            {
                return Directory.Exists(Root);
            }
        }
        /// <summary>
        /// Implements <see cref="IContentProvider.IsReadOnly"/>.
        /// </summary>
        public override bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }
        public override DateTime LastRead
        {
            get
            {
                return _lastRead;
            }
        }

        // Methods

        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="stamp">Specifies that we are only interested in changes after this date.</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <returns>Enumeration of TopicChanges</returns>
        /// <remarks>Returns a collection with zero elements in it when the topic name does not exist, or has been deleted, 
        /// or if the calling user does not have the <see cref="Permission.Read"/> permission.</remarks>
        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            TopicChangeCollection answer = new TopicChangeCollection();

            FileInfo[] infos = FileInfosForTopic(topic.LocalName);
            ArrayList sortable = new ArrayList();
            foreach (FileInfo each in infos)
            {
                sortable.Add(new FileInfoTopicData(each, Namespace));
            }
            sortable.Sort(new TimeSort());

            foreach (TopicData each in sortable)
            {
                if (each.LastModificationTime < stamp)
                {
                    continue;
                }
                QualifiedTopicRevision name = new QualifiedTopicRevision(topic.ResolveRelativeTo(Namespace));
                name.Version = each.Version;
                TopicChange change = TopicChangeFromName(name);
                answer.Add(change);
            }
            return answer;
        }
        /// <summary>
        /// Delete a content base (kills all .wiki and .awiki files; removed the dir if empty)
        /// </summary>
        public override void DeleteAllTopicsAndHistory()
        {
            DirectoryInfo dir = new DirectoryInfo(Root);
            foreach (FileInfo each in dir.GetFiles("*.wiki"))
            {
                each.Delete();
            }
            foreach (FileInfo each in dir.GetFiles("*.awiki"))
            {
                each.Delete();
            }
            if (dir.GetFiles().Length == 0)
            {
                dir.Delete(true);
            }

            //OnFederationUpdated(new FederationUpdateEventArgs(update));
        }
        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public override void DeleteTopic(UnqualifiedTopicName topic)
        {
            string path = TopicPath(topic, null);
            if (!File.Exists(path))
            {
                return;
            }

            File.Delete(path);

            // Fire the event
            //OnFederationUpdated(new FederationUpdateEventArgs(update));
        }
        public static string ExtractTopicFromHistoricalFilename(string filename)
        {
            int p = filename.IndexOf("(");
            if (p == -1)
            {
                return filename;
            }
            return filename.Substring(0, p);
        }
        public static string ExtractVersionFromHistoricalFilename(string filename)
        {
            // ab(xyz)
            // 
            int p = filename.IndexOf("(");
            if (p == -1)
            {
                return filename;
            }
            int close = filename.LastIndexOf(")");
            return filename.Substring(p + 1, close - p - 1);
        }
        public override void Initialize(NamespaceManager namespaceManager)
        {
            base.Initialize(namespaceManager);

            _root = NamespaceManager.Parameters["root"].Value; 
        }
        /// <summary>
        /// Answer whether a topic exists and is writable
        /// </summary>
        /// <param name="topic">The topic (must directly be in this content base)</param>
        /// <returns>true if the topic exists AND is writable by the current user; else false</returns>
        public override bool IsExistingTopicWritable(UnqualifiedTopicName topic)
        {

            string path = TopicPath(topic, null);
            if (!File.Exists(path))
            {
                return false;
            }
            DateTime old = File.GetLastWriteTimeUtc(path);
            try
            {
                // Hacky implementation, but there's no better with the framework to do this that just to try and see what happens!!!
                FileStream stream = File.OpenWrite(path);
                stream.Close();
            }
            catch (UnauthorizedAccessException unauth)
            {
                unauth.ToString();
                return false;
            }
            File.SetLastWriteTimeUtc(path, old);
            return true;
        }
        /// <summary>
        /// Answer a TextReader for the given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
        /// <returns>TextReader</returns>
        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            string topicFile = TopicPath(topicRevision);
            if (topicFile == null || !File.Exists(topicFile))
            {
                throw TopicNotFoundException.ForTopic(topicRevision, Namespace);
            }
            return new StreamReader(new FileStream(topicFile, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        public static TopicChange TopicChangeFromName(QualifiedTopicRevision topic)
        {
            try
            {
                Regex re = new Regex("(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?");
                if (!re.IsMatch(topic.Version))
                {
                    throw new FormatException("Illegal wiki archive filename: " + topic.Version);
                }
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

        /// <summary>
        /// Answer true if a topic exists in this ContentProviderChain
        /// </summary>
        /// <param name="name">Name of the topic</param>
        /// <returns>true if it exists</returns>
        public override bool TopicExists(UnqualifiedTopicName topicName)
        {
            return File.Exists(MakePath(Root, topicName.LocalName));
        }


        /// <summary>
        /// Answer an enumeration of all topic in the ContentProviderChain, sorted using the supplied IComparer (does not include imports)
        /// </summary>
        /// <param name="comparer">Used to sort the topics in the answer</param>
        /// <returns>A collection of <see cref="QualifiedTopicName"> objects.</see></returns>
        protected QualifiedTopicNameCollection AllTopicsSorted(IComparer comparer)
        {
            throw new NotImplementedException();

            //ArrayList answer = new ArrayList();
            //ArrayList all = new ArrayList();
            //Set present = new Set();
            //foreach (FileInfo each in new DirectoryInfo(Root).GetFiles("*.wiki"))
            //{
            //    FileInfoTopicData td = new FileInfoTopicData(each, Namespace);
            //    all.Add(td);
            //    present.Add(td.Name);
            //}
            //foreach (BackingTopic each in NamespaceManager.BackingTopics.Values)
            //{
            //    BackingTopicData td = new BackingTopicData(each);
            //    if (present.Contains(td.Name))
            //    {
            //        continue;
            //    }
            //    all.Add(td);
            //}
            //if (comparer != null)
            //{
            //    all.Sort(comparer);
            //}
            //foreach (TopicData each in all)
            //{
            //    AbsoluteTopicName name = new AbsoluteTopicName(each.Name, each.Namespace);
            //    answer.Add(name);
            //}
            //return answer;
        }

        public override QualifiedTopicNameCollection AllTopics()
        {
            return AllTopicsSorted(null);
        }

        /// <summary>
        /// Write a new version of the topic (doesn't write a new version).  Generate all needed federation update changes via the supplied generator.
        /// </summary>
        /// <param name="topic">Topic to write</param>
        /// <param name="content">New content</param>
        /// <param name="sink">Object to recieve change info about the topic</param>
        public override void WriteTopic(UnqualifiedTopicRevision topicRevision, string content)
        {
            string root = Root;
            string fullpath = MakePath(root, topicRevision.LocalName);
            bool isNew = !(File.Exists(fullpath));

            // Change it
            using (StreamWriter sw = new StreamWriter(fullpath))
            {
                sw.Write(content);
            }

        }

        public override void WriteTopicAndNewVersion(UnqualifiedTopicName topic, string content, string author)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// All of the FileInfos for the historical versions of a given topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>FileInfos</returns>
        private FileInfo[] FileInfosForTopic(string topic)
        {
            FileInfo[] answer = { };

            // If the topic does not exist, we ignore any historical versions (the result of a delete)
            if (!TipFileExists(topic))
            {
                return answer;
            }

            try
            {
                answer = new DirectoryInfo(Root).GetFiles(topic + "(*).awiki");
            }
            catch (DirectoryNotFoundException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            return answer;
        }

        /// <summary>
        /// Answer the full file system path for a given topic in a given folder.  
        /// </summary>
        /// <param name="root">File system path to the root directory for the containing content base</param>
        /// <param name="name">The name of the topic</param>
        /// <returns>Full path to the file containing the content for the most recent version of the topic</returns>

        private static string MakePath(string root, string name)
        {
            return MakePath(root, name, null); 
        }

        private static string MakePath(string root, string name, string version)
        {
            if (version == null || version.Length == 0)
            {
                return Path.Combine(root, name + ".wiki");
            }
            else
            {
                return Path.Combine(root, name + "(" + version + ").awiki");
            }
        }

        private static string MakeTopicFilename(TopicRevision topic)
        {
            if (topic.Version == null)
            {
                return topic.LocalName + ".wiki";
            }
            else
            {
                return topic.LocalName + "(" + topic.Version + ").awiki";
            }
        }

        private void Read()
        {
            if (_fileSystemStoreState == FileSystemStoreState.Loading)
            {
                throw new Exception("Recursion problem: already loading ContentProviderChain for " + _root);
            }

            NamespaceManager.ImportedNamespaces.Clear();
            //NamespaceManager.Description = null;
            //NamespaceManager.Title = null;
            //NamespaceManager.ImageURL = null;
            //            NamespaceManager.Contact = null;

            _fileSystemStoreState = FileSystemStoreState.Loading;
            _lastRead = DateTime.Now;

            //NamespaceManager.DisplaySpacesInWikiLinks = false; // the default, applied if *.config key is missing and _ContentBaseDefinition propertyName is missing			
            // TODO: Change this so it doesn't work this way any more. Reading from AppSettings is a bad idea. 
            //if (System.Configuration.ConfigurationManager.AppSettings["DisplaySpacesInWikiLinks"] != null)
            //{
            //    NamespaceManager.DisplaySpacesInWikiLinks = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["DisplaySpacesInWikiLinks"]);
            //}

            string filename = DefinitionTopicFilePath;
            if (File.Exists(filename))
            {
                _definitionTopicLastRead = File.GetLastWriteTime(filename);

                string body;
                using (StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    body = sr.ReadToEnd();
                }

                // TODO: rewrite this to deal with the fact that ExtractExplicitFieldsFromTopicBody is now GetProperties
                Hashtable hash = null;
                //Hashtable hash = NamespaceManager.ExtractExplicitFieldsFromTopicBody(body);

                //NamespaceManager.Title = (string)hash["Title"];
                string homePage = (string)hash["HomePage"];
                if (homePage != null)
                {
                    NamespaceManager.HomePage = homePage;
                }
                //                NamespaceManager.Description = (string)hash["Description"];
                //NamespaceManager.ImageURL = (string)hash["ImageURL"];
                //NamespaceManager.Contact = (string)hash["Contact"];

                //if (hash.ContainsKey("DisplaySpacesInWikiLinks")) // _ContentBaseDefinition propertyName overrides *.config setting
                //{
                //    NamespaceManager.DisplaySpacesInWikiLinks = Convert.ToBoolean(hash["DisplaySpacesInWikiLinks"]);
                //}

                string importList = (string)hash["Import"];
                if (importList != null)
                {
                    foreach (string each in Federation.ParseListPropertyValue(importList))
                    {
                        NamespaceManager.ImportedNamespaces.Add(each);
                    }
                }
            }

            _fileSystemStoreState = FileSystemStoreState.Loaded;
        }

        private static int SafeIntegerParse(string input)
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

        private bool TipFileExists(string name)
        {
            return File.Exists(MakePath(Root, name));
        }

        private string TopicPath(UnqualifiedTopicRevision revision)
        {
            return TopicPath(revision.LocalName, revision.Version); 
        }

        private string TopicPath(UnqualifiedTopicName topic, string version)
        {
            return TopicPath(topic.LocalName, version); 
        }

        private string TopicPath(string topicName, string version)
        {
            return MakePath(Root, topicName, version);
        }

    }
}
