using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GameFramework.Runtime;
using GameFramework.Runtime.Assets;

namespace GameEditor.BuildAsset
{
    public class AssetDirConfig
    {
        public const string assetDirName = "ArtistRes";
        //��Դ�����ļ���·��
        public const string configDirPath = "Assets/" + assetDirName + "/assetBuildConfig/";
        //���ù�����·��
        public const string configDirProjectPath= assetDirName + "/assetBuildConfig/";
        public const string configName = "BuildAssetConfig.asset";
        public const string assetRootPath = "Assets/" + assetDirName + "/";//��Դ��Ŀ¼

        public const string luaDir = "luaConfig";
        public const string luaConfigName = "luaPackageConfig.asset";

        public const string assetProjectPathConfig = "assetProjectConfig.la";
    }

    public class AssetManager : Editor
    {      
        [MenuItem("Tools/��Դ����", false, 0)]
        public static void Open()
        {
            if (!Directory.Exists(AssetDirConfig.assetRootPath))
            {
                if (EditorUtility.DisplayDialog("��ʾ", "û���ҵ���Դ��Ŀ¼,�Ƿ񴴽�?", "ȷ��", "ȡ��"))
                {
                     Directory.CreateDirectory(AssetDirConfig.assetRootPath);
                }
                else
                    return;
            }

            bool isAssetProject = IsAssetProject();//�Ƿ�����Դ����

            string configPath = AssetDirConfig.configDirPath + AssetDirConfig.configName;
            BuildAssetConfig assetConfig = AssetDatabase.LoadAssetAtPath<BuildAssetConfig>(configPath);
            if (assetConfig == null)//�����ļ�������
            {
                if (isAssetProject)//�������Դ�����򴴽������ļ�
                {
                    if (!Directory.Exists(AssetDirConfig.configDirPath))
                        Directory.CreateDirectory(AssetDirConfig.configDirPath);
                    var config = CreateInstance<BuildAssetConfig>();
                    AssetDatabase.CreateAsset(config, configPath);
                    AssetDatabase.Refresh();
                    assetConfig = AssetDatabase.LoadAssetAtPath<BuildAssetConfig>(configPath);
                }
            }

            if (isAssetProject)
                AssetBundleBuildSetting.Open(assetConfig);
            else
                AssetBundleBuildSetting.Open();
        }

        public static bool IsAssetProject()
        {
            bool isAssetProject = false;//�Ƿ�����Դ����
            if (File.Exists(AssetDirConfig.configDirPath + AssetDirConfig.configName))
                isAssetProject = true;
            return isAssetProject;
        }

        public static string GetAssetProjectPath()
        {
            string path = "./" + AssetDirConfig.assetRootPath + "/" + AssetDirConfig.assetProjectPathConfig;
            if (!File.Exists(path)) return string.Empty;
            return File.ReadAllText(path);
        }

        //�����ļ��б�
        public static void GenerateFileList(string path, int version,string moduleName,string[] filePattern=null)
        { 
            path = path.Replace(@"\", "/");
            AssetFileEntity fileEntity = new AssetFileEntity();
            fileEntity.files = new List<AssetFileEntity.FileItem>();

            string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            List<string> list = new List<string>();
            foreach (var file in files)
            {
                bool isAdd = false;
                if (filePattern != null)
                {
                    foreach (var pattern in filePattern)
                    {
                        if (file.EndsWith(pattern))
                        {
                            isAdd = true;
                        }
                    }
                }
                else
                    isAdd = true;
                if (isAdd)
                    list.Add(file);
            }
            files = list.ToArray();

            foreach (var file in files)
            {
                string name = file.Replace(@"\", "/").Replace(path, "");
                string md5 = Util.md5file(file);
                int size = (int)new FileInfo(file).Length;
                var item = new AssetFileEntity.FileItem();
                item.name = name;
                item.md5 = md5;
                item.size = size;
                fileEntity.files.Add(item);
            }
            fileEntity.version = version;
            fileEntity.moduleName = moduleName;
            File.WriteAllText(path + "files.txt", JsonObject.Serialize(fileEntity));
        }
    }
}
