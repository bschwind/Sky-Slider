﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.GUI;
using GraphicsToolkit.Graphics;
using SkySlider.Maps;

namespace SkySlider.Panels
{
    public class MapViewerPanel : Panel
    {
        Map map;
        FirstPersonCamera cam;
        PrimitiveBatch primBatch;
        Mesh testMesh;

        public MapViewerPanel()
            : base(Vector2.Zero, Vector2.One)
        {
            
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            base.LoadContent(content);

            map = new Map();
            cam = new FirstPersonCamera(0.5f, 10f);
            cam.Pos = new Vector3(0, 3, 13);
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
        }

        public override void Update(GameTime g)
        {
            base.Update(g);

            cam.Update(g);
        }

        public override void Draw(GameTime g)
        {
            base.Draw(g);
            Panel.Device.Clear(Color.Black);

            primBatch.Begin(PrimitiveType.LineList, cam);
            primBatch.DrawXZGrid(10, 10, Color.Blue);
            primBatch.End();

            primBatch.DrawMesh(testMesh, cam);
        }
    }
}
