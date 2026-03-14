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
    public partial class SuppliersForm : Form
    {
        private string _connectionString;

        public SuppliersForm()
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private void SuppliersForm_Load(object sender, EventArgs e)
        {
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        SuppliersID AS 'ID',
                        SuppliersName AS 'Название',
                        CONCAT(LEFT(Address,10),'...') AS 'Адрес',
                        CONCAT(LEFT(Phone,3),'****',RIGHT(Phone,2)) AS 'Номер'
                    FROM suppliers
                    WHERE is_active = 1
                    ORDER BY SuppliersName";

                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                DataTable table = new DataTable();
                adapter.Fill(table);

                dgvSuppliers.DataSource = table;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            SupplierEditForm frm = new SupplierEditForm();
            if (frm.ShowDialog() == DialogResult.OK)
                LoadSuppliers();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.CurrentRow == null)
            {
                MessageBox.Show("Выберите поставщика!");
                return;
            }

            int id = Convert.ToInt32(dgvSuppliers.CurrentRow.Cells["ID"].Value);
            SupplierEditForm frm = new SupplierEditForm(id);
            if (frm.ShowDialog() == DialogResult.OK)
                LoadSuppliers();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.CurrentRow == null)
            {
                MessageBox.Show("Выберите поставщика!");
                return;
            }

            int id = Convert.ToInt32(dgvSuppliers.CurrentRow.Cells["ID"].Value);
            if (MessageBox.Show("Удалить поставщика?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE `suppliers` SET is_active = 0 WHERE SuppliersID = @id";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                LoadSuppliers();
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            if (dgvSuppliers.CurrentRow == null)
            {
                MessageBox.Show("Выберите поставщика!");
                return;
            }

            int id = Convert.ToInt32(dgvSuppliers.CurrentRow.Cells["ID"].Value);

            SupplierDetailsForm frm = new SupplierDetailsForm(id);
            frm.ShowDialog();
        }
    }
}
