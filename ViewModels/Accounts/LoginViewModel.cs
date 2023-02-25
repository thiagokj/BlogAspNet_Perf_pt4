using System.ComponentModel.DataAnnotations;

namespace BlogAspNet_Performance.ViewModels.Accounts;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe o email.")]
    [EmailAddress(ErrorMessage = "O email é inválido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Informe a senha.")]
    public string Password { get; set; }
}
