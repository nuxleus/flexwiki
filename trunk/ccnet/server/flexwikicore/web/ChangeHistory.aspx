<%@ Page language="c#" %>
<%@ Assembly Name="ThoughtWorks.CruiseControl.Core" %> 
<%@ Import Namespace="ThoughtWorks.CruiseControl.Web" %> 
<%@ Import Namespace="ThoughtWorks.CruiseControl.Core" %> 
<%@ Import Namespace="System.IO" %> 
<%@ Import Namespace="System.Xml" %> 
<%@ Import Namespace="System.Xml.XPath" %> 
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 

<script runat="server" language="C#">
private void Page_Load(object sender, System.EventArgs e)
	{
    try
    {
        if (Request.QueryString["log"] != null)
        {
          Response.Redirect("default.aspx?log=" + Request.QueryString["log"], true); 
        }
        string path = WebUtil.GetLogDirectory(Context).FullName;
        string[] logfiles = LogFileUtil.GetLogFileNames(path); 
        Array.Sort(logfiles); 
        Array.Reverse(logfiles); 

        System.Text.StringBuilder builder = new System.Text.StringBuilder(); 

        builder.Append("<div class='modifications-sectionheader'>Pending a successful build</div>");

        bool isEven = true; 
        foreach (string logfile in logfiles)
        {
            if (LogFileUtil.IsSuccessful(logfile))
            {
                string number = LogFileUtil.ParseBuildNumber(logfile); 
                builder.AppendFormat("<br/><div class='modifications-sectionheader'>Build {0}</div>", 
                  number); 
                isEven = true; 
            }

            XPathDocument doc = new XPathDocument(Path.Combine(path, logfile)); 
            XPathNavigator navigator = doc.CreateNavigator(); 
            XPathNodeIterator iterator = 
 
navigator.Select("/cruisecontrol/modifications/modification");

            Hashtable modifications = new Hashtable(); 
  
            while (iterator.MoveNext())
            {
                string user = (string)
iterator.Current.Evaluate("string(user)");
                string comment = (string) iterator.Current.Evaluate("string(comment)");

                modifications[comment] = user; 
            }

            foreach (string comment in modifications.Keys)
            {
                builder.AppendFormat("<div class='modifications-{0}row'><pre>{1}</pre></div>", 
                  isEven ? "even" : "odd", comment);
                //isEven = !isEven; 
            }
          
        }
        
        BodyLabel.Text = builder.ToString(); 
    }
    catch(CruiseControlException ex)
    {
        Response.Write(ex.Message);
    }
	}
</script>

<html>
  <head>
    <title>ChangeHistory</title>
    <meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
    <meta name="CODE_LANGUAGE" Content="C#">
    <meta name=vs_defaultClientScript content="JavaScript">
    <meta name=vs_targetSchema
content="http://schemas.microsoft.com/intellisense/ie5">
  </head>
  <body MS_POSITIONING="GridLayout">
	  <h1>Change History</h1>
   <asp:Label id="BodyLabel" runat="server" Width="432px" Height="400px"
/>
	
  </body>
</html>
