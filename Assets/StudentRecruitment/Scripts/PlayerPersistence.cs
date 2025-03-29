using UnityEngine;
using UnityEngine.SceneManagement;

namespace StudentRecruitment
{
    public class PlayerPersistence : MonoBehaviour
    {
        private static PlayerPersistence instance;
        public static PlayerPersistence Instance => instance;

        private bool isPersistent = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                Debug.Log("[PlayerPersistence] Initialized as singleton");
            }
            else
            {
                Debug.Log("[PlayerPersistence] Destroying duplicate instance");
                Destroy(gameObject);
            }
        }

        public void MakePersistent()
        {
            if (!isPersistent)
            {
                isPersistent = true;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[PlayerPersistence] Player made persistent");
            }
        }

        public void RemovePersistence()
        {
            if (isPersistent)
            {
                isPersistent = false;
                Debug.Log("[PlayerPersistence] Player persistence removed");
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                Debug.Log("[PlayerPersistence] Instance destroyed");
            }
        }
    }
} 