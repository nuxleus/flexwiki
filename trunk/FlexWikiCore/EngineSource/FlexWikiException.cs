using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public class FlexWikiException : Exception
    {
        public FlexWikiException()
        {
        }

        public FlexWikiException(string message)
            : base(message)
        {
        }

        public FlexWikiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected FlexWikiException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }

        internal static FlexWikiException QualifiedTopicNameExpected(TopicName topicName)
        {
            if (topicName == null)
            {
                return new FlexWikiException("A null topic name was specified where a fully qualified topic name was expected."); 
            }

            return new FlexWikiException("A topic name without a namespace was specified where a fully-qualified topic name was expected. The topic name was " + 
                topicName.LocalName ?? "<<null>>"); 
        }

        internal static FlexWikiException NamespaceDoesNotExist(TopicName topic)
        {
            if (topic == null)
            {
                return new FlexWikiException("A null topic name was specified."); 
            }
            return new FlexWikiException("The namespace " + (topic.Namespace ?? "<<null>>") +
                " does not exist."); 
        }

        internal static FlexWikiException VersionDoesNotExist(TopicRevision revision)
        {
            return VersionDoesNotExist(revision.Name, revision.Version); 
        }

        internal static FlexWikiException VersionDoesNotExist(TopicName topic, string version)
        {
            if (topic == null)
            {
                return new FlexWikiException("A null topic name was specified."); 
            }
            return new FlexWikiException(string.Format("Topic {0} does not have a version {1}.", 
                topic.DottedName, 
                version ?? "<<null>>")); 
        }
    }
}
