using System;
using System.Collections.Generic;
using System.Text;

using FlexWiki.Collections;

namespace FlexWiki
{
    /// <summary>
    /// Represents a topic for which the namespace may or may not be known. 
    /// </summary>
    public class TopicName
    {
        // Constants

        private const string c_separator = ".";

        // Fields

        private string _localName;
        private string _namespace;

        // Constructors

        public TopicName()
        {
        }
        public TopicName(string name)
        {
            if (name == null)
            {
                throw new ArgumentException("A null topic name is not legal."); 
            }

            int dot = name.LastIndexOf(c_separator);
            _namespace = null;
            if (dot >= 0)
            {
                _namespace = name.Substring(0, dot);
                if (_namespace == "")
                {
                    _namespace = null;
                }
                name = name.Substring(dot + 1);
            }
            _localName = name;

        }
        public TopicName(string localName, string ns)
        {
            if (localName.Contains(c_separator))
            {
                throw new ArgumentException("An illegal local name was specified: the namespace separator is not allowed as part of a local name."); 
            }

            _localName = localName;
            _namespace = ns;
        }

        // Properties

        public string FormattedName
        {
            get
            {
                // basic breaking rule: break between lowercase to uppercase transitions
                // caveat: don't break before first cap
                bool firstCap = true;
                bool lastWasLower = false;
                string formattedName = "";
                string scan = LocalName;
                for (int i = 0; i < scan.Length; i++)
                {
                    char ch = scan[i];
                    if (!Char.IsUpper(ch))
                    {
                        formattedName += ch;
                        lastWasLower = true;
                        continue;
                    }

                    if (firstCap)
                    {
                        firstCap = false;
                        formattedName += ch;
                        lastWasLower = false;
                        continue;
                    }

                    if (lastWasLower || ((i + 1) < scan.Length && Char.IsLower(scan[i + 1])))
                    {
                        formattedName += ' ';
                    }
                    formattedName += ch;
                    lastWasLower = false;
                }
                return formattedName;

            }
        }
        public bool IsQualified
        {
            get { return Namespace != null; }
        }
        public string LocalName
        {
            get { return _localName; }
            set { _localName = value; }
        }
        public virtual string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }
        public string DottedName
        {
            get
            {
                if (IsQualified)
                {
                    return Namespace + c_separator + LocalName;
                }
                else
                {
                    return LocalName;
                }
            }
        }
        protected static string Separator
        {
            get { return c_separator; }
        }

        /// <summary>
        /// Answer a collection of topic names for the alternate forms of the topic name (e.g., signular forms of plural words)
        /// </summary>
        public TopicNameCollection AlternateForms()
        {
            TopicNameCollection answer = new TopicNameCollection();
            if (LocalName.EndsWith("s"))
            {
                answer.Add(new TopicName(LocalName.Substring(0, LocalName.Length - 1), Namespace));
            }
            if (LocalName.EndsWith("ies"))
            {
                answer.Add(new TopicName(LocalName.Substring(0, LocalName.Length - 3) + "y", Namespace));
            }
            if (LocalName.EndsWith("sses"))
            {
                answer.Add(new TopicName(LocalName.Substring(0, LocalName.Length - 2), Namespace));
            }
            if (LocalName.EndsWith("xes"))
            {
                answer.Add(new TopicName(LocalName.Substring(0, LocalName.Length - 2), Namespace));
            }
            return answer;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            TopicName that = obj as TopicName; 

            if (that == null)
            {
                return false; 
            }

            return this.DottedName == that.DottedName; 
        }
        public override int GetHashCode()
        {
            string qualifiedName = DottedName;

            if (qualifiedName == null)
            {
                return "<<null>>".GetHashCode(); 
            }

            return qualifiedName.GetHashCode(); 
        }
        public QualifiedTopicName ResolveRelativeTo(string ns)
        {
            if (IsQualified)
            {
                return new QualifiedTopicName(LocalName, Namespace);
            }
            else
            {
                return new QualifiedTopicName(LocalName, ns);
            }
        }

    }
}
