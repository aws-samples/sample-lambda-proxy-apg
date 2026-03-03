using System;
using Amazon.Lambda.Core;
using Npgsql;
using Newtonsoft.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace demo_CS_LambdaDbFunc;

public class FunctionInput
{
    public string Key1 { get; set; }
}
public class SecretsManagerService 
{
    private class dbSecrets
    {
        public string username { get; set; }
        public string password { get; set; }
        public string engine { get; set; }
        public string host { get; set; }
        public string port { get; set; }
        public string dbname { get; set; }
    }

    private SecretsManagerCache cache = new SecretsManagerCache();
    public SecretsManagerService()
    {
        var client = new AmazonSecretsManagerClient();
        cache = new SecretsManagerCache(client);
    }        // Retrieves the secret containing the username and password
    public async Task<(string Host, string Port, string DB, string Username, string Password)> GetCredentialsAsync(string secretName)
    {
        try
            {
                string secretString = await cache.GetSecretString(secretName);
                // Console.WriteLine($"Secret String: {secretString}");
                var secret = JsonConvert.DeserializeObject<dbSecrets>(secretString);
                return (secret.host, secret.port, secret.dbname, secret.username, secret.password);
            }
            catch (Exception ex)
            {
                // Handle exceptions such as secret not found or JSON parsing errors
                Console.WriteLine($"Error retrieving secret: {ex.Message}");
                throw;
            }
    }
}
public class Function
{    
    public string FunctionHandler(FunctionInput input, ILambdaContext context)
    {

        // Set secert demo-rds-proxy-secret
        var secretName = input.Key1;
        Console.WriteLine($"Secret Name is: {secretName}");
        var secretsManagerService = new SecretsManagerService();
        // Get key/value from secret
        var (Host, Port, DB, Username, Password) = secretsManagerService.GetCredentialsAsync(secretName).GetAwaiter().GetResult();

        // Overwrite Host by RDS Endpoint
        // Host = "kenchou-apg-proxy.proxy-cnznsfatfhuq.ap-northeast-1.rds.amazonaws.com";

        // Overwrite DB Name
        // DB = "postgres";

        // Confirm RDS Proxy Endpoint in the Secret picked up
        // Console.WriteLine($"RDS Proxy Endpoint: {Host}");
        // Console.WriteLine($"DB: {DB}");
        // Console.WriteLine($"Username: {Username}");
        // Console.WriteLine($"Password: {Password}");

        string connectionString = $"Host={Host};Port={Port};Database={DB};Username={Username};Password={Password};";
        var sqlQuery = "SELECT emp_id, first_name FROM employee";

        try
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                Console.WriteLine($"DB Host is: {Host}");
                Console.WriteLine($"DB Name is: {DB}");

                // Perform database operations here
                using (var command = new NpgsqlCommand(sqlQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) // Iterate through the results
                        {
                            var col1 = reader.GetInt32(0); 
                            var col2 = reader.GetString(1);

                            Console.WriteLine($"emp_id: {col1}, first_name: {col2}");
                        }
                    }
                }

                connection.Close();
            }

            return "Successfully connected to the database via RDS Proxy.";
        }
        catch (Exception ex)
        {
            return $"Failed to connect to the database: {ex.Message}";
        }

    }
}

