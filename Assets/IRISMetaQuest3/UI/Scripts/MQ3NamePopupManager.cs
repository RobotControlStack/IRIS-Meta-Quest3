using IRIS.Node;
using Oculus.Interaction.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IRIS.MetaQuest3.UI
{
    [DefaultExecutionOrder(1000)]
    public class MQ3NamePopupManager : Singleton<MQ3NamePopupManager>
    {
        [SerializeField] private GameObject nameChangePopup;
        [SerializeField] private TMP_InputField appNameInput;
        [SerializeField] private Transform headTransform;
        [SerializeField] private GameObject _spawnPoint;
        [SerializeField] private IRISSceneMenuManager isdkSceneMenuManager;
        [SerializeField, Min(0f)] private float forwardOffset = 0.4f;

        void Start()
        {
            if (!PlayerPrefs.HasKey("HostName"))
            {   
                // // Wait unitl tracking is acquired to show the name change popup (necessary for floor level tracking)
                // OVRManager.TrackingAcquired += () => OpenNameChangePopup();
                OpenNameChangePopup();
            }
        }

        void Update()
        {
            SetPose();
        }

        public void OpenNameChangePopup(string currentName = null)
        {
            isdkSceneMenuManager.blockMenuToggle = true;
            currentName ??= IRISXRNode.Instance.localInfo.nodeInfo.Name;
            
            if (nameChangePopup != null)
            {
                nameChangePopup.SetActive(true);
                SetPose();

                if (appNameInput != null)
                {
                    appNameInput.text = currentName;
                }
            }
            // OVRManager.TrackingAcquired -= () => OpenNameChangePopup();
        }

        private void SetPose()
        {
            if (nameChangePopup != null && nameChangePopup.activeInHierarchy && headTransform != null && _spawnPoint != null)
            {
                nameChangePopup.transform.position =
                    _spawnPoint.transform.position + headTransform.forward * forwardOffset;

                // look at user head
                Vector3 lookPos = headTransform.position - nameChangePopup.transform.position;
                // lookPos.y = 0; // keep the menu upright
                Quaternion rotation = Quaternion.LookRotation(-lookPos);
                nameChangePopup.transform.rotation = rotation;
                nameChangePopup.SetActive(true);
            }
        }

        public void SaveNameAndRestartChangePopup()
        {
            if (nameChangePopup != null)
            {
                IRISXRNode.Instance.Rename(appNameInput.text);
                
                // restart the app to apply the name change
                // Get the name of the current scene
                string currentSceneName = SceneManager.GetActiveScene().name;
                // Reload it
                SceneManager.LoadScene(currentSceneName);
            }
        }

        public void CloseNameChangePopup()
        {
            isdkSceneMenuManager.blockMenuToggle = false;
            if (nameChangePopup != null)
            {
                nameChangePopup.SetActive(false);
            }
        }
    }
}
