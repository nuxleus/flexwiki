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
