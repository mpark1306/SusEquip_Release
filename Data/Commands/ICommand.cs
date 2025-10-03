using System;
using System.Threading.Tasks;

namespace SusEquip.Data.Commands
{
    /// <summary>
    /// Base interface for commands that return a result
    /// </summary>
    /// <typeparam name="T">The type of result returned by the command</typeparam>
    public interface ICommand<T>
    {
        Task<T> ExecuteAsync();
    }

    /// <summary>
    /// Interface for commands that do not return a result
    /// </summary>
    public interface ICommand : ICommand<Unit>
    {
        new Task ExecuteAsync();
        Task<Unit> ICommand<Unit>.ExecuteAsync() => ExecuteAsync().ContinueWith(_ => Unit.Value);
    }


}