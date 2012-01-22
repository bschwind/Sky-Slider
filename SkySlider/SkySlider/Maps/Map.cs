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
        private int width = 10;
        private int height = 10;
        private int depth = 10;

        private Block[, ,] blocks;

        public Map()
        {
            blocks = new Block[width, height, depth];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (x == 0 || y == 0 || z == 0)
                        {
                            blocks[x, y, z].Type = 2;
                        }
                    }
                }
            }
        }

        public void DebugDraw(GameTime g, PrimitiveBatch batch, Camera cam)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        byte type = blocks[x, y, z].Type;
                        if (type == 0)
                        {
                            continue;
                        }

                        batch.DrawMesh(BlockData.GetMeshFromID(blocks[x, y, z].Type), Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)), cam);
                    }
                }
            }
        }
    }
}
