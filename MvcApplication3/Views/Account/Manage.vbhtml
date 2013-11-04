@ModelType MvcApplication3.LocalPasswordModel
@Code
    ViewData("Title") = "アカウントの管理"
End Code

<hgroup class="title">
    <h1>@ViewData("Title").</h1>
</hgroup>

<p class="message-success">@ViewData("StatusMessage")</p>

<p> としてログインしています。 <strong>@User.Identity.Name</strong>.</p>

@If ViewData("HasLocalPassword") Then
    @Html.Partial("_ChangePasswordPartial")
Else
    @Html.Partial("_SetPasswordPartial")
End If

<section id="externalLogins">
    @Html.Action("RemoveExternalLogins")

    <h3>外部ログインの追加</h3>
    @Html.Action("ExternalLoginsList", New With {.ReturnUrl = ViewData("ReturnUrl")})
</section>

@Section Scripts
    @Scripts.Render("~/bundles/jqueryval")
End Section
