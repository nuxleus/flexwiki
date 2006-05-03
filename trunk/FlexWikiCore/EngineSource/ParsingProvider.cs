using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using FlexWiki.Collections;

namespace FlexWiki
{
    internal sealed class ParsingProvider : ContentProviderBase
    {
        internal ParsingProvider(ContentProviderBase next)
            : base(next)
        {
        }

        public override DateTime LastRead
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }
        
        
        public override ParsedTopic GetParsedTopic(UnqualifiedTopicRevision revision)
        {
            TextReader textReader = TextReaderForTopic(revision);
            if (textReader == null)
            {
                return null;
            }
            string contents = null;
            try
            {
                contents = textReader.ReadToEnd();
            }
            finally
            {
                textReader.Close();
            }

            ParsedTopic parsedTopic = TopicParser.Parse(contents);

            return parsedTopic;
        }

    }
}
