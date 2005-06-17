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
	public delegate void FederationUpdateEventHandler(object sender, FederationUpdateEventArgs e);

	/// <summary>
	/// A ContentBase is the interface to all the wiki topics in a namespace.  It shields the rest of the 
	/// wiki system from worrying about where the topics are stored.  It exposes operations
	/// like reading and writing a topic's content, enumerating all toics, etc.  In its current 
	/// implementation, all content is stored in the file system; thus, all file IO is hidden 
	/// in this class.
	/// </summary>
	/// 
	[ExposedClass("NamespaceInfo", "Provides information about a namespace")]
	public abstract class ContentBase : BELObject, IComparable
	{
    public delegate void FederationUpdateEventHandler(object sender, FederationUpdateEventArgs e);

    abstract protected class TopicData
    {
      abstract public string Author {get;}
      abstract public DateTime LastModificationTime {get;}
      abstract public string Name {get;}	// get the name of the topic (unqualified and without version)
      abstract public string Namespace {get;}
      abstract public string Version {get;}
    }

    protected class BackingTopicTopicData : TopicData
    {
      BackingTopic _Back;

      public BackingTopicTopicData(BackingTopic back)
      {
        _Back = back;
      }


      public override string Author
      {
        get
        {
          return _Back.LastAuthor;
        }
      }

      public override DateTime LastModificationTime
      {
        get
        {
          return _Back.LastModificationTime;
        }
      }

      public override string Name
      {
        get
        {
          return _Back.FullName.Name;
        }
      }

      public override string Namespace
      {
        get
        {
          return _Back.FullName.Namespace;
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

    }

		
    private static string  s_anonymousUserName = "anonymous";
    private static string s_DefinitionTopicLocalName = "_ContentBaseDefinition";
    /// <summary>
    /// The name of the topic that contains external wiki definitions
    /// </summary>
    private static string s_externalWikisTopic = "ExternalWikis";
    /// <summary>
    /// Regex pattern string for extracting the first line of a multi-line property
    /// </summary>
    private static string s_multilinePropertyPattern = "^(?<leader>:?)(?<name>([A-Z_][_a-zA-Z0-9]+)):(?<delim>[\\[{])(?<val>.*)";
    /// <summary>
    /// Regex pattern string for extracting a single line property
    /// </summary>
    private static string s_propertyPattern = "^(?<leader>:?)(?<name>([A-Z_][_a-zA-Z0-9.]+)):(?<val>[^\\[{].*)";
    /// <summary>
    /// Regex for extracting a single line property
    /// </summary>
    private static Regex s_propertyRegex = new Regex(s_propertyPattern);
    /// <summary>
    /// Regex for extracting a multi-line property
    /// </summary>
    private static Regex s_multilinePropertyRegex = new Regex(s_multilinePropertyPattern);
    
    private Hashtable _BackingTopics;
    private string _Contact;
    private string _Description;
    private bool _DisplaySpacesInWikiLinks;
    private Federation _Federation;
    private string _HomePage = "HomePage";
    private string _ImageURL;
    private ArrayList _ImportedNamespaces = new ArrayList();
    private string _Namespace;
    private string _Title;


    public static string AnonymousUserName 
    {
      get { return s_anonymousUserName; }
    }
    [XmlIgnore]
    public Hashtable BackingTopics
    {
      get
      {
        if (_BackingTopics != null)
        {
          return _BackingTopics;
        }

        _BackingTopics = new Hashtable();
        return _BackingTopics;
      }
    }

    [XmlIgnore]
    public CacheRule CacheRuleForDefinition
    {
      get
      {
        return new TopicsCacheRule(Federation, DefinitionTopicName);
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
        return new AllTopicsInNamespaceCacheRule(Federation, Namespace);
      }
    }

    /// <summary>
    /// Answer the contact info for the namespace (or null for none)
    /// </summary>
    public virtual string Contact
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

    public abstract DateTime	Created 
    {
      get;
    }

    public virtual string DefaultHomePageContent
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

    public virtual string DefaultNormalBordersContent
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
	request.UniqueVisitorEvents.Snip(15).Collect
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
		aTopic.Namespace.IsReadOnly.IfFalse
		{
			MenuItem(""Edit this topic"", ""Edit"", federation.LinkMaker.LinkToEditTopic(aTopic.Fullname))
		}
		IfTrue
		{
			""""
		}
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
	aTopic.Namespace.IsReadOnly.IfFalse
	{
		MenuItem(""Rename this topic"", ""Rename"", federation.LinkMaker.LinkToRename(aTopic.Fullname))
	}
	IfTrue
	{
		""""
	},
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

    public static string DefinitionTopicLocalName
    {
      get { return s_DefinitionTopicLocalName; }
    }

    /// <summary>
    /// Answer the description for the namespace (or null if none)
    /// </summary>
    public virtual string Description
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

    public virtual bool DisplaySpacesInWikiLinks
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

    public abstract bool Exists
    {
      get;
    }

    [ExposedMethod("IsReadOnly", ExposedMethodFlags.CachePolicyForever, "Answer true if the entire namespace and all it's topics are read-only.")]
		public bool ExposedIsReadOnly
		{
			get
			{
				return IsReadOnly;
			}
		}

    /// <summary>
    /// Answer true if the content base exists
    /// </summary>
    [ExposedMethod("Exists", ExposedMethodFlags.CachePolicyNone, "Answer true if this namespace actually exists")]
    public bool ExposedExists
    {
      get
      {
        return Exists;
      }
    }
	
    public static string ExternalWikisTopic
    {
      get { return s_externalWikisTopic; }
    }
    [XmlIgnore]
    public Federation Federation
    {
      get
      {
        return _Federation;
      }
    }

    /// <summary>
    /// Answer the human-friendly title for the ContentBase (Title if available, else Namespace)
    /// </summary>
    public virtual string FriendlyTitle
    {
      get
      {
        if (Title != null)
        {
          return Title;
        }
        else
        {
          return Namespace;
        }
      }
    }

    /// <summary>
    /// Answer the name of the wiki topic that serves as the "home" page for the namespace
    /// </summary>
    public virtual string HomePage
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

    public AbsoluteTopicName HomePageTopicName
    {
      get
      {
        return new AbsoluteTopicName(HomePage, Namespace);
      }
    }

    /// <summary>
    /// Answer the Image URL for the ContentBase (or null)
    /// </summary>
    public virtual string ImageURL
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

    /// <summary>
    /// Answer an enumeration of ContentBase objects that are for the content bases imported into this one
    /// </summary>
    [XmlIgnore]
    public IEnumerable ImportedContentBases
    {
      // TODO: How is this different from ImportedNamespaces? 
      get
      {
        ArrayList answer = new ArrayList();
        foreach (string ns in ImportedNamespaces)
        {
          ContentBase cb = Federation.ContentBaseForNamespace(ns);
          if (cb != null)
          {
            answer.Add(cb);
          }
        }
        return answer;
      }
    }   

    /// <summary>
    /// Answer (or set) an Enumeration of strings that are the names of namespaces imported into this content base
    /// </summary>
    [XmlIgnore]
    public ArrayList ImportedNamespaces
    {
      get
      {
        return _ImportedNamespaces;
      }
      set
      {
        // TODO: this shouldn't be like this - assigning to a collection is weird. 
        _ImportedNamespaces = value;
      }
    }

    public abstract bool IsReadOnly
		{
			get;
		}

		public abstract DateTime	LastRead 
		{
			get;
		}
    [XmlIgnore]
    public static Regex MultilinePropertyRegex
    {
      get 
      {
        return s_multilinePropertyRegex; 
      }
    }
    /// <summary>
    /// Answer the namespace for this ContentBase
    /// </summary>
    public virtual string Namespace
    {
      get
      {
        return _Namespace;
      }
      // acb - consider removing so name can come only from the class
      set
      {
        _Namespace = value;
      }
    }

    [XmlIgnore]
    public static Regex PropertyRegex
    {
      get
      {
        return s_propertyRegex; 
      }
    }
    /// <summary>
    /// The title of the content base  (or null if the ContentBase is bogus)
    /// </summary>
    public virtual string Title
    {
      get
      {
        return _Title;
      }
      // acb - consider removing so it can come only from the class
      set
      {
        _Title = value;
      }		
    }

		

    public event FederationUpdateEventHandler FederationUpdated;

    		
    /// <summary>
    /// Given a possibly relative topic name, answer all of the absolute topic names that actually exist
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public IList AllAbsoluteTopicNamesThatExist(TopicName topic)
    {
      ArrayList answer = new ArrayList();
      foreach (string ns in TopicNamespaces(topic))
      {
        answer.Add(new AbsoluteTopicName(topic.Name, ns));
      }
      return answer;
    }

    /// <summary>
    /// A list of TopicChanges to a topic (sorted by date)
    /// </summary>
    /// <param name="topic">The topic</param>
    /// <returns>Enumeration of TopicChanges </returns>
    public IEnumerable AllChangesForTopic(LocalTopicName topic)
    {
      return AllChangesForTopicSince(topic, DateTime.MinValue, null);
    }

    public IEnumerable AllChangesForTopic(LocalTopicName topic, CompositeCacheRule rule)
    {
      return AllChangesForTopicSince(topic, DateTime.MinValue, rule);
    }

    /// <summary>
    /// A list of TopicChanges to a topic since a given date [sorted by date]
    /// </summary>
    /// <param name="topic">A given date</param>
    /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
    /// <returns>Enumeration of TopicChanges</returns>
    public IEnumerable AllChangesForTopicSince(LocalTopicName topic, DateTime stamp)
    {
      return AllChangesForTopicSince(topic, stamp, null);
    }

    /// <summary>
    /// Answer a hash: keys are topic; values are an array of topic names for referenced topics (in this content base)
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
      {
        topicList = AllTopics(false);
      }
      else 
      {
        ArrayList list = new ArrayList();
        list.Add(filterToTopic);
        topicList = list;
      }

      foreach (AbsoluteTopicName topic in topicList)
      {
        string current = Federation.Read(topic);
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
              {
                absoluteNames.AddRange(alternate.AllAbsoluteTopicNamesFor(this));
              }
            }
          }
          allReferencedTopicsFromTopic.AddRange(absoluteNames);
        }
        answer[topic] = allReferencedTopicsFromTopic;
      }
      return answer;
    }

    /// <summary>
    /// Answer an (unsorted) enumeration of all topic in the ContentBase (possibly including those in imported namespaces, too)
    /// </summary>
    /// <param name="includeImports">true to include topics from included namespaces (won't recurse)</param>
    /// <returns>Enumeration of AbsoluteTopicNames</returns>
    public IEnumerable AllTopics(bool includeImports)
    {
      IEnumerable unsortedTopics = AllTopicsUnsorted();
      if (!includeImports)
      {
        return unsortedTopics;
      }

      // If we're asked for imports, it's more complex
      ArrayList answer = new ArrayList();
      foreach (object topic in unsortedTopics)
      {
        answer.Add(topic);
      }

      foreach (ContentBase contentBase in ImportedContentBases)
      {
        foreach (object topic in contentBase.AllTopics(false))
        {
          answer.Add(topic);
        }
      }

      return answer;
    }

    /// <summary>
    /// Returns a list of all topics in this namespace (including imported namespaces)
    /// </summary>
    [ExposedMethod(ExposedMethodFlags.CachePolicyComplex, "Answer a list of all topics in this namespace (including imported namespaces)")]
    public ArrayList AllTopicsInfo(ExecutionContext ctx)
    {
      ArrayList answer = new ArrayList();
      foreach (AbsoluteTopicName name in AllTopics(true))
      {
        answer.Add(new TopicInfo(Federation, name));
      }

      // Add cache rules for all the topics in the namespaces and for the definition (in case the imports change)
      ctx.AddCacheRule(new AllTopicsInNamespaceCacheRule(Federation, Namespace));
      ctx.AddCacheRule(CacheRuleForDefinition);
      foreach (string ns in ImportedNamespaces)
      {
        ctx.AddCacheRule(new AllTopicsInNamespaceCacheRule(Federation, ns));
      }

      return answer;
    }

    /// <summary>
    /// Answer an enumeration of all topic in the ContentBase, sorted by last modified (does not include those in imported namespaces)
    /// </summary>
    /// <returns>Enumeration of AbsoluteTopicNames</returns>
    public abstract IEnumerable AllTopicsSortedLastModifiedDescending();
    [ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Gets the topics with the specified property and value (including those in the imported namespaces).  If desiredValue is omitted, all topics with the property are answered.")]
    public TopicInfoArray AllTopicsWith(ExecutionContext ctx, string property, [ExposedParameter(true)] string desiredValue)
    {
      ctx.AddCacheRule(new PropertyCacheRule(this.Federation, property));
      return this.RetrieveAllTopicsWith(property, desiredValue, true);
    }

    /// <summary>
    /// Answer all of the versions for a given topic
    /// </summary>
    /// <remarks>
    /// TODO: Change this to return TopicChanges instead of the TopicNames
    /// </remarks>
    /// <param name="topic">A topic</param>
    /// <returns>Enumeration of the topic names (with non-null versions in them) </returns>
    public abstract IEnumerable AllVersionsForTopic(LocalTopicName topic);
    /// <summary>
    /// A list of TopicChanges to a topic since a given date [sorted by date]
    /// </summary>
    /// <param name="topic">A given date</param>
    /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
    /// <param name="rule">A composite cache rule to fill with rules that represented accumulated dependencies (or null)</param>
    /// <returns>Enumeration of TopicChanges</returns>
    public abstract IEnumerable AllChangesForTopicSince(LocalTopicName topic, DateTime stamp, CompositeCacheRule rule);
    public CacheRule CacheRuleForAllPossibleInstancesOfTopic(TopicName aName)
    {
      TopicsCacheRule rule = new TopicsCacheRule(Federation);
      foreach (AbsoluteTopicName possible in aName.AllAbsoluteTopicNamesFor(this))
      {
        rule.AddTopic(possible);
      }

      CompositeCacheRule answer = new CompositeCacheRule();
      answer.Add(rule);
      answer.Add(CacheRuleForDefinition);		// Add the cache rule for the content base, too, since if that changes there might be a change in imports
      return answer;
    }

    public static string ClosingDelimiterForOpeningMultilinePropertyDelimiter(string open)
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

    public int CompareTo(object obj)
    {
      if (!(obj is ContentBase))
      {
        throw new ArgumentException("object is not a ContentBase");
      }
      return((new CaseInsensitiveComparer()).Compare(this.FriendlyTitle, ((ContentBase)(obj)).FriendlyTitle) );
    }

    /// <summary>
    /// Delete a content base (kills everything inside recursively).  Note that this does *not* include unregistering
    /// the content base within the federation.
    /// </summary>
    public abstract void Delete();
    /// <summary>
    /// Delete a topic
    /// </summary>
    /// <param name="topic"></param>
    public abstract void DeleteTopic(LocalTopicName topic);
    /// <summary>
    /// Answer the contact info for the ContentBase (or null)
    /// </summary>
    [ExposedMethod("Contact", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer a string identifying the contact for this namespace")]
    public string ExposedContact(ExecutionContext ctx)
    {
      ctx.AddCacheRule(CacheRuleForDefinition);
      return Contact;
    }
			
    /// <summary>
    /// Answer the description for the ContentBase(or null)
    /// </summary>
    [ExposedMethod("Description", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer a description of this namespace")]
    public string ExposedDescription(ExecutionContext ctx)
    {
      ctx.AddCacheRule(CacheRuleForDefinition);
      return Description;
    }

    /// <summary>
    /// Answer the Image URL for the ContentBase (or null)
    /// </summary>
    [ExposedMethod("ImageURL", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer the URL for an image that brand-marks this namespace")]
    public string ExposedImageURL(ExecutionContext ctx)
    {
      ctx.AddCacheRule(CacheRuleForDefinition);
      return ImageURL;
    }
			
    [ExposedMethod("ImportedNamespaces", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "A collection of imported namespaces")]
    public ArrayList ExposedImports(ExecutionContext ctx)
    {
      ctx.AddCacheRule(CacheRuleForDefinition);
      ArrayList answer = new ArrayList();
      foreach (ContentBase cb in ImportedContentBases)
      {
        answer.Add(cb);
      }
      return answer;
    }

    /// <summary>
    /// Answer the namespace for this ContentBase
    /// </summary>
    [ExposedMethod("Name", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer the name of this namespace")]
    public string ExposedName(ExecutionContext ctx)
    {
      ctx.AddCacheRule(CacheRuleForDefinition);
      return Namespace;
    }
			
    /// <summary>
    /// Answer the human-friendly title for the ContentBase (Title if available, else Namespace)
    /// </summary>
    [ExposedMethod("Title", ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer a title for this namespace")]
    public string ExposedTitle(ExecutionContext ctx)
    {
      ctx.AddCacheRule(CacheRuleForDefinition);
      return FriendlyTitle;
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
            {
              includedDelim = delim;
            }
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
          {
            val = delim + val;
          }
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

    /// <summary>
    /// Answer a Hashtable of the external wikis (for this ContentBase) and the replacement patterns
    /// </summary>
    /// <returns>Hashtable (keys = external wikii names, values = replacement patterns) </returns>
    public virtual Hashtable ExternalWikiHash()
    {
      Hashtable answer = new Hashtable();
      string lines;
      try
      {
        lines = Read(new LocalTopicName(ExternalWikisTopic));
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

    public BackingTopic GetBackingTopicNamed(LocalTopicName topic)
    {
      return (BackingTopic)(BackingTopics[topic.Name]);
    }

    /// <summary>
    /// Reach and answer all the properties (aka fields) for the given topic.  This includes both the 
    /// properties defined in the topic plus the extra properties that every topic has (e.g., _TopicName, _TopicFullName, _LastModifiedBy, etc.)
    /// </summary>
    /// <param name="topic"></param>
    /// <returns>Hashtable (keys = string property names, values = values [as strings]);  or null if the topic doesn't exist</returns>
    public Hashtable GetFieldsForTopic(LocalTopicName topic)
    {
      if (!TopicExistsLocally(topic))
      {
        return null;
      }

      string allLines = Read(topic);
      Hashtable answer = ExtractExplicitFieldsFromTopicBody(allLines);	
      Federation.AddImplicitPropertiesToHash(answer, topic.AsAbsoluteTopicName(Namespace), GetTopicLastAuthor(topic), GetTopicCreationTime(topic), GetTopicLastWriteTime(topic), allLines);
      return answer;
    }

    /// <summary>
    /// Answer when a topic was created
    /// </summary>
    /// <param name="topic">The topic</param>
    /// <returns></returns>
    public abstract DateTime GetTopicCreationTime(LocalTopicName topic);
    [ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Get information about the given topic")]
    public TopicInfo GetTopicInfo(string topicName)
    {
      AbsoluteTopicName abs = new AbsoluteTopicName(topicName, Namespace);
      return new TopicInfo(Federation, abs);
    }
    /// <summary>
    /// Answer when the topic was last changed
    /// </summary>
    /// <param name="topic">A topic name</param>
    /// <returns></returns>
    /// <summary>
    /// Answer the identify of the author who last modified a given topic
    /// </summary>
    /// <param name="topic"></param>
    /// <returns>a user name</returns>
    public abstract string GetTopicLastAuthor(LocalTopicName topic);
    public abstract DateTime GetTopicLastWriteTime(LocalTopicName topic);
    public static bool IsBehaviorPropertyDelimiter(string s)
    {
      return s == "{" || s == "}";
    }

    /// <summary>
    /// Answer whether a topic exists and is writable
    /// </summary>
    /// <param name="topic">The topic (must directly be in this content base)</param>
    /// <returns>true if the topic exists AND is writable by the current user; else false</returns>
    public abstract bool IsExistingTopicWritable(LocalTopicName topic);
    /// <summary>
    /// Returns the most recent version for the given topic
    /// </summary>
    /// <param name="topic">The topic</param>
    /// <returns>The most recent version string for the topic</returns>
    public abstract string LatestVersionForTopic(LocalTopicName topic);
    /// <summary>
    /// Answer the DateTime of when any of the topics in the ContentBase where last modified.
    /// </summary>
    /// <param name="includeImports">true if you also want to include all imported namespaces</param>
    /// <returns></returns>
    public virtual DateTime LastModified(bool includeImports)
    {
      DateTime answer;
      IEnumerable en = AllTopicsSortedLastModifiedDescending();
      IEnumerator e = en.GetEnumerator();
      e.MoveNext();
      AbsoluteTopicName mostRecentlyChangedTopic = (AbsoluteTopicName)(e.Current);
      answer = GetTopicLastWriteTime(mostRecentlyChangedTopic.LocalName);

      if (!includeImports)
      {
        return answer;
      }
      foreach (ContentBase each in ImportedContentBases)
      {
        DateTime thatOne = each.LastModified(false);
        if (thatOne < answer)
        {
          answer = thatOne;
        }
      }
      return answer;
    }

    /// <summary>
    /// Answer the contents of a given topic
    /// </summary>
    /// <param name="topic">The topic</param>
    /// <returns>The contents of the topic or null if it can't be read (e.g., doesn't exist)</returns>
    public string Read(LocalTopicName topic)
    {
      using (TextReader st = TextReaderForTopic(topic))
      {
        if (st == null)
        {
          return null;
        }
        return st.ReadToEnd();
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
    public abstract ArrayList RenameTopic(LocalTopicName oldName, string newName, bool fixup);
    /// <summary>
    /// Rename references (in a given topic) from one topic to a new name 
    /// </summary>
    /// <param name="topicToLookIn"></param>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    public  bool RenameTopicReferences(LocalTopicName topicToLookIn, AbsoluteTopicName oldName, AbsoluteTopicName newName, FederationUpdateGenerator gen)
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
        {
          continue;
        }

        // Now see if we got any hits or not
        string rep = Formatter.beforeWikiName + "(" + Formatter.RegexEscapeTopic(each) + ")" + Formatter.afterWikiName;
        // if the reference was fully qualified, retain that form in the new reference
        string replacementName = each.IndexOf(".") > -1 ? newName.Fullname : newName.Name;
        current = Regex.Replace(current, rep, "${before}" + replacementName + "${after}");
        any = true;
      }

      if (any)
      {
        WriteTopicAndNewVersion(topicToLookIn, current, gen);
      }

      return any;
    }

    /// <summary>
    /// Change the value of a property (aka field) in a a topic.  If the topic doesn't exist, it will be created.
    /// </summary>
    /// <param name="topic">The topic whose property is to be changed</param>
    /// <param name="field">The name of the property to change</param>
    /// <param name="rep">The new value for the field</param>
    public void SetFieldValue(LocalTopicName topic, string field, string rep, bool writeNewVersion)
    {
      if (!TopicExistsLocally(topic))
      {
        WriteTopic(topic, "");
      }

      string original = Read(topic);

      // Multiline values need to end a complete line
      string repWithLineEnd = rep;
      if (!repWithLineEnd.EndsWith("\n"))
      {
        repWithLineEnd = repWithLineEnd + "\n";
      }
      bool newValueIsMultiline = rep.IndexOf("\n") > 0;

      string simpleField = "(?<name>(" + field + ")):(?<val>[^\\[].*)";
      string multiLineField = "(?<name>(" + field + ")):\\[(?<val>[^\\[]*\\])";

      string update = original;
      if (new Regex(simpleField).IsMatch(original))
      {
        if (newValueIsMultiline)
        {
          update = Regex.Replace (original, simpleField, "${name}:[ " + repWithLineEnd + "]");
        }
        else
        {
          update = Regex.Replace (original, simpleField, "${name}: " + rep);
        }
      }
      else if (new Regex(multiLineField).IsMatch(original))
      {
        if (newValueIsMultiline)
        {
          update = Regex.Replace (original, multiLineField, "${name}:[ " + repWithLineEnd + "]");
        }
        else
        {
          update = Regex.Replace (original, multiLineField, "${name}: " + rep);
        }
      }
      else
      {
        if (!update.EndsWith("\n"))
        {
          update = update + "\n";
        }

        if (rep.IndexOf("\n") == -1)
        {
          update += field + ": " + repWithLineEnd;
        }
        else
        {
          update += field + ":[ " + repWithLineEnd + "]\n";
        }
      }
      if (writeNewVersion)
      {
        WriteTopicAndNewVersion(topic, update);
      }
      else
      {
        WriteTopic(topic, update);
      }
    }

    /// <summary>
    /// Answer a TextReader for the given topic
    /// </summary>
    /// <param name="topic"></param>
    /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
    /// <returns>TextReader</returns>
    public abstract TextReader TextReaderForTopic(LocalTopicName topic);
    /// <summary>
    /// Answer true if the given topic exists in this ContentBase or in an imported namespace (if it's relative), or any namespace (if it's absolute)
    /// </summary>
    /// <param name="topic">The topic to check for</param>
    /// <returns>true if the topic exists</returns>
    public bool TopicExists(TopicName topic)
    {
      // Is it here?
      if (topic.Namespace == Namespace)
      {
        return TopicExistsLocally(topic.LocalName);
      }

      // Is it absolute, so we can just ask the Fed?
      if (topic.Namespace != null)
      {
        return Federation.TopicExists(topic.AsAbsoluteTopicName(Namespace));
      }

      // If the namespace is unspecified, it could just be in this one
      if (topic.Namespace == null && TopicExistsLocally(topic.LocalName))
      {
        return true;
      }

      // Is it in an imported namespace?
      foreach (string ns in ImportedNamespaces)
      {
        if (topic.Namespace != null && ns != topic.Namespace)
        {
          continue;
        }

        ContentBase cb = Federation.ContentBaseForNamespace(ns);
        if (cb == null)
        {
          continue;
        }
        if (cb.TopicExistsLocally(topic.LocalName))
        {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Answer true if a topic with the given name exists in this namespace
    /// </summary>
    /// <param name="name">Unqualified topic name</param>
    /// <returns></returns>
    public bool TopicExistsLocally(string name)
    {
      return TopicExistsLocally(new AbsoluteTopicName(name).LocalName);
    }

    /// <summary>
    /// Answer true if a topic exists in this ContentBase
    /// </summary>
    /// <param name="name">Name of the topic</param>
    /// <returns>true if it exists</returns>
    public abstract bool TopicExistsLocally(LocalTopicName name);
    /// <summary>
    /// Answer an AbsoluteTopicName for the given topic name local to this ContentBase.
    /// </summary>
    /// <param name="localTopicName">A topic name</param>
    /// <returns>An AbsoluteTopicName</returns>
    public AbsoluteTopicName TopicNameFor(string localTopicName)
    {
      return new AbsoluteTopicName(localTopicName, Namespace);
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
        {
          answer.Add(each.Namespace);
        }
      }
      return answer;
    }

    [ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Answer a list of all topic in this namespace (excluding imported namespaces)")]
    public ArrayList Topics(ExecutionContext ctx)
    {
      ArrayList answer = new ArrayList();
      foreach (AbsoluteTopicName name in AllTopics(false))
      {
        answer.Add(new TopicInfo(Federation, name));
      }

      // Add cache rules for all the topics in the namespaces and for the definition (in case the imports change)
      ctx.AddCacheRule(new AllTopicsInNamespaceCacheRule(Federation, Namespace));
			
      return answer;
    }
    [ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Gets the topics with the specified property and value (excluding those in the imported namespaces).  If desiredValue is omitted, all topics with the property are answered.")]
    public TopicInfoArray TopicsWith(ExecutionContext ctx, string property, [ExposedParameter(true)] string desiredValue)
		{
			ctx.AddCacheRule(new PropertyCacheRule(this.Federation, property));
			return this.RetrieveAllTopicsWith(property, desiredValue, false);
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
      {
        return null;
      }
      if (list.Count > 1)
      {
        throw TopicIsAmbiguousException.ForTopic(topic);
      }
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
      {
        return null;
      }
      if (list.Count > 1)
      {
        throw TopicIsAmbiguousException.ForTopic(topic);
      }
      string answer = null;
      foreach (string ns in list)
      {
        answer = ns;
      }
      return answer;
    }

    public abstract void Validate();
    /// <summary>
    /// Find the version of a topic immediately previous to another version
    /// </summary>
    /// <param name="topic">The name (with version) of the topic for which you want the change previous to</param>
    /// <returns>TopicChange or null if none</returns>
    public AbsoluteTopicName VersionPreviousTo(LocalTopicName topic)
    {
      bool next = false;
      bool first = true;
      AbsoluteTopicName answer = topic.AsAbsoluteTopicName(Namespace);
      foreach (TopicChange ver in AllChangesForTopic(topic))
      {
        answer.Version = ver.Version;
        if (next)
        {
          return answer;
        }
        if (topic.Version == null && !first)	// The version prior to the most recent is the second in line
        {
          return answer;
        }
        if (ver.Version == topic.Version)
        {
          next = true;
        }
        first = false;
      }
      return null;
    }

    /// <summary>
    /// Write a new version of the topic (doesn't write a new version).
    /// </summary>
    /// <param name="topic">Topic to write</param>
    /// <param name="content">New content</param>
    public void WriteTopic(LocalTopicName  topic, string content)
    {
      WriteTopic(topic, content,  CreateFederationUpdateGenerator());
    }

    /// <summary>
    /// Write a topic (and create a historical version)
    /// </summary>
    /// <param name="topic">The topic to write</param>
    /// <param name="content">The content</param>
    public void WriteTopicAndNewVersion(LocalTopicName topic, string content)
    {
      WriteTopicAndNewVersion(topic, content, CreateFederationUpdateGenerator());
    }



    /// <summary>
    /// Answer an (unsorted) enumeration of all topic in the ContentBase (doesn't include imports)
    /// </summary>
    /// <returns>Enumeration of AbsoluteTopicNames</returns>
    protected abstract IEnumerable AllTopicsUnsorted();
    /// <summary>
    /// Create a new FederationUpdateGenerator and hook its GenerationComplete event so that when that fires, this ContentBase fires its FederationUpdate event
    /// </summary>
    protected FederationUpdateGenerator CreateFederationUpdateGenerator()
    {
      FederationUpdateGenerator answer = new FederationUpdateGenerator();
      answer.GenerationComplete += new FederationUpdateGenerator.GenerationCompleteEventHandler(FederationUpdateGeneratorGenerationComplete);
      return answer;
    }

    protected void FillFederationUpdateByComparingPropertyHashes(FederationUpdateGenerator batch, AbsoluteTopicName topic, Hashtable oldProps, Hashtable newProps)
    {
      // Loop through the old ones to find any that are changed or removed in the new
      foreach (DictionaryEntry e in oldProps)
      {
        if (newProps.ContainsKey(e.Key))
        {
          object newValue = newProps[e.Key];
          if (newValue.ToString()  != e.Value.ToString())
          {
            batch.RecordPropertyChange(topic, e.Key.ToString(), FederationUpdate.PropertyChangeType.PropertyUpdate);
          }
        }
        else
        {
          batch.RecordPropertyChange(topic, e.Key.ToString(), FederationUpdate.PropertyChangeType.PropertyRemove);
        }
      }

      // And also find the added ones by identifying those that are in the new set, but not the old
      foreach (DictionaryEntry e in newProps)
      {
        if (!oldProps.ContainsKey(e.Key))
        {
          batch.RecordPropertyChange(topic, e.Key.ToString(), FederationUpdate.PropertyChangeType.PropertyAdd);
        }
      }
    }

    protected bool IsBackingTopic(LocalTopicName  top)
    {
      return BackingTopics.ContainsKey(top.Name);
    }

    // Invoke the FederationUpdated event; called whenever topics or other things about the federation change
    protected virtual void OnFederationUpdated(FederationUpdateEventArgs e) 
    {
      if (FederationUpdated != null)
      {
        FederationUpdated(this, e);
      }
    }

    protected void SetFederation(Federation aFed)
    {
      _Federation = aFed;
    }

    /// <summary>
    /// Write a new version of the topic (doesn't write a new version).  Generate all needed federation update changes via the supplied generator.
    /// </summary>
    /// <param name="topic">Topic to write</param>
    /// <param name="content">New content</param>
    /// <param name="sink">Object to recieve change info about the topic</param>
    protected abstract void WriteTopic(LocalTopicName  topic, string content, FederationUpdateGenerator gen);

    /// <summary>
    /// A FederationUpdateGenerator has completed generation.  Fire a FederationUpdate event for this content base.
    /// </summary>
    private void FederationUpdateGeneratorGenerationComplete(object sender, GenerationCompleteEventArgs e)
    {
      OnFederationUpdated(new FederationUpdateEventArgs(e.Updates));
    }

    private TopicInfoArray RetrieveAllTopicsWith(string property, string desiredValue, bool includeImports)
		{
			// First, get all topics that have the property at all -- that's the hard work talking to the store
			TopicInfoArray all = Federation.CacheManager.GetTopicsWithProperty(property);
			if (all == null)
			{
				all = new TopicInfoArray();
				foreach (AbsoluteTopicName topic in AllTopics(true))
				{
					if (this.Federation.GetTopicProperty(topic, property) != "")
					{
						all.Add(new TopicInfo(Federation, topic));
					}
				}
				Federation.CacheManager.PutCachedTopicsWithProperty(property, all, new PropertyCacheRule(this.Federation, property));
			}

			// OK.  Now we've got the baseline set (and it's cached for future use).  Now filter out if needed based on desiredValue and includeImports
      if (desiredValue == null && includeImports)
      {
        return all;	// quick exit -- no filtering needed
      }

			TopicInfoArray answer = new TopicInfoArray();
			for (int i = 0; i < all.Count; i++)
			{
				TopicInfo each = (TopicInfo)(all.Item(i));
        if (!includeImports && (each.Fullname.Namespace != Namespace))
        {
          continue;
        }
				if (desiredValue == null)
				{
					answer.Add(each);
					continue;
				}
				object propertyValue = Federation.GetTopicProperty(each.Fullname, property);
        if (propertyValue == null)
        {
          continue;
        }
        if (((string)(propertyValue)).ToLower() != desiredValue.ToLower())
        {
          continue;
        }
				answer.Add(each);
			}
			return answer;
		}

    /// <summary>
    /// Write a topic (and create a historical version)
    /// </summary>
    /// <param name="topic">The topic to write</param>
    /// <param name="content">The content</param>
    private void WriteTopicAndNewVersion(LocalTopicName topic, string content, FederationUpdateGenerator gen)
    {
      gen.Push();
      LocalTopicName versionless = new LocalTopicName(topic.Name);

      bool isVersionlessNew = !TopicExistsLocally(versionless);
      bool isVersionedNew = !TopicExistsLocally(topic);

      string oldAuthor = null;
      if (!isVersionlessNew)
      {
        oldAuthor = GetTopicLastAuthor(topic);
      }

      // Write it
      WriteTopic(versionless, content, gen);
      WriteTopic(topic, content, gen);	

      //Generate author property change if needed
      if (!isVersionlessNew)
      {
        //See if the last modified by has changed (not this only happens when writing out the versioned tip file)
        string newLastAuthor = GetTopicLastAuthor(topic);
        if (oldAuthor != newLastAuthor)
        {
          gen.RecordPropertyChange(versionless.AsAbsoluteTopicName(Namespace), "_LastModifiedBy", FederationUpdate.PropertyChangeType.PropertyUpdate);
        }
      }

      gen.Pop();
    }

	}
}
