using UnityEngine;

namespace StudentRecruitment
{
    public class PlayerPersistence : MonoBehaviour
    {
        private static PlayerPersistence instance;
        private bool isPersistent = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void MakePersistent()
        {
            if (!isPersistent)
            {
                DontDestroyOnLoad(gameObject);
                isPersistent = true;
                Debug.Log("Player is now persistent across scenes");
            }
        }

        public void RemovePersistence()
        {
            if (isPersistent)
            {
                isPersistent = false;
                Debug.Log("Player persistence removed");
            }
        }
    }
} 