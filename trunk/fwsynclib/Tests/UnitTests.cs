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
using System.IO; 

using NUnit.Framework; 

using FlexWiki.FwSyncLib; 

namespace FlexWiki.FwSyncLib.Tests
{
	public class UnitTests
	{
		public static MockEditServiceProxy GetMockServiceProxy()
		{
			MockEditServiceProxy proxy = new MockEditServiceProxy("http://server/vdir"); 
			
			DateTime now = DateTime.Now; 
			DateTime yesterday = DateTime.Now - TimeSpan.FromDays(1); 
			DateTime lastWeek = DateTime.Now - TimeSpan.FromDays(7); 

			proxy.Populate("A", "_ContentBaseDefinition", "Version1", "Namespace: A"); 
			proxy.Populate("A", "OneTopic", "AVersion1", "A-OneTopic-Version1"); 
			proxy.Populate("A", "TwoTopic", "AVersion1", "A-TwoTopic-Version2"); 
			proxy.Populate("A", "TwoTopic", "AVersion2", "A-TwoTopic-Version2"); 

			proxy.Populate("B", "_ContentBaseDefinition", "Version1", "Namespace: B"); 
			proxy.Populate("B", "OneTopic", "BVersion1", "B-OneTopic-Version1"); 
			proxy.Populate("B", "TwoTopic", "BVersion1", "B-TwoTopic-Version1"); 
			proxy.Populate("B", "TwoTopic", "BVersion2", "B-TwoTopic-Version2"); 

			return proxy; 

		}
		

		public static string CreateTopicFile(Synchronizer sync, string nsname, string topicname, 
			string content)
		{
			string dir = Path.Combine(sync.BaseDirectory, nsname); 

			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir); 
			}

			string path = Path.Combine(dir, topicname + ".wiki"); 
			FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write); 
			StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.ASCII); 
			sw.Write(content); 
			sw.Close();

			return path; 
		}

		public static void AppendToTopic(LocalTopic topic, string content)
		{
			FileStream fs = new FileStream(topic.Path, FileMode.Append, FileAccess.Write); 
			StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.ASCII); 
			sw.WriteLine(); 
			sw.WriteLine(content); 
			sw.Close(); 

		}

	}
}