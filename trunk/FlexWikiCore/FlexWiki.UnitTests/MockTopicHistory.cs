using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.UnitTests
{
    class MockTopicHistory
    {
        private string _author;
        private string _contents;
        private DateTime _created;
        private DateTime _modified; 
        private string _version;

        internal MockTopicHistory(string contents, string author, DateTime created)
        {
            _contents = contents;
            _author = author;
            _created = created;
            _modified = created; 
            _version = TopicRevision.NewVersionStringForUser(author, created);
        }

        internal string Author
        {
            get { return _author; }
        }

        internal string Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

        internal DateTime Created
        {
            get { return _created; }
        }

        internal DateTime Modified
        {
            get { return _modified; }
            set { _modified = value; } 
        }

        internal string Version
        {
            get
            {
                return _version;

            }
        }
    }
}
