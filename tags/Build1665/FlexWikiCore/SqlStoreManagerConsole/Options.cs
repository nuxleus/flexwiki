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

namespace FlexWiki.SqlStoreManagerConsole
{
  public class Options
  {
    private Commands _command;
    private string _database; 
    private string _datadir; 
    private string _instance;
    private string _user; 

    public Commands Command
    {
      get { return _command; }
      set { _command = value; }
    }

    public string Database
    {
      get { return _database; }
      set { _database = value; }
    }

    public string DataDirectory
    {
      get { return _datadir; }
      set { _datadir = value; }
    }

    public string Instance
    {
      get { return _instance; }
      set { _instance = value; }
    }

    public string User
    {
      get { return _user; }
      set { _user = value; }
    }
  }
}