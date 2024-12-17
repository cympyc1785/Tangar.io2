using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;


namespace Tangar.io
{
    enum PlayerButtons
    {
        Fire = 0,
        UseItem = 1,
    }

    public struct PlayerInput : INetworkInput
    {
        public float HorizontalInput;
        public float VerticalInput;
        public NetworkButtons Buttons;
    }
}