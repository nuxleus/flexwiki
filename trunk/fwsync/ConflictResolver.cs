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
using System.Diagnostics;
using System.IO; 
using System.Text; 

using FlexWiki.FwSyncLib;

namespace FlexWiki.FwSync
{
  public class ConflictResolver
  {
    private string resolveWith; 
    private string fwsMerge; 
    private string tempDir; 
    private bool confirmAll; 

    public ConflictResolver(Options options)
    {
      this.resolveWith = options.ResolveWith; 
      this.fwsMerge = Environment.GetEnvironmentVariable("FWSMERGE"); 
      this.tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        @"fwsync\temp"); 
      this.confirmAll = options.ConfirmAll; 

      if (!Directory.Exists(tempDir))
      {
        Directory.CreateDirectory(tempDir); 
      }

      if (this.fwsMerge == null)
      {
        this.fwsMerge = ""; 
      }

      if (resolveWith == null)
      {
        this.resolveWith = ""; 
      }

      this.fwsMerge    = this.fwsMerge.Trim(); 
      this.resolveWith = this.resolveWith.Trim(); 

    }

    public string ResolveConflict(LocalTopic topic, string remoteContents)
    {
      string result = null; 
      if (resolveWith.Length > 0)
      {
        result = NormalMerge(topic, remoteContents, resolveWith); 
      }
      else if (fwsMerge.Length > 0)
      {
        result = NormalMerge(topic, remoteContents, fwsMerge); 
      }
      else 
      {
        result = DefaultResolve(topic, remoteContents);
      }
      
      return result; 
    }

    private string DefaultResolve(LocalTopic topic, string remoteContents)
    {
      string tempPath = Path.Combine(tempDir, topic.Name + ".wiki"); 

      StreamWriter sw = new StreamWriter(tempPath, false, Encoding.UTF8); 
      try
      {
        sw.WriteLine("Both versions of the topic {0}.{1} appear below", topic.Namespace.Name, topic.Name); 
        sw.WriteLine("The remote version appears first, then the local version.");
        sw.WriteLine();
        sw.WriteLine("Edit the file to merge the changes together. Be sure to remove these remarks, ");
        sw.WriteLine("the ===='s separating the sections, and any anything you don't want to appear ");
        sw.WriteLine("in the final copy. Then save the file and confirm the changes"); 
        sw.WriteLine();
        sw.WriteLine("==REMOTE VERSION============================");
        sw.WriteLine(FixupLineTerminators(remoteContents)); 
        sw.WriteLine();
        sw.WriteLine("==LOCAL VERSION============================");
        sw.WriteLine(FixupLineTerminators(topic.ReadContents())); 
        sw.WriteLine(); 
      }
      finally
      {
        sw.Close(); 
      }

      Process process = Process.Start("notepad.exe", tempPath); 
      process.WaitForExit(); 

      bool accept = false; 

      string result = null;   
      if (confirmAll)
      {
        accept = true; 
      }
      else
      {
        Console.Write("Accept changes (y/n)? ");
        char c = (char) Console.Read(); 

        if (c == 'y' || c == 'Y')
        {
          accept = true; 
        }
      }

      if (accept)
      {
        StreamReader sr = new StreamReader(tempPath, Encoding.Default, true); 
        try
        {
          result = sr.ReadToEnd(); 
        }
        finally
        {
          sr.Close(); 
        }
      }

      File.Delete(tempPath); 

      return result; 

    }


    private string FixupLineTerminators(string input)
    {
      StringReader sr = new StringReader(input); 
      StringWriter sw = new StringWriter(); 

      string line = null; 
      while ((line = sr.ReadLine()) != null)
      {
        sw.WriteLine(line); 
      }

      return sw.ToString(); 
    }

    private string NormalMerge(LocalTopic topic, string remoteContents, string mergeWith)
    {
      string localContentPath  = Path.Combine(tempDir, string.Format("{0}.local.wiki", topic.Name)); 
      string remoteContentPath = Path.Combine(tempDir, string.Format("{0}.remote.wiki", topic.Name)); 
      string mergedContentPath = Path.Combine(tempDir, string.Format("{0}.merged.wiki", topic.Name)); 

      StreamWriter sw = new StreamWriter(localContentPath, false, Encoding.UTF8); 
      try
      {
        sw.Write(topic.ReadContents());
      }
      finally
      {
        sw.Close(); 
      }

      sw = new StreamWriter(remoteContentPath, false, Encoding.UTF8); 
      try
      {
        sw.Write(remoteContents); 
      }
      finally
      {
        sw.Close(); 
      }

      Process process = Process.Start(mergeWith, string.Format("\"{0}\" \"{1}\" \"{2}\"", remoteContentPath, localContentPath, 
        mergedContentPath)); 
      process.WaitForExit(); 

      string result = null; 
      if (File.Exists(mergedContentPath))
      {
        StreamReader sr = new StreamReader(mergedContentPath, Encoding.Default, true); 
        try
        {
          result = sr.ReadToEnd(); 
        }
        finally
        {
          sr.Close(); 
        }
      }

      File.Delete(localContentPath);
      File.Delete(remoteContentPath); 
      File.Delete(mergedContentPath); 

      return result; 

    }
  }
}