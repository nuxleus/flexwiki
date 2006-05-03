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

            PrintTopicApplication application = new PrintTopicApplication(fsPath, lm); 
			Federation fed = new Federation(application);
			if (topic != null)
				Print(fed, new TopicName(topic));
			else
			{
				int max = 10;
				foreach (TopicName top in fed.DefaultNamespaceManager.AllTopics(ImportPolicy.DoNotIncludeImports))
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

		static void Print(Federation federation, TopicName topic)
		{
			string formattedBody = federation.GetTopicFormattedContent(new QualifiedTopicRevision(topic), null);

			// Now calculate the borders
			string leftBorder = federation.GetTopicFormattedBorder(topic, Border.Left);
			string rightBorder =federation.GetTopicFormattedBorder(topic, Border.Right);
			string topBorder = federation.GetTopicFormattedBorder(topic, Border.Top);
			string bottomBorder = federation.GetTopicFormattedBorder(topic, Border.Bottom);

//			Console.Out.WriteLine(formattedBody);
		}
	}
}
