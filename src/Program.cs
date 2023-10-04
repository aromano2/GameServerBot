using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureDiscordBot.Commands;
using AzureDiscordBot.Interfaces;
using AzureDiscordBot.Services;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AzureDiscordBot
{
    public class Program
    {
        internal const string KvUri = "https://azurediscord.vault.azure.net/";
        internal static DefaultAzureCredential _credential;
        internal static SecretClient _secretClient;
        internal static KeyVaultSecret _discordToken;
        internal static KeyVaultSecret _vmIp;

        public static async Task Main(string[] args)
       {
            Console.WriteLine("Bot started");

            GetSecrets();
            var discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = _discordToken.Value,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });

            var config = new SlashCommandsConfiguration
            {
                Services = new ServiceCollection().AddSingleton(typeof(IServerManagementService), (_) => new ServerManagementService(_secretClient, _credential)).BuildServiceProvider()
            };

            var slash = discordClient.UseSlashCommands(config);
            slash.RegisterCommands<SlashCommands>();
            await discordClient.ConnectAsync();
            await Task.Delay(-1);
       }

        private static void GetSecrets()
        {
            _credential = new DefaultAzureCredential() ?? throw new ArgumentNullException(nameof(_credential));
            _secretClient = new SecretClient(new Uri(KvUri), _credential) ?? throw new ArgumentNullException(nameof(_secretClient));
            _discordToken = _secretClient.GetSecret("devToken") ?? throw new ArgumentNullException(nameof(_discordToken));
            _vmIp = _secretClient.GetSecret("vmIp") ?? throw new ArgumentNullException(nameof(_vmIp));
            
            Console.WriteLine($"Retrieved secret: {_discordToken.Name}");
            Console.WriteLine($"Retrieved secret: {_vmIp.Name}");
        }
    }
}