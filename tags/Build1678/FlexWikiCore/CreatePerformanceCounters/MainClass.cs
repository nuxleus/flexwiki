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
using FlexWiki;

namespace CreatePerfCounters
{
	/// <summary>
	/// Summary description for Main.
	/// </summary>
	class MainClass
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				if (args[0] == "-d")
				{
					Federation.DeletePerformanceCounters();
					Console.Out.WriteLine("FlexWiki performance counters deleted.");
					return;
				}					

				Usage();
			}
			else
			{
				if (Federation.CreatePerformanceCounters())
					Console.Out.WriteLine("FlexWiki performance counters created.");
				else
					Console.Out.WriteLine("FlexWiki performance counters already exist.");
			}
		}

		static void Usage()
		{
			Console.Error.WriteLine("Run with no arguments to create counters.  Run with '-d' to delete counters.");
		}
	}
}
