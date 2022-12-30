using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using XLua;
using GameFramework.Runtime.Assets;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// UI管理器
    /// </summary>
    /// <remarks>每一个UILayout表示一个Canvas,使用UGUI的顺序渲染特点管理UI层级</remarks>
    public sealed class UIManager : IUIManager
    {
        private GameWorld gameWorld;
        private Dictionary<int, Canvas> layers;
        private Dictionary<string, IUIHandler> caches;
        private Dictionary<string, IUIHandler> handlers;
        private const int MESSAGE_BOX_UI_LAYER = 999;
        private const int LOADING_UI_LAYER = 998;
        private Vector2 screenSize = Vector2.zero;

        public Camera UICamera { get; private set; }

        public IUIHandler current
        {
            get;
            private set;
        }

        public UIManager(GameWorld world, Vector2 screenSize)
        {
            this.gameWorld = world;
            this.screenSize = screenSize;
            UICamera = GameObject.Instantiate<Camera>(Resources.Load<Camera>("Camera/UICamera"));
            UICamera.name = world.name + "_UICamera";
            UICamera.gameObject.SetParent(StaticMethod.EmptyTransform);
            layers = new Dictionary<int, Canvas>();
            caches = new Dictionary<string, IUIHandler>();
            handlers = new Dictionary<string, IUIHandler>();
        }

        /// <summary>
        /// 设置是否显示当前layout
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            if (UICamera == null)
            {
                return;
            }
            UICamera.gameObject.SetActive(active);
        }

        public IUIHandler OpenUI(string path, string name, LuaTable table)
        {
            if (caches.TryGetValue(name, out IUIHandler uiHandler))
            {
                caches.Remove(name);
                handlers.Add(name, uiHandler);
                uiHandler.OnEnable();
                return uiHandler;
            }

            AssetHandle handle = ResourcesManager.Instance.Load(path);
            if (handle == null)
            {
                return default;
            }
            GameObject gameObject = handle.CreateGameObject(null, name);
            if (gameObject == null)
            {
                return default;
            }
            gameObject.AddComponent<AssetBundleBehaviour>().AddAssetHandle(handle);
            uiHandler = new CommonUIFormHandler(this, gameObject.name, gameObject, table);
            ToLayer(uiHandler, uiHandler.layer);
            handlers.Add(gameObject.name, uiHandler);
            uiHandler.Start();
            uiHandler.OnEnable();
            return uiHandler;
        }

        public IUIHandler OpenUI(string name, LuaTable table)
        {
            if (caches.TryGetValue(name, out IUIHandler uiHandler))
            {
                caches.Remove(name);
                handlers.Add(name, uiHandler);
                uiHandler.OnEnable();
                return uiHandler;
            }
            AssetHandle handle = ResourcesManager.Instance.Load(name);
            if (handle == null)
            {
                return default;
            }
            GameObject gameObject = handle.CreateGameObject();
            if (gameObject == null)
            {
                return default;
            }
            gameObject.AddComponent<AssetBundleBehaviour>().AddAssetHandle(handle);
            gameObject.AddComponent<AssetBundleBehaviour>().AddAssetHandle(handle);
            uiHandler = new CommonUIFormHandler(this, gameObject.name, gameObject, table);
            ToLayer(uiHandler, uiHandler.layer);
            handlers.Add(gameObject.name, uiHandler);
            uiHandler.Start();
            uiHandler.OnEnable();
            return uiHandler;
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IUIHandler OpenUI(string name)
        {
            if (caches.TryGetValue(name, out IUIHandler uiHandler))
            {
                uiHandler.OnEnable();
                caches.Remove(name);
                handlers.Add(name, uiHandler);
                return uiHandler;
            }
            AssetHandle handle = ResourcesManager.Instance.Load(name);
            if (handle == null)
            {
                return default;
            }
            GameObject gameObject = handle.CreateGameObject();
            if (gameObject == null)
            {
                return default;
            }
            gameObject.AddComponent<AssetBundleBehaviour>().AddAssetHandle(handle);
            LuaTable table = LuaManager.Instance.GetTable(gameObject.name);
            uiHandler = new CommonUIFormHandler(this, gameObject.name, gameObject, table);
            ToLayer(uiHandler, uiHandler.layer);
            handlers.Add(gameObject.name, uiHandler);
            uiHandler.Start();
            uiHandler.OnEnable();
            return uiHandler;
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isCache"></param>
        public void CloseUI(string name, bool isCache = false)
        {
            IUIHandler handler = GetUIHandler(name);
            if (handler == null)
            {
                return;
            }
            handlers.Remove(name);
            if (isCache)
            {
                handler.OnDisable();
                caches.Add(name, handler);
                return;
            }
            handler.Dispose();
        }

        /// <summary>
        /// 获取UI对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IUIHandler GetUIHandler(string name)
        {
            if (handlers.TryGetValue(name, out IUIHandler handler))
            {
                return handler;
            }
            return default;
        }

        /// <summary>
        /// 清理所有UI
        /// </summary>
        public void Clear()
        {
            foreach (var item in handlers.Values)
            {
                item.Dispose();
            }
            handlers.Clear();
        }

        /// <summary>
        /// 释放当前UI管理器
        /// </summary>
        public void Dispose()
        {
            Clear();
            //UniversalAdditionalCameraData universalAdditionalCameraData = gameWorld.WorldCamera.GetComponent<UniversalAdditionalCameraData>();
            //universalAdditionalCameraData.cameraStack.Remove(UICamera);
            GameObject.DestroyImmediate(UICamera.gameObject);
            UICamera = null;
        }

        /// <summary>
        /// 将UI设置到指定的层级
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="layer"></param>
        public Canvas ToLayer(IUIHandler handler, int layer)
        {
            if (!layers.TryGetValue(layer, out Canvas canvas))
            {
                canvas = CreateCanvas(layer);
                layers.Add(layer, canvas);
            }
            handler.gameObject.SetParent(canvas.transform);
            handler.canvas = canvas;
            if (current == null || handler.layer >= current.layer)
            {
                current = handler;
            }
            return canvas;
        }
        public Canvas ToLayer(GameObject handle, int layer)
        {
            if (!layers.TryGetValue(layer, out Canvas canvas))
            {
                canvas = CreateCanvas(layer);
                layers.Add(layer, canvas);
            }
            handle.SetParent(canvas.transform);
            return canvas;
        }

        public ILoading OnLoading()
        {
            CommonLoading loading = new CommonLoading();
            if (!layers.TryGetValue(LOADING_UI_LAYER, out Canvas canvas))
            {
                canvas = CreateCanvas(LOADING_UI_LAYER);
                layers.Add(LOADING_UI_LAYER, canvas);
            }
            loading.gameObject.SetParent(canvas.transform);
            return loading;
        }

        private CommonAwaiting awaiting = null;
        public IAwaiting OnAwaitLoading()
        {
            if (awaiting != null)
            {
                awaiting.OnEnable();
                return awaiting;
            }
            awaiting = new CommonAwaiting();
            if (!layers.TryGetValue(LOADING_UI_LAYER, out Canvas canvas))
            {
                canvas = CreateCanvas(LOADING_UI_LAYER);
                layers.Add(LOADING_UI_LAYER, canvas);
            }
            awaiting.gameObject.SetParent(canvas.transform);
            return awaiting;
        }
        class MsgUnit
        {
            public string text;
            public string tilet;
            public GameFrameworkAction ok, cancel;
        }
        private IMessageBox currentMsgBox;
        private Queue<MsgUnit> msgQueue = new Queue<MsgUnit>();
        public IMessageBox OnMsgBox(string text, GameFrameworkAction ok = null, GameFrameworkAction cancel = null)
        {
            if (currentMsgBox != null)
            {
                msgQueue.Enqueue(new MsgUnit() { text = text, tilet = "Tips", ok = ok, cancel = cancel });
                return currentMsgBox;
            }
            currentMsgBox = new CommonMessageBox();
            void Cancel()
            {
                currentMsgBox.Dispose();
                currentMsgBox = null;
                CheckWaitingShowMsgBox();
                cancel?.Invoke();
            }
            void Entry()
            {
                currentMsgBox.Dispose();
                currentMsgBox = null;
                CheckWaitingShowMsgBox();
                ok?.Invoke();
            }
            if (!layers.TryGetValue(MESSAGE_BOX_UI_LAYER, out Canvas canvas))
            {
                layers.Add(MESSAGE_BOX_UI_LAYER, canvas = CreateCanvas(MESSAGE_BOX_UI_LAYER));
            }
            currentMsgBox.Show(canvas.transform, "Tips", text, Entry, cancel == null ? null : Cancel);
            return currentMsgBox;
        }

        void CheckWaitingShowMsgBox()
        {
            if (msgQueue.Count <= 0)
            {
                return;
            }
            MsgUnit msgUnit = msgQueue.Dequeue();
            OnMsgBox(msgUnit.text, msgUnit.ok, msgUnit.cancel);
        }

        private Canvas CreateCanvas(int layer)
        {
            Canvas canvas = GameObject.Instantiate<Canvas>(Resources.Load<Canvas>("Camera/Canvas"));
            canvas.name = "Canvas";
            canvas.gameObject.SetParent(UICamera.transform);
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = this.screenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0;
            canvas.worldCamera = UICamera;
            canvas.sortingOrder = layer;
            return canvas;
        }

        public void ClearMessageBox()
        {
            msgQueue.Clear();
            if (!layers.TryGetValue(MESSAGE_BOX_UI_LAYER, out Canvas canvas))
            {
                return;
            }
            GameObject.DestroyImmediate(canvas.gameObject);
            layers.Remove(MESSAGE_BOX_UI_LAYER);
        }

        public void ClearLoading()
        {
            if (!layers.TryGetValue(LOADING_UI_LAYER, out Canvas canvas))
            {
                return;
            }
            GameObject.DestroyImmediate(canvas.gameObject);
            layers.Remove(LOADING_UI_LAYER);
        }

        public void CloseWait()
        {
            if (awaiting == null)
            {
                return;
            }
            awaiting.Dispose();
        }
    }
}