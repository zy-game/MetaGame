using System.Collections.Generic;
using System.IO;

public class LocalData : Singleton<LocalData>
{
    private class Data
    {
        public Dictionary<string, object> dataMap;
    }

    private const string fileName = "localData.cfg";

    private Data data;
    private string content;
    private XLua.LuaFunction luaUpdateFunc;

    public LocalData()
    {
        string path = AppConst.DataPath + fileName;
        if (!File.Exists(path))
        {
            data = new Data();
            data.dataMap = new Dictionary<string, object>();
            content = JsonObject.Serialize(data);
        }
        else
        {
            byte[] bts = File.ReadAllBytes(path);
            bts = GZip.unzip(bts, AppConst.config.compressPassword);
            content = System.Text.Encoding.UTF8.GetString(bts);
            data = JsonObject.Deserialize<Data>(content);
        }
    }

    public T Get<T>(string key)
    {
        if (!data.dataMap.ContainsKey(key)) return default;
        var v = data.dataMap[key];
        if (!(v is T)) return default;
        return (T)v;

    }

    public void Set(string key, object value)
    {
        if (!data.dataMap.ContainsKey(key))
            data.dataMap.Add(key, value);
        else
            data.dataMap[key] = value;
    }

    public void Remove(string key)
    {
        if (!data.dataMap.ContainsKey(key)) return;
        data.dataMap.Remove(key);
    }

    public void Save()
    {
        if (data.dataMap.Count == 0) return;
        string json = JsonObject.Serialize(data);
        Save(json);
        if (luaUpdateFunc != null)
            luaUpdateFunc.Action(json);
    }

    public void Save(string json)
    {
        content = json;
        byte[] bts = System.Text.Encoding.UTF8.GetBytes(json);
        string path = AppConst.DataPath + fileName;
        if (Directory.Exists(AppConst.DataPath))
            Directory.CreateDirectory(AppConst.DataPath);
        bts = GZip.zip(bts, AppConst.config.compressPassword);
        File.WriteAllBytes(path, bts);
    }

    public string GetContent()
    {
        return content;
    }

    public void RegLuaFunc(XLua.LuaFunction func)
    {
        luaUpdateFunc = func;
    }
}
