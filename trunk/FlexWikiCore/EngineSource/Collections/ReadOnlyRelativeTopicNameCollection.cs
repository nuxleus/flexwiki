using System;
using System.Collections.Generic; 
using System.Collections.ObjectModel;
using System.Text;

namespace FlexWiki.Collections
{
    public class ReadOnlyRelativeTopicNameCollection : ReadOnlyCollection<RelativeTopicVersionKey>
    {
        public ReadOnlyRelativeTopicNameCollection(IList<RelativeTopicVersionKey> items) :
            base(items)
        {
        }
    }
}
