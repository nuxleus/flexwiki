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

<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:fwdg="http://www.flexwiki.com/schames/fwdocgen/xslthelper.xsd">
  
  <xsl:param name="verbosity">0</xsl:param>
  <xsl:param name="commentNamespace" />
  <xsl:param name="docNamespace" />
  
  <!-- 
    ** default template rule: don't do anything
    -->

  <xsl:template match="@*|node()|text()" priority="-1">
  </xsl:template>

  <!-- 
    ** HomePage template
    -->

  <xsl:template match="/">
    <xsl:if test="number($verbosity) > 2">
      <xsl:message>Generating HomePage</xsl:message>
    </xsl:if>

    <fwdg:fileset>
      <fwdg:file path="HomePage.wiki">
        <xsl:text>!! Namespaces </xsl:text>
        <xsl:text>&#0010;</xsl:text>
        <xsl:text>||*Namespace*||</xsl:text>
        <xsl:text>&#0010;</xsl:text>
        <xsl:for-each select="//namespace">
          <xsl:text>||</xsl:text>
          <xsl:apply-templates mode="topicLink" select="." />
          <xsl:text>||</xsl:text>
          <xsl:text>&#0010;</xsl:text>
        </xsl:for-each>
      </fwdg:file>

      <xsl:if test="number($verbosity) > 2">
        <xsl:message>Done generating HomePage</xsl:message>
      </xsl:if>

      <xsl:apply-templates mode="topLevel" />
    </fwdg:fileset>
  </xsl:template>

  <!-- 
    ** topLevel templates
    -->
  
  <xsl:template match="ndoc|assembly|module" mode="topLevel">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering top-level ndoc|assembly|module template</xsl:message>
    </xsl:if>

    <xsl:if test="number($verbosity) > 2">
      <xsl:message>
        <xsl:text>Matched </xsl:text>
        <xsl:value-of select="local-name(.)" />
        <xsl:text> at top level</xsl:text>
      </xsl:message>
    </xsl:if>

    <xsl:apply-templates mode="topLevel" />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  top-level ndoc|assembly|module template</xsl:message>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="namespace" mode="topLevel">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering top-level namespace template</xsl:message>
    </xsl:if>

    <xsl:if test="number($verbosity) > 2">
      <xsl:message>
        <xsl:text>Generating </xsl:text>
        <xsl:apply-templates mode="topicFileName" select="." />
        <xsl:text>:</xsl:text>
        <xsl:apply-templates mode="itemFullName" select="."/>
      </xsl:message>
    </xsl:if>

    <fwdg:file>
      <xsl:attribute name="path">
        <xsl:apply-templates mode="topicFileName" select="." />
      </xsl:attribute>

      <xsl:apply-templates mode="titleSection" select="." />
      <xsl:call-template name="makeSectionTables" />
      <xsl:call-template name="makeCommentsSection" />

    </fwdg:file>
    <xsl:apply-templates mode="topLevel" />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  top-level namespace template</xsl:message>
    </xsl:if>

  </xsl:template>
  
  <xsl:template match="class|structure|interface|enumeration" mode="topLevel">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering top-level class|structure|interface|enumeration template</xsl:message>
    </xsl:if>
    
    <xsl:if test="number($verbosity) > 2">
      <xsl:message>
        <xsl:text>Generating </xsl:text>
        <xsl:apply-templates mode="topicFileName" select="." />
        <xsl:text>:</xsl:text>
        <xsl:apply-templates mode="itemFullName" />
      </xsl:message>
    </xsl:if>

    <fwdg:file>
      <xsl:attribute name="path">
        <xsl:apply-templates mode="topicFileName" select="." />
      </xsl:attribute>

      <xsl:apply-templates mode="titleSection" select="." />
      <xsl:apply-templates mode="itemSignatureSection" select="." />
      <xsl:apply-templates mode="summarySection" select="." />
      <xsl:call-template name="makeSectionTables" />
      <xsl:call-template name="makeCommentsSection" />

    </fwdg:file>
    <xsl:apply-templates mode="topLevel" />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  top-level class|structure|interface template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="constructor|method|operator" mode="topLevel">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering top-level constructor|method|operator template</xsl:message>
    </xsl:if>
    
    <fwdg:file>
      <xsl:attribute name="path">
        <xsl:apply-templates mode="topicFileName" select="." />
      </xsl:attribute>
  
      <xsl:if test="number($verbosity) > 3">
        <xsl:message>
          <xsl:text>Generating </xsl:text>
          <xsl:apply-templates mode="topicFileName" select="." />
          <xsl:text>:</xsl:text>
          <xsl:apply-templates mode="itemFullName" />
        </xsl:message>
      </xsl:if>

      <xsl:apply-templates mode="titleSection" select="." />
      <xsl:apply-templates mode="itemSignatureSection" select="." />
      <xsl:apply-templates mode="summarySection" select="." />
      <xsl:call-template name="makeCommentsSection" />
 
    </fwdg:file>
    
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  top-level constructor|method|operator template</xsl:message>
    </xsl:if>
    
  </xsl:template>

  <xsl:template match="event|field|property" mode="topLevel">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering top-level event|field|property template</xsl:message>
    </xsl:if>
    
    <fwdg:file>
      <xsl:attribute name="path">
        <xsl:apply-templates mode="topicFileName" select="." />
      </xsl:attribute>
  
      <xsl:if test="number($verbosity) > 3">
        <xsl:message>
          <xsl:text>Generating </xsl:text>
          <xsl:apply-templates mode="topicFileName" select="." />
          <xsl:text>:</xsl:text>
          <xsl:apply-templates mode="itemFullName" />
        </xsl:message>
      </xsl:if>

      <xsl:apply-templates mode="titleSection" select="." />
      <xsl:apply-templates mode="itemSignatureSection" select="." />
      <xsl:apply-templates mode="summarySection" select="." />
      <xsl:call-template name="makeCommentsSection" />
 
    </fwdg:file>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  top-level event|field|property template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="@*|node()|text()" mode="topLevel">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering top-level default template</xsl:message>
    </xsl:if>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  top-level default template</xsl:message>
    </xsl:if>

  </xsl:template>

  <!-- 
    ** titleSection templates 
    -->

  <xsl:template match="namespace|class|interface|structure|enumeration|constructor|method|field|property|operator|event" 
    mode="titleSection">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering titleSection template</xsl:message>
    </xsl:if>

    <xsl:text>! </xsl:text> 
    <xsl:apply-templates mode="itemType" select="." />
    <xsl:text>: </xsl:text>
    <xsl:apply-templates mode="itemFullName" select="." />
    <xsl:text>&#0010;</xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  titleSection template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="@*|node()|text()" mode="titleSection">
  </xsl:template>
  
  <!-- 
    ** itemSignatureSection templates 
    -->

  <xsl:template match="constructor|method" mode="itemSignatureSection">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering constructor|method itemSignatureSection template</xsl:message>
    </xsl:if>

    <xsl:text>&#0010;</xsl:text>
    <xsl:text>!! Signature</xsl:text>
    <xsl:text>&#0010;</xsl:text>
    <xsl:apply-templates mode="visibilityKeyword" select="." />
    <xsl:text> </xsl:text>
    <xsl:apply-templates mode="contractKeyword" select="." />
    <xsl:text> </xsl:text>
    <xsl:if test="@returnType">
      <xsl:call-template name="translateType">
        <xsl:with-param name="fullName" select="@returnType" />
        <xsl:with-param name="escapeDots">true</xsl:with-param>
      </xsl:call-template>
      <xsl:text> </xsl:text>
    </xsl:if>
    <xsl:apply-templates mode="itemName" select="." />
    <xsl:apply-templates mode="signature" select="."/>
    <xsl:text>&#0010;</xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting constructor|method itemSignatureSection template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="property" mode="itemSignatureSection">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering property itemSignatureSection template</xsl:message>
    </xsl:if>

    <xsl:text>&#0010;</xsl:text>
    <xsl:text>!! Signature</xsl:text>
    <xsl:text>&#0010;</xsl:text>
    <xsl:apply-templates mode="visibilityKeyword" select="." />
    <xsl:text> </xsl:text>
    <xsl:apply-templates mode="contractKeyword" select="." />
    <xsl:text> </xsl:text>
    <xsl:call-template name="translateType">
      <xsl:with-param name="fullName" select="@type" />
      <xsl:with-param name="escapeDots">true</xsl:with-param>
    </xsl:call-template>
    <xsl:text> </xsl:text>
    <xsl:apply-templates mode="itemName" select="." />
    <xsl:apply-templates mode="signature" select="."/>
    <xsl:text>&#0010;</xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting property itemSignatureSection template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="event" mode="itemSignatureSection">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering property itemSignatureSection template</xsl:message>
    </xsl:if>

    <xsl:text>&#0010;</xsl:text>
    <xsl:text>!! Signature</xsl:text>
    <xsl:text>&#0010;</xsl:text>
    <xsl:apply-templates mode="visibilityKeyword" select="." />
    <xsl:text> </xsl:text>
    <xsl:apply-templates mode="contractKeyword" select="." />
    <xsl:text> event </xsl:text>
    <xsl:call-template name="translateType">
      <xsl:with-param name="fullName" select="@type" />
      <xsl:with-param name="escapeDots">true</xsl:with-param>
    </xsl:call-template>
    <xsl:text> </xsl:text>
    <xsl:apply-templates mode="itemName" select="." />
    <xsl:text>&#0010;</xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting event itemSignatureSection template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="field" mode="itemSignatureSection">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering field itemSignatureSection template</xsl:message>
    </xsl:if>

    <xsl:text>&#0010;</xsl:text>
    <xsl:text>!! Signature</xsl:text>
    <xsl:text>&#0010;</xsl:text>
    <xsl:apply-templates mode="visibilityKeyword" select="." />
    <xsl:text> </xsl:text>
    <xsl:call-template name="translateType">
      <xsl:with-param name="fullName" select="@type" />
      <xsl:with-param name="escapeDots">true</xsl:with-param>
    </xsl:call-template>
    <xsl:text> </xsl:text>
    <xsl:apply-templates mode="itemName" select="." />
    <xsl:text>&#0010;</xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting field itemSignatureSection template</xsl:message>
    </xsl:if>

  </xsl:template>


  <xsl:template match="class|interface|structure|enumeration" mode="itemSignatureSection">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering class|interface|structure|enumeration itemSignatureSection template</xsl:message>
    </xsl:if>

    <xsl:text>&#0010;</xsl:text>
    <xsl:text>!! Declaration</xsl:text>
    <xsl:text>&#0010;</xsl:text>
    <xsl:apply-templates mode="signature" select="."/>
    <xsl:text>&#0010;</xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting class|interface|structure|enumeration itemSignatureSection template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="@*|node()|text()" mode="itemSignatureSection">
  </xsl:template>

  <!-- 
    ** section templates 
    -->

  <xsl:template name="makeSectionTable">
    <xsl:param name="type" />
    <xsl:param name="typePlural" select="concat($type, 's')"/>
    <xsl:param name="nodes" />
    <xsl:param name="showInheritedColumn">false</xsl:param>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering makeSectionTable template</xsl:message>
    </xsl:if>

    <xsl:if test="$nodes">
      <xsl:text>&#0010;</xsl:text>
      <xsl:text>!! </xsl:text>
      <xsl:value-of select="$typePlural" />
      <xsl:text>&#0010;</xsl:text>

      <xsl:text>||*</xsl:text>
      <xsl:value-of select="$type" />
      <xsl:text>*</xsl:text>
      <xsl:if test="$showInheritedColumn = 'true'">
        <xsl:text>||*Inherited From*</xsl:text>
      </xsl:if>
      <xsl:text>||*Summary*||</xsl:text>
      <xsl:text>&#0010;</xsl:text>
      <xsl:apply-templates mode="summaryRow" select="$nodes">
        <xsl:with-param name="showInheritedColumn">
          <xsl:value-of select="$showInheritedColumn" />
        </xsl:with-param>
        <xsl:sort select="@declaringType" />
        <xsl:sort select="@name" />
      </xsl:apply-templates>
      <xsl:text>&#0010;</xsl:text>
    </xsl:if>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  makeSectionTable template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template name="makeSectionTables">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering makeSectionTables template</xsl:message>
    </xsl:if>
    
    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Class</xsl:with-param>
       <xsl:with-param name="typePlural">Classes</xsl:with-param>
       <xsl:with-param name="nodes" select="class" />
       <xsl:with-param name="showInheritedColumn">false</xsl:with-param>
    </xsl:call-template>

    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Interface</xsl:with-param>
       <xsl:with-param name="typePlural">Interfaces</xsl:with-param>
       <xsl:with-param name="nodes" select="interface" />
       <xsl:with-param name="showInheritedColumn">false</xsl:with-param>
    </xsl:call-template>
    
    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Structure</xsl:with-param>
       <xsl:with-param name="typePlural">Structures</xsl:with-param>
       <xsl:with-param name="nodes" select="structure" />
       <xsl:with-param name="showInheritedColumn">false</xsl:with-param>
    </xsl:call-template>

    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Enumeration</xsl:with-param>
       <xsl:with-param name="typePlural">Enumerations</xsl:with-param>
       <xsl:with-param name="nodes" select="enumeration" />
       <xsl:with-param name="showInheritedColumn">false</xsl:with-param>
    </xsl:call-template>
   
    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Field</xsl:with-param>
       <xsl:with-param name="typePlural">Fields</xsl:with-param>
       <xsl:with-param name="nodes" select="field" />
       <xsl:with-param name="showInheritedColumn">true</xsl:with-param>
    </xsl:call-template>
    
    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Constructor</xsl:with-param>
       <xsl:with-param name="typePlural">Constructors</xsl:with-param>
       <xsl:with-param name="nodes" select="constructor" />
       <xsl:with-param name="showInheritedColumn">true</xsl:with-param>
    </xsl:call-template>

    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Property</xsl:with-param>
       <xsl:with-param name="typePlural">Properties</xsl:with-param>
       <xsl:with-param name="nodes" select="property" />
       <xsl:with-param name="showInheritedColumn">true</xsl:with-param>
    </xsl:call-template>

    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Event</xsl:with-param>
       <xsl:with-param name="typePlural">Events</xsl:with-param>
       <xsl:with-param name="nodes" select="event" />
       <xsl:with-param name="showInheritedColumn">true</xsl:with-param>
    </xsl:call-template>
    
    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Method</xsl:with-param>
       <xsl:with-param name="typePlural">Methods</xsl:with-param>
       <xsl:with-param name="nodes" select="method" />
       <xsl:with-param name="showInheritedColumn">true</xsl:with-param>
    </xsl:call-template>
           
    <xsl:call-template name="makeSectionTable">
       <xsl:with-param name="type">Operator</xsl:with-param>
       <xsl:with-param name="typePlural">Operators</xsl:with-param>
       <xsl:with-param name="nodes" select="operator" />
       <xsl:with-param name="showInheritedColumn">true</xsl:with-param>
    </xsl:call-template>
   
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  classesSection template</xsl:message>
    </xsl:if>

  </xsl:template>


  <!-- 
    ** itemName templates 
    -->

  <xsl:template match="namespace|class|interface|structure|enumeration|method|event|property|operator|field" mode="itemName">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering non-constructor itemName template</xsl:message>
    </xsl:if>

    <xsl:value-of select="@name" />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  non-constructor itemName template</xsl:message>
    </xsl:if>
  </xsl:template>

  <xsl:template match="constructor" mode="itemName">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering constructor itemName template</xsl:message>
    </xsl:if>

    <xsl:value-of select="../@name" />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  constructor itemName template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="@*|node()|text()" mode="itemName">
    <xsl:value-of select="concat('UnknownItem', local-name(.))" />
  </xsl:template>
  
  <!-- 
    ** itemFullName templates 
    -->

  <xsl:template match="namespace|class|interface|structure|enumeration|event|property|operator|field" 
    mode="itemFullName">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering namespace|class|interface|structure|enumeration|event|property|operator|field itemFullName template</xsl:message>
    </xsl:if>

    <xsl:apply-templates mode="itemFullNameNoSignature" select="." />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  namespace|class|interface|structure|enumeration|event|property|operator|field itemFullName template</xsl:message>
    </xsl:if>
  </xsl:template>

  <xsl:template match="constructor|method" mode="itemFullName">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering constructor|method itemFullName template</xsl:message>
    </xsl:if>

    <xsl:apply-templates mode="itemFullNameNoSignature" select="." />
    <xsl:apply-templates mode="signature" select="." />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  constructor|method itemFullName template</xsl:message>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="@*|node()|text()" mode="itemFullName">
    <xsl:value-of select="concat('UnknownItem', local-name(.))" />
  </xsl:template>

  <!-- 
    ** itemFullNameNoSignature templates 
    -->

  <xsl:template match="namespace" mode="itemFullNameNoSignature">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering namespace itemFullNameNoSignature template</xsl:message>
    </xsl:if>

    <xsl:apply-templates mode="itemName" select="." />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  namespace itemFullNameNoSignature template</xsl:message>
    </xsl:if>
  </xsl:template>

  <xsl:template match="constructor|method" mode="itemFullNameNoSignature">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering constructor|method itemFullNameNoSignature template</xsl:message>
    </xsl:if>

    <xsl:apply-templates mode="typeName" select="." />
    <xsl:text>.</xsl:text>
    <xsl:apply-templates mode="itemName" select="." />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  constructor|method itemFullNameNoSignature template</xsl:message>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="class|interface|structure|enumeration|event|property|operator|field" mode="itemFullNameNoSignature">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering class|interface|structure|event|property|operator|field itemFullNameNoSignature template</xsl:message>
    </xsl:if>

    <xsl:apply-templates mode="typeName" select="." />
    <xsl:text>.</xsl:text>
    <xsl:apply-templates mode="itemName" select="." />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  class|interface|structure|event|property|operator|field itemFullNameNoSignature template</xsl:message>
    </xsl:if>
  </xsl:template> 
  
  <xsl:template match="@*|node()|text()" mode="itemFullNameNoSignature">
    <xsl:value-of select="concat('UnknownItem', local-name(.))" />
  </xsl:template>

  
  <!-- 
    ** typeName templates 
    -->
    
  <xsl:template match="class|interface|structure|enumeration" mode="typeName">
    <xsl:param name="parent">
      <xsl:apply-templates mode="typeName" select=".." />
    </xsl:param>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering class|interface|structure typeName template</xsl:message>
    </xsl:if>
    
    <xsl:if test="string-length($parent) > 0">
      <xsl:value-of select="$parent" />
      <xsl:text>.</xsl:text>  
    </xsl:if>
    
    <xsl:value-of select="@name" />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  class|interface|structure typeName template</xsl:message>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="constructor|method|event|property|operator|field" mode="typeName">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>
        <xsl:text>Entering constructor|method|event|property|operator|field typeName template on </xsl:text>
        <xsl:value-of select="local-name(.)" />
        <xsl:apply-templates mode="itemName" select="." />
      </xsl:message>
    </xsl:if>

    <xsl:apply-templates mode="typeName" select=".." />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>
        <xsl:text>Exiting constructor|method|event|property|operator|field typeName template on </xsl:text>
        <xsl:value-of select="local-name(.)" />
        <xsl:apply-templates mode="itemName" select="." />
      </xsl:message>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="/" mode="typeName">
  </xsl:template>
  
  <xsl:template match="*|node()|@*" mode="typeName">
  </xsl:template>
  
  <!-- 
    ** itemType templates 
    -->
  
  <xsl:template match="namespace" mode="itemType">
    <xsl:text>Namespace</xsl:text>
  </xsl:template>
 
  <xsl:template match="class" mode="itemType">
    <xsl:text>Class</xsl:text>
  </xsl:template>
  
  <xsl:template match="interface" mode="itemType">
    <xsl:text>Interface</xsl:text>
  </xsl:template>
  
  <xsl:template match="structure" mode="itemType">
    <xsl:text>Structure</xsl:text>
  </xsl:template>

  <xsl:template match="enumeration" mode="itemType">
    <xsl:text>Enumeration</xsl:text>
  </xsl:template>

  <xsl:template match="constructor" mode="itemType">
    <xsl:text>Constructor</xsl:text>
  </xsl:template>
  
  <xsl:template match="field" mode="itemType">
    <xsl:text>Field</xsl:text>
  </xsl:template>
  
  <xsl:template match="property" mode="itemType">
    <xsl:text>Property</xsl:text>
  </xsl:template>
  
  <xsl:template match="method" mode="itemType">
    <xsl:text>Method</xsl:text>
  </xsl:template>
  
  <xsl:template match="event" mode="itemType">
    <xsl:text>Event</xsl:text>
  </xsl:template>
  
  <xsl:template match="operator" mode="itemType">
    <xsl:text>Operator</xsl:text>
  </xsl:template>  

  <xsl:template match="*|node()|@*" mode="itemType">
    <xsl:value-of select="concat('UnknownItem', local-name(.))" />
  </xsl:template>

  <!-- 
    ** itemTypeKeyword templates 
    -->
  
  <xsl:template match="namespace" mode="itemTypeKeyword">
    <xsl:text>namespace</xsl:text>
  </xsl:template>
 
  <xsl:template match="class" mode="itemTypeKeyword">
    <xsl:text>class</xsl:text>
  </xsl:template>
  
  <xsl:template match="interface" mode="itemTypeKeyword">
    <xsl:text>interface</xsl:text>
  </xsl:template>
  
  <xsl:template match="structure" mode="itemTypeKeyword">
    <xsl:text>struct</xsl:text>
  </xsl:template>

  <xsl:template match="enumeration" mode="itemTypeKeyword">
    <xsl:text>enum</xsl:text>
  </xsl:template>

  <!-- 
    ** itemTypePlural templates 
    -->

  <xsl:template match="namespace" mode="itemTypePlural">
    <xsl:text>Namespaces</xsl:text>
  </xsl:template>
 
  <xsl:template match="class" mode="itemTypePlural">
    <xsl:text>Classes</xsl:text>
  </xsl:template>
  
  <xsl:template match="interface" mode="itemTypePlural">
    <xsl:text>Interfaces</xsl:text>
  </xsl:template>
  
  <xsl:template match="structure" mode="itemTypePlural">
    <xsl:text>Structures</xsl:text>
  </xsl:template>

  <xsl:template match="enumeration" mode="itemTypePlural">
    <xsl:text>Enumerations</xsl:text>
  </xsl:template>

  <xsl:template match="constructor" mode="itemTypePlural">
    <xsl:text>Constructors</xsl:text>
  </xsl:template>
  
  <xsl:template match="field" mode="itemTypePlural">
    <xsl:text>Fields</xsl:text>
  </xsl:template>
  
  <xsl:template match="property" mode="itemTypePlural">
    <xsl:text>Properties</xsl:text>
  </xsl:template>
  
  <xsl:template match="method" mode="itemTypePlural">
    <xsl:text>Methods</xsl:text>
  </xsl:template>
  
  <xsl:template match="event" mode="itemTypePlural">
    <xsl:text>Events</xsl:text>
  </xsl:template>
  
  <xsl:template match="operator" mode="itemTypePlural">
    <xsl:text>Operators</xsl:text>
  </xsl:template>  

  <xsl:template match="*|node()|@*" mode="itemTypePlural">
    <xsl:value-of select="concat('UnknownItem', local-name(.), 's')" />
  </xsl:template>
  
  <!-- 
    ** summarySection templates 
    -->

  <xsl:template match="class|interface|stucture|enumeration|constructor|field|method|event|property|operator" mode="summarySection">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering summarySection template</xsl:message>
    </xsl:if>

    <xsl:text>&#0010;</xsl:text>
    <xsl:text>!! Summary</xsl:text>
    <xsl:text>&#0010;</xsl:text>
    <xsl:apply-templates mode="summary" select="." />
    <xsl:text>&#0010;</xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  summarySection template</xsl:message>
    </xsl:if>
  </xsl:template>

  <xsl:template match="*|node()|@*" mode="summarySection">
  </xsl:template>
  
  <!-- 
    ** summaryRow templates 
    -->

  <xsl:template match="class|interface|structure|enumeration|constructor|field|method|event|property|operator" mode="summaryRow">
    <xsl:param name="showInheritedColumn">false</xsl:param>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering summaryRow template</xsl:message>
    </xsl:if>

    <xsl:text>||</xsl:text>
    <xsl:apply-templates mode="topicLink" select="." />
    <xsl:text>||</xsl:text>
    <xsl:if test="$showInheritedColumn = 'true'">
      <xsl:call-template name="translateType">
        <xsl:with-param name="fullName" select="@declaringType" />
        <xsl:with-param name="escapeDots">true</xsl:with-param>
      </xsl:call-template>
    <xsl:text>||</xsl:text>
    </xsl:if>
    <xsl:apply-templates mode="summary" select="." />
    <xsl:text>||</xsl:text>
    <xsl:text>&#0010;</xsl:text>
    
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  summaryRow template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="*|node()|@*" mode="summaryRow">
  </xsl:template>
  
  <!-- 
    ** Summary templates 
    -->

  <xsl:template match="namespace|class|interface|structure|enumeration|constructor|method|field|event|property|operator" 
                mode="summary">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering namespace|class|interface|structure|constructor|method|field|event|property|operator summary template</xsl:message>
    </xsl:if>
                
    <xsl:apply-templates mode="summary" select="documentation/summary"/>
 
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  namespace|class|interface|structure|constructor|method|field|event|property|operator summary template</xsl:message>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="see[@cref]" mode="summary">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering see[@cref] summary template</xsl:message>
    </xsl:if>
 
    <xsl:choose>
      <xsl:when test="//*[@id=@cref]">
        <xsl:apply-templates mode="topicLink" select="//*[@id=./@cref]" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:text> *</xsl:text>
        <xsl:value-of select="substring-after(@cref, ':')" />
        <xsl:text>* </xsl:text>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  see[@cref] summary template</xsl:message>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="see[@langword]" mode="summary">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering see[@langword] summary template</xsl:message>
    </xsl:if>

    <xsl:text> *</xsl:text>
    <xsl:value-of select="@langword" />
    <xsl:text>* </xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  see[@langword] summary template</xsl:message>
    </xsl:if>
  </xsl:template>

  <xsl:template match="c">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering c summary template</xsl:message>
    </xsl:if>

    <xsl:text> _</xsl:text>
    <xsl:apply-templates mode="summary" />
    <xsl:text>_ </xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  c summary template</xsl:message>
    </xsl:if>

  </xsl:template>
  
  <xsl:template match="text()" mode="summary">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering text() summary template</xsl:message>
    </xsl:if>

    <xsl:if test="starts-with(., ' ')">
      <xsl:text> </xsl:text>
    </xsl:if> 
    <xsl:value-of select="normalize-space(.)" />
    <xsl:if test="substring(., string-length(.)) = ' '">
      <xsl:text> </xsl:text>
    </xsl:if>
    
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  text() summary template</xsl:message>
    </xsl:if>

  </xsl:template>

  <xsl:template match="*" mode="summary">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering * summary template</xsl:message>
    </xsl:if>
  
    <xsl:apply-templates mode="summary" />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  * summary template</xsl:message>
    </xsl:if>

  </xsl:template>
  
  <!-- 
    ** topicFileName templates 
    -->
  
  <xsl:template match="namespace|class|interface|structure|enumeration|constructor|method|field|event|property|operator" mode="topicFileName">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering topicFileName template</xsl:message>
    </xsl:if>

    <xsl:apply-templates mode="topicName" select="."/>
    <xsl:text>.wiki</xsl:text>

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  topicFileName template</xsl:message>
    </xsl:if>
  </xsl:template>

  <!-- 
    ** TopicLink templates 
    -->
  
  <xsl:template match="namespace|class|interface|structure|enumeration|field|event|property" mode="topicLink">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering non-signature topicLink template</xsl:message>
    </xsl:if>

    <xsl:text>"</xsl:text>
    <xsl:apply-templates mode="itemName" select="." />
    <xsl:text>":</xsl:text>
    <xsl:apply-templates mode="topicName" select="." />

    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  non-signature topicLink template</xsl:message>
    </xsl:if>
  </xsl:template>

  <xsl:template match="constructor|method|operator" mode="topicLink">
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Entering signature topicLink template</xsl:message>
    </xsl:if>

    <xsl:text>"</xsl:text>
    <xsl:apply-templates mode="itemName" select="." />
    <xsl:apply-templates mode="signature" select="." />
    <xsl:text>":</xsl:text>
    <xsl:apply-templates mode="topicName" select="." />
    
    <xsl:if test="number($verbosity) > 3">
      <xsl:message>Exiting  signature topicLink template</xsl:message>
    </xsl:if>

  </xsl:template>
  
  <xsl:template match="*|node()|@*" mode="topicLink">
  </xsl:template>
  
  <!-- 
    ** TopicName templates 
    -->

  <xsl:template match="namespace" mode="topicName">
    <xsl:text>NamespacePage</xsl:text>
    <xsl:value-of select="fwdg:HashFromID(@name)" />
  </xsl:template>

  <xsl:template match="class|interface|structure|enumeration|constructor|field|method|event|property|operator" 
    mode="topicName">
    <xsl:apply-templates mode="itemType" select="." />
    <xsl:text>Page</xsl:text>
    <xsl:value-of select="fwdg:HashFromID(@id)" />
  </xsl:template>

  <!-- 
  ** Signature templates 
  -->

  <xsl:template match="class|interface|structure|enumeration" mode="signature">
    <xsl:apply-templates mode="visibilityKeyword" select="." />
    <xsl:text> </xsl:text>
    <xsl:apply-templates mode="itemTypeKeyword" select="." />
    <xsl:text> </xsl:text>
    <xsl:apply-templates mode="itemName" select="." />
    <xsl:if test="base|implements">
      <xsl:text>: </xsl:text>
    </xsl:if>
    <xsl:if test="base">
      <!-- It might not be a type described in the input file -->
      <xsl:choose>
        <xsl:when test="//class[@id=current()/base/@id]|interface[@id=current()/base/@id]">
          <xsl:apply-templates mode="topicLink" select="base" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="translateType">
            <xsl:with-param name="fullName" select="base/@type" />
            <xsl:with-param name="escapeDots">true</xsl:with-param>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="implements">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </xsl:if>
    
    <xsl:if test="implements">
      <xsl:for-each select="implements">
        <xsl:value-of select="." />
        <xsl:if test="position()  != last()">
          <xsl:text>, </xsl:text>
        </xsl:if>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template match="constructor|method|operator" mode="signature">
    <xsl:text>(</xsl:text>
    <xsl:for-each select="parameter">
      <xsl:if test="@isParamArray = 'true'">
        <xsl:text>params </xsl:text>
      </xsl:if>
      <xsl:if test="@direction = 'ref'">
        <xsl:text>ref </xsl:text>
      </xsl:if>
      <xsl:if test="@direction = 'out'">
        <xsl:text>out </xsl:text>
      </xsl:if>
      <xsl:call-template name="translateType">
        <xsl:with-param name="fullName" select="@type" />
      </xsl:call-template>
      <xsl:text> </xsl:text>
      <xsl:value-of select="@name" />
      <xsl:if test="position() != last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </xsl:for-each>
    <xsl:text>)</xsl:text>
  </xsl:template>
  
  <xsl:template match="property" mode="signature">
    <xsl:text>{ </xsl:text>
    <xsl:if test="@get = 'true'">
      <xsl:text>get; </xsl:text>
    </xsl:if>
    <xsl:if test="@set = 'true'">
      <xsl:text>set; </xsl:text>
    </xsl:if>
    <xsl:text> }</xsl:text>
  </xsl:template>
  
  <xsl:template match="*|node()|@*" mode="signature">
  </xsl:template>

  <!-- 
  ** translateType template
  -->
  <xsl:template name="translateType">
    <xsl:param name="fullName" />
    <xsl:param name="escapeDots">false</xsl:param>
    <xsl:choose>
      <xsl:when test="$fullName = 'System.Void'">
        <xsl:text>void</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Int32'">
        <xsl:text>int</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Object'">
        <xsl:text>object</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Double'">
        <xsl:text>double</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.String'">
        <xsl:text>string</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Boolean'">
        <xsl:text>bool</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Byte'">
        <xsl:text>byte</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Int32[]'">
        <xsl:text>int[]</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Object[]'">
        <xsl:text>object[]</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Double[]'">
        <xsl:text>double[]</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.String[]'">
        <xsl:text>string[]</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Boolean[]'">
        <xsl:text>bool[]</xsl:text>
      </xsl:when>
      <xsl:when test="$fullName = 'System.Byte[]'">
        <xsl:text>byte[]</xsl:text>
      </xsl:when>
      <!-- TODO: Add other language keywords here -->
      <xsl:otherwise>
        <!-- Sometimes needs escaping because dots in the type name look like namespaces otherwise -->
        <xsl:if test="$escapeDots = 'true'">
          <xsl:text>""</xsl:text>
        </xsl:if>
        <xsl:value-of select="$fullName" />
        <xsl:if test="$escapeDots = 'true'">
          <xsl:text>""</xsl:text>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="class|interface|structure|enumeration|constructor|field|method|event|property|operator"
    mode="visibilityKeyword">
    <xsl:choose>
      <xsl:when test="@access = 'Public'">
        <xsl:text>public</xsl:text>
      </xsl:when>
      <xsl:when test="@access = 'Family'">
        <xsl:text>protected</xsl:text>
      </xsl:when>
      <xsl:when test="@access = 'Private'">
        <xsl:text>private</xsl:text>
      </xsl:when>
      <xsl:when test="@access = 'NestedFamily'">
        <xsl:text>protected</xsl:text>
      </xsl:when>
      <xsl:when test="@access = 'NestedPublic'">
        <xsl:text>public</xsl:text>
      </xsl:when>
      <!-- TODO: Add other protection levels here -->
    </xsl:choose>
  </xsl:template>

  <xsl:template match="class|interface|structure|enumeration|constructor|field|method|event|property|operator"
    mode="contractKeyword">
    <xsl:choose>
      <xsl:when test="@contract = 'Normal'">
        <xsl:text></xsl:text>
      </xsl:when>
      <xsl:when test="@contract = 'Override'">
        <xsl:text>override</xsl:text>
      </xsl:when>
      <xsl:when test="@contract = 'Static'">
        <xsl:text>static</xsl:text>
      </xsl:when>
      <xsl:when test="@contract = 'Virtual'">
        <xsl:text>virtual</xsl:text>
      </xsl:when>
      <xsl:when test="@contract = 'Final'">
        <xsl:text>sealed</xsl:text>
      </xsl:when>

      <!-- TODO: Add other protection levels here -->
    </xsl:choose>
  </xsl:template>
  
  <!-- 
  ** makeCommentsSection template
  -->

  <xsl:template name="makeCommentsSection">
    <xsl:text>! Comments</xsl:text>
    <xsl:text>&#0010;</xsl:text>
    <xsl:text>Comments can be edited by visiting "this page":</xsl:text>
    <xsl:call-template name="commentTopicName" />
    <xsl:text>&#0010;</xsl:text>
    <xsl:text>{{</xsl:text>
    <xsl:call-template name="commentTopicName" />
    <xsl:text>}}</xsl:text>
  </xsl:template>

  <!-- 
  ** commentTopicName template
  -->

  <xsl:template name="commentTopicName">
    <xsl:value-of select="$commentNamespace" />
    <xsl:text>.</xsl:text>
    <xsl:apply-templates select="." mode="topicName" />
  </xsl:template>

</xsl:stylesheet>