using System.ComponentModel.DataAnnotations;

namespace AllergyScan.BLL.DTO
{
    public sealed class SetLanguageRequest
    {
        [Required]
        [StringLength(10, MinimumLength = 2)]
        public string Code { get; set; } = string.Empty;
    }

    public sealed record LanguageDTO(int Id, string Code, string Name);
}
