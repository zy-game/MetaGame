using System.Linq;
using System.Collections.Generic;
using System;
using XLua;
using UnityEngine;
using GameFramework.Runtime.Behaviour;
using GameFramework.Runtime.Assets;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// 游戏世界
    /// </summary>
    public class GameWorld : IWorld
    {
        public static Vector2 DefaultScreenSize = new Vector2(1080, 1920);
        class LuaAdapter
        {
            private IWorld world;
            private LuaTable table;
            private LuaFunction start;
            private LuaFunction dispose;
            private LuaFunction setActive;
            private LuaFunction fixedUpdate;
            private LuaFunction update;
            private LuaFunction lateUpdate;

            public LuaAdapter(IWorld world, LuaTable table)
            {
                if (table == null)
                {
                    return;
                }
                this.table = table;
                this.world = world;
                start = table.Get<LuaFunction>("start");
                dispose = table.Get<LuaFunction>("dispose");
                setActive = table.Get<LuaFunction>("active");
                fixedUpdate = table.Get<LuaFunction>("fixed");
                update = table.Get<LuaFunction>("update");
                lateUpdate = table.Get<LuaFunction>("late");
            }

            public void Start()
            {
                if (start == null)
                {
                    return;
                }
                start.Call(table, world);
            }

            public void Dispose()
            {
                if (dispose == null)
                {
                    return;
                }
                dispose.Call(table, world);
                start?.Dispose();
                setActive?.Dispose();
                fixedUpdate?.Dispose();
                lateUpdate?.Dispose();
                update?.Dispose();
                table?.Dispose();
            }

            public void ChangeState(bool state)
            {
                if (setActive == null)
                {
                    return;
                }
                setActive.Call(table, world, state);
            }

            public void FixedUpdate()
            {
                if (fixedUpdate == null)
                {
                    return;
                }
                fixedUpdate.Call(table, world);
            }

            public void Update()
            {
                if (update == null)
                {
                    return;
                }
                update.Call(table, world);
            }

            public void LateUpdate()
            {
                if (lateUpdate == null)
                {
                    return;
                }
                lateUpdate.Call(table, world);
            }
        }

        private bool active;
        private string luaPath;
        private LuaAdapter adapter;
        private List<IEntity> entitys;
        private List<Context> contexts;
        private List<IScriptble> runnables;
        private static string LastOpenWorldName = string.Empty;
        private static Dictionary<string, IWorld> worlds = new Dictionary<string, IWorld>();
        private Vector2 _scrren;
        public Vector2 screenSize
        {
            get
            {
                if (_scrren == Vector2.zero)
                {
                    _scrren = new Vector2(Screen.width, Screen.height);
                }
                return _scrren;
            }
        }

        /// <summary>
        /// 天空�?
        /// </summary>
        /// <value></value>
        public ISkybox skybox
        {
            get;
            private set;
        }

        /// <summary>
        /// 名称
        /// </summary>
        /// <value></value>
        public string name
        {
            get;
            private set;
        }

        /// <summary>
        /// UI管理�?
        /// </summary>
        /// <value></value>
        public IUIManager UIManager
        {
            get;
            private set;
        }

        /// <summary>
        /// 音效管理�?
        /// </summary>
        /// <value></value>
        public IAudioManager AudioManager
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前激活的
        /// </summary>
        /// <value></value>
        public static IWorld current
        {
            get;
            private set;
        }

        public Camera WorldCamera
        {
            get;
            private set;
        }

        public MapGrid Map
        {
            get;
            private set;
        }

        public InputManager input
        {
            get;
            private set;
        }

        public BehaviourManager behaviour
        {
            get;
            private set;
        }

        public GameWorld()
        {
            this.entitys = new List<IEntity>();
            this.contexts = new List<Context>();
            this.runnables = new List<IScriptble>();
        }

        private static T Generate<T>(string name, Vector2 screenSize, Camera worldCamera = null, string skyboxName = null) where T : GameWorld, new()
        {
            GameWorld world = new T();
            world.name = name;

            if (worldCamera == null)
            {
                worldCamera = GameObject.Instantiate<Camera>(Resources.Load<Camera>("Camera/Main Camera"));
            }
            world.WorldCamera = worldCamera;
            world.WorldCamera.name = name + "_WorldCamera";
            world.WorldCamera.gameObject.SetParent(StaticMethod.EmptyTransform, new Vector3(0, 2, -5), new Vector3(20, 0, 0));
            world.input = InputManager.Generate(world);
            world.behaviour = new BehaviourManager();
            world.UIManager = new UIManager(world, screenSize);
            world.AudioManager = new AudioManager(world);
            world.Awake();
            return (T)world;
        }

        /// <summary>
        /// 创建世界
        /// </summary>
        /// <param name="name">世界名称</param>
        /// <returns></returns>
        public static IWorld CreateWorld(string name, Vector2 screenSize, Camera worldCamera = null, string skyboxName = null)
        {
            return CreateWorld<GameWorld>(name, screenSize, worldCamera, skyboxName);
        }

        /// <summary>
        /// 创建世界
        /// </summary>
        /// <param name="name">世界名称</param>
        /// <returns></returns>
        public static T CreateWorld<T>(string name, Vector2 screenSize, Camera worldCamera = null, string skyboxName = null) where T : GameWorld, new()
        {


            if (!worlds.TryGetValue(name, out IWorld world))
            {
                world = Generate<T>(name, screenSize, worldCamera, skyboxName);
                worlds.Add(name, world);
            }

            return (T)ReactiveWorld(world);
        }


        public static IWorld ReactiveWorld(string name)
        {
            IWorld world = GetWorld(name);
            return ReactiveWorld(world);
        }

        public static IWorld ReactiveWorld(IWorld world)
        {
            if (world == null)
            {
                Debug.LogError(new NullReferenceException("world"));
                return default;
            }
            if (current != null)
            {
                LastOpenWorldName = current.name;
                current.SetActive(false);
            }
            Debug.Log("Reactive World:" + world.name);
            current = world;
            current.SetActive(true);
            return world;
        }

        /// <summary>
        /// 获取指定的世�?
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IWorld GetWorld(string name)
        {
            if (worlds.TryGetValue(name, out IWorld world))
            {
                return world;
            }
            return default;
        }
        /// <summary>
        /// 移除指定的世�?
        /// </summary>
        /// <param name="name"></param>
        public static void RemoveWorld(string name)
        {
            if (worlds.TryGetValue(name, out IWorld world))
            {
                world.Dispose();
                worlds.Remove(name);
            }
        }



        public virtual void Awake()
        {
            StaticMethod.AddUpdateEvent(this.name, this.Update);
            StaticMethod.AddLateUpdateEvent(this.name, this.LateUpdate);
            StaticMethod.AddFixedUpdateEvent(this.name, this.FixedUpdate);
        }

        public void LoadLuaMap(string luaPath)
        {
            string tableName = Path.GetFileNameWithoutExtension(luaPath);
            object[] datas = LuaManager.Instance.DoString($"require '{luaPath}'");
            LuaTable table = LuaManager.Instance.GetTable(tableName);
            if (table == null)
            {
                Debug.Log("not find the lua file:" + tableName + "  " + luaPath);
                return;
            }
            this.adapter = new LuaAdapter(this, table);
            UIManager.ClearLoading();
            adapter.Start();
        }

        public virtual void FixedUpdate()
        {
            if (!active)
            {
                return;
            }
            if (adapter != null)
            {
                adapter.FixedUpdate();
            }

            if (skybox != null)
            {
                skybox.FixedUpdate();
            }

            AudioManager.FixedUpdate();
            this.behaviour.Update();
        }

        public virtual void Update()
        {
            if (!active)
            {
                return;
            }
            UpdateScript();
            if (adapter != null)
            {
                adapter.Update();
            }
        }

        public virtual void LateUpdate()
        {
            if (!active)
            {
                return;
            }
            if (adapter != null)
            {
                adapter.LateUpdate();
            }
        }

        /// <summary>
        /// 创建寻路�?
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="walls"></param>
        public void CreateMapPath(float gridSize, int gridCount, List<bool> walls)
        {
            Map = new MapGrid();
            Map.Initialize(gridSize, gridCount, walls);
        }

        /// <summary>
        /// 创建一�?游戏实体对象
        /// </summary>
        /// <returns></returns>
        public IEntity CreateEntity()
        {
            return CreateEntity(string.Empty, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// 创建一�?游戏实体对象
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public IEntity CreateEntity(string name)
        {
            return CreateEntity(name, Guid.NewGuid().ToString().ToLower());
        }

        /// <summary>
        /// 创建一�?游戏实体对象
        /// </summary>
        /// <param name="path">实体名称</param>
        /// <param name="guid">实体�?一ID</param>
        /// <param name="contextName">连接的实体资�?</param>
        /// <returns></returns>
        public IEntity CreateEntity(string path, string guid)
        {
            guid = guid.Replace("-", "");
            if (HasEntity(guid))
            {
                throw new Exception("the entity guid is already exist");
            }
            GameEntity entity = new GameEntity(this, path, guid);
            entitys.Add(entity);
            return entity;
        }

        /// <summary>
        /// 加载逻辑模块
        /// </summary>
        /// <param name="scriptble"></param>
        public void AddScriptble(IScriptble scriptble)
        {
            runnables.Add(scriptble);
        }

        /// <summary>
        /// 加载逻辑模块
        /// </summary>
        /// <param name="table"></param>
        public void AddLuaScriptble(LuaTable table)
        {
            AddScriptble(new LuaScriptbleAdapter(table));
        }

        /// <summary>
        /// 释放当前世界
        /// </summary>
        public virtual void Dispose()
        {
            StaticMethod.RemoveEvent(this.name);
            adapter?.Dispose();
            input?.Dispose();
            UIManager?.Dispose();
            AudioManager?.Dispose();
            if (skybox != null)
                skybox.Dispose();
            if (current == this)
            {
                current = null;
            }
            for (var i = 0; i < entitys.Count; i++)
            {
                entitys[i].Dispose();
            }
            entitys.Clear();
            foreach (var item in runnables)
            {
                item.Dispose();
            }
            runnables.Clear();
            GameObject.DestroyImmediate(WorldCamera.gameObject);
        }

        /// <summary>
        /// 获取所有实体�?�象
        /// </summary>
        /// <returns></returns>
        public List<IEntity> GetEntitys()
        {
            return entitys;//.Cast<IEntity>();
        }

        /// <summary>
        /// 获取所有实体�?�象
        /// </summary>
        /// <returns></returns>
        public Context[] GetEntitys(int tags)
        {
            return contexts.Where(x => x.Contains(tags) && x.Count > 0).ToArray();
        }

        internal void INTERNAL_EntityChangeComponent(IEntity entity, int oldTag)
        {
            contexts.Find(x => x.Contains(oldTag))?.Remove(entity);
            var context = contexts.Find(x => x.tag == entity.tag);
            if (context == null)
            {
                context = new Context(entity.tag);
                contexts.Add(context);
            }
            context.Add(entity);
        }

        /// <summary>
        /// 获取指定的实体�?�象
        /// </summary>
        /// <param name="guid">实体�?一ID</param>
        /// <returns></returns>
        public IEntity GetEntity(string guid)
        {
            guid = guid.Replace("-", "");
            return entitys.Find(x => x.guid == guid);
        }

        /// <summary>
        /// 通过实体名称获取实体对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEntity GetEntityWithName(string name)
        {
            return entitys.Find(x => x.name == name);
        }

        /// <summary>
        /// �?否存在实体�?�象
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool HasEntity(string guid)
        {
            guid = guid.Replace("-", "");
            return GetEntity(guid) != null;
        }

        /// <summary>
        /// 移除实体对象
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveEntity(string guid)
        {
            guid = guid.Replace("-", "");
            IEntity entity = GetEntity(guid);
            if (entity == null)
            {
                return;
            }
            contexts.ForEach(x => x.Remove(entity));
            entitys.Remove(entity);
            entity.Dispose();
        }

        /// <summary>
        /// 设置�?见�?
        /// </summary>
        /// <param name="active"></param>
        public virtual void SetActive(bool active)
        {
            this.active = active;
            UIManager.SetActive(active);
            AudioManager.SetActive(active);
            input.gameObject.SetActive(active);
            if (skybox != null)
            {
                skybox.SetActive(active);
            }

            if (adapter != null)
            {
                adapter.ChangeState(active);
            }

            WorldCamera.gameObject.SetActive(active);
        }

        private void UpdateScript()
        {
            for (var i = 0; i < runnables.Count; i++)
            {
                runnables[i].Executed(this);
            }
        }

        public void ClearEntitys()
        {
            for (var i = 0; i < entitys.Count; i++)
            {
                entitys[i].Dispose();
            }
            entitys.Clear();
            contexts.ForEach(x => x.Clear());
        }
        public IUIHandler OpenUI(string path, string name, LuaTable table)
        {
            if (UIManager == null)
            {
                return null;
            }
            return UIManager.OpenUI(path, name, table);
        }
        public IUIHandler OpenUI(string name, LuaTable table)
        {
            if (UIManager == null)
            {
                return null;
            }
            return UIManager.OpenUI(name, table);
        }

        public IUIHandler OpenUI(string name)
        {
            if (UIManager == null)
            {
                return null;
            }
            return UIManager.OpenUI(name);
        }

        public IUIHandler GetUIHandler(string name)
        {
            if (UIManager == null)
            {
                return null;
            }
            return UIManager.GetUIHandler(name);
        }

        public void CloseUI(string name, bool isCache = false)
        {
            if (UIManager == null)
            {
                return;
            }
            UIManager.CloseUI(name, isCache);
        }

        public void ClearUI()
        {
            if (UIManager == null)
            {
                return;
            }
            UIManager.Clear();
        }

  

        public IMessageBox OnMsgBox(string message, GameFrameworkAction ok = null, GameFrameworkAction cancel = null)
        {
            if (UIManager == null)
            {
                return null;
            }
            return UIManager.OnMsgBox(message, ok, cancel);
        }

        public ILoading ShowLoading(string info)
        {
            if (UIManager == null)
            {
                return null;
            }
            var loading = UIManager.OnLoading();
            loading.text = info;
            return loading;
        }

        public IAwaiting OnWait()
        {
            return UIManager.OnAwaitLoading();
        }

        public void CloseWait()
        {
            UIManager.CloseWait();
        }

        public void CloseMsgBox()
        {
            UIManager.ClearMessageBox();
        }

        public void CloseLoading()
        {
            UIManager.ClearLoading();
        }

        public void LoadSkybox(string path, string skyboxName)
        {
            skybox = new CommonWorldSkybox(this);
            skybox.Initialize(0.1f, 0.05f, path, skyboxName);
        }
    }
}