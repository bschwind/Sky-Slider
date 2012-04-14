﻿using System;
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

        private List<Vector3> startEndMarkers = new List<Vector3>(); //list of Vector3s to be considered for start/end markers
        private Vector3 startLocation, endLocation; //start and end markers; get method will be needed.
        private int minManhattanDistance = 25; //minimum distance between start and end markers
        //start and end markers are only generated in the overloaded constructor.

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
                        if (y == 0) //auto-create floor
                        {
                            blocks[x, y, z].Type = 1;
                            continue;
                        }
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
                throw new Exception("Map file not found!");
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
                        if (blockDataString.Split(' ').Length == 4) //if block is start/end marker...
                        {
                            startEndMarkers.Add(new Vector3(x, y, z));
   //                         blocks[x, y, z].Type = 9;
                        }
                    }
                }
            }
            sr.Close();

            if (startEndMarkers.Count >= 2) //if there are sufficient start/end markers
            {
                generateMarkers(); //determine start/end positions
                Block startMarker = new Block();
                Block endMarker = new Block();
                startMarker.Type = 8; //for debug purposes
                endMarker.Type = 9;
                SetBlockAt((int)startLocation.X, (int)startLocation.Y, (int)startLocation.Z, startMarker);
                SetBlockAt((int)endLocation.X, (int)endLocation.Y, (int)endLocation.Z, endMarker);
            }


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

        private void generateMarkers()
        {
            Random r = new Random();
            int indexOffset = r.Next(0, startEndMarkers.Count);
            bool isSuccessful = false;

            for (int i = 0; i < startEndMarkers.Count; i++)
            {
                if (isSuccessful)
                {
                    break;
                }
                startLocation = startEndMarkers.ElementAt((i + indexOffset) % startEndMarkers.Count);
                for (int j = 0; j < startEndMarkers.Count; j++)
                {
                    endLocation = startEndMarkers.ElementAt(j);
                    if (endLocation == startLocation)
                    {
                        continue;
                    }
                    if (Math.Abs(endLocation.X - startLocation.X) + Math.Abs(endLocation.Y - startLocation.Y) + Math.Abs(endLocation.Z - startLocation.Z) < minManhattanDistance)
                    {
                        continue;
                    }

                    //if below code is reached, the start/end combination are valid
                    isSuccessful = true;
                    break;
                }
            }
            if (!isSuccessful)
            {
                throw new Exception("No valid start/end combinations.");
            }
        }
    }
}
