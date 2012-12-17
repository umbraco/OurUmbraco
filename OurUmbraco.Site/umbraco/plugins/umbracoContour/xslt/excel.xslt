<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:msxsl="urn:schemas-microsoft-com:xslt"
xmlns:user="urn:my-scripts">    
    
  <xsl:output method="text" indent="no" encoding="utf-8" />

  <xsl:param name="records" />
  
  <xsl:variable name="fields">
    <fields>
      <xsl:for-each select="$records//fields/child::*">
        <xsl:sort select="./caption" order="ascending"/>
        <field>
          <caption>
            <xsl:value-of select="./caption"/>
          </caption>
        </field>
      </xsl:for-each>
    </fields>
  </xsl:variable>
  <xsl:variable name="uniqueFields" select="msxsl:node-set($fields)//field[not(caption=preceding-sibling::field/caption)]/caption"/>
  
<xsl:template match="/">
"State","Submitted","PageId","IP","MemberId",<xsl:for-each select="$uniqueFields"><xsl:if test="position() != last()">"<xsl:value-of select="normalize-space(translate(.,',',''))"/>",</xsl:if><xsl:if test="position()  = last()">"<xsl:value-of select="normalize-space(translate(.,',',''))"/>"<xsl:text>&#xD;</xsl:text></xsl:if></xsl:for-each>
<xsl:for-each select="$records//uformrecord">"<xsl:value-of select="state"/>","<xsl:value-of select="updated"/>","<xsl:value-of select="pageid"/>","<xsl:value-of select="ip"/>","<xsl:value-of select="memberkey"/>",<xsl:variable name="record" select="."></xsl:variable><xsl:for-each select="$uniqueFields"><xsl:variable name="captionName" select="."></xsl:variable><xsl:choose><xsl:when test="count($record/fields/child::* [./caption = $captionName]) = 0">""</xsl:when><xsl:otherwise><xsl:variable name="values" select="$record//fields/child::* [./caption = $captionName]//values"></xsl:variable><xsl:choose><xsl:when test="count($values//value) &gt; 1">"<xsl:for-each select="$values//value"><xsl:if test="position() != last()"><xsl:value-of select="normalize-space(translate(.,',',''))"/>,</xsl:if><xsl:if test="position() = last()"><xsl:value-of select="normalize-space(translate(.,',',''))"/></xsl:if></xsl:for-each>"</xsl:when><xsl:otherwise>"<xsl:value-of select="normalize-space(translate($values//value,',',''))"/>"</xsl:otherwise></xsl:choose></xsl:otherwise></xsl:choose><xsl:if test="not(position() = last())">,</xsl:if></xsl:for-each><xsl:text>&#xD;</xsl:text></xsl:for-each>
    
</xsl:template>
    
   
 
</xsl:stylesheet>