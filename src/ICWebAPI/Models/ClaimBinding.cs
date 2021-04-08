using System.ComponentModel.DataAnnotations;

namespace ICWebAPI.Models
{
    public class ClaimBinding
    {
        [Required]
        [Display(Name = "Claim Tipo")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Claim Valor")]
        public string Value { get; set; }
    }
}