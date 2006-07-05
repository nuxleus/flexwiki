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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Caching;
using System.Xml.Serialization;

using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki
{
    /// <summary>
    /// A NamespaceManager is the interface to all the wiki topics in a namespace.  It shields the rest of the 
    /// wiki system from worrying about where the topics are stored.  It exposes operations
    /// like reading and writing a topic's content, enumerating all toics, etc.  
    /// </summary>
    /// 
    [ExposedClass("NamespaceInfo", "Provides information about a namespace")]
    public sealed class NamespaceManager : BELObject, IComparable
    {
        // Fields

        private static string s_bordersTopicLocalName = "_NormalBorders"; 
        private static string s_defaultHomePage = "HomePage";
        private static string s_definitionTopicName = "_ContentBaseDefinition";
        /// <summary>
        /// The name of the topic that contains external wiki definitions
        /// </summary>
        private static string s_externalWikisTopic = "ExternalWikis";

        private ContentProviderBase _contentProviderChain;
        private ExternalReferencesMap _emptyExternalReferencesMap = new ExternalReferencesMap();
        private Federation _federation;
        private readonly NamespaceProviderParameterCollection _parameters = new NamespaceProviderParameterCollection();
        private string _namespace;
        private ITimeProvider _timeProvider = new DefaultTimeProvider();

        // Constructors

        internal NamespaceManager(Federation federation, ContentProviderBase contentProvider,
            string ns, NamespaceProviderParameterCollection parameters)
        {
            _federation = federation;
            _contentProviderChain = contentProvider;
            _namespace = ns;

            if (parameters != null)
            {
                foreach (NamespaceProviderParameter parameter in parameters)
                {
                    _parameters.Add(parameter);
                }
            }

            _contentProviderChain.Initialize(this); 
        }

        // Properties

        public static string BordersTopicLocalName
        {
            get { return s_bordersTopicLocalName; }
        }
        public QualifiedTopicName BordersTopicName
        {
            get
            {
                return new QualifiedTopicName(BordersTopicLocalName, Namespace); 
            }
        }
        public string Contact
        {
            get
            {
                return GetLastPropertyValue(DefinitionTopicLocalName, "Contact");
            }
        }
        /// <summary>
        /// Answer the full <see cref="QualifiedTopicName" /> for the definition topic for this content base
        /// </summary>
        [XmlIgnore]
        public QualifiedTopicName DefinitionTopicName
        {
            get
            {
                return new QualifiedTopicName(DefinitionTopicLocalName, Namespace);
            }
        }
        public static string DefinitionTopicLocalName
        {
            get { return s_definitionTopicName; }
        }
        /// <summary>
        /// Answer the description for the namespace (or null if none)
        /// </summary>
        public string Description
        {
            get
            {
                TopicProperty property = GetTopicProperty(DefinitionTopicLocalName,
                    "Description");

                if (property == null)
                {
                    return "";
                }

                return property.LastValue;
            }
        }
        public bool DisplaySpacesInWikiLinks
        {
            get
            {
                TopicProperty property = GetTopicProperty(DefinitionTopicLocalName,
                    "DisplaySpacesInWikiLinks");

                if (property == null)
                {
                    return false;
                }

                if (!property.HasValue)
                {
                    return false;
                }

                string stringValue = property.LastValue;
                bool boolValue = false;
                bool conversionSucceeded = Boolean.TryParse(stringValue, out boolValue);

                // If the value is not a valid boolean, default to false. 
                if (conversionSucceeded)
                {
                    return boolValue;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool Exists
        {
            get
            {
                return ContentProviderChain.Exists;
            }
        }
        /// <summary>
        /// Answer true if the content base exists
        /// </summary>
        [ExposedMethod("Exists", ExposedMethodFlags.Default, "Answer true if this namespace actually exists")]
        public bool ExposedExists
        {
            get
            {
                return Exists;
            }
        }
        [ExposedMethod("IsReadOnly", ExposedMethodFlags.Default, "Answer true if the entire namespace and all it's topics are read-only.")]
        public bool ExposedIsReadOnly
        {
            get
            {
                return IsReadOnly;
            }
        }
        /// <summary>
        /// Answer a collection of the external wikis (for this ContentProviderChain) and the replacement patterns
        /// </summary>
        /// <returns>StringToStringMap (keys = external wiki names, values = replacement patterns) </returns>
        public ExternalReferencesMap ExternalReferences
        {
            get
            {
                ParsedTopic parsedTopic = ContentProviderChain.GetParsedTopic(new UnqualifiedTopicRevision(ExternalWikisTopic));

                if (parsedTopic == null)
                {
                    return EmptyExternalReferencesMap;
                }

                return parsedTopic.ExternalReferences;
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
                return _federation;
            }
        }
        /// <summary>
        /// Answer the human-friendly title for the ContentProviderChain (Title if available, else Namespace)
        /// </summary>
        public string FriendlyTitle
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
        public string HomePage
        {
            get
            {
                TopicProperty property = GetTopicProperty(DefinitionTopicLocalName,
                    "HomePage");

                if (property == null)
                {
                    return NamespaceManager.s_defaultHomePage;
                }

                if (!property.HasValue)
                {
                    return NamespaceManager.s_defaultHomePage;
                }

                return property.LastValue;

            }
            set
            {
                SetTopicPropertyValue(DefinitionTopicLocalName, "HomePage", value, false, "system"); 
            }
        }
        public QualifiedTopicName HomePageTopicName
        {
            get
            {
                return new QualifiedTopicName(HomePage, Namespace);
            }
        }
        /// <summary>
        /// Answer the Image URL for the ContentProviderChain (or null)
        /// </summary>
        public string ImageURL
        {
            get
            {
                TopicProperty property = GetTopicProperty(DefinitionTopicLocalName,
                    "ImageURL");

                if (property == null)
                {
                    return null;
                }

                if (!property.HasValue)
                {
                    return null;
                }

                return property.LastValue;

            }
        }
        //public DateTime LastRead
        //{
        //    get
        //    {
        //        throw new NotImplementedException();

        //    }
        //}
        /// <summary>
        /// Answer an enumeration of NamespaceManager objects that are for the content providers imported into this one
        /// </summary>
        /// <remarks>This differs from <see cref="ImportedNamespaces" /> in that it returns the manager
        /// objects, not just the names of the imported namespaces.</remarks>
        [XmlIgnore]
        public NamespaceManagerCollection ImportedNamespaceManagers
        {
            get
            {
                NamespaceManagerCollection importedNamespaceManagers = new NamespaceManagerCollection();
                foreach (string ns in ImportedNamespaces)
                {
                    NamespaceManager namespaceManager = Federation.NamespaceManagerForNamespace(ns);
                    if (namespaceManager != null)
                    {
                        importedNamespaceManagers.Add(namespaceManager);
                    }
                }
                return importedNamespaceManagers;
            }
        }
        /// <summary>
        /// Get an ArrayList of strings that are the names of namespaces imported into this content base. 
        /// </summary>
        /// <remarks>This will return a namespace even if that namespace does not exist.</remarks>
        [XmlIgnore]
        public IList<string> ImportedNamespaces
        {
            get
            {
                List<string> importedNamespaces = new List<string>();

                TopicProperty importProperty = GetTopicProperty(DefinitionTopicLocalName, "Import");

                if (importProperty != null)
                {

                    IList<string> imports = importProperty.AsList();

                    // If the definition topic doesn't exist yet, the imports list will come back null. 
                    if (imports != null)
                    {
                        importedNamespaces.AddRange(imports);
                    }
                }
                return importedNamespaces;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                return ContentProviderChain.IsReadOnly;
            }
        }
        /// <summary>
        /// Answer the namespace for this ContentProviderChain
        /// </summary>
        public string Namespace
        {
            get
            {
                return _namespace;
            }
        }
        public NamespaceProviderParameterCollection Parameters
        {
            get
            {
                return _parameters;
            }
        }
        /// <summary>
        /// Gets or sets the <see cref="ITimeProvider"/> that will provide wall clock
        /// values to the <see cref="NamespaceManager"/>.
        /// </summary>
        /// <remarks>
        /// This rawValue will almost always be left at its default, except when running
        /// unit tests, where there is a need to simulate events that happen farther
        /// apart in time than the resolution provided by <see cref="DateTime"/>. 
        /// </remarks>
        public ITimeProvider TimeProvider
        {
            get { return _timeProvider; }
            set { _timeProvider = value; }
        }
        /// <summary>
        /// The title of the content base  (or null if the ContentProviderChain is bogus)
        /// </summary>
        public string Title
        {
            get
            {
                return GetLastPropertyValue(DefinitionTopicLocalName, "Title");
            }
        }

        private ContentProviderBase ContentProviderChain
        {
            get
            {
                return _contentProviderChain;
            }
        }
        private ExternalReferencesMap EmptyExternalReferencesMap
        {
            get
            {
                return _emptyExternalReferencesMap;
            }
        }

        // Methods

        /// <summary>
        /// A list of TopicChanges to a topic (sorted by date, oldest first)
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <returns>Enumeration of TopicChanges </returns>
        public TopicChangeCollection AllChangesForTopic(string topic)
        {
            return AllChangesForTopic(new UnqualifiedTopicName(topic)); 
        }
        public TopicChangeCollection AllChangesForTopic(UnqualifiedTopicName topic)
        {
            return AllChangesForTopicSince(topic, DateTime.MinValue);
        }
        /// <summary>
        /// A list of TopicChanges to a topic since a given date [sorted by date]
        /// </summary>
        /// <param name="topic">A given date</param>
        /// <param name="stamp">A non-null timestamp; changes before this time won't be included in the answer </param>
        /// <param name="rule">A composite cache rule to fill with rules that represented accumulated dependencies (or null)</param>
        /// <returns>Enumeration of TopicChanges</returns>
        public TopicChangeCollection AllChangesForTopicSince(string topic, DateTime since)
        {
            return AllChangesForTopicSince(new UnqualifiedTopicName(topic), since); 
        }
        public TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            return ContentProviderChain.AllChangesForTopicSince(topic, stamp);
        }
        /// <summary>
        /// Given a local topic name, answer all of the qualified topic names that actually exist
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public QualifiedTopicNameCollection AllQualifiedTopicNamesThatExist(string topicName)
        {
            QualifiedTopicNameCollection answer = new QualifiedTopicNameCollection();

            if (TopicExists(topicName, ImportPolicy.DoNotIncludeImports))
            {
                answer.Add(QualifiedTopicNameFor(topicName));
            }

            foreach (NamespaceManager manager in ImportedNamespaceManagers)
            {
                if (manager.TopicExists(topicName, ImportPolicy.DoNotIncludeImports))
                {
                    answer.Add(manager.QualifiedTopicNameFor(topicName));
                }
            }

            return answer;
        }
        public QualifiedTopicNameCollection AllPossibleQualifiedTopicNames(TopicName topicName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Answer a hash: keys are topic; values are an array of topic names for referenced topic (in this namespace)
        /// </summary>
        /// <param name="referencing">Specific topic for which reference information is desired</param>
        /// <param name="existencePolicy">Specifies whether to only return the referenced topics that actually exist</param>
        /// <returns>A list of topics referenced by the specified topic.</returns>
        public QualifiedTopicRevisionCollection AllReferencesByTopic(string referencingTopic, ExistencePolicy existencePolicy)
        {
            return AllReferencesByTopic(new UnqualifiedTopicName(referencingTopic), existencePolicy); 
        }
        public QualifiedTopicRevisionCollection AllReferencesByTopic(UnqualifiedTopicName referencingTopic, ExistencePolicy existencePolicy)
        {
            if (referencingTopic == null)
            {
                throw new ArgumentNullException("referencingTopic");
            }

            TopicRevisionCollection references = ContentProviderChain.GetParsedTopic(new UnqualifiedTopicRevision(referencingTopic.LocalName)).TopicLinks;

            // In-scope namespaces include this namespace and every imported namespace.
            IList<string> inScopeNamespaces = new List<string>();
            inScopeNamespaces.Add(Namespace);
            foreach (string importedNamespace in ImportedNamespaces)
            {
                inScopeNamespaces.Add(importedNamespace);
            }

            // Since the list that comes back might be read-only, we make a new one. 
            QualifiedTopicRevisionCollection resolvedReferences = new QualifiedTopicRevisionCollection();
            foreach (TopicRevision reference in references)
            {
                // If the reference has no namespace, consider it to be relative to 
                // every in-scope namespace
                if (reference.Namespace == null)
                {
                    foreach (string inScopeNamespace in inScopeNamespaces)
                    {
                        resolvedReferences.Add(reference.ResolveRelativeTo(inScopeNamespace));
                    }
                }
                else
                {
                    resolvedReferences.Add(new QualifiedTopicRevision(reference.LocalName, reference.Namespace));
                }
            }

            if (existencePolicy == ExistencePolicy.ExistingOnly)
            {
                // We can't remove items from a list we're iterating over, so 
                // we need to make a new list. 
                QualifiedTopicRevisionCollection filteredReferences = new QualifiedTopicRevisionCollection();

                foreach (QualifiedTopicRevision resolvedReference in resolvedReferences)
                {
                    NamespaceManager manager = Federation.NamespaceManagerForNamespace(resolvedReference.Namespace);

                    if (manager != null)
                    {
                        if (manager.Exists)
                        {
                            if (manager.TopicExists(resolvedReference.LocalName, ImportPolicy.DoNotIncludeImports))
                            {
                                filteredReferences.Add(resolvedReference);
                            }
                        }
                    }
                }

                resolvedReferences = filteredReferences;
            }

            return resolvedReferences;

        }
        /// <summary>
        /// Answer an (unsorted) enumeration of all topic in the ContentProviderChain (possibly including those in imported namespaces, too)
        /// </summary>
        /// <param name="importPolicy">Indicates whether topics in imported namespaces
        /// should be included in the details.</param>
        /// <returns>A collection of <see cref="QualifiedTopicName" /> objects.</returns>
        public QualifiedTopicNameCollection AllTopics(ImportPolicy importPolicy)
        {
            return AllTopics(importPolicy, null);
        }
        /// <summary>
        /// Returns a list of all topics in the namespace, sorted using the specified comparison. 
        /// </summary>
        /// <param name="importPolicy">Indicates whether topics in imported namespaces
        /// should be included in the details.</param>
        /// <param name="sortCriterion">A <see cref="Comparision&gt;QualifiedTopicName&lt;"/> to use to
        /// sort the output, or null not to sort.</param>
        /// <returns>A (potentially sorted) list of <see cref="QualifiedTopicName"/> objects.</returns>
        /// <remarks>Order is not guaranteed in the absence of a sortCriteria.</remarks>
        public QualifiedTopicNameCollection AllTopics(ImportPolicy importPolicy, Comparison<QualifiedTopicName> sortCriterion)
        {
            QualifiedTopicNameCollection answer = new QualifiedTopicNameCollection();

            QualifiedTopicNameCollection unsortedTopics = ContentProviderChain.AllTopics();

            answer.AddRange(unsortedTopics); 

            if (importPolicy == ImportPolicy.IncludeImports)
            {
                foreach (NamespaceManager namespaceManager in ImportedNamespaceManagers)
                {
                    answer.AddRange(namespaceManager.AllTopics(ImportPolicy.DoNotIncludeImports));
                }
            }

            if (sortCriterion != null)
            {
                answer.Sort(sortCriterion);
            }

            return answer;
        }
        /// <summary>
        /// Answer an enumeration of all topic in the <see cref="ContentBase"/>, sorted by last modified (does not include those in imported namespaces)
        /// </summary>
        /// <returns>List of <see cref="QualifiedTopicName"/> objects.</returns>
        public QualifiedTopicNameCollection AllTopicsSortedLastModifiedDescending()
        {
            return AllTopics(ImportPolicy.DoNotIncludeImports, CompareModificationTime);
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Gets the topics with the specified property and rawValue (including those in the imported namespaces).  If desiredValue is omitted, all topics with the property are answered.")]
        public TopicInfoArray AllTopicsWith(ExecutionContext ctx, string property, [ExposedParameter(true)] string desiredValue)
        {
            return this.RetrieveAllTopicsWith(property, desiredValue, ImportPolicy.IncludeImports);
        }
        public int CompareTo(object obj)
        {
            if (!(obj is NamespaceManager))
            {
                throw new ArgumentException("object is not a NamespaceManager");
            }
            return ((new CaseInsensitiveComparer()).Compare(this.FriendlyTitle, ((NamespaceManager)(obj)).FriendlyTitle));
        }
        /// <summary>
        /// Delete a namespace (kills everything inside recursively).  Note that this does *not* include unregistering
        /// the namespace within the federation.
        /// </summary>
        public void DeleteAllTopicsAndHistory()
        {
            ContentProviderChain.DeleteAllTopicsAndHistory();
        }
        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public void DeleteTopic(string topic)
        {
            DeleteTopic(new UnqualifiedTopicName(topic)); 
        }
        public void DeleteTopic(UnqualifiedTopicName topic)
        {
            ContentProviderChain.DeleteTopic(topic);
        }
        /// <summary>
        /// Answer the contact info for the ContentProviderChain (or null)
        /// </summary>
        [ExposedMethod("Contact", ExposedMethodFlags.NeedContext, "Answer a string identifying the contact for this namespace")]
        public string ExposedContact(ExecutionContext ctx)
        {
            return Contact;
        }
        /// <summary>
        /// Answer the description for the ContentProviderChain(or null)
        /// </summary>
        [ExposedMethod("Description", ExposedMethodFlags.NeedContext, "Answer a description of this namespace")]
        public string ExposedDescription(ExecutionContext ctx)
        {
            return Description;
        }

        /// <summary>
        /// Answer the Image URL for the ContentProviderChain (or null)
        /// </summary>
        [ExposedMethod("ImageURL", ExposedMethodFlags.NeedContext, "Answer the URL for an image that brand-marks this namespace")]
        public string ExposedImageURL(ExecutionContext ctx)
        {
            return ImageURL;
        }

        [ExposedMethod("ImportedNamespaces", ExposedMethodFlags.NeedContext, "A collection of imported namespaces")]
        public ArrayList ExposedImports(ExecutionContext ctx)
        {
            ArrayList answer = new ArrayList();
            foreach (NamespaceManager namespaceManager in ImportedNamespaceManagers)
            {
                answer.Add(namespaceManager);
            }
            return answer;
        }

        /// <summary>
        /// Answer the namespace for this ContentProviderChain
        /// </summary>
        [ExposedMethod("Name", ExposedMethodFlags.NeedContext, "Answer the name of this namespace")]
        public string ExposedNamespace(ExecutionContext ctx)
        {
            return Namespace;
        }

        /// <summary>
        /// Answer the human-friendly title for the ContentProviderChain (Title if available, else Namespace)
        /// </summary>
        [ExposedMethod("Title", ExposedMethodFlags.NeedContext, "Answer a title for this namespace")]
        public string ExposedTitle(ExecutionContext ctx)
        {
            return FriendlyTitle;
        }

        /// <summary>
        /// Returns a particular content provider from the content provider chain. This
        /// method is intended to be used primarily by unit test codes. Use of this 
        /// method in production code is a likely sign that you are doing something 
        /// wrong, and may well result in incorrect behavior. 
        /// </summary>
        /// <param name="type">The type of the provider to retrieve.</param>
        /// <returns>A content provider from the content provider chain.</returns>
        /// <remarks>If more than one provider in the content chain is of the 
        /// requested type, the first provider will be returned. This
        /// method is intended to be used primarily by unit test codes. Use of this 
        /// method in production code is a likely sign that you are doing something 
        /// wrong, and may well result in incorrect behavior. </remarks>
        public ContentProviderBase GetProvider(Type type)
        {
            ContentProviderBase provider = ContentProviderChain;

            while (provider != null)
            {
                if (provider.GetType() == type)
                {
                    return provider; 
                }
                provider = provider.Next;
            }

            return null; 
        }
        /// <summary>
        /// Returns a list of all references from all topics in this namespace. 
        /// </summary>
        /// <param name="includeImports">Indicates whether topics should be filtered only
        /// to those that actually exist.</param>
        /// <returns>A map of topic names to the list of topics they reference.</returns>
        public ReferenceMap GetReferenceMap(ExistencePolicy existencePolicy)
        {
            ReferenceMap map = new ReferenceMap();

            foreach (TopicName topicName in AllTopics(ImportPolicy.DoNotIncludeImports))
            {
                map[topicName.LocalName] = AllReferencesByTopic(topicName.LocalName, existencePolicy);
            }

            return map;
        }
        /// <summary>
        /// Answer when a topic was created
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <param name="version">The version of the topic to look at. <c>null</c> for the latest version.</param>
        /// <returns></returns>
        public DateTime GetTopicCreationTime(string topic, string version)
        {
            return GetTopicCreationTime(new UnqualifiedTopicRevision(topic, version)); 
        }
        public DateTime GetTopicCreationTime(UnqualifiedTopicRevision revision)
        {
            TopicChangeCollection changes = AllChangesForTopic(revision.DottedName);

            if (changes == null)
            {
                throw TopicNotFoundException.ForTopic(revision, Namespace); 
            }

            if (revision.Version == null)
            {
                return changes.Latest.Created;
            }
            else
            {
                foreach (TopicChange change in changes)
                {
                    if (change.Version == revision.Version)
                    {
                        return change.Created; 
                    }
                }
            }

            throw TopicNotFoundException.ForTopic(revision, Namespace); 
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Get information about the given topic")]
        public TopicVersionInfo GetTopicInfo(string topicName)
        {
            QualifiedTopicRevision abs = new QualifiedTopicRevision(topicName, Namespace);
            return new TopicVersionInfo(Federation, abs);
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
        public string GetTopicLastAuthor(string topic)
        {
            TopicChangeCollection changes = AllChangesForTopic(topic);

            if (changes == null)
            {
                return Federation.AnonymousUserName; 
            }

            if (changes.Latest == null)
            {
                return Federation.AnonymousUserName; 
            }

            return changes.Latest.Author; 
        }
        /// <summary>
        /// Answer when the topic was last changed
        /// </summary>
        /// <param name="topic">A topic name</param>
        /// <returns></returns>
        public DateTime GetTopicLastModificationTime(string topic)
        {
            if (!TopicExists(topic, ImportPolicy.DoNotIncludeImports))
            {
                string message = string.Format("Could not locate a topic named {0}.{1}", Namespace, topic);
                throw new ArgumentException(message);
            }

            TopicChangeCollection changes = AllChangesForTopic(topic);

            return changes.Latest.Modified;

        }
        public TopicPropertyCollection GetTopicProperties(string topic)
        {
            return GetTopicProperties(new UnqualifiedTopicName(topic)); 
        }
        public TopicPropertyCollection GetTopicProperties(UnqualifiedTopicName topic)
        {
            ParsedTopic parsedTopic = ContentProviderChain.GetParsedTopic(new UnqualifiedTopicRevision(topic));
            
            if (parsedTopic == null)
            {
                return null; 
            }

            return parsedTopic.Properties;
        }
        /// <summary>
        /// Returns a description of a particular property on the specified topic. 
        /// </summary>
        /// <param name="topic">A <see cref="LocalTopicName"/> identifying the desired topic.</param>
        /// <param name="propertyName">The name of the desired topic.</param>
        /// <returns>A <see cref="TopicProperty" /> object if the topic exists. If the topic does not exist, 
        /// null is returned.</returns>
        /// <remarks>Note that a <see cref="TopicProperty"/> object will always be returned if the topic
        /// exists. However, if the property is not present, the object will have no values.</remarks>
        public TopicProperty GetTopicProperty(string topic, string propertyName)
        {
            return GetTopicProperty(new UnqualifiedTopicName(topic), propertyName); 
        }
        public TopicProperty GetTopicProperty(UnqualifiedTopicName topic, string propertyName)
        {
            ParsedTopic parsedTopic = ContentProviderChain.GetParsedTopic(new UnqualifiedTopicRevision(topic));

            if (parsedTopic == null)
            {
                return null;
            }

            TopicPropertyCollection properties = parsedTopic.Properties;

            if (properties == null)
            {
                return null;
            }

            if (properties.Contains(propertyName))
            {
                return properties[propertyName];
            }
            else
            {
                return new TopicProperty(propertyName);
            }
        }
        /// <summary>
        /// Answer whether a topic exists and is writable
        /// </summary>
        /// <param name="topic">The topic (must directly be in this content base)</param>
        /// <returns>true if the topic exists AND is writable by the current user; else false</returns>
        public bool IsExistingTopicWritable(string topic)
        {
            return IsExistingTopicWritable(new UnqualifiedTopicName(topic)); 
        }
        public bool IsExistingTopicWritable(UnqualifiedTopicName topic)
        {
            return ContentProviderChain.IsExistingTopicWritable(topic);
        }
        /// <summary>
        /// Returns the most recent version for the given topic
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <returns>The most recent version string for the topic</returns>
        public string LatestVersionForTopic(string topic)
        {
            return LatestVersionForTopic(new UnqualifiedTopicName(topic)); 
        }
        public string LatestVersionForTopic(UnqualifiedTopicName topic)
        {
            IList<TopicChange> changes = AllChangesForTopic(topic);

            // A null return means that the topic doesn't exist
            if (changes == null)
            {
                return null;
            }

            return changes[changes.Count - 1].Version;

        }
        /// <summary>
        /// Answer the DateTime of when any of the topics in the ContentProviderChain where last modified.
        /// </summary>
        /// <param name="includeImports">true if you also want to include all imported namespaces</param>
        /// <returns></returns>
        public DateTime LastModified(ImportPolicy importPolicy)
        {
            DateTime lastModified = DateTime.MinValue;
            foreach (TopicName topic in AllTopics(importPolicy))
            {
                DateTime modified = Federation.GetTopicLastModificationTime(topic);
                if (modified > lastModified)
                {
                    lastModified = modified;
                }
            }

            return lastModified;
        }
        /// <summary>
        /// Answer an <see cref="QualifiedTopicName"/> for the given topic name local to this ContentProviderChain.
        /// </summary>
        /// <param name="localTopicName">A topic name</param>
        /// <returns>A <see cref="QualifiedTopicName"/> object.</returns>
        public QualifiedTopicName QualifiedTopicNameFor(string localTopicName)
        {
            return QualifiedTopicNameFor(new UnqualifiedTopicName(localTopicName)); 
        }
        public QualifiedTopicName QualifiedTopicNameFor(UnqualifiedTopicName localTopicName)
        {
            return new QualifiedTopicName(localTopicName.LocalName, Namespace);
        }
        /// <summary>
        /// Answer the contents of a given topic.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <returns>The contents of the topic or null if it can't be read (e.g., doesn't exist).</returns>
        public string Read(string topic)
        {
            return Read(new UnqualifiedTopicName(topic)); 
        }
        public string Read(UnqualifiedTopicName topic)
        {
            return Read(new UnqualifiedTopicRevision(topic.LocalName, null)); 
        }
        /// <summary>
        /// Answer the contents of a given topic.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <param name="version">The version to read, or null for the latest version.</param>
        /// <returns>The contents of the topic or null if it can't be read (e.g., doesn't exist)</returns>
        public string Read(UnqualifiedTopicRevision revision)
        {
            using (TextReader st = TextReaderForTopic(revision))
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
        public RenameTopicDetails RenameTopic(UnqualifiedTopicName oldName, UnqualifiedTopicName newName, ReferenceFixupPolicy fixupPolicy, string author)
        {
            RenameTopicDetails details = new RenameTopicDetails();

            if (!TopicExists(oldName, ImportPolicy.DoNotIncludeImports))
            {
                details.Result = RenameTopicResult.SourceTopicDoesNotExist;
                return details;
            }

            if (TopicExists(newName, ImportPolicy.DoNotIncludeImports))
            {
                details.Result = RenameTopicResult.DestinationTopicExists;
                return details;
            }

            string oldTopicOldContents = Read(oldName.LocalName);
            string oldTopicNewContents = string.Format("Redirect: {0}\n\nThis topic was renamed to {0}.",
                newName.LocalName);

            WriteTopicAndNewVersion(oldName.LocalName, oldTopicNewContents, author);
            WriteTopicAndNewVersion(newName.LocalName, oldTopicOldContents, author);

            if (fixupPolicy == ReferenceFixupPolicy.FixReferences)
            {
                foreach (TopicName topicName in AllTopics(ImportPolicy.DoNotIncludeImports))
                {
                    bool renamed = RenameTopicReferences(topicName.LocalName,
                        oldName.LocalName, newName.LocalName, author);

                    if (renamed)
                    {
                        details.UpdatedReferenceTopics.Add(topicName);
                    }
                }
            }

            details.Result = RenameTopicResult.Success;

            return details;
        }
        /// <summary>
        /// Change the rawValue of a propertyName in a a topic.  If the topic doesn't exist, it will be created.
        /// </summary>
        /// <param name="topic">The topic whose propertyName is to be changed.</param>
        /// <param name="propertyName">The name of the propertyName to change.</param>
        /// <param name="value">The new rawValue for the propertyName.</param>
        public void SetTopicPropertyValue(string topic, string property, string value, bool writeNewVersion, string author)
        {
            // TODO: This will need to be overhauled once we have access to the parsed representation
            // of the topic. Right now it assumes a property syntax that might change. 
            if (!TopicExists(topic, ImportPolicy.DoNotIncludeImports))
            {
                WriteTopic(topic, "");
            }

            string original = Read(topic);

            // Multiline values need to end a complete line
            string repWithTopicPropertyEnd = value;
            if (!repWithTopicPropertyEnd.EndsWith("\n"))
            {
                repWithTopicPropertyEnd = repWithTopicPropertyEnd + "\n";
            }
            bool newValueIsMultiline = value.IndexOf("\n") > 0;

            string simpleField = "(?<name>(" + property + ")):(?<val>[^\\[].*)";
            string multiTopicPropertyField = "(?<name>(" + property + ")):\\[(?<val>[^\\[]*\\])";

            string update = original;
            if (new Regex(simpleField).IsMatch(original))
            {
                if (newValueIsMultiline)
                {
                    update = Regex.Replace(original, simpleField, "${name}:[ " + repWithTopicPropertyEnd + "]");
                }
                else
                {
                    update = Regex.Replace(original, simpleField, "${name}: " + value);
                }
            }
            else if (new Regex(multiTopicPropertyField).IsMatch(original))
            {
                if (newValueIsMultiline)
                {
                    update = Regex.Replace(original, multiTopicPropertyField, "${name}:[ " + repWithTopicPropertyEnd + "]");
                }
                else
                {
                    update = Regex.Replace(original, multiTopicPropertyField, "${name}: " + value);
                }
            }
            else
            {
                if (!update.EndsWith("\n"))
                {
                    update = update + "\n";
                }

                if (value.IndexOf("\n") == -1)
                {
                    update += property + ": " + repWithTopicPropertyEnd;
                }
                else
                {
                    update += property + ":[ " + repWithTopicPropertyEnd + "]\n";
                }
            }
            if (writeNewVersion)
            {
                WriteTopicAndNewVersion(topic, update, author);
            }
            else
            {
                WriteTopic(topic, update);
            }
        }
        /// <summary>
        /// Answer a TextReader for the latest version of a given topic.
        /// </summary>
        /// <param name="topic"></param>
        /// <exception cref="TopicNotFoundException">Thrown when the topic doesn't exist</exception>
        /// <returns>TextReader</returns>
        public TextReader TextReaderForTopic(string topic)
        {
            return TextReaderForTopic(new UnqualifiedTopicName(topic)); 
        }
        public TextReader TextReaderForTopic(UnqualifiedTopicName topic)
        {
            return TextReaderForTopic(new UnqualifiedTopicRevision(topic.LocalName, null));
        }
        public TextReader TextReaderForTopic(UnqualifiedTopicRevision revision)
        {
            return ContentProviderChain.TextReaderForTopic(revision); 
        }
        public bool TopicExists(string topic, ImportPolicy importPolicy)
        {
            return TopicExists(new UnqualifiedTopicName(topic), importPolicy); 
        }
        public bool TopicExists(UnqualifiedTopicName topic, ImportPolicy importPolicy)
        {
            bool existsLocally = ContentProviderChain.TopicExists(topic);

            if (importPolicy == ImportPolicy.DoNotIncludeImports)
            {
                return existsLocally;
            }
            else
            {
                if (existsLocally)
                {
                    return true;
                }
                else
                {
                    foreach (NamespaceManager importedManager in ImportedNamespaceManagers)
                    {
                        if (importedManager.TopicExists(topic, ImportPolicy.DoNotIncludeImports))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
        /// <summary>
        /// Answer true if the given topic exists in this namespace or in an imported namespace (if it's relative), or in the given namespace (if it's qualified)
        /// </summary>
        /// <param name="topic">The topic to check for</param>
        /// <returns>true if the topic exists</returns>
        /// <remarks>importPolicy is ignored if the topic name is qualified.</remarks>
        public bool TopicExists(TopicRevision topic, ImportPolicy importPolicy)
        {
            if (topic.IsQualified)
            {
                return Federation.TopicExists(new QualifiedTopicRevision(topic.LocalName, topic.Namespace));
            }
            else
            {
                return TopicExists(topic.LocalName, importPolicy);
            }
        }
        /// <summary>
        /// Answer a collection of namespaces in which the topic actually exists
        /// </summary>
        /// <param name="topic">The topic you want to search for in this namespace.</param>
        /// <returns>A list of namespaces (as strings); empty if none</returns>
        public NamespaceCollection TopicNamespaces(string topic)
        {
            NamespaceCollection answer = new NamespaceCollection();

            if (TopicExists(topic, ImportPolicy.DoNotIncludeImports))
            {
                answer.Add(Namespace); 
            }

            foreach (NamespaceManager manager in ImportedNamespaceManagers)
            {
                if (manager.TopicExists(topic, ImportPolicy.DoNotIncludeImports))
                {
                    answer.Add(manager.Namespace);
                }
            }
            return answer;
        }
        [ExposedMethod(ExposedMethodFlags.NeedContext, "Answer a list of all topic in this namespace (excluding imported namespaces)")]
        public ArrayList Topics(ExecutionContext ctx)
        {
            ArrayList answer = new ArrayList();
            foreach (TopicName name in AllTopics(ImportPolicy.DoNotIncludeImports))
            {
                answer.Add(new TopicVersionInfo(Federation, new QualifiedTopicRevision(name)));
            }

            // Add cache rules for all the topics in the namespaces and for the definition (in case the imports change)
            return answer;
        }
        [ExposedMethod(ExposedMethodFlags.Default, "Gets the topics with the specified property and rawValue (excluding those in the imported namespaces).  If desiredValue is omitted, all topics with the property are answered.")]
        public TopicInfoArray TopicsWith(ExecutionContext ctx, string property, [ExposedParameter(true)] string desiredValue)
        {
            return this.RetrieveAllTopicsWith(property, desiredValue, ImportPolicy.DoNotIncludeImports);
        }
        public override string ToString()
        {
            return Namespace;
        }
        /// <summary>
        /// Answer the full name of the topic (qualified with namespace) if it exists.  If it doesn't exist at all, answer null.
        /// If it does, but it's ambiguous, then throw TopicIsAmbiguousException
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>Full name or null if it doesn't exist (by throw TopicIsAmbiguousException if it's ambiguous)</returns>
        public TopicName UnambiguousTopicNameFor(string topic)
        {
            NamespaceCollection list = TopicNamespaces(topic);
            if (list.Count == 0)
            {
                return null;
            }
            if (list.Count > 1)
            {
                throw TopicIsAmbiguousException.ForTopic(new TopicRevision(topic));
            }
            return new TopicName(topic, (string)list[0]);
        }
        /// <summary>
        /// Find the version of a topic immediately previous to another version
        /// </summary>
        /// <param name="topic">The name (with version) of the topic for which you want the change previous to</param>
        /// <returns>TopicChange or null if none</returns>
        public QualifiedTopicRevision VersionPreviousTo(string topic, string version)
        {
            QualifiedTopicRevision answer = new QualifiedTopicRevision(topic, Namespace);
            answer.Version = version;

            TopicChangeCollection changes = AllChangesForTopic(topic);

            if (changes == null)
            {
                return null; 
            }

            int match = -1;

            if (version == null)
            {
                match = changes.Count - 1;
            }
            else
            {
                for (int i = 0; i < changes.Count; ++i)
                {
                    if (changes[i].Version == version)
                    {
                        match = i;
                        break;
                    }
                }
            }

            if (match == 0)
            {
                return null; 
            }

            if (match == -1)
            {
                return null;
            }

            answer.Version = changes[match - 1].Version;

            return answer; 

        }
        /// <summary>
        /// Write new contents over the latest version of the topic (doesn't write a new version).
        /// </summary>
        /// <param name="topic">Topic to write</param>
        /// <param name="content">New content</param>
        public void WriteTopic(string topic, string contents)
        {
            WriteTopic(new UnqualifiedTopicRevision(topic), contents); 
        }
        /// <summary>
        /// Write new contents over the specified version of the topic (doesn't write a new version).
        /// </summary>
        /// <param name="topic">Topic to overwrite.</param>
        /// <param name="version">Version to overwrite.</param>
        /// <param name="content">New content.</param>
        public void WriteTopic(UnqualifiedTopicRevision revision, string content)
        {
            if (revision.Version != null)
            {
                if (!TopicVersionExists(revision))
                {
                    throw FlexWikiException.VersionDoesNotExist(new QualifiedTopicName(revision.LocalName, this.Namespace),revision.Version); 
                }
            }
            ContentProviderChain.WriteTopic(revision, content);
        }
        /// <summary>
        /// Write a topic (and create a historical version)
        /// </summary>
        /// <param name="topic">The topic to write</param>
        /// <param name="content">The content</param>
        public void WriteTopicAndNewVersion(string topic, string content, string author)
        {
            WriteTopicAndNewVersion(new UnqualifiedTopicName(topic), content, author); 
        }
        public void WriteTopicAndNewVersion(UnqualifiedTopicName topic, string content, string author)
        {
            ContentProviderChain.WriteTopicAndNewVersion(topic, content, author);
        }

        // Private methods

        private int CompareModificationTime(TopicName topicA, TopicName topicB)
        {
            NamespaceManager managerA = Federation.NamespaceManagerForNamespace(topicA.Namespace);
            NamespaceManager managerB = Federation.NamespaceManagerForNamespace(topicB.Namespace);

            DateTime modifiedA = managerA.GetTopicLastModificationTime(topicA.LocalName);
            DateTime modifiedB = managerB.GetTopicLastModificationTime(topicB.LocalName);

            return modifiedB.CompareTo(modifiedA);
        }
        /// <summary>
        /// Gets the last rawValue of the specified propertyName from the specified topic.
        /// </summary>
        /// <param name="topic">The topic to look in.</param>
        /// <param name="propertyName">The propertyName to retrieve.</param>
        /// <returns>The last rawValue of the propertyName, or null if the propertyName is absent</returns>
        private string GetLastPropertyValue(string topic, string propertyName)
        {
            TopicProperty topicProperty = GetTopicProperty(topic, propertyName);

            // If for some reason the topic is absent, return null
            if (topicProperty == null)
            {
                return null;
            }

            IList<string> values = topicProperty.AsList();

            if (values.Count == 0)
            {
                return null;
            }
            else
            {
                return values[values.Count - 1];
            }
        }
        /// <summary>
        /// Rename references (in a given topic) from one topic to a new name 
        /// </summary>
        /// <param name="topicToLookIn"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        private bool RenameTopicReferences(string topicToLookIn, string oldName, string newName, string author)
        {
            //CA I'm not thrilled about using this older implementation - it seems 
            //CA like this functionality should come out of the new parsing provider
            //CA somehow, but there's already enough in the rearchitecture without goofing
            //CA with that as well...
            string current = Read(topicToLookIn);
            MatchCollection wikiNames = Formatter.extractWikiNames.Matches(current);
            ArrayList processed = new ArrayList();
            bool any = false;
            TopicRevision newTopicRevision = new QualifiedTopicRevision(newName, this.Namespace);
            TopicName newTopicName = new TopicName(newTopicRevision.LocalName, newTopicRevision.Namespace); 
            foreach (Match m in wikiNames)
            {
                string each = m.Groups["topic"].ToString();
                if (processed.Contains(each))
                {
                    continue;   // skip dup	
                }
                processed.Add(each);

                TopicRevision relName = new TopicRevision(TopicParser.StripTopicNameEscapes(each));

                // See if this is the old name.  The only way it can be is if it's unqualified or if it's qualified with the current namespace.

                bool hit = (relName.LocalName == oldName) && (relName.Namespace == null || relName.Namespace == this.Namespace);
                if (!hit)
                {
                    continue;
                }

                // Now see if we got any hits or not
                string rep = Formatter.s_beforeWikiName + "(" + Formatter.RegexEscapeTopic(each) + ")" + Formatter.s_afterWikiName;
                // if the reference was fully qualified, retain that form in the new reference
                string replacementName = each.IndexOf(".") > -1 ? newTopicName.DottedName : newTopicName.LocalName;
                current = Regex.Replace(current, rep, "${before}" + replacementName + "${after}");
                any = true;
            }

            if (any)
            {
                WriteTopicAndNewVersion(topicToLookIn, current, author);
            }

            return any;
        }///// <summary>
        ///// Write a new version of the topic (doesn't write a new version).  Generate all needed federation update changes via the supplied generator.
        ///// </summary>
        ///// <param name="topic">Topic to write</param>
        ///// <param name="content">New content</param>
        ///// <param name="sink">Object to recieve change info about the topic</param>
        //private void WriteTopic(LocalTopicName topic, string content, FederationUpdateGenerator gen)
        //{
        //}
        private TopicInfoArray RetrieveAllTopicsWith(string propertyName, string desiredValue, ImportPolicy importPolicy)
        {
            TopicInfoArray topicInfos = new TopicInfoArray();
            foreach (QualifiedTopicName namespaceQualifiedTopicName in AllTopics(importPolicy))
            {
                TopicProperty property = Federation.GetTopicProperty(namespaceQualifiedTopicName, 
                    propertyName);

                if (property != null)
                {
                    if (property.HasValue)
                    {
                        foreach (TopicPropertyValue propertyValue in property.Values)
                        {
                            string value = propertyValue.RawValue; 
                            if (desiredValue == null || (0 == String.Compare(desiredValue, value, true)))
                            {
                                topicInfos.Add(new TopicVersionInfo(this.Federation,
                                    new QualifiedTopicRevision(namespaceQualifiedTopicName)));
                                break;
                            }
                        }
                    }
                }
            }

            return topicInfos;

        }

        private bool TopicVersionExists(UnqualifiedTopicRevision revision)
        {
            TopicChangeCollection changes = AllChangesForTopic(revision.AsUnqualifiedTopicName());

            if (changes == null)
            {
                return false; 
            }

            foreach (TopicChange change in changes)
            {
                if (change.Version == revision.Version)
                {
                    return true; 
                }
            }

            return false; 
        }

    }
}
