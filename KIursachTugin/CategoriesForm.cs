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
    public partial class CategoriesForm : Form
    {
        private readonly string _connectionString;

        public CategoriesForm()
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private void CategoriesForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
        }

        private void LoadCategories()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT CategoriesID AS 'ID', CategoriesName AS 'Название' FROM categories WHERE is_active = 1 ORDER BY CategoriesName";
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);
                dgvCategories.DataSource = table;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            CategoryEditForm frm = new CategoryEditForm();
            if (frm.ShowDialog() == DialogResult.OK)
                LoadCategories();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow == null)
            {
                MessageBox.Show("Выберите категорию!");
                return;
            }

            int id = Convert.ToInt32(dgvCategories.CurrentRow.Cells["ID"].Value);
            CategoryEditForm frm = new CategoryEditForm(id);
            if (frm.ShowDialog() == DialogResult.OK)
                LoadCategories();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvCategories.CurrentRow == null)
            {
                MessageBox.Show("Выберите категорию!");
                return;
            }

            int id = Convert.ToInt32(dgvCategories.CurrentRow.Cells["ID"].Value);
            if (MessageBox.Show("Удалить выбранную категорию?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE `categories` SET is_active = 0 WHERE CategoriesID = @id";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }

                LoadCategories();
            }
        }
    }
}