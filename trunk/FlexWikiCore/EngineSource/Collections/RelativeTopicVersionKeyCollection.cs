using System;
using System.Collections.ObjectModel;
using System.Text;

namespace FlexWiki.Collections
{
    public class RelativeTopicVersionKeyCollection : Collection<RelativeTopicVersionKey>
    {
        public void AddRange(RelativeTopicVersionKeyCollection items)
        {
            foreach (RelativeTopicVersionKey item in items)
            {
                this.Add(item); 
            }
        }

        public ReadOnlyRelativeTopicNameCollection AsReadOnly()
        {
            return new ReadOnlyRelativeTopicNameCollection(this); 
        }
    }
}
