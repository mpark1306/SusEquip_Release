using System.Threading.Tasks;

namespace SusEquip.Data.Commands.Handlers
{
    /// <summary>
    /// Interface for handling specific types of commands
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle</typeparam>
    /// <typeparam name="TResult">The type of result returned by the command</typeparam>
    public interface ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command);
    }

    /// <summary>
    /// Interface for handling commands without a return value
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle</typeparam>
    public interface ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Task HandleAsync(TCommand command);
    }
}