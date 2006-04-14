using System;
using System.Collections.ObjectModel;

namespace FlexWiki.UnitTests
{
    class MockTopicCollection : KeyedCollection<string, MockTopic>
    {
        protected override string GetKeyForItem(MockTopic item)
        {
            return item.Name; 
        }
    }
}
