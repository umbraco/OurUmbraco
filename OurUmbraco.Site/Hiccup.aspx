<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Hiccup.aspx.cs" Inherits="OurUmbraco.Our.Hiccup" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
    <title><asp:Literal ID="titleLiteral" runat="server"></asp:Literal></title>
	
	<style type="text/css">
        body { margin: 40px; padding: 0; background: #fff; font-family: Helvetica, Arial , sans-serif; }
        body div { width: 600px; }
        a, h1, h3 {color: #7ec245;}
        p, h4{color: #575757;}
        h2 { font-size: 18px; font-weight: bold; margin: 0px 0 8px 0; padding: 0; }
        p { font-size: 12px; line-height: 18px; font-weight: normal; margin: 0 0 20px 0; padding: 0; }
    </style>
</head>
<body>
    <div>
        <h1><asp:Literal ID="header1" runat="server"></asp:Literal> hiccuped</h1>
        <h3>
            <asp:Literal ID="header3" runat="server"></asp:Literal> has had a minor hiccup and an<br />
            error has occured.
        </h3>

        <p>
            Sorry for the inconvenience, we are feeding it vinegar off a spoon and getting it to drink glasses of water 
            backwards as fast as we can and one of the friendly Umbraco HQ members will be<br />
            looking into this problem asap.
        </p>
        
        <p>
            <strong>
                This will not affect your umbraco powered website, it is only the <asp:Literal ID="notAffect" runat="server"></asp:Literal> website<br />
                which has had a slight hiccup.
            </strong>
        </p>
        
        <p>
            In the meantime why don't you take a look at the <a href="http://search.twitter.com/search?q=umbraco" target="_blank" rel="noreferrer noopener">Umbraco twitter stream</a>?

            
        </p>

        <p>
            Sorry for the inconvenience<br/>
	        The Umbraco HQ Team
        </p>
    </div>
</body>
</html>