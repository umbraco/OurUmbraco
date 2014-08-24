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
        <a href="/documentation/Using-Umbraco/Creating-Basic-Site/">
            Creating a Basic Website using Umbraco v7
        </a>
         <small>
            An in-depth guide to installing, creating document types/templates/css/javascript, and adding your own content
        </small>
    </li>

    <li>
        <a href="/documentation/Installation/">
            Installing Umbraco
        </a>
        <small>
            Installing using WebMatrix, manually, or using NuGet
        </small>
    </li>

    <li>
    <a href="/documentation/Reference/Templating/Mvc/">
        Working with Mvc Views in Umbraco
    </a>
    <small>
        Documentation on Views, Partial Views, Surface Controllers, Child Actions, Querying, Custom Controllers and more
    </small>
    </li>

    <li>
        <a href="/documentation/Reference/WebApi/">
            Using Umbraco's WebApi to create REST services
        </a>
        <small>
            Create your own REST services by using Umbraco's WebApi
        </small>
    </li>

  <li>
    <a href="/documentation/Reference/Management-v6/">
      Umbraco Management API
    </a>
    <small>
      Reference for all the Services and Models available in the API
    </small>
  </li>

  <li>
    <a href="/documentation/Reference/Querying/">
      Querying content, media, members and relations
    </a>
    <small>
      How to query Umbraco data using Razor Macros or C#
    </small>
  </li>

  <li>
    <a href="/documentation/reference/Events-v6/">
      Events in the API
    </a>
    <small>
		Documentation on the events available in Umbraco API and how to register them
    </small>
  </li>
</ul>


</xsl:template>

</xsl:stylesheet>