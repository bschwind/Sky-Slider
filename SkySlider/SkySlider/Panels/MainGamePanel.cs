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
using SkySlider.Maps;
using SkySlider.Players;
using SkySlider.Networking;
using Lidgren.Network;
using System.Threading;

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
        private SpriteBatch sb;
        private SpriteFont sf;
        private Player player;

        private Vector3 objectiveLocation; //block players must reach

        //Networking Code
        private Dictionary<string, RemotePlayer> remotePlayers;
        private string localPlayerName;
        private NetClient client;
        private bool singleplayer;
        private ASCIIEncoding encoder = new ASCIIEncoding();
        private int frameSkip = 5;
        private int currentFrame = 0;
        private float objectiveCoolDown = 2f;
        private float currentCoolDown;
        private bool coolingDown;

        private bool gameOver = false;
        
        public MainGamePanel()
            : base(Vector2.Zero, Vector2.One)
        {
            singleplayer = false;

            remotePlayers = new Dictionary<string, RemotePlayer>();

            Random rand = new Random();
            localPlayerName = "" + rand.Next(0, 255);

            NetPeerConfiguration config = new NetPeerConfiguration("SkySlider");
            config.EnableMessageType(NetIncomingMessageType.Data);
            client = new NetClient(config);
            client.Start();
            client.Connect("localhost", 16645);

            NetOutgoingMessage newClientMsg = client.CreateMessage();
            newClientMsg.Write((byte)ClientToServerProtocol.NewConnection);
            newClientMsg.Write(localPlayerName);
            Thread.Sleep(500);

            client.SendMessage(newClientMsg, NetDeliveryMethod.ReliableOrdered);

            Thread thread = new Thread(new ThreadStart(listenForPackets));
            thread.Start();
        }

        void listenForPackets()
        {
            while (true)
            {
                NetIncomingMessage msg;
                while ((msg = client.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.Data:
                            ServerToClientProtocol protocol = (ServerToClientProtocol)msg.ReadByte();
                            switch (protocol)
                            {
                                case ServerToClientProtocol.NewClientConnected:
                                    string name = msg.ReadString();
                                    Console.WriteLine("New client connected: " + name);
                                    remotePlayers.Add(name, new RemotePlayer());
                                    break;
                                case ServerToClientProtocol.ListOfClients:
                                    int numNames = msg.ReadInt32();
                                    for (int i = 0; i < numNames; i++)
                                    {
                                        string playerName = msg.ReadString();
                                        if (playerName != localPlayerName)
                                        {
                                            remotePlayers.Add(playerName, new RemotePlayer());
                                        }
                                    }
                                    break;
                                case ServerToClientProtocol.ClientPositionUpdated:
                                    name = msg.ReadString();
                                    float x = msg.ReadSingle();
                                    float y = msg.ReadSingle();
                                    float z = msg.ReadSingle();

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
                                    name = msg.ReadString();
                                    remotePlayers.Remove(name);
                                    Console.WriteLine(name + " has disconnected");
                                    break;
                                case ServerToClientProtocol.UpdateObjective:
                                    int objX = msg.ReadInt32();
                                    int objY = msg.ReadInt32();
                                    int objZ = msg.ReadInt32();
                                    //Update objective location
                                    objectiveLocation = new Vector3(objX, objY, objZ);
                                    break;
                            }
                            break;
                        case NetIncomingMessageType.ErrorMessage:
                            Console.WriteLine(msg.ReadString());
                            break;
                        default:
                            Console.WriteLine("Unhandled type: " + msg.MessageType);
                            break;
                    }
                }
            }
        }

        void client_OnDataReceived(byte[] data, int bytesRead)
        {
            /*byte protocolByte = data[0];
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
                case ServerToClientProtocol.UpdateObjective:
                    int objX = BitConverter.ToInt32(data, 1);
                    int objY = BitConverter.ToInt32(data, 1 + sizeof(int));
                    int objZ = BitConverter.ToInt32(data, 1 + (2 * sizeof(int)));
                    //Update objective location
                    objectiveLocation = new Vector3(objX, objY, objZ);
                    break;
            }*/
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
            mb.AddQuad(new Vector3(1, map.Height - 1, 1), new Vector3(map.Width - 1, map.Height - 1, 1), new Vector3(map.Width - 1, 1, 1), new Vector3(1, 1, 1), false, Vector2.Zero, new Vector2(map.Width, map.Height));
            //add front wall
            mb.AddQuad(new Vector3(map.Width - 1, map.Height - 1, map.Depth - 1), new Vector3(1, map.Height - 1, map.Depth - 1), new Vector3(1, 1, map.Depth - 1), new Vector3(map.Width - 1, 1, map.Depth - 1), false, Vector2.Zero, new Vector2(map.Width, map.Height));
            //add left wall
            mb.AddQuad(new Vector3(1, map.Height - 1, map.Depth - 1), new Vector3(1, map.Height - 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, map.Depth - 1), false, Vector2.Zero, new Vector2(map.Depth, map.Height));
            //add right wall
            mb.AddQuad(new Vector3(map.Width - 1, map.Height - 1, 1), new Vector3(map.Width - 1, map.Height - 1, map.Depth - 1), new Vector3(map.Width - 1, 1, map.Depth - 1), new Vector3(map.Width - 1, 1, 1), false, Vector2.Zero, new Vector2(map.Depth, map.Height));
            //add floor
            mb.AddQuad(new Vector3(1, 1, 1), new Vector3(map.Width - 1, 1, 1), new Vector3(map.Width - 1, 1, map.Depth - 1), new Vector3(1, 1, map.Depth - 1), false, Vector2.Zero, new Vector2(map.Width, map.Depth));
            walls = mb.End();
            walls.Texture = content.Load<Texture2D>("Textures/BlockTextures/Walls");

            if (singleplayer)
            {
                objectiveLocation = map.getNextObjective(new Vector3(-1, -1, -1)); //get first objective
            }

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


            sb = new SpriteBatch(Device);
            sf = content.Load<SpriteFont>("Fonts/Helvetica");
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
                    //NetworkSender.Disconnect(localPlayerName, client);
                    NetOutgoingMessage disconnectMessage = client.CreateMessage();
                    disconnectMessage.Write((byte)ClientToServerProtocol.Disconnect);
                    disconnectMessage.Write(localPlayerName);
                    client.SendMessage(disconnectMessage, NetDeliveryMethod.ReliableOrdered);
                    client.Disconnect("see ya");
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
                //NetworkSender.SendPlayerPosToServer(player.Body.Pos, localPlayerName, client);
                NetOutgoingMessage newPosMsg = client.CreateMessage();
                newPosMsg.Write((byte)ClientToServerProtocol.UpdatePosition);
                newPosMsg.Write(localPlayerName);
                newPosMsg.Write(player.Body.Pos.X);
                newPosMsg.Write(player.Body.Pos.Y);
                newPosMsg.Write(player.Body.Pos.Z);

                client.SendMessage(newPosMsg, NetDeliveryMethod.ReliableOrdered);
            }

            if (singleplayer && objectiveLocation == new Vector3(-1, -1, -1))
            {
                gameOver = true;
            }
        }
        /// <summary>
        /// If the current objective has been reached, change the objective
        /// </summary>
        /// <param name="g"></param>
        private void updateObjective(GameTime g)
        {
            if (coolingDown)
            {
                currentCoolDown += (float)g.ElapsedGameTime.TotalSeconds;
                if (currentCoolDown > objectiveCoolDown)
                {
                    coolingDown = false;
                }
            }

            //If we're touching the objective
            if (((int)player.Body.Pos.X == objectiveLocation.X) &&
                ((int)player.Body.Pos.Y == objectiveLocation.Y) &&
                ((int)player.Body.Pos.Z == objectiveLocation.Z))
            {
                if (!coolingDown)
                {
                    player.givePoint();
                    /*objectiveLocation = map.getNextObjective(new Vector3((int)player.Body.Pos.X,
                        (int)player.Body.Pos.Y,
                        (int)player.Body.Pos.Z));*/
                    //NetworkSender.SendObjectiveHit(localPlayerName, client);
                    NetOutgoingMessage objectiveHitMsg = client.CreateMessage();
                    objectiveHitMsg.Write((byte)ClientToServerProtocol.ObjectiveHit);
                    objectiveHitMsg.Write(localPlayerName);

                    client.SendMessage(objectiveHitMsg, NetDeliveryMethod.ReliableOrdered);

                    coolingDown = true;
                }
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
                    primBatch.DrawMesh(sphere, Matrix.CreateScale(sb.Radius) * Matrix.CreateTranslation(engine.GetBodies()[i].Pos), player.Cam, new Vector3((int.Parse(localPlayerName) % 5) / 5.0f, (int.Parse(localPlayerName) % 3) / 3.0f, (int.Parse(localPlayerName) % 2) / 2.0f));
                }
            }

            //Draw remote players
            foreach (RemotePlayer p in remotePlayers.Values)
            {
                primBatch.DrawMesh(sphere, Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(p.Position), player.Cam, new Vector3((int.Parse(p.Name) % 4) / 4.0f, (int.Parse(p.Name) % 3) / 3.0f, (int.Parse(p.Name) % 2) / 2.0f));
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
            primBatch.FillTriangle(tipPos + new Vector3(0, 0.2f, 0), B + new Vector3(0, 0.2f, 0), A + new Vector3(0, 0.2f, 0), new Color(255, 105, 0));
            primBatch.FillTriangle(tipPos + new Vector3(0, 0.2f, 0), A + new Vector3(0, 0.2f, 0), B + new Vector3(0, 0.2f, 0), new Color(255, 105, 0));
            primBatch.End();

            map.DebugDraw(g, primBatch, player.Cam);

            //if (gameOver)
            //{
            //    sb.Begin();
            //    sb.DrawString(sf, "Game Over!", new Vector2(this.width / 2, this.height / 2), Color.DarkCyan);
            //    sb.End();
            //}
        }
    }
}
