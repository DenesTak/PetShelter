namespace PetShelterBackend.Settings;

public class MongoDBSettings
{
    public string Host { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public int Port { get; init; }

    private bool IsRunningLocally => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    
    public string ConnectionString => IsRunningLocally ?  $"mongodb://{Host}:{Port}" : $"mongodb+srv://{Username}:{Password}@{Host}";
    
}