using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace KIursachTugin
{
    public partial class UserDetailsForm : Form
    {
        private int userId;
        private string connectionString;

        public UserDetailsForm(int id)
        {
            InitializeComponent();
            userId = id;
            connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"]
                .ConnectionString;
        }

        private void UserDetailsForm_Load(object sender, EventArgs e)
        {
            LoadUser();
        }

        private void LoadUser()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                SELECT u.UserLogin,
                       u.UserSurname,
                       u.UserName,
                       u.UserPatronymic,
                       r.RoleName
                FROM user u
                LEFT JOIN role r ON u.RoleID = r.RoleID
                WHERE u.UserID = @id";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblLogin.Text = "Логин: " + reader["UserLogin"].ToString();
                        lblSurname.Text = "Фамилия: " + reader["UserSurname"].ToString();
                        lblName.Text = "Имя: " + reader["UserName"].ToString();
                        lblPatronymic.Text = "Отчество: " + reader["UserPatronymic"].ToString();
                        lblRole.Text = "Роль: " + reader["RoleName"].ToString();
                    }
                }
            }
        }
    }
}
