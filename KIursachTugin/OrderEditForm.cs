using KIursachTugin.Models;
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
    public partial class OrderEditForm : Form
    {
        private string _connectionString;
        private DataTable cartTable;

        public OrderEditForm()
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private void OrderEditForm_Load(object sender, EventArgs e)
        {
            LoadProducts();        

            txtSeller.Text = Session.RoleName + " — ID: " + Session.UserID;
            LoadProducts();
            InitializeCartTable();
            dgvCart.Columns["Количество"].ReadOnly = false;
        }

        private void LoadProducts(string search = "")
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = @"
                    SELECT ProductID,
                           Name AS 'Товар',
                           Price AS 'Цена',
                           Quantity AS 'Остаток'
                    FROM products
                    WHERE is_active = 1
                    AND Name LIKE @s
                    ORDER BY Name";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@s", "%" + search + "%");

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    dgvProductsList.DataSource = table;
                }
            }
        }
        private void AddProductToCart(int id, string name, decimal price)
        {
            DataRow existing = cartTable.Rows
                .Cast<DataRow>()
                .FirstOrDefault(r => (int)r["ProductID"] == id);

            int stock = Convert.ToInt32(dgvProductsList.CurrentRow.Cells["Остаток"].Value);

            if (existing != null)
                existing["Количество"] = (int)existing["Количество"] + 1;
            else
                cartTable.Rows.Add(id, name, price, 1);

            UpdateTotal();
        }

        private void InitializeCartTable()
        {
            cartTable = new DataTable();
            cartTable.Columns.Add("ProductID", typeof(int));
            cartTable.Columns.Add("Название", typeof(string));
            cartTable.Columns.Add("Цена", typeof(decimal));
            cartTable.Columns.Add("Количество", typeof(int));
            cartTable.Columns.Add("Сумма", typeof(decimal), "Цена * Количество");

            dgvCart.DataSource = cartTable;
            foreach (DataGridViewColumn col in dgvCart.Columns)
                col.ReadOnly = true;

            dgvCart.Columns["Количество"].ReadOnly = false;
        }
        private void dgvCart_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvCart.CurrentCell.ColumnIndex == dgvCart.Columns["Количество"].Index)
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                {
                    tb.KeyPress -= Qty_KeyPress;
                    tb.KeyPress += Qty_KeyPress;
                }
            }
        }

        private void Qty_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }
        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (dgvProductsList.CurrentRow == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }


            int id = Convert.ToInt32(dgvProductsList.CurrentRow.Cells["ProductID"].Value);
            string name = dgvProductsList.CurrentRow.Cells["Товар"].Value.ToString();
            decimal price = Convert.ToDecimal(dgvProductsList.CurrentRow.Cells["Цена"].Value);
            int stock = Convert.ToInt32(dgvProductsList.CurrentRow.Cells["Остаток"].Value);

            AddProductToCart(id, name, price);
        }
        private void dgvProductsList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            int id = Convert.ToInt32(dgvProductsList.Rows[e.RowIndex].Cells["ProductID"].Value);
            string name = dgvProductsList.Rows[e.RowIndex].Cells["Товар"].Value.ToString();
            decimal price = Convert.ToDecimal(dgvProductsList.Rows[e.RowIndex].Cells["Цена"].Value);

            AddProductToCart(id, name, price);
        }
        private void btnRemoveFromCart_Click(object sender, EventArgs e)
        {
            if (dgvCart.CurrentRow == null) return;

            dgvCart.Rows.Remove(dgvCart.CurrentRow);
            UpdateTotal();
        }
        private void UpdateTotal()
        {
            decimal total = 0;

            foreach (DataRow row in cartTable.Rows)
                total += Convert.ToDecimal(row["Сумма"]);

            lblTotal.Text = "Итого: " + total.ToString("0.00") + " руб.";
        }
        private void btnSaveOrder_Click(object sender, EventArgs e)
        {
            if (cartTable.Rows.Count == 0)
            {
                MessageBox.Show("Добавьте товары!");
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlTransaction tx = conn.BeginTransaction();

                try
                {
                    string sqlOrder = @"
                        INSERT INTO orders(UserID, Date, Status, TotalAmount)
                        VALUES(@user, NOW(), 'Новый', @sum);
                        SELECT LAST_INSERT_ID();";

                    decimal total = cartTable.AsEnumerable()
                        .Sum(r => r.Field<decimal>("Сумма"));

                    MySqlCommand cmdOrder = new MySqlCommand(sqlOrder, conn, tx);
                    cmdOrder.Parameters.AddWithValue("@user", Session.UserID);
                    cmdOrder.Parameters.AddWithValue("@sum", total);

                    int orderId = Convert.ToInt32(cmdOrder.ExecuteScalar());

                    foreach (DataRow row in cartTable.Rows)
                    {
                        string sqlItem = @"
                            INSERT INTO order_items(OrderID, ProductID, Quantity, Price)
                            VALUES(@oid, @pid, @qty, @price)";

                        MySqlCommand cmdItem = new MySqlCommand(sqlItem, conn, tx);
                        cmdItem.Parameters.AddWithValue("@oid", orderId);
                        cmdItem.Parameters.AddWithValue("@pid", row["ProductID"]);
                        cmdItem.Parameters.AddWithValue("@qty", row["Количество"]);
                        cmdItem.Parameters.AddWithValue("@price", row["Цена"]);
                        cmdItem.ExecuteNonQuery();
                    }

                    tx.Commit();
                    MessageBox.Show("Заказ оформлен!");
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch
                {
                    tx.Rollback();
                    MessageBox.Show("Ошибка сохранения заказа");
                }
            }
        }
        private void dgvCart_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (dgvCart.Columns[e.ColumnIndex].Name == "Количество")
            {
                int qty = Convert.ToInt32(dgvCart.Rows[e.RowIndex].Cells["Количество"].Value);
                int stock = Convert.ToInt32(dgvProductsList.Rows[e.RowIndex].Cells["Остаток"].Value);

                if (qty <= 0)
                    qty = 1;

                if (qty > stock)
                {
                    MessageBox.Show("Количество превышает остаток");
                    qty = stock;
                }

                dgvCart.Rows[e.RowIndex].Cells["Количество"].Value = qty;
                UpdateTotal();
            }
        }
        private void dgvCart_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
