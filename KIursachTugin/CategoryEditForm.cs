using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace KIursachTugin
{
    public partial class CategoryEditForm : Form
    {
        private readonly string _connectionString;
        private readonly int? _categoryId;

        public CategoryEditForm(int? categoryId = null)
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _categoryId = categoryId;
        }

        private void CategoryEditForm_Load(object sender, EventArgs e)
        {
            if (_categoryId.HasValue)
                LoadCategoryData();
        }

        private void LoadCategoryData()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT CategoriesName FROM categories WHERE CategoriesID = @id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", _categoryId);
                object result = cmd.ExecuteScalar();
                if (result != null)
                    txtName.Text = result.ToString();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название категории!");
                return;
            }

          
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql;

                if (_categoryId.HasValue)
                {
                sql = "UPDATE categories SET CategoriesName=@name WHERE CategoriesID=@id";
                }
                else
                {
                    sql = "INSERT INTO categories (CategoriesName) VALUES (@name)";
                }

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());

                if (_categoryId.HasValue)
                    cmd.Parameters.AddWithValue("@id", _categoryId);

                cmd.ExecuteNonQuery();
                }
            

            MessageBox.Show("Категория сохранена!");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !((e.KeyChar >= 'А' && e.KeyChar <= 'Я') || (e.KeyChar >= 'а' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё'))
            {
                e.Handled = true; // блокируем ввод
            }
        }
        private void txtName_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
