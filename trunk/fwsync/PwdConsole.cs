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

// This code taken from http://madtechnology.net/blog/archive/2003/12/11/306.aspx

using System; 
using System.Text; 
using System.Runtime.InteropServices; 

public class PwdConsole 
{
  [DllImport("kernel32", SetLastError=true)]
  static extern IntPtr GetStdHandle(IntPtr whichHandle);
  [DllImport("kernel32", SetLastError=true)]
  static extern bool GetConsoleMode(IntPtr handle, out uint mode);
  [DllImport("kernel32", SetLastError=true)]
  static extern bool SetConsoleMode(IntPtr handle, uint mode);

  static readonly IntPtr STD_INPUT_HANDLE = new IntPtr(-10);
  const int ENABLE_LINE_INPUT = 2;
  const uint ENABLE_ECHO_INPUT = 4;

  public static string ReadLine() 
  {
    // turn off console echo
    IntPtr hConsole = GetStdHandle(STD_INPUT_HANDLE);
    uint oldMode;
    if (!GetConsoleMode(hConsole, out oldMode)) 
    {
      throw new ApplicationException("GetConsoleMode failed");
    }
    uint newMode = oldMode & ~(ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT);
    if (!SetConsoleMode(hConsole, newMode)) 
    {
      throw new ApplicationException("SetConsoleMode failed");
    }
    int i;
    StringBuilder secret = new StringBuilder();
    while (true) 
    {
      i = Console.Read ();
      if (i == 13)     // break when 
        break;
      secret.Append((char) i);
      Console.Write ("*");
    }

    Console.WriteLine();
    // restore console echo and line input mode
    if (!SetConsoleMode(hConsole, oldMode)) 
    {
      throw new ApplicationException("SetConsoleMode failed");
    }
    return secret.ToString();
  }
}
