Imports System.Diagnostics.CodeAnalysis
Imports System.Security.Principal
Imports System.Transactions
Imports System.Web.Routing
Imports DotNetOpenAuth.AspNet
Imports Microsoft.Web.WebPages.OAuth
Imports WebMatrix.WebData

<Authorize()> _
<InitializeSimpleMembership()> _
Public Class AccountController
    Inherits System.Web.Mvc.Controller

    '
    ' GET: /Account/Login

    <AllowAnonymous()> _
    Public Function Login(ByVal returnUrl As String) As ActionResult
        ViewData("ReturnUrl") = returnUrl
        Return View()
    End Function

    '
    ' POST: /Account/Login

    <HttpPost()> _
    <AllowAnonymous()> _
    <ValidateAntiForgeryToken()> _
    Public Function Login(ByVal model As LoginModel, ByVal returnUrl As String) As ActionResult
        If ModelState.IsValid AndAlso WebSecurity.Login(model.UserName, model.Password, persistCookie:=model.RememberMe) Then
            Return RedirectToLocal(returnUrl)
        End If

        ' ここで問題が発生した場合はフォームを再表示します
        ModelState.AddModelError("", "指定されたユーザー名またはパスワードが正しくありません。")
        Return View(model)
    End Function

    '
    ' POST: /Account/LogOff

    <HttpPost()> _
    <ValidateAntiForgeryToken()> _
    Public Function LogOff() As ActionResult
        WebSecurity.Logout()

        Return RedirectToAction("Index", "Home")
    End Function

    '
    ' GET: /Account/Register

    <AllowAnonymous()> _
    Public Function Register() As ActionResult
        Return View()
    End Function

    '
    ' POST: /Account/Register

    <HttpPost()> _
    <AllowAnonymous()> _
    <ValidateAntiForgeryToken()> _
    Public Function Register(ByVal model As RegisterModel) As ActionResult
        If ModelState.IsValid Then
            ' ユーザーの登録を試みます
            Try
                WebSecurity.CreateUserAndAccount(model.UserName, model.Password)
                WebSecurity.Login(model.UserName, model.Password)
                Return RedirectToAction("Index", "Home")
            Catch e As MembershipCreateUserException

                ModelState.AddModelError("", ErrorCodeToString(e.StatusCode))
            End Try
        End If

        ' ここで問題が発生した場合はフォームを再表示します
        Return View(model)
    End Function

    '
    ' POST: /Account/Disassociate

    <HttpPost()> _
    <ValidateAntiForgeryToken()> _
    Public Function Disassociate(ByVal provider As String, ByVal providerUserId As String) As ActionResult
        ' ユーザーが誤って一度にすべてのアカウントの関連付けを解除しないように、トランザクションをラップします。

        Dim ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId)
        Dim message As ManageMessageId? = Nothing

        ' 現在ログインしているユーザーが所有者の場合にのみ、アカウントの関連付けを解除します
        If ownerAccount = User.Identity.Name Then
            ' トランザクションを使用して、ユーザーが最後のログイン資格情報を削除しないようにします
            Using scope As New TransactionScope(TransactionScopeOption.Required, New TransactionOptions With {.IsolationLevel = IsolationLevel.Serializable})
                Dim hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name))
                If hasLocalAccount OrElse OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1 Then
                    OAuthWebSecurity.DeleteAccount(provider, providerUserId)
                    scope.Complete()
                    message = ManageMessageId.RemoveLoginSuccess
                End If
            End Using
        End If

        Return RedirectToAction("Manage", New With {.Message = message})
    End Function

    '
    ' GET: /Account/Manage

    Public Function Manage(ByVal message As ManageMessageId?) As ActionResult
        ViewData("StatusMessage") =
            If(message = ManageMessageId.ChangePasswordSuccess, "パスワードが変更されました。", _
                If(message = ManageMessageId.SetPasswordSuccess, "パスワードが設定されました。", _
                    If(message = ManageMessageId.RemoveLoginSuccess, "外部ログインが削除されました。", _
                        "")))

        ViewData("HasLocalPassword") = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name))
        ViewData("ReturnUrl") = Url.Action("Manage")
        Return View()
    End Function

    '
    ' POST: /Account/Manage

    <HttpPost()> _
    <ValidateAntiForgeryToken()> _
    Public Function Manage(ByVal model As LocalPasswordModel) As ActionResult
        Dim hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name))
        ViewData("HasLocalPassword") = hasLocalAccount
        ViewData("ReturnUrl") = Url.Action("Manage")
        If hasLocalAccount Then
            If ModelState.IsValid Then
                ' 特定のエラー シナリオでは、ChangePassword は false を返す代わりに例外をスローします。
                Dim changePasswordSucceeded As Boolean

                Try
                    changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword)
                Catch e As Exception
                    changePasswordSucceeded = False
                End Try

                If changePasswordSucceeded Then
                    Return RedirectToAction("Manage", New With {.Message = ManageMessageId.ChangePasswordSuccess})
                Else
                    ModelState.AddModelError("", "現在のパスワードが正しくないか、新しいパスワードが無効です。")
                End If
            End If
        Else
            ' ユーザーにローカル パスワードがないため、
            'OldPassword フィールドがないことに原因があるすべての検証エラーを削除します
            Dim state = ModelState("OldPassword")
            If state IsNot Nothing Then
                state.Errors.Clear()
            End If

            If ModelState.IsValid Then
                Try
                    WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword)
                    Return RedirectToAction("Manage", New With {.Message = ManageMessageId.SetPasswordSuccess})
                Catch e As Exception
                    ModelState.AddModelError("", String.Format("ローカル アカウントを作成できません。名前 ""{0}"" のアカウントは既に存在している可能性があります。", User.Identity.Name))
                End Try
            End If
        End If

        ' ここで問題が発生した場合はフォームを再表示します 
        Return View(model)
    End Function

    '
    ' POST: /Account/ExternalLogin

    <HttpPost()> _
    <AllowAnonymous()> _
    <ValidateAntiForgeryToken()> _
    Public Function ExternalLogin(ByVal provider As String, ByVal returnUrl As String) As ActionResult
        Return New ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", New With {.ReturnUrl = returnUrl}))
    End Function

    '
    ' GET: /Account/ExternalLoginCallback

    <AllowAnonymous()> _
    Public Function ExternalLoginCallback(ByVal returnUrl As String) As ActionResult
        Dim result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", New With {.ReturnUrl = returnUrl}))
        If Not result.IsSuccessful Then
            Return RedirectToAction("ExternalLoginFailure")
        End If

        If OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie:=False) Then
            Return RedirectToLocal(returnUrl)
        End If

        If User.Identity.IsAuthenticated Then
            ' 現在のユーザーがログインしている場合、新しいアカウントを追加します
            OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name)
            Return RedirectToLocal(returnUrl)
        Else
            ' ユーザーは新規なので、希望するメンバーシップ名を要求します
            Dim loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId)
            ViewData("ProviderDisplayName") = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName
            ViewData("ReturnUrl") = returnUrl
            Return View("ExternalLoginConfirmation", New RegisterExternalLoginModel With {.UserName = result.UserName, .ExternalLoginData = loginData})
        End If
    End Function

    '
    ' POST: /Account/ExternalLoginConfirmation

    <HttpPost()> _
    <AllowAnonymous()> _
    <ValidateAntiForgeryToken()> _
    Public Function ExternalLoginConfirmation(ByVal model As RegisterExternalLoginModel, ByVal returnUrl As String) As ActionResult
        Dim provider As String = Nothing
        Dim providerUserId As String = Nothing

        If User.Identity.IsAuthenticated OrElse Not OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, provider, providerUserId) Then
            Return RedirectToAction("管理")
        End If

        If ModelState.IsValid Then
            ' データベースに新しいユーザーを挿入します
            Using db As New UsersContext()
                Dim user = db.UserProfiles.FirstOrDefault(Function(u) u.UserName.ToLower() = model.UserName.ToLower())
                ' ユーザーが既に存在するかどうかを確認します
                If user Is Nothing Then
                    ' プロファイル テーブルに名前を挿入します
                    db.UserProfiles.Add(New UserProfile With {.UserName = model.UserName})
                    db.SaveChanges()

                    OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName)
                    OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie:=False)

                    Return RedirectToLocal(returnUrl)
                Else
                    ModelState.AddModelError("UserName", "このユーザー名は既に存在します。別のユーザー名を入力してください。")
                End If
            End Using
        End If

        ViewData("ProviderDisplayName") = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName
        ViewData("ReturnUrl") = returnUrl
        Return View(model)
    End Function

    '
    ' GET: /Account/ExternalLoginFailure

    <AllowAnonymous()> _
    Public Function ExternalLoginFailure() As ActionResult
        Return View()
    End Function

    <AllowAnonymous()> _
    <ChildActionOnly()> _
    Public Function ExternalLoginsList(ByVal returnUrl As String) As ActionResult
        ViewData("ReturnUrl") = returnUrl
        Return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData)
    End Function

    <ChildActionOnly()> _
    Public Function RemoveExternalLogins() As ActionResult
        Dim accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name)
        Dim externalLogins = New List(Of ExternalLogin)()
        For Each account As OAuthAccount In accounts
            Dim clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider)

            externalLogins.Add(New ExternalLogin With { _
                .Provider = account.Provider, _
                .ProviderDisplayName = clientData.DisplayName, _
                .ProviderUserId = account.ProviderUserId _
            })
        Next

        ViewData("ShowRemoveButton") = externalLogins.Count > 1 OrElse OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name))
        Return PartialView("_RemoveExternalLoginsPartial", externalLogins)
    End Function

#Region "ヘルパー"
    Private Function RedirectToLocal(ByVal returnUrl As String) As ActionResult
        If Url.IsLocalUrl(returnUrl) Then
            Return Redirect(returnUrl)
        Else
            Return RedirectToAction("Index", "Home")
        End If
    End Function

    Public Enum ManageMessageId
        ChangePasswordSuccess
        SetPasswordSuccess
        RemoveLoginSuccess
    End Enum

    Friend Class ExternalLoginResult
        Inherits System.Web.Mvc.ActionResult

        Private ReadOnly _provider As String
        Private ReadOnly _returnUrl As String

        Public Sub New(ByVal provider As String, ByVal returnUrl As String)
            _provider = provider
            _returnUrl = returnUrl
        End Sub

        Public ReadOnly Property Provider() As String
            Get
                Return _provider
            End Get
        End Property

        Public ReadOnly Property ReturnUrl() As String
            Get
                Return _returnUrl
            End Get
        End Property

        Public Overrides Sub ExecuteResult(ByVal context As ControllerContext)
            OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl)
        End Sub
    End Class

    Public Function ErrorCodeToString(ByVal createStatus As MembershipCreateStatus) As String
        ' すべてのステータス コードの一覧については、http://go.microsoft.com/fwlink/?LinkID=177550 を
        ' 参照してください。
        Select Case createStatus
            Case MembershipCreateStatus.DuplicateUserName
                Return "このユーザー名は既に存在します。別のユーザー名を入力してください。"

            Case MembershipCreateStatus.DuplicateEmail
                Return "その電子メール アドレスのユーザー名は既に存在します。別の電子メール アドレスを入力してください。"

            Case MembershipCreateStatus.InvalidPassword
                Return "指定されたパスワードは無効です。有効なパスワードの値を入力してください。"

            Case MembershipCreateStatus.InvalidEmail
                Return "指定された電子メール アドレスは無効です。値を確認してやり直してください。"

            Case MembershipCreateStatus.InvalidAnswer
                Return "パスワードの回復用に指定された回答が無効です。値を確認してやり直してください。"

            Case MembershipCreateStatus.InvalidQuestion
                Return "パスワードの回復用に指定された質問が無効です。値を確認してやり直してください。"

            Case MembershipCreateStatus.InvalidUserName
                Return "指定されたユーザー名は無効です。値を確認してやり直してください。"

            Case MembershipCreateStatus.ProviderError
                Return "認証プロバイダーからエラーが返されました。入力を確認してやり直してください。問題が解決しない場合は、システム管理者に連絡してください。"

            Case MembershipCreateStatus.UserRejected
                Return "ユーザーの作成要求が取り消されました。入力を確認してやり直してください。問題が解決しない場合は、システム管理者に連絡してください。"

            Case Else
                Return "不明なエラーが発生しました。入力を確認してやり直してください。問題が解決しない場合は、システム管理者に連絡してください。"
        End Select
    End Function
#End Region

End Class
