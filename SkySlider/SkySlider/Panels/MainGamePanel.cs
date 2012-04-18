using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GraphicsToolkit.GUI;
using GraphicsToolkit.Graphics;
using GraphicsToolkit.Input;
using GraphicsToolkit.Physics._3D;
using GraphicsToolkit.Physics._3D.Partitions;
using GraphicsToolkit.Physics._3D.Bodies;
using GraphicsToolkit.Networking;
using SkySlider.Maps;
using SkySlider.Players;
using SkySlider.Networking;

namespace SkySlider.Panels
{
    public class MainGamePanel : Panel
    {
        private PhysicsEngine3D engine;
        private MapPartition partition;
        private Map map;
        private FirstPersonCamera cam;
        private Mesh sphere;
        private Mesh box;
        private PrimitiveBatch primBatch;
        private Player player;

        private Vector3 objectiveLocation; //block players must reach

        //Networking Code
        private Dictionary<string, RemotePlayer> remotePlayers;
        private string localPlayerName;
        private Client client;
        private bool singleplayer = false;
        private ASCIIEncoding encoder = new ASCIIEncoding();
        private int frameSkip = 5;
        private int currentFrame = 0;
        
        public MainGamePanel()
            : base(Vector2.Zero, Vector2.One)
        {
            remotePlayers = new Dictionary<string, RemotePlayer>();
            client = new Client();
            client.OnDataReceived += new ClientHandlePacketData(client_OnDataReceived);
            try
            {
                client.ConnectToServer("localhost", 16645);
                singleplayer = false;
            }
            catch
            {
                Console.WriteLine("Could not connect to server, initiating single player mode");
                singleplayer = true;
            }

            if (!singleplayer)
            {
                Random rand = new Random();
                localPlayerName = "Brian" + rand.Next(0, 500);
                NetworkSender.SendNewPlayerToServer(localPlayerName, client);
            }
        }

        void client_OnDataReceived(byte[] data, int bytesRead)
        {
            byte protocolByte = data[0];
            ServerToClientProtocol protocol = (ServerToClientProtocol)protocolByte;

            switch (protocol)
            {
                case ServerToClientProtocol.NewClientConnected:
                    string name = encoder.GetString(data, 1, bytesRead - 1);
                    Console.WriteLine("New client connected: " + name);
                    remotePlayers.Add(name, new RemotePlayer());
                    break;
                case ServerToClientProtocol.ClientPositionUpdated:
                    byte nameLength = data[1];
                    name = encoder.GetString(data, 1, nameLength);
                    float x = BitConverter.ToSingle(data, 2 + nameLength);
                    float y = BitConverter.ToSingle(data, 2 + sizeof(float) + nameLength);
                    float z = BitConverter.ToSingle(data, 2 + (2*sizeof(float)) + nameLength);

                    Console.WriteLine("Got position data. " + name + " is at " + x + " " + y + " " + z);
                    remotePlayers[name].Position = new Vector3(x, y, z);
                    break;
                case ServerToClientProtocol.ClientDisconnected:

                    break;
            }
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            base.LoadContent(content);

            BlockData.Initialize(Device, content);

            //cam = new FirstPersonCamera(0.5f, 10);
            //cam.Pos = new Vector3(3, 3, 13);

            map = new Map("Content/Levels/Level1-1.txt"); //load map
            objectiveLocation = map.getNextObjective(new Vector3(-1, -1, -1)); //get first objective

            partition = new MapPartition(map);

            player = new Player(new Vector3(5,5,5)); //spawn player

            engine = new PhysicsEngine3D(partition);
            engine.AddRigidBody(player.Body); //physics body of player

            MeshBuilder mb = new MeshBuilder(Device);
            sphere = mb.CreateSphere(1f, 10, 10);
            box = mb.CreateBox(0.25f, 0.25f, 0.25f); //box to draw at objective

            primBatch = new PrimitiveBatch(Device);
        }

        public override void Update(GameTime g)
        {
            base.Update(g);

            if (InputHandler.IsKeyPressed(Keys.Escape))
            {
                ExitDesired = true;
                if (!singleplayer)
                {
                    NetworkSender.Disconnect(localPlayerName, client);
                    client.Disconnect();
                }
            }

            player.Update(g);
            engine.Update(g);
            updateObjective(g);

            currentFrame++;
            if (currentFrame > frameSkip)
            {
                currentFrame = 0;
                //Send updated position
                NetworkSender.SendPlayerPosToServer(player.Body.Pos, localPlayerName, client);
            }
        }
        /// <summary>
        /// If the current objective has been reached, change the objective
        /// </summary>
        /// <param name="g"></param>
        private void updateObjective(GameTime g)
        {
            if (((int)player.Body.Pos.X == objectiveLocation.X) &&
                ((int)player.Body.Pos.Y == objectiveLocation.Y) &&
                ((int)player.Body.Pos.Z == objectiveLocation.Z))
            {
                player.givePoint();
                objectiveLocation = map.getNextObjective(new Vector3((int)player.Body.Pos.X,
                    (int)player.Body.Pos.Y,
                    (int)player.Body.Pos.Z));
            }
        }

        public override void Draw(GameTime g)
        {
            base.Draw(g);
            //draw spheres
            for (int i = 0; i < engine.GetBodies().Count; i++)
            {
                SphereBody sb = engine.GetBodies()[i] as SphereBody;
                if (sb != null)
                {
                    primBatch.DrawMesh(sphere, Matrix.CreateScale(sb.Radius) * Matrix.CreateTranslation(engine.GetBodies()[i].Pos), player.Cam);
                }
            }

            //Draw remote players
            foreach (RemotePlayer p in remotePlayers.Values)
            {
                primBatch.DrawMesh(sphere, Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(p.Position), player.Cam);
            }

            //Draw box at objective
            primBatch.DrawMesh(box, Matrix.CreateScale(1f) * Matrix.CreateTranslation(objectiveLocation + new Vector3(0.5f, 0.5f, 0.5f)), player.Cam);

            //Draw waypoint pointing to next objective
            Vector3 playerToObjective = objectiveLocation + new Vector3(0.5f, 0.5f, 0.5f) - player.Body.Pos;
            playerToObjective.Normalize();
            Vector3 tipPos = player.Body.Pos + 0.5f * playerToObjective;
            Vector3 tBase = Vector3.Cross(Vector3.Up, playerToObjective);
            tBase.Normalize();
            Vector3 A = player.Body.Pos + 0.2f * tBase;
            Vector3 B = player.Body.Pos - 0.2f * tBase;

            primBatch.Begin(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, player.Cam);
     //       primBatch.DrawLine(player.Body.Pos, objectiveLocation + new Vector3(0.5f, 0.5f, 0.5f), Color.Aqua);
            primBatch.FillTriangle(tipPos + new Vector3(0, 0.2f, 0), B + new Vector3(0, 0.2f, 0), A + new Vector3(0, 0.2f, 0), Color.BlanchedAlmond);
            primBatch.FillTriangle(tipPos + new Vector3(0, 0.2f, 0), A + new Vector3(0, 0.2f, 0), B + new Vector3(0, 0.2f, 0), Color.BlanchedAlmond);
            primBatch.End();

            map.DebugDraw(g, primBatch, player.Cam);
        }
    }
}
