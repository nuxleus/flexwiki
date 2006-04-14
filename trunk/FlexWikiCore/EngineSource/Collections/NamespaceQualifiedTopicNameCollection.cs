using System;
using System.Collections.ObjectModel;

namespace FlexWiki.Collections
{
    public class NamespaceQualifiedTopicNameCollection : Collection<NamespaceQualifiedTopicName>
    {
        public void AddRange(NamespaceQualifiedTopicNameCollection items)
        {
            foreach (NamespaceQualifiedTopicName item in items)
            {
                this.Add(item); 
            }
        }
    }
}
