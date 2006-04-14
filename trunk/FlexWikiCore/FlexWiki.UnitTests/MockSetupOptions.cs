using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki.UnitTests
{
    [Flags]
    internal enum MockSetupOptions
    {
        Default                     = 0x00, 
        ReadOnlyStore               = 0x01, 
        EnableBuiltinTopics         = 0x02,
        StoreDoesNotExist           = 0x04,
    }
}
