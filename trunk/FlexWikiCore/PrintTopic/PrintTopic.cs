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
using FlexWiki;
using FlexWiki.Formatting;

namespace PrintTopic
{
	/// <summary>
	/// Summary description for PrintTopic.
	/// </summary>
	class PrintTopic
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// 

		class Cache : IFederationCache
		{
			Hashtable Hash = new Hashtable();

			#region IFederationCache Members

			class Entry
			{
				public object Value;
				public CacheRule Rule;
				public Entry(object v)
				{
					Value = v;
				}
				public Entry(object v, CacheRule r)
				{
					Value = v;
					Rule = r;
				}
			}

			public object this[string key]
			{
				get
				{
					Entry e = (Entry)Hash[key];
					if (e == null)
						return null;
					return e.Value;
				}
				set
				{
					Hash[key] = new Entry(value);
				}
			}

			public void Put(string key, object val, CacheRule rule)
			{
				Hash[key] = new Entry(val, rule);
			}

			void FlexWiki.IFederationCache.Put(string key, object val)
			{
				this[key] = val;
			}

			public CacheRule GetRuleForKey(string key)
			{
				Entry e = (Entry)Hash[key];
				if (e == null)
					return null;
				return e.Rule;
			}

			public object Get(string key)
			{
				return this[key];
			}

			public System.Collections.ICollection Keys
			{
				get
				{
					return Hash.Keys;
				}
			}

			#endregion
		}

		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Usage();
				return;
			}
			string fsPath = args[0];
			string topic = null;
			if (args.Length > 1)
				topic = args[1];
			LinkMaker lm = new LinkMaker("http://dummy");

			Federation fed = new Federation(fsPath, OutputFormat.HTML, lm);
			Cache cache = new Cache();
			fed.FederationCache = cache;
			if (topic != null)
				Print(fed, new AbsoluteTopicName(topic));
			else
			{
				int max = 10;
				foreach (AbsoluteTopicName top in fed.DefaultContentBase.AllTopics(false))
				{
					Print(fed, top);
					if (max-- <= 0)
						break;
				}
			}
		}

		static void Usage()
		{
			Console.Out.WriteLine("PrintTopic -- collect and format a FlexWiki topic");
			Console.Out.WriteLine("Usage: PrintTopic [path to federation config] {[abs topic name]}");
		}

		static void Print(Federation federation, AbsoluteTopicName topic)
		{
			bool diffs = false;

			string formattedBody = federation.GetTopicFormattedContent(topic, diffs);

			// Now calculate the borders
			string leftBorder = federation.GetTopicFormattedBorder(topic, Border.Left);
			string rightBorder =federation.GetTopicFormattedBorder(topic, Border.Right);
			string topBorder = federation.GetTopicFormattedBorder(topic, Border.Top);
			string bottomBorder = federation.GetTopicFormattedBorder(topic, Border.Bottom);

//			Console.Out.WriteLine(formattedBody);
		}
	}
}
