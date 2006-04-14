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

using FlexWiki.Collections; 

namespace FlexWiki.UnitTests
{
    internal sealed class TestNamespace
    {
        private string _name;
        private readonly NamespaceProviderParameterCollection _parameters =
            new NamespaceProviderParameterCollection(); 
        private TestTopic[] _topics;

        internal TestNamespace(string name, params TestTopic[] topics)
            :
            this(name, null, topics)
        {
        }

        internal TestNamespace(string name, NamespaceProviderParameterCollection parameters,
            params TestTopic[] topics)
        {
            _name = name;
            _topics = topics;

            if (parameters != null)
            {
                foreach (NamespaceProviderParameter parameter in parameters)
                {
                    _parameters.Add(parameter); 
                }
            }
        }

        internal string Name
        {
            get { return _name; }
        }

        internal NamespaceProviderParameterCollection Parameters
        {
            get { return _parameters; }
        }

        internal TestTopic[] Topics
        {
            get { return _topics; }
        }
    }
}
