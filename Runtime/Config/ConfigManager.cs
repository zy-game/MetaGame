using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace GameFramework.Runtime.Config
{
    /// <summary>
    /// 配置表管理器
    /// </summary>
    public sealed class ConfigManager : Singleton<ConfigManager>, IConfigManager
    {
        private Dictionary<string, IConfigTable> configs = new Dictionary<string, IConfigTable>();

        public void Update()
        {
        }

        /// <summary>
        /// 加载配置表
        /// </summary>
        /// <param name="configName">配置名</param>
        /// <returns></returns>
        public IConfigTable LoadConfig(string configName)
        {
            if (configs.TryGetValue(configName, out IConfigTable table))
            {
                return table;
            }
            DefaultConfigTable defaultConfigTable = new DefaultConfigTable();
            defaultConfigTable.LoadConfig(configName);
            if (defaultConfigTable.Count <= 0)
            {
                return default;
            }
            configs.Add(configName, defaultConfigTable);
            return table;
        }

        public string LoadLuaConfig(string configName)
        {
            configName = configName.EndsWith(AppConst.ConfigExtension) ? configName : configName + AppConst.ConfigExtension;
            byte[] bytes = GZip.unzip(Utility.ReadFileData(AppConst.ConfigPath + configName), AppConst.config.compressPassword);
            if (bytes == null || bytes.Length <= 0)
            {
                return string.Empty;
            }
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 卸载配置
        /// </summary>
        /// <param name="configName">配置名</param>
        public void UnloadConfig(string configName)
        {
            IConfigTable table = GetConfigure(configName);
            if (table == null)
            {
                return;
            }
            table.Dispose();
            configs.Remove(configName);
        }

        /// <summary>
        /// 获取指定的配置表
        /// </summary>
        /// <param name="configName">配置名</param>
        /// <returns></returns>
        public IConfigTable GetConfigure(string configName)
        {
            if (configs.TryGetValue(configName, out IConfigTable table))
            {
                return table;
            }
            return default;
        }

        /// <summary>
        /// 清理所有配置
        /// </summary>
        public void Clear()
        {
            foreach (var item in configs.Values)
            {
                item.Dispose();
            }
            configs.Clear();
        }

        /// <summary>
        /// 指定的配置表是否已经加载
        /// </summary>
        /// <param name="configName">配置名</param>
        /// <returns></returns>
        public bool HasLoadConfig(string configName)
        {
            return configs.ContainsKey(configName);
        }
    }
}