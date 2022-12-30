using GameFramework.Runtime.Assets;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class AppConst
{
    public const string AppName = "metac";
    public static LocalCommonConfig config;


    /// <summary>
    /// 获取平台名称
    /// </summary>
    public static string PlatformName
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
            }
            return config.editorPlatform;
        }
    }

    public static int NetworkHeartbeatInternalTime
    {
        get
        {
            return 10000;
        }
    }

    private static int mobilePlatform = -1;
    public static bool IsMobilePlatform
    {
        get
        {
            if (mobilePlatform == -1)
                mobilePlatform = Application.isMobilePlatform ? 1 : 0;
            return mobilePlatform == 1;
        }
    }

    public static string ConfigPath
    {
        get
        {
            return DataPath + config.configModuleName + "/";
        }
    }

    public static string ConfigExtension
    {
        get
        {
            return ".ini";
        }
    }

    /// <summary>
    /// 数据存放目录
    /// </summary>
    public static string DataPath
    {
        get
        {
            string game = AppName.ToLower();

            if (Application.isMobilePlatform)
            {
                return Application.persistentDataPath + "/" + game + "/";
            }
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                int i = Application.dataPath.LastIndexOf('/');
                return Application.dataPath.Substring(0, i + 1) + game + "/";
            }
            if (Application.isEditor)
            {
                string lcoalPath = Application.dataPath;
                return $"{Application.dataPath}/../{game}/";// lcoalPath.Substring(0, lcoalPath.Length - 6) + game + "/";
            }
            return Application.dataPath + "/" + game + "/";
        }
    }

    /// <summary>
    /// 应用程序内容路径
    /// </summary>
    public static string AppContentPath
    {
        get
        {
            string path = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    path = "jar:file://" + Application.dataPath + "!/assets/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    path = Application.dataPath + "/Raw/";
                    break;
                default:
                    path = Application.dataPath + "/StreamingAssets/";
                    break;
            }
            return path;
        }
    }

    /// <summary>
    /// 本地配置路径
    /// </summary>
    public static string AppConfigPath
    {
        get
        {
            return AppContentPath + "localConfig.txt";
        }
    }

    /// <summary>
    /// 本地资源路径
    /// </summary>
    public static string AppAssetPath
    {
        get
        {
            return AppContentPath + "assets/";
        }
    }

    /// <summary>
    /// 网络数据缓存路径
    /// </summary>
    public static string WebDataCachePath
    {
        get
        {
            return DataPath + "remotecache/";
        }
    }

    //加载本地配置
    public static IEnumerator LoadLocalConfig()
    {
        string path = AppConfigPath;
        using UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            config = JsonObject.Deserialize<LocalCommonConfig>(request.downloadHandler.text);
            yield break;
        }
        else
        {
            Debug.LogError("加载本地配置错误!");
        }
    }

    public static string GetModuleUrl(string moduleName)
    {
        return config.asseturl + PlatformName + "/" + moduleName + "/";
    }

    //lua代码包下载地址
    private static string luaCodeUrl;
    public static string LuaCodeUrl
    {
        get
        {
            if (string.IsNullOrEmpty(luaCodeUrl))
                luaCodeUrl = config.asseturl + config.codeModuleName + "/";
            return luaCodeUrl;
        }
    }

    public static string GetModulePath(string moduleName)
    {
        return DataPath + moduleName + "/";
    }
}
