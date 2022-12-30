using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameFramework.Runtime.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GameEditor.BuildAsset
{
    public class UploaderParam
    {
        public BaseEditorWindow owner;
        public UploadAddressItem address;
        public string buildAssetRoot;
        public string moduleName;
        public string platformName;
        public int localVersion;

        public string ModuleUrl
        {
            get
            {
                string path = address.downloadPath;
                if (!string.IsNullOrEmpty(platformName))
                    path += platformName + "/";
                if (!string.IsNullOrEmpty(moduleName))
                    path += moduleName + "/";
                return path;
            }
        }

        public string FileNameStarts
        {
            get
            {
                return platformName + "/" + moduleName + "/";
            }
        }
    }

    public abstract class AssetUploader
    {
        protected UploaderParam uploderParam;
        protected AssetFileEntity localFileList;
        protected AssetFileEntity removeFileList;
        protected BaseEditorWindow owner;
        public List<FileItem> uploadAssets;//Ҫ�ϴ�����Դ
        public List<FileItem> delItems;//Ҫɾ������Դ
        protected Queue<FileItem> uploadItems;//��Ҫ���µ��ļ�

        public AssetUploader(UploaderParam param)
        {
            this.uploderParam = param;
            this.owner = param.owner;
            string fileName = param.buildAssetRoot + "files.txt";
            if (!File.Exists(fileName))
            {
                EditorUtility.DisplayDialog("����", "�Ҳ��������ļ��б�����", "����");
                return;
            }
            string fileListContent = File.ReadAllText(fileName);
            localFileList = JsonObject.Deserialize<AssetFileEntity>(fileListContent);
        }

        //����Զ���ļ��б�
        public virtual void LoadRemoteFileList(Action<AssetFileEntity> action)
        {
            owner.StartCor(LoadRomoteFileListCor(action));            
        }

        private IEnumerator LoadRomoteFileListCor(Action<AssetFileEntity> action)
        {
            string url = uploderParam.ModuleUrl+ "files.txt";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return new WaitWebRequest(request.SendWebRequest());
            if (request.result == UnityWebRequest.Result.Success)
            {
                string content = request.downloadHandler.text;
                removeFileList = JsonObject.Deserialize<AssetFileEntity>(content);
                CheckAsset();
                action?.Invoke(removeFileList);
            }
            else
            {
                removeFileList = new AssetFileEntity();
                removeFileList.files = new List<AssetFileEntity.FileItem>();
                CheckAsset();
                action?.Invoke(removeFileList);
            }
        }

        private void CheckAsset()
        {
            uploadAssets = new List<FileItem>();
            foreach (var item in localFileList.files)
            {
                if (!removeFileList.ContainsMd5(item))
                {
                    FileItem fileItem = new FileItem();
                    fileItem.name = item.name;
                    fileItem.fileItem = item;
                    uploadAssets.Add(fileItem);
                }
            }

            delItems = new List<FileItem>();
            foreach (var item in removeFileList.files)
            {
                if (!localFileList.ContainsName(item))
                {
                    FileItem fileItem = new FileItem();
                    fileItem.name = item.name;
                    fileItem.fileItem = item;
                    delItems.Add(fileItem);
                }
            }
        }

        //�ϴ���Դ
        public abstract void UploadAsset(Action<int,bool> uploadFunc);

        //����Զ�̰汾����
        public virtual void UpdateRemoteVersionConfig(string content,Action<bool> callback)
        {
            
        }

        public virtual void Clear() 
        {
            owner.StopCorAll();
        }

        public bool Equals(UploaderParam param)
        {
            return param.address.Equals(uploderParam.address)&& param.localVersion==uploderParam.localVersion&&param.buildAssetRoot==uploderParam.buildAssetRoot;
        }

        //Ҫ���µ��ļ�
        public class FileItem
        {
            public string name;
            public float progress;
            public int downloadCount;//�������ش���
            public AssetFileEntity.FileItem fileItem;
            public bool isUpload=true;//�Ƿ�Ҫ�ϴ�
        }
    }
}