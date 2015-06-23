#Working with Contour data in XSLT
Umbraco Contour includes an XSLT Extension library which is accessible through the XSLT Editor in the developer section.
##Record XML Format
The Library contains a number of methods which returns records as XML in the below format

	<?xml version="1.0" encoding="utf-8"?> 
	<uformrecords> 
		<uformrecord> 
			<state>Approved</state> 
			<created>2009-11-13T10:01:55</created> 
			<updated>2009-11-13T10:01:55</updated> 
			<id>119ecc43-df79-46e1-9020-b2e27e239175</id> 
			<ip>127.0.0.1</ip> 
			<pageid>0</pageid> 
			<memberkey></memberkey> 
			<fields> 
				<name record="119ecc43-df79-46e1-9020-b2e27e239175" sortorder="0"> 
					<key>2295187e-0345-4260-a406-eabcc1e774e2</key> 
					<fieldKey>e6157c93-0b54-4415-b7ba-5c7c2c953b70</fieldKey> 
					<caption>Name</caption> 
					<datatype>String</datatype> 
					<values> 
						<value><![CDATA[My Name]]></value> 
					</values> 
				</name> 
				<email record="119ecc43-df79-46e1-9020-b2e27e239175" sortorder="1"> 
					<key>a92875a8-938d-4ba0-990a-59a3518ce62c</key> 
					<fieldKey>d8b10ffb-c437-4a44-8df6-01e6af5ac26f</fieldKey>
					<caption>Email</caption> <datatype>String</datatype> 
					<values> 
						<value><![CDATA[pph@testdomain.com]]></value> 
					</values> 
				</email> 
			</fields> 
		</uformrecord> 
	</uformrecords>
All record nodes are contained in a <uformrecords> element. All records consist of a <uformrecord> element with some meta data on it. The <uformrecord> contains a field element which contains a collection of nodes reflecting the form data fields.

The naming of the child elements inside the <fields> element are named accordingly to the caption of the field converted to lower case and with all foreign characters removed.

The element contains a <values> element which contains all entered values in individual <value> elements. A field can have multiple values, a checkboxlist for instance can save multiple values.

##Umbraco Contour Library methods
Umbraco Contour includes a libary for easy access to record data in the xml format. The library is located in the class Umbraco.Forms.Library

###GetApprovedRecordsFromPage
	XPathNodeIterator GetApprovedRecordsFromPage(int pageId)
Returns All records with the state set to approved from all forms on the umbraco page with the id = pageId as a XPathNodeIterator
###GetApprovedRecordsFromFormOnPage
	XPathNodeIterator GetApprovedRecordsFromFormOnPage(int pageId, string formId)
Returns All records with the state set to approved from the form with the id = formId on the umbraco page with the id = pageId as a XPathNodeIterator
###GetRecordsFromPage
	XPathNodeIterator GetRecordsFromPage(int pageId)
Returns All records from all forms on the umbraco page with the id = pageId as a XPathNodeIterator
###GetRecordsFromFormOnPage
	XPathNodeIterator GetRecordsFromFormOnPage(int pageId, string formId)
Returns All records from the form with the id = formId on the umbraco page with the id = pageId as a XPathNodeIterator
###GetRecordsFromForm
	XPathNodeIterator GetRecordsFromForm(string formId)
Returns All records from the form with the ID = formId as a XPathNodeIterator
###GetRecords
	XPathNodeIterator GetRecord(string recordId)
Returns the specific record with the ID = recordId as a XPathNodeIterator
##Sample XPath statements
These samples are provided as an introduction to working with Contour xml data. It does however follow the XPath standard and the above xml format can work with any valid XPath. The below snippets needs the standard umbraco xslt file to work, so simply create a new xslt file and insert the snippets on that file.
###Get all records form the current page

	<ul> 
	<xsl:for-each select="umbraco.contour:GetRecordsFromPage($currentPage/@id)//uformrecord"> 
	<xsl:sort select="created" order="ascending"/> 
		<li> 
			A record with the state set to <xsl:value-of select="state"/> 
			was created on <xsl:value-of select="umbraco.library:LongDate(created)"/> 
		</li> 
	</xsl:for-each> 
	</ul>
###To select all fields on a record
	<xsl:for-each select="umbraco.contour:GetRecord($id)/uformrecord/fields/child::*"> 
		<xsl:sort select="caption" order="ascending"/> 
			<h4> 
				<xsl:value-of select=" caption"/> 
			</h4> 
	</xsl:for-each>
###To Select a field with a specific caption
	<xsl:variable name="record" select="umbraco.contour:GetRecord($id)"/> 
	<xsl:variable name="email" select="$record/uformrecord/fields/child::* [caption = 'Email']"/> 

	or 

	<xsl:variable name="email" select="$record/uformrecord/fields/email"/>

