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
	public class Set : IEnumerable
	{
		Hashtable hash = new Hashtable();

		public void Add(object obj)
		{
			if (hash.ContainsKey(obj))
				return;
			hash[obj] = null;
		}

		public void AddRange(ICollection c)
		{
			foreach (object obj in c)
				Add(obj);
		}

		public bool Contains(object obj)
		{
			return hash.ContainsKey(obj);
		}

		public int Count
		{
			get
			{
				return hash.Count;
			}
		}

		public IEnumerator GetEnumerator()
		{
			return hash.Keys.GetEnumerator();
		}

		public void Remove(object obj)
		{
			hash.Remove(obj);
		}

		public object First
		{
			get
			{
				foreach (object obj in hash.Keys)
					return obj;
				return null;
			}
		}
	}
}