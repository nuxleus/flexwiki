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
using System.Collections.Generic; 
using System.Globalization; 

namespace FlexWiki
{
  /// <summary>
  /// A collection of useful routines
  /// </summary>
  public static class Utilities
  {
    public static bool CaseInsensitiveEquals(string a, string b)
    {
      if (a.Length != b.Length) 
      {
        return false;
      }

      return string.Compare(a, 0, b, 0, a.Length, true, CultureInfo.InvariantCulture) == 0; 
    }
    public static void CopyDictionary<TKey, TValue>(IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> destination)
    {
      foreach (KeyValuePair<TKey, TValue> pair in source)
      {
        destination.Add(pair.Key, pair.Value); 
      }
    }
    public static IDictionary<TKey, TValueReadOnly> MakeReadOnlyDictionary<TKey, TValue, TValueReadOnly>(IDictionary<TKey, TValue> input)
      where TValue : IProvideReadOnlyView<TValueReadOnly>
    {
      Dictionary<TKey, TValueReadOnly> readOnlyDictionary = new Dictionary<TKey, TValueReadOnly>();

      foreach (KeyValuePair<TKey, TValue> entry in input)
      {
        readOnlyDictionary.Add(entry.Key, entry.Value.AsReadOnly());
      }

      // Would be nice if Dictionary supported AsReadOnly, but it doesn't. 
      return readOnlyDictionary;
    }
    public static IList<U> MakeReadOnlyList<T, U>(IList<T> input) where T : IProvideReadOnlyView<U>
    {
      List<U> readOnlyList = new List<U>(); 
      
      foreach (T item in input)
      {
        readOnlyList.Add(item.AsReadOnly()); 
      }
      
      return readOnlyList.AsReadOnly();
    }

  }
}