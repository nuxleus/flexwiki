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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using FlexWiki.Collections; 

namespace FlexWiki
{
    /// <summary>
    /// Represents the values of a propertyName within a wiki topic. 
    /// </summary>
    public class TopicProperty
    {
        public TopicProperty(string name)
        {
            _name = name;
        }

        private string _name;
        private readonly TopicPropertyValueCollection _values = new TopicPropertyValueCollection();

        public IList<string> AsList()
        {
            List<string> values = new List<string>();

            foreach (TopicPropertyValue propertyValue in Values)
            {
                foreach (string value in propertyValue.AsList())
                {
                    values.Add(value); 
                }
            }

            return values;
        }

        public bool HasValue
        {
            get
            {
                return Values.Count != 0; 
            }
        }

        public string LastValue
        {
            get
            {
                int count = Values.Count;

                if (count == 0)
                {
                    return null; 
                }

                return Values[count - 1].RawValue; 
            }
        }

        public string Name
        {
            get { return _name; }
        }

        public TopicPropertyValueCollection Values
        {
            get { return _values; }
        }

    }
}
