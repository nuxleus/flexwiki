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

namespace FlexWiki.FwDocGen
{
  public class App
  {
    public static int Main(string[] args)
    {
      Console.WriteLine(Utilities.Copyright); 

      Options options = null; 
      try
      {
        options = Utilities.ParseCommandLine(args); 
      }
      catch (ParseCommandLineException e)
      {
        Console.WriteLine("Error: " + e.Message); 
        Console.WriteLine(); 
        Console.WriteLine(); 
        Console.WriteLine(Utilities.Usage); 
        return -1; 
      }

      if (options.Debug)
      {
        System.Diagnostics.Debugger.Break(); 
      }

      Generator.Run(options); 

      Console.WriteLine("Generation completed successfully");

      return 0; 

    }
  }
}