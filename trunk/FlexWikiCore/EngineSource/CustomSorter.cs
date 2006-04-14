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
    internal class CustomSorter : IComparer
    {
        #region IComparer Members

        Block block;
        ExecutionContext ctx;

        public CustomSorter(Block b, ExecutionContext c)
        {
            block = b;
            ctx = c;
        }

        public int Compare(object x, object y)
        {
            ArrayList a1 = new ArrayList();
            a1.Add(x);
            IBELObject xKey = block.Value(ctx, a1);

            ArrayList a2 = new ArrayList();
            a2.Add(y);
            IBELObject yKey = block.Value(ctx, a2);

            IComparable xc = xKey as IComparable;
            if (xc == null)
                throw new ExecutionException(null, "Can not compare objects of type " + BELType.ExternalTypeNameForType(xKey.GetType()));
            IComparable yc = yKey as IComparable;
            if (yc == null)
                throw new ExecutionException(null, "Can not compare objects of type " + BELType.ExternalTypeNameForType(yKey.GetType()));

            int a = xc.CompareTo(yc);
            return a;
        }

        #endregion
    }

}
