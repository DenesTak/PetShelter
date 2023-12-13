using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PetShelterBackend.Models;
using PetShelterBackend.Repository;
using PetShelterBackend.Settings;

namespace PetShelterBackend;

public class Startup
{
    public IConfiguration Configuration { get; }
    
    public Startup(ConfigurationManager configuration)
    {
        Configuration = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PetShelterContext>(provider =>
            new PetShelterContext(new DbContextOptionsBuilder<PetShelterContext>()
                .UseNpgsql(Configuration.GetConnectionString("PostgresConnection"))
                .Options));
        
        var settings = Configuration.GetSection("MongoDBSettings");
        var client = new MongoClient(settings.Get<MongoDBSettings>().ConnectionString);
        var database = client.GetDatabase("PetShelter");

        services.AddSingleton<IMongoDatabase>(database);
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PetShelter API",
                Version = "v1",
                Description = "API for managing pets in a shelter"
            });
        });

        services.AddSingleton<IMongoCollection<Pet>>(serviceProvider =>
        {
            var db = serviceProvider.GetRequiredService<IMongoDatabase>();
            return db.GetCollection<Pet>("Pets");
        });

        services.AddSingleton<IMongoCollection<Shelter>>(serviceProvider =>
        {
            var db = serviceProvider.GetRequiredService<IMongoDatabase>();
            return db.GetCollection<Shelter>("Shelters");
        });

        services.AddSingleton<IMongoRepository<Pet>, MongoRepository<Pet>>();
        services.AddSingleton<IMongoRepository<Shelter>, MongoRepository<Shelter>>();

        services.AddScoped<IPostgreSQLRepository<Pet>, PostgreSQLRepository<Pet>>();
        services.AddScoped<IPostgreSQLRepository<Shelter>, PostgreSQLRepository<Shelter>>();
        

        
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public async Task Configure(WebApplication app, IWebHostEnvironment env)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await services.GetRequiredService<PetShelterContext>().Database.EnsureCreatedAsync();
        }
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PetShelter API V1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
    }
}