using MediatR;

namespace AspNetCore.Simple.Sdk.Mediator
{
    public interface ICommand : IRequest
    {
    }

    public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand> where TCommand : ICommand, IRequest<Unit>
    {
    }

    public interface ICommandHandler<in TCommand, TResult> : IRequestHandler<TCommand, TResult> where TCommand : ICommand<TResult>
    {
    }


    public abstract class CommandHandlerWithoutResponse<TRequest> : AsyncRequestHandler<TRequest> where TRequest : IRequest
    {
    }


    public interface ICommand<out TResult> : IRequest<TResult>
    {
    }
}
