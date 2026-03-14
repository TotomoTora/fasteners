using System;
using System.Linq;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows.Forms;
using KIursachTugin.DataAccess;
using KIursachTugin.Models;


namespace KIursachTugin
{
    public static class IdleManager
    {
        private static Timer idleTimer;
        private static int idleSeconds = 0;
        private static int idleLimit;


        // Флаг, чтобы MessageFilter добавлялся только один раз

        private static bool filterAdded = false;

        public static void Start()
        {
            idleLimit = int.Parse(ConfigurationManager.AppSettings["IdleTimeoutSeconds"]);

            if (idleTimer != null)
            {
                idleTimer.Stop();
                idleTimer.Dispose();
                idleTimer = null;
            }

            idleSeconds = 0;

            idleTimer = new Timer();
            idleTimer.Interval = 1000;
            idleTimer.Tick += CheckIdle;
            idleTimer.Start();

            if (!filterAdded)
            {
                Application.AddMessageFilter(new ActivityMessageFilter());
                filterAdded = true;
            }
        }

        private static void CheckIdle(object sender, EventArgs e)
        {
            idleSeconds++;

            if (idleSeconds >= idleLimit)
            {
                idleTimer.Stop();
                MessageBox.Show(
                    "Система была заблокирована из-за отсутствия активности.",
                    "Блокировка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LockApplication();
            }
        }

        public static void ResetTimer()
        {
            idleSeconds = 0;
            if (idleTimer != null && !idleTimer.Enabled)
                idleTimer.Start();
        }

        private static void LockApplication()
        {
            // Закрываем все формы кроме LoginForm
            foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
            {
                if (!(form is LoginForm))
                    form.Close();
            }
            LoginForm loginForm = new LoginForm();
            loginForm.Show();

            // Перезапускаем таймер после показа формы авторизации
            Start();
        }
    }

    class ActivityMessageFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
            const int WM_MOUSEMOVE = 0x0200;
            const int WM_KEYDOWN = 0x0100;
            const int WM_LBUTTONDOWN = 0x0201;

            if (m.Msg == WM_MOUSEMOVE || m.Msg == WM_KEYDOWN || m.Msg == WM_LBUTTONDOWN)
            {
                IdleManager.ResetTimer();
            }

            return false;
        }
    }
}