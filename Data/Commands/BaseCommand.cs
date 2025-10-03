using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SusEquip.Data.Commands
{
    /// <summary>
    /// Base abstract class for commands that return a result
    /// </summary>
    /// <typeparam name="T">The type of result returned by the command</typeparam>
    public abstract class BaseCommand<T> : ICommand<T>
    {
        protected readonly ILogger Logger;

        protected BaseCommand(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Task<T> ExecuteAsync();
    }

    /// <summary>
    /// Base abstract class for commands that do not return a result
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        protected readonly ILogger Logger;

        protected BaseCommand(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Task ExecuteAsync();
    }
}