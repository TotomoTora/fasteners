using KIursachTugin.DataAccess;
using KIursachTugin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//connectionString="Server=10.207.106.12;Database=db103;Uid=user103;Pwd=hf32;SslMode=None;"

namespace KIursachTugin
{
    public partial class LoginForm: Form
    {
        private int failedAttempts = 0;
        private readonly UserRepository userRepo;

        public LoginForm()
        {
            InitializeComponent();
            userRepo = new UserRepository();
        }
        public static class CurrentUser
        {
            public static int Id;
            public static string Login;
            public static string Role;
        }

        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();
            try
            {
                User user = userRepo.GetUserByLoginAndPassword(login, password);


                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    lblError.Text = "Введите логин и пароль!";
                    return;
                }

                if (user != null)
                {
                    // Открытие главной формы
                    CurrentUser.Id = user.Id;
                    CurrentUser.Role = user.RoleName;

                    Session.UserID = user.Id;
                    Session.RoleName = user.RoleName;



                    MainForm mainForm = new MainForm(user);
                    this.Hide();
                    mainForm.Show();
                }
                else
                {
                    failedAttempts++;
                    if (failedAttempts == 1)
                    {
                        lblError.Text = "Неверный логин или пароль.";
                    }
                    else
                    {
                        MessageBox.Show("Повторите ввод логина и пароля!");
                        txtLogin.Clear();
                        txtPassword.Clear();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка подключения к Бд");
            }
        }
        

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private void txtLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && !((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z')))
            {
                e.Handled = true; // блокируем ввод
            }
        }
    }
}
