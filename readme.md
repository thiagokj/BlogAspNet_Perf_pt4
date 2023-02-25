# BlogAspNet - Performance

Projeto para revisão de conceito e aprendizado,
continuação do projeto [BlogAspNet](https://github.com/thiagokj/BlogAspNet_Validations)

Alguns exemplos sobre Performance.

## Performance

Crie ViewModels para retornar dados especificos, como apenas alguns campos de um objeto.

```Csharp
public class ListPostsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public DateTime LastUpdateDate { get; set; }
    public string Category { get; set; }
    public string Author { get; set; }
}

// Exemplo do Controller
public class PostController : ControllerBase
{
    [HttpGet("v1/posts")]
    public async Task<IActionResult> GetAsync(
        [FromServices] BlogDataContext context)
    {
        var posts = await context
            .Posts
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Select(x =>
            new ListPostsViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                LastUpdateDate = x.LastUpdateDate,
                Category = x.Category.Name,
                Author = $"{x.Author.Name} ({x.Author.Email})"
            })
            .ToListAsync();

        return Ok(posts);
    }
}
```

Obs: Caso sejam adicionados os Includes e não seja especificado o retorno com a instrução Select,
pode ocorrer um loop no retorno, resultado em erro.
Isso ocorre por conta da **Serialização e Deserialização** automática do Asp.net.

Para contornar essa situação, informe as opções de JSON no builder.

```Csharp
...
void ConfigureMVC(WebApplicationBuilder builder)
{
    builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    })
    .AddJsonOptions(x =>
    {
        /*
            Essa opção ignora ciclo de loop.
            Ex: Uma Categoria tem uma lista de Posts e cada Post possui uma Categoria.
            No exemplo, essa verificação seria feita infinitamente, com uma classe referenciando a outra.
        */
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Ignora objetos nulos, não renderizando (trazendo) seus valores no retorno.
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    });
}
```

Para evitar que algum campo seja retornado por padrão, pode ser adicionado o atributo na Model **[JsonIgnore]**.

```Csharp
public class User
{
...
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    [JsonIgnore]
    public string PasswordHash { get; set; }
    [JsonIgnore]
    public string Image { get; set; }
...
```

## Paginação de Dados

Sempre trabalhe com paginação, evitando trazer um volume grande de dados, retorne somente o necessário.

Adicione ao controller os atribudos via QueryString para pegar a pagina atual e a quantidade de registros.

```Csharp
...
    public async Task<IActionResult> GetAsync(
        [FromServices] BlogDataContext context,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 25)
```

Adicione a paginação antes de retornar o método.
```Csharp
...
Category = x.Category.Name,
Author = $"{x.Author.Name} ({x.Author.Email})"
})
.Skip(page * pageSize) // Inicia na pagina 0
.Take(pageSize) // Retorna os primeiros 25 registros, depois +25 (50) registros, depois  + 25 (75)...
.ToListAsync();
```

## Cache

Adicione cache para consultas feitas com frequencia, onde as tabelas não costuma sobre tanta alteração.

```Csharp
...
// Adicione ao Builder
void ConfigureMVC(WebApplicationBuilder builder)
{
    builder.Services.AddMemoryCache();
    ...
}

// Adicione a instrução de cache no controller como no exemplo abaixo

[HttpGet("v1/categories")]
        public async Task<IActionResult> GetAsync(
            [FromServices] IMemoryCache cache,
            [FromServices] BlogDataContext context)
        {
            try
            {
                var categories = await cache.GetOrCreate("CategoriesCache", entry =>
                {
                    // Faz um refresh a cada 1 hora
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                    return GetCategories(context);
                });

                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch
            {
                return StatusCode(500,
                    new ResultViewModel<List<Category>>("05XXE2 - Falha interna no servidor."));
            }
        }

        // Guarda retorno da lista conforme tempo de expiração
        private List<Category> GetCategories(BlogDataContext context)
        {
            return context.Categories.ToList();
        }
```

## Compressão

Habilite a compressão para reduzir e otimizar o tamanho do JSON das respostas da API para o Frontend.

```Csharp
...
// Adicione ao Builder
void ConfigureMVC(WebApplicationBuilder builder)
{
    ...
    builder.Services.AddResponseCompression(options =>
    {
        options.Providers.Add<GzipCompressionProvider>();
    });
    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });
}

// Faça a chamada no app builder

...
app.UseResponseCompression();
```