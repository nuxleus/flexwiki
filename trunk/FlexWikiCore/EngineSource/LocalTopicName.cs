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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for LocalTopicName.
	/// </summary>
	public class LocalTopicName : IComparable
	{
		public LocalTopicName()
		{
		}
		
		public LocalTopicName(string name)
		{
			Name = name;
		}

		public LocalTopicName(string name, string ver)
		{
			Name = name;
			Version = ver;
		}

		string _Name;

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

		string _Version;
		public string Version
		{
			get
			{
				return _Version;
			}
			set
			{
				_Version = value;
			}
		}

		public override string ToString()
		{
			return NameWithVersion;
		}

		public string NameWithVersion
		{
			get
			{
				string answer = Name;
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
				Name = v;
			}
		}

		/// <summary>
		/// Compare two LocalTopicNames.  They are equal if their name and version components are equal (case-insensitive)
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			return obj is LocalTopicName && ((LocalTopicName)obj).NameWithVersion.ToLower()  == NameWithVersion.ToLower();
		}

		public override int GetHashCode()
		{
			return NameWithVersion.GetHashCode ();
		}

		/// <summary>
		/// Answer the name with spaces inserted to make the name more readable
		/// </summary> 
		public string FormattedName
		{
			get
			{
				// basic breaking rule: break between lowercase to uppercase transitions
				// caveat: don't break before first cap
				bool firstCap = true;
				bool lastWasLower = false;
				string answer = "";
				string scan = Name;
				for (int i = 0; i < scan.Length; i++)
				{
					char ch = Name[i];
					if (!Char.IsUpper(ch))
					{
						answer += ch;
						lastWasLower = true;
						continue;
					}

					if (firstCap)
					{
						firstCap = false;
						answer += ch;
						lastWasLower = false;
						continue;
					}

					if (lastWasLower || ((i + 1) < scan.Length && Char.IsLower(Name[i + 1])))
					{
						answer += ' ';
					}
					answer += ch;
					lastWasLower = false;
				}
				return answer;
			}
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj is LocalTopicName)
				return -1;
			return NameWithVersion.CompareTo((obj as LocalTopicName).NameWithVersion);
		}

		#endregion

	}
}
