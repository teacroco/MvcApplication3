@ModelType ICollection(Of MvcApplication3.ExternalLogin)

@If Model.Count > 0 Then
    @<h3>登録されている外部ログイン</h3>
    @<table>
        <tbody>
        @For Each externalLogin As MvcApplication3.ExternalLogin In Model
            @<tr>
                <td>@externalLogin.ProviderDisplayName</td>
                <td>
                    @If ViewData("ShowRemoveButton") Then
                            Using Html.BeginForm("Disassociate", "Account")
                            @Html.AntiForgeryToken()
                            @<div>
                                @Html.Hidden("provider", externalLogin.Provider)
                                @Html.Hidden("providerUserId", externalLogin.ProviderUserId)
                                <input type="submit" value="削除" title="この @externalLogin.ProviderDisplayName 資格情報をアカウントから削除" />
                            </div>
                        End Using
                    Else
                        @: &nbsp;
                    End If
                </td>
            </tr>
        Next
        </tbody>
    </table>
End If