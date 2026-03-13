using KIursachTugin.DataAccess;
using KIursachTugin.Models;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KIursachTugin
{
    public partial class LoginForm : Form
    {
        private int failedAttempts = 0;
        private readonly UserRepository userRepo;
        private Size defaultSize; // исходный размер формы

        public LoginForm()
        {
            InitializeComponent();
            userRepo = new UserRepository();

            defaultSize = this.Size; // сохраняем исходный размер

            // Скрываем CAPTCHA изначально
            picCaptcha.Visible = false;
            txtCaptcha.Visible = false;
            btnRefreshCaptcha.Visible = false;
        }

        public static class CurrentUser
        {
            public static int Id;
            public static string Login;
            public static string Role;
        }

        private async void btnLogin_Click_1(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            try
            {
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    lblError.Text = "Введите логин и пароль!";
                    return;
                }

                User user = userRepo.GetUserByLoginAndPassword(login, password);

                // Проверяем CAPTCHA если это не первая попытка
                if (failedAttempts > 0)
                {
                    if (txtCaptcha.Text != CaptchaGenerator.CurrentCaptcha)
                    {
                        MessageBox.Show("Неверная CAPTCHA!");
                        failedAttempts++;
                        await BlockLoginAsync();
                        RefreshCaptcha();
                        return;
                    }
                }

                if (user != null)
                {
                    // Сохраняем данные пользователя
                    Session.CurrentUser = user;
                    CurrentUser.Id = user.Id;
                    CurrentUser.Login = user.Login;
                    CurrentUser.Role = user.RoleName;
                    Session.UserID = user.Id;
                    Session.RoleName = user.RoleName;

                    // Скрываем форму и открываем главную
                    HideCaptcha(); // возвращаем форму к изначальному размеру
                    this.Hide();
                    MainForm mainForm = new MainForm(user);
                    mainForm.Show();

                    // Старт IdleManager
                    IdleManager.Start();
                }
                else
                {
                    failedAttempts++;

                    if (failedAttempts == 1)
                    {
                        lblError.Text = "Неверный логин или пароль.";

                        ShowCaptcha(); // показываем CAPTCHA и увеличиваем форму
                        RefreshCaptcha();
                    }
                    else
                    {
                        MessageBox.Show("Повторите ввод логина и пароля!");
                        txtLogin.Clear();
                        txtPassword.Clear();
                        await BlockLoginAsync();
                        RefreshCaptcha();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка подключения к БД");
            }
        }

        private void RefreshCaptcha()
        {
            picCaptcha.Image?.Dispose();
            picCaptcha.Image = CaptchaGenerator.GenerateCaptcha(picCaptcha.Width, picCaptcha.Height);
            txtCaptcha.Clear();
        }

        private async Task BlockLoginAsync()
        {
            btnLogin.Enabled = false;
            int seconds = 10;
            for (int i = seconds; i > 0; i--)
            {
                btnLogin.Text = $"Попробуйте через {i}s";
                await Task.Delay(1000);
            }
            btnLogin.Text = "Войти";
            btnLogin.Enabled = true;
        }

        private void btnRefreshCaptcha_Click(object sender, EventArgs e)
        {
            RefreshCaptcha();
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
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                !((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z')))
            {
                e.Handled = true; // блокируем ввод
            }
        }

        // Показ CAPTCHA и увеличение формы
        private void ShowCaptcha()
        {
            picCaptcha.Visible = true;
            txtCaptcha.Visible = true;
            btnRefreshCaptcha.Visible = true;

            int extraHeight = picCaptcha.Height + txtCaptcha.Height + 20;
            this.Size = new Size(this.Width, defaultSize.Height + extraHeight);
        }

        // Скрытие CAPTCHA и возврат формы к исходному размеру
        private void HideCaptcha()
        {
            picCaptcha.Visible = false;
            txtCaptcha.Visible = false;
            btnRefreshCaptcha.Visible = false;

            this.Size = defaultSize;
        }
    }
}