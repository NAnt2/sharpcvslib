<xsl:stylesheet version="1.0"
     xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:template match="/">
        <html>
            <head>
                <title>SharpCvsLib API Documentation</title>
                <link rel="stylesheet" type="text/css" href="MSDN.css"></link>
                <script language="javascript" type="text/javascript" src="scroll.js">
                </script>
                
                <style>
                    .license {text-align:right;margin-right: 10%;}
                    .namespace-summary {margin-left: 5%;}
                </style>
            </head>
            <body id="bodyID" class="dtBODY">
                <div id="nsbanner">
                    <div id="bannerrow1">
                        <table class="bannerparthead" cellspacing="0">
                            <tr id="hdr">
                                <td class="runninghead">SharpCvsLib</td>
                                <td class="product">
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div id="TitleRow">
                        <div class="license">
                            <a href="license.html">license</a>
                        </div>
                        <h1 class="dtH1">SharpCvsLib API</h1>
                    </div>
                </div>
                <br />
                <h1>Namespace Summary:</h1>
                <hr />
                <br />
                <div class="namespace-summary">
                    <ul>
                        <xsl:for-each select="//namespaces/namespace">
                            <li>
                                <xsl:element name="a">
                                    <xsl:attribute name="href">
                                        <xsl:value-of select="@name" /><xsl:text>.html</xsl:text>
                                    </xsl:attribute>
                                    <xsl:value-of select="@name" />
                                </xsl:element>
                                <br />
                            </li>    
                        </xsl:for-each>
                    </ul>    
                </div>    
            </body>

        </html>    
    </xsl:template>


</xsl:stylesheet>