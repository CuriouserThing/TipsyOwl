using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TipsyOwl
{
    public class DiscordEventHandler : IDisposable
    {
        private DiscordEventHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            Client = client;
            Commands = commands;
            Services = services;
        }

        private DiscordSocketClient Client { get; }
        private CommandService Commands { get; }
        private IServiceProvider Services { get; }

        private void Register()
        {
            Client.MessageReceived += ClientOnMessageReceivedAsync;
            Commands.CommandExecuted += CommandsOnCommandExecutedAsync;
        }

        private void Deregister()
        {
            Client.MessageReceived -= ClientOnMessageReceivedAsync;
            Commands.CommandExecuted -= CommandsOnCommandExecutedAsync;
        }

        public static DiscordEventHandler CreateAndRegister(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            var handler = new DiscordEventHandler(client, commands, services);
            handler.Register();
            return handler;
        }

        private async Task ClientOnMessageReceivedAsync(SocketMessage message)
        {
            CommandDispatcher dispatcher = Services.GetRequiredService<CommandDispatcher>();
            await dispatcher.DispatchFromMessageReceivedAsync(message);
        }

        private async Task CommandsOnCommandExecutedAsync(Optional<CommandInfo> arg1, ICommandContext arg2, IResult arg3)
        {
            ICommandResultHandler resultHandler = Services.GetRequiredService<ICommandResultHandler>();
            await resultHandler.HandleResult(arg1, arg2, arg3);
        }

        #region IDisposable

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Deregister();
                }

                _disposedValue = true;
            }
        }

        ~DiscordEventHandler()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
