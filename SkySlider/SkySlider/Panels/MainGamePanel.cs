using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
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
        private Mesh destination;
        private Mesh walls;
        private PrimitiveBatch primBatch;
        private Player player;

        private Vector3 objectiveLocation; //block players must reach

        //Networking Code
        private Dictionary<string, RemotePlayer> remotePlayers;
        private string localPlayerName;
        private Client client;
        private bool singleplayer;
        private ASCIIEncoding encoder = new ASCIIEncoding();
        private int frameSkip = 0;
        private int currentFrame = 0;
        
        public MainGamePanel()
            : base(Vector2.Zero, Vector2.One)
        {
            singleplayer = false;

            remotePlayers = new Dictionary<string, RemotePlayer>();
            client = new Client();
            client.OnDataReceived += new ClientHandlePacketData(client_OnDataReceived);
            if (!singleplayer)
            {
                try
                {
                    client.ConnectToServer("156.143.93.190", 16645);
                    singleplayer = false;
                }
                catch
                {
                    Console.WriteLine("Could not connect to server, initiating single player mode");
                }
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
                case ServerToClientProtocol.ListOfClients:
                    byte numNames = data[1];
                    int[] nameLengths = new int[numNames];
                    for (int i = 0; i < numNames; i++)
                    {
                        nameLengths[i] = data[2 + i];
                    }
                    int currentIndex = 2 + numNames;
                    for (int i = 0; i < nameLengths.Length; i++)
                    {
                        string playerName = encoder.GetString(data, currentIndex, nameLengths[i]);
                        if (playerName != localPlayerName)
                        {
                            remotePlayers.Add(playerName, new RemotePlayer());
                        }
                        currentIndex += nameLengths[i];
                    }
                    break;
                case ServerToClientProtocol.ClientPositionUpdated:
                    byte nameLength = data[1];
                    name = encoder.GetString(data, 2, nameLength);
                    float x = BitConverter.ToSingle(data, 2 + nameLength);
                    float y = BitConverter.ToSingle(data, 2 + sizeof(float) + nameLength);
                    float z = BitConverter.ToSingle(data, 2 + (2*sizeof(float)) + nameLength);

                    Console.WriteLine("Got position data. " + name + " is at " + x + " " + y + " " + z);
                    try
                    {
                        remotePlayers[name].Position = new Vector3(x, y, z);
                    }
                    catch
                    {
                        //Whatevs
                    }
                    
                    break;
                case ServerToClientProtocol.ClientDisconnected:
                    name = encoder.GetString(data, 1, bytesRead - 1);
                    remotePlayers.Remove(name);
                    Console.WriteLine(name + " has disconnected");
                    break;
            }
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            base.LoadContent(content);

            BlockData.Initialize(Device, content);

            //cam = new FirstPersonCamera(0.5f, 10);
            //cam.Pos = new Vector3(3, 3, 13)

            map = new Map("Content/Levels/Level1-1.txt"); //load map

            MeshBuilder mb = new MeshBuilder(Device);
            mb.Begin();
            //add back wall
            mb.AddQuad(new Vector3(1, map.Height - 1, 1), new Vector3(map.Width - 1, map.Height, 1), new Vector3(map.Width - 1, 0, 1), new Vector3(1, 1, 1), false, Vector2.Zero, new Vector2(map.Width, map.Height));
            //add front wall
            mb.AddQuad(new Vector3(map.Width - 1, map.Height, map.Depth - 1), new Vector3(1, map.Height - 1, map.Depth - 1), new Vector3(1, 1, map.Depth - 1), new Vector3(map.Width - 1, 0, map.Depth - 1), false, Vector2.Zero, new Vector2(map.Width, map.Height));
            //add left wall
            mb.AddQuad(new Vector3(1, map.Height - 1, map.Depth - 1), new Vector3(1, map.Height - 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, map.Depth - 1), false, Vector2.Zero, new Vector2(map.Depth, map.Height));
            //add right wall
            mb.AddQuad(new Vector3(map.Width - 1, map.Height - 1, 1), new Vector3(map.Width - 1, map.Height - 1, map.Depth - 1), new Vector3(map.Width - 1, 1, map.Depth - 1), new Vector3(map.Width - 1, 1, 1), false, Vector2.Zero, new Vector2(map.Depth, map.Height));
            //add floor
            mb.AddQuad(new Vector3(1, 1, 1), new Vector3(map.Width - 1, 1, 1), new Vector3(map.Width - 1, 1, map.Depth - 1), new Vector3(1, 1, map.Depth - 1), false, Vector2.Zero, new Vector2(map.Width, map.Depth));
            walls = mb.End();
            walls.Texture = content.Load<Texture2D>("Textures/BlockTextures/Walls");

            objectiveLocation = map.getNextObjective(new Vector3(-1, -1, -1)); //get first objective

            partition = new MapPartition(map);

            player = new Player(new Vector3(5,5,5)); //spawn player
            engine = new PhysicsEngine3D(partition);
            engine.Gravity = new Vector3(0, -0.1f, 0);
            engine.AddRigidBody(player.Body); //physics body of player
            engine.AddRigidBody(new PlaneBody(Vector3.Up, new Vector3(0, 1f, 0)));
            engine.AddRigidBody(new PlaneBody(new Vector3(1f, 0, 0), new Vector3(1f, 0, 0)));
            engine.AddRigidBody(new PlaneBody(new Vector3(-1f, 0, 0), new Vector3(map.Width - 1, 0, 0)));
            engine.AddRigidBody(new PlaneBody(new Vector3(0, 0, 1f), new Vector3(0, 0, 1f)));
            engine.AddRigidBody(new PlaneBody(new Vector3(0, 0, -1f), new Vector3(0, 0, map.Depth - 1)));

            sphere = mb.CreateSphere(1f, 10, 10);
            destination = mb.CreateSphere(0.5f, 12, 12); //box to draw at objective
            destination.Texture = content.Load<Texture2D>("Textures/BlockTextures/Destination");

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
            if (!singleplayer && currentFrame > frameSkip)
            {
                currentFrame = 0;
                //Send updated position
                NetworkSender.SendPlayerPosToServer(player.Body.Pos, localPlayerName, client);
            }

            if (objectiveLocation == new Vector3(-1, -1, -1))
            {
                //game-over code goes here
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
            
            //Draw sphere at objective
            primBatch.DrawMesh(destination, Matrix.CreateScale(1f) * Matrix.CreateTranslation(objectiveLocation + new Vector3(0.5f, 0.5f, 0.5f)), player.Cam);

            //Draw walls
            primBatch.DrawMesh(walls, Matrix.Identity, player.Cam);

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
            primBatch.FillTriangle(tipPos + new Vector3(0, 0.2f, 0), B + new Vector3(0, 0.2f, 0), A + new Vector3(0, 0.2f, 0), new Color(255, 105, 0));
            primBatch.FillTriangle(tipPos + new Vector3(0, 0.2f, 0), A + new Vector3(0, 0.2f, 0), B + new Vector3(0, 0.2f, 0), new Color(255, 105, 0));
            primBatch.End();

            map.DebugDraw(g, primBatch, player.Cam);
        }
    }
}
