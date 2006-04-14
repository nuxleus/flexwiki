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

using NUnit.Framework;
using FlexWiki.Formatting;
using System.Collections;

namespace FlexWiki.UnitTests
{
  class Dummy : BELObject
  {
    static int _NeverCounter = 0;

    [ExposedMethod(ExposedMethodFlags.Default, "")]
    public int NumberNever
    {
      get
      {
        return _NeverCounter++;
      }
    }

    static int _ForeverCounter = 10;
    [ExposedMethod(ExposedMethodFlags.Default, "")]
    public int NumberForever
    {
      get
      {
        return _ForeverCounter++;
      }
    }

    public override IOutputSequence ToOutputSequence()
    {
      return null;
    }

    [ExposedMethod(ExposedMethodFlags.AllowsVariableArguments, "")]
    public int ArgCounterZero(ExecutionContext ctx)
    {
      return ctx.TopFrame.ExtraArguments.Count;
    }

    [ExposedMethod(ExposedMethodFlags.AllowsVariableArguments, "")]
    public string ExtractExtraArg(ExecutionContext ctx, int which)
    {
      return (string)(ctx.TopFrame.ExtraArguments[which]);
    }

  }

}
