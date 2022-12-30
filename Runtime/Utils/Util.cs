using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace GameFramework.Runtime
{
    public static class Util
    {
        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string md5file(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        public static bool IsNull(UnityEngine.Object obj)
        {
            if (obj == null) return true;
            if (!obj) return true;
            return false;
        }

        public static void LuaThread(XLua.LuaFunction func)
        {
            new Thread(() =>
            {
                func.Call();
            }).Start();

        }

        public static byte[] StringToByte(string content)
        {
            if (string.IsNullOrEmpty(content)) return new byte[0];
            return Encoding.UTF8.GetBytes(content);
        }

        public static UnityWebRequest PostJson(string url, string json)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.uploadHandler = new UploadHandlerRaw(StringToByte(json));
            return request;
        }

        private static long lastTicks;
        public static void LogTicks(string tag, bool start = false)
        {
            long cur = DateTime.Now.Ticks;
            if (start)
            {
                lastTicks = 0;
                Debug.Log(tag + ":deltaTicks start");
            }
            else Debug.Log(tag + ":" + (cur - lastTicks) / 10000);
            lastTicks = cur;
        }

        public static string ReadText(string path)
        {
            if (File.Exists(path))
                return File.ReadAllText(path);

            return string.Empty;
        }

        //aescbcpkcs5 加密
        public static string Encrypt(string toEncrypt, string key, string iv)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] ivArray = Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            rDel.Padding = PaddingMode.Zeros;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        //aescbcpkcs5 解密
        public static string Decrypt(string toDecrypt, string key, string iv)
        {
            byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] ivArray = Encoding.UTF8.GetBytes(iv);
            byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.IV = ivArray;
            rDel.Mode = CipherMode.CBC;
            rDel.Padding = PaddingMode.Zeros;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
        }

        public static bool IsPointUI()
        {
            int finger = -1;
#if !UNITY_EDITOR
            finger = 0;
#endif
            return EventSystem.current.IsPointerOverGameObject(finger);
        }

        public static RaycastHit Raycast(Vector3 position, string layerName, Camera camera = null)
        {
            int finger = -1;
#if !UNITY_EDITOR
            finger = 0;
#endif
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                return default;
            }
            if (EventSystem.current.IsPointerOverGameObject(finger))
            {
                return default;
            }
            Ray ray = (camera == null ? Camera.main : camera).ScreenPointToRay(position);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000, LayerMask.GetMask(layerName)))
            {
                return hitInfo;
            }
            return default;
        }


        public static RaycastHit Raycast(Vector3 point, int layerMask, Camera camera )
        {
            Ray ray = camera.ScreenPointToRay(point);
            Physics.Raycast(ray, out RaycastHit hitInfo,1000,layerMask);
            return hitInfo;
        }


        public static RaycastHit Raycast(Vector3 point, Vector3 dir, int layerMask, float dis = 100)
        {
            Physics.Raycast(point, dir, out RaycastHit hitInfo, dis, layerMask);
            return hitInfo;
        }

        public static void SetShaderGlobalInt(string name, int v)
        {
            CorManager.Instance.DelayCall(null, 0, () =>
            {
                Shader.SetGlobalInteger(name, v);
            });
        }

        public static void SetShaderGlobalFloat(string name, float v)
        {
            Shader.SetGlobalFloat(name, v);
        }
    }
}