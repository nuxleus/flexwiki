// Creates a new IIS virtual directory
//
// Usage: 
// cscript.exe makevdirs.js machine vdir path [ parent ] 
// 
//   machine : the name of the machine on which to create the
//             virtual directory (e.g. isengard)
//   vdir    : the name of the new virtual directory to create
//             (e.g. FlexWiki)
//   path    : the path to the directory where the vdir will
//             be rooted (e.g. C:\inetpub\wwwroot\flexwiki)
//   parent  : Optional. The name of the parent of the new 
//             virtual directory (e.g. /somevdir). Defaults 
//             to the default root directory. 
 
     
// This function actually creates the virtual directory
// with the specified properties. Note that execution of 
// the script doesn't start here, but begins below. 
function CreateVDir(parent, name, path)
{
  // Delete the existing vDir if present
  try
  {
    parent.Delete("IIsWebVirtualDir", name); 
  }
  catch (ex) {} 
  
  var vdir = parent.Create("IIsWebVirtualDir", name); 
  
  vdir.Put("Path", path); 
  vdir.AccessRead = true; 
  vdir.AccessScript = true; 
  vdir.AuthFlags = 5;       // Anonymous and integrated auth
  vdir.AppCreate2(0);       // Create an in-proc application for this vdir
  vdir.DefaultDoc = "default.aspx"; 
  vdir.SetInfo(); 
  
  return vdir; 
}


// Script execution begins here

// Read in the mandatory command line arguments
var machinename = WScript.Arguments(0); 
var vdirname = WScript.Arguments(1); 
var fileroot = WScript.Arguments(2); 

// If the parent argument was specified, read it in
var vdirparent = null; 
if (WScript.Arguments.Length > 3)
{
  var vdirparentname = WScript.Arguments(3); 
  vdirparent = GetObject("IIS://" + machinename + "/W3SVC/1/Root" + vdirparentname); 
}
// Otherwise, default to the default root vdir
else
{
  vdirparent = GetObject("IIS://" + machinename + "/W3SVC/1/Root"); 
}

// Call the function above to actually create the virtual directory
CreateVDir(vdirparent, vdirname, fileroot); 
