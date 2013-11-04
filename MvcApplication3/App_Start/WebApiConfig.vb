Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web.Http

Public Class WebApiConfig
    Public Shared Sub Register(ByVal config As HttpConfiguration)
        config.Routes.MapHttpRoute( _
            name:="DefaultApi", _
            routeTemplate:="api/{controller}/{id}", _
            defaults:=New With {.id = RouteParameter.Optional} _
        )
        
        'IQueryable または IQueryable(Of T) 戻り値の型を持つアクションのクエリのサポートを有効にするには、次のコード行のコメントを解除してください。
        '予期しないクエリまたは悪意のあるクエリの処理を避けるには、QueryableAttribute の検証設定を使用して受信するクエリを検証してください。
        '詳細については、http://go.microsoft.com/fwlink/?LinkId=279712 を参照してください。
        'config.EnableQuerySupport()
    End Sub
End Class