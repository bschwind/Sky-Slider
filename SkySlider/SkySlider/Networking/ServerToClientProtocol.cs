using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkySlider.Networking
{
    public enum ServerToClientProtocol
    {
        NewClientConnected,
        ListOfClients,
        ClientPositionUpdated,
        ClientDisconnected
    }
}
