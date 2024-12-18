using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangar.io
{
    
    public class PlayerCamera : NetworkBehaviour
    {
        public GameObject _personalCamera;
        private GameObject _cameraInstance;
        public string modelChildName = "Model";
        // Start is called before the first frame update
        private void Start()
        {
            if (!IsLocalPlayer())
            {
                return;
            }

            Transform modelTransform = transform.Find(modelChildName);
            _cameraInstance = Instantiate(_personalCamera);
            _cameraInstance.transform.SetParent(modelTransform);
            Vector3 newCameraPosition = modelTransform.position;
            newCameraPosition.y = 20.0f;
            _cameraInstance.transform.position = newCameraPosition;
        }

        private bool IsLocalPlayer()
        {
            return GetComponent<NetworkObject>().HasInputAuthority;
        }
    }
}

