using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlexWiki
{
    public interface IUnparsedContentProvider : IContentProvider
    {
        IUnparsedContentProvider Next { get; }
    }
}
