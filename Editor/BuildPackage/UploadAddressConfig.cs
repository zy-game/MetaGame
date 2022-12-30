using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameEditor.BuildAsset
{
    [System.Serializable]
    public class UploadAddressItem
    {
        public string alias;
        public string sid;
        public string pid;
        public string token;
        public string downloadPath;
        public string uploadPath;
    }

    public class UploadAddressConfig : ScriptableObject
    {
        public List<UploadAddressItem> paths;
        public int current;
    }
}
