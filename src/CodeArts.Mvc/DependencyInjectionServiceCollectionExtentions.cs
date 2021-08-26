using CodeArts;
#if NETCOREAPP2_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Http;
#endif
using System;
using System.Linq;
using System.Collections.Generic;
using CodeArts.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 服务集合扩展。
    /// </summary>
    public static class DependencyInjectionServiceCollectionExtentions
    {
#if NETCOREAPP2_0_OR_GREATER
        /// <summary>
        /// 使用依赖注入（注入继承<see cref="ControllerBase"/>的构造函数参数类型，也会注入【继承<see cref="ControllerBase"/>的构造函数参数】以及【其参数类型的构造函数参数】）。
        /// </summary>
#else
        /// <summary>
        /// 使用依赖注入（注入继承<see cref="ApiController"/>的构造函数参数类型，也会注入【继承<see cref="ApiController"/>的构造函数参数】以及【其参数类型的构造函数参数】）。
        /// </summary>
#endif
        public static IServiceCollection UseDependencyInjection(this IServiceCollection services) => UseDependencyInjection(services, new DependencyInjectionOptions());

#if NETCOREAPP2_0_OR_GREATER
        /// <summary>
        /// 使用依赖注入（注入继承<see cref="ControllerBase"/>的构造函数参数类型，也会注入【继承<see cref="ControllerBase"/>的构造函数参数】以及【其参数类型的构造函数参数】）。
        /// </summary>
#else
        /// <summary>
        /// 使用依赖注入（注入继承<see cref="ApiController"/>的构造函数参数类型，也会注入【继承<see cref="ApiController"/>的构造函数参数】以及【其参数类型的构造函数参数】）。
        /// </summary>
#endif
        public static IServiceCollection UseDependencyInjection(this IServiceCollection services, DependencyInjectionOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            string pattern = options.Pattern;

            var assemblys = AssemblyFinder.Find(pattern);

            var assemblyTypes = assemblys
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass || x.IsInterface)
                .ToList();

#if NETCOREAPP2_0_OR_GREATER
            var controllerBaseType = typeof(ControllerBase);
#else
            var controllerBaseType = typeof(ApiController);
#endif

            var controllerTypes = assemblyTypes
                .Where(x => x.IsClass && !x.IsAbstract && controllerBaseType.IsAssignableFrom(x))
                .ToList();

            int maxDepth = options.MaxDepth;
            ServiceLifetime lifetime = options.Lifetime;

            if (!Enum.IsDefined(typeof(ServiceLifetime), lifetime))
            {
                throw new InvalidCastException($"“{lifetime}”不是有效的“ServiceLifetime”枚举值！");
            }

            foreach (var controllerType in controllerTypes)
            {
                bool flag = false;

                foreach (var constructorInfo in controllerType.GetConstructors().Where(x => x.IsPublic))
                {
                    flag = true;

                    foreach (var parameterInfo in constructorInfo.GetParameters())
                    {
                        if (parameterInfo.IsOptional || Di(services, parameterInfo.ParameterType, assemblyTypes, 0, maxDepth, lifetime))
                        {
                            continue;
                        }

                        flag = false;

                        break;
                    }

                    if (flag)
                    {
                        break;
                    }
                }

                if (flag)
                {
                    continue;
                }

                throw new TypeLoadException($"Controller '{controllerType.FullName}' cannot be created and the current maximum dependency injection depth is {maxDepth}.");
            }

            return services;
        }

        private static bool Di(IServiceCollection services, Type serviceType, List<Type> assemblyTypes, int depth, int maxDepth, ServiceLifetime lifetime)
        {
            if (services.Any(x => x.ServiceType == serviceType))
            {
                return true;
            }

            if (serviceType == typeof(IServiceProvider))
            {
                return true;
            }

            //? 集合获取。
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                serviceType = serviceType.GetGenericArguments()[0];
            }

            if (depth >= maxDepth)
            {
                if (serviceType.IsGenericType)
                {
                    var typeDefinition = serviceType.GetGenericTypeDefinition();

                    if (services.Any(x => x.ServiceType == typeDefinition))
                    {
                        return true;
                    }
                }

                return false;
            }

            var implementationTypes = (serviceType.IsInterface || serviceType.IsAbstract)
                ? assemblyTypes
                    .Where(y => y.IsClass && !y.IsAbstract && serviceType.IsAssignableFrom(y))
                    .ToList()
                : new List<Type> { serviceType };

            bool flag = false;

            foreach (var implementationType in implementationTypes)
            {
                foreach (var constructorInfo in implementationType.GetConstructors().Where(x => x.IsPublic))
                {
                    flag = true;

                    foreach (var parameterInfo in constructorInfo.GetParameters())
                    {
                        if (parameterInfo.IsOptional)
                        {
                            continue;
                        }

                        if (Di(services, parameterInfo.ParameterType, assemblyTypes, depth + 1, maxDepth, lifetime))
                        {
                            continue;
                        }

                        flag = false;

                        break;
                    }

                    if (flag)
                    {
                        break;
                    }
                }

                if (flag)
                {
                    switch (lifetime)
                    {
                        case ServiceLifetime.Singleton:
                            services.AddSingleton(serviceType, implementationType);
                            break;
                        case ServiceLifetime.Scoped:
                            services.AddScoped(serviceType, implementationType);
                            break;
                        case ServiceLifetime.Transient:
                        default:
                            services.AddTransient(serviceType, implementationType);
                            break;
                    }

                    break;
                }
            }

            if (!flag)
            {
                if (serviceType.IsGenericType)
                {
                    var typeDefinition = serviceType.GetGenericTypeDefinition();

                    if (services.Any(x => x.ServiceType == typeDefinition))
                    {
                        return true;
                    }
                }
            }

            return flag;
        }
    }
}