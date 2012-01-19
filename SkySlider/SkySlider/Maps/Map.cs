using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.Graphics;

namespace SkySlider.Maps
{
    public class Map
    {
        private int width = 100;
        private int height = 40;
        private int depth = 100;

        private Block[, ,] blocks;

        public Map()
        {
            blocks = new Block[width, height, depth];
        }

        public void DebugDraw(GameTime g, PrimitiveBatch batch, Camera cam)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        batch.DrawMesh(BlockData.GetMeshFromID(blocks[x,y,z].Type), cam);
                    }
                }
            }
        }
    }
}
