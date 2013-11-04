@ModelType MvcApplication3.RegisterExternalLoginModel
@Code
    ViewData("Title") = "登録"
End Code

<hgroup class="title">
    <h1>@ViewData("Title").</h1>
    <h2>@ViewData("ProviderDisplayName") アカウントを関連付けます。</h2>
</hgroup>

@Using Html.BeginForm("ExternalLoginConfirmation", "Account", New With {.ReturnUrl = ViewData("ReturnUrl")})
    @Html.AntiForgeryToken()
    @Html.ValidationSummary(true)

    @<fieldset>
        <legend>関連付けフォーム</legend>
        <p>
            によって正常に認証されました。<strong>@ViewData("ProviderDisplayName")</strong> によって正常に認証されました。
下にこのサイトのユーザー名を入力し、[確認] ボタンを押して、
ログインを完了してください。
        </p>
        <ol>
            <li class="name">
                @Html.LabelFor(Function(m) m.UserName)
                @Html.TextBoxFor(Function(m) m.UserName)
                @Html.ValidationMessageFor(Function(m) m.UserName)
            </li>
        </ol>
        @Html.HiddenFor(Function(m) m.ExternalLoginData)
        <input type="submit" value="登録" />
    </fieldset>
End Using

@Section Scripts
    @Scripts.Render("~/bundles/jqueryval")
End Section
