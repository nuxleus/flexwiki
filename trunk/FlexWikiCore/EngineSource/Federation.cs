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
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;

using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("Federation", "Represents the entire Wiki federation")]
	public class Federation : BELObject
	{
		public DateTime Created = DateTime.Now;

		/// <summary>
		/// The registry of all known ContentBases.  Keys are root directories and values are ContentBases.
		/// </summary>
		Hashtable	 _RootToContentBaseMap = new Hashtable();

		/// <summary>
		/// Answer an enumeration of all the ContentBases
		/// </summary>
		public IEnumerable ContentBases
		{
			get
			{
				return _RootToContentBaseMap.Values;
			}
		}

		/// <summary>
		/// Answer the ContentBase registered for the given root (and create it if it doesn't exist)
		/// </summary>
		/// <param name="root">the directory</param>
		/// <returns>ContentBase</returns>
		public ContentBase ContentBaseForRoot(string root)
		{
			return ContentBaseForRoot(root, true);
		}

		/// <summary>
		/// Answer the ContentBase registered for the given root (and either answer null or create as indicated by parms)
		/// </summary>
		/// <param name="possiblyRelativeRoot">the directory (.\ is relative to the namespace config)</param>
		/// <param name="isCreatedIfAbsent">true to created and answer the ContentBase if it doesn't exist; else answer null if it doesn't</param>
		/// <returns>ContentBase or null if none registered</returns>
		public ContentBase ContentBaseForRoot(string possiblyRelativeRoot, bool isCreatedIfAbsent)
		{
			string root = possiblyRelativeRoot;
			if (FederationNamespaceMapFilename != null)
			{
				FileInfo fi = new FileInfo(FederationNamespaceMapFilename);
				root = AbsoluteRoot(possiblyRelativeRoot, fi.DirectoryName);
			}
			ContentBase answer = (ContentBase)( _RootToContentBaseMap[root]);
			if (answer != null)
				return answer;
			if (!isCreatedIfAbsent)
				return null;
			answer = ContentBase.zzzSecretDoNotUseNewContentBaseDemandCreatedInFederation(root, this);
			return answer;
		}

		/// <summary>
		/// Set the registered ContentBase for the given root; overwrites any existing one in place (so be careful)
		/// </summary>
		/// <param name="root"></param>
		/// <param name="cb"></param>
		public void SetContentBaseForRoot(string root, ContentBase cb)
		{
			_RootToContentBaseMap[root] = cb;
		}

		/// <summary>
		/// Answer the ContentBase for the given namespace (or null if it's an absent namespace)
		/// </summary>
		/// <param name="ns">Name of the namespace</param>
		/// <returns></returns>
		[ExposedMethod("GetNamespaceInfo", ExposedMethodFlags.CachePolicyNone, "Answer the given namespace")]
		public ContentBase ContentBaseForNamespace(string ns)
		{
			string root = RootForNamespace(ns);
			if (root == null)
				return null;
			return ContentBaseForRoot(root, true);
		}

		/// <summary>
		/// Answer the DynamicNamespace for the given namespace (or null if it's an absent namespace)
		/// </summary>
		/// <param name="ns">Name of the namespace</param>
		/// <returns></returns>
		[ExposedMethod("GetNamespace", ExposedMethodFlags.CachePolicyNone, "Answer an object whose properties are all of the topics in the given namespace")]
		public DynamicNamespace DynamicNamespaceForNamespace(string ns)
		{
			if (ContentBaseForNamespace(ns) == null)
				return null;
			return new DynamicNamespace(this, ns);
		}

		/// <summary>
		/// Answer the DynamicTopic for the given topic (or null if it's an absent)
		/// </summary>
		/// <param name="top">Name of the topic (including namespace)</param>
		/// <returns></returns>
		[ExposedMethod("GetTopic", ExposedMethodFlags.CachePolicyNone, "Answer an object whose properties are those of the given topic")]
		public DynamicTopic DynamicTopicForTopic(string top)
		{
			AbsoluteTopicName abs = new AbsoluteTopicName(top);
			if (abs.Namespace == null)
				throw new Exception("Only fully-qualified topic names can be used with GetTopic(): only got " + top);
			DynamicNamespace dns = DynamicNamespaceForNamespace(abs.Namespace);
			return dns.DynamicTopicFor(abs.Name);
		}

		
		/// <summary>
		/// Answer the TopicInfo for the given topic (or null if it's an absent)
		/// </summary>
		/// <param name="top">Name of the topic (including namespace)</param>
		/// <returns></returns>
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Get information about the given topic")]
		public TopicInfo GetTopicInfo(string top)
		{
			AbsoluteTopicName abs = new AbsoluteTopicName(top);
			if (abs.Namespace == null)
				throw new Exception("Only fully-qualified topic names can be used with GetTopicInfo(): only got " + top);
			return new TopicInfo(this, abs);
		}

		/// <summary>
		/// Answer the ContentBase for the given topic (will be based on its namespace)
		/// </summary>
		/// <param name="topic"></param>
		/// <returns></returns>
		public ContentBase ContentBaseForTopic(AbsoluteTopicName topic)
		{
			return ContentBaseForNamespace(topic.Namespace);
		}


		/// <summary>
		/// Invalidate and reload the information for the ContentBase with the given root
		/// </summary>
		/// <param name="root"></param>
		public void InvalidateRoot(string root)
		{
			ContentBase cb = ContentBaseForRoot(root, false);
			if (cb == null)
				return;
			cb.Validate();
		}

		Hashtable _NamespaceToRoot = new Hashtable();
		Hashtable _RootToNamespace = new Hashtable();

		string _DefaultNamespace;

		public string DefaultNamespace
		{
			get
			{
				return _DefaultNamespace;
			}
			set
			{
				_DefaultNamespace = value;
			}
		}

		public string FederationNamespaceMapFilename;

		/// <summary>
		/// Answer a new Federation
		/// </summary>
		public Federation(OutputFormat format, LinkMaker linker)
		{
			Initialize(format, linker);
		}

		void Initialize(OutputFormat format, LinkMaker linker)
		{
			Format = format;
			LinkMaker = linker;
		}

		/// <summary>
		/// Answer a new federation loaded from the given configuration file
		/// </summary>
		/// <param name="configFile">Path to the config file</param>
		public Federation(string configFile, OutputFormat format, LinkMaker linker)
		{
			Initialize(format, linker);
			FileInfo info = new FileInfo(configFile);
			LoadFromConfiguration(FederationConfiguration.FromFile(configFile), info.DirectoryName);
		}


		LinkMaker _LinkMaker;
		LinkMaker LinkMaker
		{
			get
			{
				return _LinkMaker;
			}
			set
			{
				_LinkMaker = value;
			}
		}

		[ExposedMethod("LinkMaker", ExposedMethodFlags.CachePolicyNone, "Answer a LinkMaker configured to produce hyperlinks for the federation")]
		public LinkMaker ExposedLinkMaker
		{
			get
			{
				return LinkMaker;
			}
		}


		OutputFormat _Format;
		OutputFormat Format
		{
			get
			{
				return _Format;
			}
			set
			{
				_Format = value;
			}
		}


		/// <summary>
		/// Answer whether a topic exists and is writable
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <returns>true if the topic exists AND is writable by the current user; else false</returns>
		public bool IsExistingTopicWritable(AbsoluteTopicName topic)
		{
			ContentBase cb = ContentBaseForTopic(topic);
			if (cb == null)
				return false;
			return cb.IsExistingTopicWritable(topic);
		}


		IFederationCache _FederationCache;
		public IFederationCache FederationCache
		{
			get
			{
				return _FederationCache;
			}
			set
			{
				_FederationCache = value;
			}
		}


		public string GetTopicUnformattedContent(AbsoluteTopicName name)
		{
			CachedTopic top = GetCachedTopic(name);
			if (top == null)
				return null;
			return top.UnformattedContent;
		}

		public DateTime GetTopicCreationTime(AbsoluteTopicName name)
		{
			CachedTopic top = GetCachedTopic(name);
			if (top == null)
				return DateTime.MinValue;
			return top.CreationTime;
		}

		public DateTime GetTopicModificationTime(AbsoluteTopicName name)
		{
			CachedTopic top = GetCachedTopic(name);
			if (top == null)
				return DateTime.MinValue;
			return top.LastModified;
		}

		public string GetTopicLastModifiedBy(AbsoluteTopicName name)
		{
			CachedTopic top = GetCachedTopic(name);
			if (top == null)
				return null;
			return top.LastModifiedBy;
		}

		public Hashtable GetTopicProperties(AbsoluteTopicName name)
		{
			CachedTopic top = GetCachedTopic(name);
			if (top == null)
				return null;
			return top.Properties;
		}

		static string TopicFormattedContentKey(AbsoluteTopicName name, bool includeDiffs)
		{
			return "FormattedPage." + name.FullnameWithVersion + (includeDiffs ? "/diff" : "");
		}

		public string TopicFormattedBorderKey(AbsoluteTopicName name, Border border)
		{
			return "FormattedBorder." + name.FullnameWithVersion + "." + border.ToString();
		}


		public string GetTopicFormattedContent(AbsoluteTopicName name, bool includeDiffs)
		{
			string key = TopicFormattedContentKey(name, includeDiffs);
			string answer = null;
			if (FederationCache != null)
				answer = (string)FederationCache.Get(key);
			if (answer != null)
				return answer;
			CompositeCacheRule rule = new CompositeCacheRule();				
			answer = Formatter.FormattedTopic(name, Format, includeDiffs,  this, LinkMaker, rule);
			if (FederationCache != null)
				FederationCache.Put(key, answer, rule);
			return answer;
		}

		public string GetTopicFormattedBorder(AbsoluteTopicName name, Border border)
		{
			string key = TopicFormattedBorderKey(name, border);
			string answer = null;
			if (FederationCache != null)
				answer = (string)FederationCache.Get(key);
			if (answer != null)
				return answer;
			// OK, we need to figure it out.  
			CompositeCacheRule rule = new CompositeCacheRule();				
			IEnumerable borderText = BorderText(name, border, rule);
			WikiOutput output = new HTMLWikiOutput(true);
			foreach (IBELObject borderComponent in borderText)
			{
				IOutputSequence seq = borderComponent.ToOutputSequence();
				// output sequence -> pure presentation tree
				IWikiToPresentation presenter = Formatter.WikiToPresentation(name, output, ContentBaseForTopic(name), LinkMaker, null, 0, rule);
				IPresentation pres = seq.ToPresentation(presenter);
				pres.OutputTo(output);
			}
			answer = output.ToString();
			if (FederationCache != null)
				FederationCache.Put(key, answer, rule);
			return answer;
		}

		/// <summary>
		/// Answer a list of the wikitext components (IBELObjects) of the given border.  If nothing specifies any border; answer the system default
		/// </summary>
		/// <param name="name"></param>
		/// <param name="border"></param>
		/// <param name="rule"></param> 
		/// <returns></returns>
		IEnumerable BorderText(AbsoluteTopicName name, Border border, CompositeCacheRule rule)
		{
			ArrayList answer = new ArrayList();
			ContentBase cb;
			string bordersTopicsProperty = "Borders";

			ArrayList borderTopics = new ArrayList();

			// Start with whatever the namespace defines
			if (Borders != null)
			{
				foreach (string at in ParseListPropertyValue(Borders))
				{
					AbsoluteTopicName abs = new AbsoluteTopicName(at);
					cb = ContentBaseForTopic(abs);
					if (abs == null || cb == null)
						throw new Exception("Unknown namespace listed in border topic (" + at +") listed in federation configuration Borders property.");
					borderTopics.Add(at);
				}
			}


			// If the namespace, specifies border topics, get them
			cb = ContentBaseForTopic(name);
			if (cb != null)
			{
				borderTopics.AddRange(GetTopicListPropertyValue(cb.DefinitionTopicName, bordersTopicsProperty));
				rule.Add(cb.CacheRuleForAllPossibleInstancesOfTopic(cb.DefinitionTopicName));
			}

			// If there are no border topics specified for the federation or the namespace, add the default (_NormalBorders from the local namespace)
			if (borderTopics.Count == 0)
				borderTopics.Add("_NormalBorders");


			// Finally, any border elements form the topic itself (skip the def topic so we don't get double borders!)
			if (cb == null || cb.DefinitionTopicName.ToString() !=  name.ToString())
				borderTopics.AddRange(GetTopicListPropertyValue(name, bordersTopicsProperty));


			Set done = new Set();
			foreach (string borderTopicName in borderTopics)
			{
				// Figure out what the absolute topic name is that we're going to get this topic from
				RelativeTopicName rel = new RelativeTopicName(borderTopicName);
				if (rel.Namespace == null)
					rel.Namespace = name.Namespace;
				AbsoluteTopicName abs = new AbsoluteTopicName(rel.Name, rel.Namespace);
				if (done.Contains(abs))
					continue;
				done.Add(abs);
				IBELObject s = BorderPropertyFromTopic(name, abs, border, rule);
				if (s != null)
			 		answer.Add(s);
			}			

			return answer;
		}


		IBELObject BorderPropertyFromTopic(AbsoluteTopicName relativeToTopic, AbsoluteTopicName abs, Border border, CompositeCacheRule rule)
		{
			ContentBase cb = ContentBaseForTopic(abs);
			if (cb == null)
				return null;
			rule.Add(cb.CacheRuleForAllPossibleInstancesOfTopic(abs));
			if (!cb.TopicExists(abs))
				return null;

			// OK, looks like the topic exist -- let's see if the property is there
			string borderPropertyName = BorderPropertyName(border);
			string prop = GetTopicProperty(abs, borderPropertyName);
			if (prop == null || prop == "")
				return null;

			// Yup, so evaluate it!
			string code = "federation.GetTopic(\"" + abs.Fullname + "\")." + borderPropertyName + "(federation.GetTopicInfo(\"" + relativeToTopic + "\"))";

			BehaviorInterpreter interpreter = new BehaviorInterpreter(code, this, this.WikiTalkVersion, null);
			if (!interpreter.Parse())
				throw new Exception("Border property expression failed to parse.");
			TopicContext topicContext = new TopicContext(this, this.ContentBaseForTopic(abs), new TopicInfo(this, abs));
			IBELObject obj = interpreter.EvaluateToObject(topicContext, null);
			if (interpreter.ErrorString != null)
				obj = new BELString(interpreter.ErrorString);

			foreach (CacheRule r in interpreter.CacheRules)
				rule.Add(r);
			return obj;
		}

		public ILogEventFactory _LogEventFactory;

		public ILogEventFactory LogEventFactory
		{
			get
			{
				return _LogEventFactory;
			}
			set
			{
				_LogEventFactory = value;
			}
		}


		string BorderPropertyName(Border border)
		{
			switch (border)
			{
				case Border.Bottom:
					return "BottomBorder";
				case Border.Left:
					return "LeftBorder";
				case Border.Right:
					return "RightBorder";
				case Border.Top:
					return "TopBorder";
				default:
					return "";		// shouldn't really happen
			}
		}


		public IEnumerable GetTopicChanges(AbsoluteTopicName name)
		{
			CachedTopic top = GetCachedTopic(name);
			if (top == null)
				return null;
			return top.Changes;
		}

		static string TopicChangesKey(AbsoluteTopicName name)
		{
			return "TopicInfo." + name.Fullname;
		}

		CachedTopic GetCachedTopic(AbsoluteTopicName name)
		{
			string key = TopicChangesKey(name);
			CachedTopic answer = null;
			if (FederationCache != null)
				answer = (CachedTopic)FederationCache.Get(key);
			if (answer != null)
				return answer;
			ContentBase cb = ContentBaseForTopic(name);
			if (cb ==null || !cb.TopicExists(name))
				return null;
			answer = new CachedTopic(name);
			CompositeCacheRule rule = new CompositeCacheRule();
			rule.Add(cb.CacheRuleForAllPossibleInstancesOfTopic(name));
			answer.Changes = cb.AllChangesForTopic(name, rule);
			answer.CreationTime = cb.GetTopicCreationTime(name);
			answer.LastModified = cb.GetTopicLastWriteTime(name);
			answer.LastModifiedBy = cb.GetTopicLastAuthor(name);
			answer.UnformattedContent = cb.Read(name);
			answer.Properties = ContentBase.ExtractExplicitFieldsFromTopicBody(answer.UnformattedContent);
			ContentBase.AddImplicitPropertiesToHash(answer.Properties, name, answer.LastModifiedBy, answer.CreationTime, answer.LastModified, answer.UnformattedContent);
			if (FederationCache != null)
				FederationCache.Put(key, answer, rule);
			return answer;
		}

		[ExposedMethod("Namespaces", ExposedMethodFlags.CachePolicyNone, "Answer an array of namespaces in the federation")]
		public ArrayList AllNamespaces
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (string ns in Namespaces)
					answer.Add(ContentBaseForNamespace(ns));
				return answer;
			}
		}		

		public ICollection Namespaces
		{
			get
			{
				return _NamespaceToRoot.Keys;
			}
		}

		public ICollection Roots
		{
			get
			{
				return _RootToNamespace.Keys;
			}
		}

		public string NamespaceForRoot(string root)
		{
			if (_RootToNamespace.ContainsKey(root))
				return (string)(_RootToNamespace[root]);
			return null;
		}

		public string RootForNamespace(string ns)
		{
			if (null == ns)
				return null;
			if (_NamespaceToRoot.ContainsKey(ns))
				return (string)(_NamespaceToRoot[ns]);
			return null;
		}

		public ContentBase DefaultContentBase
		{
			get
			{
				return ContentBaseForNamespace(DefaultNamespace);
			}
		}

		/// <summary>
		/// Answer an asbolute path name for the given path; relatives (only those that start with '.\') are converted 
		/// to absolute relative to the given root
		/// </summary>
		/// <param name="possiblyRelativeRoot"></param>
		/// <returns></returns>
		static string AbsoluteRoot(string possiblyRelativeRoot, string rootBase)
		{
			if (!possiblyRelativeRoot.StartsWith(".\\"))
				return possiblyRelativeRoot;
			return rootBase + "\\" + possiblyRelativeRoot.Substring(2);			
		}

		/// <summary>
		/// Load the Federation up with all of the configuration information in the given FederationConfiguration.  
		/// Directories listed in each NamespaceToRoot are made relative to the given relativeDirectoryBase.
		/// If relativeDirectoryBase is null and a relative reference is used, an Exception will be throw
		/// </summary>
		/// <param name="config"></param>
		/// <param name="relativeDirectoryBase">Directory path (without the trailing slash)</param>
		public void LoadFromConfiguration(FederationConfiguration config, string relativeDirectoryBase)
		{
			FederationNamespaceMapFilename = config.FederationNamespaceMapFilename;
			_RootToContentBaseMap = new Hashtable();
			_NamespaceToRoot = new Hashtable();
			_RootToNamespace = new Hashtable();
			foreach (NamespaceToRoot map in config.NamespaceMappings)
			{
				string abs = AbsoluteRoot(map.Root, relativeDirectoryBase);

				Register(map.Namespace, abs);
				ContentBaseForRoot(abs, true);
				((ContentBase)_RootToContentBaseMap[abs]).Secure=map.Secure;
			}
			_DefaultNamespace = config.DefaultNamespace;
			Borders = config.Borders;
			AboutWikiString = config.AboutWikiString;
			WikiTalkVersion = config.WikiTalkVersion;
			DefaultDirectoryForNewNamespaces = config.DefaultDirectoryForNewNamespaces;

			FederationNamespaceMapLastRead = config.FederationNamespaceMapLastRead;
		}


		public string _DefaultDirectoryForNewNamespaces;
		public string DefaultDirectoryForNewNamespaces
		{
			get
			{
				return _DefaultDirectoryForNewNamespaces;
			}
			set
			{
				_DefaultDirectoryForNewNamespaces = value;
			}
		}

		public string _Borders;
		public string Borders
		{
			get
			{
				return _Borders;
			}
			set
			{
				_Borders = value;
			}
		}
		

		public string _AboutWikiString;
		[ExposedMethod("About", ExposedMethodFlags.CachePolicyNone, "Answer the 'about' string for the federation")]
		public string AboutWikiString
		{
			get
			{
				return _AboutWikiString;
			}
			set
			{
				_AboutWikiString = value;
			}
		}

		public int _WikiTalkVersion;
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the current version of the WikITalk language enabled for use in this Federation")]
		public int WikiTalkVersion
		{
			get
			{
				return _WikiTalkVersion;
			}
			set
			{
				_WikiTalkVersion = value;
			}
		}

		public void Register(string theNamespace, string root)
		{
			_NamespaceToRoot[theNamespace] = root;
			_RootToNamespace[root] = theNamespace;
		}

		DateTime FederationNamespaceMapLastRead = DateTime.MinValue;

		public void Validate()
		{
			foreach (ContentBase cb in ContentBases)
				cb.Validate();
			if (FederationNamespaceMapFilename == null || !File.Exists(FederationNamespaceMapFilename))
				return;
			DateTime lastmod = File.GetLastWriteTime(FederationNamespaceMapFilename);
			if (FederationNamespaceMapLastRead >= lastmod)
				return;
			FederationConfiguration config = FederationConfiguration.FromFile(FederationNamespaceMapFilename);
			FileInfo info = new FileInfo(FederationNamespaceMapFilename);
			LoadFromConfiguration(config, info.DirectoryName);
		}

		public void SetTopicProperty(AbsoluteTopicName topic, string field, string value, bool writeNewVersion)
		{
			ContentBaseForTopic(topic).SetFieldValue(topic, field, value, writeNewVersion);
		}

		public string GetTopicProperty(AbsoluteTopicName topic, string field)
		{
			Hashtable fields = GetTopicProperties(topic);
			if (fields == null || !fields.ContainsKey(field))
				return "";
			return (string)(fields[field]);
		}

		public ArrayList GetTopicListPropertyValue(AbsoluteTopicName topic, string field)
		{
			return ParseListPropertyValue(GetTopicProperty(topic, field));
		}

		static public ArrayList ParseListPropertyValue(string val)
		{
			ArrayList answer = new ArrayList();
			if (val == null || val.Length == 0)
				return answer;
			string [] vals = val.Split(new char[]{','});
			foreach (string s in vals)
			{
				answer.Add(s.Trim());
			}
			return answer;
		}
		
	}
}
