using System;
using System.Configuration;
using System.Data.EntityClient;

namespace EncryptConfig
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter the path to the App.config file of the target application:");
            string configPath = Console.ReadLine();
            EncryptConnectionString(configPath);
        }

        private static void EncryptConnectionString(string configPath)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(configPath);
                var connectionStringsSection = config.GetSection("connectionStrings") as ConnectionStringsSection;

                if (connectionStringsSection != null)
                {
                    foreach (ConnectionStringSettings connectionStringSettings in connectionStringsSection.ConnectionStrings)
                    {
                        if (connectionStringSettings.ProviderName == "System.Data.EntityClient")
                        {
                            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder(connectionStringSettings.ConnectionString);
                            string providerConnectionString = entityBuilder.ProviderConnectionString;

                            if (!providerConnectionString.Contains("Encrypted"))
                            {
                                // Encrypt the provider connection string
                                byte[] encryptedBytes = System.Text.Encoding.UTF8.GetBytes(providerConnectionString);
                                string encryptedConnectionString = Convert.ToBase64String(encryptedBytes);

                                // Update the Entity Connection String
                                entityBuilder.ProviderConnectionString = $"Encrypted={encryptedConnectionString};";
                                connectionStringSettings.ConnectionString = entityBuilder.ToString();

                                config.Save();
                                Console.WriteLine("Connection string has been encrypted.");
                            }
                            else
                            {
                                Console.WriteLine("Connection string is already encrypted.");
                            }

                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Connection string not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
