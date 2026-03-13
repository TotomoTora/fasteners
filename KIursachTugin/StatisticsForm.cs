using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text;

namespace KIursachTugin.Forms
{
    public partial class StatisticsForm : Form
    {
        private readonly string _connectionString;
        private readonly string _logPath;

        public StatisticsForm()
        {
            InitializeComponent();
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics_errors.log");
        }

        private void StatisticsForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Устанавливаем культура для корректного отображения дат/чисел
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");

                // Начальные значения диапазона
                dtFrom.Value = DateTime.Now.AddMonths(-1);
                dtTo.Value = DateTime.Now;
                dtFrom.MaxDate = DateTime.Today;
                dtTo.MaxDate = DateTime.Today;
                // Настройка графика (статичная часть)
                ConfigureChartAppearance();

                // Загрузка данных
                LoadStatistics();
            }
            catch (Exception ex)
            {
                LogError("Load error", ex);
                MessageBox.Show("Ошибка при загрузке формы: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureChartAppearance()
        {
            chartSales.Series.Clear();
            chartSales.ChartAreas.Clear();
            chartSales.Titles.Clear();

            ChartArea area = new ChartArea("MainArea");
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.LabelStyle.Format = "dd.MM";
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            area.AxisX.IntervalType = DateTimeIntervalType.Days;
            area.AxisX.Title = "Дата";
            area.AxisY.Title = "Выручка (₽)";
            chartSales.ChartAreas.Add(area);

            // Легенда
            if (chartSales.Legends.Count == 0)
                chartSales.Legends.Add(new Legend("Legend"));

            chartSales.Titles.Add("Динамика выручки (выполненные заказы)");
        }

        private void LoadStatistics()
        {
            try
            {
                LoadSummary();
                LoadTopProducts();
                LoadSalesChart();
            }
            catch (Exception ex)
            {
                LogError("LoadStatistics error", ex);
                MessageBox.Show("Ошибка при загрузке статистики: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSummary()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            COUNT(*) AS TotalOrders,
                            SUM(COALESCE(TotalAmount,0)) AS TotalRevenue,
                            SUM(CASE WHEN Status = 'Выполнен' THEN 1 ELSE 0 END) AS CompletedOrders
                        FROM orders
                        WHERE Date BETWEEN @from AND @to;";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@from", dtFrom.Value.Date);
                        cmd.Parameters.AddWithValue("@to", dtTo.Value.Date.AddDays(1));

                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                int totalOrders = r["TotalOrders"] != DBNull.Value ? Convert.ToInt32(r["TotalOrders"]) : 0;
                                int completed = r["CompletedOrders"] != DBNull.Value ? Convert.ToInt32(r["CompletedOrders"]) : 0;
                                decimal revenue = r["TotalRevenue"] != DBNull.Value ? Convert.ToDecimal(r["TotalRevenue"]) : 0m;

                                lblTotalOrders.Text = string.Format("Всего заказов: {0}", totalOrders);
                                lblCompleted.Text = string.Format("Выполнено: {0}", completed);
                                lblRevenue.Text = string.Format("Выручка: {0} ₽", revenue.ToString("0.00"));
                            }
                            else
                            {
                                lblTotalOrders.Text = "Всего заказов: 0";
                                lblCompleted.Text = "Выполнено: 0";
                                lblRevenue.Text = "Выручка: 0.00 ₽";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("LoadSummary error", ex);
                throw; // пробрасываем, чтобы внешний обработчик отобразил сообщение
            }
        }

        private void LoadTopProducts()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT p.Name AS Товар, SUM(oi.Quantity) AS Количество
                        FROM order_items oi
                        JOIN products p ON oi.ProductID = p.ProductID
                        JOIN orders o ON oi.OrderID = o.OrderID
                        WHERE o.Date BETWEEN @from AND @to
                        GROUP BY p.Name
                        ORDER BY SUM(oi.Quantity) DESC
                        LIMIT 10;";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@from", dtFrom.Value.Date);
                        cmd.Parameters.AddWithValue("@to", dtTo.Value.Date.AddDays(1));

                        DataTable table = new DataTable();
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            adapter.Fill(table);
                        }

                        dgvTopProducts.DataSource = table;

                        // Небольшая косметика столбцов
                        if (dgvTopProducts.Columns["Товар"] != null)
                            dgvTopProducts.Columns["Товар"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        if (dgvTopProducts.Columns["Количество"] != null)
                            dgvTopProducts.Columns["Количество"].Width = 100;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("LoadTopProducts error", ex);
                throw;
            }
        }

        private void LoadSalesChart()
        {
            try
            {
                DataTable table = new DataTable();

                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    string sql = @"
                        SELECT DATE(o.Date) AS OrderDate,
                               COALESCE(SUM(o.TotalAmount), 0) AS Revenue
                        FROM orders o
                        WHERE o.Status = 'Выполнен'
                          AND o.Date BETWEEN @from AND @to
                        GROUP BY DATE(o.Date)
                        ORDER BY DATE(o.Date);";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.
                        AddWithValue("@from", dtFrom.Value.Date);
                        cmd.Parameters.AddWithValue("@to", dtTo.Value.Date.AddDays(1));
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            adapter.Fill(table);
                        }
                    }
                }

                chartSales.Series.Clear();

                // Настроим серию
                Series series = new Series("Продажи");
                series.ChartType = SeriesChartType.Line;
                series.BorderWidth = 3;
                series.Color = Color.SteelBlue;
                series.XValueType = ChartValueType.DateTime;
                series.YValueType = ChartValueType.Double;
                series.MarkerStyle = MarkerStyle.Circle;
                series.MarkerSize = 7;
                series.IsValueShownAsLabel = false; // можно включить при надобности

                chartSales.Series.Add(series);

                chartSales.Titles.Clear();
                if (table.Rows.Count == 0)
                {
                    chartSales.Titles.Add("Нет данных за выбранный период");
                    return;
                }

                // Добавим точки вручную, чтобы избежать проблем с типами
                foreach (DataRow row in table.Rows)
                {
                    object ordObj = row["OrderDate"];
                    object revObj = row["Revenue"];

                    DateTime dt;
                    decimal revenue;

                    // Парсим дату — она приходит как DATE (MySQL) или строка
                    if (ordObj == DBNull.Value || !DateTime.TryParse(ordObj.ToString(), out dt))
                    {
                        // пропускаем строку с некорректной датой
                        continue;
                    }

                    if (revObj == DBNull.Value)
                        revenue = 0m;
                    else
                        revenue = Convert.ToDecimal(revObj);

                    // Добавляем точку
                    series.Points.AddXY(dt, Convert.ToDouble(revenue));
                }

                // Формат оси X
                ChartArea ca = chartSales.ChartAreas[0];
                ca.AxisX.LabelStyle.Format = "dd.MM";
                ca.AxisX.IntervalType = DateTimeIntervalType.Days;
                ca.RecalculateAxesScale();
            }
            catch (Exception ex)
            {
                LogError("LoadSalesChart error", ex);
                throw;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                LoadStatistics();
            }
            catch (Exception ex)
            {
                LogError("Refresh error", ex);
                MessageBox.Show("Ошибка при обновлении: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            DateTime dateFrom = dtFrom.Value.Date;
            DateTime dateTo = dtTo.Value.Date.AddDays(1).AddSeconds(-1);

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel файл (*.xls)|*.xls",
                FileName = $"Отчет_Продажи_{DateTime.Now:yyyyMMdd}.xls"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();

                    // 1) Продажи по товарам (кол-во, цена, сумма)
                    DataTable dtProducts = new DataTable();
                    using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT p.Name AS 'Товар',
                       SUM(oi.Quantity) AS 'Количество',
                       oi.Price AS 'Цена',
                       SUM(oi.Quantity * oi.Price) AS 'Сумма'
                FROM orders o
                JOIN order_items oi ON o.OrderID = oi.OrderID
                JOIN products p ON oi.ProductID = p.ProductID
                WHERE o.Date BETWEEN @d1 AND @d2
                  AND o.Status = 'Выполнен'
                GROUP BY p.Name, oi.Price
                ORDER BY SUM(oi.Quantity) DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@d1", dateFrom);
                        cmd.Parameters.AddWithValue("@d2", dateTo);
                        new MySqlDataAdapter(cmd).Fill(dtProducts);
                    }

                    // 2) Статистика по продавцам
                    DataTable dtSellers = new DataTable();
                    using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT u.UserLogin AS 'Продавец',
                       COUNT(o.OrderID) AS 'Количество заказов',
                       SUM(o.TotalAmount) AS 'Выручка'
                FROM orders o
                LEFT JOIN user u ON o.UserID = u.UserID
                WHERE o.Date BETWEEN @d1 AND @d2
                  AND o.Status = 'Выполнен'
                GROUP BY u.UserLogin
                ORDER BY SUM(o.TotalAmount) DESC", conn))
                    {
                        cmd.Parameters.AddWithValue("@d1", dateFrom);
                        cmd.Parameters.AddWithValue("@d2", dateTo);
                        new MySqlDataAdapter(cmd).Fill(dtSellers);
                    }

                    // 3) Анализ по датам
                    DataTable dtByDate = new DataTable();
                    using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT DATE(o.Date) AS 'Дата',
                       COUNT(*) AS 'Заказов',
                       SUM(o.TotalAmount) AS 'Выручка'
                FROM orders o
                WHERE o.Date BETWEEN @d1 AND @d2
                  AND o.Status = 'Выполнен'
                GROUP BY DATE(o.Date)
                ORDER BY DATE(o.Date)", conn))
                    {
                        cmd.Parameters.AddWithValue("@d1", dateFrom);
                        cmd.Parameters.AddWithValue("@d2", dateTo);
                        new MySqlDataAdapter(cmd).Fill(dtByDate);
                    }

                    // 4) Средний чек
                    decimal avgCheck = 0;
                    using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT IFNULL(AVG(TotalAmount),0)
                FROM orders
                WHERE Date BETWEEN @d1 AND @d2
                  AND Status = 'Выполнен'", conn))
                    {
                        cmd.Parameters.AddWithValue("@d1", dateFrom);
                        cmd.Parameters.AddWithValue("@d2", dateTo);
                        avgCheck = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    // 5) Отмененные заказы
                    int canceled = 0;
                    using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT COUNT(*)
                FROM orders
                WHERE Date BETWEEN @d1 AND @d2
                  AND Status = 'Отменён'", conn))
                    {
                        cmd.Parameters.AddWithValue("@d1", dateFrom);
                        cmd.Parameters.AddWithValue("@d2", dateTo);
                        canceled = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 6) Остатки товаров
                    DataTable dtStock = new DataTable();
                    using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT Name AS 'Товар',
                       Quantity AS 'Остаток'
                FROM products
                WHERE is_active = 1
                ORDER BY Name", conn))
                    {
                        new MySqlDataAdapter(cmd).Fill(dtStock);
                    }

                    // 7) Лучший товар (по выручке)
                    string bestProduct = "-";
                    using (MySqlCommand cmd = new MySqlCommand(@"
                SELECT p.Name
                FROM orders o
                JOIN order_items oi ON o.OrderID = oi.OrderID
                JOIN products p ON oi.ProductID = p.ProductID
                WHERE o.Date BETWEEN @d1 AND @d2
                  AND o.Status = 'Выполнен'
                GROUP BY p.Name
                ORDER BY SUM(oi.Quantity * oi.Price) DESC
                LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@d1", dateFrom);
                        cmd.Parameters.AddWithValue("@d2", dateTo);
                        object res = cmd.ExecuteScalar();
                        if (res != null) bestProduct = res.ToString();
                    }

                    // === Формируем Excel-совместимый HTML ===
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("<html><head><meta charset='utf-8'></head><body>");
                        sw.WriteLine("<h2 style='text-align:center'>МАГАЗИН КРЕПЕЖА — ОТЧЕТ ПО ПРОДАЖАМ</h2>");
                        sw.WriteLine($"<p><b>Период:</b> {dateFrom:dd.MM.yyyy} — {dateTo:dd.MM.yyyy}</p>");
                        sw.WriteLine($"<p><b>Средний чек:</b> {avgCheck:0.00} руб.</p>");
                        sw.WriteLine($"<p><b>Отмененные заказы:</b> {canceled}</p>");
                        sw.WriteLine($"<p><b>Лучший товар:</b> {bestProduct}</p>");

                        WriteTable(sw, "Продажи по товарам", dtProducts);
                        WriteTable(sw, "Статистика по продавцам", dtSellers);
                        WriteTable(sw, "Динамика по датам", dtByDate);
                        WriteTable(sw, "Остатки товаров", dtStock);

                        sw.WriteLine("<p>Отчет сформирован автоматически системой учета продаж крепежа.</p>");
                        sw.WriteLine("</body></html>");
                    }
                }

                MessageBox.Show("Полный отчет сформирован!");
            }
            catch (Exception ex)
            {
                LogError("ExportFullStatistics", ex);
                MessageBox.Show("Ошибка экспорта:\n" + ex.Message);
            }
        }

        // Универсальная печать таблицы в HTML
        private void WriteTable(StreamWriter sw, string title, DataTable dt)
        {
            sw.WriteLine($"<h3>{title}</h3>");
            sw.WriteLine("<table border='1' cellspacing='0' cellpadding='4'>");

            // заголовки
            sw.WriteLine("<tr style='background:#e6e6e6;font-weight:bold'>");
            foreach (DataColumn col in dt.Columns)
                sw.WriteLine($"<td>{col.ColumnName}</td>");
            sw.WriteLine("</tr>");

            // строки
            foreach (DataRow row in dt.Rows)
            {
                sw.WriteLine("<tr>");
                foreach (var item in row.ItemArray)
                    sw.WriteLine($"<td>{item}</td>");
                sw.WriteLine("</tr>");
            }

            sw.WriteLine("</table>");
        }

        // Простой логгер ошибок в файл
        private void LogError(string where, Exception ex)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(_logPath, true))
                {
                    sw.WriteLine("----------------------------------------------------");
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    sw.WriteLine(where);
                    sw.WriteLine(ex.ToString());
                }
            }
            catch
            {
                // Ничего не делаем, чтобы не ломать UI при логировании
            }
        }

        private void dateTo_ValueChanged(object sender, EventArgs e)
        {
            dtTo.MinDate = dtFrom.Value.Date;
        }

        private void dateFrom_ValueChanged(object sender, EventArgs e)
        {
            dtTo.MinDate = dtFrom.Value.Date;
        }
    }
}