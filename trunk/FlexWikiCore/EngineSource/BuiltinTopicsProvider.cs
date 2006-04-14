using System;
using System.Collections.Generic;
using System.Text;

using FlexWiki.Collections; 

namespace FlexWiki
{
    public class BuiltinTopicsProvider : IUnparsedContentProvider
    {
        // Constructors

        public BuiltinTopicsProvider(IUnparsedContentProvider next) 
        {
            _next = next; 
        }

        // Constants
        #region Constants
        private const string _defaultHomePageContent = @"@flexWiki=http://www.flexwiki.com/default.aspx/FlexWiki/$$$.html

!About Wiki
If you're new to WikiWiki@flexWiki, you should read the VisitorWelcome@flexWiki or OneMinuteWiki@flexWiki .  The two most important things to know are 
  1. follow the links to follow the thoughts and  
  1. YouAreEncouragedToChangeTheWiki@flexWiki

Check out the FlexWikiFaq@flexWiki as a means  to collaborate on questions you may have on FlexWiki@flexWiki
";
        private const string _defaultNormalBordersContent = @"
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

        private IUnparsedContentProvider _next;

        // Properties

        bool IContentProvider.Exists
        {
            get { throw new NotImplementedException(); }
        }

        bool IContentProvider.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        DateTime IContentProvider.LastRead
        {
            get { throw new NotImplementedException(); }
        }

        IUnparsedContentProvider IUnparsedContentProvider.Next
        {
            get { return _next; }
        }

        // Methods

        TopicChangeCollection IContentProvider.AllChangesForTopicSince(string topic, DateTime stamp)
        {
            throw new NotImplementedException();
        }

        NamespaceQualifiedTopicNameCollection IContentProvider.AllTopics()
        {
            throw new NotImplementedException();
        }

        void IContentProvider.DeleteAllTopicsAndHistory()
        {
            throw new NotImplementedException();
        }

        void IContentProvider.DeleteTopic(string topic)
        {
            throw new NotImplementedException();
        }

        void IContentProvider.Initialize(NamespaceManager manager)
        {
            throw new NotImplementedException();
        }

        bool IContentProvider.IsExistingTopicWritable(string topic)
        {
            throw new NotImplementedException();
        }

        System.IO.TextReader IContentProvider.TextReaderForTopic(string topic, string version)
        {
            throw new NotImplementedException();
        }

        bool IContentProvider.TopicExists(string name)
        {
            throw new NotImplementedException();
        }

        void IContentProvider.WriteTopic(string topic, string version, string content)
        {
            throw new NotImplementedException();
        }

        void IContentProvider.WriteTopicAndNewVersion(string topic, string content, string author)
        {
            throw new NotImplementedException();
        }

    }
}
