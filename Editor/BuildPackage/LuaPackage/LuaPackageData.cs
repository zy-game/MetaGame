using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LuaPackageData : ScriptableObject
{
    [System.Serializable]
    public class BuildLuaItem
    {
        public Object asset;
        public string name;
    }
    public string buildRootPath;
    public List<BuildLuaItem> items;
}