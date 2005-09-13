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
using System.Web.Caching;
using System.Collections;
using System.Text;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for FilesCacheRule.
	/// </summary>
	public class FilesCacheRule : CacheRule
	{
		public FilesCacheRule()
		{
		}

		public FilesCacheRule(string path)
		{
			_Files.Add(path);
		}

		public FilesCacheRule(IEnumerable list)
		{
			foreach (string path in list)
			{
				_Files.Add(path);
			}
		}

		ArrayList _Files = new ArrayList();

		public IList Files
		{
			get
			{
				return _Files;
			}
		}

		public override ICollection AllLeafRules
		{
			get
			{
				return ArrayList.Repeat(this, 1);
			}
		}

		public void AddFile(string path)
		{
			_Files.Add(path);
		}

		public override CacheDependency GetCacheDependency(CacheDependency inner)
		{
			return new CacheDependency((string[])(_Files.ToArray(typeof(string))), null, inner);
		}

		public override string Description 
		{
			get
			{
				StringBuilder answer = new StringBuilder();
				foreach (string path in _Files)
				{
					if (answer.Length > 0)
						answer.Append(", ");
					answer.Append(path);
				}
				return answer.ToString();
			}
		}


	}
}
