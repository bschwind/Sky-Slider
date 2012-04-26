using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkySlider.Networking
{
    public enum ClientToServerProtocol
    {
        NewConnection = 0,
        UpdatePosition = 1,
        Disconnect = 2,
        ObjectiveHit = 3
    }
}
