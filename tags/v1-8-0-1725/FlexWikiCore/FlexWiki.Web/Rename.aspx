<%@ Page language="c#" Codebehind="Rename.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Rename" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>Rename Topic</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="Robots" content="NOINDEX, NOFOLLOW">
		<%= InsertStylesheetReferences() %>
	</HEAD>
	<body>
	<table width='100%' height='100%' border='0' cellpadding='7' cellspacing='0' ><tr>
<%	DoLeftBorder(); %>
	<td valign='top'><fieldset>
<%
	if (Request.HttpMethod == "POST")
	{
		Response.Write("<legend class='DialogTitle'>Rename <b>" + FlexWiki.Web.HTMLWriter.Escape(OldName)  + "</b></legend>");
		PerformRename();
	}
	else
	{
			// See if the topic is read only
			if (IsTopicReadOnly)
			{
				Response.Write("<legend class='DialogTitle'><b>Topic (" + AbsTopicName + ") can non be changed.</b></legend>");
			}
			else
			{
%>
				<legend class='DialogTitle'>
				Rename <b>
					<%= FlexWiki.Web.HTMLWriter.Escape(AbsTopicName.Name)  %>
				</b>
				</legend>
			<p><b>&nbsp;Important guidelines</b> (rename is not always straightforward!):
				<ul>
					<li>
						When you rename a topic, you may be able to ask (depending on how this site is configured) to have references to the topic 
						automatically updated.
						<ul>
							<li>
							References from other namespaces are not updated
							<li>
								Plural references are not found</li>
						</ul>
					<li>
					Because not all references can be reliably found (e.g., because a topic has 
					been bookmarked by someone, or is referenced from another namespace), a page will be 
					automatically generated that tells people where the new page is.
					<li>
						Often when you rename a topic you are changing its meaning. As a result, you 
						might want to change the text surrounding the references to the topic; please 
						consider reviewing the current references when you are done renaming to be sure 
						they still make sense.</li>
				</ul>
				<hr noshade size='1'>
				<form id="RenameForm" method="post" ACTION="Rename.aspx">
					<input style='DISPLAY: none' type="text"  name="oldName" value ="<%= FlexWiki.Web.HTMLWriter.Escape(AbsTopicName.Name)  %>">
					<b>&nbsp;Old</b> name:
					<%= FlexWiki.Web.HTMLWriter.Escape(AbsTopicName.Name)  %>
					<br>
					<b>&nbsp;New</b> name: <input style='FONT-SIZE: x-small' type="text"  name="newName" value ="<%= FlexWiki.Web.HTMLWriter.Escape(AbsTopicName.Name)  %>">
					<p>
					<%
					{
						bool fixupDisabled = false; 
						try
						{
							fixupDisabled = bool.Parse(System.Configuration.ConfigurationSettings.AppSettings["DisableRenameFixup"]); 
						}
						catch
						{
						}
						if (!fixupDisabled)
						{					
					%>
						<input type="checkbox" id="fixup" name="fixup"><label for="fixup">Automatically update references</label>
					<%
						}
						else
						{
					%>
						<p><i>Automatically updating references from other topics has been disabled on this site</i></p>
					<%
						}
					}
					%>
						<div style='DISPLAY: none'>
							<br>
							<input type="checkbox" checked id="leaveRedirect" name="leaveRedirect"><label for="leaveRedirect">Generate 
								a redirect page under the old name</label>
						</div>
					<P></P>
					<p>
						<input style='DISPLAY: none' type="text"  name="namespace" value ="<%= FlexWiki.Web.HTMLWriter.Escape(AbsTopicName.Namespace)  %>">
						<input type='submit' ID="SaveButton" Value="Rename">
					</p>
				</form>
				<%
			}
	}
	%>
	</fieldset></td>
<%  DoRightBorder(); %>
	</tr></table>
	</body>
</HTML>
