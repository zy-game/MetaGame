using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GameFramework.Runtime
{
    [RequireComponent(typeof(UniversalAdditionalCameraData))]
    public class CameraRendererLayer : MonoBehaviour
    {
        public int layer = 1;
        [HideInInspector]
        public bool isCopy = false;
        public static List<CameraRendererLayer> mUniversals = new List<CameraRendererLayer>();
        private Camera cam;
        private static UniversalAdditionalCameraData baseUniversal;
        public static Camera BaseCamera { get; private set; }

        private void Awake()
        {
            if (isCopy) return;
            mUniversals.Add(this);
            cam = GetComponent<Camera>();
            if (layer == 0)
            {
                baseUniversal = GetComponent<UniversalAdditionalCameraData>();
                BaseCamera = cam;
            }
        }

        private void Start()
        {
            if (layer == 0) return;
            SortCameraStack();
        }

        private void SortCameraStack()
        {
            baseUniversal.cameraStack.Clear();
            if (mUniversals.Count <= 1) return;
            mUniversals.Sort((a, b) =>
            {
                if (a.layer > b.layer) return 1;
                return -1;
            });


            foreach (var v in mUniversals)
            {
                if (v.layer != 0) baseUniversal.cameraStack.Add(v.cam);
            }
        }

        private void OnDestroy()
        {
            mUniversals.Remove(this);
            SortCameraStack();
        }

        public static Camera CopyBaseCamera()
        {
            if (!BaseCamera) return null;
            Camera newCam = new GameObject("NewBaseCamera").AddComponent<Camera>();
            newCam.CopyFrom(BaseCamera);
            var cameraData= newCam.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.AddRange(baseUniversal.cameraStack);
            cameraData.cameraStack.Clear();

            mUniversals.Sort((a, b) =>
            {
                if (a.layer > b.layer) return 1;
                return -1;
            });
            foreach (var v in mUniversals)
            {
                if (v.layer != 0) cameraData.cameraStack.Add(v.cam);
            }
            return newCam;
        }

        public static void SetBaseCameraActive(bool active)
        {
            if (!BaseCamera) return;
            BaseCamera.gameObject.SetActive(active);
        }

    }
}