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
using System.Text;
using System.Text.RegularExpressions;


namespace FlexWiki
{
	/// <summary>
	/// A TopicChange describes a topic change (which topic, who changed it and when)
	/// </summary>
	[ExposedClass("TopicChange", "Describes a single change to a topic")]
	public class TopicChange : BELObject, IComparable
	{
		public TopicChange(AbsoluteTopicName topic)
		{

			try
			{
				_Topic = topic;

				Regex re = new Regex("(?<year>[0-9]{4})-(?<month>[0-9]{2})-(?<day>[0-9]{2})-(?<hour>[0-9]{2})-(?<minute>[0-9]{2})-(?<second>[0-9]{2})(\\.(?<fraction>[0-9]+))?(-(?<name>.*))?");
				if (!re.IsMatch(topic.Version))
					throw new FormatException("Illegal wiki archive filename: " + topic.Version);
				Match match = re.Match(topic.Version);
				// Format into: "2/16/1992 12:15:12";
				int frac = 0;
				if (match.Groups["fraction"] != null)
				{
					string fracs = "0." + match.Groups["fraction"].Value;
					try
					{
						Decimal f = Decimal.Parse(fracs, new System.Globalization.CultureInfo("en-US"));
						frac = (int)(1000 * f);
					}
					catch (FormatException ex)
					{
						ex.ToString();		// shut up compiler;
					}
				}

				_Timestamp = new DateTime(
					SafeIntegerParse(match.Groups["year"].Value), // month
					SafeIntegerParse(match.Groups["month"].Value), // day
					SafeIntegerParse(match.Groups["day"].Value), // year
					SafeIntegerParse(match.Groups["hour"].Value), // hour
					SafeIntegerParse(match.Groups["minute"].Value), // minutes
					SafeIntegerParse(match.Groups["second"].Value), // seconds
					frac);
				_Author = match.Groups["name"].Value;
			}
			catch (Exception ex)
			{
				throw new Exception("Exception processing topic change " + topic.Version + " - " + ex.ToString());
			}
		}

		int SafeIntegerParse(string input)
		{
			try
			{
				return Int32.Parse(input);
			}
			catch (FormatException ex)
			{
				ex.ToString();		// shut up compiler!
				return 0;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the version stamp for this change")]
		public string Version
		{
			get
			{
				return Topic.Version;
			}
		}

                                                     
		public int CompareTo(object obj)
		{
			if (!(obj is TopicChange))
				return -1;
			TopicChange other = (TopicChange)obj;
			int answer;
			answer = Topic.CompareTo(other.Topic);
			if (answer != 0)
				return answer;
			return Timestamp.CompareTo(other.Timestamp);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the full name of the topic whose change is describe dby this TopicChange")]
		public string Fullname
		{
			get
			{
				return Topic.ToString();
			}
		}


			public AbsoluteTopicName Topic
		{
			get
			{
				return _Topic;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the name of the author of this change")]
		public string Author
		{
			get
			{
				return _Author;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a DateTime that indicates when the change was made")]
		public DateTime Timestamp
		{
			get
			{
				return _Timestamp;
			}
		}

		private AbsoluteTopicName _Topic;
		private string _Author;
		private DateTime _Timestamp;	
	}
}
