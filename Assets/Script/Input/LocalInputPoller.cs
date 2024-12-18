using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tangar.io
{
    public class LocalInputPoller : MonoBehaviour, INetworkRunnerCallbacks
    {
        private const string AXIS_HORIZONTAL = "Horizontal";
        private const string AXIS_VERTICAL = "Vertical";
        private const string BUTTON_FIRE1 = "Fire1";
        private const string BUTTON_USE_ITEM = "Fire2"; // Firing Item temporarily set as Fire2
        private const string BUTTON_JUMP = "Jump"; // Can be used as an alternative fire button to shoot with SPACE
        public FloatingJoystick floatingJoystick = null;
        public Button fireButton;
        public Button useItemButton;
        private bool isFirePressed = false;
        private bool isUseItemPressed = false;

        // The INetworkRunnerCallbacks of this LocalInputPoller are automatically detected
        // because the script is located on the same object as the NetworkRunner and
        // NetworkRunnerCallbacks scripts.

        public void Start()
        {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                if (SceneManager.GetActiveScene().name == "InGame")
                {
                    floatingJoystick = GameObject.FindObjectOfType<FloatingJoystick>();
                    fireButton = GameObject.Find("FireButton").GetComponent<Button>();
                    useItemButton = GameObject.Find("UseItem").GetComponent<Button>();
                    fireButton.onClick.AddListener(() => { isFirePressed = true; });
                    useItemButton.onClick.AddListener(() => { isUseItemPressed = true; });
                }
            };
            
            
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            PlayerInput localInput = new PlayerInput();

            if(floatingJoystick != null && floatingJoystick._pointerDown)
            {
                localInput.HorizontalInput = floatingJoystick.Horizontal;
                localInput.VerticalInput = floatingJoystick.Vertical;
            }
            else
            {
                localInput.HorizontalInput = Input.GetAxis(AXIS_HORIZONTAL);
                localInput.VerticalInput = Input.GetAxis(AXIS_VERTICAL);
            }
            localInput.Buttons.Set(PlayerButtons.Fire, isFirePressed);
            localInput.Buttons.Set(PlayerButtons.UseItem, isUseItemPressed);

            isFirePressed = false;
            isUseItemPressed = false;

            input.Set(localInput);
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }
        
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }
        
        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
    }
}
