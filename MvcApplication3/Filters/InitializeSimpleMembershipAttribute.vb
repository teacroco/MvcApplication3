Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Threading
Imports WebMatrix.WebData

<AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
Public NotInheritable Class InitializeSimpleMembershipAttribute
    Inherits ActionFilterAttribute

    Private Shared _initializer As SimpleMembershipInitializer
    Private Shared _initializerLock As New Object
    Private Shared _isInitialized As Boolean

    Public Overrides Sub OnActionExecuting(filterContext As ActionExecutingContext)
        ' アプリを起動するごとに ASP.NET Simple Membership が一度だけ初期化されるようにします
        LazyInitializer.EnsureInitialized(_initializer, _isInitialized, _initializerLock)
    End Sub

    Private Class SimpleMembershipInitializer
        Public Sub New()
            Database.SetInitializer(Of UsersContext)(Nothing)

            Try
                Using context As New UsersContext()
                    If Not context.Database.Exists() Then
                        ' Entity Framework 移行スキーマを使用しないで SimpleMembership データベースを作成します
                        CType(context, IObjectContextAdapter).ObjectContext.CreateDatabase()
                    End If
                End Using

                WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables:=True)
            Catch ex As Exception
                Throw New InvalidOperationException("ASP.NET Simple Membership データベースを初期化できませんでした。詳細については、http://go.microsoft.com/fwlink/?LinkId=256588 を参照してください。", ex)
            End Try
        End Sub
    End Class
End Class
