<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestAspxParser.aspx.cs" Inherits="PocExample.TestAspxParser" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
        
        <% Response.Write("Statement text"); %>
        
        <%= "Expression text" %>
        
        <%: "<b>HtmlEncoded</b> expression text" %>
        
        <asp:Button ID="Button1" runat="server" Text="Button" />
    <div>
    
    </div>
    </form>
</body>
</html>
