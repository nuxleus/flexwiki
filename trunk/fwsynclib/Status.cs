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

//                      local	file
//            ==============================
// remote	       Unchanged	       Changed     Not present
//=========== ---------------- --------------- -----------
//Unchanged       UpToDate     LocallyModified NoLocalFile
//Changed     LocallyOutOfDate    InConflict     Missing
//Not present	  LocallyAdded	   LocallyAdded      N/A


namespace FlexWiki.FwSyncLib
{
  [Flags]
  public enum Status
  {
    UpToDate          = 0x1,  // The file is identical with the latest revision in the repository.
    LocallyModified   = 0x2,  // You have edited the file, and not yet committed your changes.
    LocallyAdded      = 0x4,  // You have added the file with `add', and not yet committed your  changes.
    NoLocalFile       = 0x8,  // The local file does not exist - it needs to be retrieved from the repository
    LocallyOutOfDate  = 0x10, // Someone else has committed a newer revision to the repository.
    InConflict        = 0x20  // Someone else has committed a newer revision to the repository, and you have also made modifications to the file.
  }
}