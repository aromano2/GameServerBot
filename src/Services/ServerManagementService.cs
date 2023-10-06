using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.Security.KeyVault.Secrets;
using AzureDiscordBot.Interfaces;
using Serilog;
using System;
using System.Threading.Tasks;

namespace AzureDiscordBot.Services
{
    public class ServerManagementService : IServerManagementService
    {
        internal SecretClient _secretClient;
        internal DefaultAzureCredential _credential;
        internal KeyVaultSecret _secretVmId;
        internal KeyVaultSecret _secretUsername;
        internal KeyVaultSecret _secretPassword;
        internal KeyVaultSecret _secretDiscordToken;
        internal KeyVaultSecret _secretValheimUrl;
        internal KeyVaultSecret _secretValheimPassword;
        internal KeyVaultSecret _secretVrisingUrl;
        internal KeyVaultSecret _secretVrisingPassword;
        internal ResourceIdentifier _vmId;
        internal ArmClient _armClient;
        internal VirtualMachineResource _vm;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="secretClient"></param>
        /// <param name="credential"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ServerManagementService(SecretClient secretClient, DefaultAzureCredential credential, KeyVaultSecret secretVmId, KeyVaultSecret secretUsername,
            KeyVaultSecret secretPassword, KeyVaultSecret secretValheimUrl, KeyVaultSecret secretValheimPassword,
            KeyVaultSecret secretVrisingUrl, KeyVaultSecret secretVrisingPassword, ResourceIdentifier vmId, ArmClient armClient, VirtualMachineResource vm)
        {
            _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
            _secretVmId = secretVmId ?? throw new ArgumentNullException(nameof(secretVmId));
            _secretUsername = secretUsername ?? throw new ArgumentNullException(nameof(secretUsername));
            _secretPassword = secretPassword ?? throw new ArgumentNullException(nameof(secretPassword));
            _secretValheimUrl = secretValheimUrl ?? throw new ArgumentNullException(nameof(secretValheimUrl));
            _secretValheimPassword = secretValheimPassword ?? throw new ArgumentNullException(nameof(secretValheimPassword));
            _secretVrisingUrl = secretVrisingUrl ?? throw new ArgumentNullException(nameof(secretVrisingUrl));
            _secretVrisingPassword = secretVrisingPassword ?? throw new ArgumentNullException(nameof(secretVrisingPassword));
            _vmId = vmId ?? throw new ArgumentNullException(nameof(vmId));
            _armClient = armClient ?? throw new ArgumentNullException(nameof(armClient));
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
        }

        /// <summary>
        /// Starts the virtual machine.
        /// </summary>
        /// <returns></returns>
        public async Task StartVm()
        {
            Log.Logger.Information($"Starting VM.");
            await _vm.PowerOnAsync(Azure.WaitUntil.Completed);
            Log.Logger.Information($"Started VM.");
        }

        /// <summary>
        /// Stops the virtual machine.
        /// </summary>
        /// <returns></returns>
        public async Task StopVm()
        {
            Log.Logger.Information($"Stopping VM.");
            await _vm.DeallocateAsync(Azure.WaitUntil.Started);
            Log.Logger.Information($"Stopped VM.");
        }

        /// <summary>
        /// Gets the power state of the virtual machine.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVmStatus()
        {
            var cleanPowerState = string.Empty;
            var vmStatus = await _vm.GetAsync();
            Log.Logger.Information($"Getting VM status.");

            if (vmStatus is not null)
            {
                var vmState = vmStatus.Value.InstanceView().Value.Statuses[1].Code;
                string vmPowerState = vmState.Replace("PowerState/", string.Empty);
                cleanPowerState = vmPowerState == "deallocated" ? "Stopped" : "Running";
            }
            else
            {
                cleanPowerState = "Unknown";
            }

            Log.Logger.Information($"VM is {cleanPowerState}.");
            return cleanPowerState;
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

            var gameState = string.Empty;

            Log.Logger.Information($"Getting status of {name}.");
            RunCommandInput command = new("RunPowerShellScript")
            {
                Script = { $"(Get-ScheduledTask -TaskName '{name} start').State" }
            };

            var result = await _vm.RunCommandAsync(Azure.WaitUntil.Completed, command);
            if (result.HasValue)
            {
                gameState = result.Value.Value[0].Message;
            }

            if (gameState.ToLower().Equals("ready"))
            {
                gameState = "Stopped";
            }

            Log.Logger.Information($"{name} is {gameState}.");
            return gameState;
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

            Log.Logger.Information($"Starting {name}.");
            RunCommandInput command = new("RunPowerShellScript")
            {
                Script = { $"Start-ScheduledTask -TaskName '{name} start'" }
            };

            await _vm.RunCommandAsync(Azure.WaitUntil.Completed, command);
            Log.Logger.Information($"Started {name}.");
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

            Log.Logger.Information($"Stopping {name}.");
            RunCommandInput command = new("RunPowerShellScript")
            {
                Script = { $"Start-ScheduledTask -TaskName '{name} stop'" }
            };

            await _vm.RunCommandAsync(Azure.WaitUntil.Completed, command);
            Log.Logger.Information($"Stopped {name}.");
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

            Log.Logger.Information($"Starting update for {name}.");

            RunCommandInput command = new("RunPowerShellScript")
            {
                Script = { $"Start-ScheduledTask -TaskName '{name} update'" }
            };

            await _vm.RunCommandAsync(Azure.WaitUntil.Completed, command);
            Log.Logger.Information($"Updated {name}.");
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

            switch (name)
            {
                case "Valheim":
                    return $"{name} Connection info:\r\nIP: {_secretValheimUrl.Value}\r\nPassword: {_secretValheimPassword.Value} \r\n";
                case "VRising":
                    return $"{name} Connection info:\r\nIP: {_secretVrisingUrl.Value}\r\nLanServer: Checked\r\nPassword: {_secretVrisingPassword.Value}\r\n";
            }

            return string.Empty;
        }
    }
}
