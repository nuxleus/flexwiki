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

namespace FlexWiki.SqlProvider
{
	/// <summary>
	/// This class contains information about topics stored in Sql similar 
	/// to FileInfo class for topics stored in files.
	/// </summary>
	public class SqlInfoForTopic
	{
		private string name;
		private DateTime lastWriteTime;

		public SqlInfoForTopic(string name, DateTime lastWriteTime)
		{
			this.name = name;
			this.lastWriteTime = lastWriteTime;
		}

		public override string ToString()
		{
			return Name;
		}


		public string Name
		{
			get
			{
				return name;
			}
		}

		public DateTime LastWriteTime
		{
			get
			{
				return lastWriteTime;
			}
		}


	}
}
