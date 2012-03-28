using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.Graphics;
using System.IO;

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
                            blocks[x, y, z].Type = 4;
                        }
                        else
                        {
                            blocks[x, y, z].Type = 0;
                        }
                    }
                }
            }
        }

        public Map(String dataDirectory)
        {
            if (!File.Exists(dataDirectory))
            {
                FileStream fs = File.Create(dataDirectory);
                fs.Close();
            }
            StreamReader sr = new StreamReader(dataDirectory);
            string widthHeightDepthString = sr.ReadLine();
            this.width = int.Parse(widthHeightDepthString.Split(' ')[0]);
            this.height = int.Parse(widthHeightDepthString.Split(' ')[1]);
            this.depth = int.Parse(widthHeightDepthString.Split(' ')[2]);

            blocks = new Block[width, height, depth];
            string blockDataString;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        blockDataString = sr.ReadLine();
                        blocks[x, y, z].Type = byte.Parse(blockDataString.Split(' ')[0]);
                        blocks[x, y, z].Rotation = byte.Parse(blockDataString.Split(' ')[1]);
                        blocks[x, y, z].RotationAxis = byte.Parse(blockDataString.Split(' ')[2]);
                    }
                }
            }
            sr.Close();

        }

        public Block GetBlockAt(int row, int col, int stack)
        {
            return blocks[(int)MathHelper.Clamp(row, 0, width-1), (int)MathHelper.Clamp(col, 0, height-1), (int)MathHelper.Clamp(stack, 0, depth-1)];
        }

        public void SetBlockAt(int row, int col, int stack, Block b)
        {
            blocks[(int)MathHelper.Clamp(row, 0, width-1), (int)MathHelper.Clamp(col, 0, height-1), (int)MathHelper.Clamp(stack, 0, depth-1)] = b;
        }

        public int GetRow(Vector3 pos)
        {
            return (int)pos.X;
        }

        public int GetCol(Vector3 pos)
        {
            return (int)pos.Y;
        }

        public int GetStack(Vector3 pos)
        {
            return (int)pos.Z;
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
