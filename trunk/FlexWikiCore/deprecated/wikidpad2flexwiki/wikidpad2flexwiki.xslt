<?xml version="1.0" encoding="UTF-8" ?>
<!--
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
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="text" encoding="utf-8" />
	
	<xsl:template match="text()">
		<xsl:value-of select="translate(., '&#13;', ' ')" />
	</xsl:template>
	
	<xsl:template match="wikiword">
		<xsl:apply-templates />
	</xsl:template>
	
	<xsl:template match="h1">
		<xsl:text>!</xsl:text>
		<xsl:apply-templates />
		<xsl:text>&#10;</xsl:text>
	</xsl:template>

	<xsl:template match="h2">
		<xsl:text>!!</xsl:text>
		<xsl:apply-templates />
		<xsl:text>&#10;</xsl:text>
	</xsl:template>

	<xsl:template match="h3">
		<xsl:text>!!!</xsl:text>
		<xsl:apply-templates />
		<xsl:text>&#10;</xsl:text>
	</xsl:template>
	
	<xsl:template match="li">
		<xsl:text>&#09;* </xsl:text>
		<xsl:apply-templates />
		<xsl:text>&#10;</xsl:text>
	</xsl:template>
	
	<xsl:template match="b">
		<xsl:text>*</xsl:text>
		<xsl:apply-templates />
		<xsl:text>*</xsl:text>
	</xsl:template>
	
</xsl:stylesheet>

  