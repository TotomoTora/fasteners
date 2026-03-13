using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace KIursachTugin
{
    public static class PasswordHelper
    {
        // генерирует хэш
        public static string HashPassword(string password)
        {
            // workFactor можно увеличить (cost) — по умолчанию 10
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        // проверка пароля
        public static bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
