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
using System.Xml.Linq;

namespace KIursachTugin
{
    public partial class SupplierEditForm : Form
    {
        private string _connectionString;
        private int? _supplierId;

        public SupplierEditForm(int? supplierId = null)
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _supplierId = supplierId;
        }

        private void SupplierEditForm_Load(object sender, EventArgs e)
        {
            if (_supplierId.HasValue)
                LoadSupplierData();
        }

        private void LoadSupplierData()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT SuppliersName, Address, Phone FROM suppliers WHERE SuppliersID = @id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", _supplierId);
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                    {
                        txtName.Text = Convert.ToString(r["SuppliersName"]);
                        txtAddress.Text = Convert.ToString(r["Address"]);
                        maskedTextBox1.Text = Convert.ToString(r["Phone"]);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название поставщика!");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql;

                if (_supplierId.HasValue)
                {
                    sql = @"UPDATE suppliers 
                            SET SuppliersName=@name, Address=@address, Phone=@phone 
                            WHERE SuppliersID=@id";
                }
                else
                {
                    sql = @"INSERT INTO suppliers (SuppliersName, Address, Phone) 
                            VALUES (@name, @address, @phone)";
                }

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
                cmd.Parameters.AddWithValue("@phone", maskedTextBox1.Text.Trim());

                if (_supplierId.HasValue)
                    cmd.Parameters.AddWithValue("@id", _supplierId);

                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("Поставщик сохранён!");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void textname_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                !((e.KeyChar >= 'А' && e.KeyChar <= 'Я') || (e.KeyChar >= 'а' && e.KeyChar <= 'я') || e.KeyChar == 'Ё' || e.KeyChar == 'ё') &&
                !((e.KeyChar >= 'A' && e.KeyChar <= 'Z') || (e.KeyChar >= 'a' && e.KeyChar <= 'z')))
            {
                e.Handled = true;
            }
        }
        
        private void txtPhone_TextChanged(object sender, EventArgs e)
        {

        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }
    }
}
