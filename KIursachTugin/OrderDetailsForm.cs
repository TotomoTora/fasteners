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
    public partial class OrderDetailsForm : Form
    {
        private string _connectionString;
        private int _orderId;

        public OrderDetailsForm(int orderId)
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _orderId = orderId;
        }

        private void OrderDetailsForm_Load(object sender, EventArgs e)
        {
            LoadStatuses();
            LoadOrderInfo();
            LoadOrderItems();
        }

        private void LoadOrderInfo()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = @"SELECT o.OrderID, 
                                      u.UserLogin AS Customer, 
                                      o.Date, 
                                      o.TotalAmount, 
                                      o.Status
                               FROM orders o
                               LEFT JOIN user u ON o.UserID = u.UserID
                               WHERE o.OrderID = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", _orderId);

                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            lblOrderIdValue.Text = Convert.ToString(r["OrderID"]);
                            lblUserValue.Text = Convert.ToString(r["Customer"]);
                            lblDateValue.Text = Convert.ToDateTime(r["Date"]).ToString("dd.MM.yyyy HH:mm");
                            if (r["TotalAmount"] != DBNull.Value)
                                lblTotalValue.Text = Convert.ToDecimal(r["TotalAmount"]).ToString("0.00") + " ₽";
                            else
                                lblTotalValue.Text = "0.00 ₽";
                            cbStatus.SelectedItem = Convert.ToString(r["Status"]);
                        }
                    }
                }
            }
        }

        private void LoadOrderItems()
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = @"SELECT p.Name AS 'Товар',
                                      oi.Price AS 'Цена',
                                      oi.Quantity AS 'Количество',
                                      (oi.Price * oi.Quantity) AS 'Сумма'
                               FROM order_items oi
                               JOIN products p ON oi.ProductID = p.ProductID
                               WHERE oi.OrderID = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", _orderId);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dgvItems.DataSource = table;
                }
            }
        }
        private void LoadStatuses()
        {
            cbStatus.Items.Clear();
            cbStatus.Items.Add("Новый");
            cbStatus.Items.Add("В обрабтке");
            cbStatus.Items.Add("Выполнен");
            cbStatus.Items.Add("Отменен");
        }

        private void btnChangeStatus_Click(object sender, EventArgs e)
        {
            if (cbStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите новый статус!");
                return;
            }
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = "UPDATE orders SET Status = @status WHERE OrderID = @id";
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", cbStatus.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@id", _orderId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Статус обновлён!");
            this.Close();
        }
    }
}
