using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PetShelterBackend.Models;
using PetShelterBackend.Models.PostgreSqlModel;
using PetShelterBackend.Repository;
using PetShelterBackend.Settings;

namespace PetShelterBackend;

public class Startup
{
    public Startup(ConfigurationManager configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton(provider =>
            new PetShelterContext(new DbContextOptionsBuilder<PetShelterContext>()
                .UseNpgsql(Configuration.GetConnectionString("PostgresConnection"))
                .Options));

        var settings = Configuration.GetSection("MongoDBSettings");
        var client = new MongoClient(settings.Get<MongoDBSettings>().ConnectionString);
        var database = client.GetDatabase("PetShelter");

        services.AddSingleton(database);

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PetShelter API",
                Version = "v1",
                Description = "API for managing pets in a shelter"
            });
        });

        services.AddSingleton<IMongoCollection<PetMongo>>(serviceProvider =>
        {
            var db = serviceProvider.GetRequiredService<IMongoDatabase>();
            return db.GetCollection<PetMongo>("Pets");
        });

        services.AddSingleton<IMongoCollection<PetShelterMongo>>(serviceProvider =>
        {
            var db = serviceProvider.GetRequiredService<IMongoDatabase>();
            return db.GetCollection<PetShelterMongo>("PetShelters");
        });

        services.AddSingleton<IMongoRepository<PetMongo>, MongoRepository<PetMongo>>();
        services.AddSingleton<IMongoRepository<PetShelterMongo>, MongoRepository<PetShelterMongo>>();

        services.AddScoped<IPostgreSQLRepository<PetPost>, PostgreSQLRepository<PetPost>>();
        services.AddScoped<IPostgreSQLRepository<PetShelterPost>, PostgreSQLRepository<PetShelterPost>>();


        services.AddControllers(options => { options.SuppressAsyncSuffixInActionNames = false; });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddScoped<PerformanceTest>();
        services.AddScoped<CsvDataService>();
    }

    public async Task Configure(WebApplication app, IWebHostEnvironment env)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            await services.GetRequiredService<PetShelterContext>().Database.EnsureCreatedAsync();
            /*if (env.IsDevelopment())
            {
                var performanceTest = scope.ServiceProvider.GetRequiredService<PerformanceTest>();
                await performanceTest.TestForX(100);
                await performanceTest.TestForX(1000);
                await performanceTest.TestForX(100000);
            }*/
            var insertRealData = scope.ServiceProvider.GetRequiredService<CsvDataService>();
            insertRealData.InsertDataFromCsvAsync("real-pet-data.csv");
        }
        app.UseCors(builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
            
        
        if (env.IsDevelopment() || env.IsProduction())
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