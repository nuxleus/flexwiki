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
using System.Xml.Serialization;
using System.Reflection;

namespace FlexWiki
{
    /// <summary>
    /// 
    /// </summary>
    public class NamespaceProviderParameter
    {
        public NamespaceProviderParameter(string name, string val)
        {
            _Name = name;
            _Value = val;
        }

        public NamespaceProviderParameter()
        {
        }

        [XmlAttribute]
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }
        string _Name;

        [XmlAttribute]
        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }
        string _Value;

    }
}
