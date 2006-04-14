using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public class TopicPropertyValue
    {
        public TopicPropertyValue()
        {
        }

        public TopicPropertyValue(string rawValue)
        {
            _rawValue = rawValue; 
        }

        private string _rawValue;

        public string RawValue
        {
            get
            {
                return _rawValue; 
            }
        }

        //CA Can't decide if this should be a method or a propertyName
        public IList<string> AsList()
        {
            return TopicParser.SplitTopicPropertyValue(_rawValue); 
        }
    }
}
