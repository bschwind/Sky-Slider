using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GraphicsToolkit.Networking;
using Microsoft.Xna.Framework;

namespace SkySlider.Networking
{
    public static class NetworkSender
    {
        private static ASCIIEncoding encoder = new ASCIIEncoding();

        public static void SendNewPlayerToServer(string name, Client client)
        {
            byte[] data = new byte[1 + name.Length];
            data[0] = (byte)ClientToServerProtocol.NewConnection;
            Array.ConstrainedCopy(encoder.GetBytes(name), 0, data, 1, name.Length);

            client.SendImmediate(data);
        }

        public static void SendPlayerPosToServer(Vector3 pos, string name, Client client)
        {
            byte[] data = new byte[2 + name.Length + sizeof(float)*3];
            data[0] = (byte)ClientToServerProtocol.UpdatePosition;
            data[1] = (byte)name.Length;
            Array.ConstrainedCopy(encoder.GetBytes(name), 0, data, 2, name.Length);
            Array.ConstrainedCopy(BitConverter.GetBytes(pos.X), 0, data, 2 + name.Length, sizeof(float));
            Array.ConstrainedCopy(BitConverter.GetBytes(pos.Y), 0, data, 2 + sizeof(float) + name.Length, sizeof(float));
            Array.ConstrainedCopy(BitConverter.GetBytes(pos.Z), 0, data, 2 + (sizeof(float)*2) + name.Length, sizeof(float));

            client.SendImmediate(data);
        }

        public static void Disconnect(string name, Client client)
        {
            byte[] data = new byte[1 + name.Length];
            data[0] = (byte)ClientToServerProtocol.Disconnect;
            Array.ConstrainedCopy(encoder.GetBytes(name), 0, data, 1, name.Length);

            client.SendImmediate(data);
        }

        public static void SendObjectiveHit(string name, Client client)
        {
            byte[] data = new byte[1 + name.Length];
            data[0] = (byte)ClientToServerProtocol.ObjectiveHit;
            Array.ConstrainedCopy(encoder.GetBytes(name), 0, data, 1, name.Length);

            client.SendImmediate(data);
        }
    }
}
