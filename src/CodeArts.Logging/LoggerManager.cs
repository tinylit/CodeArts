#if NETSTANDARD2_0_OR_GREATER
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// 日志管理器。
    /// 使用<see cref="RegisterLogging(ILoggerFactory)"/>或<see cref="RegisterLogging(Func{ILoggerFactory})"/>初始化后，可用；否则返回<see cref="NullLogger"/>或<see cref="NullLogger{T}"/>日志实例。
    /// </summary>
    public static class LoggerManager
    {
        private static ILoggerFactory factory;
        private static Func<ILoggerFactory> loggerFactory;

        /// <summary>
        /// 注册日志工厂。
        /// </summary>
        /// <param name="loggerFactory">日志工厂。</param>
        public static void RegisterLogging(ILoggerFactory loggerFactory) => factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        /// <summary>
        /// 注册日志工厂。
        /// </summary>
        /// <param name="loggerFactory">日志工厂。</param>
        public static void RegisterLogging(Func<ILoggerFactory> loggerFactory) => LoggerManager.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

#if NETSTANDARD2_1_OR_GREATER
        private static ILoggerFactory Factory => factory ??= loggerFactory?.Invoke();
#else
        private static ILoggerFactory Factory => factory ?? (factory = loggerFactory?.Invoke());
#endif

        /// <summary>
        /// 获取一个日志记录器。
        /// </summary>
        /// <typeparam name="T">类型。</typeparam>
        /// <returns></returns>
        public static ILogger<T> GetLogger<T>() => Factory?.CreateLogger<T>() ?? NullLogger<T>.Instance;

        /// <summary>
        /// 获取一个日志记录器。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <returns></returns>
        public static ILogger GetLogger(Type type) => Factory?.CreateLogger(type) ?? NullLogger.Instance;
    }
}
#endif