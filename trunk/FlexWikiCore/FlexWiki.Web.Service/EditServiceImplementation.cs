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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml;

using FlexWiki;

namespace FlexWiki.Web.Services
{
    /// <summary>
    /// Summary description for EditService.
    /// </summary>
    [WebService(Namespace = "http://www.flexwiki.com/webservices/")]
    public class EditServiceImplementation : System.Web.Services.WebService
    {
        private LinkMaker _linkMaker;

        public EditServiceImplementation()
        {
            EstablishFederation();
        }


        protected Federation Federation
        {
            get
            {
                return (Federation)(Application["---FEDERATION---"]);
            }
            set
            {
                Application["---FEDERATION---"] = value;
            }
        }

        protected LinkMaker TheLinkMaker
        {
            get
            {
                return _linkMaker;
            }
        }


        /// <summary>
        /// CanEdit checks to see if the user is Authenticated using supplied credentials in the Web Service proxy.
        /// </summary>
        /// <returns>An attribution in the form of domain\username or null if the user isn't authenticated.</returns>
        // TODO: make this into something real, perhaps requiring the client to send a new Author object.
        [WebMethod]
        public string CanEdit()
        {
            string visitorIdentityString = null;

            return GetVisitorIdentity(visitorIdentityString);
        }
        /// <summary>
        /// Returns all the namespaces in the Federation.
        /// </summary>
        /// <returns>A ContentBaseCollection of all the ContentStores for the Federation.</returns>
        [WebMethod]
        public WireTypes.ContentBaseCollection GetAllNamespaces()
        {
            WireTypes.ContentBaseCollection contentBases = new WireTypes.ContentBaseCollection();

            foreach (NamespaceManager namespaceManager in Federation.NamespaceManagers)
            {
                WireTypes.ContentBase wireFormat = new WireTypes.ContentBase(namespaceManager);
                contentBases.Add(wireFormat);
            }

            return contentBases;
        }

        /// <summary>
        /// Returns the AbsoluteTopicNames for a given Namespace.
        /// </summary>
        /// <param name="cb">The ContentProviderChain.</param>
        /// <returns>A AbsoluteTopicNameCollection of the AbsoluteTopicNames for the ContentProviderChain</returns>
        [WebMethod]
        public WireTypes.AbsoluteTopicNameCollection GetAllTopics(WireTypes.ContentBase cb)
        {
            WireTypes.AbsoluteTopicNameCollection topicNames = new WireTypes.AbsoluteTopicNameCollection();

            foreach (TopicName ab in Federation.NamespaceManagerForNamespace(cb.Namespace).AllTopics(ImportPolicy.DoNotIncludeImports))
            {
                // We need to also return the current version
                NamespaceManager contentStoreManager2 = Federation.NamespaceManagerForTopic(ab);
                WireTypes.AbsoluteTopicName atn = new WireTypes.AbsoluteTopicName();
                atn.Name = ab.LocalName;
                atn.Namespace = ab.Namespace;
                string version = contentStoreManager2.LatestVersionForTopic(ab.LocalName);
                if (version == null)
                {
                    // There's only one version, so just use the empty string
                    version = "";
                }
                atn.Version = version;
                topicNames.Add(atn);
            }

            return topicNames;
        }

        /// <summary>
        /// Returns the default namespace in the Federation. 
        /// </summary>
        /// <returns>A ContentProviderChain of the default namespace.</returns>
        [WebMethod]
        public WireTypes.ContentBase GetDefaultNamespace()
        {
            if (Federation.DefaultNamespaceManager == null)
            {
                throw new Exception("No default namespace defined by the federation.");
            }

            return new WireTypes.ContentBase(Federation.DefaultNamespaceManager);
        }
        /// <summary>
        /// Returns the formatted HTML for a given Topic.
        /// </summary>
        /// <param name="topicName">An AbsoluteTopicName.</param>
        /// <returns>Formatted HTML string.</returns>
        [WebMethod]
        public string GetHtmlForTopic(WireTypes.AbsoluteTopicName topicName)
        {
            QualifiedTopicRevision atn = new QualifiedTopicRevision(topicName.Name, topicName.Namespace);
            return InternalGetHtmlForTopic(atn, null);
        }

        /// <summary>
        /// Returns the formatted HTML for a previous version of a given Topic. 
        /// </summary>
        /// <param name="topicName">An AbsoluteTopicName.</param>
        /// <param name="version">The version string to return. The list of known version strings for a given Topic can be obtained by calling <see cref="GetVersionsForTopic"/>.</param>
        /// <returns>Formatted HTML string.</returns>
        [WebMethod]
        public string GetHtmlForTopicVersion(WireTypes.AbsoluteTopicName topicName, string version)
        {
            QualifiedTopicRevision atn = new QualifiedTopicRevision(topicName.Name, topicName.Namespace);
            return InternalGetHtmlForTopic(atn, version);
        }

        /// <summary>
        /// Returns the formatted HTML version of the given text for a Topic.
        /// </summary>
        /// <param name="topicName">An AbsoluteTopicName.</param>
        /// <param name="textToFormat">The text to format.</param>
        /// <returns>Formatted HTML string.</returns>
        [WebMethod]
        public string GetPreviewForTopic(WireTypes.AbsoluteTopicName topicName, string textToFormat)
        {
            _linkMaker = new LinkMaker(RootUrl(Context.Request));

            // OmarS: why do I have to do this?
            NamespaceManager relativeToBase = Federation.NamespaceManagerForNamespace(topicName.Namespace);

            return FlexWiki.Formatting.Formatter.FormattedString(null, textToFormat, OutputFormat.HTML, relativeToBase, _linkMaker);
        }

        /// <summary>
        /// Returns the raw Text for a version of a given Topic. 
        /// </summary>
        /// <param name="topicName">An AbsoluteTopicName.</param>
        /// <returns>Raw Text.</returns>
        [WebMethod]
        public string GetTextForTopic(WireTypes.AbsoluteTopicName topicName)
        {
            string content = null;
            // OmarS: Do I need to check for Topic existence?
            QualifiedTopicRevision atn = new QualifiedTopicRevision(topicName.Name, topicName.Namespace);
            atn.Version = topicName.Version; 

            if (Federation.TopicExists(atn))
            {
                content = Federation.Read(atn);
            }
            if (content == null)
            {
                content = "[enter your text here]";
            }

            return content;
        }

        /// <summary>
        /// Returns a collection of versions for a given Topic.
        /// </summary>
        /// <param name="topicName">An AbsoluteTopicName.</param>
        /// <returns>StringCollection of version strings.</returns>
        [WebMethod]
        public StringCollection GetVersionsForTopic(WireTypes.AbsoluteTopicName topicName)
        {
            StringCollection topicVersions = new StringCollection();

            QualifiedTopicRevision atn = new QualifiedTopicRevision(topicName.Name, topicName.Namespace);
            IEnumerable changeList = Federation.NamespaceManagerForTopic(atn).AllChangesForTopic(atn.LocalName);

            foreach (TopicChange change in changeList)
            {
                topicVersions.Add(change.Version);
            }

            return topicVersions;
        }

        [WebMethod]
        public WireTypes.WikiVersion GetWikiVersion()
        {
            WireTypes.WikiVersion wikiVersion = new WireTypes.WikiVersion();
            Version assemblyVersion = typeof(Federation).Assembly.GetName().Version;

            wikiVersion.Major = assemblyVersion.Major;
            wikiVersion.Minor = assemblyVersion.Minor;
            wikiVersion.Build = assemblyVersion.Build;
            wikiVersion.Revision = assemblyVersion.Revision;

            return wikiVersion;
        }
        /// <summary>
        /// Restores a given Topic to a previous version.
        /// </summary>
        /// <param name="topicName">An AbsoluteTopicName.</param>
        /// <param name="postedTopicText">The new unformatted text.</param>
        /// <param name="visitorIdentityString">The visitor identity string.</param>
        [WebMethod]
        public void RestoreTopic(WireTypes.AbsoluteTopicName topicName, string visitorIdentityString, string version)
        {
            if (version != null && version == topicName.Version)
            {
                throw new Exception("Version not found");
            }
            else
            {
                QualifiedTopicRevision atn = new QualifiedTopicRevision(topicName.Name, topicName.Namespace);
                IEnumerable changeList = Federation.NamespaceManagerForTopic(atn).AllChangesForTopic(atn.LocalName);

                foreach (TopicChange change in changeList)
                {
                    if (change.Version == version)
                    {
                        WriteNewTopic(change.TopicRevision, Federation.Read(change.TopicRevision), visitorIdentityString, version);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the text for a given Topic.
        /// </summary>
        /// <param name="topicName">An AbsoluteTopicName.</param>
        /// <param name="postedTopicText">The new unformatted text.</param>
        /// <param name="visitorIdentityString">The visitor identity string.</param>
        [WebMethod]
        public void SetTextForTopic(WireTypes.AbsoluteTopicName topicName, string postedTopicText, string visitorIdentityString)
        {
            QualifiedTopicRevision atn = new QualifiedTopicRevision(topicName.Name, topicName.Namespace);
            WriteNewTopic(atn, postedTopicText, GetVisitorIdentity(visitorIdentityString), null);
        }


        private void EstablishFederation()
        {
            if (Federation != null)
            {
                return;
            }

            throw new NotImplementedException("Need to make sure we set up the federation appropriately."); 
        }

        private XmlElement GetWsdl()
        {
            ServiceDescriptionReflector reflector = new ServiceDescriptionReflector();
            reflector.Reflect(typeof(EditServiceImplementation), HttpContext.Current.Request.RawUrl);

            if (reflector.ServiceDescriptions.Count > 1)
            {
                throw new Exception("I'll deal with multiple service descriptions later");
            }

            MemoryStream ms = new MemoryStream();
            XmlTextWriter wtr = new XmlTextWriter(ms, System.Text.Encoding.UTF8);
            wtr.Formatting = System.Xml.Formatting.Indented;
            reflector.ServiceDescriptions[0].Write(wtr);
            wtr.Close();
            ms.Position = 0;

            XmlDocument doc = new XmlDocument();
            doc.Load(ms);

            return doc.DocumentElement;
        }
        private string GetVisitorIdentity(string visitorIdentityString)
        {
            // if we are using Windows Authenticaiton, override the attribution with the Windows domain/username
            if (User.Identity.IsAuthenticated)
            {
                return User.Identity.Name;
            }
            else if (visitorIdentityString == null || visitorIdentityString.Length == 0)
            {
                return Context.Request.UserHostAddress;
            }
            else
            {
                return visitorIdentityString;
            }
        }
        private string InternalGetHtmlForTopic(QualifiedTopicRevision topicName, string version)
        {
            _linkMaker = new LinkMaker(RootUrl(Context.Request));

            if (version != null && version == topicName.Version)
            {
                return FlexWiki.Formatting.Formatter.FormattedTopic(topicName, OutputFormat.HTML, null, Federation, _linkMaker);
            }
            else
            {
                IEnumerable changeList;
                changeList = Federation.NamespaceManagerForTopic(topicName).AllChangesForTopic(topicName.LocalName);

                foreach (TopicChange change in changeList)
                {
                    if (change.Version == version)
                    {
                        return FlexWiki.Formatting.Formatter.FormattedTopic(change.TopicRevision, OutputFormat.HTML, null, Federation, _linkMaker);
                    }
                }

                return FlexWiki.Formatting.Formatter.FormattedTopic(topicName, OutputFormat.HTML, null, Federation, _linkMaker);
            }
        }

        private string RootUrl(HttpRequest req)
        {
            string full = req.Url.ToString();
            if (req.Url.Query != null && req.Url.Query.Length > 0)
            {
                full = full.Substring(0, full.Length - req.Url.Query.Length);
            }
            if (req.PathInfo != null && req.PathInfo.Length > 0)
            {
                full = full.Substring(0, full.Length - (req.PathInfo.Length + 1));
            }
            full = full.Substring(0, full.LastIndexOf('/') + 1);
            Uri fullUri = new Uri(full);
            full = fullUri.AbsolutePath.ToString();
            return full;
        }

        private void WriteNewTopic(QualifiedTopicRevision theTopic, string postedTopicText, string visitorIdentityString, string version)
        {
            _linkMaker = new LinkMaker(RootUrl(Context.Request));

            QualifiedTopicRevision newVersionName = new QualifiedTopicRevision(theTopic.LocalName, theTopic.Namespace);
            newVersionName.Version = TopicRevision.NewVersionStringForUser(visitorIdentityString, Federation.TimeProvider);
            Federation.NamespaceManagerForTopic(newVersionName).WriteTopicAndNewVersion(newVersionName.LocalName, postedTopicText, visitorIdentityString);
        }


    }
}
