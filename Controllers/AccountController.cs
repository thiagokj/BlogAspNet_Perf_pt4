using BlogAspNet_Performance.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using Microsoft.AspNetCore.Authorization;
using System.Text.RegularExpressions;
using BlogAspNet_Performance.Extensions;
using BlogAspNet_Performance.Models;
using BlogAspNet_Performance.ViewModels.Accounts;
using BlogAspNet_Performance.Services;
using BlogAspNet_Performance.Data;

namespace BlogAspNet_Performance.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    [HttpPost("v1/accounts/")]
    public async Task<IActionResult> Post(
        [FromBody] RegisterViewModel model,
        [FromServices] EmailService emailService,
        [FromServices] BlogDataContext context
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = new User
        {
            Name = model.Name,
            Email = model.Email,
            Slug = model.Email.Replace("@", "-").Replace(".", "-")
        };

        var password = PasswordGenerator.Generate(25);
        user.PasswordHash = PasswordHasher.Hash(password);

        try
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            //emailService.Send(
            //    user.Name,
            //    user.Email,
            //    "Bem vindo ao blog!",
            //    $"Sua senha para acesso: <strong>{password}</strong>");

            return Ok(new ResultViewModel<dynamic>(new
            {
                user = user.Email
            }));
        }
        catch (DbUpdateException)
        {
            return StatusCode(400,
                new ResultViewModel<string>("A84F2X - Email já cadastrado."));
        }
        catch
        {
            return StatusCode(500,
                new ResultViewModel<string>("512XX - Falha interna do servidor."));
        }
    }

    [HttpPost("v1/accounts/login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginViewModel model,
        [FromServices] BlogDataContext context,
        [FromServices] TokenService tokenService)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

        var user = await context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == model.Email);

        if (user == null)
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos."));

        // Compara a senha digitada com a senha em formato de hash no banco de dados é diferente.
        if (!PasswordHasher.Verify(user.PasswordHash, model.Password))
            return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos."));

        // Caso o usuario exista e a senha esteja correta, gera o token de acesso.
        try
        {
            var token = tokenService.GenerateToken(user);
            return Ok(new ResultViewModel<string>(token, null));
        }
        catch
        {
            return StatusCode(500,
            new ResultViewModel<string>("AS5X01 - Falha interna no servidor."));
        }
    }

    [Authorize]
    [HttpPost("v1/accounts/upload-image")]
    public async Task<IActionResult> UploadImage(
        [FromBody] UploadImageViewModel model,
        [FromServices] BlogDataContext context)
    {
        // utilize Guid para evitar cache.
        var fileName = $"{Guid.NewGuid()}.jpg";

        // tratamento para remover metadados da imagem, normalmente enviada pelo frontend.
        // Dessa forma é possivel converter a string para bytes.
        var data = new Regex(@"data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");

        // Converte a string para bytes.
        var bytes = Convert.FromBase64String(data);

        try
        {
            await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<string>("I50X1 - Falha interna no servidor."));
        }

        var user = await context
            .Users
            .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

        if (user == null)
            return NotFound(new ResultViewModel<User>("Usuário não encontrado."));

        user.Image = $"{Configuration.ApplicationUrl}/images/{fileName}";
        try
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            return StatusCode(500, new ResultViewModel<string>("I50XU - Falha interna no servidor."));
        }

        return Ok(new ResultViewModel<string>("Imagem alterada com sucesso.", null));
    }
}
