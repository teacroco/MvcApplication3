@If Request.IsAuthenticated Then
    @<text>
        こんにちは、@Html.ActionLink(User.Identity.Name, "Manage", "Account", routeValues:=Nothing, htmlAttributes:=New With {.class = "username", .title = "管理"})!
        @Using Html.BeginForm("LogOff", "Account", FormMethod.Post, New With {.id = "logoutForm"})
            @Html.AntiForgeryToken()
            @<a href="javascript:document.getElementById('logoutForm').submit()">ログオフ</a>
        End Using
    </text>
Else
    @<ul>
        <li>@Html.ActionLink("登録", "Register", "Account", routeValues:=Nothing, htmlAttributes:=New With {.id = "registerLink"})</li>
        <li>@Html.ActionLink("ログイン", "Login", "Account", routeValues:=Nothing, htmlAttributes:=New With {.id = "loginLink"})</li>
    </ul>
End If
