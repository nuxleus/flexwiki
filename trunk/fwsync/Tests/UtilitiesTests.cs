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
using System.IO; 
using System.Text; 

using NUnit.Framework; 

using FlexWiki.FwSyncLib; 

namespace FlexWiki.FwSync.Tests
{
	[TestFixture]
	public class UtilitiesTests
	{
    [Test]
    public void ParseCommandLineCommit1()
    {
      string[] commandline = new string[] {"commit", "/attribution", "CraigAndera"}; 
      Options options = Utilities.ParseCommandLine(commandline);

      Assert.AreEqual(Commands.Commit, options.Command, "Checking that command was parsed as 'commit'"); 
      Assert.AreEqual("CraigAndera", options.Attribution, "Checking that identity was parsed as 'CraigAndera'"); 
      Assert.AreEqual(0, options.Files.Count, "Checking that correct number of files were parsed"); 
      Assert.IsFalse(options.IgnoreConflict, "Checking that /ignoreconflict was not picked up"); 
    }

    [Test]
    public void ParseCommandLineCommit2()
    {
      string[] commandline = new string[] {"commit", "/ignoreconflict", "/attribution", "CraigAndera", "somedir"}; 
      Options options = Utilities.ParseCommandLine(commandline);

      Assert.AreEqual(Commands.Commit, options.Command, "Checking that command was parsed as 'commit'"); 
      Assert.AreEqual("CraigAndera", options.Attribution, "Checking that attribution was parsed as 'CraigAndera'"); 
      Assert.AreEqual(1, options.Files.Count, "Checking that correct number of files were parsed"); 
      Assert.AreEqual("somedir", options.Files[0], "Checking that directory was parsed as 'somedir'"); 
      Assert.IsTrue(options.IgnoreConflict, "Checking that /ignoreconflict was picked up");
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "Unrecognized option on commit: /foo")]
    public void ParseCommandLineCommitError1()
    {
      string[] commandline = new string[] {"commit", "/foo"}; 
      Options options = Utilities.ParseCommandLine(commandline);
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "Didn't find /attribution switch, which is required")]
    public void ParseCommandLineCommitError2()
    {
      string[] commandline = new string[] {"commit"}; 
      Options options = Utilities.ParseCommandLine(commandline);
    }



    [Test]
    [ExpectedException(typeof(ParseCommandLineException))]
    public void ParseCommandLineErrors1()
    {
      // Make sure we barf on null command line
      Options options = Utilities.ParseCommandLine(null); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException))]
    public void ParseCommandLineErrors2()
    {
      // Make sure we barf on an empty command line
      string[] commandline = new string[0]; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException))]
    public void ParseCommandLineErrors3()
    {
      // Make sure we barf on an illegal option (/blah)
      string[] commandline = 
        new string[] { "/blah", "initialize", "http://localhost/foo" } ; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException))]
    public void ParseCommandLineErrors4()
    {
      // Make sure we barf on an illegal command (gork)
      string[] commandline = new string[] { "/debug", "gork", "http://localhost/foo" }; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/localonly may not be specified with the update command")]
    public void ParseCommandLineErrors5()
    {
      // Make sure we barf on localonly with update
      string[] commandline = new string[] { "/localonly", "update" }; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/localonly may not be specified with the commit command")]
    public void ParseCommandLineErrors6()
    {
      // Make sure we barf on localonly with commit
      string[] commandline = new string[] { "/localonly", "commit" }; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException),
       "No command was specified.")]
    public void ParseCommandLineErrors7()
    {
      string[] commandline = new string[] { "/localonly" }; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    public void ParseCommandLineHelp()
    {
      string[] commandline = new string[] {"help"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.General, options.HelpTopic, "Checking that help topic is general"); 
    }

    [Test]
    public void ParseCommandLineHelpCommands()
    {
      string[] commandline = new string[] {"help", "commands"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Commands, options.HelpTopic, "Checking that help topic is commands"); 
    }


    [Test]
    public void ParseCommandLineHelpCommit()
    {
      string[] commandline = new string[] {"help", "commit"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Commit, options.HelpTopic, "Checking that help topic is commit"); 
    }

    [Test]
    public void ParseCommandLineHelpHelp()
    {
      string[] commandline = new string[] {"help", "help"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Help, options.HelpTopic, "Checking that help topic is help"); 
    }

    [Test]
    public void ParseCommandLineHelpInit()
    {
      string[] commandline = new string[] {"help", "init"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Init, options.HelpTopic, "Checking that help topic is init"); 
    }

    [Test]
    public void ParseCommandLineHelpResolve()
    {
      string[] commandline = new string[] {"help", "resolve"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Resolve, options.HelpTopic, "Checking that help topic is resolve"); 
    }

    [Test]
    public void ParseCommandLineHelpStatus()
    {
      string[] commandline = new string[] {"help", "status"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Status, options.HelpTopic, "Checking that help topic is status"); 
    }

    [Test]
    public void ParseCommandLineHelpUpdate()
    {
      string[] commandline = new string[] {"help", "update"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Update, options.HelpTopic, "Checking that help topic is update"); 
    }

    [Test]
    public void ParseCommandLineHelpOptions()
    {
      string[] commandline = new string[] {"help", "options"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Options, options.HelpTopic, "Checking that help topic is options"); 
    }

    [Test]
    public void ParseCommandLineHelpEnvironment()
    {
      string[] commandline = new string[] {"help", "environment"};
      Options options = Utilities.ParseCommandLine(commandline); 
      Assert.AreEqual(Commands.Help, options.Command, "Checking that command is help");
      Assert.AreEqual(HelpTopics.Environment, options.HelpTopic, "Checking that help topic is environment"); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "Unknown help topic 'foobar'.")]
    public void ParseCommandLineHelpError1()
    {
      string[] commandline = new string[] {"help", "foobar"};
      Options options = Utilities.ParseCommandLine(commandline); 
    }


    [Test]
		public void ParseCommandLineInitialize1()
		{
			string[] commandline = new string[] {"/url", "http://localhost/foo", "Init", "somedir"};
			Options options = Utilities.ParseCommandLine(commandline); 
			AssertInitializeParsed(options, "http://localhost/foo/EditService.asmx", "somedir", false); 
		}

    [Test]
    public void ParseCommandLineInitialize2()
    {
      string[] commandline = new string[] {"/url", "http://localhost/foo", "Init"};
      Options options = Utilities.ParseCommandLine(commandline); 
      AssertInitializeParsed(options, "http://localhost/foo/EditService.asmx", ".", false, 0, false); 
    }

    [Test]
    public void ParseCommandLineInitialize3()
    {
      string[] commandline = new string[] {"/localonly", "/url", "http://localhost/foo", "Init"};
      Options options = Utilities.ParseCommandLine(commandline); 
      AssertInitializeParsed(options, "http://localhost/foo/EditService.asmx", ".", false, 0, true); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "Usage error: initialize requires a uri and a base directory")]
    public void ParseCommandLineInitializeError1()
    {
      string[] commandline = new string[] {"init",  "somedir", "SomeTopic"}; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "Usage error: initialize must be passed exactly one empty directory as an argument")]
    public void ParseCommandLineInitializeError2()
    {
      string[] commandline = new string[] {"/url", "http://localhost/foo", "init",  "somedir", "SomeTopic"}; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    public void ParseCommandLineStatus1()
    {
      string[] commandline = new string[] {"status"}; 
      Options options = Utilities.ParseCommandLine(commandline);

      Assert.AreEqual(Commands.Status, options.Command, "Checking that command was parsed as 'Status'"); 
      Assert.AreEqual(0, options.Files.Count, "Checking that directory was parsed correctly"); 
    }

    [Test]
    public void ParseCommandLineStatus2()
    {
      string[] commandline = new string[] {"status", "/show", "UONA", "somedir"}; 
      Options options = Utilities.ParseCommandLine(commandline);

      Assert.AreEqual(Commands.Status, options.Command, "Checking that command was parsed as 'Status'"); 
      Assert.AreEqual(1, options.Files.Count, "Checking that right number of files were parsed");
      Assert.AreEqual("somedir", options.Files[0], "Checking that directory was parsed as 'somedir'"); 
      Assert.IsFalse(options.LocalOnly, "Checking that localonly option was not picked up"); 
      Assert.AreEqual(Status.UpToDate | Status.LocallyOutOfDate | Status.NoLocalFile | Status.LocallyAdded, 
        options.Show, "Checking that /show switch was parsed correctly"); 
    }

    [Test]
    public void ParseCommandLineStatus3()
    {
      string[] commandline = new string[] {"/localonly", "status"}; 
      Options options = Utilities.ParseCommandLine(commandline);

      Assert.AreEqual(Commands.Status, options.Command, "Checking that command was parsed as 'Status'"); 
      Assert.IsTrue(options.LocalOnly, "Checking that localonly option was picked up"); 
    }
    [Test]
    public void ParseCommandLineStatus4()
    {
      string[] commandline = new string[] {"status", "somedir", "extrafile.wiki"}; 
      Options options = Utilities.ParseCommandLine(commandline);

      Assert.AreEqual(Commands.Status, options.Command, "Checking that command was parsed as 'Status'"); 
      Assert.AreEqual(2, options.Files.Count, "Checking that correct number of files were parsed"); 
      Assert.AreEqual("somedir", options.Files[0], "Checking that first directory was parsed as 'somedir'"); 
      Assert.AreEqual("extrafile.wiki", options.Files[1], "Checking that second file was parsed as 'extrafile.wiki'"); 

    }

    [Test]
      [ExpectedException(typeof(ParseCommandLineException), 
         "Unknown status code 'B'")]
    public void ParseCommandLineStatusError1()
    {
      string[] commandline = new string[] {"status", "/show",  "UONB", "somedir"}; 
      Options options = Utilities.ParseCommandLine(commandline);
    }


    [Test]
    public void ParseCommandLineResolve1()
    {
      string[] commandline = new string[] {"resolve", "/use", "winmerge.exe"}; 
      Options options = Utilities.ParseCommandLine(commandline);

      Assert.AreEqual(Commands.Resolve, options.Command, "Checking that command was parsed as 'resolve'"); 
      Assert.AreEqual(0, options.Files.Count, "Checking that correct number of files were parsed"); 
      Assert.AreEqual("winmerge.exe", options.ResolveWith, 
        "Checking that /use switch was picked up correctly"); 
    }

    [Test]
    public void ParseCommandLineResolve2()
    {
      string[] commandline = new string[] {"/y", "resolve", "foo"}; 
      Options options = Utilities.ParseCommandLine(commandline);

      Assert.AreEqual(Commands.Resolve, options.Command, "Checking that command was parsed as 'resolve'"); 
      Assert.IsTrue(options.ConfirmAll, "Checking that /y option was parsed correctly");
      Assert.AreEqual(1, options.Files.Count, "Checking that correct number of files were parsed"); 
      Assert.IsNull(options.ResolveWith, "Checking that /use switch was picked up correctly"); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "Unrecognized option on resolve: /foo")]
    public void ParseCommandLineResolveError1()
    {
      string[] commandline = new string[] {"/y", "resolve", "/foo", "foo"}; 
      Options options = Utilities.ParseCommandLine(commandline);
    }

    [Test]
    public void ParseCommandLineUpdate1()
    {
      string[] commandline = new string[] {"update"}; 
      Options options = Utilities.ParseCommandLine(commandline); 

      Assert.AreEqual(Commands.Update, options.Command, "Checking that command was parsed as 'update'"); 
      Assert.AreEqual(false, options.Debug, "Checking that debug was parsed as false"); 
      Assert.AreEqual(0, options.Files.Count, "Checking that no files were chosen"); 
    }

    [Test]
    public void ParseCommandLineUpdate2()
    {
      string[] commandline = new string[] {"/debug", "update"}; 
      Options options = Utilities.ParseCommandLine(commandline); 

      Assert.AreEqual(Commands.Update, options.Command, "Checking that command was parsed as 'update'"); 
      Assert.AreEqual(true, options.Debug, "Checking that debug was parsed as true"); 
    }

    [Test]
    public void ParseCommandLineUpdate3()
    {
      string[] commandline = new string[] {"/debug", "update", @"C:\temp\wiki\foo.wiki", @"C:\temp\wiki\bar.wiki"}; 
      Options options = Utilities.ParseCommandLine(commandline); 

      Assert.AreEqual(Commands.Update, options.Command, "Checking that command was parsed as 'update'"); 
      Assert.AreEqual(true, options.Debug, "Checking that debug was parsed as true"); 
      Assert.AreEqual(2, options.Files.Count, "Checking that correct number of files were parsed"); 
      Assert.AreEqual(@"C:\temp\wiki\foo.wiki", options.Files[0], "Checking that first file was parsed correctly"); 
      Assert.AreEqual(@"C:\temp\wiki\bar.wiki", options.Files[1], "Checking that second file was parsed correctly"); 
    }

    [Test]
    public void ParseCommandLineUpdate4()
    {
      string[] commandline = new string[] {"/debug", "update", @"C:\temp\wiki"}; 
      Options options = Utilities.ParseCommandLine(commandline); 

      Assert.AreEqual(Commands.Update, options.Command, "Checking that command was parsed as 'update'"); 
      Assert.AreEqual(true, options.Debug, "Checking that debug was parsed as true"); 
      Assert.AreEqual(1, options.Files.Count, "Checking that correct number of files were parsed"); 
      Assert.AreEqual(@"C:\temp\wiki", options.Files[0], "Checking that directory was parsed correctly"); 
    }
    
    [Test] 
    public void ParseDefCredSwitch1()
    {
      string[] commandline = new string[] {"/defcred", "update"}; 
      Options options = Utilities.ParseCommandLine(commandline); 
      AssertCredentialsParsed(options, true, null, null); 
    }

    [Test] 
    public void ParseDefCredSwitch2()
    {
      string[] commandline = new string[] {"update"}; 
      Options options = Utilities.ParseCommandLine(commandline); 
      AssertCredentialsParsed(options, false, null, null); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "No arguments were supplied")]  
    public void ParseEmptyCommandLine()
    {
      string[] commandline = new string[0]; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    public void ParseNoAuthSwitch()
    {
      string[] commandline = new string[] {"/noauth", "update"};
      Options options = Utilities.ParseCommandLine(commandline);
 
      Assert.IsFalse(options.UseDefaultCredentials, "Checking that default creditials are not allowed");
      Assert.IsNull(options.Credentials, "Checking that credentials are null");
    }

    [Test]
    public void ParseNoAuthSwitchOverridesDefcred()
    {
      string[] commandline = new string[] {"/noauth", "/defcred", "update"};
      Options options = Utilities.ParseCommandLine(commandline);
 
      AssertNoAuth(options); 
    }

    [Test]
    public void ParseNoAuthSwitchOverridesUser()
    {
      string[] commandline = new string[] {"/noauth", "/user", "candera", "/password", "foobar", "update"};
      Options options = Utilities.ParseCommandLine(commandline);
 
      AssertNoAuth(options); 
    }

    [Test]
    public void ParseUsernameSwitch1()
    {
      string[] commandline = new string[] {"/user", "candera", "update"};
      Options options = Utilities.ParseCommandLine(commandline); 
      AssertCredentialsParsed(options, false, "candera", ""); 
    }

    [Test] 
    public void ParseUsernameAndPasswordSwitch1()
    {
      string[] commandline = new string[] {"/user", "candera", "update"}; 
      GetPasswordCallback gpc = new GetPasswordCallback(GetPasswordCallback); 
      Options options = Utilities.ParseCommandLine(commandline, gpc); 
      AssertCredentialsParsed(options, false, "candera", "Enter password for user candera: " + "candera"); 
    }

    [Test] 
    public void ParseUsernameAndPasswordSwitch2()
    {
      string[] commandline = new string[] {"/user", "candera", "/password", "foo", "update"}; 
      GetPasswordCallback gpc = new GetPasswordCallback(GetPasswordCallback); 
      Options options = Utilities.ParseCommandLine(commandline, gpc); 
      AssertCredentialsParsed(options, false, "candera", "foo"); 
    }

    [Test] 
    public void ParseUsernameAndPasswordSwitch3()
    {
      string[] commandline = new string[] {"/password", "foo", "/user", "candera", "update"}; 
      GetPasswordCallback gpc = new GetPasswordCallback(GetPasswordCallback); 
      Options options = Utilities.ParseCommandLine(commandline, gpc); 
      AssertCredentialsParsed(options, false, "candera", "foo"); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
      "/user and /password switches are mutually exclusive with /defcred")]
    public void ParseUsernameAndDefCredMutEx1()
    {
      string[] commandline = new string[] {"/defcred", "/user", "candera", "update"}; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/user and /password switches are mutually exclusive with /defcred")]
    public void ParseUsernameAndDefCredMutEx2()
    {
      string[] commandline = new string[] {"/defcred", "/password", "candera", "update"}; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/user and /password switches are mutually exclusive with /defcred")]
    public void ParseUsernameAndDefCredMutEx3()
    {
      string[] commandline = new string[] {"/defcred", "/user", "candera", "/password", "foo", "update"}; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/user and /password switches are mutually exclusive with /defcred")]
    public void ParseUsernameAndDefCredMutEx4()
    {
      string[] commandline = new string[] {"/defcred", "/password", "candera", "/user", "candera", "update"}; 
      Options options = Utilities.ParseCommandLine(commandline); 
    }

  
    private void AssertCredentialsParsed(Options options, bool usedefault, string user, 
      string password)
    {
      Assert.AreEqual(usedefault, options.UseDefaultCredentials, "Checking that default credentials switch was parsed properly"); 

      if (user == null && password == null)
      {
        Assert.IsNull(options.Credentials, "Checking that credentials were null"); 
      }
      else
      {
        Assert.IsNotNull(options.Credentials, "Checking that credentials were not null"); 
        Assert.AreEqual(user, options.Credentials.UserName, "Checking that user was parsed properly"); 
        Assert.AreEqual(password, options.Credentials.Password, "Checking that password was parsed properly");
      }
    }

    private void AssertInitializeParsed(Options options, string uri, string dir, bool debug)
    {
      AssertInitializeParsed(options, uri, dir, debug, false); 
    }

    private void AssertInitializeParsed(Options options, string uri, string dir, bool debug, 
      bool localOnly)
    {
      AssertInitializeParsed(options, uri, dir, debug, 1, localOnly); 
    }

    private void AssertInitializeParsed(Options options, string uri, string dir, bool debug, 
      int numDirs, bool localOnly)
    {
      Assert.AreEqual(Commands.Initialize, options.Command, "Checking that command was parsed as 'Initialize'"); 
      Assert.AreEqual(uri, options.EditServiceUri, "Checking that the baseuri was parsed correctly"); 
      Assert.AreEqual(numDirs, options.Files.Count, "Checking that right number of directories were parsed"); 
      if (numDirs > 0)
      {
        Assert.AreEqual(dir, options.Files[0], "Checking that the directory was parsed correctly"); 
      }
      Assert.AreEqual(debug, options.Debug, "Checking that debug command line switch was parsed correctly"); 			
      Assert.AreEqual(localOnly, options.LocalOnly, "Checking that localonly command line switch was parsed correctly"); 
    }

    private void AssertNoAuth(Options options)
    {
      Assert.IsFalse(options.UseDefaultCredentials, "Checking that default creditials are not allowed");
      Assert.IsNull(options.Credentials, "Checking that credentials are null");
    }
    private string GetPasswordCallback(string prompt, string username)
    {
      return prompt + username; 
    }


  }
}
