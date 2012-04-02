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
using SkySlider.Maps;

namespace SkySlider.Panels
{
    public class MainGamePanel : Panel
    {
        private PhysicsEngine3D engine;
        private MapPartition partition;
        private Map map;
        private FirstPersonCamera cam;
        private Mesh sphere;
        private PrimitiveBatch primBatch;
        private Player player;

        public MainGamePanel()
            : base(Vector2.Zero, Vector2.One)
        {

        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            base.LoadContent(content);

            BlockData.Initialize(Device, content);

            cam = new FirstPersonCamera(0.5f, 10);
            cam.Pos = new Vector3(3, 3, 13);

            map = new Map();
            partition = new MapPartition(map);

            player = new Player(new Vector3(5,5,5));

            engine = new PhysicsEngine3D(partition);
            engine.AddRigidBody(player.Body);

            MeshBuilder mb = new MeshBuilder(Device);
            sphere = mb.CreateSphere(1f, 10, 10);

            primBatch = new PrimitiveBatch(Device);
        }

        public override void Update(GameTime g)
        {
            base.Update(g);

            if (InputHandler.IsKeyPressed(Keys.Space))
            {
                engine.AddRigidBody(new SphereBody(cam.Pos, cam.Dir * 10f, 1f, 0.1f));
            }

            //cam.Update(g);
            player.Update(g);
            engine.Update(g);
        }

        public override void Draw(GameTime g)
        {
            base.Draw(g);

            for (int i = 0; i < engine.GetBodies().Count; i++)
            {
                SphereBody sb = engine.GetBodies()[i] as SphereBody;
                if (sb != null)
                {
                    primBatch.DrawMesh(sphere, Matrix.CreateScale(sb.Radius) * Matrix.CreateTranslation(engine.GetBodies()[i].Pos), player.Cam);
                }
            }

            map.DebugDraw(g, primBatch, player.Cam);
        }
    }
}
