using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SusEquip.Data.Commands.Handlers;

namespace SusEquip.Data.Commands
{
    /// <summary>
    /// Interface for executing commands
    /// </summary>
    public interface ICommandExecutor
    {
        Task<T> ExecuteAsync<T>(ICommand<T> command);
        Task ExecuteAsync(ICommand command);
        Task<T> ExecuteWithHandlerAsync<T>(ICommand<T> command);
    }

    /// <summary>
    /// Command executor that provides centralized command execution with logging and error handling
    /// </summary>
    public class CommandExecutor : ICommandExecutor
    {
        private readonly ILogger<CommandExecutor> _logger;
        private readonly IServiceProvider _serviceProvider;

        public CommandExecutor(ILogger<CommandExecutor> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Executes a command that returns a result
        /// </summary>
        /// <typeparam name="T">The type of result returned by the command</typeparam>
        /// <param name="command">The command to execute</param>
        /// <returns>The result of the command execution</returns>
        public async Task<T> ExecuteAsync<T>(ICommand<T> command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var commandType = command.GetType().Name;
            _logger.LogDebug("Executing command {CommandType}", commandType);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await command.ExecuteAsync();
                stopwatch.Stop();
                
                _logger.LogDebug("Command {CommandType} executed successfully in {ElapsedMs}ms", 
                    commandType, stopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Command {CommandType} failed after {ElapsedMs}ms", 
                    commandType, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }

        /// <summary>
        /// Executes a command that does not return a result
        /// </summary>
        /// <param name="command">The command to execute</param>
        public async Task ExecuteAsync(ICommand command)
        {
            await ExecuteAsync<Unit>(command);
        }

        /// <summary>
        /// Executes a command using a registered command handler (preferred method)
        /// </summary>
        /// <typeparam name="T">The type of result returned by the command</typeparam>
        /// <param name="command">The command to execute</param>
        /// <returns>The result of the command execution through handler</returns>
        public async Task<T> ExecuteWithHandlerAsync<T>(ICommand<T> command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var commandType = command.GetType();
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(T));
            
            var handler = _serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                _logger.LogWarning("No handler found for command {CommandType}, falling back to direct execution", 
                    commandType.Name);
                return await ExecuteAsync(command);
            }

            var commandTypeName = commandType.Name;
            _logger.LogDebug("Executing command {CommandType} with handler", commandTypeName);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var handleMethod = handlerType.GetMethod("HandleAsync");
                if (handleMethod == null)
                {
                    throw new InvalidOperationException($"Handler for {commandType.Name} does not have HandleAsync method");
                }

                var task = (Task<T>)handleMethod.Invoke(handler, new object[] { command })!;
                var result = await task;
                
                stopwatch.Stop();
                _logger.LogDebug("Command {CommandType} executed with handler successfully in {ElapsedMs}ms", 
                    commandTypeName, stopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Command {CommandType} failed with handler after {ElapsedMs}ms", 
                    commandTypeName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }

    /// <summary>
    /// Unit type for commands that don't return a value
    /// </summary>
    public struct Unit
    {
        public static readonly Unit Value = new Unit();
    }
}