<%@ Page language="c#" %>
<HTML>
  <HEAD>
    <TITLE>NAnt Results</TITLE>
  </HEAD>
  <div id="BodyArea" runat="server" />
</HTML>

<script runat="server" language="C#">
  private void Page_Load(object sender, EventArgs e)
  {
    string xslFilename = WebUtil.GetXslFilename("NAnt.xsl", Request);
    BodyArea.InnerHtml = new PageTransformer(WebUtil.ResolveLogFile(Context),xslFilename).LoadPageContent();
  }
</script>

