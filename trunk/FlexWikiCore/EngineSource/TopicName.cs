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
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace FlexWiki
{
	/// <summary>
	/// Abstract class that holds a topic name (composed of three parts: name, namespace and version).  
	/// There are two key subclasses (AbsoluteTopicName and RelativeTopicName).
	/// </summary>
	public abstract class TopicName : IComparable
	{
		LocalTopicName _LocalName = new LocalTopicName();
		string _Namespace;

		public override string ToString()
		{
			return FullnameWithVersion;
		}

		public string FullnameWithVersion
		{
			get
			{
				string answer = Fullname;
				if (Version != null)
					answer += "(" + Version + ")";
				return answer;
			}
			set
			{ 
				string v = value;
				// start by triming off the version if present
				Version = null;
				if (v.EndsWith(")"))
				{
					int open = v.IndexOf("(");
					if (open >= 0)
					{
						Version = v.Substring(open + 1, v.Length - open - 2);
						if (Version == "")
							Version = null;
						v = v.Substring(0, open);
					}
				}

				// Now grab any namespace
				int dot = v.LastIndexOf(Separator);
				Namespace = null;
				if (dot >= 0)
				{
					Namespace = v.Substring(0, dot);
					if (Namespace == "")
						Namespace = null;
					v = v.Substring(dot + 1);
				}
				Name = v;
			}
		}

		/// <summary>
		/// Answer a collection of topic names for the alternate forms of the topic name (e.g., signular forms of plural words)
		/// </summary>
		[XmlIgnore]
		public IEnumerable AlternateForms
		{
			get
			{
				ArrayList answer = new ArrayList();
				string each = ToString();
				if (each.EndsWith("s"))
					answer.Add(NewOfSameType(each.Substring(0, each.Length - 1)));
				if (each.EndsWith("ies"))
					answer.Add(NewOfSameType(each.Substring(0, each.Length - 3) + "y"));
				if (each.EndsWith("sses"))
					answer.Add(NewOfSameType(each.Substring(0, each.Length - 2)));
				if (each.EndsWith("xes"))
					answer.Add(NewOfSameType(each.Substring(0, each.Length - 2)));
				return answer;
			}
		}

		public abstract TopicName NewOfSameType(string topic);

		/// <summary>
		/// Compare two TopicNames.  Topic names are equal if their name, namespace and version components are equal (case-insensitive)
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return obj is TopicName && ((TopicName)obj).FullnameWithVersion.ToLower()  == FullnameWithVersion.ToLower();
		}

		public override int GetHashCode()
		{
			return Fullname.GetHashCode ();
		}

		/// <summary>
		/// Answer a version string that can be used to identify a topic version for the supplied user.
		/// </summary>
		/// <param name="user">a username string</param>
		/// <returns></returns>
		public static string NewVersionStringForUser(string user)
		{
			string u = user;
			u = u.Replace('\\', '-');
			u = u.Replace('?', '-');
			u = u.Replace('/', '-');
			u = u.Replace(':', '-');
			return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss.ffff") + "-" + u;
		}

		/// <summary>
		/// Remove any escape characters that are used to force string to be wiki names that wouldn't otherwise be (e.g., '[' and ']')
		/// </summary>
		public static string StripEscapes(string v)
		{
			string answer = v;
			answer = answer.Replace("[", "");
			answer = answer.Replace("]", "");
			return answer;
		}

		/// <summary>
		/// Answer the name (without namespace) with spaces inserted to make the name more readable
		/// </summary> 
		public string FormattedName
		{
			get
			{
				return LocalName.FormattedName;
			}
		}

		public LocalTopicName LocalName
		{
			get
			{
				return _LocalName;
			}
		}

		public string Version
		{
			get
			{
				return LocalName.Version;
			}
			set
			{
				LocalName.Version = value;
			}
		}

		/// <summary>
		/// Answer this as an absolute topic name.  If this is an absolute name already, answer it.  If it isn't, 
		/// answer an absolute name, filling in any unspecified namespace with the supplied default.
		/// </summary>
		/// <param name="defaultNamespace"></param>
		/// <returns></returns>
		public abstract AbsoluteTopicName AsAbsoluteTopicName(string defaultNamespace);

		/// <summary>
		/// Default contructor for XML Serialization.
		/// </summary>
		public TopicName()
		{		
		}

		public TopicName(string topic)
		{
			FullnameWithVersion = topic;
		}

		static protected string Separator = ".";

		public TopicName(string topic, string theNamespace)
		{
			Name = topic;
			Namespace = theNamespace;
		}

		public string Fullname
		{
			get
			{
				string answer = "";
				if (Namespace != null)
					answer += Namespace + Separator;
				answer += Name;
				return answer;
			}
			set
			{ 
				string v = value;
				int dot = v.LastIndexOf(Separator);
				Namespace = null;
				if (dot >= 0)
				{
					Namespace = v.Substring(0, dot);
					if (Namespace == "")
						Namespace = null;
					v = v.Substring(dot + 1);
				}
				Name = v;
			}
		}

		public string Name
		{
			get
			{
				return LocalName.Name;
			}
			set
			{
				LocalName.Name = value;
			}
		}

		public string Namespace
		{
			get
			{
				return _Namespace;
			}
			set
			{
				_Namespace = value;
			}
		}

		public abstract IList AllAbsoluteTopicNamesFor(ContentBase cb);

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj is TopicName)
				return -1;
			return FullnameWithVersion.CompareTo((obj as TopicName).FullnameWithVersion);
		}

		#endregion
	}
}
