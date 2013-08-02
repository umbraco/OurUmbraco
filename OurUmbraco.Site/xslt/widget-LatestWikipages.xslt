<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">

<ul class="wiki summary">

  <li>
    <a href="/documentation/Reference/Mvc/">
      Working with Mvc
    </a>
    <small>
      Documentation on Views, Partial Views, Surface Controllers, Child Actions, Querying, Custom Controllers and more
    </small>
  </li>
  <li>
    <a href="/documentation/Reference/Management-v6/">
      Umbraco v6 API
    </a>
    <small>
      Reference for all the Services and Models available in the v6 API
    </small>
  </li>
  <li>
    <a href="/documentation/Reference/Management/">
      Umbraco v4 API
    </a>
    <small>
      Reference for accessing Nodes, Documents, Members and more
    </small>
  </li>
  <li>
    <a href="/documentation/Reference/Querying/">
      Querying content, media, members and relations
    </a>
    <small>
      How to query Umbraco data using Razor Macros or C# using DynamicNode or uQuery
    </small>
  </li>
  <li>
    <a href="/documentation/Reference/Events/">
      Events
    </a>
    <small>
      Documentation on the events available in Umbraco how to register them
    </small>
  </li>
  <li>
    <a href="/documentation/Reference/Templating/">
      Templating
    </a>
    <small>
      Documentation on how to work with masterpages and macros
    </small>
  </li>


  <!-- xsl:for-each select="umbraco.library:GetXmlNodeById(1054)/descendant::* [@isDoc]">
<xsl:sort select="@createDate" order="descending"/>
<xsl:if test="position() &lt;= 5">
  <li><a href="{umbraco.library:NiceUrl(@id)}"><xsl:value-of select="@nodeName"/></a>
  <small>In <xsl:value-of select="../@nodeName"/>,
    
    <xsl:if test="author != '' and string(author) != '0'">
    by <xsl:value-of select="umbraco.library:GetMemberName(author)"/> 
    </xsl:if>
    </small>
  </li>
</xsl:if>
</-->
</ul>


</xsl:template>

</xsl:stylesheet>