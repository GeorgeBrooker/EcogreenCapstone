using ShopRepository.Data; 
namespace ShopRepository.Tests;

public class ShopRepositoryTest
{
    private readonly WebApplicationFactory<Program> webApplication;
    public ShopRepositoryTest()
    {
        webApplication = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    //Mock the repository implementation
                    //to remove infra dependencies for Test project
                    services.AddScoped<IShopRepo, ShopRepo>();
                });
            });
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    public async Task Call_GetApiBooks_ShouldReturn_LimitedListOfBooks(int limit)
    {
        var client = webApplication.CreateClient();
        var books = await client.GetFromJsonAsync<IList<Book>>($"/api/Books?limit={limit}");

        Assert.NotEmpty(books);
        Assert.Equal(limit, books?.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Call_GetApiBook_ShouldReturn_BadRequest(int limit)
    {
        var client = webApplication.CreateClient();
        var result = await client.GetAsync($"/api/Books?limit={limit}");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, result?.StatusCode);

    }
}