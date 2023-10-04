using System.Threading.Tasks;

namespace AzureDiscordBot.Interfaces
{
    public interface IServerManagementService
    {
        Task StartVm();

        Task StopVm();

        Task<string> GetVmStatus();

        Task<string> GetGameStatus(string name);

        Task StartGame(string name);

        Task StopGame(string name);

        Task UpdateGame(string name);

        string GetGameInfo(string name);
    }
}
