using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class AssetVersion
    {
        public static AssetVersion remoteVersion;
        public static AssetVersion localVersion;
        public static AssetFileEntity remoteCodeFileList;
        public static AssetFileEntity localCodeFileList;


        public int packageVersion;
        public Dictionary<string, int> versionMap;
        public Dictionary<string, string[]> dependsMap;
        public Dictionary<string, string[]> codeLinkMap;

        public AssetVersion()
        {
            versionMap = new Dictionary<string, int>();
            dependsMap = new Dictionary<string, string[]>();
            codeLinkMap = new Dictionary<string, string[]>();
        }

        public void UpdateVersion(string name, int version)
        {
            if (versionMap.ContainsKey(name))
            {
                versionMap[name] = version;
                return;
            }
            versionMap.Add(name, version);
        }

        public void UpdateDepends(string name, string[] depends)
        {
            if (versionMap.ContainsKey(name))
            {
                dependsMap[name] = depends;
                return;
            }
            dependsMap.Add(name, depends);
        }

        public void UpdateLinkCodePackage(string name, string[] codePackage)
        {
            if (codeLinkMap.ContainsKey(name))
            {
                codeLinkMap[name] = codePackage;
                return;
            }
            codeLinkMap.Add(name, codePackage);
        }

        /// <summary>
        /// 查找模块版本
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        public int FindVersion(string moduleName)
        {
            if (versionMap.ContainsKey(moduleName))
                return versionMap[moduleName];
            return 0;
        }

        //深度依赖查询
        public int FindDepends(string moduleName, List<string> dependList)
        {
            if (dependList == null) return 0;
            if (dependsMap == null || !dependsMap.ContainsKey(moduleName)) return 0;
            string[] depends = dependsMap[moduleName];
            if (depends != null)
            {
                foreach (var d in depends)
                {
                    if (dependList.Contains(d))
                        continue;
                    dependList.Add(d);
                    FindDepends(moduleName, dependList);
                }
            }
            return dependList.Count;
        }

        public string[] LoadCodePackageNames(string moduleName)
        {
            if (!codeLinkMap.ContainsKey(moduleName))
                return new string[0];
            return codeLinkMap[moduleName];
        }

        // 查询资源包依赖
        public string[] FindDenpends(string moduleName)
        {
            List<string> dependList = new List<string>();
            FindDepends(moduleName, dependList);
            return dependList.ToArray();
        }

        //获取资源更新模块
        public static List<string> GetUpdateModules(string modeleName, out bool error)
        {
            Debug.Log("update module:" + modeleName);
            string[] depends = remoteVersion.FindDenpends(modeleName);
            List<string> updateModules = new List<string>();

            //模块内代码更新检测
            Dictionary<string, string[]> codeCheckMap = new();
            codeCheckMap.Add(modeleName, remoteVersion.LoadCodePackageNames(modeleName));
            foreach (var v in depends)
            {
                codeCheckMap.Add(v, remoteVersion.LoadCodePackageNames(v));
            }

            foreach (var m in codeCheckMap)
            {
                string mn = m.Key;
                foreach (var codeName in m.Value)
                {
                    var item = remoteCodeFileList.FindItem(codeName + AppConst.config.buildLuaCodeExtName);
                    if (item == null)
                    {
                        error = true;
                        Debug.LogError("代码配置上找不到模块关联的代码包:moduleName:" + mn + "  codePackageName:" + codeName);
                        return null;
                    }

                    if (!localCodeFileList.ContainsMd5(item))
                    {
                        updateModules.Add(mn);
                        break;
                    }
                }
            }

            //模块版本检测
            if (!CompareVersion(modeleName))
            {
                if (!updateModules.Contains(modeleName))
                    updateModules.Add(modeleName);
            }
            foreach (var name in depends)
            {
                if (!CompareVersion(name))
                {
                    if (!updateModules.Contains(name))
                        updateModules.Add(name);
                }
            }

            error = false;
            return updateModules;
        }

        //获取需要更新的资源包
        public static string[] GetUpdateModulesName(string modeleName)
        {
            if (remoteVersion == null)
            {
                Debug.LogError("远程配置为空");
                return new string[0];
            }
            if (localVersion == null)
            {
                Debug.LogError("本地配置为空");
                return new string[0];
            }
            List<string> updateModules = new List<string>();
            string[] depneds = remoteVersion.FindDenpends(modeleName);
            if (!CompareVersion(modeleName))
                updateModules.Add(modeleName);
            foreach (var name in depneds)
            {
                if (!CompareVersion(name))
                {
                    if (!updateModules.Contains(name))
                        updateModules.Add(name);
                }
            }
            return updateModules.ToArray();
        }

        //对比版本
        public static bool CompareVersion(string moduleName)
        {
            int local = localVersion.FindVersion(moduleName);
            int remote = remoteVersion.FindVersion(moduleName);
            return local == remote;
        }


    }
}
