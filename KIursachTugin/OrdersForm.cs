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
using static KIursachTugin.LoginForm;
using Word = Microsoft.Office.Interop.Word;

namespace KIursachTugin
{
    public partial class OrdersForm : Form
    {
        private string _connectionString;

        public OrdersForm()
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        private void OrdersForm_Load(object sender, EventArgs e)
        {
            LoadStatuses();
            LoadOrders();
        }

        private void LoadStatuses()
        {
            cbStatus.Items.Clear();
            cbStatus.Items.Add("Все");
            cbStatus.Items.Add("Новый");
            cbStatus.Items.Add("Выполнен");
            cbStatus.Items.Add("Отменён");
            cbStatus.SelectedIndex = 0;
        }

        private void LoadOrders(string search = "", string status = "")
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = @"SELECT o.OrderID AS 'ID',
                                      u.UserLogin AS 'Пользователь',
                                      o.Date AS 'Дата заказа',
                                      o.TotalAmount AS 'Сумма',
                                      o.Status AS 'Статус'
                               FROM orders o
                               LEFT JOIN user u ON o.UserID = u.UserID
                               WHERE (u.UserLogin LIKE @s OR o.OrderID LIKE @s)
                               AND (@st = '' OR o.Status = @st)
                               ORDER BY o.Date DESC";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@s", "%" + search + "%");
                    cmd.Parameters.AddWithValue("@st", status);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dgvOrders.DataSource = table;
                }
            }
        }

        private void ApplyFilters()
        {
            string search = txtSearch.Text.Trim();
            string status = "";

            if (cbStatus.SelectedItem != null && cbStatus.SelectedItem.ToString() != "Все")
                status = cbStatus.SelectedItem.ToString();

            LoadOrders(search, status);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
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
        private void cbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cbStatus.SelectedIndex = 0; // "Все"
            ApplyFilters();
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            OrderEditForm form = new OrderEditForm();
            if (form.ShowDialog() == DialogResult.OK)
                ApplyFilters();
        }

        private void btnDetails_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvOrders.CurrentRow.Cells["ID"].Value);
            OrderDetailsForm form = new OrderDetailsForm(id);
            form.ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null) return;

            int id = Convert.ToInt32(dgvOrders.CurrentRow.Cells["ID"].Value);
            DialogResult res = MessageBox.Show("Отменить заказ?", "Подтверждение", MessageBoxButtons.YesNo);
            if (res == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE orders SET Status='Отменён' WHERE OrderID=@id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                ApplyFilters();
            }
        }
        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (dgvOrders.CurrentRow == null)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }


            int id = Convert.ToInt32(dgvOrders.CurrentRow.Cells["ID"].Value);
            PrintReceipt(id);
        }


        private void PrintReceipt(int orderId)
    {
        Word.Application wordApp = new Word.Application();
        wordApp.Visible = true;

        Word.Document doc = wordApp.Documents.Add();

        // Размер страницы как чек
        doc.PageSetup.PageWidth = 300;
        doc.PageSetup.PageHeight = 800;
        doc.PageSetup.LeftMargin = 25;
        doc.PageSetup.RightMargin = 25;

        string line = "------------------------------------------";

        string cashier = "—";
        decimal total = 0;
        DateTime orderDate = DateTime.Now;

        // ===== ПОЛУЧАЕМ ДАННЫЕ ЗАКАЗА =====
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            conn.Open();

            string sqlOrder = @"
            SELECT o.TotalAmount,
                   o.Date,
                   u.UserLogin
            FROM orders o
            LEFT JOIN user u ON o.UserID = u.UserID
            WHERE o.OrderID = @id";

            using (MySqlCommand cmd = new MySqlCommand(sqlOrder, conn))
            {
                cmd.Parameters.AddWithValue("@id", orderId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader["TotalAmount"] != DBNull.Value)
                            total = Convert.ToDecimal(reader["TotalAmount"]);

                        if (reader["Date"] != DBNull.Value)
                            orderDate = Convert.ToDateTime(reader["Date"]);

                        if (reader["UserLogin"] != DBNull.Value)
                            cashier = reader["UserLogin"].ToString();
                    }
                }
            }
        }

        // ===== ШАПКА =====
        AddCentered(doc, "МАГАЗИН КРЕПЕЖА", 12, true);
        AddCentered(doc, "г. Заволжье, РФ", 10, false);
        AddCentered(doc, "ИНН 0000000000", 10, false);
        AddCentered(doc, line, 10, false);

        AddLeft(doc, $"Чек № {orderId}", 10);
        AddLeft(doc, $"Дата: {orderDate:dd.MM.yyyy HH:mm}", 10);
        AddLeft(doc, $"Кассир: {cashier}", 10);

        AddCentered(doc, line, 10, false);

        // ===== ТАБЛИЦА ТОВАРОВ =====
        Word.Table table = doc.Tables.Add(
            doc.Paragraphs.Add().Range,
            1, 4);

        table.Borders.Enable = 0;
        table.Range.Font.Name = "Courier New";
        table.Range.Font.Size = 10;

        table.Cell(1, 1).Range.Text = "Товар";
        table.Cell(1, 2).Range.Text = "Кол-во";
        table.Cell(1, 3).Range.Text = "Цена";
        table.Cell(1, 4).Range.Text = "Сумма";

        table.Columns[1].Width = 130;
        table.Columns[2].Width = 40;
        table.Columns[3].Width = 45;
        table.Columns[4].Width = 55;

        int rowIndex = 1;

        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            conn.Open();

            string sqlItems = @"
            SELECT p.Name,
                   oi.Quantity,
                   oi.Price,
                   (oi.Quantity * oi.Price) AS Sum
            FROM order_items oi
            JOIN products p ON oi.ProductID = p.ProductID
            WHERE oi.OrderID = @id";

            using (MySqlCommand cmd = new MySqlCommand(sqlItems, conn))
            {
                cmd.Parameters.AddWithValue("@id", orderId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        table.Rows.Add();
                        rowIndex++;

                        string name = reader["Name"].ToString();
                        int qty = Convert.ToInt32(reader["Quantity"]);
                        decimal price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]);
                        decimal sum = reader["Sum"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Sum"]);

                        table.Cell(rowIndex, 1).Range.Text = name;
                        table.Cell(rowIndex, 2).Range.Text = qty.ToString();
                        table.Cell(rowIndex, 3).Range.Text = price.ToString("0.00");
                        table.Cell(rowIndex, 4).Range.Text = sum.ToString("0.00");
                    }
                }
            }
        }

        // ===== ИТОГ =====
        AddCentered(doc, line, 10, false);

        AddRight(doc, $"ИТОГО: {total:0.00}руб", 11, true);
        AddRight(doc, $"Наличные: {total:0.00} руб", 10, false);
        AddRight(doc, $"Сдача: 0.00 руб", 10, false);

        AddCentered(doc, line, 10, false);

        // ===== ПОДВАЛ =====
        AddCentered(doc, "Ждем вас снова!", 10, false);

}

        private void AddCentered(Word.Document doc, string text, int size, bool bold)
        {
            Word.Paragraph p = doc.Paragraphs.Add();
            p.Range.Text = text;
            p.Range.Font.Name = "Courier New";
            p.Range.Font.Size = size;
            p.Range.Font.Bold = bold ? 1 : 0;
            p.Range.ParagraphFormat.Alignment =
                Word.WdParagraphAlignment.wdAlignParagraphCenter;
            p.Range.InsertParagraphAfter();
        }

        private void AddLeft(Word.Document doc, string text, int size)
        {
            Word.Paragraph p = doc.Paragraphs.Add();
            p.Range.Text = text;
            p.Range.Font.Name = "Courier New";
            p.Range.Font.Size = size;
            p.Range.ParagraphFormat.Alignment =
                Word.WdParagraphAlignment.wdAlignParagraphLeft;
            p.Range.InsertParagraphAfter();
        }

        private void AddRight(Word.Document doc, string text, int size, bool bold)
        {
            Word.Paragraph p = doc.Paragraphs.Add();
            p.Range.Text = text;
            p.Range.Font.Name = "Courier New";
            p.Range.Font.Size = size;
            p.Range.Font.Bold = bold ? 1 : 0;
            p.Range.ParagraphFormat.Alignment =
                Word.WdParagraphAlignment.wdAlignParagraphRight;
            p.Range.InsertParagraphAfter();
        }

        private DataTable GetOrderItems(int orderId)
        {
            DataTable dt = new DataTable();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT p.Name,
                           oi.Quantity,
                           p.Price
                    FROM order_items oi
                    JOIN products p ON oi.ProductID = p.ProductID
                    WHERE oi.OrderID = @id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", orderId);
                    new MySqlDataAdapter(cmd).Fill(dt);
                }
            }

            return dt;
        }
    }
}