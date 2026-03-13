using KIursachTugin.Models;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;

namespace KIursachTugin.DataAccess
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }



        // Получение списка категорий
        public DataTable GetCategories()
        {
            DataTable table = new DataTable();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT CategoriesID, CategoriesName FROM categories ORDER BY CategoriesName";
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }


        // Получение списка поставщиков
        public DataTable GetSuppliers()
        {
            DataTable table = new DataTable();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT SuppliersID, SuppliersName FROM suppliers ORDER BY SuppliersName";
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }

        // Получение одного товара по ID


        public Product GetProductById(int id)
        {
            Product product = null;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM products WHERE ProductID=@id";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new Product
                            {
                                ProductID = Convert.ToInt32(reader["ProductID"]),
                                Name = reader["Name"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Description = reader["Description"].ToString(),
                                CategoriesID = reader["CategoriesID"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["CategoriesID"]),
                                SuppliersID = reader["SuppliersID"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["SuppliersID"]),
                                ImagePath = reader["ImagePath"] == DBNull.Value ? null : reader["ImagePath"].ToString()
                            };
                        }
                    }
                }
            }

            return product;
        }

        // Добавление товара
        public void AddProduct(Product p)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = @"INSERT INTO products 
                                (Name, Price, Quantity, CategoriesID, SuppliersID, Description, ImagePath)
                                VALUES 
                                (@Name, @Price, @Quantity, @CategoriesID, @SuppliersID, @Description, @ImagePath)";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", p.Name);
                    cmd.Parameters.AddWithValue("@Price", p.Price);
                    cmd.Parameters.AddWithValue("@CategoriesID", p.CategoriesID.HasValue ? (object)p.CategoriesID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SuppliersID", p.SuppliersID.HasValue ? (object)p.SuppliersID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Quantity", p.Quantity);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(p.Description) ? "" : p.Description);
                    cmd.Parameters.AddWithValue("@ImagePath", p.ImagePath);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Обновление товара
        public void UpdateProduct(Product p)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string sql = @"UPDATE products SET
                                Name=@Name,
                                Price=@Price,
                                Quantity=@Quantity,
                                CategoriesID=@CategoriesID,
                                SuppliersID=@SuppliersID,
                                Description=@Description,
                                ImagePath=@ImagePath
                                WHERE ProductID=@ProductID";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", p.Name);
                    cmd.Parameters.AddWithValue("@Price", p.Price);
                    cmd.Parameters.AddWithValue("@CategoriesID", p.CategoriesID.HasValue ? (object)p.CategoriesID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SuppliersID", p.SuppliersID.HasValue ? (object)p.SuppliersID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Quantity", p.Quantity);
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(p.Description) ? "" : p.Description);
                    cmd.Parameters.AddWithValue("@ProductID", p.ProductID);
                    cmd.Parameters.AddWithValue("@ImagePath", p.ImagePath);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}