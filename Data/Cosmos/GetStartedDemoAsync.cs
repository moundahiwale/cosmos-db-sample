using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using todo.Model;

namespace todo.Data.Cosmos
{
    public class GetStartedDemoAsync
    {
        private string EndpointUrl = Environment.GetEnvironmentVariable("EndpointUrl");
        private string PrimaryKey = Environment.GetEnvironmentVariable("PrimaryKey");
        private CosmosClient cosmosClient;

        // Client options is needed to bypass the SSL connection/remote certificate invalid error. 
        // https://github.com/Azure/azure-cosmos-dotnet-v3/issues/1551#issuecomment-670673346
        // This error occurs even after certificate is imported as mentioned here (search for 'To import the TLS/SSL certificate')
        // https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21#running-on-docker
        CosmosClientOptions cosmosClientOptions = new CosmosClientOptions()
        {
            HttpClientFactory = () =>
            {
                HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                };
                return new HttpClient(httpMessageHandler);
            },
            ConnectionMode = ConnectionMode.Gateway
        };
        private Database database;
        private Container container;
        private string databaseId = "FamilyDatabase";
        private string containerId = "FamilyContainer";
        
        public async Task StartDemoAsync()
        {
            cosmosClient = new CosmosClient(EndpointUrl, PrimaryKey, cosmosClientOptions);
            await CreateDatabaseAsync();
            await CreateContainerAsync();
            await AddItemsToContainerAsync();
            await QueryItemsAsync();
        }
        private async Task CreateDatabaseAsync()
        {
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            System.Console.WriteLine($"Created database ", database.Id);
        }
        private async Task CreateContainerAsync()
        {
            container = await database.CreateContainerIfNotExistsAsync(containerId, "/LastName");
            System.Console.WriteLine($"Created container ", container.Id);
        }
        private async Task AddItemsToContainerAsync()
        {
            var andersenFamily = new Family
            {
                Id = "Andersen.1",
                LastName = "Andersen",
                Parents = new Parent[]
                {
                    new Parent {FirstName = "Thomas"},
                    new Parent{FirstName = "Mary Kay"}
                },
                Children = new Child[]
                {
                    new Child
                    {
                        FirstName = "First Child",
                        Gender = "male",
                        Grade = 5,
                        Pets = new Pet[]
                        {
                            new Pet {GivenName = "Fluffy"}
                        }
                    }
                },
                Address = new Address { State = "WA", Country = "US", City = "Seattle" },
                IsRegistered = false
            };

            try
            {
                ItemResponse<Family> andersenFamilyResponse = await container.CreateItemAsync<Family>(andersenFamily, new PartitionKey(andersenFamily.LastName));
                System.Console.WriteLine($"Create item {andersenFamilyResponse.Resource.Id}. Operation consumed {andersenFamilyResponse.RequestCharge} RUs. \n");
            }
            catch (System.Exception)
            {
                System.Console.WriteLine($"Item {andersenFamily.Id} already exists \n");
            }
        }
        private async Task QueryItemsAsync()
        {
            var sqlQueryText = "SELECT * FROM c WHERE c.LastName = 'Andersen'";
            System.Console.WriteLine($"Running query {sqlQueryText}");

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Family> queryResultIterator = container.GetItemQueryIterator<Family>(queryDefinition);

            List<Family> families = new List<Family>();

            while(queryResultIterator.HasMoreResults)
            {
                FeedResponse<Family> currentResultSet = await queryResultIterator.ReadNextAsync();
                foreach (var family in currentResultSet)
                {
                    families.Add(family);
                    System.Console.WriteLine($"\t Read {family}");
                }
            }
        }
    }
}