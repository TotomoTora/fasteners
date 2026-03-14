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
using static KIursachTugin.LoginForm;

namespace KIursachTugin
{
    public partial class UsersForm : Form
    {
        private readonly string _connectionString;

        public UsersForm()
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private void UsersForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT u.UserID,
                           u.UserLogin AS 'Логин',
                    CONCAT(
                            LEFT(u.UserName,1),'******',
                            LEFT(u.UserSurname,1),'******',
                            LEFT(u.UserPatronymic,1),'******'
                            ) AS 'ФИО',
                    r.RoleName AS 'Роль'
                    FROM user u
                    LEFT JOIN role r ON u.RoleID = r.RoleID
                    WHERE u.is_active = 1
                    ORDER BY u.UserLogin";

                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgvUsers.DataSource = table;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            UserEditForm frm = new UserEditForm();
            if (frm.ShowDialog() == DialogResult.OK)
                LoadUsers();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя!");
                return;
            }

            int id = Convert.ToInt32(dgvUsers.CurrentRow.Cells["UserID"].Value);
            UserEditForm frm = new UserEditForm(id);
            if (frm.ShowDialog() == DialogResult.OK)
                LoadUsers();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя!");
                return;
            }

            int id = Convert.ToInt32(dgvUsers.CurrentRow.Cells["UserID"].Value);
       
            if (id == CurrentUser.Id)
            {
                MessageBox.Show(
                    "Нельзя удалить пользователя, под которым выполнен вход.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(
                    "Пользователь будет деактивирован.\nПродолжить?",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "UPDATE `user` SET is_active = 0 WHERE UserID = @id";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            LoadUsers();
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null)
            {
                MessageBox.Show("Выберите пользователя!");
                return;
            }

            int id = Convert.ToInt32(dgvUsers.CurrentRow.Cells["UserID"].Value);

            UserDetailsForm frm = new UserDetailsForm(id);
            frm.ShowDialog();
        }
    }
}