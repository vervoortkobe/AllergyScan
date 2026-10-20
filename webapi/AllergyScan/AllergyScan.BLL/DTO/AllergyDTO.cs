using System.ComponentModel.DataAnnotations;

namespace AllergyScan.BLL.DTO
{
    public sealed class CreateAllergyRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }

    public sealed class UpdateAllergyRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }

    public sealed record AllergyDTO(int Id, string Name, DateTime CreatedAt);
}
