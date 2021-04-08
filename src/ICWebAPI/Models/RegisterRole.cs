using System.ComponentModel.DataAnnotations;

namespace ICWebAPI.Models
{
    public class RegisterRole
    {
        [Required]
        [StringLength(256, ErrorMessage = "O {0} deve ter pelo menos {2} caracteres.", MinimumLength = 2)]
        [Display(Name = "Nome do Papel")]
        public string Name { get; set; }
    }
}