using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GameEditor.BuildAsset
{
    public class CodeUploderRemote : AssetUploderRemote
    {

        public CodeUploderRemote(UploaderParam param) : base(param)
        {

        }

        public override void UploadAsset(Action<int, bool> uploadFunc)
        {
            List<FileItem> tempItems = new List<FileItem>();
            foreach (var item in uploadAssets)
            {
                if(item.isUpload) tempItems.Add(item);
            }
            uploadAssets = tempItems;

            tempItems = new List<FileItem>();
            foreach (var del in delItems)
            {
                if (del.isUpload) tempItems.Add(del);
            }
            delItems = tempItems;

            base.UploadAsset(uploadFunc);
        }

        protected override void FileUploadFinished(Action<int, bool> uploadFunc)
        {
            foreach (var item in uploadAssets)
            {
                if (item.isUpload)
                {
                    removeFileList.UpdateItem(item.fileItem);
                }
            }

            owner.StartCor(DeleteFile(uploadFunc));
        }

        //删除多余的资源
        private IEnumerator DeleteFile(Action<int, bool> uploadFunc)
        {
            foreach (var del in delItems)
            {
                string fileName = uploderParam.FileNameStarts + del.name;
                using var request = RequestDelete(fileName);
                yield return new WaitWebRequest(request.SendWebRequest());
                if (request.result == UnityWebRequest.Result.Success)
                {
                    uploadCount++;
                    uploadFunc(uploadCount, false);
                    del.progress = 1;
                    owner.Repaint();
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "删除资源错误", "返回");
                    Debug.LogError("删除资源错误:" + fileName);
                    yield break;
                }
            }

            foreach (var del in base.delItems)
            {
                if (del.isUpload)
                {
                    if (!removeFileList.files.Remove(del.fileItem))
                    {
                        EditorUtility.DisplayDialog("错误", "资源列表配置移除错误", "返回");
                        Debug.LogError("资源列表配置移除错误:" + del.fileItem.name);
                        yield break;
                    }
                }
            }
            owner.StartCor(UploadFileList(uploadFunc,System.Text.Encoding.UTF8.GetBytes(JsonObject.Serialize(removeFileList))));
        }
      
    }
}