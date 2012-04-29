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

            map = new Map(Device, "startPosTest.txt");
    //        map = new Map();
            BlockData.Initialize(Device, content);
            primBatch = new PrimitiveBatch(Device);

            MeshBuilder mb = new MeshBuilder(Device);
            mb.Begin();

            //mb.AddCylinder(1, 1, 50);
            mb.AddSphere(1, 20, 20);
            testMesh = mb.End();

            mNode = new MeshNode(testMesh);
            mNode.SetPivot(new Vector3(0, -1f, 0));
            mNode.SetScl(new Vector3(1, 5, 1));
            MeshNode child = new MeshNode(testMesh);
            child.SetPos(new Vector3(0, 2, 0));
            MeshNode another = new MeshNode(testMesh);
            another.SetPos(new Vector3(0, 2, 0));


            //Mesh sphere = mb.CreateSphere(0.1f, 10, 10);
            //startMarker = new MeshNode(sphere);
            //startMarker.SetPos(map.StartPos);
            //child.AddChild(another);
            //mNode.AddChild(child);
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
            mNode.SetRotation(Quaternion.CreateFromAxisAngle(Vector3.Right, rotation));
        }

        public override void Draw(GameTime g)
        {
            base.Draw(g);

            primBatch.Begin(PrimitiveType.LineList, cam);
            primBatch.DrawXZGrid(map.Width, map.Depth, Color.Blue);
            primBatch.DrawXYGrid(map.Width, map.Height, Color.Red);
            primBatch.DrawYZGrid(map.Height, map.Depth, Color.Green);

  //          primBatch.DrawAABB(new Vector3((int)map.StartPos.X, (int)map.StartPos.Y, (int)map.StartPos.Z) + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), Color.Gold);
            primBatch.End();

            
            //mNode.Draw(g, Matrix.Identity, primBatch, cam);

            map.DebugDraw(g, primBatch, cam);
        }
    }
}
