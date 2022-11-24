using System;
using System.Threading;
using System.Threading.Tasks;
using AspNetCore.Simple.Sdk.ErrorHandling;
using AspNetCore.Simple.Sdk.Extensions;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetCore.Simple.Sdk.Test.Mediator
{
    public record Person(string Name);

    public record GetPersonByName(string Name) : IQuery<Person>;

    public class PersonHandler : IQueryHandler<GetPersonByName, Person>
    {
        public Task<Person> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new Person(string.Empty));
        }
    }

    public static class AddGetPersonByNameValidatorExtension
    {
        public static void AddGetPersonByNameValidator(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IRequestValidator<GetPersonByName>, GetPersonByNameValidator>();
        }
    }

    public class GetPersonByNameValidator : IRequestValidator<GetPersonByName>
    {
        public Task ValidateAsync(GetPersonByName request)
        {
            if (request.Name.IsNullOrWhiteSpace())
            {
                throw new ProblemDetailsException($"{nameof(GetPersonByName)} request contains invalid data", $"The property: {nameof(request.Name)} was null, empty or whitespace");
            }

            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class ValidationTest
    {
        private readonly IMediator _mediator;

        public ValidationTest()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMediatR(typeof(ValidationTest));
            serviceCollection.AddValidationBehavior();
            serviceCollection.AddSingletonIfNotExists<IServiceProvider, ServiceProvider>();
            serviceCollection.AddGetPersonByNameValidator();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            _mediator = serviceProvider.GetOrThrowMissingException<IMediator>();
        }

        [TestMethod]
        public Task If_Validator_Exists_User_Response_Exception_Have_To_Be_Thrown()
        {
            return Assert.ThrowsExceptionAsync<ProblemDetailsException>(() => _mediator.SendAsync(new GetPersonByName(string.Empty)));
        }
    }
}
