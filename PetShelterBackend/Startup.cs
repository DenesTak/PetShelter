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

        services.AddSingleton<IRepository<Pet>, Repository<Pet>>();
        services.AddSingleton<IRepository<Shelter>, Repository<Shelter>>();
        
        services.AddControllers(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;
        });
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public async Task Configure(WebApplication app, IWebHostEnvironment env)
    {
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