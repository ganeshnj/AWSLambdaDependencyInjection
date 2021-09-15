using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaDependencyInjection
{
    public class Function
    {
        private readonly ServiceProvider serviceProvider;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(ILambdaContext context)
        {
            context.Logger.LogLine("FunctionHandler started");
            using (var scope = serviceProvider.CreateScope())
            {
                var transientOperation = scope.ServiceProvider.GetService<IOperationTransient>();
                var scopedOperation = scope.ServiceProvider.GetService<IOperationScoped>();
                var singletonOperation = scope.ServiceProvider.GetService<IOperationSingleton>();
                var instanceSingletonOperation = scope.ServiceProvider.GetService<IOperationSingletonInstance>();
                var operationService = scope.ServiceProvider.GetService<OperationService>();

                //var result = Parallel.For(0, 2, async (i) =>
                //{
                //    context.Logger.LogLine($"Task {i} started");
                //    await Task.Delay(TimeSpan.FromSeconds(10));

                //    context.Logger.LogLine("Function Operations");
                //    context.Logger.LogLine($"TransientOperation: {transientOperation.OperationId}");
                //    context.Logger.LogLine($"ScopedOperation: {scopedOperation.OperationId}");
                //    context.Logger.LogLine($"SingletonOperation: {singletonOperation.OperationId}");
                //    context.Logger.LogLine($"InstanceSingletonOperation: {instanceSingletonOperation.OperationId}");

                //    context.Logger.LogLine("OperationService Operations");
                //    context.Logger.LogLine($"TransientOperation: {operationService.TransientOperation.OperationId}");
                //    context.Logger.LogLine($"ScopedOperation: {operationService.ScopedOperation.OperationId}");
                //    context.Logger.LogLine($"SingletonOperation: {operationService.SingletonOperation.OperationId}");
                //    context.Logger.LogLine($"InstanceSingletonOperation: {operationService.SingletonInstanceOperation.OperationId}");

                //    await Task.Delay(TimeSpan.FromSeconds(10));
                //    context.Logger.LogLine($"Task {i} ended");
                //});
                //context.Logger.LogLine($"IsCompleted: {result.IsCompleted}");


                var tasks = new Task[2];
                for (int i = 0; i < 2; i++)
                {
                    var task = Task.Run(async () =>
                    {
                        int id = i;
                        context.Logger.LogLine($"Task {id} started");
                        await Task.Delay(TimeSpan.FromSeconds(10));

                        context.Logger.LogLine("Function Operations");
                        context.Logger.LogLine($"TransientOperation: {transientOperation.OperationId}");
                        context.Logger.LogLine($"ScopedOperation: {scopedOperation.OperationId}");
                        context.Logger.LogLine($"SingletonOperation: {singletonOperation.OperationId}");
                        context.Logger.LogLine($"InstanceSingletonOperation: {instanceSingletonOperation.OperationId}");

                        context.Logger.LogLine("OperationService Operations");
                        context.Logger.LogLine($"TransientOperation: {operationService.TransientOperation.OperationId}");
                        context.Logger.LogLine($"ScopedOperation: {operationService.ScopedOperation.OperationId}");
                        context.Logger.LogLine($"SingletonOperation: {operationService.SingletonOperation.OperationId}");
                        context.Logger.LogLine($"InstanceSingletonOperation: {operationService.SingletonInstanceOperation.OperationId}");

                        await Task.Delay(TimeSpan.FromSeconds(10));
                        context.Logger.LogLine($"Task {id} ended");
                    });
                    tasks[i] = task;
                }

                Task.WaitAll(tasks);

                context.Logger.LogLine("FunctionHandler exited");
            }
        }

        public Function()
        {
            var services = new ServiceCollection();
            services.ConfigureServices();
            serviceProvider = services.BuildServiceProvider();
        }
    }

    public static class Startup
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services)
        {
            services.AddTransient<IOperationTransient, Operation>();
            services.AddScoped<IOperationScoped, Operation>();
            services.AddSingleton<IOperationSingleton, Operation>();
            services.AddSingleton<IOperationSingletonInstance>(new Operation(Guid.Empty));
            services.AddTransient<OperationService, OperationService>();

            return services;
        }
    }

    public interface IOperation
    {
        Guid OperationId { get; }
    }

    public interface IOperationTransient : IOperation
    {
    }

    public interface IOperationScoped : IOperation
    {
    }

    public interface IOperationSingleton : IOperation
    {
    }

    public interface IOperationSingletonInstance : IOperation
    {
    }

    public class Operation : IOperationTransient, IOperationScoped, IOperationSingleton, IOperationSingletonInstance
    {
        Guid _guid;
        public Operation() : this(Guid.NewGuid())
        {

        }

        public Operation(Guid guid)
        {
            _guid = guid;
        }

        public Guid OperationId => _guid;
    }

    public class OperationService
    {
        public IOperationTransient TransientOperation { get; }
        public IOperationScoped ScopedOperation { get; }
        public IOperationSingleton SingletonOperation { get; }
        public IOperationSingletonInstance SingletonInstanceOperation { get; }

        public OperationService(IOperationTransient transientOperation,
            IOperationScoped scopedOperation,
            IOperationSingleton singletonOperation,
            IOperationSingletonInstance instanceOperation)
        {
            TransientOperation = transientOperation;
            ScopedOperation = scopedOperation;
            SingletonOperation = singletonOperation;
            SingletonInstanceOperation = instanceOperation;
        }
    }
}
