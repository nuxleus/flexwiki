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

using FlexWiki.Collections; 

namespace FlexWiki.UnitTests
{
    /// <summary>
    /// Summary description for WikiTestUtilities.
    /// </summary>
    internal static class WikiTestUtilities
    {
        internal static Federation SetupFederation(string siteUrl, TestContentSet content)
        {
            return SetupFederation(siteUrl, content, MockSetupOptions.Default); 
        }

        internal static Federation SetupFederation(string siteUrl, TestContentSet content,
            FederationConfiguration federationConfiguration)
        {
            return SetupFederation(siteUrl, content, MockSetupOptions.Default, federationConfiguration); 
        }

        internal static Federation SetupFederation(string siteUrl, TestContentSet content, MockSetupOptions options)
        {
            return SetupFederation(siteUrl, content, options, new FederationConfiguration()); 
        }

        internal static Federation SetupFederation(string siteUrl, TestContentSet content, MockSetupOptions options, 
            FederationConfiguration federationConfiguration)
        {
            LinkMaker linkMaker = new LinkMaker(siteUrl);
            MockWikiApplication application = new MockWikiApplication(
                federationConfiguration, linkMaker, 
                OutputFormat.HTML, 
                new MockTimeProvider(TimeSpan.FromSeconds(1)));
            Federation federation = new Federation(application);

            foreach (TestNamespace ns in content.Namespaces)
            {
                NamespaceManager storeManager = CreateMockStore(federation, ns.Name, options, ns.Parameters);

                foreach (TestTopic topic in ns.Topics)
                {
                    WriteTestTopicAndNewVersion(storeManager, topic.Name, topic.Content, topic.Author);
                }
            }

            return federation;

        }

        internal static NamespaceManager CreateMockStore(Federation federation, string ns)
        {
            return CreateMockStore(federation, ns, MockSetupOptions.Default); 
        }

        internal static NamespaceManager CreateMockStore(Federation federation, string ns, MockSetupOptions options)
        {
            return CreateMockStore(federation, ns, options, null); 
        }

        internal static NamespaceManager CreateMockStore(Federation federation, string ns,
            NamespaceProviderParameterCollection parameters)
        {
            return CreateMockStore(federation, ns, MockSetupOptions.Default, parameters); 
        }

        internal static NamespaceManager CreateMockStore(Federation federation, string ns, 
            MockSetupOptions options, NamespaceProviderParameterCollection parameters)
        {
            MockContentStore store = new MockContentStore(options);
            return federation.RegisterNamespace(store, ns, parameters);
        }

        internal static QualifiedTopicRevision WriteTestTopicAndNewVersion(NamespaceManager namespaceManager,
          string localName, string content, string author)
        {
            QualifiedTopicRevision name = new QualifiedTopicRevision(localName, namespaceManager.Namespace);
            name.Version = QualifiedTopicRevision.NewVersionStringForUser(author, 
                namespaceManager.Federation.TimeProvider.Now);
            namespaceManager.WriteTopicAndNewVersion(name.LocalName, content, author);
            return name;
        }



    }
}
