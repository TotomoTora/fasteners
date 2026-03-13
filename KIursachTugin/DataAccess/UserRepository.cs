using KIursachTugin.Models;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace KIursachTugin.DataAccess
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
            // Читаем строку подключения из App.config
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        /// <summary>
        /// Авторизация пользователя по логину и паролю
        /// </summary>

        public User GetUserByLoginAndPassword(string login, string password)
        {
            User user = null;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT u.UserID,u.UserName,u.UserSurname,u.UserPatronymic, u.UserLogin, u.UserPassword, u.RoleID, r.RoleName AS role
                    FROM user u
                    JOIN role r ON u.RoleID = r.RoleID
                    WHERE u.UserLogin = @login AND u.UserPassword = @password
                    LIMIT 1";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", password);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                Id = reader.GetInt32("UserID"),
                                UserName = reader.GetString("UserName"),
                                UserSurname = reader.GetString("UserSurname"),
                                UserPatronymic = reader.GetString("UserPatronymic"),
                                Login = reader.GetString("UserLogin"),
                                PasswordHash = reader.GetString("UserPassword"),
                                RoleID = reader.GetInt32("RoleID"),
                                RoleName = reader.GetString("role")
                            };
                        } 
                    }
                }
            }
        return user;
        }
    }
}
