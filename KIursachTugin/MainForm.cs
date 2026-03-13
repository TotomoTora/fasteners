using KIursachTugin.Forms;
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

namespace KIursachTugin
{
    public partial class MainForm: Form
    {
        private User currentUser;

        public MainForm(User user)
        {
            InitializeComponent();
            currentUser = user;
            lblWelcome.Text = $"{user.UserName} {user.UserPatronymic} ({user.RoleName})";

            if (user.RoleID == 3)
            {
                поставщикиToolStripMenuItem.Visible = false;
                пользователиToolStripMenuItem.Visible = false;
                статистикаToolStripMenuItem.Visible = false;
                категорииToolStripMenuItem.Visible = false;

            }
            else if (user.RoleID == 2)
            {
                пользователиToolStripMenuItem.Visible = false;

            }
            else if (user.RoleID == 1)
            {
                заказыToolStripMenuItem.Visible = false;
                поставщикиToolStripMenuItem.Visible = false;
                категорииToolStripMenuItem.Visible = false;
            }
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void товарыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms
            .Cast<Form>()
            .FirstOrDefault(f => f is ProductsForm);

            if (existingForm != null)
            {
                existingForm.Activate(); // вывести на передний план
                return;
            }

            ProductsForm form = new ProductsForm();
            form.Show();

        }

        private void заказыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms
            .Cast<Form>()
            .FirstOrDefault(f => f is OrdersForm);

            if (existingForm != null)
            {
                existingForm.Activate(); // вывести на передний план
                return;
            }

            OrdersForm form = new OrdersForm();
            form.Show();
        }

        private void сменаПользователяToolStripMenuItem_Click(object sender, EventArgs e)
        {

            LoginForm loginForm = new LoginForm();
            loginForm.Show();
            this.Close();

        }

        private void пользователиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms
            .Cast<Form>()
            .FirstOrDefault(f => f is UsersForm);

            if (existingForm != null)
            {
                existingForm.Activate(); // вывести на передний план
                return;
            }

            UsersForm form = new UsersForm();
            form.Show();
        }

        private void поставщикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms
            .Cast<Form>()
            .FirstOrDefault(f => f is SuppliersForm);

            if (existingForm != null)
            {
                existingForm.Activate(); // вывести на передний план
                return;
            }

            SuppliersForm form = new SuppliersForm();
            form.Show();
        }

        private void статистикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms
            .Cast<Form>()
            .FirstOrDefault(f => f is StatisticsForm);

            if (existingForm != null)
            {
                existingForm.Activate(); // вывести на передний план
                return;
            }

            StatisticsForm form = new StatisticsForm();
            form.Show();
        }

        private void категорииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form existingForm = Application.OpenForms
            .Cast<Form>()
            .FirstOrDefault(f => f is CategoriesForm);

            if (existingForm != null)
            {
                existingForm.Activate(); // вывести на передний план
                return;
            }

            CategoriesForm form = new CategoriesForm();
            form.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
