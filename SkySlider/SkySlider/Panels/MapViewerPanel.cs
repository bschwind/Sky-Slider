using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.GUI;
using GraphicsToolkit.Graphics;
using GraphicsToolkit.Input;
using GraphicsToolkit.Graphics.SceneGraph;
using SkySlider.Maps;

namespace SkySlider.Panels
{
    public class MapViewerPanel : Panel
    {
        Map map;
        FirstPersonCamera cam;
        PrimitiveBatch primBatch;
        Mesh testMesh;
        MeshNode mNode;
        float rotation = 0f;

        public MapViewerPanel(Vector2 upLeft, Vector2 botRight)
            : base(upLeft, botRight)
        {
            cam = new FirstPersonCamera(0.5f, 10f);
            cam.Pos = new Vector3(0, 3, 13);
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            base.LoadContent(content);

            map = new Map();
            BlockData.BuildMeshes(Device, content);
            primBatch = new PrimitiveBatch(Device);

            MeshBuilder mb = new MeshBuilder(Device);
            mb.Begin();
            //Fix up this case
            mb.AddCylinder(1, 1, 50);
            testMesh = mb.End();

            mNode = new MeshNode(testMesh);
            mNode.SetPos(new Vector3(0, 0, 0));
            mNode.SetScl(new Vector3(1, 1, 1));

            MeshNode childNode = new MeshNode(testMesh);
            childNode.SetPos(new Vector3(0, 2, 0));
            childNode.SetScl(new Vector3(1));

            MeshNode anotherNode = new MeshNode(testMesh);
            anotherNode.SetPos(new Vector3(0, 2, 0));
            anotherNode.SetScl(new Vector3(50, 1, 1));
            childNode.AddChild(anotherNode);

            mNode.AddChild(childNode);
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            cam.Resize();
        }

        public override void Update(GameTime g)
        {
            base.Update(g);
            cam.Update(g);

            rotation += 0.1f;
            mNode.SetRotation(Quaternion.CreateFromAxisAngle(Vector3.Up, rotation));
        }

        public override void Draw(GameTime g)
        {
            base.Draw(g);

            primBatch.Begin(PrimitiveType.LineList, cam);
            primBatch.DrawXZGrid(10, 10, Color.Blue);
            primBatch.DrawXYGrid(10, 10, Color.Red);
            primBatch.DrawYZGrid(10, 10, Color.Green);
            primBatch.End();

            mNode.Draw(g, Matrix.Identity, primBatch, cam);

            //map.DebugDraw(g, primBatch, cam);
        }
    }
}
