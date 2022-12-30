using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameFramework
{
    public sealed class InputEvent
    {
        public Vector3 position;
        public float scrollWheel;
        private bool isUsed;
        internal InputEvent(Vector3 position)
        {
            this.position = position;
            isUsed = false;
        }

        internal InputEvent(float position)
        {
            this.scrollWheel = position;
            isUsed = false;
        }
        public void Use()
        {
            isUsed = true;
        }

        public bool HasUseged()
        {
            return isUsed;
        }
    }
    public sealed class InputManager : MonoBehaviour, GObject
    {

        class Shortcut
        {
            public string name { get; private set; }
            public KeyCode key1 { get; private set; }
            public KeyCode key2 { get; private set; }
            public KeyCode key3 { get; private set; }

            public bool isClick = false;
            public bool isUp = false;
            public GameFrameworkAction<bool> callback { get; private set; }

            public KeyCode endKey;
            public void CanCall()
            {
                if (!Input.GetKey(key2))
                {
                    isClick = false;
                    return;
                }
                if (key3 == KeyCode.None)
                {
                    isClick = true;
                    callback(true);
                    return;
                }
                if (!Input.GetKey(key3))
                {
                    isClick = false;
                    return;
                }
                isClick = true;
                callback(true);
            }

            public static Shortcut Generic(string name, bool isUp, GameFrameworkAction<bool> callback, KeyCode key, KeyCode key2 = KeyCode.None, KeyCode key3 = KeyCode.None)
            {
                Shortcut shortKey = new Shortcut();
                shortKey.name = name;
                shortKey.key1 = key;
                shortKey.key2 = key2;
                shortKey.key3 = key3;
                shortKey.endKey = key3 == KeyCode.None ? key2 == KeyCode.None ? KeyCode.None : key2 : key3;
                shortKey.callback = callback;
                shortKey.isUp = isUp;
                return shortKey;
            }
        }
        private List<Shortcut> shorts;

        private void Awake()
        {
            shorts = new List<Shortcut>();
        }

        public void Dispose()
        {
        }

        public static InputManager Generate(IWorld world)
        {
            InputManager manager = world.WorldCamera.gameObject.AddComponent<InputManager>();
            return manager;
        }

        private void Update()
        {
            //先判断首键，如果ctrl或者是alt键找到所有首键是对应键的快捷键
            if (shorts == null || shorts.Count <= 0)
            {
                return;
            }
            for (int i = shorts.Count - 1; i >= 0; i--)
            {
                if (shorts[i].key1 == KeyCode.LeftControl || shorts[i].key1 == KeyCode.LeftAlt)
                {
                    if (Input.GetKey(shorts[i].key1))
                    {
                        if (Input.GetKeyUp(shorts[i].endKey))
                        {
                            if (shorts[i].isUp)
                            {
                                shorts[i].callback(true);
                            }
                            else
                            {
                                shorts[i].callback(false);
                            }
                        }
                        else if (Input.GetKey(shorts[i].endKey))
                        {
                            if (!shorts[i].isUp)
                            {
                                shorts[i].callback(true);
                            }
                        }
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftAlt))
                    {
                        continue;
                    }

                    if (shorts[i].key1 == KeyCode.Mouse0 && EventSystem.current.IsPointerOverGameObject())
                    {
                        continue;
                    }
                    if (Input.GetKey(shorts[i].key1))
                    {
                        if (shorts[i].isUp)
                        {
                            continue;
                        }
                        Debug.Log("key down:" + shorts[i].key1);
                        shorts[i].callback(true);
                    }
                    if (Input.GetKeyUp(shorts[i].key1))
                    {
                        shorts[i].callback(false);
                    }
                }
            }
        }

        public void RegisterKeyDown(string name, KeyCode key, GameFrameworkAction<bool> callback)
        {
            shorts.Add(Shortcut.Generic(name, false, callback, key));
        }

        public void RegisterKeyDown(string name, KeyCode key1, KeyCode key2, GameFrameworkAction<bool> callback)
        {
            shorts.Add(Shortcut.Generic(name, false, callback, key1, key2));
        }

        public void RegisterKeyDown(string name, KeyCode key1, KeyCode key2, KeyCode key3, GameFrameworkAction<bool> callback)
        {
            shorts.Add(Shortcut.Generic(name, false, callback, key1, key2, key3));
        }
        public void RegisterKeyUp(string name, KeyCode key, GameFrameworkAction<bool> callback)
        {
            shorts.Add(Shortcut.Generic(name, true, callback, key));
        }
        public void RegisterKeyUp(string name, KeyCode key1, KeyCode key2, GameFrameworkAction<bool> callback)
        {
            shorts.Add(Shortcut.Generic(name, true, callback, key1, key2));
        }

        public void RegisterKeyUp(string name, KeyCode key1, KeyCode key2, KeyCode key3, GameFrameworkAction<bool> callback)
        {
            shorts.Add(Shortcut.Generic(name, true, callback, key1, key2, key3));
        }
        public void RegisterDoubleClick(KeyCode keyCode, GameFrameworkAction callback)
        {

        }

        public void RegisterLongClick(KeyCode keyCode, float time, GameFrameworkAction callback)
        {

        }

        public void Remove(string name)
        {
            for (int i = shorts.Count - 1; i >= 0; i--)
            {
                if (shorts[i].name != name)
                {
                    continue;
                }
                Debug.Log("remove key:" + shorts[i].key1 + " ," + shorts[i].key2 + " ," + shorts[i].key3);
                shorts.Remove(shorts[i]);
            }
        }
    }
}