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
using System.Diagnostics;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class FileSystemStore : ReadWriteStore
	{

		/// <summary>
		/// The file system path to the folder that contains the content
		/// </summary>
		string						_Root;
		
		override public DateTime	Created 
		{
			get
			{
				return _Created;
			}
		}

		override public DateTime	LastRead 
		{
			get
			{
				return _LastRead;
			}
		}

		DateTime _LastRead;
		DateTime _Created = DateTime.Now;
				
		/// <summary>
		/// Default constructor for XML Serialization
		/// </summary>
		public FileSystemStore()
		{
		}

		public FileSystemStore(Federation aFed, string ns, string root)
		{
			// For FileSystemStores, the only thing in the connection string is the root
			SetFederation(aFed);

			Namespace = ns;

			// Calculate the root -- taking into account the relative location of the directory if needed
			if (root.StartsWith(".\\"))
			{
				FileInfo fi = new FileInfo(aFed.FederationNamespaceMapFilename);
				root = fi.DirectoryName + "\\" + root.Substring(2);
			}
			_Root = root;
			_State = State.New;
			if (Directory.Exists(root))
			{
				Read();
			}
			else
			{
				try
				{
					Directory.CreateDirectory(root);
				} 
				catch (DirectoryNotFoundException e)
				{
					e.ToString();
				}
			}
		}



		/// <summary>
		/// Answer the file system path to the definition topic file
		/// </summary>
		string DefinitionTopicFilePath
		{
			get
			{
				return Root + "\\" + DefinitionTopicFilename;
			}
		}

		/// <summary>
		/// Answer the file name for the definition topic file (without the path)
		/// </summary>
		static string DefinitionTopicFilename
		{
			get
			{
				return DefinitionTopicLocalName + ".wiki";
			}
		}


		DateTime DefinitionTopicLastRead = DateTime.MinValue;

		override public void Validate()
		{
			if (!File.Exists(DefinitionTopicFilePath))
				return;
			DateTime lastmod = File.GetLastWriteTime(DefinitionTopicFilePath);
			if (DefinitionTopicLastRead >= lastmod)
				return;
			Read();
		}

		enum State
		{
			New, 
			Loading,
			Loaded
		};

		State	_State = State.New;

		void Read()
		{
			if (_State == State.Loading)
				throw new Exception("Recursion problem: already loading ContentBase for " + _Root);

			ImportedNamespaces = new ArrayList();
			Description = null;
			Title = null;
			ImageURL = null;
			Contact = null;

			_State = State.Loading;
			_LastRead = DateTime.Now;

			DisplaySpacesInWikiLinks = false; // the default, applied if *.config key is missing and _ContentBaseDefinition property is missing			
			if (System.Configuration.ConfigurationSettings.AppSettings["DisplaySpacesInWikiLinks"] != null)
				DisplaySpacesInWikiLinks = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["DisplaySpacesInWikiLinks"]);

			string filename = DefinitionTopicFilePath;
			if (File.Exists(filename))
			{
				DefinitionTopicLastRead = File.GetLastWriteTime(filename);

				string body;
				using (StreamReader sr = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
				{
					body = sr.ReadToEnd();
				}
				
				Hashtable hash = ExtractExplicitFieldsFromTopicBody(body);

				Title = (string)hash["Title"];
				string homePage = (string)hash["HomePage"];
				if (homePage != null)
				{
					HomePage = homePage;
				}
				Description = (string)hash["Description"];
				ImageURL = (string)hash["ImageURL"];
				Contact = (string)hash["Contact"];

				if(hash.ContainsKey("DisplaySpacesInWikiLinks")) // _ContentBaseDefinition property overrides *.config setting
					DisplaySpacesInWikiLinks = Convert.ToBoolean(hash["DisplaySpacesInWikiLinks"]);

				string importList = (string)hash["Import"]; 
				if (importList != null)
				{
					foreach (string each in Federation.ParseListPropertyValue(importList))
					{
						ImportedNamespaces.Add(each);
					}
				}
			}

			// Establish backing topics
			AbsoluteTopicName a;
			BackingTopic top;
			
			a = new AbsoluteTopicName((HomePage == null ? "HomePage" : HomePage), Namespace);
			top = new BackingTopic(a, DefaultHomePageContent, true);
			BackingTopics[a.Name] = top;
			
			a = new AbsoluteTopicName("_NormalBorders", Namespace);
			top = new BackingTopic(a, DefaultNormalBordersContent, true);
			BackingTopics[a.Name] = top;

			_State = State.Loaded;
		}

		/// <summary>
		/// The root directory for the content base
		/// </summary>
		public string Root
		{
			get
			{
				return _Root;
			}
		}

		override public bool Exists
		{
			get
			{
				return Directory.Exists(Root);
			}
		}

		string TopicPath(LocalTopicName localTopicName)
		{
			return MakePath(Root, localTopicName);
		}

		/// <summary>
		/// Answer the full file system path for a given topic in a given folder.  
		/// </summary>
		/// <param name="root">File system path to the root directory for the containing content base</param>
		/// <param name="name">The name of the topic</param>
		/// <returns>Full path to the file containing the content for the most recent version of the topic</returns>
		static string MakePath(string root, LocalTopicName name)
		{
			if (name.Version == null || name.Version.Length == 0)
				return root + "\\" + name.Name + ".wiki";
			else
				return root + "\\" + name.Name + "(" + name.Version + ").awiki";
		}
 
		/// <summary>
		/// Answer true if a topic exists in this ContentBase
		/// </summary>
		/// <param name="name">Name of the topic</param>
		/// <returns>true if it exists</returns>
		override public bool TopicExistsLocally(LocalTopicName name)
		{
			if (BackingTopics.ContainsKey(name.Name))
				return true;
			return File.Exists(MakePath(Root, name));
		}

		/// <summary>
		/// Answer when the topic was last changed
		/// </summary>
		/// <param name="topic">A topic name</param>
		/// <returns></returns>
		override public DateTime GetTopicLastWriteTime(LocalTopicName topic)
		{
			string path = TopicPath(topic);
			if (File.Exists(path))
				return File.GetLastWriteTime(path);
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				return back.LastModificationTime;
			throw TopicNotFoundException.ForTopic(topic, Namespace);
		}


		/// <summary>
		/// Answer whether a topic exists and is writable
		/// </summary>
		/// <param name="topic">The topic (must directly be in this content base)</param>
		/// <returns>true if the topic exists AND is writable by the current user; else false</returns>
		override public bool IsExistingTopicWritable(LocalTopicName topic)
		{

			string path = TopicPath(topic);
			if (!File.Exists(path))
			{
				BackingTopic back = GetBackingTopicNamed(topic);
				if (back == null)
					return false;
				return back.CanOverride;
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
		/// Answer when a topic was created
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <returns></returns>
		override public DateTime GetTopicCreationTime(LocalTopicName topic)
		{
			string path = TopicPath(topic);
			if (File.Exists(path))
				return File.GetCreationTime(path);
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				return back.CreationTime;
			throw TopicNotFoundException.ForTopic(topic, Namespace);			
		}

		/// <summary>
		/// Answer the identify of the author who last modified a given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <returns>a user name</returns>
		override public string GetTopicLastAuthor(LocalTopicName topic)
		{

			FileInfo[] infos = FileInfosForTopic(topic);
			if (infos.Length == 0)
			{
				BackingTopic back = GetBackingTopicNamed(topic);
				if (back != null)
					return back.LastAuthor;
				return AnonymousUserName;
			}

			ArrayList all = new ArrayList();
			foreach (FileInfo each in infos)
				all.Add(new FileInfoTopicData(each, Namespace));
			all.Sort(new TimeSort());
			TopicData info = (TopicData)(all[0]);
			string auth = info.Author;
			if (auth == null)
				return AnonymousUserName;
			return auth;
		}

		public static string ExtractTopicFromHistoricalFilename(string filename)
		{
			int p = filename.IndexOf("(");
			if (p == -1)
				return filename;
			return filename.Substring(0, p);
		}

		public static string ExtractVersionFromHistoricalFilename(string filename)
		{
			// ab(xyz)
			// 
			int p = filename.IndexOf("(");
			if (p == -1)
				return filename;
			int close = filename.LastIndexOf(")");
			return filename.Substring(p + 1, close - p - 1);
		}

		static string MakeTopicFilename(TopicName topic)
		{
			if (topic.Version == null)
				return topic.Name + ".wiki";
			else
				return topic.Name + "(" + topic.Version + ").awiki";
		}

		public static Regex HistoricalFileNameRegex = new Regex("[^(]+\\((?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?\\)");

		/// <summary>
		/// Answer a TextReader for the given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
		/// <returns>TextReader</returns>
		override public TextReader TextReaderForTopic(LocalTopicName topic)
		{
			string topicFile = TopicPath(topic);
			if (topicFile == null || !File.Exists(topicFile))
			{
				BackingTopic back = GetBackingTopicNamed(topic);
				if (back != null)
					return new StringReader(back.Body);
				throw TopicNotFoundException.ForTopic(topic, Namespace);
			}
			return new StreamReader(new FileStream(topicFile, FileMode.Open, FileAccess.Read, FileShare.Read));
		}

		/// <summary>
		/// Delete a topic
		/// </summary>
		/// <param name="topic"></param>
		override public void DeleteTopic(LocalTopicName topic)
		{
			string path = TopicPath(topic);
			if (!File.Exists(path))
				return;

			// Delete the sucker!
			File.Delete(path);

			// Fire the event
			FederationUpdate update = new FederationUpdate();
			update.RecordDeletedTopic(topic.AsAbsoluteTopicName(Namespace));
			OnFederationUpdated(new FederationUpdateEventArgs(update));
		}

		/// <summary>
		/// Answer an enumeration of all topic in the ContentBase, sorted using the supplied IComparer (does not include imports)
		/// </summary>
		/// <param name="comparer">Used to sort the topics in the answer</param>
		/// <returns>Enumeration of AbsoluteTopicNames</returns>
		protected IEnumerable AllTopicsSorted(IComparer comparer)
		{
			ArrayList answer = new ArrayList();
			ArrayList all = new ArrayList();
			Set present = new Set();
			foreach (FileInfo each in new DirectoryInfo(Root).GetFiles("*.wiki"))
			{
				FileInfoTopicData td = new FileInfoTopicData(each, Namespace);
				all.Add(td);
				present.Add(td.Name);
			}
			foreach (BackingTopic each in BackingTopics.Values)
			{
				BackingTopicTopicData td = new BackingTopicTopicData(each);
				if (present.Contains(td.Name))
					continue;
				all.Add(td);				
			}
			if (comparer != null)
				all.Sort(comparer);
			foreach (TopicData each in all)
			{
				AbsoluteTopicName name = new AbsoluteTopicName(each.Name, each.Namespace);
				answer.Add(name);
			}
			return answer;
		}

		/// <summary>
		/// Answer all of the versions for a given topic
		/// </summary>
		/// <remarks>
		/// TODO: Change this to return TopicChanges instead of the TopicNames
		/// </remarks>
		/// <param name="topic">A topic</param>
		/// <returns>Enumeration of the topic names (with non-null versions in them) </returns>
		override public IEnumerable AllVersionsForTopic(LocalTopicName topic)
		{
			ArrayList answer = new ArrayList();
			FileInfo[] infos = FileInfosForTopic(topic);
			ArrayList sortable = new ArrayList();
			foreach (FileInfo each in infos)
				sortable.Add(new FileInfoTopicData(each, Namespace));
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				sortable.Add(new BackingTopicTopicData(back));
			sortable.Sort(new TimeSort());
			foreach (TopicData each in sortable)
			{
				AbsoluteTopicName name = topic.AsAbsoluteTopicName(Namespace);
				name.Version = each.Version;
				answer.Add(name);
			}
			return answer;
		}

		/// <summary>
		/// Returns the most recent version for the given topic
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <returns>The most recent version string for the topic</returns>
		override public string LatestVersionForTopic(LocalTopicName topic)
		{				
			ArrayList sortable = new ArrayList();

			FileInfo [] infos = FileInfosForTopic(topic);
			foreach (FileInfo each in infos)
				sortable.Add(new FileInfoTopicData(each, Namespace));
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				sortable.Add(new BackingTopicTopicData(back));
			
			if (sortable.Count == 0)
				return null;
			sortable.Sort(new TimeSort());

			return ((TopicData)(sortable[0])).Version; 
		}

		bool TipFileExists(string name)
		{
			return File.Exists(MakePath(Root, new LocalTopicName(name)));
		}
    
		/// <summary>
		/// All of the FileInfos for the historical versions of a given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <returns>FileInfos</returns>
		FileInfo[] FileInfosForTopic(LocalTopicName topic)
		{
			FileInfo [] answer = {};

			// If the topic does not exist, we ignore any historical versions (the result of a delete)
			if (!TipFileExists(topic.Name))
				return answer;

			try
			{
				answer = new DirectoryInfo(Root).GetFiles(topic.Name + "(*).awiki");
			}
			catch (DirectoryNotFoundException e)
			{
				System.Diagnostics.Debug.WriteLine(e.ToString()); 
			}
			return answer;
		}

		/// <summary>
		/// A list of TopicChanges to a topic since a given date [sorted by date]
		/// </summary>
		/// <param name="topic">A given date</param>
		/// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
		/// <returns>Enumeration of TopicChanges</returns>
		override public IEnumerable AllChangesForTopicSince(LocalTopicName topic, DateTime stamp, CompositeCacheRule rule)
		{
			ArrayList answer = new ArrayList();
			FileInfo[] infos = FileInfosForTopic(topic);
			ArrayList sortable = new ArrayList();
			foreach (FileInfo each in infos)
				sortable.Add(new FileInfoTopicData(each, Namespace));
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				sortable.Add(new BackingTopicTopicData(back));
			sortable.Sort(new TimeSort());
			TopicsCacheRule tcr = null;
			if (rule != null)
			{
				tcr = new TopicsCacheRule(Federation);
				tcr.AddTopic(topic.AsAbsoluteTopicName(Namespace));
				rule.Add(tcr);
			}
			foreach (TopicData each in sortable)
			{
				if (each.LastModificationTime < stamp)
					continue;
				AbsoluteTopicName name = topic.AsAbsoluteTopicName(Namespace);
				name.Version = each.Version;
				TopicChange change = TopicChangeFromName(name);
				answer.Add(change);
				if (tcr != null)
					tcr.AddTopic(name.AsAbsoluteTopicName(Namespace));
			}
			return answer;
		}

		static public TopicChange TopicChangeFromName(AbsoluteTopicName topic)
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
		/// <param name="sink">Object to recieve change info about the topic</param>
		override protected void WriteTopic(LocalTopicName topic, string content, FederationUpdateGenerator gen)
		{
			string root = Root;
			string fullpath = MakePath(root, topic);
			bool isNew = !(File.Exists(fullpath));

			// Get old topic so we can analyze it for properties to compare with the new one
			string oldText = null;
			Hashtable oldProperties = null;
			if (!isNew)
			{
				using (StreamReader sr = new StreamReader(new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.Read)))
				{
					oldText = sr.ReadToEnd();
				}
				oldProperties = ExtractExplicitFieldsFromTopicBody(oldText);	
			}

			// Change it
			using (StreamWriter sw = new StreamWriter(fullpath))
			{
				sw.Write(content);
			}

			// Quick check to see if we're about to let somebody write the DefinitionTopic for this ContentBase.  
			// If so, we reset our Info object to reread
			string pathToDefinitionTopic = MakePath(Root, DefinitionTopicName.LocalName);
			if (fullpath == pathToDefinitionTopic)
				this.Federation.InvalidateNamespace(Namespace);

			// Record changes
			try
			{
				AbsoluteTopicName absTopic = topic.AsAbsoluteTopicName(Namespace);

				gen.Push();

				// Record the topic-level change
				if (isNew)
					gen.RecordCreatedTopic(absTopic);
				else
					gen.RecordUpdatedTopic(absTopic);

				//	Now process the properties
				Hashtable newProperties = ExtractExplicitFieldsFromTopicBody(content);
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
		}



		/// <summary>
		/// Rename the given topic.  If requested, find references and fix them up.  Answer a report of what was fixed up.  Throw a DuplicationTopicException
		/// if the new name is the name of a topic that already exists.
		/// </summary>
		/// <param name="oldName">Old topic name</param>
		/// <param name="newName">The new name</param>
		/// <param name="fixup">true to fixup referenced topic *in this namespace*; false to do no fixups</param>
		/// <returns>ArrayList of strings that can be reported back to the user of what happened during the fixup process</returns>
		override public ArrayList RenameTopic(LocalTopicName oldName, string newName, bool fixup)
		{
			FederationUpdateGenerator gen = CreateFederationUpdateGenerator();

			// TRIGGER
			ArrayList answer = new ArrayList();
			string root = Root;
			string pathToTopicFile =  root + "\\" + oldName.Name + ".wiki";
			string pathToArchiveFolder = root + "\\archive\\" + oldName.Name;
			string newNameForTopicFile = root + "\\" + newName + ".wiki";
			string newNameForArchiveFolder = root + "\\archive\\" + newName;
			AbsoluteTopicName newFullName = new AbsoluteTopicName(newName, Namespace);

			// Make sure it's not goign to overwrite an existing topic
			if (TopicExistsLocally(newName))
			{
				throw DuplicateTopicException.ForTopic(newFullName);
			}

			// If the topic does not exist (e.g., it's a backing topic), don't bother...
			if (!TipFileExists(oldName.Name))
			{
				answer.Add("This topic can not be renamed (it is probably a backing topic).");
				return answer;
			}

			try
			{
				gen.Push();

				// Rename the archive files, too
				foreach (FileInfo each in FileInfosForTopic(oldName))
				{
					AbsoluteTopicName newNameForThisVersion = new AbsoluteTopicName(newName, Namespace);
					newNameForThisVersion.Version = ExtractVersionFromHistoricalFilename(each.Name);

					AbsoluteTopicName oldNameForThisVersion = new AbsoluteTopicName(oldName.Name, Namespace);
					oldNameForThisVersion.Version = newNameForThisVersion.Version;
					
					string newFilename = MakePath(root, newNameForThisVersion.LocalName);
					File.Move(each.FullName, newFilename);

					// record changes (a delete for the old one and an add for the new one)
					gen.RecordCreatedTopic(newNameForThisVersion);
					gen.RecordDeletedTopic(oldNameForThisVersion);
				}

				// Rename the topic file
				File.Move(pathToTopicFile, newNameForTopicFile);

				// Record changes (a delete for the old one and an add for the new one)
				gen.RecordCreatedTopic(newFullName);
				gen.RecordDeletedTopic(oldName.AsAbsoluteTopicName(Namespace));

				// Now get ready to do fixups
				if (!fixup)
					return answer;

				// OK, we need to do the hard work
				AbsoluteTopicName oldabs = oldName.AsAbsoluteTopicName(Namespace);
				AbsoluteTopicName newabs = new AbsoluteTopicName(newName, oldabs.Namespace);
				
				// Now the master loop
				foreach (AbsoluteTopicName topic in AllTopics(false))
					if (RenameTopicReferences(topic.LocalName, oldabs, newabs, gen))
						answer.Add("Found and replaced references in " + topic);
			}
			finally
			{
				gen.Pop();
			}

			return answer;
		}


		/// Delete a content base (kills all .wiki and .awiki files; removed the dir if empty)
		/// </summary>
		override public void Delete()
		{
			FederationUpdate update = new FederationUpdate();
			foreach (AbsoluteTopicName topic in AllTopics(false))
				if (!IsBackingTopic(topic.LocalName))
					update.RecordDeletedTopic(topic);

			DirectoryInfo dir = new DirectoryInfo(Root);
			foreach (FileInfo each in dir.GetFiles("*.wiki"))
				each.Delete();
			foreach (FileInfo each in dir.GetFiles("*.awiki"))
				each.Delete();
			if (dir.GetFiles().Length == 0)
				dir.Delete(true);

			OnFederationUpdated(new FederationUpdateEventArgs(update));
		}

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

		public override IEnumerable AllTopicsSortedLastModifiedDescending()
		{
			return AllTopicsSorted(new TimeSort());
		}

		protected override IEnumerable AllTopicsUnsorted()
		{
			return AllTopicsSorted(null);
		}


		class FileInfoTopicData : ContentBase.TopicData
		{
			public FileInfoTopicData(FileInfo info, string ns)
			{
				_Info = info;
				_Namespace = ns;
			}

			public override string Namespace
			{
				get
				{
					return _Namespace;
				}
			}

			string _Namespace;

			FileInfo _Info;

			public override DateTime LastModificationTime
			{
				get
				{
					return _Info.LastWriteTime;
				}
			}

			public override string Version
			{
				get
				{
					return ExtractVersionFromHistoricalFilename(Path.GetFileNameWithoutExtension(_Info.ToString()));
				}
			}

			public override string Name
			{
				get
				{
					return Path.GetFileNameWithoutExtension(_Info.ToString());
				}
			}


			public string FullName
			{
				get
				{
					return _Info.FullName;
				}
			}

			public override string Author
			{
				get
				{
					string filename = _Info.Name;
					// remove the extension
					filename = filename.Substring(0, filename.Length - _Info.Extension.Length);
					Match m = HistoricalFileNameRegex.Match(filename);
					if (!m.Success)
						return null;
					if (m.Groups["name"].Captures.Count == 0)
						return null;
					return m.Groups["name"].Value;
				}
			}

		}



	}
}
