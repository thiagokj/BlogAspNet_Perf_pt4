using System.ComponentModel.DataAnnotations;

namespace BlogAspNet_Performance.ViewModels.Categories
{
    public class EditorCategoryViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(40, MinimumLength = 3,
            ErrorMessage = "Esse campo deve conter entre 3 e 40 caracteres.")]
        public string Name { get; set; }

        [Required]
        public string Slug { get; set; }
    }
}
