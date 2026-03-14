using System;
using KIursachTugin.DataAccess;
using KIursachTugin.Models;
using KIursachTugin.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;

namespace KIursachTugin
{
    public partial class ProductsForm : Form
    {
        private ProductRepository repo;
        private string _connectionString;

        private int pageSize = 4;      // товаров на странице
        private int currentPage = 1;   // текущая страница
        private int totalRows = 0;     // всего товаров
        private int totalPages = 0;    // всего страниц


        public ProductsForm()
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            InitializeComponent();
            repo = new ProductRepository();
        }


        private void ProductsForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadProducts();
            ApplyRoleAccess();
        }
        private void ApplyRoleAccess()
        {
            bool isSeller = Session.RoleName == "Продавец";

            btnAdd.Visible = !isSeller;
            btnEdit.Visible = !isSeller;
            btnDelete.Visible = !isSeller;

            btnAdd.Enabled = !isSeller;
            btnEdit.Enabled = !isSeller;
            btnDelete.Enabled = !isSeller;
        }

        private void LoadCategories()
        {

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT CategoriesName, CategoriesID FROM categories WHERE is_active = 1";
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                DataTable table = new DataTable();
                DataRow allRow = table.NewRow();
                adapter.Fill(table);
                cbCategory.DataSource = table;
                allRow["CategoriesName"] = "Все категории";
                table.Rows.InsertAt(allRow, 0);
                cbCategory.DisplayMember = "CategoriesName";
                cbCategory.ValueMember = "CategoriesID";
                cbCategory.SelectedIndex = -1;
            }
        }

        private void LoadProducts(string search = "", int categoryId = 0)
        {
            string connStr = System.Configuration.ConfigurationManager
                .ConnectionStrings["DefaultConnection"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                // считаем количество товаров
                string countSql = @"
        SELECT COUNT(*)
        FROM products p
        WHERE p.is_active = 1
        AND (p.Name LIKE @s OR p.Description LIKE @s)
        AND (@cat = 0 OR p.CategoriesID = @cat)";

                using (MySqlCommand countCmd = new MySqlCommand(countSql, conn))
                {
                    countCmd.Parameters.AddWithValue("@s", "%" + search + "%");
                    countCmd.Parameters.AddWithValue("@cat", categoryId);

                    totalRows = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                totalPages = (int)Math.Ceiling((double)totalRows / pageSize);

                int offset = (currentPage - 1) * pageSize;

                string sql = @"
        SELECT p.ProductID AS 'ID',
               p.Name AS 'Название',
               p.Price AS 'Цена',
               p.Quantity AS 'Кол-во',
               c.CategoriesName AS 'Категория',
               s.SuppliersName AS 'Поставщик',
               p.Description AS 'Описание',
               p.ImagePath
        FROM products p
        LEFT JOIN categories c ON p.CategoriesID = c.CategoriesID
        LEFT JOIN suppliers s ON p.SuppliersID = s.SuppliersID
        WHERE p.is_active = 1
        AND (p.Name LIKE @s OR p.Description LIKE @s)
        AND (@cat = 0 OR p.CategoriesID = @cat)
        ORDER BY p.Name
        LIMIT @limit OFFSET @offset";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@s", "%" + search + "%");
                    cmd.Parameters.AddWithValue("@cat", categoryId);
                    cmd.Parameters.AddWithValue("@limit", pageSize);
                    cmd.Parameters.AddWithValue("@offset", offset);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    if (!table.Columns.Contains("Фото"))
                        table.Columns.Add("Фото", typeof(Image));

                    foreach (DataRow row in table.Rows)
                    {
                        string imagePath = row["ImagePath"]?.ToString();
                        string fullPath = Path.Combine(Application.StartupPath, imagePath ?? "");

                        if (!string.IsNullOrEmpty(imagePath) && File.Exists(fullPath))
                        {
                            using (var img = Image.FromFile(fullPath))
                                row["Фото"] = new Bitmap(img, new Size(80, 80));
                        }
                        else
                        {
                            string defaultPath = Path.Combine(Application.StartupPath, "Images/no_image.png");
                            if (File.Exists(defaultPath))
                            {
                                using (var img = Image.FromFile(defaultPath))
                                    row["Фото"] = new Bitmap(img, new Size(80, 80));
                            }
                        }
                    }

                    dgvProducts.DataSource = table;

                    if (dgvProducts.Columns.Contains("ImagePath"))
                        dgvProducts.Columns["ImagePath"].Visible = false;

                    if (dgvProducts.Columns.Contains("Фото"))
                    {
                        DataGridViewImageColumn imgCol =
                            (DataGridViewImageColumn)dgvProducts.Columns["Фото"];

                        imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                    }

                    dgvProducts.RowTemplate.Height = 80;

                    lblPage.Text = $"Страница {currentPage} из {totalPages}";
                }
            }
        }

        private void ApplyFilters(bool resetPage = true)
        {
            string search = txtSearch.Text.Trim();
            int categoryId = 0;

            if (cbCategory.SelectedValue != null && cbCategory.SelectedValue is int)
                categoryId = (int)cbCategory.SelectedValue;

            if (resetPage)
                currentPage = 1; // сброс страницы только при новом фильтре

            LoadProducts(search, categoryId);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters(resetPage: true); // сброс на первую страницу при поиске
        }
        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters(resetPage: true); // сброс на первую страницу при выборе категории
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cbCategory.SelectedIndex = 0; // "Все"
            ApplyFilters();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ProductEditForm form = new ProductEditForm();
            if (form.ShowDialog() == DialogResult.OK)
                ApplyFilters();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvProducts.CurrentRow.Cells["ID"].Value);
            ProductEditForm form = new ProductEditForm(id);
            if (form.ShowDialog() == DialogResult.OK)
                ApplyFilters();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvProducts.CurrentRow.Cells["ID"].Value);
            DialogResult result = MessageBox.Show(
                "Удалить выбранный товар?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                string connStr = System.Configuration.ConfigurationManager
                    .ConnectionStrings["DefaultConnection"].ConnectionString;

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "UPDATE products SET is_active = 0 WHERE ProductID = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                ApplyFilters();
            }
        }
        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                !((e.KeyChar >= 'А' && e.KeyChar <= 'Я') || (e.KeyChar >= 'а' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё') &&
                !((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z')))
            {
                e.Handled = true;
            }
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                ApplyFilters(resetPage: false);
            }
        }
        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                ApplyFilters(resetPage: false);
            }
        }
    }
}