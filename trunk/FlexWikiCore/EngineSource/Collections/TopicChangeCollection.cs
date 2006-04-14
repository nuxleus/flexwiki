using System;
using System.Collections.ObjectModel;

namespace FlexWiki.Collections
{
    public class TopicChangeCollection : Collection<TopicChange>
    {
        public TopicChange Latest
        {
            get
            {
                if (this.Count == 0)
                {
                    return null; 
                }

                return this[this.Count - 1]; 
            }
        }
    }
}
