using System;
using System.Collections.ObjectModel;
using System.Text;

namespace FlexWiki.Collections
{
    public class TopicRevisionCollection : Collection<TopicRevision>
    {
        public void AddRange(TopicRevisionCollection items)
        {
            foreach (TopicRevision item in items)
            {
                this.Add(item); 
            }
        }

    }
}
