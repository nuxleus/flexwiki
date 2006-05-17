<%@ Page language="c#" Codebehind="LastModified.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.LastModified" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<head>
		<title>Last Modified</title>
		<meta name="Robots" content="NOINDEX, NOFOLLOW">
		<%= InsertStylesheetReferences() %>
<script  type="text/javascript" language="javascript">
function filter() {
	var author = AuthorFilter.options[AuthorFilter.selectedIndex].text;
	var table = document.getElementById('MainTable');
	for (var i = 0; i < table.rows.length; i++)	{
		var row = table.rows.item(i);
		var authorcell = row.cells.item(2);
		var show = true;
		if (author != '[All]' && authorcell.innerHTML != author) {	show = false;	}
		if (show) {
			try  {
			 row.style.display = 'table-row';
			} catch (e) {
			  row.style.display = 'block';
			}
		}
		else
		{
			row.style.display = 'none';
		}
	}
}


function changeNamespace()
{
	var ns = NamespaceFilter.options[NamespaceFilter.selectedIndex].text;
	var newURL = 'LastModified.aspx?namespace=' + ns;
	window.location = newURL;	
}
</script>
</head>
	<body class='Dialog'>
	<fieldset>
	<legend class='DialogTitle'>Recent Changes</legend>
	<p>Namespace: <%=NamespaceFilter()%></p>
		<% DoSearch(); %>
	</fieldset>
	</body>
</HTML>
