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
using System.Web.Caching;
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
	/// A ContentBase is the interface to all the wiki topics in a namespace.  It shields the rest of the 
	/// wiki system from worrying about where the topics are stored.  It exposes operations
	/// like reading and writing a topic's content, enumerating all toics, etc.  In its current 
	/// implementation, all content is stored in the file system; thus, all file IO is hidden 
	/// in this class.
	/// </summary>
	/// 
	[ExposedClass("NamespaceInfo", "Provides information about a namespace")]
	public class ContentBase : BELObject, IComparable
	{
		/// <summary>
		/// The file system path to the folder that contains the content
		/// </summary>
		string						_Root;
		
		public DateTime	Created = DateTime.Now;
		public DateTime LastRead;

		/// <summary>
		/// Default constructor for XML Serialization.
		/// </summary>
		public ContentBase()
		{
		}

		/// <summary>
		/// Answer the content base for the given root (stored in the Federation; create and register on demand if it doesn't exist)
		/// </summary>
		/// <param name="root"></param>
		public static ContentBase zzzSecretDoNotUseNewContentBaseDemandCreatedInFederation(string root, Federation aFederation)
		{
			ContentBase answer = aFederation.ContentBaseForRoot(root, false);
			if (answer != null)
				return answer;
			answer = new ContentBase(root, aFederation);
			aFederation.SetContentBaseForRoot(root, answer);
			return answer;
		}

		Federation _Federation;

		/// <summary>
		/// Create a ContentBase to represent the content base at the given physical path in the given federation.  Read in the definition if the content base already exists.
		/// Create the directory if it doesn't exist.
		/// </summary>
		/// <param name="physicalPath">Path to the directory containing the content base</param>
		ContentBase(string physicalPath, Federation aFederation)
		{
			_Root = physicalPath;
			_Federation = aFederation;
			_State = State.New;
			if (Directory.Exists(physicalPath))
			{
				Read();
			}
			else
			{
				try
				{
					Directory.CreateDirectory(physicalPath);
				} 
				catch (DirectoryNotFoundException e)
				{
					e.ToString();
				}
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Gets the topics with the specified property and value (excluding those in the imported namespaces).")]
		public TopicInfoArray TopicsWith(string property, string desiredValue)
		{
			return this.RetrieveAllTopicsWith(property, desiredValue, false);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Gets the topics with the specified property and value (including those in the imported namespaces).")]
		public TopicInfoArray AllTopicsWith(string property, string desiredValue)
		{
			return this.RetrieveAllTopicsWith(property, desiredValue, true);
		}

		private TopicInfoArray RetrieveAllTopicsWith(string property, string desiredValue, bool includeImports)
		{
			TopicInfoArray answer = this.TopicInfoCacheContains(this.CreateTopicInfoCacheKey(property,  desiredValue, includeImports));

			if( answer != null )
			{
				return answer;
			}

			answer = new TopicInfoArray();
			CompositeCacheRule compositeCacheRule = new CompositeCacheRule();

			foreach(AbsoluteTopicName topic in AllTopics(includeImports))
			{
				if(topic.Name.StartsWith("_"))
					continue;

				string propertyValue = Federation.GetTopicProperty(topic, property);

				if( propertyValue.ToLower() == desiredValue.ToLower() )
				{
					ContentBase contentBaseForTopic = Federation.ContentBaseForTopic(topic);
					answer.Add(new TopicInfo(Federation, topic));
					compositeCacheRule.Add(contentBaseForTopic.CacheRuleForAllPossibleInstancesOfTopic(topic));
				}
			}

			this.UpdateTopicInfoCache(this.CreateTopicInfoCacheKey(property,  desiredValue, includeImports), answer, compositeCacheRule);

			return answer;
		}

		private string CreateTopicInfoCacheKey(string property, string desiredValue, bool includeImports)
		{
			return (this.Namespace + "_" + property + "_" + desiredValue + "_" + ((includeImports) ? Boolean.TrueString: Boolean.FalseString));
		}

		private TopicInfoArray TopicInfoCacheContains(string key)
		{
			object cacheContents = this.Federation.FederationCache.Get(key);
			if( cacheContents != null )
			{
				return (TopicInfoArray)cacheContents; 
			}
			else
			{
				return null;
			}
		}

		private void UpdateTopicInfoCache(string key, TopicInfoArray topicInfoArray, CacheRule cacheRule)
		{
			// Will overwrite an existing item in the cache.
			if( this.Federation.FederationCache != null )
			{
				this.Federation.FederationCache.Put(key, topicInfoArray, cacheRule);
			}
		}


		Hashtable _BackingTopics;
		[XmlIgnore]
		public Hashtable BackingTopics
		{
			get
			{
				if (_BackingTopics != null)
					return _BackingTopics;
				_BackingTopics = new Hashtable();
				return _BackingTopics;
			}
		}

		enum State
		{
			New, 
			Loading,
			Loaded
		};

		State	_State = State.New;

		/// <summary>
		/// Answer if this ContentBase is secure
		/// </summary>
		public bool Secure
		{
			get{return _Secure;}
			set{_Secure = value;}
		}
		bool _Secure=false;


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
		public static string DefinitionTopicFilename
		{
			get
			{
				return DefinitionTopicLocalName + ".wiki";
			}
		}


		
		DateTime DefinitionTopicLastRead = DateTime.MinValue;

		public void Validate()
		{
			if (!File.Exists(DefinitionTopicFilePath))
				return;
			DateTime lastmod = File.GetLastWriteTime(DefinitionTopicFilePath);
			if (DefinitionTopicLastRead >= lastmod)
				return;
			Read();
		}

		void Read()
		{
			if (_State == State.Loading)
				throw new Exception("Recursion problem: already loading ContentBase for " + _Root);

			_ImportedNamespaces = new ArrayList();
			_Namespace = null;
			_Description = null;
			_Title = null;
			_ImageURL = null;
			_Contact = null;
	
			string filename = DefinitionTopicFilePath;
			if (!File.Exists(filename))
				return;

			DefinitionTopicLastRead = File.GetLastWriteTime(filename);

			_State = State.Loading;
			LastRead = DateTime.Now;
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
			Namespace = (string)hash["Namespace"];
			Description = (string)hash["Description"];
			ImageURL = (string)hash["ImageURL"];
			Contact = (string)hash["Contact"];

			_DisplaySpacesInWikiLinks = false; // the default, applied if *.config key is missing and _ContentBaseDefinition property is missing
			if(hash.ContainsKey("DisplaySpacesInWikiLinks")) // _ContentBaseDefinition property overrides *.config setting
			{
				DisplaySpacesInWikiLinks = Convert.ToBoolean(hash["DisplaySpacesInWikiLinks"]);
			}
			else // No _ContentBaseDefinition property, so look for *.config setting
			{
				DisplaySpacesInWikiLinks = (System.Configuration.ConfigurationSettings.AppSettings["DisplaySpacesInWikiLinks"] != null ? Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["DisplaySpacesInWikiLinks"]) : _DisplaySpacesInWikiLinks);
			}

			string importList = (string)hash["Import"]; 
			if (importList != null)
			{
				foreach (string each in Federation.ParseListPropertyValue(importList))
				{
					_ImportedNamespaces.Add(each);
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


		string _Description;
		/// <summary>
		/// Answer the description for the ContentBase(or null)
		/// </summary>
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a description of this namespace")]
		public string Description
		{
			get
			{
				return _Description;
			}
			set
			{
				_Description = value;
			}
		}

		string _Contact;

		/// <summary>
		/// Answer the contact info for the ContentBase (or null)
		/// </summary>
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a string identifying the contact for this namespace")]
		public string Contact
		{
			get
			{
				return _Contact;
			}
			set
			{
				_Contact = value;
			}
		}

		string _ImageURL;
		/// <summary>
		/// Answer the Image URL for the ContentBase (or null)
		/// </summary>
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the URL for an image that brand-marks this namespace")]
		public string ImageURL
		{
			get
			{
				return _ImageURL;
			}
			set
			{
				_ImageURL = value;
			}
		}

		string _HomePage = "HomePage";
		/// <summary>
		/// Answer the name of the wiki topic that serves as the "home" page for the namespace
		/// </summary>
		public string HomePage
		{
			get
			{
				return _HomePage;
			}
			set
			{
				_HomePage = value;
			}
		}


		/// <summary>
		/// Answer the human-friendly title for the ContentBase (Title if available, else Namespace)
		/// </summary>
		[ExposedMethod("Title", ExposedMethodFlags.CachePolicyNone, "Answer a title for this namespace")]
		public string FriendlyTitle
		{
			get
			{
				if (Title != null)
					return Title;
				else
					return Namespace;
			}
		}

		bool _DisplaySpacesInWikiLinks = false;
		public bool DisplaySpacesInWikiLinks
		{
			get
			{
				return _DisplaySpacesInWikiLinks;
			}
			set
			{
				_DisplaySpacesInWikiLinks = value;
			}
		}

		/// <summary>
		/// Answer an AbsoluteTopicName for the given topic name local to this ContentBase.
		/// </summary>
		/// <param name="localTopicName">A topic name</param>
		/// <returns>An AbsoluteTopicName</returns>
		public AbsoluteTopicName TopicNameFor(string localTopicName)
		{
			return new AbsoluteTopicName(localTopicName, Namespace);
		}

		ArrayList _ImportedNamespaces = new ArrayList();
		/// <summary>
		/// Answer an Enumeration of strings that are the names of namespaces imported into this content base
		/// </summary>
		[XmlIgnore]
		public ICollection ImportedNamespaces
		{
			get
			{
				return _ImportedNamespaces;
			}
		}

		string _Namespace;
		/// <summary>
		/// Answer the namespace for this ContentBase
		/// </summary>
		[ExposedMethod("Name", ExposedMethodFlags.CachePolicyNone, "Answer the name of this namespace")]
		public string Namespace
		{
			get
			{
				return _Namespace;
			}
			set
			{
				_Namespace = value;
			}
		}



		string _Title;

		/// <summary>
		/// The title of the content base  (or null if the ContentBase is bogus)
		/// </summary>
		public string Title
		{
			get
			{
				return _Title;
			}
			set
			{
				_Title = value;
			}		
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

		/// <summary>
		/// Answer true if the content base exists
		/// </summary>
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if this namespace actually exists")]
		public bool Exists
		{
			get
			{
				return Directory.Exists(Root);
			}
		}


		/// <summary>
		/// Answer true if the given topic exists in this ContentBase (or in an imported namespace)
		/// </summary>
		/// <param name="topic">The topic to check for</param>
		/// <returns>true if the topic exists</returns>
		public bool TopicExists(TopicName topic)
		{
			AbsoluteTopicName abs = topic as AbsoluteTopicName;
			if (abs != null)
			{
				ContentBase cb = Federation.ContentBaseForTopic(abs);
				if (cb == null)
					return false;
				return cb.TopicExistsLocally(abs);
			}
			// Must be relative

			// Is it here?
			if (TopicExistsLocally(topic))
				return true;
			// Is it there?
			foreach (string ns in ImportedNamespaces)
			{
				ContentBase cb = Federation.ContentBaseForNamespace(ns);
				if (cb == null)
					continue;
				if (cb.TopicExistsLocally(abs))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Answer a hash with keys as AbsoluteTopicNames and values as full file paths for topics that actually exist (in this contentbase)
		/// </summary>
		/// <param name="topic">The topic you want paths for</param>
		/// <returns>HashTable (keys=AbsoluteTopicNames; values=file paths)</returns>
		Hashtable TopicPaths(TopicName topic)
		{
			Hashtable answer = new Hashtable();
			foreach (DictionaryEntry each in TopicRoots(topic))
			{
				string root = (string)each.Value;
				string path = MakePath(root, topic);
				if (File.Exists(path))
					answer[each.Key] = path;
			}
			return answer;
		}

		/// <summary>
		/// Answer a collection of namespaces in which the topic actually exists
		/// </summary>
		/// <param name="topic">The topic you want to search for in all namespaces (might be relative, in which case it's relative to this content base)</param>
		/// <returns>A list of namespaces (as strings); empty if none</returns>
		public IList TopicNamespaces(TopicName topic)
		{
			ArrayList answer = new ArrayList();
			foreach (AbsoluteTopicName each in topic.AllAbsoluteTopicNamesFor(this))
			{
				if (TopicExists(each))
					answer.Add(each.Namespace);
			}
			return answer;
		}

		public CacheRule CacheRuleForAllPossibleInstancesOfTopic(TopicName aName)
		{
			FilesCacheRule files = new FilesCacheRule();
			CompositeCacheRule answer = new CompositeCacheRule();
			foreach (AbsoluteTopicName possible in aName.AllAbsoluteTopicNamesFor(this))
			{
				string r = RootForNamespace(possible.Namespace);
				if (r != null)
					files.AddFile(MakePath(r, possible));
			}
			answer.Add(files);
			answer.Add(CacheRuleForDefinition);		// Add the cache rule for the content base, too, since if that changes there might be a change in imports
			return answer;
		}

		[XmlIgnore]
		public CacheRule CacheRuleForDefinition
		{
			get
			{
				return new FilesCacheRule(DefinitionTopicFilePath);
			}
		}

		/// <summary>
		/// Answer a CacheRule for this ContentBase (for all topics in the content base)
		/// </summary>
		/// <returns>A CacheRule</returns>
		[XmlIgnore]
		public CacheRule CacheRule
		{
			get
			{
				return new FilesCacheRule(Root);
			}
		}

		string TopicPath(TopicName localTopicName)
		{
			if (localTopicName.Namespace == null || localTopicName.Namespace == Namespace)
				return MakePath(Root, localTopicName);
			AbsoluteTopicName abs = localTopicName.AsAbsoluteTopicName(Namespace);
			ContentBase cb = Federation.ContentBaseForTopic(abs);
			if (cb == null)
				return null;
			return cb.TopicPath(abs);
		}

		/// <summary>
		/// Answer path to the topic name.  Throw if there's not exactly 1. 
		/// </summary>
		/// <param name="topic"></param>
		/// <returns></returns>
		
		string UnambiguousTopicPath(TopicName topic)
		{
			Hashtable paths = TopicPaths(topic);
			if (paths.Count == 0)
				throw TopicNotFoundException.ForTopic(topic);
			if (paths.Count > 1)
				throw TopicIsAmbiguousException.ForTopic(topic);
			string answer = null;
			foreach (string path in paths.Values)
				answer = path;
			return answer;
		}

		/// <summary>
		/// Answer the full name of the topic (qualified with namespace) if it exists.  If it doesn't exist at all, answer null
		/// If it does, but it's ambiguous, then throw TopicIsAmbiguousException
		/// </summary>
		/// <param name="topic"></param>
		/// <returns>Full name or null if it doesn't exist (by throw TopicIsAmbiguousException if it's ambiguous)</returns>
		public AbsoluteTopicName UnambiguousTopicNameFor(TopicName topic)
		{
			IList list = TopicNamespaces(topic);
			if (list.Count == 0)
				return null;
			if (list.Count > 1)
				throw TopicIsAmbiguousException.ForTopic(topic);
			return new AbsoluteTopicName(topic.Name, (string)list[0]);
		}
			
		/// <summary>
		/// Answer the namespace that the topic lives in.  It might be unambiguous, in which case TopicIsAmbiguousException will be thrown
		/// </summary>
		/// <param name="topic"></param>
		/// <returns></returns>
		public string UnambiguousTopicNamespace(TopicName topic)
		{
			IList list = TopicNamespaces(topic);
			if (list.Count == 0)
				return null;
			if (list.Count > 1)
				throw TopicIsAmbiguousException.ForTopic(topic);
			string answer = null;
			foreach (string ns in list)
				answer = ns;
			return answer;
		}


		/// <summary>
		/// Answer hash with keys equal to AbsoluteTopicNames and values as physical folder roots for all content bases that could contain the topic.
		/// Note that no existence checking is done; we list all possible roots.  
		/// Callers must prune the result down if they want based on whether the file exists.
		/// </summary>
		/// <param name="topic">The topic you want the info for (relative or absolute)</param>
		/// <returns>HashTable with keys as AbsoluteTopicNames and values as paths to the root folders</returns>
		Hashtable TopicRoots(TopicName topic)
		{
			Hashtable answer = new Hashtable();
			foreach (AbsoluteTopicName each in topic.AllAbsoluteTopicNamesFor(this))
			{
				string root = RootForNamespace(each.Namespace);
				if (root != null)
					answer[each] = root;
			}
			return answer;
		}

		/// <summary>
		/// Given a possibly relative topic name, answer all of the absolute topic names that actually exist
		/// </summary>
		/// <param name="topic"></param>
		/// <returns></returns>
		public IList AllAbsoluteTopicNamesThatExist(TopicName topic)
		{
			ArrayList answer = new ArrayList();
			foreach (string ns in TopicNamespaces(topic))
				answer.Add(new AbsoluteTopicName(topic.Name, ns));
			return answer;
		}

		/// <summary>
		/// Answer the full file system path for a given topic in a given folder.  
		/// </summary>
		/// <param name="root">File system path to the root directory for the containing content base</param>
		/// <param name="name">The name of the topic</param>
		/// <returns>Full path to the file containing the content for the most recent version of the topic</returns>
		static string MakePath(string root, TopicName name)
		{
			if (name.Version == null || name.Version.Length == 0)
				return root + "\\" + name.Name + ".wiki";
			else
				return root + "\\" + name.Name + "(" + name.Version + ").awiki";
		}
 

		/// <summary>
		/// Answer true if a topic exists in this ContentBase
		/// </summary>
		/// <param name="name">Name of the topic (namespace is ignored)</param>
		/// <returns>true if it exists</returns>
		public bool TopicExistsLocally(TopicName name)
		{
			if (BackingTopics.ContainsKey(name.Name))
				return true;
			return File.Exists(MakePath(Root, name));
		}

		public bool TopicExistsLocally(string name)
		{
			return TopicExistsLocally(new AbsoluteTopicName(name));
		}

		/// <summary>
		/// Answer the directory root for the given namespace
		/// </summary>
		/// <param name="ns">The namespace</param>
		/// <returns>Full path to the root for the given namespace or null if it can't be found in the federation</returns>
		string RootForNamespace(string ns)
		{
			if (ns == null && Namespace == null)
				return Root;
			if (ns == null)
				return null;
			return this.Federation.RootForNamespace(ns);
		}

		/// <summary>
		/// Answer an enumeration of ContentBase objects that are for the content bases imported into this one
		/// </summary>
		[XmlIgnore]
		public IEnumerable ImportedContentBases
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (string ns in ImportedNamespaces)
				{
					ContentBase cb = Federation.ContentBaseForNamespace(ns);
					if (cb != null)
						answer.Add(cb);
				}
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "ImportedNamespaces")]
		[XmlIgnore]
		public ArrayList ExposedImportedNamespaces
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (ContentBase cb in ImportedContentBases)
					answer.Add(cb);
				return answer;
			}
		}



		/// <summary>
		/// Answer the full AbsoluteTopicName for the definition topic for this content base
		/// </summary>
		[XmlIgnore]
		public AbsoluteTopicName DefinitionTopicName
		{
			get
			{
				return new AbsoluteTopicName(DefinitionTopicLocalName, Namespace);
			}
		}

		public static string DefinitionTopicLocalName = "_ContentBaseDefinition";



		[XmlIgnore]
		public Federation Federation
		{
			get
			{
				return _Federation;
			}
		}

		/// <summary>
		/// Answer when the topic was last changed
		/// </summary>
		/// <param name="topic">A topic name</param>
		/// <returns></returns>
		public DateTime GetTopicLastWriteTime(AbsoluteTopicName topic)
		{
			string path = TopicPath(topic);
			if (File.Exists(path))
				return File.GetLastWriteTime(path);
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				return back.LastModificationTime;
			throw TopicNotFoundException.ForTopic(topic);
		}

		public BackingTopic GetBackingTopicNamed(AbsoluteTopicName topic)
		{
			if (topic.Namespace == Namespace)
				return (BackingTopic)(BackingTopics[topic.Name]);
			ContentBase cb = Federation.ContentBaseForTopic(topic);
			if (cb == null)
				return null;
			return cb.GetBackingTopicNamed(topic);
		}

		/// <summary>
		/// Answer whether a topic exists and is writable
		/// </summary>
		/// <param name="topic">The topic (must directly be in this content base)</param>
		/// <returns>true if the topic exists AND is writable by the current user; else false</returns>
		public bool IsExistingTopicWritable(AbsoluteTopicName topic)
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
		public DateTime GetTopicCreationTime(AbsoluteTopicName topic)
		{
			string path = TopicPath(topic);
			if (File.Exists(path))
				return File.GetCreationTime(path);
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				return back.CreationTime;
			throw TopicNotFoundException.ForTopic(topic);			
		}

		/// <summary>
		/// Answer the DateTime of when any of the topics in teh ContentBase where last modified.
		/// </summary>
		/// <param name="includeImports">true if you also want to include all imported namespaces</param>
		/// <returns></returns>
		public DateTime LastModified(bool includeImports)
		{
			IEnumerable en = AllTopicsSortedLastModifiedDescending(includeImports);
			IEnumerator e = en.GetEnumerator();
			e.MoveNext();
			AbsoluteTopicName mostRecentlyChangedTopic = (AbsoluteTopicName)(e.Current);
			return GetTopicLastWriteTime(mostRecentlyChangedTopic);
		}

		static public string  AnonymousUserName = "anonymous";

		/// <summary>
		/// Answer the identify of the author who last modified a given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <returns>a user name</returns>
		public string GetTopicLastAuthor(AbsoluteTopicName topic)
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
				all.Add(new FileInfoTopicData(each));
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
		/// The name of the topic that contains external wiki definitions
		/// </summary>
		public static string ExternalWikisTopic = "ExternalWikis";
		

		/// <summary>
		/// Answer a Hashtable of the external wikis (for this ContentBase) and the replacement patterns
		/// </summary>
		/// <returns>Hashtable (keys = external wikii names, values = replacement patterns) </returns>
		public Hashtable ExternalWikiHash()
		{
			Hashtable answer = new Hashtable();
			string lines;
			try
			{
				lines = Read(new AbsoluteTopicName(ExternalWikisTopic, Namespace));
			}
			catch (TopicNotFoundException e)
			{
				e.ToString();
				return answer;
			}
			foreach (string line in lines.Split (new char[]{'\n'}))
			{
				string l = line.Replace("\r", "");
				Formatter.StripExternalWikiDef(answer, l);
			}

			return answer;
		}

		/// <summary>
		/// Regex pattern string for extracting a single line property
		/// </summary>
		public static string PropertyPattern = "^(?<leader>:?)(?<name>([A-Z_][_a-zA-Z0-9.]+)):(?<val>[^\\[{].*)";
		/// <summary>
		/// Regex pattern string for extracting the first line of a multi-line property
		/// </summary>
		public static string MultilinePropertyPattern = "^(?<leader>:?)(?<name>([A-Z_][_a-zA-Z0-9]+)):(?<delim>[\\[{])(?<val>.*)";

		/// <summary>
		/// Regex for extracting a single line property
		/// </summary>
		public static Regex PropertyRegex = new Regex(PropertyPattern);
		/// <summary>
		/// Regex for extracting a multi-line property
		/// </summary>
		public static Regex MultilinePropertyRegex = new Regex(MultilinePropertyPattern);

		/// <summary>
		/// Answer the contents of a given topic
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <returns>The contents of the topic or null if it can't be read (e.g., doesn't exist)</returns>
		public string Read(AbsoluteTopicName topic)
		{
			using (TextReader st = TextReaderForTopic(topic))
			{
				if (st == null)
					return null;
				return st.ReadToEnd();
			}
		}
	
		/// <summary>
		/// Reach and answer all the properties (aka fields) for the given topic.  This includes both the 
		/// properties defined in the topic plus the extra properties that every topic has (e.g., _TopicName, _TopicFullName, _LastModifiedBy, etc.)
		/// </summary>
		/// <param name="topic"></param>
		/// <returns>Hashtable (keys = string property names, values = values [as strings]);  or null if the topic doesn't exist</returns>
		public Hashtable GetFieldsForTopic(AbsoluteTopicName topic)
		{
			if (!TopicExists(topic))
				return null;

			string allLines = Read(topic);
			Hashtable answer = ExtractExplicitFieldsFromTopicBody(allLines);	
			AddImplicitPropertiesToHash(answer, topic, GetTopicLastAuthor(topic), GetTopicCreationTime(topic), GetTopicLastWriteTime(topic), allLines);
			return answer;
		}

		public static void AddImplicitPropertiesToHash(Hashtable hash, AbsoluteTopicName topic, string lastModBy, DateTime creation, DateTime modification, string content)
		{
			hash["_TopicName"] = topic.Name;
			hash["_TopicFullName"] = topic.FullnameWithVersion;
			hash["_LastModifiedBy"] = lastModBy;
			hash["_CreationTime"] = creation.ToString();
			hash["_ModificationTime"] = modification.ToString();
			hash["_Body"] = content;
		}

		/// <summary>
		/// Parse properties (aka fields) from a topic body
		/// </summary>
		/// <param name="body">The text body of a wiki topic to be parsed</param>
		/// <returns>Hashtable (keys = string property names, values = values [as strings])</returns>
		public static Hashtable ExtractExplicitFieldsFromTopicBody(string body)
		{
			Hashtable answer = new Hashtable();
			string inMultiline = null;
			string delim = null;
			foreach (string line in body.Split (new char[]{'\n'}))
			{
				if (inMultiline != null)
				{
					if (line.StartsWith(delim))
					{
						string includedDelim = "";
						if (IsBehaviorPropertyDelimiter(delim))
							includedDelim = delim;
						answer[inMultiline] = answer[inMultiline].ToString().Trim() + includedDelim;
						inMultiline = null;
						continue;
					}
					answer[inMultiline] = answer[inMultiline] + "\n" + line;
				} 
				else if (MultilinePropertyRegex.IsMatch(line))
				{
					Match m = MultilinePropertyRegex.Match(line);
					string each = m.Groups["name"].Value;
					string val = m.Groups["val"].Value;
					inMultiline = each;
					delim = m.Groups["delim"].Value;
					if (IsBehaviorPropertyDelimiter(delim))
						val = delim + val;
					delim = ClosingDelimiterForOpeningMultilinePropertyDelimiter(delim);
					answer[each] = val;
				}
				else if (PropertyRegex.IsMatch(line))
				{
					Match m = PropertyRegex.Match(line);
					string each = m.Groups["name"].Value;
					string val = m.Groups["val"].Value;
					answer[each] = val.Trim();
				}
			}


			return answer;
		}

		static public bool IsBehaviorPropertyDelimiter(string s)
		{
			return s == "{" || s == "}";
		}

		static public string ClosingDelimiterForOpeningMultilinePropertyDelimiter(string open)
		{
			switch (open)
			{
				case "[":
					return "]";
				case "{":
					return "}";
			}
			throw new Exception("Illegal multiline property delimiter.");
		}

		/// <summary>
		/// Change the value of a property (aka field) in a a topic.  If the topic doesn't exist, it will be created.
		/// </summary>
		/// <param name="topic">The topic whose property is to be changed</param>
		/// <param name="field">The name of the property to change</param>
		/// <param name="rep">The new value for the field</param>
		public void SetFieldValue(AbsoluteTopicName topic, string field, string rep, bool writeNewVersion)
		{
			if (!TopicExists(topic))
			{
				WriteTopic(topic, "");
			}

			string original = Read(topic);

			// Multiline values need to end a complete line
			string repWithLineEnd = rep;
			if (!repWithLineEnd.EndsWith("\n"))
				repWithLineEnd = repWithLineEnd + "\n";
			bool newValueIsMultiline = rep.IndexOf("\n") > 0;

			string simpleField = "(?<name>(" + field + ")):(?<val>[^\\[].*)";
			string multiLineField = "(?<name>(" + field + ")):\\[(?<val>[^\\[]*\\])";

			string update = original;
			if (new Regex(simpleField).IsMatch(original))
			{
				if (newValueIsMultiline)
					update = Regex.Replace (original, simpleField, "${name}:[ " + repWithLineEnd + "]");
				else
					update = Regex.Replace (original, simpleField, "${name}: " + rep);
			}
			else if (new Regex(multiLineField).IsMatch(original))
			{
				if (newValueIsMultiline)
					update = Regex.Replace (original, multiLineField, "${name}:[ " + repWithLineEnd + "]");
				else
					update = Regex.Replace (original, multiLineField, "${name}: " + rep);
			}
			else
			{
				if (!update.EndsWith("\n"))
					update = update + "\n";
				if (rep.IndexOf("\n") == -1)
					update += field + ": " + repWithLineEnd;
				else
					update += field + ":[ " + repWithLineEnd + "]\n";
			}
	
			if (writeNewVersion)
				WriteTopicAndNewVersion(topic, update);
			else
				WriteTopic(topic, update);
		}

		/// <summary>
		/// Answer a TextReader for the given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
		/// <returns>TextReader</returns>
		public TextReader TextReaderForTopic(AbsoluteTopicName topic)
		{
			string topicFile = TopicPath(topic);
			if (topicFile == null || !File.Exists(topicFile))
			{
				BackingTopic back = GetBackingTopicNamed(topic);
				if (back != null)
					return new StringReader(back.Body);
				throw TopicNotFoundException.ForTopic(topic);
			}
			return new StreamReader(new FileStream(topicFile, FileMode.Open, FileAccess.Read, FileShare.Read));
		}

		/// <summary>
		/// Delete a topic
		/// </summary>
		/// <param name="topic"></param>
		public void DeleteTopic(AbsoluteTopicName topic)
		{
			string path = TopicPath(topic);
			if (File.Exists(path))
				File.Delete(path);
		}

		/// <summary>
		/// IComparer for FileInfos
		/// </summary>
		class TimeSort : IComparer
		{
			public int Compare(object left, object right)
			{
				return ((TopicData)right).LastModificationTime.CompareTo(((TopicData)left).LastModificationTime);
			}
		}

		/// <summary>
		/// Answer an enumeration of all topic in the ContentBase (possibly including those in imported namespaces, too)
		/// </summary>
		/// <param name="includeImports">true to include topics from included namespaces (won't recurse)</param>
		/// <returns>Enumeration of AbsoluteTopicNames</returns>
		public IEnumerable AllTopics(bool includeImports)
		{
			return AllTopicsSorted(null, includeImports);
		}

		/// <summary>
		/// Returns a list of all topics in this namespace (including imported namespaces)
		/// </summary>
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a list of all topics in this namespace (including imported namespaces)")]
		[XmlIgnore]
		public ArrayList AllTopicsInfo
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (AbsoluteTopicName name in AllTopics(true))
					answer.Add(new TopicInfo(Federation, name));
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a list of all topic in this namespace (excluding imported namespaces)")]
		[XmlIgnore]
		public ArrayList Topics
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (AbsoluteTopicName name in AllTopics(false))
					answer.Add(new TopicInfo(Federation, name));
				return answer;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Get information about the given topic")]
		public TopicInfo GetTopicInfo(string topicName)
		{
			AbsoluteTopicName abs = new AbsoluteTopicName(topicName, Namespace);
			return new TopicInfo(Federation, abs);
		}

		/// <summary>
		/// Answer an enumeration of all topic in the ContentBase, sorted by last modified (possibly including those in imported namespaces, too)
		/// </summary>
		/// <param name="includeImports">true to include topics from included namespaces (won't recurse)</param>
		/// <returns>Enumeration of AbsoluteTopicNames</returns>
		public IEnumerable AllTopicsSortedLastModifiedDescending(bool includeImports)
		{
			return AllTopicsSorted(new TimeSort(), includeImports);
		}


		/// <summary>
		/// Answer an enumeration of all topic in the ContentBase, sorted using the supplied IComparer (possibly including those in imported namespaces, too)
		/// </summary>
		/// <param name="comparer">Used to sort the topics in the answer</param>
		/// <param name="includeImports">true to include topics from included namespaces (won't recurse)</param>
		/// <returns>Enumeration of AbsoluteTopicNames</returns>
		IEnumerable AllTopicsSorted(IComparer comparer, bool includeImports)
		{
			ArrayList answer = new ArrayList();
			ArrayList all = new ArrayList();
			Set present = new Set();
			foreach (FileInfo each in new DirectoryInfo(Root).GetFiles("*.wiki"))
			{
				FileInfoTopicData td = new FileInfoTopicData(each);
				td.Tag = Namespace;
				all.Add(td);
				present.Add(td.Name);
			}
			foreach (BackingTopic each in BackingTopics.Values)
			{
				BackingTopicTopicData td = new BackingTopicTopicData(each);
				if (present.Contains(td.Name))
					continue;
				td.Tag = Namespace;
				all.Add(td);				
			}
			if (includeImports)
			{
				foreach (ContentBase eachBase in ImportedContentBases)
				{
					Set already = new Set();
					foreach (FileInfo each in new DirectoryInfo(eachBase.Root).GetFiles("*.wiki"))
					{
						FileInfoTopicData td = new FileInfoTopicData(each);
						td.Tag = eachBase.Namespace;
						all.Add(td);
						already.Add(td.Name);
					}
					foreach (BackingTopic each in eachBase.BackingTopics.Values)
					{
						BackingTopicTopicData td = new BackingTopicTopicData(each);
						if (present.Contains(td.Name))
							continue;
						td.Tag = eachBase.Namespace;
						all.Add(td);				
					}
				}
			}
			if (comparer != null)
				all.Sort(comparer);
			foreach (TopicData each in all)
			{
				AbsoluteTopicName name = new AbsoluteTopicName(each.Name, (string)each.Tag);
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

		public IEnumerable AllVersionsForTopic(AbsoluteTopicName topic)
		{
			ArrayList answer = new ArrayList();
			FileInfo[] infos = FileInfosForTopic(topic);
			ArrayList sortable = new ArrayList();
			foreach (FileInfo each in infos)
				sortable.Add(new FileInfoTopicData(each));
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				sortable.Add(new BackingTopicTopicData(back));
			sortable.Sort(new TimeSort());
			foreach (TopicData each in sortable)
			{
				AbsoluteTopicName name = new AbsoluteTopicName(topic.Name, topic.Namespace);
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
		public string LatestVersionForTopic(AbsoluteTopicName topic)
		{				
			ArrayList sortable = new ArrayList();

			FileInfo [] infos = FileInfosForTopic(topic);
			foreach (FileInfo each in infos)
				sortable.Add(new FileInfoTopicData(each));
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
			return File.Exists(MakePath(Root, new AbsoluteTopicName(name)));
		}
    
		/// <summary>
		/// All of the FileInfos for the historical versions of a given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <returns>FileInfos</returns>
		FileInfo[] FileInfosForTopic(AbsoluteTopicName topic)
		{
			FileInfo [] answer = {};

			// If the topic does not exist, we ignore any historical versions (the result of a delete)
			if (!TipFileExists(topic.Name))
				return answer;

			string root = RootForNamespace(topic.Namespace);
			if (root == null)
				throw NamespaceNotFoundException.ForNamespace(topic.Namespace);
			try
			{
				answer = new DirectoryInfo(root).GetFiles(topic.Name + "(*).awiki");
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
		public IEnumerable AllChangesForTopicSince(AbsoluteTopicName topic, DateTime stamp, CompositeCacheRule rule)
		{
			ArrayList answer = new ArrayList();
			FileInfo[] infos = FileInfosForTopic(topic);
			ArrayList sortable = new ArrayList();
			foreach (FileInfo each in infos)
				sortable.Add(new FileInfoTopicData(each));
			BackingTopic back = GetBackingTopicNamed(topic);
			if (back != null)
				sortable.Add(new BackingTopicTopicData(back));
			sortable.Sort(new TimeSort());
			FilesCacheRule fcr = null;
			if (rule != null)
			{
				fcr = new FilesCacheRule();
				// make sure we include the main .wiki file (if there is one -- it might be a backing topic in which case the file won't exist)
				string path = TopicPath(topic);
				if (File.Exists(path))
				{
					rule.Add(fcr);
					fcr.AddFile(path);
				}
			}
			foreach (TopicData each in sortable)
			{
				if (each.LastModificationTime < stamp)
					continue;
				AbsoluteTopicName name = new AbsoluteTopicName(topic.Name, topic.Namespace);
				name.Version = each.Version;
				TopicChange change = new TopicChange(name);
				answer.Add(change);
				if (rule != null && each is FileInfoTopicData)
					fcr.AddFile(((FileInfoTopicData)each).FullName);
			}
			return answer;
		}

		public IEnumerable AllChangesForTopicSince(AbsoluteTopicName topic, DateTime stamp)
		{
			return AllChangesForTopicSince(topic, stamp, null);
		}

		/// <summary>
		/// A list of TopicChanges to a topic (sorted by date)
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <returns>Enumeration of TopicChanges </returns>
		public IEnumerable AllChangesForTopic(AbsoluteTopicName topic)
		{
			return AllChangesForTopicSince(topic, DateTime.MinValue, null);
		}

		public IEnumerable AllChangesForTopic(AbsoluteTopicName topic, CompositeCacheRule rule)
		{
			return AllChangesForTopicSince(topic, DateTime.MinValue, rule);
		}

		/// <summary>
		/// Find the version of a topic immediately previous to another version
		/// </summary>
		/// <param name="topic">The name (with version) of the topic for which you want the change previous to</param>
		/// <returns>TopicChange or null if none</returns>
		public AbsoluteTopicName VersionPreviousTo(AbsoluteTopicName topic)
		{
			bool next = false;
			bool first = true;
			AbsoluteTopicName answer = new AbsoluteTopicName(topic.Fullname);
			foreach (TopicChange ver in AllChangesForTopic(topic))
			{
				answer.Version = ver.Version;
				if (next)
					return answer;
				if (topic.Version == null && !first)	// The version prior to the most recent is the second in line
					return answer;
				if (ver.Version == topic.Version)
					next = true;
				first = false;
			}
			return null;
		}
		
		/// <summary>
		/// Write a new version of the topic (doesn't write a new version)
		/// </summary>
		/// <param name="topic">Topic to write</param>
		/// <param name="content">New content</param>
		public void WriteTopic(AbsoluteTopicName  topic, string content)
		{
			string root = this.RootForNamespace(topic.Namespace);
			string fullpath = MakePath(root, topic);
			using (StreamWriter sw = new StreamWriter(fullpath))
			{
				sw.Write(content);
			}

			// Quick check to see if we're about to let somebody write the DefinitionTopic for this ContentBase.  
			// If so, we reset our Info object to reread
			string pathToDefinitionTopic = MakePath(Root, DefinitionTopicName);
			if (fullpath == pathToDefinitionTopic)
				this.Federation.InvalidateRoot(Root);
		}

		/// <summary>
		/// Rename the given topic.  If requested, find references and fix them up.  Answer a report of what was fixed up.  Throw a DuplicationTopicException
		/// if the new name is the name of a topic that already exists.
		/// </summary>
		/// <param name="oldName">Old topic name</param>
		/// <param name="newName">The new name</param>
		/// <param name="fixup">true to fixup referenced topic *in this namespace*; false to do no fixups</param>
		/// <returns>ArrayList of strings that can be reported back to the user of what happened during the fixup process</returns>
		public ArrayList RenameTopic(AbsoluteTopicName oldName, string newName, bool fixup)
		{
			ArrayList answer = new ArrayList();
			string root = RootForNamespace(oldName.Namespace);
			string pathToTopicFile =  root + "\\" + oldName.Name + ".wiki";
			string pathToArchiveFolder = root + "\\archive\\" + oldName.Name;
			string newNameForTopicFile = root + "\\" + newName + ".wiki";
			string newNameForArchiveFolder = root + "\\archive\\" + newName;
			AbsoluteTopicName newFullName = new AbsoluteTopicName(newName, Namespace);

			// Make sure it's not goign to overwrite an existing topic
			if (TopicExistsLocally(new AbsoluteTopicName(newName)))
			{
				throw DuplicateTopicException.ForTopic(newFullName);
			}

			// If the topic does not exist (e.g., it's a backing topic), don't bother...
			if (!TipFileExists(oldName.Name))
			{
				answer.Add("This topic can not be renamed (it is probably a backing topic).");
				return answer;
			}

			// Rename the archive files, too
			foreach (FileInfo each in FileInfosForTopic(oldName))
			{
				AbsoluteTopicName newNameForThisVersion = new AbsoluteTopicName(newName, oldName.Namespace);
				newNameForThisVersion.Version = ExtractVersionFromHistoricalFilename(each.Name);
				string newFilename = MakePath(root, newNameForThisVersion);
				File.Move(each.FullName, newFilename);
			}

			// Rename the topic file
			File.Move(pathToTopicFile, newNameForTopicFile);


			// Now get ready to do fixups
			if (!fixup)
				return answer;

			// OK, we need to do the hard work
			AbsoluteTopicName oldabs = oldName;
			AbsoluteTopicName newabs = new AbsoluteTopicName(newName, oldabs.Namespace);
			
			// Now the master loop
			ContentBase holder = Federation.ContentBaseForNamespace(oldName.Namespace);
			foreach (AbsoluteTopicName topic in holder.AllTopics(false))
				if (holder.RenameTopicReferences(topic, oldabs, newabs))
					answer.Add("Found and replaced references in " + topic);
			return answer;
		}

		/// <summary>
		/// Answer a hash: keys are topic; values are an array of topic names for referenced topics (in any content base)
		/// </summary>
		/// <param name="filterToTopic">Specific topic for which reference information is desired (null gets info for all topics)</param>
		/// <param name="existingOnly">Specific whether to only return the referenced topics that actually exist</param>
		/// <returns></returns>
		public Hashtable AllReferencesByTopic(AbsoluteTopicName filterToTopic, bool existingOnly)
		{
			Hashtable relativesToRefs = new Hashtable();
			Hashtable answer = new Hashtable();
			IEnumerable topicList;

			if (filterToTopic == null)
				topicList = AllTopics(false);
			else 
			{
				ArrayList list = new ArrayList();
				list.Add(filterToTopic);
				topicList = list;
			}

			foreach (AbsoluteTopicName topic in topicList)
			{
				string current = Read(topic);
				MatchCollection wikiNames = Formatter.extractWikiNames.Matches(current);
				ArrayList processed = new ArrayList();
				ArrayList allReferencedTopicsFromTopic = new ArrayList();
				foreach (Match m in wikiNames)
				{
					string each = m.Groups["topic"].ToString();
					if (processed.Contains(each))
					{
						continue;   // skip dup	
					}
					processed.Add(each);
				
					RelativeTopicName relName = new RelativeTopicName(TopicName.StripEscapes(each));
					
					// Now either calculate the full list of referenced names and cache it or get it from the cache
					ArrayList absoluteNames = (ArrayList)(relativesToRefs[relName]);
					if (absoluteNames == null)
					{
						absoluteNames = new ArrayList();
						relativesToRefs[relName] = absoluteNames;
						// Start with the singulars in the various reachable namespaces, then add the plurals
						if (existingOnly)
						{
							absoluteNames.AddRange(AllAbsoluteTopicNamesThatExist(relName));
							foreach (TopicName alternate in relName.AlternateForms)
								absoluteNames.AddRange(AllAbsoluteTopicNamesThatExist(alternate));
						}
						else
						{
							absoluteNames.AddRange(relName.AllAbsoluteTopicNamesFor(this));
							foreach (TopicName alternate in relName.AlternateForms)
								absoluteNames.AddRange(alternate.AllAbsoluteTopicNamesFor(this));
						}
					}
					allReferencedTopicsFromTopic.AddRange(absoluteNames);
				}
				answer[topic] = allReferencedTopicsFromTopic;
			}
			return answer;
		}


		/// <summary>
		/// Rename references (in a given topic) from one topic to a new name 
		/// </summary>
		/// <param name="topicToLookIn"></param>
		/// <param name="oldName"></param>
		/// <param name="newName"></param>
		/// <returns></returns>
		bool RenameTopicReferences(AbsoluteTopicName topicToLookIn, AbsoluteTopicName oldName, AbsoluteTopicName newName)
		{
			string current = Read(topicToLookIn);
			MatchCollection wikiNames = Formatter.extractWikiNames.Matches(current);
			ArrayList processed = new ArrayList();
			bool any = false;
			foreach (Match m in wikiNames)
			{
				string each = m.Groups["topic"].ToString();
				if (processed.Contains(each))
				{
					continue;   // skip dup	
				}
				processed.Add(each);
			
				RelativeTopicName relName = new RelativeTopicName(TopicName.StripEscapes(each));

				// See if this is the old name.  The only way it can be is if it's unqualified or if it's qualified with the current namespace.

				bool hit = (relName.Name == oldName.Name) && (relName.Namespace == null || relName.Namespace ==  oldName.Namespace);
				if (!hit)
					continue;

				// Now see if we got any hits or not
				string rep = Formatter.beforeWikiName + "(" + Formatter.RegexEscapeTopic(each) + ")" + Formatter.afterWikiName;
				// if the reference was fully qualified, retain that form in the new reference
				string replacementName = each.IndexOf(".") > -1 ? newName.Fullname : newName.Name;
				current = Regex.Replace(current, rep, "${before}" + replacementName + "${after}");
				any = true;
			}

			if (any)
				WriteTopicAndNewVersion(topicToLookIn, current);

			return any;
		}
	
		/// <summary>
		/// Write a topic (and create a historical version)
		/// </summary>
		/// <param name="topic">The topic to write</param>
		/// <param name="content">The content</param>
		public void WriteTopicAndNewVersion(AbsoluteTopicName topic, string content)
		{
			AbsoluteTopicName versionless = new AbsoluteTopicName(topic.Name, topic.Namespace);
			WriteTopic(versionless, content);
			WriteTopic(topic, content);
		}

		/// <summary>
		/// Delete a content base (kills everything inside recursively)
		/// </summary>
		public void Delete()
		{
			DirectoryInfo dir = new DirectoryInfo(Root);
			dir.Delete(true);
		}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (!(obj is ContentBase))
				throw new ArgumentException("object is not a ContentBase");
			return((new CaseInsensitiveComparer()).Compare(this.FriendlyTitle, ((ContentBase)(obj)).FriendlyTitle) );
		}
		#endregion



		abstract class TopicData
		{
			abstract public DateTime LastModificationTime {get;}
			abstract public string Version {get;}
			abstract public string Author {get;}
			abstract public string Name {get;}	// get the name of teh topic (unqualified and without version)

			public object Tag;	// General bucket for extra stuff that a client can use
		}

		class BackingTopicTopicData : TopicData
		{
			public BackingTopicTopicData(BackingTopic back)
			{
				_Back = back;
			}

			BackingTopic _Back;

			public override string Name
			{
				get
				{
					return _Back.FullName.Name;
				}
			}


			public override DateTime LastModificationTime
			{
				get
				{
					return _Back.LastModificationTime;
				}
			}

			public override string Version
			{
				get
				{
					//  TODO -- THe code here is stolen from TopicName.NewVersionStringForUser 
					//  I would have modified that so this encoding only appeared in one place but I
					// couldn't check out the file.

					string u = Author;
					u = u.Replace('\\', '-');
					u = u.Replace('?', '-');
					u = u.Replace('/', '-');
					u = u.Replace(':', '-');
					return LastModificationTime.ToString("yyyy-MM-dd-HH-mm-ss.ffff") + "-" + u;
				}
			}

			public override string Author
			{
				get
				{
					return _Back.LastAuthor;
				}
			}

		}

		class FileInfoTopicData : TopicData
		{
			public FileInfoTopicData(FileInfo info)
			{
				_Info = info;
			}

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


		static string DefaultHomePageContent
		{
			get
			{
				return @"
@flexWiki=http://www.flexwiki.com/default.aspx/$$$

!About Wiki
If you're new to WikiWiki@flexWiki, you should read the VisitorWelcome@flexWiki or OneMinuteWiki@flexWiki .  The two most important things to know are 
	1. follow the links to follow the thoughts and  
	1. YouAreEncouragedToChangeTheWiki@flexWiki

Check out the FlexWikiFaq@flexWiki as a means  to collaborate on questions you may have on FlexWiki@flexWiki
";
			}
		}


		static string DefaultNormalBordersContent
		{
			get
			{
				return @"
:MenuItem:{ tip, command, url |
	with (Presentations) 
	{[
		""||{T-}"", 
		Image(federation.LinkMaker.LinkToImage(""images/go.gif""), command, url), 
		""||{+}"", 
		Link(url, command, tip), 
		""||"", 
		Newline
	]}
}


RightBorder:{
aTopic|
	[
	request.IsAuthenticated.IfTrue
	{[
		""||{C2+}"",
		""Welcome '''"", 
		request.AuthenticatedUserName,
		""'''"",
		""||"",
		Newline,
		request.CanLogInAndOut.IfTrue
		{[	
			""||"",
			with (Presentations)
			{
				Link(federation.LinkMaker.LinkToLogoff(aTopic.Namespace.Name), ""Log off"", ""Log off."")
			},
			""||"",
			Newline
		]}
		IfFalse{""""},
	]}
	IfFalse
	{
		""""
	},
	namespace.Description,
	Newline, ""----"", Newline, 
	federation.About,
	Newline, ""----"", Newline,
	""*Recent Topics*"",
	Newline,
	request.VisitorEvents.Snip(15).Collect
	{ each |
		[
		Tab, 
		""*"",
		Presentations.Link(federation.LinkMaker.LinkToTopic(each.Fullname), each.Name),
		Newline
		]
	}
	]
}


LeftBorder:{
aTopic |
	[
request.AreDifferencesShown.IfTrue
	{
		MenuItem(""Don't highlight differences between this topic and previous version"", ""Hide Changes"", federation.LinkMaker.LinkToTopic(aTopic.Fullname))
	}
	IfFalse
	{
		MenuItem(""Show differences between this topic and previous version"", ""Show Changes"", federation.LinkMaker.LinkToTopicWithDiffs(aTopic.Fullname))
	},
	aTopic.Version.IfNull
	{
		MenuItem(""Edit this topic"", ""Edit"", federation.LinkMaker.LinkToEditTopic(aTopic.Fullname))
	}
	Else
	{
		""""
	},
	MenuItem(""Show printable view of this topic"", ""Print"", federation.LinkMaker.LinkToPrintView(aTopic.Fullname)),
	MenuItem(""Show recently changed topics"", ""Recent Changes"", federation.LinkMaker.LinkToRecentChanges(aTopic.Namespace.Name)),
	MenuItem(""Show RRS feeds to keep up-to-date"", ""Subscriptions"", federation.LinkMaker.LinkToSubscriptions(aTopic.Namespace.Name)),
	MenuItem(""Show disconnected topics"", ""Lost and Found"", federation.LinkMaker.LinkToLostAndFound(aTopic.Namespace.Name)),
	MenuItem(""Find references to this topic"", ""Find References"", federation.LinkMaker.LinkToSearchFor(null, aTopic.Name)),
	MenuItem(""Rename this topic"", ""Rename"", federation.LinkMaker.LinkToRename(aTopic.Fullname)),
	""----"", Newline,
	[
		""||{T-}"",
		""'''Search'''"", 
		""||"",
		Newline, 
		""||{+}"",
		Presentations.FormStart(federation.LinkMaker.LinkToSearchNamespace(aTopic.Namespace.Name), ""get""),
		Presentations.HiddenField(""namespace"", aTopic.Namespace.Name),
		Presentations.InputField(""search"", """", 15),
		Presentations.ImageButton(""goButton"", federation.LinkMaker.LinkToImage(""images/go-dark.gif""), ""Search for this text""), 
		Presentations.FormEnd(),
		""||"",
		Newline
	],
	Newline, ""----"", Newline,
	[
		""'''History'''"", Newline,
		aTopic.Changes.Snip(5).Collect
		{ each |
			[
				""||{T-+}"", 
				Presentations.Link(federation.LinkMaker.LinkToTopic(each.Fullname), [each.Timestamp].ToString), 
				""||"", 
				Newline,
				""||{T-+}``"", 
				each.Author, 
				""``||"", 
				Newline
			]
		},
		Newline,
		MenuItem(""List all versions of this topic"", ""List all versions"", federation.LinkMaker.LinkToVersions(aTopic.Fullname)),
		aTopic.Version.IfNotNull
		{[
			Newline,
			Presentations.FormStart(federation.LinkMaker.LinkToRestore(aTopic.Fullname), ""post""),
			Presentations.HiddenField(""RestoreTopic"", aTopic.Fullname),
			Presentations.SubmitButton(""restoreButton"", ""Restore Version""), 
			Presentations.FormEnd(),
		]}
		Else
		{
			""""
		},
		Newline
	]

	]
}

";
			}
		}





	}
}
