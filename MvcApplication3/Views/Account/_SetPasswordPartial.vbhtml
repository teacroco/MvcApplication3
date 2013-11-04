@ModelType MvcApplication3.LocalPasswordModel

<p>
    このサイトのローカル パスワードがありません。外部ログイン
なしでログインできるように、ローカル パスワードを追加してください。
</p>

@Using Html.BeginForm("Manage", "Account")
    @Html.AntiForgeryToken()
    @Html.ValidationSummary()

    @<fieldset>
        <legend>パスワードの設定フォーム</legend>
        <ol>
            <li>
                @Html.LabelFor(Function(m) m.NewPassword)
                @Html.PasswordFor(Function(m) m.NewPassword)
            </li>
            <li>
                @Html.LabelFor(Function(m) m.ConfirmPassword)
                @Html.PasswordFor(Function(m) m.ConfirmPassword)
            </li>
        </ol>
        <input type="submit" value="パスワードの設定" />
    </fieldset>
End Using
