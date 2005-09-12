<?xml version="1.0" ?><xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" ><xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/><xsl:template match="/rss/channel">!<xsl:value-of select="title"/><xsl:attribute name="href"><xsl:value-of select="link"/></xsl:attribute><xsl:value-of select="lastBuildDate"/>
||'''Published Date'''||'''Title'''||
<xsl:for-each select="item">||<xsl:value-of select="pubDate"/>||"<xsl:value-of select="title"/>":<xsl:value-of select="link"/>||
</xsl:for-each>
</xsl:template>
</xsl:stylesheet>
