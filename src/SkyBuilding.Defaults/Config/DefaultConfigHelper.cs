﻿#if NETSTANDARD2_0 ||NETSTANDARD2_1
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace SkyBuilding.Config
{
    /// <summary>
    /// Json 配置助手
    /// </summary>
    public class DefaultConfigHelper : IConfigHelper
    {
        private static IConfigurationBuilder _builder;

        /// <summary>
        /// 获取默认配置
        /// </summary>
        /// <param name="useConfigCenter"></param>
        /// <returns></returns>
        static IConfigurationBuilder ConfigurationBuilder()
        {
            string currentDir = Directory.GetCurrentDirectory();

            var builder = new ConfigurationBuilder()
                 .SetBasePath(currentDir);

            var path = Path.Combine(currentDir, "appsettings.json");

            if (File.Exists(path))
            {
                builder.AddJsonFile(path, false, true);
            }

            return builder;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DefaultConfigHelper() : this(_builder ?? (_builder = ConfigurationBuilder()))
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="builder">配置</param>
        public DefaultConfigHelper(IConfigurationBuilder builder) : this(builder.Build())
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="config">配置</param>
        public DefaultConfigHelper(IConfigurationRoot config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _callbackRegistration = config.GetReloadToken()
                .RegisterChangeCallback(ConfigChanged, config);
        }

        private readonly IConfigurationRoot _config;
        private IDisposable _callbackRegistration;

        /// <summary> 配置文件变更事件 </summary>
        public event Action<object> OnConfigChanged;

        /// <summary> 当前配置 </summary>
        public IConfiguration Config => _config;

        /// <summary>
        /// 配置变更事件
        /// </summary>
        /// <param name="state"></param>
        private void ConfigChanged(object state)
        {
            OnConfigChanged?.Invoke(state);
            _callbackRegistration?.Dispose();
            _callbackRegistration = _config.GetReloadToken()
                .RegisterChangeCallback(ConfigChanged, state);
        }

        /// <summary>
        /// 配置文件读取
        /// </summary>
        /// <typeparam name="T">读取数据类型</typeparam>
        /// <param name="key">健</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public T Get<T>(string key, T defaultValue = default)
        {
            //简单类型直接获取其值
            try
            {
                var type = typeof(T);
                if (type.IsSimpleType())
                    return _config.GetValue(key, defaultValue);

                //其他复杂类型
                return _config.GetSection(key).Get<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary> 重新加载配置 </summary>
        public void Reload() { _config.Reload(); }
    }
}
#else
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web.Configuration;

namespace SkyBuilding.Config
{
    /// <summary>
    /// Json 配置助手
    /// </summary>
    public class DefaultConfigHelper : DesignMode.Singleton<DefaultConfigHelper>, IConfigHelper
    {
        private readonly Configuration Config;
        private readonly Dictionary<string, string> Configs;
        private readonly Dictionary<string, ConnectionStringSettings> ConnectionStrings;

        private DefaultConfigHelper()
        {
            Configs = new Dictionary<string, string>();

            ConnectionStrings = new Dictionary<string, ConnectionStringSettings>();

            Config = WebConfigurationManager.OpenWebConfiguration("~");

            Reload();

            var filePath = Config.FilePath;
            var fileName = Path.GetFileName(filePath);
            var path = filePath.Substring(0, filePath.Length - fileName.Length);

            using (var watcher = new FileSystemWatcher(path, fileName))
            {
                watcher.Changed += Watcher_Changed;
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            OnConfigChanged?.Invoke(sender);

            Reload();
        }

        /// <summary>
        /// 无效
        /// </summary>
        public event Action<object> OnConfigChanged;

        public T Get<T>(string key, T defaultValue = default)
        {
            var type = typeof(T);

            if (type.IsValueType || type == typeof(string))
            {
                if (Configs.TryGetValue(key, out string value))
                {
                    return value.CastTo(defaultValue);
                }
            }
            else if (ConnectionStrings.TryGetValue(key, out ConnectionStringSettings settings))
            {
                return settings.MapTo<T>(defaultValue);
            }

            return defaultValue;
        }

        /// <summary> 重新加载配置 </summary>
        public void Reload()
        {
            ConnectionStrings.Clear();

            var connectionStrings = Config.ConnectionStrings;

            foreach (ConnectionStringSettings stringSettings in connectionStrings.ConnectionStrings)
            {
                ConnectionStrings.Add(stringSettings.Name, stringSettings);
            }

            Configs.Clear();

            var appSettings = Config.AppSettings;

            foreach (KeyValueConfigurationElement kv in appSettings.Settings)
            {
                Configs.Add(kv.Key, kv.Value);
            }
        }
    }
}
#endif