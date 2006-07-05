using System;
using System.Collections.Generic;
using System.IO; 
using System.Text;

using FlexWiki.Collections;

namespace FlexWiki
{
    public class BuiltinTopicsProvider : ContentProviderBase
    {
        // Constants
        #region Constants
        private const string c_builtInAuthor = "FlexWiki"; 
        private const string c_defaultHomePageContent = @"@flexWiki=http://www.flexwiki.com/default.aspx/FlexWiki/$$$.html

!About Wiki
If you're new to WikiWiki@flexWiki, you should read the VisitorWelcome@flexWiki or OneMinuteWiki@flexWiki .  The two most important things to know are 
  1. follow the links to follow the thoughts and  
  1. YouAreEncouragedToChangeTheWiki@flexWiki

Check out the FlexWikiFaq@flexWiki as a means  to collaborate on questions you may have on FlexWiki@flexWiki
";
        private const string c_defaultNormalBordersContent = @"
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
        #endregion Constants
        
        // Fields

        // Constructors

        public BuiltinTopicsProvider(ContentProviderBase next)
            : base(next)
        {
        }


        // Properties

        // Methods

        public override TopicChangeCollection AllChangesForTopicSince(UnqualifiedTopicName topic, DateTime stamp)
        {
            TopicChangeCollection changes = Next.AllChangesForTopicSince(topic, stamp);

            if (IsBuiltInTopic(topic))
            {
                // All the built-in topics have a default revision at DateTime.MinValue. If the 
                // timestamp is later than that, we don't report back the default revision.
                if (stamp == DateTime.MinValue)
                {
                    if (changes == null)
                    {
                        changes = new TopicChangeCollection();
                    }

                    changes.Insert(0,
                        new TopicChange(
                            new QualifiedTopicRevision(
                                topic.LocalName,
                                NamespaceManager.Namespace,
                                QualifiedTopicRevision.NewVersionStringForUser(c_builtInAuthor, DateTime.MinValue)),
                            DateTime.MinValue,
                            c_builtInAuthor));
                }
            }

            return changes; 
        }
        public override QualifiedTopicNameCollection AllTopics()
        {
            QualifiedTopicNameCollection topics = Next.AllTopics();

            foreach (QualifiedTopicName builtInTopic in GetBuiltInTopics())
            {
                if (!topics.Contains(builtInTopic))
                {
                    topics.Add(builtInTopic); 
                }
            }

            return topics; 
        }
        public override TextReader TextReaderForTopic(UnqualifiedTopicRevision topicRevision)
        {
            // We need to special-case the definition topic because otherwise we 
            // cause an infinite loop: this method calls NamespaceManager.HomePage, 
            // which calls TextReaderForTopic on the definition topic to figure out
            // which topic is the home page. 
            if (IsDefinitionTopic(topicRevision))
            {
                return Next.TextReaderForTopic(topicRevision); 
            }
            else if (!IsBuiltInTopic(topicRevision))
            {
                return Next.TextReaderForTopic(topicRevision); 
            }
            else if (topicRevision.Version == null)
            {
                TopicChangeCollection changes = Next.AllChangesForTopicSince(
                    topicRevision.AsUnqualifiedTopicName(), DateTime.MinValue);
                if (changes == null || changes.Count == 0)
                {
                    string defaultContent = DefaultContentFor(topicRevision.LocalName);
                    return new StringReader(defaultContent);
                }
                else
                {
                    return Next.TextReaderForTopic(topicRevision);
                }
            }
            else if (topicRevision.Version == TopicRevision.NewVersionStringForUser(c_builtInAuthor, DateTime.MinValue))
            {
                string defaultContent = DefaultContentFor(topicRevision.LocalName);
                return new StringReader(defaultContent);
            }
            else
            {
                return Next.TextReaderForTopic(topicRevision);
            }
        }
        public override bool TopicExists(UnqualifiedTopicName name)
        {
            if (IsBuiltInTopic(name))
            {
                return true; 
            }

            return Next.TopicExists(name); 
        }

        private string DefaultContentFor(string topic)
        {
            QualifiedTopicName topicName = new QualifiedTopicName(topic, Namespace);

            if (topicName.Equals(NamespaceManager.HomePageTopicName))
            {
                return c_defaultHomePageContent; 
            }
            else if (topicName.Equals(NamespaceManager.BordersTopicName))
            {
                return c_defaultNormalBordersContent;
            }
            else
            {
                return null; 
            }
            
        }
        private QualifiedTopicNameCollection GetBuiltInTopics()
        {
            // We need to build this every time, because the name of the HomePage can change dynamically.
            QualifiedTopicNameCollection builtInTopics = new QualifiedTopicNameCollection();

            builtInTopics.Add(NamespaceManager.HomePageTopicName);
            builtInTopics.Add(NamespaceManager.BordersTopicName);

            return builtInTopics;
        }
        private bool IsBuiltInTopic(UnqualifiedTopicRevision revision)
        {
            return IsBuiltInTopic(new UnqualifiedTopicName(revision.LocalName));
        }
        private bool IsBuiltInTopic(UnqualifiedTopicName topic)
        {
            QualifiedTopicName qualifiedTopicName = new QualifiedTopicName(topic.LocalName, Namespace);
            return GetBuiltInTopics().Contains(qualifiedTopicName); 
        }
        private bool IsDefinitionTopic(UnqualifiedTopicRevision topicRevision)
        {
            QualifiedTopicName topicName = new QualifiedTopicName(topicRevision.LocalName, Namespace);
            return topicName.Equals(NamespaceManager.DefinitionTopicName);
        }


    }
}
