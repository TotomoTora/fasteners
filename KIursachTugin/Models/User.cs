using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIursachTugin;

namespace KIursachTugin.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Login { get; set; }

        // Храним только хэш
        [Required, StringLength(255)]
        public string PasswordHash { get; set; }

        [Required, StringLength(100)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        public string UserSurname { get; set; }

        [Required, StringLength(100)]
        public string UserPatronymic { get; set; }

        public int RoleID { get; set; }       // FK в БД
      
        public string RoleName { get; set; }  // удобство для отображения

        // Простейшая валидация с использованием DataAnnotations
        public IEnumerable<ValidationResult> Validate()
        {
            var ctx = new ValidationContext(this);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(this, ctx, results, validateAllProperties: true);
            return results;
        }
    }
}

