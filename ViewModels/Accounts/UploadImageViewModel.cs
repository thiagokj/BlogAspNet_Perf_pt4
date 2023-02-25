using System.ComponentModel.DataAnnotations;

namespace BlogAspNet_Performance.ViewModels.Accounts
{
    public class UploadImageViewModel
    {
        // Normalmente no frontend é feito o upload de imagem com uma string base64
        [Required(ErrorMessage = "Imagem inválida")]
        public string Base64Image { get; set; }
    }
}
