using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEvent : GlobalEvent<object>
{
    public static void Notify(string key)
    {
        Notify(key, null);
    }
}

public class GlobalEvent<T>
{
    private static Dictionary<string, List<Action<T>>> map = new Dictionary<string, List<Action<T>>>();

    public static void AddEvent(string key, Action<T> func)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("key 不能为空");
            return;
        }

        List<Action<T>> list = null;
        if (!map.ContainsKey(key))
        {
            list = new List<Action<T>>();
            map.Add(key, list);
        }
        else
        {
            list = map[key];
        }
        if (list.Contains(func))
        {
            Debug.LogError("已添加对应的事件方法");
            return;
        }
        list.Insert(0, func);
    }

    public static void Notify(string key, T t)
    {
        Debug.Log("notify global event:" + key);
        List<Action<T>> list = null;
        if (!map.TryGetValue(key, out list))
            return;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            SafeRun(list[i], t);
            //list[i](t);
        }
    }

    private static void SafeRun(Action<T> ac, T val)
    {
        try
        {
            ac(val);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public static bool Remove(string key)
    {
        return map.Remove(key);
    }

    public static bool Remove(string key, Action<T> func)
    {
        List<Action<T>> list = null;
        if (!map.TryGetValue(key, out list))
            return false;
        bool wall = list.Remove(func);
        return wall;
    }

    public static void ClearAll()
    {
        map.Clear();
    }
}
