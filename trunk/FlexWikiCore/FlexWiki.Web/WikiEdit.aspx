<%@ Page language="c#" Codebehind="WikiEdit.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.WikiEdit" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>
			<%= TheTopic.ToString() %>
			(edit)</title>
		<meta name="Robots" content="NOINDEX, NOFOLLOW">
		<%= InsertStylesheetReferences() %>   
		<script  type=""text/jscript"" language="jscript">

function CalcEditBoxHeight()
{
	var answer = CalcEditZoneHeight();
	if (document.all("ButtonBar") != null)
		answer -= ButtonBar.clientHeight;
	return answer;
}

function CalcEditZoneHeight()
{
	var answer = MainHeight();
	if (PreviewArea.style.display == "block")
		answer -= PreviewArea.clientHeight;
	if (SearchArea.style.display == "block")
		answer -= SearchArea.clientHeight;
	return answer;
}

function ShowTip(tipid)
{
	var s = document.all(tipid);
	TipArea.innerHTML = s.innerHTML;
	TipArea.style.display = 'block';
}

function preview()
{
	searchOff();
	PreviewArea.style.display = 'block';
	
	var s = document.all("Text1").value;
	document.all("body").value = s;
	document.all("form2").submit();
}

function Save()
{
	var r = document.all("ReturnTopic");
	if (r != null)
		r.value = ""; // prevent return action by emptying this out
	document.all("Form1").submit();
}

function SaveAndReturn()
{
	document.all("Form1").submit();
}

function previewOff()
{
	PreviewArea.style.display = 'none';
}

function search()
{
	previewOff();
	SearchArea.style.display = 'block';
}

function searchOff()
{
	SearchArea.style.display = 'none';
}

function newSearch()
{
	SearchPane.location="search.aspx";
}

function MainHeight()
{
	var answer = document.body.clientHeight;
	var e;
	return answer;
}
			
function MainWidth()
{
	var answer = document.body.clientWidth;
	var e;
	
	e = document.getElementById("Sidebar");
	if (e != null)
		answer -= e.scrollWidth;
	return answer;
}


		</script>
		<style>

@media all
{
    tool\:tip   {
                behavior: url(tooltip_js.htc)
                }
}

.EditZone {
	background: #404040;
	overflow: hidden;
	height: expression(CalcEditZoneHeight());
	width: 100%;
}

.tip
{
    font-weight: bold;
}

.tipBody
{
	font: 8pt Verdana;
}

.TipArea
{
	margin: 3px;
	display: none;
	border: 1px solid #ffcc00;
	font: 8pt Verdana;
	padding: 4px;
}

.EditBox {
    font: 9pt Courier New;
    background: whitesmoke;
	height: expression(CalcEditBoxHeight());
	width: 100%;
}

.PreviewArea
{
	background: #404040;
	padding: 1px;
	height: 250px;
	width: expression(MainWidth());
}

.SearchArea
{
	background: #404040;
	padding: 1px;
	height: 250px;
	width: expression(MainWidth());
}

		</style>
	</HEAD>
	<% DoPage(); %>
</HTML>
