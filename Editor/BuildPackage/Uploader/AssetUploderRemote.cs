using GameFramework.Runtime.Assets;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Text;
using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace GameEditor.BuildAsset
{
    public class AssetUploderRemote : AssetUploader
    {
        private class UploadApi
        {
            public const string create = "resource/v1/matter/create/";
            public const string delete = "resource/v1/matter/delete/";
            public const string done = "resource/v1/matter/#name/done";
        }

        public class RequestUploadData
        {
            public string sid;
            public string pid;
            public string name;
            public string md5;
            public int size;
            public string type;
        }

        public class UploadHeaders
        {

        }

        protected const int maxUploderCount = 8;//ͬʱ�ϴ�����
        protected const int connectLimitCount = 5;//���ӳ���
        protected int uploadCount;
        protected int curUploderCorCount = 0;
        protected int uploderCorFinishedCount = 0;
        protected bool uploderComplete;
        protected bool uploderErr;

        public AssetUploderRemote(UploaderParam param) : base(param)
        {

        }

        public override void UploadAsset(Action<int, bool> uploadFunc)
        {
            uploadItems = new();
            foreach (var item in uploadAssets)
            {
                uploadItems.Enqueue(item);
            }
            curUploderCorCount = maxUploderCount;
            if (uploadItems.Count < maxUploderCount) curUploderCorCount = uploadItems.Count;
            for (int i = 0; i < curUploderCorCount; i++)
            {
                owner.StartCor(Uploder(uploadFunc));
            }
        }

        protected UnityWebRequest PostJson(string url, string token, string json)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            request.SetRequestHeader("Authorization", token);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            return request;
        }

        //�����ļ�
        protected UnityWebRequest RequestCreate(string fileName, string md5, int size)
        {
            RequestUploadData data = new RequestUploadData
            {
                sid = uploderParam.address.sid,
                pid = uploderParam.address.pid,
                name = fileName,
                md5 = md5,
                size = size,
                type = "application/octet-stream",
            };
            string url= uploderParam.address.uploadPath + UploadApi.create;
            return PostJson(url, uploderParam.address.token, JsonObject.Serialize(data));
        }

        //ɾ���ļ�
        protected UnityWebRequest RequestDelete(string fileName)
        {
            RequestUploadData data = new RequestUploadData
            {
                sid = uploderParam.address.sid,
                pid = uploderParam.address.pid,
                name = fileName,
            };
            string url = uploderParam.address.uploadPath + UploadApi.delete;
            return PostJson(url, uploderParam.address.token, JsonObject.Serialize(data));
        }

        //�ϴ��ļ�
        protected UnityWebRequest RequestUpload(string content, string fileName, byte[] fileBytes = null)
        {
            var map = JsonObject.Deserialize<JObject>(content);
            if (!map.ContainsKey("data")) return null;
            var requestData = map["data"];
            string link = requestData["up_link"].ToString();
            var headers = requestData["headers"];
            fileBytes = fileBytes == null ? File.ReadAllBytes(uploderParam.buildAssetRoot + fileName) : fileBytes;
            UnityWebRequest uploaderRequest = UnityWebRequest.Put(link, fileBytes);
            foreach (var v in headers)
            {
                var jp = v.ToObject<JProperty>();

                string value = jp.Value[0].ToString();
                string key = jp.Name;
                uploaderRequest.SetRequestHeader(key, value);
            }
            return uploaderRequest;
        }

        //֪ͨ�ļ��ϴ��ɹ�
        protected UnityWebRequest RequestDone(string fileName)
        {
            return PostJson(uploderParam.address.uploadPath + UploadApi.done.Replace("#name",fileName), uploderParam.address.token,"");
        }

        protected IEnumerator Uploder(Action<int, bool> uploadFunc)
        {
            while (uploadItems.Count > 0)
            {
                FileItem item = uploadItems.Dequeue();
                item.downloadCount++;
                string fileName = uploderParam.FileNameStarts + item.name;
                using UnityWebRequest request = RequestCreate(fileName, item.fileItem.md5, item.fileItem.size);
                yield return new WaitWebRequest(request.SendWebRequest());
                bool isErr = false;
                string errMsg = string.Empty;
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string content = request.downloadHandler.text;

                    using UnityWebRequest uploaderRequest = RequestUpload(content, item.name);
                    if (uploaderRequest == null)
                    {
                        Debug.Log("error:" + request.url);
                        isErr = true;
                        errMsg = content;
                    }
                    else
                    {
                        uploaderRequest.SendWebRequest();
                        while (!uploaderRequest.isDone)
                        {
                            item.progress = uploaderRequest.uploadProgress;
                            owner.Repaint();
                            yield return new Wait(0.03f);
                        }
                        if (uploaderRequest.result == UnityWebRequest.Result.Success)
                        {
                            uploadCount++;
                            uploadFunc(uploadCount, false);
                            item.progress = 1;
                            owner.Repaint();
                        }
                        else
                        {
                            isErr = true;
                            errMsg = uploaderRequest.error;
                            Debug.Log("error:"+ request.url);
                        }
                    }
                }
                else
                {
                    Debug.Log("error:" + request.url);
                    isErr = true;
                    errMsg = request.error;
                }

                if (isErr)
                {
                    if (item.downloadCount > connectLimitCount)
                    {
                        Debug.LogError("��Դ�ϴ�����:" + item.name + "\n" + errMsg);
                        uploderErr = true;
                    }
                    else
                    {
                        //���������ϴ�
                        uploadItems.Enqueue(item);
                    }
                }

                if (uploderErr) break;
            }

            uploderCorFinishedCount++;
            //�ϴ�����
            if (uploderCorFinishedCount >= curUploderCorCount) 
            {
                FileUploadFinished(uploadFunc);
            }
        }

        protected virtual void FileUploadFinished(Action<int, bool> uploadFunc)
        {
            owner.StartCor(DeleteFile(uploadFunc));
        }

        //ɾ���������Դ
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
                    EditorUtility.DisplayDialog("����", "ɾ����Դ����", "����");
                    Debug.LogError("ɾ����Դ����:"+ fileName);
                    Debug.LogError(request.error);
                    yield break;
                }
            }

            owner.StartCor(UploadFileList(uploadFunc));
        }

        //�ϴ��ļ��б�
        protected IEnumerator UploadFileList(Action<int, bool> uploadFunc,byte[] bytes=null)
        {
            if (uploderComplete) yield break;
            uploderComplete = true;

            //----�ϴ��ļ��б�
            if (uploderErr)
            {
                EditorUtility.DisplayDialog("����", "��Դ�ϴ�����", "ȷ��");
                yield break;
            }
            
            //------------�ϴ��ļ��б�
            string fileListPath = uploderParam.buildAssetRoot + "files.txt";
            string fileListMD5 ="files";
            int fileListSize = (int)new FileInfo(fileListPath).Length;
            using var request = RequestCreate(uploderParam.FileNameStarts+"files.txt", fileListMD5, fileListSize);
            yield return new WaitWebRequest(request.SendWebRequest());
            bool isError = false;
            string errMsg = string.Empty;
            if (request.result == UnityWebRequest.Result.Success)
            {
                string content = request.downloadHandler.text;
                using UnityWebRequest uploaderRequest = RequestUpload(content, "files.txt", bytes);
                yield return new WaitWebRequest(uploaderRequest.SendWebRequest());
                if (uploaderRequest.result == UnityWebRequest.Result.Success)
                {
                    uploadFunc(uploadAssets.Count, true);
                }
                else
                {
                    isError = true;
                    errMsg = uploaderRequest.error;
                }
            }
            else
            {
                isError = true;
                errMsg = request.error;
            }

            if (isError)
            {
                EditorUtility.DisplayDialog("����", "�ļ��б��ϴ�����", "ȷ��");
                Debug.LogError(errMsg);
            }
        }

        public override void UpdateRemoteVersionConfig( string content, Action<bool> callback)
        {
            owner.StartCor(UpdateRemoteVersionConfigCor(content, callback));
        }

        //����Զ�̰汾����
        private IEnumerator UpdateRemoteVersionConfigCor(string versionContent, Action<bool> callback)
        {
            string fileName = uploderParam.platformName + "/version.txt";
            string fileListMD5 = "version";
            int fileListSize = versionContent.Length;
            using var request = RequestCreate(fileName, fileListMD5, fileListSize);
            yield return new WaitWebRequest(request.SendWebRequest());
            bool isError = false;
            string errMsg = string.Empty;
            if (request.result == UnityWebRequest.Result.Success)
            {
                string content = request.downloadHandler.text;
                using UnityWebRequest uploaderRequest = RequestUpload(content, null, Encoding.UTF8.GetBytes(versionContent));
                yield return new WaitWebRequest(uploaderRequest.SendWebRequest());
                if (uploaderRequest.result == UnityWebRequest.Result.Success)
                {

                }
                else
                {
                    isError = true;
                    errMsg = uploaderRequest.error;
                }
            }
            else
            {
                isError = true;
                errMsg = request.error;
            }

            if (isError)
            {
                EditorUtility.DisplayDialog("����", "�ļ��б��ϴ�����", "ȷ��");
                Debug.LogError(errMsg);
            }
            else
            {
                using var uploadFinishedResult = RequestDone(fileName);
                yield return new WaitWebRequest(uploadFinishedResult.SendWebRequest());
                callback(true);
            }
        }
    }
}