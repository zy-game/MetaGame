using GameFramework.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework.Runtime.Game
{
    public sealed class CommonAwaiting : IAwaiting
    {
        private GameObject _object;
        public CommonAwaiting()
        {
            _object = GameObject.Instantiate(Resources.Load<GameObject>("Loading/Awaiting"));
        }

        public GameObject gameObject
        {
            get
            {
                return _object;
            }
        }

        public void Dispose()
        {
            gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            gameObject.SetActive(true);
        }
    }
}