<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:output method="html" />

  <xsl:template match="/">
    <xsl:if test="not (/cruisecontrol/build/@error) and not (/cruisecontrol/exception) and (/cruisecontrol/version)">
      <xsl:variable name="vermajor" select="/cruisecontrol/version/major" />
      <xsl:variable name="verminor" select="/cruisecontrol/version/minor" />
      <xsl:variable name="verbuild" select="/cruisecontrol/version/build" />
      <xsl:variable name="verrevision" select="/cruisecontrol/version/revision" />
      <xsl:variable name="project" select="/cruisecontrol/@project" />
      
      <xsl:variable 
	name="projectURL" 
	select="document('../ccnet.config')/cruisecontrol/project[@name=$project]/publishers/email/projectUrl/text()" />

<!--      
      <debug>
        <property name="project" value="{$project}" />
        <property name="projectURL" value="{$projectURL}" />
        <property name="count(//projects)" value="{count(document('../ccnet.config')//project)}" />
        <property name="local-name(document('../ccnet.config'))" 
                  value="{local-name(document('../ccnet.config'))}" />
      </debug>
    -->  
      <table class="section-table" cellpadding="2" cellspacing="0" border="0" width="98%">
        <tr>
          <td class="modifications-sectionheader">
             Download links
          </td>
        </tr>
        <tr>
          <td>
            Download binaries and source <a href="{$projectURL}/download/{$vermajor}.{$verminor}.{$verbuild}.{$verrevision}/">here</a>
          </td>
        </tr>
      </table>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
