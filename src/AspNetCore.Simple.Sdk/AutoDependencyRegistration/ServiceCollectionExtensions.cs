using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Simple.Sdk.AutoDependencyRegistration
{

    public class LifetimeDetector
    {
        public ServiceLifetime DetectFor<T>()
        {
            return DetectFor(typeof(T));
        }

        public ServiceLifetime DetectFor(Type type)
        {
            var lifetimeAttribute = type.GetCustomAttribute<ServiceRegistrationAttribute>();
            if (lifetimeAttribute.IsNull())
            {
                return ServiceLifetime.Singleton;
            }

            return lifetimeAttribute.ServiceLifetime;
        }
    }

    public class AutoRegistrationFactory
    {
        public AutoRegistration Create(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var lifetimeDetector = new LifetimeDetector();
            var appsettingsRegistrationStrategy = new AppSettingsRegistrationStrategy(lifetimeDetector, serviceCollection, configuration);
            var interfaceDetector = new InterfaceDetector();
            var serviceRegistrationStrategy = new ServiceRegistrationStrategy(serviceCollection, lifetimeDetector, interfaceDetector);
            var registrationCheck = new RegistrationCheck(serviceCollection);
            var dependencyDetector = new DependencyDetector();
            var registrationStrategy = new RegistrationStartegy(new IRegistrationStrategy[] { appsettingsRegistrationStrategy, serviceRegistrationStrategy }, registrationCheck, dependencyDetector);
            var autoRegistration = new AutoRegistration(registrationStrategy);
            return autoRegistration;
        }
    }

    public class AutoRegistration
    {
        private readonly RegistrationStartegy _registrationStrategy;

        internal AutoRegistration(RegistrationStartegy registrationStrategy)
        {
            _registrationStrategy = registrationStrategy;
        }

        public void DoAutoRegistrationFor<T>()
        {
            DoAutoRegistrationFor(typeof(T));
        }

        public void DoAutoRegistrationFor(Type type)
        {
            _registrationStrategy.DoAutoRegistration(type);
        }
    }

    internal class RegistrationStartegy
    {
        private readonly IEnumerable<IRegistrationStrategy> _registrationStrategies;
        private readonly RegistrationCheck _registrationCheck;
        private readonly DependencyDetector _dependencyDetector;

        public RegistrationStartegy(IEnumerable<IRegistrationStrategy> registrationStrategies, RegistrationCheck registrationCheck, DependencyDetector dependencyDetector)
        {
            _registrationStrategies = registrationStrategies;
            _registrationCheck = registrationCheck;
            _dependencyDetector = dependencyDetector;
        }

        internal void DoAutoRegistration(Type type)
        {
            if (_registrationCheck.IsAlreadyRegistered(type))
            {
                return;
            }

            // 1. Detect dependencies 
            var dependencies = _dependencyDetector.FindDependenciesFor(type).ToList();

            // 2. Register dependencies first
            foreach (var dependency in dependencies)
            {
                DoAutoRegistrationInternal(dependency);
            }

            // 3. Register root type
            DoAutoRegistrationInternal(type);
        }

        internal void DoAutoRegistrationInternal(Type type)
        {
            if (_registrationCheck.IsAlreadyRegistered(type))
            {
                return;
            }

            var registrationResult = _registrationStrategies.Aggregate(false, (current, registrationStrategy) => registrationStrategy.DoAutoRegistration(type, current));
            if (registrationResult.IsFalse())
            {
                throw new MissingRegistrationStrategyException($"For type: '{type.Name}' in namespace: '{type.Namespace}' we do not have a strategy to register it correctly. Please check that you use: '{nameof(ServiceRegistrationAttribute)}' for services or '{nameof(AppSettingsRegistrationAttribute)}' for any kind of app settings");
            }
        }

    }

    internal class MissingRegistrationStrategyException : Exception
    {
        public MissingRegistrationStrategyException(string message) : base(message)
        {
        }
    }

    internal class RegistrationCheck
    {
        private readonly IServiceCollection _serviceCollection;

        public RegistrationCheck(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        internal bool IsAlreadyRegistered(Type type)
        {
            return _serviceCollection.Any(registration => registration.ServiceType == type && registration.ImplementationType == type);
        }
    }

    internal class AppSettingsRegistrationStrategy : IRegistrationStrategy
    {
        private readonly LifetimeDetector _lifetimeDetector;
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        public AppSettingsRegistrationStrategy(LifetimeDetector lifetimeDetector, IServiceCollection serviceCollection, IConfiguration configuration)
        {
            _lifetimeDetector = lifetimeDetector;
            _serviceCollection = serviceCollection;
            _configuration = configuration;
        }

        public bool DoAutoRegistration(Type type, bool registrationDone)
        {
            if (registrationDone)
            {
                return true;
            }

            var appsettingsAttribute = type.GetCustomAttribute<AppSettingsRegistrationAttribute>();
            if (appsettingsAttribute.IsNull())
            {
                return false;
            }

            var settings = _configuration.GetSection(appsettingsAttribute.AppSettingsName).Get(appsettingsAttribute.SettingsType);
            var validator = Activator.CreateInstance(appsettingsAttribute.Validator).Cast<ISettingsValidatorBase>();
            validator.ValidateBase(settings);
            var lifetime = _lifetimeDetector.DetectFor(type);
            _serviceCollection.Add(new ServiceDescriptor(type, _ => settings, lifetime));
            return true;
        }
    }

    internal interface IRegistrationStrategy
    {
        bool DoAutoRegistration(Type type, bool registrationDone);
    }

    internal class ServiceRegistrationStrategy : IRegistrationStrategy
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly LifetimeDetector _lifetimeDetector;
        private readonly InterfaceDetector _interfaceDetector;

        public ServiceRegistrationStrategy(IServiceCollection serviceCollection, LifetimeDetector lifetimeDetector, InterfaceDetector interfaceDetector)
        {
            _serviceCollection = serviceCollection;
            _lifetimeDetector = lifetimeDetector;
            _interfaceDetector = interfaceDetector;
        }

        public bool DoAutoRegistration(Type type, bool registrationDone)
        {
            if (registrationDone)
            {
                return true;
            }

            if (type.HasCustomAttribute<AppSettingsRegistrationAttribute>())
            {
                return false;
            }

            var interfaceToRegisterFor = _interfaceDetector.DetectInterface(type);
            var lifetime = _lifetimeDetector.DetectFor(type);
            var interfaceType = interfaceToRegisterFor ?? type;
            // If no interface exists we register the same type for interface and implementation !
            _serviceCollection.Add(new ServiceDescriptor(interfaceType, type, lifetime));
            return true;
        }
    }

    internal class DependencyDetector
    {
        internal IEnumerable<Type> FindDependenciesFor(Type type)
        {
            var dependencies = FindDependenciesForInternal(type);
            var filterDuplicate = dependencies.Distinct(dependency => dependency.FullName);
            return filterDuplicate;
        }

        private IEnumerable<Type> FindDependenciesForInternal(Type type)
        {
            // First we have to detect the constructor with the most parameters !!
            // Hint: Maybe add an attribute like MEF or JSON which should be the correct injection constructor
            //       In good designs with correct constructor implementations there should be only one with the correct dependencies
            var parameterInfos = type.GetConstructors()
                                     .Select(ctor => new { Parameters = ctor.GetParameters(), Constructor = ctor })
                                     .OrderByDescending(ctorInfo => ctorInfo.Parameters.Length)
                                     .FirstOrDefault()?.Parameters;

            if (parameterInfos.IsNullOrEmpty())
            {
                yield break;
            }

            foreach (var parameterInfo in parameterInfos)
            {
                var nextDependencies = FindDependenciesForInternal(parameterInfo.ParameterType);
                foreach (var nextDependency in nextDependencies)
                {
                    yield return nextDependency;
                }

                yield return parameterInfo.ParameterType;
            }
        }
    }

    internal class InterfaceDetector
    {
        internal Type? DetectInterface(Type typeToRegister)
        {
            // 1. Explicit given interface type has priority 1.
            var serviceRegistration = typeToRegister.GetCustomAttribute<ServiceRegistrationAttribute>();
            if (serviceRegistration?.InterfaceType is not null)
            {
                return serviceRegistration.InterfaceType;
            }

            // 2. Check if no explicit interface type was given, the amount of interfaces
            var interfaces = typeToRegister.GetInterfaces();

            // 3. If we have exactly only one interface we use that one.
            //    Important if this auto detected interface is not what you want, please provide the explicit interface, or null for the interface type registration.
            if (interfaces.Length == 1)
            {
                return interfaces.First();
            }

            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceRegistrationAttribute : Attribute
    {
        public ServiceRegistrationAttribute(ServiceLifetime serviceLifetime, Type? interfaceType = default)
        {
            ServiceLifetime = serviceLifetime;
            InterfaceType = interfaceType;
        }

        public ServiceLifetime ServiceLifetime { get; }

        public Type? InterfaceType { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AppSettingsRegistrationAttribute : ServiceRegistrationAttribute
    {
        public AppSettingsRegistrationAttribute(string appSettingsName,
                                                Type settingsType,
                                                ServiceLifetime serviceLifetime) : this(appSettingsName, settingsType, serviceLifetime, typeof(DefaultValidator))
        {

        }

        public AppSettingsRegistrationAttribute(string appSettingsName, Type settingsType, ServiceLifetime serviceLifetime, Type validator) : base(serviceLifetime)
        {
            AppSettingsName = appSettingsName;
            SettingsType = settingsType;
            Validator = validator;

            if (typeof(ISettingsValidatorBase).IsAssignableFrom(validator).IsFalse())
            {
                throw new InvalidValidatorTypeException(validator);
            }
        }

        public string AppSettingsName { get; }

        public Type SettingsType { get; }

        public Type Validator { get; }
    }

    public class InvalidValidatorTypeException : Exception
    {
        public InvalidValidatorTypeException(Type type) : base($"The validator type: {type.Name} is not implementing: '{nameof(SettingsValidator<object>)}'. Please check your implementation.")
        {

        }
    }

    public class DefaultValidator : SettingsValidator<DefaultValidator>
    {
        public override void Validate(DefaultValidator setting)
        {
            // Default validation, if no validator is given - nothing will be validated !
        }
    }

    public abstract class SettingsValidator<T> : ISettingsValidatorBase
    {
        public abstract void Validate(T setting);

        public void ValidateBase(object setting)
        {
            if (setting is T typedSetting)
            {
                Validate(typedSetting);
            }
        }
    }

    public interface ISettingsValidatorBase
    {
        void ValidateBase(object setting);
    }
}
