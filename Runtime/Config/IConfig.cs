using System.Collections.Generic;
namespace GameFramework.Runtime.Config
{
    /// <summary>
    /// 配置表
    /// </summary>
    public interface IConfig : GObject
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        /// <remarks>同一类型的配置的唯一ID</remarks>
        int id { get; }

        /// <summary>
        /// 配置名
        /// </summary>
        string name { get; }
    }
}
