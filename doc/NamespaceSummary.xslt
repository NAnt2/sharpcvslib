<xsl:stylesheet version="1.0"
     xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:template match="/">
        <html>
            <head>
                <title>SharpCvsLib API Documentation</title>
            </head>
            <body>
                <xsl:for-each select="//namespaces/namespace">
                    <xsl:element name="a">
                        <xsl:attribute name="href">
                            <xsl:value-of select="@name" /><xsl:text>.html</xsl:text>
                        </xsl:attribute>
                        <xsl:value-of select="@name" />
                    </xsl:element>
                    <br />
                </xsl:for-each>
            </body>

        </html>    
    </xsl:template>


</xsl:stylesheet>