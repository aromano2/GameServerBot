using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.Security.KeyVault.Secrets;
using AzureDiscordBot.Commands;
using AzureDiscordBot.Interfaces;
using AzureDiscordBot.Services;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
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
        internal static KeyVaultSecret _secretVmId;
        internal static KeyVaultSecret _secretUsername;
        internal static KeyVaultSecret _secretPassword;
        internal static KeyVaultSecret _secretDiscordToken;
        internal static KeyVaultSecret _secretValheimUrl;
        internal static KeyVaultSecret _secretValheimPassword;
        internal static KeyVaultSecret _secretVrisingUrl;
        internal static KeyVaultSecret _secretVrisingPassword;
        internal static ResourceIdentifier _vmId;
        internal static ArmClient _armClient;
        internal static VirtualMachineResource _vm;
        internal static ILoggerFactory _logFactory;

        public static async Task Main(string[] args)
       {
            Initialize();
            var discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = _discordToken.Value,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                LoggerFactory = _logFactory
            });

            var config = new SlashCommandsConfiguration
            {
                Services = new ServiceCollection().AddSingleton(Log.Logger)
                .AddSingleton(typeof(IServerManagementService), (_) => new ServerManagementService(
                    _secretClient, _credential, _secretVmId, _secretUsername, _secretPassword, _secretValheimUrl,
                    _secretValheimPassword, _secretVrisingUrl, _secretVrisingPassword, _vmId, _armClient, _vm))
                .BuildServiceProvider()
            };

            var slash = discordClient.UseSlashCommands(config);
            slash.RegisterCommands<SlashCommands>();
            await discordClient.ConnectAsync();
            await Task.Delay(-1);
       }

        private static void Initialize()
        {
            _credential = new DefaultAzureCredential() ?? throw new ArgumentNullException(nameof(_credential));
            _secretClient = new SecretClient(new Uri(KvUri), _credential) ?? throw new ArgumentNullException(nameof(_secretClient));
            _discordToken = _secretClient.GetSecret("devToken") ?? throw new ArgumentNullException(nameof(_discordToken));
            _vmIp = _secretClient.GetSecret("vmIp") ?? throw new ArgumentNullException(nameof(_vmIp));
            _secretVmId = _secretClient.GetSecret("VmId") ?? throw new ArgumentNullException(nameof(_secretVmId));
            _secretUsername = _secretClient.GetSecret("username") ?? throw new ArgumentNullException(nameof(_secretUsername));
            _secretPassword = _secretClient.GetSecret("password") ?? throw new ArgumentNullException(nameof(_secretPassword));
            _secretValheimUrl = _secretClient.GetSecret("valheimUrl") ?? throw new ArgumentNullException(nameof(_secretValheimUrl));
            _secretValheimPassword = _secretClient.GetSecret("valheimPassword") ?? throw new ArgumentNullException(nameof(_secretValheimPassword));
            _secretVrisingUrl = _secretClient.GetSecret("vrisingUrl") ?? throw new ArgumentNullException(nameof(_secretVrisingUrl));
            _secretVrisingPassword = _secretClient.GetSecret("vrisingPassword") ?? throw new ArgumentNullException(nameof(_secretVrisingPassword));
            _vmId = new ResourceIdentifier(_secretVmId.Value) ?? throw new ArgumentNullException(nameof(_vmId));
            _armClient = new ArmClient(_credential) ?? throw new ArgumentNullException(nameof(_armClient));
            _vm = _armClient.GetVirtualMachineResource(_vmId) ?? throw new ArgumentNullException(nameof(_vm));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();
            _logFactory = new LoggerFactory().AddSerilog();
            Log.Logger.Information("Bot started");
            Log.Logger.Information($"Retrieved secret: {_secretVmId.Name}");
            Log.Logger.Information($"Retrieved secret: {_secretUsername.Name}");
            Log.Logger.Information($"Retrieved secret: {_secretPassword.Name}");
            Log.Logger.Information($"Retrieved secret: {_secretValheimUrl.Name}");
            Log.Logger.Information($"Retrieved secret: {_secretValheimPassword.Name}");
            Log.Logger.Information($"Retrieved secret: {_secretVrisingUrl.Name}");
            Log.Logger.Information($"Retrieved secret: {_secretVrisingPassword.Name}");
            Log.Logger.Information($"Retrieved secret: {_discordToken.Name}");
            Log.Logger.Information($"Retrieved secret: {_vmIp.Name}");
        }
    }
}