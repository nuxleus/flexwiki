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
			if (Federation.SetupPerformanceCounterCategory())
				Console.Out.WriteLine("FlexWiki performance counters created.");
			else
				Console.Out.WriteLine("FlexWiki performance counters already exist.");
		}
	}
}
