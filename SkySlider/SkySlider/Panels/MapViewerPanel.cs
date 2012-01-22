using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.GUI;
using GraphicsToolkit.Graphics;
using GraphicsToolkit.Input;
using SkySlider.Maps;

namespace SkySlider.Panels
{
    public class MapViewerPanel : Panel
    {
        Map map;
        FirstPersonCamera cam;
        Camera2D c2;
        PrimitiveBatch primBatch;
        Mesh testMesh;

        public MapViewerPanel(Vector2 upLeft, Vector2 botRight)
            : base(upLeft, botRight)
        {
            cam = new FirstPersonCamera(0.5f, 10f);
            cam.Pos = new Vector3(0, 3, 13);
            c2 = new Camera2D(Vector2.Zero, this);
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
            mb.AddCylinder(0.3f, 3, 100);
            testMesh = mb.End();
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
            cam.Resize();
            c2.Resize();
        }

        public override void Update(GameTime g)
        {
            base.Update(g);
            cam.Update(g);
            //c2.Update(g);
        }

        public override void Draw(GameTime g)
        {
            base.Draw(g);

            primBatch.Begin(PrimitiveType.LineList, cam);
            primBatch.DrawXZGrid(10, 10, Color.Blue);
            primBatch.DrawXYGrid(10, 10, Color.Red);
            primBatch.DrawYZGrid(10, 10, Color.Green);
            //primBatch.Draw2DGrid(10, 10, Color.Orange);
            primBatch.End();

            map.DebugDraw(g, primBatch, cam);
        }
    }
}
