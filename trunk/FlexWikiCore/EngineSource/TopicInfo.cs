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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for Topic.
	/// </summary>
	[ExposedClass("TopicInfo", "Provides information about a topic")]
	public class TopicInfo : BELObject, IComparable
	{
		public TopicInfo(Federation aFed, AbsoluteTopicName name)
		{
			_Fullname = name;
			_Federation = aFed;
		}

		AbsoluteTopicName _Fullname;

		public AbsoluteTopicName Fullname
		{
			get
			{
				return _Fullname;
			}
		}

		[ExposedMethod("Fullname", ExposedMethodFlags.CachePolicyNone, "Answer the complete name of the topic (including namespace and version, if present)")]
		public string ExposedFullname
		{
			get
			{
				return _Fullname.ToString();
			}
		}


		Federation _Federation;

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the Federation containing the topic")]
		public Federation Federation
		{
			get
			{
				return _Federation;
			}
		}

		ContentBase _ContentBase;
		[ExposedMethod("Namespace", ExposedMethodFlags.CachePolicyNone, "Answer the Namespace for this topic")]
		public ContentBase ContentBase
		{
			get
			{
				if (_ContentBase != null)
					return _ContentBase;
				_ContentBase = Federation.ContentBaseForTopic(Fullname);
				return _ContentBase;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a list of TopicChanges describing this topic's history")]
		public ArrayList Changes
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (TopicChange each in Federation.GetTopicChanges(Fullname))
					answer.Add(each);
				return answer;
			}
		}

		public bool Exists
		{
			get
			{
				return ContentBase.TopicExists(Fullname);
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a the (full)name of the topic")]
		public string Name
		{
			get
			{
				return Fullname.Name;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the version stamp for this version of the topic")]
		public string Version
		{
			get
			{
				return Fullname.Version;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the given property from the topic")]
		public string GetProperty(string propertyName)
		{
			return Federation.GetTopicProperty(Fullname, propertyName);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer an array of the values in the given list property")]
		public ArrayList GetListProperty(string propertyName)
		{
			return Federation.GetTopicListPropertyValue(Fullname, propertyName);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if the given toipic has the given property; else false")]
		public bool HasProperty(string propertyName)
		{
			Hashtable hash = Federation.GetTopicProperties(Fullname);
			if (hash == null)
				return false;
			return hash.ContainsKey(propertyName);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the list of property names for this topic")]
		public ArrayList PropertyNames
		{
			get
			{
				return new ArrayList(Federation.GetTopicProperties(Fullname).Keys);
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the Summary property's value")]
		public string Summary
		{
			get
			{
				return GetProperty("Summary");
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the Keywords property's value")]
		public string Keywords
		{
			get
			{
				return GetProperty("Keywords");
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer an array containgin all the keywords listed in the Keywords property")]
		public ArrayList KeywordsList
		{
			get
			{
				return GetListProperty("Keywords");
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a DateTime inndicating when the topic was last modified")]
		public DateTime LastModified
		{
			get
			{
				return Federation.GetTopicModificationTime(Fullname); 
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a DateTime inndicating when the topic was created")]
		public DateTime Created
		{
			get
			{
				return Federation.GetTopicCreationTime(Fullname); 
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a string indicating who last modified the topic")]
		public string LastModifiedBy
		{
			get
			{
				return Federation.GetTopicLastModifiedBy(Fullname); 
			}
		}
		#region IComparable Members

		public int CompareTo(object obj)
		{
			return this.Fullname.Fullname.CompareTo(((TopicInfo)obj).Fullname.Fullname);
		}

		#endregion
	}
}
