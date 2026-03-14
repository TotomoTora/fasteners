using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace KIursachTugin
{
    public partial class SupplierDetailsForm : Form
    {
        private int supplierId;
        private string connectionString;

        public SupplierDetailsForm(int id)
        {
            InitializeComponent();
            supplierId = id;
            connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"]
                .ConnectionString;
        }

        private void SupplierDetailsForm_Load(object sender, EventArgs e)
        {
            LoadSupplier();
        }

        private void LoadSupplier()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
                SELECT SuppliersName, Address, Phone
                FROM suppliers
                WHERE SuppliersID = @id";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", supplierId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lblName.Text = reader["SuppliersName"].ToString();
                        lblAddress.Text = reader["Address"].ToString();
                        lblPhone.Text = reader["Phone"].ToString();
                    }
                }
            }
        }
    }
}