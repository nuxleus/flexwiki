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
using System.Text;
using FlexWiki.Formatting;


namespace FlexWiki
{
  class DefaultSorter : IComparer
  {
    #region IComparer Members

    public DefaultSorter()
    {
    }

    public int Compare(object x, object y)
    {
      IComparable	xc = x as IComparable;
      if (xc == null)
        throw new ExecutionException(null, "Can not compare objects of type " + BELType.ExternalTypeNameForType(x.GetType()));
      IComparable	yc = y as IComparable;
      if (yc == null)
        throw new ExecutionException(null, "Can not compare objects of type " + BELType.ExternalTypeNameForType(y.GetType()));

      return xc.CompareTo(yc);
    }

    #endregion
  }
}
