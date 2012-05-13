using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
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
        private PrimitiveBatch primBatch;
        private SpriteBatch sBatch;
        private SpriteFont sf;

        private Map map;

        private Mesh sphere;
        private Mesh destination;
        private Mesh walls;
        
        private Player player;

        private Effect normalMapEffect;

        private Vector3 objectiveLocation; //block players must reach
        private Vector3 playerColor;

        //Networking Code
        private Dictionary<string, RemotePlayer> remotePlayers;
        private string localPlayerName;
        private NetClient client;
        private bool singleplayer;
        private int frameSkip = 4;
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

            setupNetworking();
        }

        private void setupNetworking()
        {
            NetPeerConfiguration config = new NetPeerConfiguration("SkySlider");
            config.EnableMessageType(NetIncomingMessageType.Data);
            client = new NetClient(config);
            client.Start();
            client.Connect("156.143.93.190", 16645);

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
            Thread.Sleep(50);

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

                                    try
                                    {
                                        remotePlayers[name].Position = new Vector3(x, y, z);
                                    }
                                    catch
                                    {
                                        
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
                                    int score = msg.ReadInt32();

                                    if (player != null)
                                    {
                                        player.Score = score;
                                    }
                                    //Update objective location
                                    objectiveLocation = new Vector3(objX, objY, objZ);
                                    break;
                            }
                            break;
                        case NetIncomingMessageType.ErrorMessage:
                            Console.WriteLine(msg.ReadString());
                            break;
                        default:
                            
                            break;
                    }

                    client.Recycle(msg);
                }

            }
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            base.LoadContent(content);

            BlockData.Initialize(Device, content);

            //map = new Map(Device, "Content/Levels/ramps.txt"); //Load map
            map = new Map(Device, "Content/Levels/Level1-1.txt");

            MeshBuilder mb = new MeshBuilder(Device);

            buildWalls(mb, content);

            setupNormalMapEffect(content);

            if (singleplayer)
            {
                objectiveLocation = map.getNextObjective(new Vector3(-1, -1, -1)); //get first objective
            }

            partition = new MapPartition(map);

            player = new Player(new Vector3(map.Width/2,20,map.Depth - 2)); //spawn player

            initializePhysicsEngine();

            //Misc initialization stuff
            sphere = mb.CreateSphere(1f, 10, 10);
            destination = mb.CreateSphere(0.5f, 12, 12); //sphere to draw at objective
            destination.Texture = content.Load<Texture2D>("Textures/BlockTextures/Destination");

            sBatch = new SpriteBatch(Device);
            sf = content.Load<SpriteFont>("Fonts/Helvetica");
            primBatch = new PrimitiveBatch(Device);

            playerColor = new Vector3((int.Parse(localPlayerName) % 5) / 5.0f, (int.Parse(localPlayerName) % 3) / 3.0f, (int.Parse(localPlayerName) % 2) / 2.0f);   
        }

        private void setupNormalMapEffect(ContentManager content)
        {
            //Set up the normal mapping effect
            Vector4 lightColor = new Vector4(1, 1, 1, 1);
            Vector4 ambientLightColor = new Vector4(.2f, .2f, .2f, 1);
            float shininess = 0.5f;
            float specularPower = 5.0f;

            normalMapEffect = content.Load<Effect>("Effects/NormalMap");
            normalMapEffect.Parameters["LightColor"].SetValue(lightColor);
            normalMapEffect.Parameters["AmbientLightColor"].SetValue(ambientLightColor);

            normalMapEffect.Parameters["Shininess"].SetValue(shininess);
            normalMapEffect.Parameters["SpecularPower"].SetValue(specularPower);
        }

        private void buildWalls(MeshBuilder mb, ContentManager content)
        {
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
            walls.NormalMap = content.Load<Texture2D>("Textures/BlockTextures/WallsN");
        }

        private void initializePhysicsEngine()
        {
            engine = new PhysicsEngine3D(partition);
            engine.Gravity = new Vector3(0, -0.1f, 0);
            engine.AddRigidBody(player.Body); //physics body of player

            //floor and walls
            engine.AddRigidBody(new PlaneBody(Vector3.Up, new Vector3(0, 1f, 0)));
            engine.AddRigidBody(new PlaneBody(new Vector3(1f, 0, 0), new Vector3(1f, 0, 0)));
            engine.AddRigidBody(new PlaneBody(new Vector3(-1f, 0, 0), new Vector3(map.Width - 1, 0, 0)));
            engine.AddRigidBody(new PlaneBody(new Vector3(0, 0, 1f), new Vector3(0, 0, 1f)));
            engine.AddRigidBody(new PlaneBody(new Vector3(0, 0, -1f), new Vector3(0, 0, map.Depth - 1)));
        }

        private void disconnectNetwork()
        {
            NetOutgoingMessage disconnectMessage = client.CreateMessage();
            disconnectMessage.Write((byte)ClientToServerProtocol.Disconnect);
            disconnectMessage.Write(localPlayerName);
            client.SendMessage(disconnectMessage, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
            client.Disconnect("bye");
        }

        public override void Update(GameTime g)
        {
            base.Update(g);

            if (InputHandler.IsKeyPressed(Keys.Escape))
            {
                ExitDesired = true;
                if (!singleplayer)
                {
                    disconnectNetwork();
                }
            }

            player.Update(g);
            engine.Update(g);
            updateObjective(g);

            updatePositionSending();

            if (objectiveLocation == new Vector3(-1, -1, -1))
            {
                gameOver = true;
            }
        }

        private void updatePositionSending()
        {
            currentFrame++;
            if (!singleplayer && currentFrame > frameSkip)
            {
                currentFrame = 0;
                //Send updated position
                NetOutgoingMessage newPosMsg = client.CreateMessage();
                newPosMsg.Write((byte)ClientToServerProtocol.UpdatePosition);
                newPosMsg.Write(localPlayerName);
                newPosMsg.Write(player.Body.Pos.X);
                newPosMsg.Write(player.Body.Pos.Y);
                newPosMsg.Write(player.Body.Pos.Z);

                client.SendMessage(newPosMsg, NetDeliveryMethod.Unreliable);
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
                    currentCoolDown = 0f;
                }
            }

            //If we're touching the objective
            if (((int)player.Body.Pos.X == objectiveLocation.X) &&
                ((int)player.Body.Pos.Y == objectiveLocation.Y) &&
                ((int)player.Body.Pos.Z == objectiveLocation.Z))
            {
                if (singleplayer)
                {
                    player.Score = player.Score + 1;
                    objectiveLocation = map.getNextObjective(objectiveLocation);
                }
                else
                {
                    if (!coolingDown)
                    {
                        NetOutgoingMessage objectiveHitMsg = client.CreateMessage();
                        objectiveHitMsg.Write((byte)ClientToServerProtocol.ObjectiveHit);
                        objectiveHitMsg.Write(localPlayerName);

                        client.SendMessage(objectiveHitMsg, NetDeliveryMethod.ReliableOrdered);
                        coolingDown = true;
                    }
                }
            }
        }

        public override void Draw(GameTime g)
        {
            base.Draw(g);

            normalMapEffect.Parameters["LightPosition"].SetValue(player.Body.Pos);
            normalMapEffect.CurrentTechnique = normalMapEffect.Techniques["NormalMapping"];

            drawPhysicsSpheres();
            drawRemotePlayers();
            drawObjective();
            drawWalls();
            drawDirectionArrow();
            normalMapEffect.CurrentTechnique = normalMapEffect.Techniques["NormalMappingInstancing"];
            drawMap(g);
            drawHUD();
        }

        private void drawHUD()
        {
            sBatch.Begin();
            sBatch.DrawString(sf, "Player ID: " + localPlayerName, new Vector2(20, 20), Color.AntiqueWhite);
            sBatch.DrawString(sf, "Score: " + player.Score.ToString(), new Vector2(20, 80), Color.AntiqueWhite);
            if (!singleplayer)
            {
                sBatch.DrawString(sf, "Players Online: " + (remotePlayers.Count + 1), new Vector2(this.width - 325, 20), Color.AntiqueWhite);
            }

            if (gameOver)
            {

                sBatch.DrawString(sf, "Game Over! Welcome to FREE RUN MODE!!!!!!!", new Vector2(this.width / 2 - 400, this.height / 2), Color.AntiqueWhite);
            }
            sBatch.End();
            Device.BlendState = BlendState.Opaque;
            Device.DepthStencilState = DepthStencilState.Default;
            Device.SamplerStates[0] = SamplerState.LinearWrap;
        }

        private void drawMap(GameTime g)
        {
            //map.DebugDraw(g, primBatch, player.Cam, normalMapEffect);
            map.DrawInstanced(g, primBatch, player.Cam, normalMapEffect);
        }

        private void drawDirectionArrow()
        {
            if (!gameOver)
            {
                Vector3 playerToObjective = objectiveLocation + new Vector3(0.5f, 0.5f, 0.5f) - player.Body.Pos;
                playerToObjective.Normalize();
                Vector3 tipPos = player.Body.Pos + 0.5f * playerToObjective;
                Vector3 tBase = Vector3.Cross(Vector3.Up, playerToObjective);
                tBase.Normalize();
                Vector3 A = player.Body.Pos + 0.16f * tBase;
                Vector3 B = player.Body.Pos - 0.16f * tBase;
                tipPos -= playerToObjective * .1f;
                A -= playerToObjective * .1f;
                B -= playerToObjective * .1f;

                primBatch.Begin(PrimitiveType.TriangleList, player.Cam);
                primBatch.FillTriangle(tipPos + new Vector3(0, 0.25f, 0), B + new Vector3(0, 0.25f, 0), A + new Vector3(0, 0.25f, 0), new Color(255, 105, 0));
                primBatch.FillTriangle(tipPos + new Vector3(0, 0.25f, 0), A + new Vector3(0, 0.25f, 0), B + new Vector3(0, 0.25f, 0), new Color(255, 105, 0));
                primBatch.End();
            }
        }

        private void drawWalls()
        {
            primBatch.DrawMesh(walls, Matrix.Identity, player.Cam, normalMapEffect);
        }

        private void drawObjective()
        {
            if (!gameOver)
            {
                primBatch.DrawMesh(destination, Matrix.CreateScale(1f) * Matrix.CreateTranslation(objectiveLocation + new Vector3(0.5f, 0.5f, 0.5f)), player.Cam);
            }
        }

        private void drawRemotePlayers()
        {
            foreach (string name in remotePlayers.Keys)
            {
                RemotePlayer p = remotePlayers[name];
                primBatch.DrawMesh(sphere, Matrix.CreateScale(0.18f) * Matrix.CreateTranslation(p.Position), player.Cam, new Vector3((int.Parse(name) % 4) / 4.0f, (int.Parse(name) % 3) / 3.0f, (int.Parse(name) % 2) / 2.0f));
            }
        }

        private void drawPhysicsSpheres()
        {
            for (int i = 0; i < engine.GetBodies().Count; i++)
            {
                SphereBody sb = engine.GetBodies()[i] as SphereBody;
                if (sb != null)
                {
                    if (sb.InContact)
                    {
                        playerColor = Color.Red.ToVector3();
                    }
                    else
                    {
                        playerColor = Color.Blue.ToVector3();
                    }
                    primBatch.DrawMesh(sphere, Matrix.CreateScale(sb.Radius) * Matrix.CreateTranslation(engine.GetBodies()[i].Pos), player.Cam, playerColor);
                }
            }
        }
    }
}
