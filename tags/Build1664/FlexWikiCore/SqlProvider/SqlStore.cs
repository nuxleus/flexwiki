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
using System.IO;
using System.Text.RegularExpressions;
using FlexWiki;

namespace SqlProvider
{
	/// <summary>
	/// Summary description for SqlStore.
	/// </summary>
	public class SqlStore: ReadWriteStore
	{
		private string _ConnectionString;
		DateTime DefinitionTopicLastRead = DateTime.MinValue;

		public SqlStore()
		{

		}

		public SqlStore(Federation aFed, string ns, string connectionString)
		{
			SetFederation(aFed);

			Namespace = ns;

			_ConnectionString = connectionString;

			_State = State.New;
			// Check if the namespace exists in the database
			// If does not exist then create the database.
			if( SqlNamespaceProvider.Exists(Namespace, ConnectionString) )
			{
				Read();
			}
			else
			{
				SqlHelper.CreateNamespace(Namespace, ConnectionString);
			}
		}

		static string MakeTopicName(LocalTopicName name)
		{
			if (name.Version == null || name.Version.Length == 0)
				return name.Name;
			else
				return name.Name + "(" + name.Version + ")";
		}

		enum State
		{
			New, 
			Loading,
			Loaded
		};

		State	_State = State.New;
		
		/// <summary>
		/// Read the Namespace details from the store.
		/// </summary>
		private void Read()
		{
			if (_State == State.Loading)
				throw new Exception("Recursion problem: already loading ContentBase for " + Namespace);

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

			if (SqlHelper.TopicExists(Namespace, ContentBase.DefinitionTopicLocalName, ConnectionString))
			{
				DefinitionTopicLastRead = SqlHelper.GetTopicLastWriteTime(Namespace, ContentBase.DefinitionTopicLocalName, ConnectionString);
			
				Hashtable hash = ExtractExplicitFieldsFromTopicBody(SqlHelper.GetTopicBody(Namespace, ContentBase.DefinitionTopicLocalName, ConnectionString));

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
			
			a = new AbsoluteTopicName("HomePage", Namespace);
			top = new BackingTopic(a, DefaultHomePageContent, true);
			BackingTopics[a.Name] = top;
			
			a = new AbsoluteTopicName("_NormalBorders", Namespace);
			top = new BackingTopic(a, DefaultNormalBordersContent, true);
			BackingTopics[a.Name] = top;

			_State = State.Loaded;
		}

		/// <summary>
		/// Sql Connection String for the namespace corresponding to this store.
		/// </summary>
		public string ConnectionString
		{
			get
			{
				return _ConnectionString;
			}
		}

		DateTime _Created = DateTime.Now;
		public override DateTime Created
		{
			get { return _Created; }
		}

		DateTime _LastRead;
		public override DateTime LastRead
		{
			get { return _LastRead; }
		}

		public override void Validate()
		{
			if (!SqlHelper.TopicExists(Namespace, ContentBase.DefinitionTopicLocalName, ConnectionString))
				return;
			DateTime lastmod = SqlHelper.GetTopicLastWriteTime(Namespace, ContentBase.DefinitionTopicLocalName, ConnectionString);
			if (DefinitionTopicLastRead >= lastmod)
				return;
			Read();
		}

		public override bool Exists
		{
			get { return SqlHelper.NamespaceExists(Namespace, _ConnectionString); }
		}

		/// <summary>
		/// Answer true if a topic exists in this ContentBase
		/// </summary>
		/// <param name="name">Name of the topic</param>
		/// <returns>true if it exists</returns>
		public override bool TopicExistsLocally(LocalTopicName name)
		{
			if (BackingTopics.ContainsKey(name.Name))
				return true;
			return SqlHelper.TopicExists(Namespace, MakeTopicName(name), _ConnectionString);
		}

		/// <summary>
		/// Answer when the topic was last changed
		/// </summary>
		/// <param name="topic">A topic name</param>
		/// <returns></returns>
		public override DateTime GetTopicLastWriteTime(LocalTopicName topic)
		{
			string topicName = MakeTopicName(topic);
			if (SqlHelper.TopicExists(Namespace, topicName, _ConnectionString))
				return SqlHelper.GetTopicLastWriteTime(Namespace, topicName, ConnectionString);
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
		public override bool IsExistingTopicWritable(LocalTopicName topic)
		{
			if( !SqlHelper.TopicExists(Namespace, topic.Name, _ConnectionString) )
			{
				BackingTopic back = GetBackingTopicNamed(topic);
				if (back == null)
					return false;
				return back.CanOverride;
			}
			
			return SqlHelper.IsExistingTopicWritable(Namespace, topic.Name, _ConnectionString);
		}

		/// <summary>
		/// Answer when a topic was created
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <returns></returns>
		public override DateTime GetTopicCreationTime(LocalTopicName topic)
		{
			if( SqlHelper.TopicExists(Namespace, topic.Name, _ConnectionString) )
			{
				return SqlHelper.GetTopicCreationTime(Namespace, topic.Name, _ConnectionString);
			}

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
		public override string GetTopicLastAuthor(LocalTopicName topic)
		{
			SqlInfoForTopic latestVersionInfo = SqlHelper.GetSqlTopicInfoForLatestTopicVersion(Namespace, topic.Name, _ConnectionString );

			if (latestVersionInfo == null)
			{
				BackingTopic back = GetBackingTopicNamed(topic);
				if (back != null)
					return back.LastAuthor;
				return AnonymousUserName;
			}

			TopicData info = new SqlInfoTopicData(latestVersionInfo, Namespace);
			string auth = info.Author;
			if (auth == null)
				return AnonymousUserName;
			return auth;

		}

		private SqlInfoForTopic[] SqlTopicInfosForTopic(LocalTopicName topic)
		{
			return SqlHelper.GetSqlTopicInfosForTopic(Namespace, topic.Name, _ConnectionString );
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

		/// <summary>
		/// Answer a TextReader for the given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
		/// <returns>TextReader</returns>
		public override TextReader TextReaderForTopic(LocalTopicName topic)
		{
			string topicName = MakeTopicName(topic);
			if (topicName == null || !SqlHelper.TopicExists(Namespace, topicName, _ConnectionString))
			{
				BackingTopic back = GetBackingTopicNamed(topic);
				if (back != null)
					return new StringReader(back.Body);
				throw TopicNotFoundException.ForTopic(topic, Namespace);
			}
			return new StringReader(SqlHelper.GetTopicBody(Namespace, topicName, _ConnectionString));
		}

		/// <summary>
		/// Delete a topic
		/// </summary>
		/// <param name="topic"></param>
		public override void DeleteTopic(LocalTopicName topic)
		{
			if( !SqlHelper.TopicExists(Namespace, topic.Name, _ConnectionString) )
			{
				return;
			}

			SqlHelper.DeleteTopic(Namespace, topic.Name, _ConnectionString);

			// Fire the event
			FederationUpdate update = new FederationUpdate();
			update.RecordDeletedTopic(topic.AsAbsoluteTopicName(Namespace));
			OnFederationUpdated(new FederationUpdateEventArgs(update));
		}

		/// <summary>
		/// Answer an (unsorted) enumeration of all topic in the ContentBase (doesn't include imports)
		/// </summary>
		/// <returns>Enumeration of AbsoluteTopicNames</returns>
		protected override IEnumerable AllTopicsUnsorted()
		{
			return AllTopicsSorted(null);
		}

		/// <summary>
		/// Answer an enumeration of all topic in the ContentBase, sorted by last modified (does not include those in imported namespaces)
		/// </summary>
		/// <returns>Enumeration of AbsoluteTopicNames</returns>
		public override IEnumerable AllTopicsSortedLastModifiedDescending()
		{
			return AllTopicsSorted(new TimeSort());
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
			foreach (SqlInfoForTopic each in SqlHelper.GetSqlTopicInfoForNonArchiveTopics(Namespace, comparer != null, _ConnectionString))
			{
				SqlInfoTopicData td = new SqlInfoTopicData(each, Namespace);
				all.Add(td);
				present.Add(td.Name);
			}
			bool sortAgain = false;
			foreach (BackingTopic each in BackingTopics.Values)
			{
				BackingTopicTopicData td = new BackingTopicTopicData(each);
				if (present.Contains(td.Name))
					continue;
				sortAgain = true;
				all.Add(td);				
			}
			if (comparer != null && sortAgain)
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
		public override IEnumerable AllVersionsForTopic(LocalTopicName topic)
		{
			ArrayList answer = new ArrayList();
			SqlInfoForTopic[] infos = SqlTopicInfosForTopic(topic);
			ArrayList sortable = new ArrayList();
			foreach (SqlInfoForTopic each in infos)
				sortable.Add(new SqlInfoTopicData(each, Namespace));
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
		public override string LatestVersionForTopic(LocalTopicName topic)
		{
			ArrayList sortable = new ArrayList();

			SqlInfoForTopic latestVersionInfo = SqlHelper.GetSqlTopicInfoForLatestTopicVersion(Namespace, topic.Name, _ConnectionString );

			if( latestVersionInfo != null )
				sortable.Add(new SqlInfoTopicData(latestVersionInfo, Namespace));

			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				sortable.Add(new BackingTopicTopicData(back));
			
			if (sortable.Count == 0)
				return null;
			sortable.Sort(new TimeSort());

			return ((TopicData)(sortable[0])).Version; 
		}

		/// <summary>
		/// A list of TopicChanges to a topic since a given date [sorted by date]
		/// </summary>
		/// <param name="topic">A given date</param>
		/// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
		/// <param name="rule">A composite cache rule to fill with rules that represented accumulated dependencies (or null)</param>
		/// <returns>Enumeration of TopicChanges</returns>
		public override IEnumerable AllChangesForTopicSince(LocalTopicName topic, DateTime stamp, CompositeCacheRule rule)
		{
			ArrayList answer = new ArrayList();
			SqlInfoForTopic[] infos = SqlHelper.GetSqlTopicInfosForTopicSince(Namespace, topic.Name, stamp, ConnectionString);
			ArrayList sortable = new ArrayList();
			foreach (SqlInfoForTopic each in infos)
				sortable.Add(new SqlInfoTopicData(each, Namespace));
			BackingTopic back = GetBackingTopicNamed(topic);
			bool sortAgain = false;
			if (back != null)
			{
				sortAgain = true;
				sortable.Add(new BackingTopicTopicData(back));
			}
			if( sortAgain )
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
		/// <param name="gen">Object to recieve change info about the topic</param>
		protected override void WriteTopic(LocalTopicName topic, string content, FederationUpdateGenerator gen)
		{
			string topicName = MakeTopicName(topic);
			bool isNew = !(SqlHelper.TopicExists(Namespace, topicName, _ConnectionString));

			// Get old topic so we can analyze it for properties to compare with the new one
			string oldText = null;
			Hashtable oldProperties = null;
			if (!isNew)
			{
				oldText = SqlHelper.GetTopicBody(Namespace, topicName, _ConnectionString);
				oldProperties = ExtractExplicitFieldsFromTopicBody(oldText);	
			}

			SqlHelper.WriteTopic(Namespace, topicName, LastWriteTime(topic.NameWithVersion) ,_ConnectionString, content, ((topic.Version != null && topic.Version.Length > 0)?true:false));

			// Quick check to see if we're about to let somebody write the DefinitionTopic for this ContentBase.  
			// If so, we reset our Info object to reread
			if (topicName == ContentBase.DefinitionTopicLocalName)
				this.Federation.InvalidateNamespace(Namespace);

			// Record changes
			RecordTopicChanges(topic, gen, isNew, content, oldText, oldProperties);
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
				millisecond = Int32.Parse(m.Groups["fraction"].Value.Substring(0,3));
	
			return new DateTime(year, month, day, hour, minute, second, millisecond);
		}

		private void RecordTopicChanges(LocalTopicName topic, FederationUpdateGenerator gen, bool isNew, string content, string oldText, Hashtable oldProperties)
		{
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
		public override ArrayList RenameTopic(LocalTopicName oldName, string newName, bool fixup)
		{
			FederationUpdateGenerator gen = CreateFederationUpdateGenerator();

			// TRIGGER
			ArrayList answer = new ArrayList();
			string currentTopicName =  oldName.Name;
			string newTopicName = newName;
			AbsoluteTopicName newFullName = new AbsoluteTopicName(newName, Namespace);

			// Make sure it's not goign to overwrite an existing topic
			if (TopicExistsLocally(newName))
			{
				throw DuplicateTopicException.ForTopic(newFullName);
			}

			// If the topic does not exist (e.g., it's a backing topic), don't bother...
			if (!TipTopicRecordExists(oldName.Name))
			{
				answer.Add("This topic can not be renamed (it is probably a backing topic).");
				return answer;
			}

			try
			{
				gen.Push();

				// Rename the archive files, too
				foreach (SqlInfoForTopic each in SqlTopicInfosForTopic(oldName))
				{
					AbsoluteTopicName newNameForThisVersion = new AbsoluteTopicName(newName, Namespace);
					newNameForThisVersion.Version = ExtractVersionFromTopicName(each.Name);

					AbsoluteTopicName oldNameForThisVersion = new AbsoluteTopicName(oldName.Name, Namespace);
					oldNameForThisVersion.Version = newNameForThisVersion.Version;
					
					SqlHelper.RenameTopic(Namespace, each.Name, MakeTopicName(newNameForThisVersion.LocalName), _ConnectionString);

					// record changes (a delete for the old one and an add for the new one)
					gen.RecordCreatedTopic(newNameForThisVersion);
					gen.RecordDeletedTopic(oldNameForThisVersion);
				}

				// Rename the topic file
				SqlHelper.RenameTopic(Namespace, currentTopicName, newTopicName, _ConnectionString);

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

		/// Delete a content base (kills everything inside recursively).  Note that this does *not* include unregistering
		/// the content base within the federation.
		/// </summary>
		public override void Delete()
		{
			FederationUpdate update = new FederationUpdate();
			foreach (AbsoluteTopicName topic in AllTopics(false))
				if (!IsBackingTopic(topic.LocalName))
					update.RecordDeletedTopic(topic);

			SqlHelper.DeleteNamespace(Namespace, _ConnectionString);
			OnFederationUpdated(new FederationUpdateEventArgs(update));
		}

		private bool TipTopicRecordExists(string topicName)
		{
			return SqlHelper.TopicExists(Namespace, MakeTopicName(new LocalTopicName(topicName)), _ConnectionString);
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

		public static Regex TopicNameRegex = new Regex("[^(]+\\((?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?\\)");

		class SqlInfoTopicData : ContentBase.TopicData
		{
			string _Namespace;
			SqlInfoForTopic _Info;

			public SqlInfoTopicData(SqlInfoForTopic info, string ns)
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
					return ExtractVersionFromTopicName(_Info.Name);
				}
			}

			public override string Name
			{
				get
				{
					return ExtractNameFromTopicName(_Info.Name);
				}
			}


			public string FullName
			{
				get
				{
					return _Namespace + "." + Name;;
				}
			}

			public override string Author
			{
				get
				{
					Match m = TopicNameRegex.Match(_Info.Name);
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
