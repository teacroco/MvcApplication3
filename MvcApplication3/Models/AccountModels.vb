Imports System.ComponentModel
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Globalization
Imports System.Data.Entity

Public Class UsersContext
    Inherits DbContext

    Public Sub New()
        MyBase.New("DefaultConnection")
    End Sub

    Public Property UserProfiles As DbSet(Of UserProfile)
End Class

<Table("UserProfile")> _
Public Class UserProfile
    <Key()> _
    <DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)> _
    Public Property UserId As Integer

    Public Property UserName As String
End Class

Public Class RegisterExternalLoginModel
    <Required()> _
    <Display(Name:="ユーザー名")> _
    Public Property UserName As String

    Public Property ExternalLoginData As String
End Class

Public Class LocalPasswordModel
    <Required()> _
    <DataType(DataType.Password)> _
    <Display(Name:="現在のパスワード")> _
    Public Property OldPassword As String

    <Required()> _
    <StringLength(100, ErrorMessage:="{0} の長さは {2} 文字以上である必要があります。", MinimumLength:=6)> _
    <DataType(DataType.Password)> _
    <Display(Name:="新しいパスワード")> _
    Public Property NewPassword As String

    <DataType(DataType.Password)> _
    <Display(Name:="新しいパスワードの確認入力")> _
    <Compare("NewPassword", ErrorMessage:="新しいパスワードと確認のパスワードが一致しません。")> _
    Public Property ConfirmPassword As String
End Class

Public Class LoginModel
    <Required()> _
    <Display(Name:="ユーザー名")> _
    Public Property UserName As String

    <Required()> _
    <DataType(DataType.Password)> _
    <Display(Name:="パスワード")> _
    Public Property Password As String

    <Display(Name:="このアカウントを記憶する")> _
    Public Property RememberMe As Boolean
End Class

Public Class RegisterModel
    <Required()> _
    <Display(Name:="ユーザー名")> _
    Public Property UserName As String

    <Required()> _
    <StringLength(100, ErrorMessage:="{0} の長さは {2} 文字以上である必要があります。", MinimumLength:=6)> _
    <DataType(DataType.Password)> _
    <Display(Name:="パスワード")> _
    Public Property Password As String

    <DataType(DataType.Password)> _
    <Display(Name:="パスワードの確認入力")> _
    <Compare("Password", ErrorMessage:="パスワードと確認のパスワードが一致しません。")> _
    Public Property ConfirmPassword As String
End Class

Public Class ExternalLogin
    Public Property Provider As String
    Public Property ProviderDisplayName As String
    Public Property ProviderUserId As String
End Class
