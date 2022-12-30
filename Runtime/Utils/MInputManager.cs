using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime
{

    public class MInputEvent
    {
        public const int Down = 1;
        public const int Up = 2;
        public const int Press = 3;
        public const int Scale = 4;
        public const int Drag = 5;
        public const int Click = 6;
    }

    public class MInputManager : SingletonBehaviour<MInputManager>
    {
        public static float clickLimit = 100;

        private Dictionary<int, List<Action>> inputEventMap = new Dictionary<int, List<Action>>();
        private Dictionary<int, List<Action>> removeMap = new Dictionary<int, List<Action>>();
        private bool isRemove = false;
        private int lastTouchCount = 0;

        public Vector3 Point
        {
            get
            {
                return Input.mousePosition;
            }
        }

        public Vector3 RawPoint
        {
            get
            {
                return Input.mousePosition / ScreenWidth;
            }
        }

        public float ScaleDelta
        {
            get; private set;
        }

        public Vector2 DragDelta
        {
            get; private set;
        }

        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        private void Awake()
        {
            ScreenWidth = Screen.width;
            ScreenHeight = Screen.height;
        }

        public void AddEvent(int eventId, Action action)
        {

            List<Action> list = null;
            if (inputEventMap.ContainsKey(eventId))
                list = inputEventMap[eventId];
            else
            {
                list = new List<Action>();
                inputEventMap.Add(eventId, list);
            }
            if (list.Contains(action)) return;
            list.Add(action);
        }

        public void RemoveEvent(int eventId, Action action)
        {
            List<Action> list = null;
            if (removeMap.ContainsKey(eventId))
                list = removeMap[eventId];
            else
            {
                list = new List<Action>();
                removeMap.Add(eventId, list);
            }
            if (list.Contains(action)) return;
            list.Add(action);
            isRemove = true;
        }

        private void RemoveEvent()
        {
            foreach (var v in removeMap)
            {
                int id = v.Key;
                List<Action> removeList = v.Value;
                List<Action> list = FindListBuyId(id);
                if (list != null && list.Count > 0)
                {
                    foreach (var func in removeList)
                    {
                        list.Remove(func);
                    }
                }
                removeList.Clear();
            }
        }

        private List<Action> FindListBuyId(int id)
        {
            if (!inputEventMap.ContainsKey(id))
                return null;
            return inputEventMap[id];
        }

        private void Update()
        {
            if (isRemove)
            {
                RemoveEvent();
                isRemove = false;
            }

            bool singleInput = true;
#if UNITY_ANDROID || UNITY_IPHONE
            if (Input.touchCount > 1)
                singleInput = false;
            else
            {
                if (Input.touchCount == 1)
                {
                    if (lastTouchCount > 1)
                    {
                        lastDragPoint = Point;
                        lastDownPoint = Point;
                    }
                }
            }
            lastTouchCount = Input.touchCount;
#endif
            if (singleInput)
            {
                InputDown();
                InputPress();
                InputUp();
            }

            InputScale();
        }

        private void InputDown()
        {

            //编辑器或者Window下使用
            if (Input.GetMouseButtonDown(0))
            {
                List<Action> list = FindListBuyId(MInputEvent.Down);
                if (list != null && list.Count > 0)
                {
                    foreach (Action func in list)
                    {
                        func();
                    }
                }

                lastDragPoint = Point;
                lastDownPoint = Point;
            }

        }

        private void InputUp()
        {
#if UNITY_ANDROID || UNITY_IPHONE
            if (Input.touchCount > 1)
                return;
#endif

            //编辑器或者Window下使用
            if (Input.GetMouseButtonUp(0))
            {
                List<Action> list = FindListBuyId(MInputEvent.Up);
                if (list != null && list.Count > 0)
                {
                    foreach (Action func in list)
                    {
                        func();
                    }
                }

                InputClick();

            }
        }

        private void InputPress()
        {
            //编辑器或者Window下使用
            if (Input.GetMouseButton(0))
            {
                List<Action> list = FindListBuyId(MInputEvent.Press);
                if (list != null && list.Count > 0)
                {
                    foreach (Action func in list)
                    {
                        func();
                    }
                }
                InputDrag();
            }
        }

        private float doubleTouchDis = 0;
        public void InputScale()
        {
            List<Action> list = FindListBuyId(MInputEvent.Scale);
            if (list == null || list.Count == 0)
            {
                doubleTouchDis = 0;
                ScaleDelta = 0;
                return;
            }

            //编辑器或者Window下使用
            float dv = 0;

            if (!AppConst.IsMobilePlatform)
            {
                dv = Input.GetAxis("Mouse ScrollWheel");
            }
            else
            {
                if (Input.touchCount == 2)
                {
                    float touchesDis = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                    if (doubleTouchDis == 0)
                        doubleTouchDis = touchesDis;
                    else
                        dv = (touchesDis - doubleTouchDis) / ScreenWidth;
                    doubleTouchDis = touchesDis;
                }
                else
                    doubleTouchDis = 0;
            }

            ScaleDelta = dv;
            if (dv > 0 || dv < 0)
            {
                foreach (Action func in list)
                {
                    func();
                }
            }
        }

        private Vector3 lastDragPoint;
        private void InputDrag()
        {

            List<Action> list = FindListBuyId(MInputEvent.Drag);

            if (list == null || list.Count == 0) return;
            DragDelta = (Point - lastDragPoint) / ScreenWidth;
            lastDragPoint = Point;
            if (Mathf.Abs(DragDelta.x) > 0 || Mathf.Abs(DragDelta.y) > 0)
            {
                foreach (Action func in list)
                {
                    func();
                }
            }
        }

        private Vector3 lastDownPoint;
        private void InputClick()
        {
            float disSqr = DistanceSqr(lastDownPoint, Point);
            if (disSqr > clickLimit) return;
            List<Action> list = FindListBuyId(MInputEvent.Click);
            if (list == null || list.Count == 0) return;
            foreach (Action func in list)
            {
                func();
            }
        }

        private float DistanceSqr(Vector3 v1, Vector3 v2)
        {
            return (v1.x - v2.x) * (v1.x - v2.x) + (v1.y - v2.y) * (v1.y - v2.y);
        }
    }
}