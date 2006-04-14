using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; 

namespace FlexWiki
{
    public class TopicPropertyCollection : KeyedCollection<string, TopicProperty>
    {
        public IList<string> Names
        {
            get
            {
                List<string> names = new List<string>();

                foreach (TopicProperty property in this)
                {
                    names.Add(property.Name); 
                }

                return names; 
            }
        }

        public void AddRange(IEnumerable<TopicProperty> properties)
        {
            foreach (TopicProperty property in properties)
            {
                this.Add(property); 
            }
        }

        protected override string GetKeyForItem(TopicProperty item)
        {
            return item.Name; 
        }
    }
}
