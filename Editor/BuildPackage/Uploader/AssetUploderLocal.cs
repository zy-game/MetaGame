using System;
using System.Collections;
using System.IO;

namespace GameEditor.BuildAsset
{
    public class AssetUploderLocal : AssetUploader
    {
        public AssetUploderLocal(UploaderParam param) : base(param)
        {
        }

        public override void UploadAsset(Action<int, bool> uploadFunc)
        {
            if (uploadAssets.Count == 0) return;
            owner.StartCor(CopyToDisk(uploadFunc));
        }

        private IEnumerator CopyToDisk(Action<int, bool> uploadFunc)
        {
            int count = 0;
            foreach (var v in uploadAssets)
            {
                string remotePath = uploderParam.ModuleUrl + v.name;
                string localPath = uploderParam.buildAssetRoot + v.name;
                FileInfo removteFi = new FileInfo(remotePath);
                if (!removteFi.Directory.Exists)
                    Directory.CreateDirectory(removteFi.Directory.FullName);               
                if (File.Exists(remotePath))
                    File.Delete(remotePath);
                File.Copy(localPath, remotePath);
                uploadFunc?.Invoke(++count, false);
                v.progress = 1;
                owner.Repaint();

                removeFileList.UpdateItem(v.fileItem);
                yield return new Wait(0);
            }

            if (delItems.Count > 0)
            {
                foreach (var v in delItems)
                {
                    string remotePath = uploderParam.ModuleUrl + v.name;
                    if (File.Exists(remotePath))
                    {
                        File.Delete(remotePath);
                        uploadFunc?.Invoke(++count, false);
                        v.progress = 1;
                        owner.Repaint();
                        removeFileList.files.Remove(v.fileItem);
                        yield return new Wait(0);
                    }
                }
            }

            string fileListPath = uploderParam.ModuleUrl + "files.txt";
            if (File.Exists(fileListPath))
                File.Delete(fileListPath);
            UnityEngine.Debug.Log(uploderParam.ModuleUrl);
            File.WriteAllText(uploderParam.ModuleUrl + "files.txt",JsonObject.Serialize(removeFileList));
            uploadFunc?.Invoke(uploadAssets.Count, true);
        }

        public override void UpdateRemoteVersionConfig(string content, Action<bool> callback)
        {
            File.WriteAllText(uploderParam.address.downloadPath + uploderParam.platformName + "/version.txt", content);
            callback(true);
        }


    }
}