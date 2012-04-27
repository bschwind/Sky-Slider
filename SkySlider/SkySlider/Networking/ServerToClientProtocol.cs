using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkySlider.Networking
{
    public enum ServerToClientProtocol
    {
        NewClientConnected = 0,
        ListOfClients = 1,
        ClientPositionUpdated = 2,
        ClientDisconnected = 3,
        UpdateObjective = 4,
        UpdateScore = 5
    }
}
