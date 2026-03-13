using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KIursachTugin
{
    public partial class UserEditForm : Form
    {
        private readonly string _connectionString;
        private readonly int? _userId;

        public UserEditForm(int? userId = null)
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _userId = userId;
        }

        private void UserEditForm_Load(object sender, EventArgs e)
        {
            LoadRoles();
            if (_userId.HasValue)
                LoadUserData();
        }
        private void LoadRoles()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT RoleID, RoleName FROM role ORDER BY RoleName";
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                cbRole.DataSource = table;
                cbRole.DisplayMember = "RoleName";
                cbRole.ValueMember = "RoleID";

            }
        }

        private void LoadUserData()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM user WHERE UserID = @id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", _userId);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        txtLogin.Text = Convert.ToString(r["UserLogin"]);
                        txtPassword.Text = Convert.ToString(r["UserPassword"]);
                        txtSurname.Text = Convert.ToString(r["UserSurname"]);
                        txtName.Text = Convert.ToString(r["UserName"]);
                        txtPatronymic.Text = Convert.ToString(r["UserPatronymic"]);
                        if (r["RoleID"] != DBNull.Value)
                            cbRole.SelectedValue = Convert.ToInt32(r["RoleID"]);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                MessageBox.Show("Введите логин!");
                return;
            }

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql;

                if (_userId.HasValue)
                {
                    sql = @"UPDATE user 
                            SET UserLogin=@login, UserPassword=@pass, 
                                UserSurname=@surname, UserName=@name, UserPatronymic=@patr, 
                                RoleID=@role 
                            WHERE UserID=@id";
                }
                else
                {
                    sql = @"INSERT INTO user (UserLogin, UserPassword, UserSurname, UserName, UserPatronymic, RoleID)
                            VALUES (@login, @pass, @surname, @name, @patr, @role)";
                }

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@login", txtLogin.Text.Trim());
                cmd.Parameters.AddWithValue("@pass", txtPassword.Text.Trim());
                cmd.Parameters.AddWithValue("@surname", txtSurname.Text.Trim());
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@patr", txtPatronymic.Text.Trim());
                cmd.Parameters.AddWithValue("@role", cbRole.SelectedValue ?? DBNull.Value);

                if (_userId.HasValue)
                    cmd.Parameters.AddWithValue("@id", _userId);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Пользователь сохранён!");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txtLogin_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z')))
            {
                e.Handled = true; // блокируем ввод
            }
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !((e.KeyChar >= 'А' && e.KeyChar <= 'Я') || (e.KeyChar >= 'а' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё'))
            {
                e.Handled = true; // блокируем ввод
            }
        }
    }
}
