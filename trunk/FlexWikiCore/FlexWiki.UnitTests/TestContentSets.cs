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

using FlexWiki.Collections;

namespace FlexWiki.UnitTests
{
    internal static class TestContentSets
    {
        internal static TestContentSet Empty
        {
            get
            {
                return new TestContentSet();
            }
        }
        internal static TestContentSet ImportingReferencingSet
        {
            get
            {
                return new TestContentSet(
                  new TestNamespace("NamespaceOne",
                    new TestTopic("_ContentBaseDefinition", "author", "Import: NamespaceTwo"),
                    new TestTopic("ReferencingTopic", "author", "References ReferencedTopic and NonExistentTopic and ImportedTopic and NamespaceTwo.OtherTopic"),
                    new TestTopic("ReferencedTopic", "author", "This topic is the target of a reference.")
                  ),
                  new TestNamespace("NamespaceTwo",
                    new TestTopic("ReferencedTopic", "author", "This topic is also the target of a reference."),
                    new TestTopic("ImportedTopic", "author", "This topic is also the target of a reference via an import."),
                    new TestTopic("OtherTopic", "author", "This topic exists only in NamespaceTwo, but it references NamespaceOne.ReferencedTopic.")
                  )
                );
            }
        }
        internal static TestContentSet MultipleTopicsWithProperties
        {
            get
            {
                return new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "author1", @"PropertyOne: Value one
PropertyTwo: Value two
OtherProperty: Some value
OtherProperty: Some other value"),
                        new TestTopic("TopicTwo", "author2", @"PropertyTwo: Value two
PropertyThree: Value three
OtherProperty: Some other value"),
                        new TestTopic("TopicThree", "author3", @"PropertyThree: Value three, 
PropertyFour: Value four"), 
                        new TestTopic("_ContentBaseDefinition", "author", @"Import: NamespaceTwo")
                    ), 
                    new TestNamespace("NamespaceTwo", 
                        new TestTopic("TopicOne", "author1-2", @"PropertyOne: Value one"), 
                        new TestTopic("TopicFour", "author4-1", @"PropertyFive: Value five")
                    )
                );
            }
        }
        internal static TestContentSet MultipleVersions
        {
            get
            {
                return new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new TestTopic("TopicOne", "author1", "content1"),
                        new TestTopic("TopicOne", "author2", "content2"),
                        new TestTopic("TopicOne", "author3", "content3")
                )
                );
            }
        }
        internal static TestContentSet NamespaceWithInfoProperties
        {
            get
            {
                return new TestContentSet(
                  new TestNamespace("NamespaceOne",
                    new TestTopic("_ContentBaseDefinition", "author", @"Title: The title
Contact: Craig Andera
Description: The description
DisplaySpacesInWikiLinks: true
ImageURL: http://server/vdir/
HomePage: DifferentHomePage")
                  )
                );
            }
        }
        internal static TestContentSet NonImportingReferencingSet
        {
            get
            {
                return new TestContentSet(
                  new TestNamespace("NamespaceOne",
                    new TestTopic("ReferencingTopic", "author", "References ReferencedTopic and NamespaceTwo.ReferencedTopic and NonExistentTopic"),
                    new TestTopic("ReferencedTopic", "author", "This topic is the target of a reference.")
                  ),
                  new TestNamespace("NamespaceTwo",
                    new TestTopic("ReferencedTopic", "author", "This topic is also the target of a reference."),
                    new TestTopic("OtherTopic", "author", "This topic exists only in the other namespace.")
                  )
                );
            }
        }
        internal static TestContentSet SimpleImport
        {
            get
            {
                return
                  new TestContentSet(
                    new TestNamespace("NamespaceOne",
                      new TestTopic("TopicOne", "author", "content"),
                      new TestTopic("_ContentBaseDefinition", "author", "Import: NamespaceTwo")
                    ),
                    new TestNamespace("NamespaceTwo",
                      new TestTopic("TopicOne", "author", "content in NamespaceTwo")
                    )
                  );
            }
        }
        internal static TestContentSet SingleEmptyNamespace
        {
            get
            {
                return new TestContentSet(
                  new TestNamespace("NamespaceOne")
                );
            }
        }
        internal static TestContentSet SingleEmptyNamespaceWithParameters
        {
            get
            {
                return new TestContentSet(
                    new TestNamespace("NamespaceOne",
                        new NamespaceProviderParameterCollection(
                            new NamespaceProviderParameter("foo", "bar"),
                            new NamespaceProviderParameter("quux", "baaz")
                        )
                    )
                );
            }
        }
        internal static TestContentSet SingleTopicNoImports
        {
            get
            {
                return new TestContentSet(new TestNamespace("NamespaceOne", new TestTopic("TopicOne", "author", "content")));
            }
        }
        internal static TestContentSet SingleTopicWithProperty
        {
            get
            {
                return new TestContentSet(new TestNamespace("NamespaceOne",
                    new TestTopic("TopicOne", "author", "Property: Old rawValue")));
            }
        }


    }
}