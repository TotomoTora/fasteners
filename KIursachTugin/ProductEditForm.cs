using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KIursachTugin.DataAccess;
using KIursachTugin.Models;
using MySql.Data.MySqlClient;

namespace KIursachTugin
{
    public partial class ProductEditForm : Form
    {
        private string currentImagePath = null;
        private readonly ProductRepository _repository;
        private int? productId;
        private string _connectionString;

        public ProductEditForm(int? id = null)
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["DefaultConnection"].ConnectionString;

            _repository = new ProductRepository();
            productId = id;
        }

        private void ProductEditForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadSuppliers();

            if (productId.HasValue)
                LoadProduct(productId.Value);
            else
                LoadDefaultImage();
        }

        private void LoadCategories()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT CategoriesName, CategoriesID FROM categories WHERE is_active = 1";
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);

                cbCategory.DataSource = table;
                cbCategory.DisplayMember = "CategoriesName";
                cbCategory.ValueMember = "CategoriesID";
                cbCategory.SelectedIndex = -1;
            }
        }

        private void LoadSuppliers()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT SuppliersID, SuppliersName FROM suppliers WHERE is_active = 1";
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);

                cbSupplier.DataSource = table;
                cbSupplier.DisplayMember = "SuppliersName";
                cbSupplier.ValueMember = "SuppliersID";
                cbSupplier.SelectedIndex = -1;
            }
        }

        private void LoadProduct(int id)
        {
            Product p = _repository.GetProductById(id);
            if (p == null) return;

            txtName.Text = p.Name;
            nudPrice.Value = p.Price;
            nudQuantity.Value = p.Quantity;
            txtDescription.Text = p.Description;

            if (p.CategoriesID.HasValue)
                cbCategory.SelectedValue = p.CategoriesID.Value;

            if (p.SuppliersID.HasValue)
                cbSupplier.SelectedValue = p.SuppliersID.Value;

            currentImagePath = p.ImagePath;
            LoadImage(currentImagePath);
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string imagesFolder = Path.Combine(Application.StartupPath, "Images");

                if (!Directory.Exists(imagesFolder))
                    Directory.CreateDirectory(imagesFolder);

                // Удаляем старое изображение
                if (!string.IsNullOrEmpty(currentImagePath))
                {
                    string oldPath = Path.Combine(Application.StartupPath, currentImagePath);
                    if (File.Exists(oldPath))
                        File.Delete(oldPath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ofd.FileName);
                string newPath = Path.Combine(imagesFolder, fileName);

                FileInfo fi = new FileInfo(ofd.FileName);
                // Ограничение 2 МБ
                if (fi.Length > 2 * 1024 * 1024)
                {
                    MessageBox.Show(
                        "Файл слишком большой. Максимальный размер — 2 МБ.",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                File.Copy(ofd.FileName, newPath, true);
                currentImagePath = "Images/" + fileName;

                using (var img = Image.FromFile(newPath))
                {
                    pbImage.Image = new Bitmap(img);
                }
            }
        }

        private void btnRemoveImage_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentImagePath))
            {
                string oldPath = Path.Combine(Application.StartupPath, currentImagePath);
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            currentImagePath = null;
            LoadDefaultImage();
        }

        private void LoadImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                LoadDefaultImage();
                return;
            }

            string fullPath = Path.Combine(Application.StartupPath, imagePath);

            if (File.Exists(fullPath))
            {
                using (var img = Image.FromFile(fullPath))
                {
                    pbImage.Image = new Bitmap(img);
                }
            }
            else
            {
                LoadDefaultImage();
            }
        }

        private void LoadDefaultImage()
        {
            string defaultPath = Path.Combine(Application.StartupPath, "Images/no_image.png");

            if (File.Exists(defaultPath))
            {
                using (var img = Image.FromFile(defaultPath))
                {
                    pbImage.Image = new Bitmap(img);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара!");
                return;
            }

            Product p = new Product
            {
                Name = txtName.Text.Trim(),
                Price = nudPrice.Value,
                Quantity = (int)nudQuantity.Value,
                CategoriesID = cbCategory.SelectedValue == null ? null : (int?)Convert.ToInt32(cbCategory.SelectedValue),
                SuppliersID = cbSupplier.SelectedValue == null ? null : (int?)Convert.ToInt32(cbSupplier.SelectedValue),
                Description = txtDescription.Text.Trim(),
                ImagePath = currentImagePath
            };

            if (productId.HasValue)
            {
                p.ProductID = productId.Value;
                _repository.UpdateProduct(p);
                MessageBox.Show("Товар обновлён!");
            }
            else
            {
                _repository.AddProduct(p);
                MessageBox.Show("Товар добавлен!");
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) &&
                e.KeyChar != ',' &&
                e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !((e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                  (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                  (e.KeyChar == 'Ё') || (e.KeyChar == 'ё') ||
                   e.KeyChar == ' '))
            {
                e.Handled = true;
            }
        }
    }
}