using AzureDiscordBot.Interfaces;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Threading.Tasks;

namespace AzureDiscordBot.Commands
{
    public class SlashCommands : ApplicationCommandModule
    {
        internal IServerManagementService ServerManagementService;

        public SlashCommands(IServerManagementService serverManagementService)
        {
            ServerManagementService = serverManagementService ?? throw new ArgumentNullException(nameof(serverManagementService));
        }

        /// <summary>
        /// Command to start game server.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [SlashCommand("gameserverstart", "Command to start dedicated game server.")]
        public async Task GameServerStartCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ServerManagementService.StartVm();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Server started"));
        }

        /// <summary>
        /// Command to stop game server.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [SlashCommand("gameserverstop", "Command to stop dedicated game server.")]
        public async Task GameServerStopCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ServerManagementService.StopVm();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Server stopped"));
        }

        /// <summary>
        /// Command to get game server status.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [SlashCommand("gameserverstatus", "Command to see the status of the game server.")]
        public async Task GameServerStatusCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var status = await ServerManagementService.GetVmStatus();
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Game server status: {status}"));
        }

        /// <summary>
        /// Command to get game status.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        [SlashCommand("gamestatus", "Command to see status of a game on the game server.")]
        public async Task GameStatusCommand(InteractionContext ctx,
            [Choice("Valheim", "Valheim")]
            [Choice("VRising", "VRising")]
            [Option("game", "Game to see status of")] string game)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var status = await ServerManagementService.GetGameStatus(game);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{game} is {status}"));
        }

        /// <summary>
        /// Command to start game.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        [SlashCommand("gamestart", "Command to start a game on the game server.")]
        public async Task GameStartCommand(InteractionContext ctx,
            [Choice("Valheim", "Valheim")][Choice("VRising", "VRising")][Option("game", "Game to start")] string game)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ServerManagementService.StartGame(game);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{game} started"));
        }

        /// <summary>
        /// Command to stop game.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        [SlashCommand("gamestop", "Command to stop a game on the game server.")]
        public async Task GameStopCommand(InteractionContext ctx,
            [Choice("Valheim", "Valheim")][Choice("VRising", "VRising")][Option("game", "Game to stop")] string game)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ServerManagementService.StopGame(game);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{game} stopped"));
        }

        /// <summary>
        /// Command to update game.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        [SlashCommand("gameupdate", "Command to update a game on the game server.")]
        public async Task GameUpdateCommand(InteractionContext ctx,
            [Choice("Valheim", "Valheim")][Choice("VRising", "VRising")][Option("game", "Game to update")] string game)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ServerManagementService.UpdateGame(game);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{game} updated"));
        }

        /// <summary>
        /// Command to get game connection info.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        [SlashCommand("gameinfo", "Command to show connection information for a game.")]
        public async Task GameInfoCommand(InteractionContext ctx,
            [Choice("Valheim", "Valheim")][Choice("VRising", "VRising")][Option("game", "Game to get connection info for")] string game)
        {
            var info = await Task.Run(() => ServerManagementService.GetGameInfo(game));
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"{info}"));
        }
    }
}
