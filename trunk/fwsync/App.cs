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
using System.Collections.Specialized; 
using System.IO; 
using System.Net; 

using FlexWiki.FwSyncLib;

namespace FlexWiki.FwSync
{
	public class App
	{
		private const int SUCCESS = 0; 
		private const int ERROR_USAGE = 1; 
		private const int ERROR_NOT_IMPLEMENTED = 2; 
    private const int ERROR_EXCEPTION = 3; 
    private const int ERROR_CONFLICT = 4; 

		public static int Main(string[] args)
		{
      try
      {
        PrintCopyright(); 

        string envOptions = Environment.GetEnvironmentVariable("FWSOPTIONS");
        
        if (envOptions != null)
        {
          envOptions = envOptions.Trim(); 

          if (envOptions.Length > 0)
          {
            string[] additionalArgs = envOptions.Split(' '); 

            ArrayList al = new ArrayList();
            al.AddRange(additionalArgs);
            al.AddRange(args); 

            args = (string[]) al.ToArray(typeof(string)); 

            Console.WriteLine("Running with modified command line:");
            Console.WriteLine("fwsync {0}", string.Join(" ", args)); 
          }
        }

        Options options = null; 
        GetPasswordCallback gpc = new GetPasswordCallback(GetPassword); 
        try	
        {
          options = Utilities.ParseCommandLine(args, gpc); 
        }
        catch (ParseCommandLineException pcle)
        {
          Console.WriteLine(pcle.Message); 
          Utilities.PrintUsage(); 
          return ERROR_USAGE; 
        }

        if (options.Debug)
        {
          System.Diagnostics.Debugger.Break(); 
        }

        FwEditServiceProxy proxy = CreateEditServiceProxy(options); 
        string basedir = Synchronizer.CalculateBaseDir(options.Files); 
        Synchronizer sync = new Synchronizer(basedir, proxy); 
        sync.Progress += new ProgressCallback(PrintProgress);
        
        if (options.Command == Commands.Initialize)
        {
          sync.Initialize(options.LocalOnly); 
        }
        else if (options.Command == Commands.Update)
        {
          sync.Update(options.Files); 
        }
        else if (options.Command == Commands.Status)
        {
          sync.SyncToLocal(); 

          if (!options.LocalOnly)
          {
            sync.SyncToRemote(); 
          }

          PrintStatus(sync, options.Files, options.Show); 
        }
        else if (options.Command == Commands.Commit)
        {
          try
          {
            sync.Commit(options.Attribution, options.Files, options.IgnoreConflict); 
          }
          catch (ConflictException c)
          {
            Console.WriteLine(c.Message); 
            return ERROR_CONFLICT; 
          }
        }
        else if (options.Command == Commands.Help)
        {
          switch (options.HelpTopic)
          {
            case HelpTopics.Commands: 
              Utilities.PrintCommandsHelp();
              break;
            case HelpTopics.Commit:
              Utilities.PrintCommitHelp(); 
              break; 
            case HelpTopics.Environment:
              Utilities.PrintEnvironmentHelp();
              break;
            case HelpTopics.General:
              Utilities.PrintGeneralHelp(); 
              break;
            case HelpTopics.Help:
              Utilities.PrintHelpHelp(); 
              break; 
            case HelpTopics.Init: 
              Utilities.PrintInitHelp(); 
              break; 
            case HelpTopics.Options:
              Utilities.PrintOptionsHelp();
              break;
            case HelpTopics.Status:
              Utilities.PrintStatusHelp();
              break;
            case HelpTopics.Resolve:
              Utilities.PrintResolveHelp(); 
              break; 
            case HelpTopics.Update:
              Utilities.PrintUpdateHelp();
              break; 
            default:
              Utilities.PrintGeneralHelp();
              break;
          }
        }
        else if (options.Command == Commands.Resolve)
        {
          ConflictResolver cr = new ConflictResolver(options); 
          ResolveConflictCallback rccb = new ResolveConflictCallback(cr.ResolveConflict); 
          sync.Resolve(rccb, options.Files); 
        }
        else
        {
          Console.WriteLine("Command {0} not currently implemented", options.Command); 
          return ERROR_NOT_IMPLEMENTED; 
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("An unhandled exception was thrown.\n\n{0}", e); 
        return ERROR_EXCEPTION; 
      }

			return SUCCESS; 
			
		}

    
    private static void PrintStatus(Synchronizer sync, StringCollection files, Status show)
    {
      if (show == (Status) 0)
      {
        show = Status.InConflict | Status.LocallyAdded | Status.LocallyModified | Status.LocallyOutOfDate | 
          Status.NoLocalFile | Status.UpToDate; 
      }

      LocalTopicList topics = sync.GetTopicsForFiles(files); 
      foreach (LocalTopic topic in topics)
      {
        if ((topic.Status & show) != 0)
        {
          Console.WriteLine("{0,-20} : {1}.{2}", topic.Status, topic.Namespace.Name, topic.Name); 
        }
      }
    }

    private static FwEditServiceProxy CreateEditServiceProxy(Options options)
    {
      FwEditServiceProxy proxy = new FwEditServiceProxy(); 

      if (options.EditServiceUri != null)
      {
        proxy.Uri = options.EditServiceUri; 
      }

      // This prevents the proxy from reauthenticating on every single
      // request. Since authentication adds two roundtrips for /defcred
      // authentication, this can nearly triple the speed of some operations
      proxy.UnsafeAuthenticatedConnectionSharing = true; 

      if (options.UseDefaultCredentials)
      {
        proxy.Credentials = CredentialCache.DefaultCredentials; 
      }
      else if (options.Credentials != null)
      {
        proxy.Credentials = options.Credentials; 
      }

      return proxy; 
    }

    private static string GetPassword(string prompt, string username)
    {
      Console.Write(prompt); 
      return PwdConsole.ReadLine(); 
    }

    private static void PrintCopyright()
    {
      Console.WriteLine("fwsync " + 
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + 
        " Copyright (c) 2004 Craig Andera candera@wangdera.com"); 
    }

    private static void PrintProgress(object sender, ProgressEventArgs args)
    {
      if (args.EventType == EventType.Committed)
      {
        Console.WriteLine("Committed {0}.{1}: was {2}, is now {3}", 
          args.Topic.Namespace.Name, args.Topic.Name, args.OldStatus, args.NewStatus); 
      }
      else if (args.EventType == EventType.UpdatedLocal)
      {
        Console.WriteLine("Updated {0}.{1}: was {2}, is now {3}", 
          args.Topic.Namespace.Name, args.Topic.Name, args.OldStatus, args.NewStatus); 
      }
      else if (args.EventType == EventType.ConflictSkipped)
      {
        Console.WriteLine("Skipping {0}.{1} because it is InConflict", 
          args.Topic.Namespace.Name, args.Topic.Name); 
      }
      else
      {
        Console.Write("."); 
      }
    }
  }
}
