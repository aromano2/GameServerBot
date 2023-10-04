using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.Security.KeyVault.Secrets;
using AzureDiscordBot.Interfaces;
using System;
using System.Threading.Tasks;

namespace AzureDiscordBot.Services
{
    public class ServerManagementService : IServerManagementService
    {
        internal SecretClient _secretClient;
        internal DefaultAzureCredential _credential;
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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="secretClient"></param>
        /// <param name="credential"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ServerManagementService(SecretClient secretClient, DefaultAzureCredential credential)
        {
            _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            GetSecrets();
        }

        /// <summary>
        /// Gets the key vault secrets.
        /// </summary>
        private void GetSecrets()
        {
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
        }

        /// <summary>
        /// Starts the virtual machine.
        /// </summary>
        /// <returns></returns>
        public async Task StartVm() => await _vm.PowerOnAsync(Azure.WaitUntil.Completed);

        /// <summary>
        /// Stops the virtual machine.
        /// </summary>
        /// <returns></returns>
        public async Task StopVm() => await _vm.DeallocateAsync(Azure.WaitUntil.Started);

        /// <summary>
        /// Gets the power state of the virtual machine.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVmStatus()
        {
            var vmStatus = await _vm.GetAsync();
            if (vmStatus is not null)
            {
                var vmState = vmStatus.Value.InstanceView().Value.Statuses[1].Code;
                string vmPowerState = vmState.Replace("PowerState/", string.Empty);
                return vmPowerState == "deallocated" ? "Stopped" : "Running";
            }

            return "Unknown";
        }

        /// <summary>
        /// Gets the running state of the game.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<string> GetGameStatus(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            RunCommandInput command = new("RunPowerShellScript")
            {
                Script = { $"(Get-ScheduledTask -TaskName '{name} start').State" }
            };

            var result = await _vm.RunCommandAsync(Azure.WaitUntil.Completed, command);
            if (result.HasValue)
            {
                return result.Value.Value[0].Message;
            }

            return string.Empty;
        }

        /// <summary>
        /// Starts game.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task StartGame(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            RunCommandInput command = new("RunPowerShellScript")
            {
                Script = { $"Start-ScheduledTask -TaskName '{name} start'" }
            };

            await _vm.RunCommandAsync(Azure.WaitUntil.Completed, command);
        }

        /// <summary>
        /// Stops game.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task StopGame(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            RunCommandInput command = new("RunPowerShellScript")
            {
                Script = { $"Start-ScheduledTask -TaskName '{name} stop'" }
            };

            await _vm.RunCommandAsync(Azure.WaitUntil.Completed, command);
        }

        /// <summary>
        /// Updates game.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task UpdateGame(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            };

            RunCommandInput command = new("RunPowerShellScript")
            {
                Script = { $"Start-ScheduledTask -TaskName '{name} update'" }
            };

            await _vm.RunCommandAsync(Azure.WaitUntil.Completed, command);
        }

        /// <summary>
        /// Gets the connection info for the game.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetGameInfo(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            };

            var info = string.Empty;
            switch (name)
            {
                case "Valheim":
                    info = $"{name} Connection info:\r\nIP: {_secretValheimUrl.Value}\r\nPassword: {_secretValheimPassword.Value} \r\n";
                    break;
                case "VRising":
                    info = $"{name} Connection info:\r\nIP: {_secretVrisingUrl.Value}\r\nLanServer: Checked\r\nPassword: {_secretVrisingPassword.Value}\r\n";
                    break;
            }

            return info;
        }
    }
}
