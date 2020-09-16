using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using todo.Data.Cosmos;

namespace todo
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                System.Console.WriteLine("Beginning operations...\n");
                var startup = new GetStartedDemoAsync();
                await startup.StartDemoAsync();
            }
            catch (CosmosException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine($"{de.StatusCode} error occurred: {de}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
            }
            finally
            {
                Console.WriteLine("End of demo, press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
