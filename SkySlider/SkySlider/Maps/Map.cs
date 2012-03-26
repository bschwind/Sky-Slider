﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.Graphics;

namespace SkySlider.Maps
{
    /// <summary>
    /// Holds data for and draws the 3D array of blocks that make up the map
    /// </summary>
    public class Map
    {
        private int width = 10;
        private int height = 10;
        private int depth = 10;

        private Block[, ,] blocks; //3D array of blocks

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
        }

        public int Depth
        {
            get
            {
                return depth;
            }
        }

        /// <summary>
        /// Generates the map by placing blocks into the block array
        /// </summary>
        public Map()
        {
            blocks = new Block[width, height, depth];
            Random r = new Random();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        blocks[x, y, z].Type = (byte)r.Next(0, 11);
                        blocks[x, y, z].RotationAxis = (byte)0;
                        blocks[x, y, z].Rotation = (byte)0;
                        if (r.Next() % 4 == 0)
                        {
                            blocks[x, y, z].Type = 1;
                        }
                        else
                        {
                            blocks[x, y, z].Type = 0;
                        }
                    }
                }
            }
        }

        public Block GetBlockAt(int row, int col, int stack)
        {
            return blocks[row, col, stack];
        }

        public void SetBlockAt(int row, int col, int stack, Block b)
        {
            blocks[row, col, stack] = b;
        }

        /// <summary>
        /// Draws each block
        /// </summary>
        /// <param name="g"></param>
        /// <param name="batch"></param>
        /// <param name="cam"></param>
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

                        batch.DrawMesh(BlockData.GetMeshFromID(blocks[x, y, z].Type), BlockData.GetRotationMatrix(blocks[x,y,z]) * Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)), cam);
                    }
                }
            }
        }
    }
}
