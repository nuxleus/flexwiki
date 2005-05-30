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
using System.Diagnostics;
using System.Reflection;
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
		public delegate void FederationUpdateEventHandler(object sender, FederationUpdateEventArgs e);
		public event FederationUpdateEventHandler FederationUpdated;

		// Invoke the FederationUpdated event; called whenever topics or other things about the federation change
		protected virtual void OnFederationUpdated(FederationUpdateEventArgs e) 
		{
			if (FederationUpdated != null)
				FederationUpdated(this, e);
		}

		public DateTime Created = DateTime.Now;

		/// <summary>
		/// The registry of all known ContentBases.  Keys are namespace names and values are ContentBases.
		/// </summary>
		Hashtable	 _NamespaceToContentBaseMap = new Hashtable();

		/// <summary>
		/// Answer an enumeration of all the ContentBases
		/// </summary>
		public IEnumerable ContentBases
		{
			get
			{
				return _NamespaceToContentBaseMap.Values;
			}
		}

		/// <summary>
		/// Create a namespace provider for the description and then give it a chance to create its namespace(s)
		/// </summary>
		/// <param name="def"></param>
		void LoadNamespacesFromProviderDefinition(NamespaceProviderDefinition def)
		{
			// We need to create it
			Type providerType = def.ProviderType;
			if (providerType == null)
				throw new Exception("Unrecognized type (" + def.Type + ") in assembly (" + (def.AssemblyName == null ? "[default]" : def.AssemblyName) + ")");
			object c = Activator.CreateInstance(providerType);
			INamespaceProvider provider = c as INamespaceProvider;
			if (provider == null)
				throw new Exception("Illegal type (" + def.Type + ") in assembly (" + (def.AssemblyName == null ? "[default]" : def.AssemblyName) + ").  Type must implement INamespaceProvider.");
			def.SetParametersInProvider(provider);
			provider.LoadNamespaces(this);
		}

		/// <summary>
		/// Register the given ContentBase in the federation.
		/// </summary>
		/// <param name="cb"></param>
		public void RegisterNamespace(ContentBase cb)
		{		
			_NamespaceToContentBaseMap[cb.Namespace] = cb;

			// Hook the FederationUpdate events so we can aggregate them for listeners to the Federation
			cb.FederationUpdated += new ContentBase.FederationUpdateEventHandler(FederationUpdateMonitor);	
			RecordNamespacesListChanged();
		}


		/// <summary>
		/// Unregister the given ContentBase in the federation.
		/// </summary>
		/// <param name="cb"></param>
		public void UnregisterNamespace(ContentBase cb)
		{		
			cb.FederationUpdated -= new ContentBase.FederationUpdateEventHandler(FederationUpdateMonitor);	
			_NamespaceToContentBaseMap.Remove(cb.Namespace);
			RecordNamespacesListChanged();
		}



		void FederationUpdateMonitor(object sender, FederationUpdateEventArgs e) 
		{
			// One of the content bases we're hooked to has fired a federation update event -- just pass it on
			OnFederationUpdated(new FederationUpdateEventArgs(e.Updates));
		}

		/// <summary>
		/// Answer the ContentBase for the given namespace (or null if it's an absent namespace)
		/// </summary>
		/// <param name="ns">Name of the namespace</param>
		/// <returns></returns>
		[ExposedMethod("GetNamespaceInfo", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer the given namespace")]
		public ContentBase ExposedContentBaseForNamespace(ExecutionContext ctx, string ns)
		{
			ctx.AddCacheRule(this.CacheRuleForNamespaces);
			ContentBase answer = ContentBaseForNamespace(ns);
			if (answer != null)
				ctx.AddCacheRule(answer.CacheRuleForDefinition);
			return answer;
		}


		FederationUpdateGenerator _UpdateGenerator= null;
		FederationUpdateGenerator UpdateGenerator
		{
			get
			{
				if (_UpdateGenerator != null)
					return _UpdateGenerator;
				_UpdateGenerator = new FederationUpdateGenerator();
				_UpdateGenerator.GenerationComplete += new FederationUpdateGenerator.GenerationCompleteEventHandler(FederationUpdateGeneratorGenerationComplete);
				return _UpdateGenerator;
			}
		}

		/// <summary>
		/// A FederationUpdateGenerator has completed generation.  Fire a FederationUpdate event for this content base.
		/// </summary>
		void FederationUpdateGeneratorGenerationComplete(object sender, GenerationCompleteEventArgs e)
		{
			OnFederationUpdated(new FederationUpdateEventArgs(e.Updates));
		}

		/// <summary>
		/// Answer the ContentBase for the given namespace (or null if it's an absent namespace)
		/// </summary>
		/// <param name="ns">Name of the namespace</param>
		/// <returns></returns>
		public ContentBase ContentBaseForNamespace(string ns)
		{
			return (ContentBase)(_NamespaceToContentBaseMap[ns]);
		}

		/// <summary>
		/// Answer the DynamicNamespace for the given namespace (or null if it's an absent namespace)
		/// </summary>
		/// <param name="ns">Name of the namespace</param>
		/// <returns></returns>
		[ExposedMethod("GetNamespace", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer an object whose properties are all of the topics in the given namespace")]
		public DynamicNamespace DynamicNamespaceForNamespace(ExecutionContext ctx, string ns)
		{
			if (ExposedContentBaseForNamespace(ctx, ns) == null)
				return null;
			return new DynamicNamespace(this, ns);
		}

		/// <summary>
		/// Answer the DynamicTopic for the given topic (or null if it's an absent)
		/// </summary>
		/// <param name="top">Name of the topic (including namespace)</param>
		/// <returns></returns>
		[ExposedMethod("GetTopic", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer an object whose properties are those of the given topic")]
		public DynamicTopic DynamicTopicForTopic(ExecutionContext ctx, string top)
		{
			AbsoluteTopicName abs = new AbsoluteTopicName(top);
			if (abs.Namespace == null)
				throw new Exception("Only fully-qualified topic names can be used with GetTopic(): only got " + top);
			DynamicNamespace dns = DynamicNamespaceForNamespace(ctx, abs.Namespace);
			ctx.AddCacheRule(new TopicsCacheRule(this, abs));
			return dns.DynamicTopicFor(abs.Name);
		}

		
		/// <summary>
		/// Answer the TopicInfo for the given topic (or null if it's an absent)
		/// </summary>
		/// <param name="top">Name of the topic (including namespace)</param>
		/// <returns></returns>
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Get information about the given topic")]
		public TopicInfo GetTopicInfo(ExecutionContext ctx, string top)
		{
			AbsoluteTopicName abs = new AbsoluteTopicName(top);
			ctx.AddCacheRule(new TopicsCacheRule(this, abs));
			return GetTopicInfo(top);
		}

		/// <summary>
		/// Answer the TopicInfo for the given topic (or null if it's an absent)
		/// </summary>
		/// <param name="top">Name of the topic (including namespace)</param>
		/// <returns></returns>
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


		Set _BlacklistedExternalLinkPrefixes = new Set();
		/// <summary>
		/// Answer the set of blacklisted link prefixes.  Treat this set as read-only, please, and use AddBlacklistedExternalLink() and RemoveBlacklistedExternalLink().
		/// </summary>
		public Set BlacklistedExternalLinkPrefixes
		{
			get
			{
				return _BlacklistedExternalLinkPrefixes;
			}
		}

		/// <summary>
		/// Answer whether or not a link is blacklisted.  You pass in a full URL and this does checking against all blacklist rules.
		/// </summary>
		/// <param name="url"></param>
		/// <returns>true if blacklisted</returns>
		public bool IsLinkBlacklisted(string url)
		{
			foreach (string prefix in BlacklistedExternalLinkPrefixes)
				if (url.ToUpper().StartsWith(prefix.ToUpper()))
					return true;
			return false;
		}

		public void AddBlacklistedExternalLinkPrefix(string link)
		{
			if (_BlacklistedExternalLinkPrefixes.Contains(link))
				return;
			_BlacklistedExternalLinkPrefixes.Add(link);
			RecordFederationPropertiesChanged();
		}

		public void RemoveBlacklistedExternalLinkPrefix(string link)
		{
			if (!_BlacklistedExternalLinkPrefixes.Contains(link))
				return;
			_BlacklistedExternalLinkPrefixes.Remove(link);
			RecordFederationPropertiesChanged();
		}


		/// <summary>
		/// Invalidate and reload the information for the ContentBase with the given namespace
		/// </summary>
		/// <param name="root"></param>
		public void InvalidateNamespace(string ns)
		{
			ContentBase cb = ContentBaseForNamespace(ns);
			if (cb == null)
				return;
			cb.Validate();
		}

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
				RecordFederationPropertiesChanged();
			}
		}

		void RecordFederationPropertiesChanged()
		{
			UpdateGenerator.Push();
			try
			{
				UpdateGenerator.RecordFederationPropertiesChanged();
			}
			finally
			{
				UpdateGenerator.Pop();
			}
		}

		void RecordNamespacesListChanged()
		{
			UpdateGenerator.Push();
			try
			{
				UpdateGenerator.RecordNamespaceListChanged();
			}
			finally
			{
				UpdateGenerator.Pop();
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
			InitializePerformanceCounters();
		}

		private static void InitializePerformanceCounters()
		{
			SetupPerformanceCounters();
		}

		public struct PerformanceCounterNames
		{
			public static string TopicReads = "Topic reads";
			public static string TopicWrite = "Topic writes";
			public static string TopicFormat = "Topic formats";
			public static string TopicsCompared = "Topics compared";
			public static string MethodInvocation = "WikiTalk method invocations";
		};

		private static string PerformanceCounterCategoryName = "FlexWiki";

		// key = string name; value = PerformanceCounter
		static Hashtable _PerformanceCounterHash = new Hashtable();

		public static PerformanceCounter GetPerformanceCounter(string name)
		{
			return (PerformanceCounter)(_PerformanceCounterHash[name]);
		}

		static bool PerformanceCounterCategoryExists
		{
			get
			{
				try
				{
					return PerformanceCounterCategory.Exists(PerformanceCounterCategoryName);
				}
				catch (Exception)
				{
					return false;
				}
			}
		}
			
		public static bool CreatePerformanceCounters()
		{        
			if (!PerformanceCounterCategoryExists)
			{
				CounterCreationDataCollection CCDC = new CounterCreationDataCollection();

				// Add the counter.
				CreateCounter(CCDC, PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterNames.TopicReads);
				CreateCounter(CCDC, PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterNames.TopicWrite);
				CreateCounter(CCDC, PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterNames.TopicFormat);
				CreateCounter(CCDC, PerformanceCounterType.RateOfCountsPerSecond64, PerformanceCounterNames.MethodInvocation);
            
				// Create the category.
				PerformanceCounterCategory.Create(PerformanceCounterCategoryName, 
					"FlexWiki application performance", 
					CCDC);                
				return(true);
			}
			else
			{
				return(false);
			}
		}

		public static void DeletePerformanceCounters()
		{
			PerformanceCounterCategory.Delete(PerformanceCounterCategoryName);
		}

		private static void CreateCounter(CounterCreationDataCollection ccdc, PerformanceCounterType type, string name)
		{
			CounterCreationData ccd;
			ccd = new CounterCreationData();
			ccd.CounterType = type;
			ccd.CounterName = name;
			ccdc.Add(ccd);
		}

		private static void SetupPerformanceCounters()
		{
			// If the counters exist, use them
			if (!PerformanceCounterCategoryExists) 
				return;
			SetupCounter(PerformanceCounterNames.TopicReads);
			SetupCounter(PerformanceCounterNames.TopicWrite);
			SetupCounter(PerformanceCounterNames.TopicFormat);
			SetupCounter(PerformanceCounterNames.TopicsCompared);
			SetupCounter(PerformanceCounterNames.MethodInvocation);
		}

		private static void SetupCounter(string name)
		{
			_PerformanceCounterHash[name] = InitializePerformanceCounter(name);
		}

		private static PerformanceCounter InitializePerformanceCounter(string name)
		{
			PerformanceCounter answer = new PerformanceCounter(PerformanceCounterCategoryName, name, false);
			answer.RawValue = 0;
			return answer;
		}


		/// <summary>
		/// Answer a new federation loaded from the given configuration file
		/// </summary>
		/// <param name="configFile">Path to the config file</param>
		public Federation(string configFile, OutputFormat format, LinkMaker linker)
		{
			Initialize(format, linker);
			FileInfo info = new FileInfo(configFile);
			LoadFromConfiguration(FederationConfiguration.FromFile(configFile));
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
				RecordFederationPropertiesChanged();
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
				RecordFederationPropertiesChanged();
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
			return cb.IsExistingTopicWritable(topic.LocalName);
		}


		/// <summary>
		/// Enable caching for this federation.  Use a default, internal cache that is just a dumb, in-memory cache.
		/// If you have a smarter cache available, consider using EnableCaching(IFederationCache aCache).
		/// </summary>
		public void EnableCaching()
		{
			EnableCaching(new GenericCache());
		}

		/// <summary>
		/// Enable caching for this federation and use the supplied IFederationCache as the underlying store for 
		/// the caching.  By using this method, you can pass in a cache that's smarter about memory allocation and 
		/// resource utilization than the GenericCache used by default.
		/// </summary>
		/// <param name="aCache"></param>
		public void EnableCaching(IFederationCache aCache)
		{
			_FederationCacheManager = new FederationCacheManager(this, aCache);
		}

		FederationCacheManager  _FederationCacheManager;
		public FederationCacheManager CacheManager
		{
			get
			{
				return _FederationCacheManager;
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

		public bool IsBlacklisted(string wikiText)
		{
			if (wikiText == null)
				return false;
			string proposed = wikiText;
			foreach (string each in BlacklistedExternalLinkPrefixes)
			{
				if (proposed.ToUpper().IndexOf(each.ToUpper()) >= 0)
					return true;
			}
			return false;
		}

		public string GetTopicFormattedContent(AbsoluteTopicName name, AbsoluteTopicName withDiffsToThisTopic)
		{
			string answer = null;
			if (CacheManager != null)
			{
				answer = (string)CacheManager.GetCachedTopicFormattedContent(name, withDiffsToThisTopic);
			}
			if (answer != null)
			{
				return answer;
			}
			CompositeCacheRule rule = new CompositeCacheRule();				

			// If the content is blacklisted and this is a historical version, answer dummy content
			if (name.Version != null && IsBlacklisted(GetTopicUnformattedContent(name)))
			{
				answer = Formatter.FormattedString(@"%red big%This historical version of this topic contains content that has been banned by policy from appearing on this site.",
					Format, this.ContentBaseForTopic(name), LinkMaker, rule);
			}
			else
			{
				answer = Formatter.FormattedTopic(name, Format, withDiffsToThisTopic,  this, LinkMaker, rule);
			}
			if (CacheManager != null)
			{
				CacheManager.PutCachedTopicFormattedContent(name, withDiffsToThisTopic, answer, rule);
			}
			return answer;
		}

		public string GetTopicFormattedBorder(AbsoluteTopicName name, Border border)
		{
			string answer = null;
			if (CacheManager != null)
			{
				answer = (string)CacheManager.GetCachedTopicFormattedBorder(name, border);
			}
			if (answer != null)
			{
				return answer;
			}
			// OK, we need to figure it out.  
			CompositeCacheRule rule = new CompositeCacheRule();				
			IEnumerable borderText = BorderText(name, border, rule);
			WikiOutput output = new HTMLWikiOutput(null);
			foreach (IBELObject borderComponent in borderText)
			{
				IOutputSequence seq = borderComponent.ToOutputSequence();
				// output sequence -> pure presentation tree
				IWikiToPresentation presenter = Formatter.WikiToPresentation(name, output, ContentBaseForTopic(name), LinkMaker, null, 0, rule);
				IPresentation pres = seq.ToPresentation(presenter);
				pres.OutputTo(output);
			}
			answer = output.ToString();
			if (CacheManager != null)
				CacheManager.PutCachedTopicFormattedBorder(name, border, answer, rule);
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

		CachedTopic GetCachedTopic(AbsoluteTopicName name)
		{
			CachedTopic answer = null;
			if (CacheManager != null)
				answer = (CachedTopic)CacheManager.GetCachedTopic(name);
			if (answer != null)
				return answer;
			ContentBase cb = ContentBaseForTopic(name);
			if (cb ==null || !cb.TopicExists(name))
				return null;
			answer = new CachedTopic(name);
			CompositeCacheRule rule = new CompositeCacheRule();
			rule.Add(cb.CacheRuleForAllPossibleInstancesOfTopic(name));
			answer.Changes = cb.AllChangesForTopic(name.LocalName, rule);
			answer.CreationTime = cb.GetTopicCreationTime(name.LocalName);
			answer.LastModified = cb.GetTopicLastWriteTime(name.LocalName);
			answer.LastModifiedBy = cb.GetTopicLastAuthor(name.LocalName);
			answer.UnformattedContent = cb.Read(name.LocalName);
			answer.Properties = ContentBase.ExtractExplicitFieldsFromTopicBody(answer.UnformattedContent);
			AddImplicitPropertiesToHash(answer.Properties, name, answer.LastModifiedBy, answer.CreationTime, answer.LastModified, answer.UnformattedContent);
			if (CacheManager != null)
				CacheManager.PutCachedTopic(name, answer, rule);
			return answer;
		}

		public CacheRule CacheRuleForNamespaces
		{
			get
			{
				return new FederationNamespacesCacheRule(this);
			}
		}

		public CacheRule CacheRuleForFederationProperties
		{
			get
			{
				return new FederationPropertiesCacheRule(this);
			}
		}

		[ExposedMethod("Namespaces", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer an array of namespaces in the federation")]
		public ArrayList ExposedNamespaces(ExecutionContext ctx)
		{
			ctx.AddCacheRule(this.CacheRuleForNamespaces);
			return AllNamespaces;
		}


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
				return _NamespaceToContentBaseMap.Keys;
			}
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
		public void LoadFromConfiguration(FederationConfiguration config)
		{
			try
			{
				_CurrentConfiguration = config;
				UpdateGenerator.Push();
				FederationNamespaceMapFilename = config.FederationNamespaceMapFilename;
				_NamespaceToContentBaseMap = new Hashtable();
				foreach (NamespaceProviderDefinition def in config.NamespaceMappings)
					LoadNamespacesFromProviderDefinition(def);
				_DefaultNamespace = config.DefaultNamespace;
				Borders = config.Borders;
				AboutWikiString = config.AboutWikiString;
				WikiTalkVersion = config.WikiTalkVersion;
				NoFollowExternalHyperlinks = config.NoFollowExternalHyperlinks != 0;
				foreach (string link in config.BlacklistedExternalLinks)
					AddBlacklistedExternalLinkPrefix(link);
				DefaultDirectoryForNewNamespaces = config.DefaultDirectoryForNewNamespaces;
				FederationNamespaceMapLastRead = config.FederationNamespaceMapLastRead;
				UpdateGenerator.RecordNamespaceListChanged();
				UpdateGenerator.RecordFederationPropertiesChanged();
			}
			finally
			{
				UpdateGenerator.Pop();
			}
		}

		bool _NoFollowExternalHyperlinks = false;
		public bool NoFollowExternalHyperlinks
		{
			get
			{
				return _NoFollowExternalHyperlinks;
			}
			set
			{
				if (value == _NoFollowExternalHyperlinks)
					return;
				_NoFollowExternalHyperlinks = value;
				RecordFederationPropertiesChanged();
			}
		}

		FederationConfiguration _CurrentConfiguration = null;
		public FederationConfiguration CurrentConfiguration
		{
			get
			{
				return _CurrentConfiguration;
			}
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
				RecordFederationPropertiesChanged();
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
				RecordFederationPropertiesChanged();
			}
		}
		

		public string _AboutWikiString;
		[ExposedMethod("About", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer the 'about' string for the federation")]
		public string ExposedAbout(ExecutionContext ctx)
		{
			ctx.AddCacheRule(CacheRuleForFederationProperties);
			return AboutWikiString;
		}

		public string AboutWikiString
		{
			get
			{
				return _AboutWikiString;
			}
			set
			{
				_AboutWikiString = value;
				RecordFederationPropertiesChanged();
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
				RecordFederationPropertiesChanged();
			}
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
			LoadFromConfiguration(config);
		}

		public void SetTopicProperty(AbsoluteTopicName topic, string field, string value, bool writeNewVersion)
		{
			ContentBaseForTopic(topic).SetFieldValue(topic.LocalName, field, value, writeNewVersion);
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

		public override bool Equals(object obj)
		{
			Federation other = obj as Federation;
			if (other == null)
				return false;
			if (other.FederationNamespaceMapFilename != FederationNamespaceMapFilename)
				return false;
			return true;
		}

		public override int GetHashCode()
		{
			int answer = 0;
			if (FederationNamespaceMapFilename != null)
				answer ^= FederationNamespaceMapFilename.GetHashCode();
			return answer;
		}

		/// <summary>
		/// Answer all the properties (aka fields) for the given topic.  Uncached!
		/// This includes both the  properties defined in the topic plus the extra properties that every 
		/// topic has (e.g., _TopicName, _TopicFullName, _LastModifiedBy, etc.)
		/// </summary>
		/// <param name="topic"></param>
		/// <returns>Hashtable (keys = string property names, values = values [as strings?]);  or null if the topic doesn't exist</returns>
		public Hashtable GetFieldsForTopic(AbsoluteTopicName topic)
		{
			ContentBase cb = ContentBaseForTopic(topic);
			if (cb == null)
				return null;
			return cb.GetFieldsForTopic(topic.LocalName);
		}


		/// <summary>
		/// Answer the contents of a given topic (uncached!)
		/// </summary>
		/// <param name="topic">The topic</param>
		/// <returns>The contents of the topic or null if it can't be read (e.g., doesn't exist)</returns>
		public string Read(AbsoluteTopicName topic)
		{
			ContentBase cb = ContentBaseForTopic(topic);
			if (cb == null)
				return null;
			return cb.Read(topic.LocalName);
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
			ContentBase cb = ContentBaseForTopic(oldName);
			if (cb == null)
				throw NamespaceNotFoundException.ForNamespace(oldName.Namespace);
			return cb.RenameTopic(oldName.LocalName, newName, fixup);	
		}

		public static void AddImplicitPropertiesToHash(Hashtable hash, AbsoluteTopicName topic, string lastModBy, DateTime creation, DateTime modification, string content)
		{
			// Remember that this list is closely bound to some implicit knowledge of what these properties are in the logic in WriteTopic that send change notifications for properties
			// If you add/change these properties, you need to carefully revie wthat code too to be sure it fires the right changed events
			hash["_TopicName"] = topic.Name;
			hash["_TopicFullName"] = topic.FullnameWithVersion;
			hash["_LastModifiedBy"] = lastModBy;
			hash["_CreationTime"] = creation.ToString();
			hash["_ModificationTime"] = modification.ToString();
			hash["_Body"] = content;
		}

		/// <summary>
		/// Answer true if the topic exists; else false
		/// </summary>
		public bool TopicExists(AbsoluteTopicName topic)
		{
			ContentBase cb = ContentBaseForTopic(topic);
			if (cb == null)
				return false;
			return cb.TopicExistsLocally(topic.LocalName);
		}

		public void DeleteTopic(AbsoluteTopicName topic)
		{
			ContentBase cb = ContentBaseForTopic(topic);
			if (cb == null)
				return ;
			cb.DeleteTopic(topic.LocalName);
		}

	}
}
