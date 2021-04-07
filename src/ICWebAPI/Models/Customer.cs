using System;
using System.ComponentModel.DataAnnotations;

namespace ICWebAPI.Models
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(120, ErrorMessage = "O campo {0} precisa ter entre {2} e {120} caracteres", MinimumLength = 2)]
        public string Name { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
        [StringLength(120, ErrorMessage = "O campo {0} precisa ter entre {2} e {120} caracteres", MinimumLength = 2)]
        public string Email { get; set; }
    }
}
