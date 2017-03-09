<%@ Page Language="C#" AutoEventWireup="true" Inherits="AI.DotNetAnalyser.FuncTests.WebForms.MasterPages.MasterPagesFormWithBaseMaster" 
    MasterPageFile="BaseMasterPage.master"%>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <% var adminPassword= "value"; %>
            <%= Request.Params %>
            <%: "<b>HtmlEncoded</b> expression text" %>
        </div>
    </form>
    <script runat="server">
        protected void Page_Load(object arg, EventArgs e)
        {
            try
            {
                Random r = new Random();
            }
            catch
            {
            }
        }
    </script>
</body>
</html>
