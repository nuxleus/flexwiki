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
using System.IO;
using System.Text.RegularExpressions;

using FlexWiki.Collections;
using FlexWiki.Formatting;

namespace FlexWiki
{
    /// <summary>
    /// Summary description for TopicParser.
    /// </summary>
    public static class TopicParser
    {
        /// <summary>
        /// Regex pattern string for extracting a single line propertyName
        /// </summary>
        private const string c_propertyPattern = "^(?<leader>:?)(?<name>([A-Z_][_a-zA-Z0-9.]+)):(?<val>[^\\[{].*)";
        /// <summary>
        /// Regex pattern string for extracting the first line of a multi-line propertyName
        /// </summary>
        private const string c_multilinePropertyPattern = "^(?<leader>:?)(?<name>([A-Z_][_a-zA-Z0-9]+)):(?<delim>[\\[{])(?<val>.*)";

        /// <summary>
        /// Regex for extracting a multi-line propertyName
        /// </summary>
        private static Regex s_multilinePropertyRegex = new Regex(c_multilinePropertyPattern);
        /// <summary>
        /// Regex for extracting a single line propertyName
        /// </summary>
        private static Regex s_propertyRegex = new Regex(c_propertyPattern);



        public static string ClosingDelimiterForOpeningMultilinePropertyDelimiter(string open)
        {
            if (open == null)
            {
                throw new ArgumentNullException("open");
            }

            switch (open)
            {
                case "[":
                    return "]";
                case "{":
                    return "}";
            }
            throw new FormatException("Illegal multiline property delimiter: " + open);
        }
        public static bool IsBehaviorPropertyDelimiter(string s)
        {
            return s == "{" || s == "}";
        }
        public static Regex MultilinePropertyRegex
        {
            get
            {
                return s_multilinePropertyRegex;
            }
        }
        public static ParsedTopic Parse(string text)
        {
            TopicPropertyCollection properties = ParseProperties(text);
            TopicRevisionCollection topicLinks = ParseTopicLinks(text);
            ExternalReferencesMap externalReferences = ParseExternalReferences(text);

            ParsedTopic parsedTopic = new ParsedTopic(properties, topicLinks, externalReferences);

            return parsedTopic;
        }
        public static ParsedTopic Parse(TextReader textReader)
        {
            return Parse(textReader.ReadToEnd());
        }
        public static Regex PropertyRegex
        {
            get
            {
                return s_propertyRegex;
            }
        }
        public static IList<string> SplitTopicPropertyValue(string value)
        {
            string[] values = value.Split(',');

            List<string> listOfValues = new List<string>(values.Length);

            foreach (string v in values)
            {
                listOfValues.Add(v.Trim());
            }

            return listOfValues;
        }
        /// <summary>
        /// Remove any escape characters that are used to force string to be wiki names that wouldn't otherwise be (e.g., '[' and ']')
        /// </summary>
        // TODO: Make this private once parsing moves out of Formatter and into this class       
        public static string StripTopicNameEscapes(string v)
        {
            string answer = v;
            answer = answer.Replace("[", "");
            answer = answer.Replace("]", "");
            return answer;
        }

        private static ExternalReferencesMap ParseExternalReferences(string text)
        {
            // TODO: Move Formatter functionality to Parser

            ExternalReferencesMap references = new ExternalReferencesMap();

            foreach (string line in text.Split('\n'))
            {
                string l = line.Replace("\r", "");
                Formatter.StripExternalWikiDef(references, l);
            }

            return references;

        }
        private static TopicPropertyCollection ParseProperties(string text)
        {
            // TODO: Deal with multiline imports.
            Regex propertyPattern = new Regex(c_propertyPattern, RegexOptions.Multiline);

            TopicPropertyCollection properties = new TopicPropertyCollection();
            foreach (Match match in propertyPattern.Matches(text))
            {
                string name = match.Groups["name"].Value;
                string rawValue = match.Groups["val"].Value.Trim();

                TopicProperty topicProperty = null;
                if (!properties.Contains(name))
                {
                    topicProperty = new TopicProperty(name);
                    properties.Add(topicProperty);
                }
                else
                {
                    topicProperty = properties[name];
                }

                TopicPropertyValue value = new TopicPropertyValue(rawValue);

                topicProperty.Values.Add(value);

            }
            return properties;
        }
        private static TopicRevisionCollection ParseTopicLinks(string text)
        {
            TopicRevisionCollection referencedTopics = new TopicRevisionCollection();

            // TODO: Move Formatter functionality to TopicParser
            MatchCollection wikiNames = Formatter.extractWikiNames.Matches(text);

            List<string> processed = new List<string>();

            foreach (Match m in wikiNames)
            {
                string each = m.Groups["topic"].ToString();
                if (processed.Contains(each))
                {
                    continue;   // skip duplicates
                }

                processed.Add(each);

                TopicRevision referencedTopic = new TopicRevision(StripTopicNameEscapes(each));
                referencedTopics.Add(referencedTopic);
            }

            return referencedTopics;
        }



    }
}
