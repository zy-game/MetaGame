using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//本地配置
public class LocalCommonConfig
{
    [Desc("版本号")]
    public int version;
    [Desc("资源扩展名")]
    public string assetBundleExtName = ".unity3d";
    [Desc("远程地址")]
    public string remoteurl;
    [Desc("远程下载配置密钥")]
    public string remotekey;
    [Desc("使用本地编译资源")]
    public bool localReleaseAsset;
    [Desc("帧率")]
    public int gameFrameRate=45;
    [Desc("公共模块名")]
    public string commonModuleName = "common";
    [Desc("主模块名")]
    public string mainModuleName = "main";
    [Desc("配置模块名")]
    public string configModuleName = "config";
    [Desc("Lua代码扩展名")]
    public string buildLuaCodeExtName = ".luabytes";
    [Desc("编辑器平台")]
    public string editorPlatform = "Windows";
    //代码模块名
    public string codeModuleName = "luapackage";
    [Desc("压缩密码")]
    public string compressPassword = "123456";
    [Desc("编辑器下使用luaBytes")]
    public bool editorUseLuaBytes = false;
    [Desc("编辑器下使用资源更新")]
    public bool editorUpdateAssets = false;
    [Desc("编辑器下加载资源包")]
    public bool editorLoadAssetBundle = false;

    //http 连接地址
    [Newtonsoft.Json.JsonIgnore]
    public string httpUrl;
    //主websocket连接地址
    [Newtonsoft.Json.JsonIgnore]
    public string websocketUrl;
    //资源服务器地址
    [Newtonsoft.Json.JsonIgnore]
    public string asseturl;
    //配置地址
    [Newtonsoft.Json.JsonIgnore]
    public string configUrl;

    //使用测试地址
    public bool useTestUrl = false;
    public List<TestUrl> testUrls;

#if UNITY_EDITOR
    public static LocalCommonConfig Get()
    {
        string path = AppConst.AppConfigPath;
        if (!System.IO.File.Exists(path)) return new LocalCommonConfig();
        string content = System.IO.File.ReadAllText(path);
        return JsonObject.Deserialize<LocalCommonConfig>(content);
    }
#endif

}

//??????
public class TestUrl
{
    public string desc;
    public string assetUrl;
    public string httpUrl;
    public string websocketUrl;
    public string configUrl;
    public bool isUse;
}

public class DescAttribute : Attribute  
{
    public readonly string desc;
    public readonly int type;

    public DescAttribute(string desc) 
    {
        this.desc = desc;
    }

    public DescAttribute(string desc, int type)
    {
        this.desc = desc;
        this.type = type;
    }
}