using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��������
public class LocalCommonConfig
{
    [Desc("�汾��")]
    public int version;
    [Desc("��Դ��չ��")]
    public string assetBundleExtName = ".unity3d";
    [Desc("Զ�̵�ַ")]
    public string remoteurl;
    [Desc("Զ������������Կ")]
    public string remotekey;
    [Desc("ʹ�ñ��ر�����Դ")]
    public bool localReleaseAsset;
    [Desc("֡��")]
    public int gameFrameRate=45;
    [Desc("����ģ����")]
    public string commonModuleName = "common";
    [Desc("��ģ����")]
    public string mainModuleName = "main";
    [Desc("����ģ����")]
    public string configModuleName = "config";
    [Desc("Lua������չ��")]
    public string buildLuaCodeExtName = ".luabytes";
    [Desc("�༭��ƽ̨")]
    public string editorPlatform = "Windows";
    //����ģ����
    public string codeModuleName = "luapackage";
    [Desc("ѹ������")]
    public string compressPassword = "123456";
    [Desc("�༭����ʹ��luaBytes")]
    public bool editorUseLuaBytes = false;
    [Desc("�༭����ʹ����Դ����")]
    public bool editorUpdateAssets = false;
    [Desc("�༭���¼�����Դ��")]
    public bool editorLoadAssetBundle = false;

    //http ���ӵ�ַ
    [Newtonsoft.Json.JsonIgnore]
    public string httpUrl;
    //��websocket���ӵ�ַ
    [Newtonsoft.Json.JsonIgnore]
    public string websocketUrl;
    //��Դ��������ַ
    [Newtonsoft.Json.JsonIgnore]
    public string asseturl;
    //���õ�ַ
    [Newtonsoft.Json.JsonIgnore]
    public string configUrl;

    //ʹ�ò��Ե�ַ
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