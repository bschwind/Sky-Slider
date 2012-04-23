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
        private PrimitiveBatch primBatch;
        private Player player;

        private Vector3 objectiveLocation; //block players must reach
        
        public MainGamePanel()
            : base(Vector2.Zero, Vector2.One)
        {

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
            engine.Gravity = new Vector3(0, -0.1f, 0);
            engine.AddRigidBody(player.Body); //physics body of player

            MeshBuilder mb = new MeshBuilder(Device);
            sphere = mb.CreateSphere(1f, 10, 10);
            destination = mb.CreateSphere(0.5f, 12, 12); //box to draw at objective
            destination.Texture = content.Load<Texture2D>("Textures/BlockTextures/Destination");

            primBatch = new PrimitiveBatch(Device);
        }

        public override void Update(GameTime g)
        {
            base.Update(g);
            player.Update(g);
            engine.Update(g);
            updateObjective(g);
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
            //Draw sphere at objective
            primBatch.DrawMesh(destination, Matrix.CreateScale(1f) * Matrix.CreateTranslation(objectiveLocation + new Vector3(0.5f, 0.5f, 0.5f)), player.Cam);

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
