using System.Net.Http;
using System.Collections;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using GameFramework.Runtime.Game;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;
using System.Net;
using UnityEngine.UI;

namespace GameFramework
{

    //[特性(布局种类、有序、字符集、自动)]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class FileDlg
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public String file = null;
        public int maxFile = 0;
        public String fileTitle = null;
        public int maxFileTitle = 0;
        public String initialDir = null;
        public String title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public String defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public String templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }
    //调用系统函数
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileDlg : FileDlg
    {

    }
    public class OpenFileDialog
    {
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileDlg ofd);
    }
    public class SaveFileDialog
    {
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] SaveFileDlg ofd);
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class SaveFileDlg : FileDlg
    {

    }
    public sealed class TaskCompletionSource : TaskCompletionSource<int>
    {
        public static readonly TaskCompletionSource Void = CreateVoidTcs();

        public TaskCompletionSource(object state) : base(state)
        {
        }

        public TaskCompletionSource()
        {
        }

        public bool TryComplete() => this.TrySetResult(0);

        public void Complete() => this.SetResult(0);

        // todo: support cancellation token where used
        public bool SetUncancellable() => true;

        public override string ToString() => "TaskCompletionSource[status: " + this.Task.Status.ToString() + "]";

        static TaskCompletionSource CreateVoidTcs()
        {
            var tcs = new TaskCompletionSource();
            tcs.TryComplete();
            return tcs;
        }
    }

    [XLua.LuaCallCSharp]
    public static partial class Utility
    {



    }


    [XLua.LuaCallCSharp]
    public static class StaticMethod
    {
        class ColliderEvent : MonoBehaviour
        {
            private void OnCollisionEnter(Collision other)
            {

            }
            private void OnCollisionEnter2D(Collision2D other)
            {

            }
            private void OnCollisionExit(Collision other)
            {

            }
            private void OnCollisionExit2D(Collision2D other)
            {

            }
            private void OnCollisionStay(Collision other)
            {

            }
            private void OnCollisionStay2D(Collision2D other)
            {

            }
            private void OnTriggerEnter(Collider other)
            {

            }
            private void OnTriggerEnter2D(Collider2D other)
            {

            }
            private void OnTriggerExit(Collider other)
            {

            }
            private void OnTriggerExit2D(Collider2D other)
            {

            }
            private void OnTriggerStay(Collider other)
            {

            }
            private void OnTriggerStay2D(Collider2D other)
            {

            }
        }
        class ApiAdapter : MonoBehaviour
        {
            class UpdateEventContext
            {
                public string name;
                public GameFrameworkAction callback;
            }
            class QuitEventContext
            {
                public string name;
                public GameFrameworkAction callback;
            }
            class ForcusEventContext
            {
                public string name;
                public GameFrameworkAction<bool> callback;
            }
            class RollTimeEventContext
            {
                public string name;
                public float time;
                public float interval;
                public bool forward;
                public float now;
                public GameFrameworkAction<float> callback;
            }
            class DelayEventContext
            {
                public string name;
                public float time;
                public float start;
                public GameFrameworkAction callback;
            }

            private List<QuitEventContext> quitEvents = new List<QuitEventContext>();
            private List<DelayEventContext> delayEvents = new List<DelayEventContext>();
            private List<UpdateEventContext> updateEvents = new List<UpdateEventContext>();
            private List<ForcusEventContext> forcusEvents = new List<ForcusEventContext>();
            private List<UpdateEventContext> lateupdateEvents = new List<UpdateEventContext>();
            private List<UpdateEventContext> fixedupdateEvents = new List<UpdateEventContext>();
            private List<RollTimeEventContext> rolltimeEvents = new List<RollTimeEventContext>();

            private void OnApplicationQuit()
            {
                for (int i = quitEvents.Count - 1; i >= 0; i--)
                {
                    QuitEventContext context = quitEvents[i];
                    if (context == null)
                    {
                        forcusEvents.RemoveAt(i);
                        continue;
                    }
                    context.callback?.Invoke();
                }
            }
            private void OnApplicationFocus(bool focusStatus)
            {
                for (int i = forcusEvents.Count - 1; i >= 0; i--)
                {
                    ForcusEventContext context = forcusEvents[i];
                    if (context == null)
                    {
                        forcusEvents.RemoveAt(i);
                        continue;
                    }
                    context.callback?.Invoke(focusStatus);
                }
            }
            private void FixedUpdate()
            {
                for (int i = fixedupdateEvents.Count - 1; i >= 0; i--)
                {
                    UpdateEventContext context = fixedupdateEvents[i];
                    if (context == null)
                    {
                        fixedupdateEvents.RemoveAt(i);
                        continue;
                    }
                    context.callback?.Invoke();
                }
            }
            private void LateUpdate()
            {
                for (int i = lateupdateEvents.Count - 1; i >= 0; i--)
                {
                    UpdateEventContext context = lateupdateEvents[i];
                    if (context == null)
                    {
                        lateupdateEvents.RemoveAt(i);
                        continue;
                    }
                    context.callback?.Invoke();
                }
            }
            private void Update()
            {
                CheckDelayRun();
                CheckRollTime();
                for (int i = updateEvents.Count - 1; i >= 0; i--)
                {
                    UpdateEventContext context = updateEvents[i];
                    if (context == null)
                    {
                        updateEvents.RemoveAt(i);
                        continue;
                    }
                    context.callback?.Invoke();
                }
            }

            private void CheckRollTime()
            {
                for (int i = rolltimeEvents.Count - 1; i >= 0; i--)
                {
                    RollTimeEventContext context = rolltimeEvents[i];
                    if (context == null)
                    {
                        rolltimeEvents.RemoveAt(i);
                        continue;
                    }
                    if (context.forward)
                    {
                        context.now += context.interval;
                        context.callback?.Invoke(context.now);
                        if (context.now >= context.time)
                        {
                            rolltimeEvents.Remove(context);
                        }
                    }
                    else
                    {
                        context.now -= context.interval;
                        context.callback?.Invoke(context.now);
                        if (context.now <= 0)
                        {
                            rolltimeEvents.Remove(context);
                        }
                    }
                }
            }

            private void CheckDelayRun()
            {
                for (int i = delayEvents.Count - 1; i >= 0; i--)
                {
                    DelayEventContext context = delayEvents[i];
                    if (context == null)
                    {
                        delayEvents.RemoveAt(i);
                        continue;
                    }
                    if (Time.realtimeSinceStartup - context.start < context.time)
                    {
                        continue;
                    }
                    context.callback?.Invoke();
                    delayEvents.Remove(context);
                }
            }

            public void ListenerUpdate(string name, GameFrameworkAction callback)
            {
                updateEvents.Add(new UpdateEventContext() { name = name, callback = callback });
            }
            public void ListenerLateUpdate(string name, GameFrameworkAction callback)
            {
                lateupdateEvents.Add(new UpdateEventContext() { name = name, callback = callback });
            }
            public void ListenerFixedUpdate(string name, GameFrameworkAction callback)
            {
                fixedupdateEvents.Add(new UpdateEventContext() { name = name, callback = callback });
            }
            public void ListenerForcus(string name, GameFrameworkAction<bool> callback)
            {
                forcusEvents.Add(new ForcusEventContext() { name = name, callback = callback });
            }
            public void ListenerQuit(string name, GameFrameworkAction callback)
            {
                quitEvents.Add(new QuitEventContext() { name = name, callback = callback });
            }
            public void ListenerDelayRun(string name, float time, GameFrameworkAction callback)
            {
                delayEvents.Add(new DelayEventContext() { name = name, time = time, start = Time.realtimeSinceStartup, callback = callback });
            }
            public void ListenerRollTime(string name, float time, float interval, bool isForwad, GameFrameworkAction<float> callback)
            {
                rolltimeEvents.Add(new RollTimeEventContext() { name = name, time = time, interval = interval, forward = isForwad, callback = callback, now = isForwad ? 0 : time });
            }

            public void Remove(string name)
            {
                QuitEventContext quitEvent = quitEvents.Find(x => x.name == name);
                if (quitEvent != null)
                {
                    quitEvents.Remove(quitEvent);
                }
                DelayEventContext delayEvent = delayEvents.Find(x => x.name == name);
                if (delayEvent != null)
                {
                    delayEvents.Remove(delayEvent);
                }
                UpdateEventContext updateEvent = updateEvents.Find(x => x.name == name);
                if (updateEvent != null)
                {
                    updateEvents.Remove(updateEvent);
                }
                updateEvent = lateupdateEvents.Find(x => x.name == name);
                if (updateEvent != null)
                {
                    lateupdateEvents.Remove(updateEvent);
                }
                updateEvent = fixedupdateEvents.Find(x => x.name == name);
                if (updateEvent != null)
                {
                    fixedupdateEvents.Remove(updateEvent);
                }
                ForcusEventContext forcusEvent = forcusEvents.Find(x => x.name == name);
                if (forcusEvent != null)
                {
                    forcusEvents.Remove(forcusEvent);
                }
                RollTimeEventContext rollTimeEvent = rolltimeEvents.Find(x => x.name == name);
                if (rollTimeEvent != null)
                {
                    rolltimeEvents.Remove(rollTimeEvent);
                }
            }
        }
        public static Transform EmptyTransform = null;
        public static GameObject EmptyGameObject = null;
        private static ApiAdapter api;

        private static void EnsureApiObject()
        {
            if (api != null)
            {
                return;
            }
            GameObject adapter = new GameObject("Api");
            GameObject.DontDestroyOnLoad(adapter);
            api = adapter.AddComponent<ApiAdapter>();
        }

        public static void AddColliderEnterEvent(GameEntity entity)
        {

        }

        public static void AddColliderStayEvent(GameEntity entity)
        {

        }

        public static void AddColliderExitEvent(GameEntity entity)
        {

        }

        public static void AddForcusEvent(string name, GameFrameworkAction<bool> callback)
        {
            EnsureApiObject();
            api.ListenerForcus(name, callback);
        }

        public static void AddAppQuitEvent(string name, GameFrameworkAction callback)
        {
            EnsureApiObject();
            api.ListenerQuit(name, callback);
        }
        public static void AddDelayRun(string name, float time, GameFrameworkAction callback)
        {
            EnsureApiObject();
            api.ListenerDelayRun(name, time, callback);
        }
        public static void AddDelayRun(string name, float time, object args, GameFrameworkAction<object> callback)
        {
            AddDelayRun(name, time, () => { callback(args); });
        }
        public static void AddRollTime(string name, float time, float interval, bool isForwad, GameFrameworkAction<float> callback)
        {
            EnsureApiObject();
            api.ListenerRollTime(name, time, interval, isForwad, callback);
        }
        public static void AddFixedUpdateEvent(string name, GameFrameworkAction callback)
        {
            EnsureApiObject();
            api.ListenerFixedUpdate(name, callback);
        }
        public static void AddLateUpdateEvent(string name, GameFrameworkAction callback)
        {
            EnsureApiObject();
            api.ListenerLateUpdate(name, callback);
        }
        public static void AddUpdateEvent(string name, GameFrameworkAction callback)
        {
            EnsureApiObject();
            api.ListenerUpdate(name, callback);
        }

        public static void RemoveEvent(string name)
        {
            EnsureApiObject();
            api.Remove(name);
        }

        public static string UploadBinary(byte[] bytes, string url, string headers)
        {
            if (bytes == null || bytes.Length <= 0)
            {
                return "data cannot be null";
            }
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Method = "Put";
            request.Proxy = null;
            request.Timeout = 5000;
            request.KeepAlive = false;
            request.ContentLength = bytes.Length;
            request.ServicePoint.Expect100Continue = false;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.ServicePoint.ConnectionLimit = 65500;
            request.AllowWriteStreamBuffering = false;
            Dictionary<string, string> map = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(headers);
            foreach (var item in map)
            {
                if (item.Key == "Content-Type")
                {
                    request.ContentType = item.Value;
                }
                else
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            Stream stream = request.GetRequestStream();
            stream.Write(bytes);
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static string Upload(string filePath, string url, string headers)
        {
            if (File.Exists(filePath) == false)
            {
                return "error";
            }
            byte[] bytes = File.ReadAllBytes(filePath);
            return UploadBinary(bytes, url, headers);
        }

        public static string UploadTexture(Texture2D texture, string url, string header)
        {
            if (texture == null || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(header))
            {
                return "the texture or url and header cannot be null";
            }
            return UploadBinary(texture.EncodeToPNG(), url, header);
        }

        /// <summary>
        /// 截屏
        /// </summary>
        /// <param name="width">截屏宽度</param>
        /// <param name="height">截屏高度</param>
        /// <param name="offset_x">截屏起始位置</param>
        /// <param name="offset_y">截屏起始位置</param>
        /// <param name="completed">完成回调</param>
        public static void Screenshot(int width, int height, int offset_x, int offset_y, GameFrameworkAction<Texture2D> completed)
        {
            CorManager.Instance.StartCoroutine(getScreenTexture(width, height, offset_x, offset_y, completed));
        }

        /// <summary>
        /// 截屏
        /// </summary>
        /// <param name="width">截屏宽度</param>
        /// <param name="height">截屏高度</param>
        /// <param name="offset_x">截屏起始位置</param>
        /// <param name="offset_y">截屏起始位置</param>
        /// <param name="completed">完成回调</param>
        public static void ScreenshotAsSrpite(int width, int height, int offset_x, int offset_y, GameFrameworkAction<Sprite> completed)
        {
            CorManager.Instance.StartCoroutine(getScreenTexture(width, height, offset_x, offset_y, args =>
            {
                Sprite sprite = Sprite.Create(args, new Rect(0, 0, args.width, args.height), Vector2.one / 2, 100);
                completed(sprite);
            }));
        }

        static IEnumerator getScreenTexture(int width, int height, int offset_x, int offset_y, GameFrameworkAction<Texture2D> completed)
        {
            yield return new WaitForEndOfFrame();
            //需要正确设置好图片保存格式
            Texture2D t = new Texture2D(width, height, TextureFormat.RGB24, true);
            //按照设定区域读取像素；注意是以左下角为原点读取
            t.ReadPixels(new Rect(offset_x, offset_y, width, height), 0, 0, false);
            t.Apply();
            completed(t);
        }

        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static string GetMd5HashAsBytes(byte[] input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(input);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static int Angle(Vector3 dir, Vector3 form, Vector3 to)
        {
            Vector3 distance = form - to;
            if (distance.z > 0)
            {
                return (int)Vector3.Angle(dir, distance);
            }
            return 360 - (int)Vector3.Angle(dir, distance);
        }

        public static IList GenericList(Type type)
        {
            return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        public static string SaveFile(string tilet, string fileName, string extension)
        {
            SaveFileDlg pth = new SaveFileDlg();
            pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
            // pth.filter = $"{extension} (*.{extension})";//文件类型
            pth.file = new string(new char[256]);
            pth.maxFile = pth.file.Length;
            pth.file = fileName;//保存文件的默认名字  
            pth.fileTitle = new string(new char[64]);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = PlayerPrefs.HasKey("EditorSavePath") ? PlayerPrefs.GetString("EditorSavePath") : "C:\\";  // 文件的默认保存路径
            pth.title = tilet;//
            pth.defExt = extension;
            pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            string filepath = string.Empty;
            if (SaveFileDialog.GetSaveFileName(pth))
            {
                filepath = pth.file;//选择需要保存的文件路径;  
                filepath = filepath.Replace('\\', '/');
                PlayerPrefs.GetString("EditorSavePath", filepath);
            }
            return filepath;
        }
        /// <summary>
        /// 打开文件
        /// </summary>
        public static string OpenFile(string tilet, string extension)
        {
            OpenFileDlg pth = new OpenFileDlg();
            pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
            // pth.filter = $"{extension} (*.{extension})|*.{extension}";
            pth.file = new string(new char[256]);
            pth.maxFile = pth.file.Length;
            pth.fileTitle = new string(new char[64]);
            pth.maxFileTitle = pth.fileTitle.Length;
            pth.initialDir = PlayerPrefs.HasKey("EditorSavePath") ? PlayerPrefs.GetString("EditorSavePath") : "C:\\";  // 默认路径
            pth.title = tilet;
            pth.defExt = extension;
            pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
            //0x00080000   是否使用新版文件选择窗口
            //0x00000200   是否可以多选文件
            string filepath = string.Empty;
            if (OpenFileDialog.GetOpenFileName(pth))
            {
                filepath = pth.file;//选择需要打开的文件路径;  
                PlayerPrefs.GetString("EditorSavePath", filepath);
            }
            return filepath;
        }

        public static Color GetScreenColor(Vector2 InputPosition, RectTransform rectT, Texture2D texture)
        {
            CanvasScaler scaler = rectT.GetComponentInParent<CanvasScaler>();
            float width = rectT.rect.width * rectT.localScale.x;
            float heigth = rectT.rect.height * rectT.localScale.y;

            //屏幕分辨率到自定义的 CanvasScaler 分辨率的坐标转换
            Vector2 screenPosition = new Vector2(InputPosition.x / Screen.width * scaler.referenceResolution.x,
                                                 InputPosition.y / Screen.height * scaler.referenceResolution.y);
            //UI空间坐标到屏幕坐标的转换
            //图片范围（四个角坐标）
            Vector2 v0 = new Vector2(rectT.localPosition.x + (scaler.referenceResolution.x - width) / 2
                , rectT.localPosition.y + (scaler.referenceResolution.y - heigth) / 2);
            Vector2 v1 = new Vector2(rectT.localPosition.x + (scaler.referenceResolution.x + width) / 2
                , rectT.localPosition.y + (scaler.referenceResolution.y + heigth) / 2);

            if (screenPosition.x < v1.x && screenPosition.x > v0.x && screenPosition.y < v1.y && screenPosition.y > v0.y)
            {
                //点击到图片区域
                float x = (screenPosition.x - v0.x) / width * texture.width;
                float y = (screenPosition.y - v0.y) / heigth * texture.height;
                return texture.GetPixel((int)x, (int)y);
            }
            else
            {
                Debug.Log("点击到图片之外");
                return Color.white;
            }
        }


        public static Texture2D OpenTexture(string filePath, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(ReadFileData(filePath));
            return texture;
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static byte[] ReadFileData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                UnityEngine.Debug.Log("not find the file:" + filePath);
                return null;
            }
            byte[] bytes = File.ReadAllBytes(filePath);
            UnityEngine.Debug.Log("read the file:" + filePath + " length:" + bytes.Length);
            return bytes;
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Task<byte[]> ReadFileDataAsync(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(filePath))
            {
                return null;
            }
            return File.ReadAllBytesAsync(filePath);
        }

        public static string ReadFileAllText(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                return string.Empty;
            }
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        public static void WriteFileData(string filePath, byte[] bytes)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Task WriteFileDataAsync(string filePath, byte[] bytes)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return File.WriteAllBytesAsync(filePath, bytes);
        }

        public static byte[] SerializeMessage(short opcode, byte[] bytes)
        {
            byte[] op = BitConverter.GetBytes(opcode);
            return op.Concat(bytes).ToArray();
        }




        public static RaycastHit Raycast(Vector3 position)
        {

            //声明变量，用于保存信息
            RaycastHit hitInfo;
            //发射射线，起点是当前物体位置，方向是世界前方
            if (Physics.Raycast(position, Vector3.forward, out hitInfo))
            {
                return hitInfo;
            }
            return default;
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
    }
    [XLua.LuaCallCSharp]
    public static class ExtensionMethod
    {

        public static void SetParent(this GameObject gameObject, Transform parent)
        {
            gameObject.SetParent(parent, Vector3.zero);
        }

        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position)
        {
            gameObject.SetParent(parent, position, Vector3.zero);
        }

        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position, Vector3 rotation)
        {
            gameObject.SetParent(parent, position, rotation, Vector3.one);
        }

        public static void SetParent(this GameObject gameObject, Transform parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = Quaternion.Euler(rotation);
            gameObject.transform.localScale = scale;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                return;
            }
            if (rectTransform.anchorMax == Vector2.one)
            {
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }

        public static void SetParent(this GameObject gameObject, GameObject parent)
        {
            gameObject.SetParent(parent, Vector3.zero);
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, Vector3 position)
        {
            gameObject.SetParent(parent, position, Vector3.zero);
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, Vector3 position, Vector3 rotation)
        {
            gameObject.SetParent(parent, position, rotation, Vector3.one);
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Transform transform = parent == null ? null : parent.transform;
            gameObject.SetParent(transform, position, rotation, scale);
        }


        public static Vector3 ToScreenPosition(this GameObject gameObject, Camera camera, Canvas canvas)
        {
            //世界转屏幕 Camera_Main世界的摄像机
            Vector3 pos = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            Vector3 worldPoint;
            //屏幕转UI  ui(当前的canvas)  _camera_UiCamera(UI的摄像机)
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.GetComponent<RectTransform>(), pos, camera, out worldPoint))
            {
                return worldPoint;
            }
            return Vector2.zero;
        }

        public static Vector3 ToWorldPosition(this GameObject gameObject, Camera camera)
        {
            return camera.ScreenToWorldPoint(gameObject.transform.localPosition);
        }

        public static Vector3 ToWorldPosition(this Vector3 point, Camera camera)
        {
            return camera.ScreenToWorldPoint(point);
        }
        public static RaycastHit Raycast(Vector3 position, int layerMask, Camera camera = null)
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
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000, layerMask))
            {
                return hitInfo;
            }
            return default;
        }

        public static RaycastHit Raycast(Vector3 point, Vector3 dir, int layerMask,float dis=100)
        {
            Physics.Raycast(point, dir, out RaycastHit hitInfo, dis, layerMask);
            return hitInfo;
        }

        public static void MoveTo(this GameObject gameObject, Vector3 position, float time)
        {
            gameObject.transform.MoveTo(position, time);
        }

        public static void MoveTo(this Component component, Vector3 position, float time)
        {
            component.transform.DOMove(position, time);
        }

        public static void MovePath(this GameObject gameObject, Vector3[] paths, float time)
        {
            if (gameObject == null)
            {
                return;
            }
            gameObject.transform.MovePath(paths, time);
        }


        public static void MovePath(this Component component, Vector3[] paths, float time)
        {
            if (component == null)
            {
                return;
            }
            if (paths == null || paths.Length <= 0)
            {
                return;
            }
            component.DOKill();
            component.transform.DOPath(paths, time).SetEase(Ease.Linear);
        }
        public static void MovePathAndLookPath(this GameObject gameObject, Vector3[] paths, float time)
        {
            if (gameObject == null)
            {
                return;
            }
            gameObject.transform.MovePathAndLookPath(paths, time);
        }
        public static void MovePathAndLookPath(this Component component, Vector3[] paths, float time)
        {
            if (component == null)
            {
                return;
            }
            if (paths == null || paths.Length <= 0)
            {
                return;
            }
            void OnChange(int index)
            {
                component.transform.DOLookAt(paths[index], 0.2f, AxisConstraint.Y, null);
            }
            component.DOKill();
            component.transform.DOPath(paths, time).SetEase(Ease.Linear).OnWaypointChange(OnChange);
        }

        public static void LookRotation(this GameObject gameObject, Vector3 dir)
        {
            Quaternion quaternion = Quaternion.LookRotation(dir);
            quaternion.x = 0;
            quaternion.z = 0;
            gameObject.transform.rotation = quaternion;
        }

        public static void LookRotation(this Component component, Vector3 dir)
        {
            Quaternion quaternion = Quaternion.LookRotation(dir);
            quaternion.x = 0;
            quaternion.z = 0;
            component.transform.rotation = quaternion;
        }

        public static int Round(this float value)
        {
            return (int)MathF.Round(value);
        }

        public static GameObject GetChild(this GameObject basic, string name)
        {
            if (basic == null)
            {
                return default;
            }
            Transform transform = basic.transform.Find(name);
            if (transform == null)
            {
                return default;
            }
            return transform.gameObject;
        }

        public static GameObject GetChild(this Transform basic, string name)
        {
            if (basic == null)
            {
                return default;
            }
            Transform transform = basic.Find(name);
            if (transform == null)
            {
                return default;
            }
            return transform.gameObject;
        }

        public static GameObject GetChild(this Component basic, string name)
        {
            if (basic == null)
            {
                return default;
            }
            Transform transform = basic.transform.Find(name);
            if (transform == null)
            {
                return default;
            }
            return transform.gameObject;
        }

        public static void SetText(this GameObject gameObject, string text)
        {
            if (gameObject == null)
            {
                return;
            }
            UnityEngine.UI.Text com = gameObject.GetComponent<UnityEngine.UI.Text>();
            if (com == null)
            {
                return;
            }
            com.text = text;
        }

        public static string GetText(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return default;
            }
            UnityEngine.UI.Text com = gameObject.GetComponent<UnityEngine.UI.Text>();
            if (com == null)
            {
                return default;
            }
            return com.text;
        }

        public static void SetSprite(this GameObject gameObject, Sprite sprite)
        {
            if (gameObject == null)
            {
                return;
            }
            UnityEngine.UI.Image com = gameObject.GetComponent<UnityEngine.UI.Image>();
            if (com == null)
            {
                return;
            }
            com.sprite = sprite;
        }

        public static Sprite GetSprite(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return default;
            }
            UnityEngine.UI.Image com = gameObject.GetComponent<UnityEngine.UI.Image>();
            if (com == null)
            {
                return default;
            }
            return com.sprite;
        }
    }
}
