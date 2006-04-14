#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace FlexWiki
{
    /// <summary>
    /// Abstracts a content provider. 
    /// </summary>
    /// <remarks>Content providers can sit in front of physical content stores (which implement
    /// <see cref="IUnparsedContentProvider"/> that actually
    /// store content (e.g. <see cref="FileSystemStore"/>. Content providers are filtering implementations that 
    /// provide services over content stores (e.g. caching or security).</remarks>
    public interface IParsedContentProvider : IContentProvider
    {
        /// <summary>
        /// Gets or sets the next <see cref="IParsedContentProvider"/> in the filter chain, if any. 
        /// </summary>
        IParsedContentProvider Next { get; }

        /// <summary>
        /// Returns the parsed representation of the specified topic. 
        /// </summary>
        /// <param name="topic">The topic for which to return the parsed representation.</param>
        /// <returns>A <see cref="ParsedTopic"/> object containing the parsed representation of the topic.</returns>
        ParsedTopic GetParsedTopic(string topic); 

    }
}
