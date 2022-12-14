using System;
using UnityEngine;

namespace GameFramework.Runtime.Game
{
    /// <summary>
    /// 公用世界天空盒
    /// </summary>
    public sealed class CommonWorldSkybox : ISkybox
    {
        private GameWorld gameWorld;
        private UnityEngine.Skybox _skybox;
        private float RotationInternalTime;
        private float RatationInternalAngle;
        private float lastRotaionTime;
        private float current_Rotation = 0;

        public CommonWorldSkybox(GameWorld gameWorld)
        {
            RotationInternalTime = 0.1f;
            this.gameWorld = gameWorld;
            _skybox = CameraRendererLayer.BaseCamera.gameObject.AddComponent<UnityEngine.Skybox>();
        }

        /// <summary>
        /// release the skybox
        /// </summary>
        public void Dispose()
        {
            GameObject.DestroyImmediate(_skybox);
            _skybox = null;
        }

        /// <summary>
        /// update the skybox
        /// </summary>
        public void FixedUpdate()
        {
            if (_skybox == null || _skybox.material == null)
            {
                return;
            }
            if (Time.realtimeSinceStartup - lastRotaionTime > RotationInternalTime)
            {
                lastRotaionTime = Time.realtimeSinceStartup;
                _skybox.material.SetFloat("_Rotation", current_Rotation);
                current_Rotation += RatationInternalAngle;
            }
        }

        /// <summary>
        /// init the skybox
        /// </summary>
        /// <param name="skyName"></param>
        public void Initialize(float rotationTime, float rotationAngle, string path, string skyName)
        {
            RotationInternalTime = rotationTime;
            RatationInternalAngle = rotationAngle;
            Debug.Log("set skybox:" + skyName);
            Material material = Assets.ResourcesManager.Instance.LoadAsset<Material>(path, this._skybox.gameObject, skyName);
            if (material == null)
            {
                return;
            }
            _skybox.material = material;
        }

        /// <summary>
        /// set the skybox active
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            _skybox.enabled = active;
        }
    }
}