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
using System.Xml; 
using System.IO; 
using System.Collections;
using System.Collections.Specialized; 
using System.Security.Cryptography; 
using System.Net; 
using System.Text; 

using FlexWiki.FwSyncLib; 

namespace FlexWiki.FwSync
{
	public class Utilities
	{
    public static Options ParseCommandLine(string[] args)
    {
      return ParseCommandLine(args, null); 
    }

    public static Options ParseCommandLine(string[] args, GetPasswordCallback gpc)
    {
      if (args == null || args.Length == 0)
      {
        throw new ParseCommandLineException("No arguments were supplied"); 
      }

      Options options = new Options(); 

      int i = ParseOptions(options, args, gpc); 

      if (i >= args.Length)
      {
        throw new ParseCommandLineException("No command was specified."); 
      }

      string command = args[i].ToUpper(); 

      if (command == "INIT")
      {
        options.Command = Commands.Initialize; 
        ++i; 

        ParseFiles(options, args, i); 

        if (options.EditServiceUri == null)
        {
          throw new ParseCommandLineException("Usage error: initialize requires a uri and a base directory"); 
        }

        if (options.Files.Count > 1)
        {
          throw new ParseCommandLineException("Usage error: initialize must be passed exactly one empty directory as an argument"); 
        }
 
      }
      else if (command == "UPDATE")
      {
        if (options.LocalOnly)
        {
          throw new ParseCommandLineException("/localonly may not be specified with the update command"); 
        }
        
        options.Command = Commands.Update; 
        ++i; 
        ParseFiles(options, args, i); 
      }
      else if (command == "STATUS")
      {
        options.Command = Commands.Status; 
        ++i; 
        i = ParseStatusOptions(options, args, i); 
        ParseFiles(options, args, i); 
      }
      else if (command == "COMMIT")
      {
        if (options.LocalOnly)
        {
          throw new ParseCommandLineException("/localonly may not be specified with the commit command"); 
        }

        options.Command = Commands.Commit; 
        ++i;
        i = ParseCommitOptions(options, args, i);
        ParseFiles(options, args, i); 
      }
      else if (command == "HELP")
      {
        options.Command = Commands.Help; 
        ++i; 
        i = ParseHelpOptions(options, args, i); 
      }
      else if (command == "RESOLVE")
      {
        options.Command = Commands.Resolve; 
        ++i; 
        i = ParseResolveOptions(options, args, i); 
        ParseFiles(options, args, i); 
      }
      else
      {
        throw new ParseCommandLineException("Unrecognized command " + command); 
      }

      return options; 

    }


    public static void PrintUsage()
    {
      Console.WriteLine(@"Run 'fwsync help' to get more detailed help.");
    }


    public static void PrintCommandsHelp()
    {
      Console.WriteLine(@"
Available Commands: 
  
  commit   - Push local changes to the remote wiki
  help     - Print help
  init     - Initialize the local wiki files
  update   - Retrieve updated content from the remote wiki
  status   - Print status of topics
  resolve  - Fix any conflicts between local and remote content

To get more detailed help for these commands, run fwsync help <command>. For
example, run 'fwsync help init' to get help on the init command.");
    }
    
    public static void PrintCommitHelp()
    {
      Console.WriteLine(@"
fwsync commit

This command uploads to a remote wiki any changes you have made to local
files. 

Usage: 

  fwsync commit /attribution <identity> [ /ignoreconflict ] [ <path> ... ] 

  /ignoreconflict   Skip topics that are InConflict (see fwsync help
                    status) rather than preventing the commit. 

  <identity>  A name or email address to which the changes being committed
              should be attributed. This will show up in the history
              section of the FlexWiki website.
  <path> ...  A list of one or more files or directories separated by
              spaces. Paths with spaces in them must be enclosed in double
              quotes. If no path is specified, the current directory is
              used. Specifying a directory is the same as specifying all
              topics in that directory. 

The commit command is the mechanism by which local changes are moved to the
FlexWiki website specified at init (see fwsync help init) time. Any
specified topics that have a status (see fwsync help status) of
LocallyModified or LocallyAdded will be submitted to the wiki, where they
will become the new version of that topic. 

By default, the commit command will refuse to commit *any* topics if at
least one of the specified topics is InConflict. However, if the
/ignoreconflict switch is specified, InConflict files will be ignored, and
LocallyModified and LocallyAdded files will be allowed to be committed. 

The /attribution switch is required, but is ignored when the /defcred or
/user (see fwsync help options) switches are specified. This may be
addressed in a future version of fwsync.");
    }

    public static void PrintEnvironmentHelp()
    {
      Console.WriteLine(@"
FwSync makes use of the following environment variables, if set: 

FWSMERGE     Set this environment variable to be the path to a tool to use
             when merging files with the resolve command (see fwsync help
             commands). The tool should accept a path to a copy of the
             remote file as the first argument, a path to the local file as
             the second argument, and a path to an output file where the
             results of the merge should be saved as the third
             argument. Standard merge tools such as WinMerge
             (http://winmerge.sourceforge.net) conform to this pattern, and
             should work with fwsync. This environment variable is
             overridden by the resolve command's /use switch, if specified.

FWSOPTIONS   When set, the contents of this environment variable are
             prepended to the command line of every invocation of
             fwsync. For example, if FWSOPTIONS is set to '/defcred', then
             invoking the command
             
                 fwsync update

             is equivalent to invoking 

                 fwsync /defcred update

             instead.");
    }

    public static void PrintGeneralHelp()
    {
      Console.WriteLine(@"
Usage: fwsync <globaloptions> <command> <commandoptions>

Run one of the following commands to get more detailed help:
  fwsync help commands     Help on available commands and their options
  fwsync help options      Help on available global options
  fwsync help environment  Help on environment variables that fwsync uses"); 
    }

    public static void PrintHelpHelp()
    {
 Console.WriteLine(@"
fwsync help

This command prints help for the fwsync program. 

Usage: 

  fwsync help [ <helpTopic> ]

  <helpTopic> The name of a topic on which you would like further help. 

Run without a help topic, this command prints general help for the fwsync
program, listing further available help topics. When run with a help topic,
prints help about that topic, if it is available. "); 
    }

    public static void PrintInitHelp()
    {
      Console.WriteLine(@"
fwsync init

This command performs the initial setup for an fwsync-synchronized wiki.

Usage: 

  fwsync [ /localonly ] /url <wiki url> init [ <directory> ]

  /localonly  Set up the wiki locally, but don't connect to the web site to
              get a list of topics. 

  <directory> A directory somewhere on your local disk. Relative or absolute
              paths may be used. If no directory is specified, the current
              directory is used. This directory is called the base
              directory.
  <wiki url>  A URL like http://www.flexwiki.com or
              http://www.myserver.com/wiki where a FlexWiki website lives. 

FlexWiki websites are organized into a series of namespaces. When running
the init command, the directory you specify is known as the 'base
directory'. Topics for each namespace are kept as individual .wiki files
within a subdirectory of the base directory with the same name as the
corresponding namespace.

The primary job of init is to create the file 'fwstatus.xml', which is kept
in the base directory. This file holds information about things like what
version of each topic is thought to be on the server, which helps figure
out the status of each file (see fwsync help status). 

Init requires the /url option - the specified URL will be stored and
remembered for use later by other commands acting in the same
directory. Init will connect to the server at the address specified by the
/url option to download information about the remote wiki unless the
/localonly option is specified. When /localonly is specified, fwsync merely
creates the fwstatus.xml file and populates it with information about any
.wiki files already present on the hard disk in subdirectories of the base
directory."); 
    }

    public static void PrintOptionsHelp()
    {
      Console.WriteLine(@"
These options may be specified with any command (except as noted): 
  /debug          Cause the program to break into a debugger after launching
  /defcred        Use the credentials of the currently logged-in user (the 
                  DEFault CREDentials) to authenticate to the web service. 
                  This option may not be specified with the /user nor with 
                  the /password option. 
  /user <name>    Specifiy the user with which to authenticate to the web 
                  service. If no /password is provided, the user will be 
                  prompted to provide one. This option may not be specified
                  with the /defcred option. 
  /password <pwd> Specify the password with which to authenticat to the web
                  service. If this option is not specified when the /user 
                  option is provided, the user will be prompted to provide
                  a password. This option may not be specified with the 
                  /defcred option. 
  /url <url>      Specify a URL for the FlexWiki instance you're synchronizing
                  with. This parameter is optional on all commands except
                  init, as it will be stored in the configuration file. 
  /localonly      Tells fwsync not to connect to the web instance of the wiki
                  when performing this command. Useful for getting status of
                  the local wiki when network activity is impossible or
                  undesired. Illegal with the commit and update commands. 
  /noauth         Do not authenticate. If this switch is present, the /defcred, 
                  /user, and /password switches are ignored. 
  /y              Do not prompt the user for confirmation - answer yes to every
                  question.");
    }

    public static void PrintResolveHelp()
    {
      Console.WriteLine(@"
fwsync resolve

This command allows the user to integrate changes made on the local wiki
with changes made on the remote wiki. 

Usage: 

  fwsync [ /y ] resolve [ /use <mergeprogram> ] [ <path> ... ]

  /y     Instruct fwsync to accept all merged changes. Has no effect when
         /use or FWSMERGE (see below) is set. 
  
  <mergeprogram> Specifies a program (such as WinMerge) to use to integrate
                 differences between local and remote files. This program
                 should follow the same convention as the FWSMERGE
                 environment variable (see fwsync help environment).   

Merges changes from the repository with the changes you have locally. By
default, the program will display both copies of the topic in notepad, and
you will have to hand-edit them to resolve changes. The program will ask you
to confirm each merge unless you specify the /y global option (see fwsync
help options). 

The /use switch (see below) or the FWMERGE environment variable (see fwsync
help environment) can be specified to tell fwsync to launch a merge program
instead to resolve the conflicts.

This command ignores files whose status is not InConflict (see fwsync help
status).");
    }

    public static void PrintStatusHelp()
    {
      Console.WriteLine(@"
fwsync status

This command displays the status of topics. 

Usage: 

   fwsync [ /localonly ] status [ /show { U|O|M|N|C|A } ] [ <path> ... ] 

   /localonly Prevents fwsync from connecting to the remote wiki to update
              version information. Prints status more quickly, but may not
              accurately reflect what is on the FlexWiki website. 
   /show      When present, specifies one or more status codes (see below)
              to limit display to. For example, /show U specifies that only
              UpToDate files should be shown. More than one status code may
              be specified.

   <path>  A list of one or more files, topics, or directories separated by
           spaces. Paths with spaces in them must be enclosed in double
           quotes. If no path is specified, the current directory is
           used. Specifying a directory is the same as specifying all topics
           in that directory.

The status command operates by comparing the local (filesystem) and remote
(repository) versions of a topic. By examining the difference between the
state of a topic now and the state when it was last updated (see fwsync
help update), fwsync can determine which of the following states the topic
is in: 

  U = UpToDate         The file is identical with the latest revision in the 
                       repository.
  M = LocallyModified  You have edited the file, and not yet committed your 
                       changes
  A = LocallyAdded     You have added the file to the local file system and 
                       not yet committed your changes.
  N = NoLocalFile      The local file does not exist - it needs to be 
                       retrieved from the repository via the update command.
  O = LocallyOutOfDate Someone else has committed a newer revision to the 
                       repository. You can retrieve the latest version with 
                       the update command.
  C = InConflict       Someone else has committed a newer revision to the
                       repository since your last update, and you have also
                       made modifications to the file. You will not be able
                       to commit this topic while it InConflict. See fwsync
                       help resolve command for instructions on how to deal
                       with conflicts."); 
		}

    public static void PrintUpdateHelp()
    {
      Console.WriteLine(@"
fwsync update

This command retrieves fresh copies of any topics that have changed on the
remote wiki. 

Usage: 

   fwsync update [ <path> ... ] 

   <path> ... A list of one or more files or directories separated by
              spaces. Paths with spaces in them must be enclosed in double
              quotes. If no path is specified, the current directory is
              used. Specifying a directory is the same as specifying all
              topics in that directory.

Retrieves any files that have been added or changed on the remote server,
unless they have also been modified on the local machine. If they have also
been modified on the local machine, the topic is in conflict (see fwsync
help status) and must be resolved (see fwsync help resolve). ");
    }
		
    
    private static int ParseCommitOptions(Options options, string[] args, int i)
    {
      for (; i < args.Length; ++i)
      {
        if (!(args[i].StartsWith("/") || args[i].StartsWith("-")))
        {
          break; 
        }

        string option = args[i].ToUpper().Substring(1); 
      
        if (option == "ATTRIBUTION")
        {
          options.Attribution = args[++i]; 
        }
        else if (option == "IGNORECONFLICT")
        {
          options.IgnoreConflict = true; 
        }
        else
        {
          throw new ParseCommandLineException("Unrecognized option on commit: " + args[i]); 
        }  
      }
      
      if (options.Attribution == null)
      {
        throw new ParseCommandLineException("Didn't find /attribution switch, which is required"); 
      }

      return i; 
    }
  
    private static void ParseFiles(Options options, string[] args, int i)
    {
      for (; i < args.Length; ++i)
      {
        options.Files.Add(args[i]); 
      }
    }

    private static int ParseHelpOptions(Options options, string[] args, int i)
    {
      if (args.Length <= i)
      {
        options.HelpTopic = HelpTopics.General; 
      }
      else
      {
        string helpTopic = args[i].ToUpper(); 

        if (helpTopic == "COMMANDS")
        {
          options.HelpTopic = HelpTopics.Commands; 
        }
        else if (helpTopic == "COMMIT")
        {
          options.HelpTopic = HelpTopics.Commit;
        }
        else if (helpTopic == "ENVIRONMENT")
        {
          options.HelpTopic = HelpTopics.Environment; 
        }
        else if (helpTopic == "HELP")
        {
          options.HelpTopic = HelpTopics.Help; 
        }
        else if (helpTopic == "INIT")
        {
          options.HelpTopic = HelpTopics.Init; 
        }
        else if (helpTopic == "OPTIONS")
        {
          options.HelpTopic = HelpTopics.Options; 
        }
        else if (helpTopic == "RESOLVE")
        {
          options.HelpTopic = HelpTopics.Resolve; 
        }
        else if (helpTopic == "STATUS")
        {
          options.HelpTopic = HelpTopics.Status; 
        }
        else if (helpTopic == "UPDATE")
        {
          options.HelpTopic = HelpTopics.Update; 
        }
        else 
        {
          string message = string.Format("Unknown help topic '{0}'.", args[i]);
          throw new ParseCommandLineException(message); 
        }
      }

      return i + 1; 
      
    }

    private static int ParseOptions(Options options, string[] args, GetPasswordCallback gpc)
    {
      bool passwordSpecified = false; 
      bool noauthSpecified = false; 
      int i = 0; 
      for (i = 0; i < args.Length; ++i)
      {
        if (!(args[i].StartsWith("/") || args[i].StartsWith("-")))
        {
          break; 
        }

        string option = args[i].ToUpper().Substring(1); 

        if (option == "DEBUG")
        {
          options.Debug = true; 
        }
        else if (option == "DEFCRED")
        {
          options.UseDefaultCredentials = true; 
        }
        else if (option == "USER")
        {
          if (options.Credentials == null)
          {
            options.Credentials = new NetworkCredential();
          }

          options.Credentials.UserName = args[++i]; 

        }
        else if (option == "PASSWORD")
        {
          if (options.Credentials == null)
          {
            options.Credentials = new NetworkCredential();
          }

          options.Credentials.Password = args[++i]; 
          passwordSpecified = true; 

        }
        else if (option == "URL")
        {
          options.EditServiceUri = args[++i]; 
          if (!options.EditServiceUri.EndsWith("/"))
          {
            options.EditServiceUri = options.EditServiceUri + "/"; 
          }
          options.EditServiceUri += "EditService.asmx"; 

        }
        else if (option == "LOCALONLY")
        {
          options.LocalOnly = true; 
        }
        else if (option == "Y")
        {
          options.ConfirmAll = true; 
        }
        else if (option == "NOAUTH")
        {
          noauthSpecified = true; 
        }
        else
        {
          throw new ParseCommandLineException("Unrecognized option " + args[i]); 
        }
      }

      if (options.Credentials != null && options.UseDefaultCredentials)
      {
        throw new ParseCommandLineException("/user and /password switches are mutually exclusive with /defcred"); 
      }

      if (options.Credentials != null)
      {
        if (gpc != null)
        {
          if (!passwordSpecified)
          {
            options.Credentials.Password = gpc("Enter password for user " + 
              options.Credentials.UserName + ": ", options.Credentials.UserName); 
          }
        }
      }

      if (noauthSpecified)
      {
        options.Credentials = null; 
        options.UseDefaultCredentials = false; 
      }

      return i; 
    }

    private static int ParseResolveOptions(Options options, string[] args, int i)
    {
      for (; i < args.Length; ++i)
      {
        if (!(args[i].StartsWith("/") || args[i].StartsWith("-")))
        {
          break; 
        }

        string option = args[i].ToUpper().Substring(1); 
      
        if (option == "USE")
        {
          options.ResolveWith = args[++i]; 
        }
        else
        {
          throw new ParseCommandLineException("Unrecognized option on resolve: " + args[i]); 
        }  
      }
      
      return i; 
    }

    private static int ParseStatusOptions(Options options, string[] args, int i)
    {
      for (; i < args.Length; ++i)
      {
        if (!(args[i].StartsWith("/") || args[i].StartsWith("-")))
        {
          break; 
        }

        string option = args[i].ToUpper().Substring(1); 
      
        if (option == "SHOW")
        {
          string types = args[++i].ToUpper(); 

          foreach (char type in types)
          {
            if (type == 'C')
            {
              options.Show |= Status.InConflict; 
            }
            else if (type == 'A')
            {
              options.Show |= Status.LocallyAdded; 
            }
            else if (type == 'M')
            {
              options.Show |= Status.LocallyModified; 
            }
            else if (type == 'O')
            {
              options.Show |= Status.LocallyOutOfDate; 
            }
            else if (type == 'N')
            {
              options.Show |= Status.NoLocalFile; 
            }
            else if (type == 'U')
            {
              options.Show |= Status.UpToDate; 
            }
            else 
            {
              string message = string.Format("Unknown status code '{0}'", type); 
              throw new ParseCommandLineException(message); 
            }
          }
        }
        else
        {
          throw new ParseCommandLineException("Unrecognized option on status: " + args[i]); 
        }  
      }
      
      return i; 

    }
    
	}
}

