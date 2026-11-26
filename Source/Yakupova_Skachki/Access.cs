using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows;

namespace Yakupova_Skachki
{
    public partial class Login : Window
    {
        internal static bool IsAdmin { get; private set; }
        internal static bool CanRecordResults { get; private set; }
        public Login() { InitializeComponent(); Username.Focus(); }

        private void Enter(object sender, RoutedEventArgs args)
        {
            CanRecordResults = IsAdmin = false;
            try
            {
                using (var db = new Yakupova_Skachki3Entities())
                using (var cmd = db.Database.Connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Salt, PasswordHash, IsAdmin, RecordsResults FROM dbo.AppAccounts WHERE Login=@login";
                    cmd.Parameters.Add(new SqlParameter("@login", SqlDbType.NVarChar, 60) { Value = Username.Text.Trim() });
                    db.Database.Connection.Open();
                    using (var row = cmd.ExecuteReader())
                    {
                        bool valid = false;
                        if (Username.Text.Trim().Length <= 60 && row.Read())
                        {
                            using (var derive = new Rfc2898DeriveBytes(Secret.Password, (byte[])row[0], 210000, HashAlgorithmName.SHA256))
                            {
                                byte[] actual = derive.GetBytes(32), expected = (byte[])row[1];
                                int diff = 0;
                                for (int i = 0; i < actual.Length; i++) diff |= actual[i] ^ expected[i];
                                valid = diff == 0;
                            }
                            IsAdmin = valid && row.GetBoolean(2);
                            CanRecordResults = IsAdmin || (valid && row.GetBoolean(3));
                        }
                        if (!valid) { Notice.Text = "Проверьте логин и пароль."; Secret.Clear(); Secret.Focus(); return; }
                    }
                }
                var home = new MainWindow();
                Application.Current.MainWindow = home;
                home.Show();
                Close();
            }
            catch (Exception) { CanRecordResults = IsAdmin = false; Notice.Text = "Не удалось загрузить данные. Проверьте подключение к базе и установку обновления протокола."; }
        }
    }
}
