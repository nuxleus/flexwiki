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
  public class Options
  {
    #region Fields
    private string docDir; 
    private string docNamespace; 
    private string commentsDir; 
    private string commentsNamespace; 
    private string inputPath; 
    private string templateDir; 
    private string generator; 
    private bool writeTemplatesOnly; 
    private bool verbose; 
    private bool debug; 
    #endregion Fields

    #region Properties
    public bool Debug
    {
      get { return debug; }
      set { debug = value; }
    }

    public string DocDir
    {
      get { return docDir; }
      set { docDir = value; }
    }

    public string DocNamespace
    {
      get { return docNamespace; }
      set { docNamespace = value; }
    }

    public string CommentsDir
    {
      get { return commentsDir; }
      set { commentsDir = value; }
    }

    public string CommentsNamespace
    {
      get { return commentsNamespace; }
      set { commentsNamespace = value; }
    }

    public string InputPath
    {
      get { return inputPath; }
      set { inputPath = value; }
    }

    public string TemplateDir
    {
      get { return templateDir; }
      set { templateDir = value; }
    }

    public string Generator
    {
      get { return generator; }
      set { generator = value; }
    }

    public bool Verbose
    {
      get { return verbose; }
      set { verbose = value; }
    }

    public bool WriteTemplatesOnly
    {
      get { return writeTemplatesOnly; }
      set { writeTemplatesOnly = value; }
    }
    #endregion Properties
    
  }
}