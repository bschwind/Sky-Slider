﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.Graphics;
using GraphicsToolkit.Physics._3D.Geometry;
using GraphicsToolkit.Physics._3D.Bodies;
using GraphicsToolkit.Physics._3D;
using System.IO;

namespace SkySlider.Maps
{
    /// <summary>
    /// Holds data for and draws the 3D array of blocks that make up the map
    /// </summary>
    public class Map
    {
        private int width = 900;
        private int height = 5;
        private int depth = 900;

        private Block[, ,] blocks; //3D array of blocks

        private List<Vector3> objectiveVectors = new List<Vector3>(); //list of positions players must reach
        private Vector3 startLocation, endLocation; //start and end markers; get method will be needed.
        private int minManhattanDistance = 25; //minimum distance between start and end markers
        //start and end markers are only generated in the overloaded constructor.

        //private Dictionary<byte, Matrix[]> blockTransforms;
        private Dictionary<byte, VertexBuffer> blockTransforms;

        private bool isTestMap = true;

        // To store instance transform matrices in a vertex buffer, we use this custom
        // vertex type which encodes 4x4 matrices as a set of four Vector4 values.
        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

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
        public Map(GraphicsDevice device)
        {
            blockTransforms = new Dictionary<byte, VertexBuffer>();
            Dictionary<byte, List<Matrix>> tempBlockTransforms = new Dictionary<byte, List<Matrix>>();
            for (int i = 1; i < 12; i++)
            {
                tempBlockTransforms.Add((byte)i, new List<Matrix>());
            }

            blocks = new Block[width, height, depth];
            Random r = new Random();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        if (r.Next() % 29 == 0)
                        {
                            blocks[x, y, z].Type = (byte)r.Next(1, 12);
                            blocks[x, y, z].Type = 7;
                            blocks[x, y, z].RotationAxis = (byte)4;
                            blocks[x, y, z].Rotation = (byte)1;
                        }

                        if ((y == 0) || (x == 0) || (z == 0) || (x == width - 1) || (z == depth - 1)) //auto-create floor and walls
                        {
                            blocks[x, y, z].Type = 0;
                        }

                        if (tempBlockTransforms.ContainsKey(blocks[x, y, z].Type))
                        {
                            tempBlockTransforms[blocks[x, y, z].Type].Add(BlockData.GetRotationMatrix(blocks[x, y, z]) * Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)));
                        }
                    }
                }
            }

            foreach (byte b in tempBlockTransforms.Keys)
            {
                if (tempBlockTransforms[b].Count > 0)
                {
                    blockTransforms.Add(b, new VertexBuffer(device, instanceVertexDeclaration, tempBlockTransforms[b].Count, BufferUsage.None));
                    blockTransforms[b].SetData(tempBlockTransforms[b].ToArray());
                }
            }
        }
        /// <summary>
        /// Loads a previously saved map file
        /// </summary>
        /// <param name="dataDirectory">Path to the map file</param>
        public Map(GraphicsDevice device, String dataDirectory)
        {
            blockTransforms = new Dictionary<byte, VertexBuffer>();
            Dictionary<byte, List<Matrix>> tempBlockTransforms = new Dictionary<byte, List<Matrix>>();
            for (int i = 1; i < 12; i++)
            {
                tempBlockTransforms.Add((byte)i, new List<Matrix>());
            }

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
                            objectiveVectors.Add(new Vector3(x, y, z));
                            //blocks[x, y, z].Type = 9;
                        }
                        if ((x == width - 1) || (z == depth - 1) || (x == 0) || (z == 0) || (y == 0))
                        {
                            blocks[x, y, z].Type = 0;
                            
                        }

                        if(tempBlockTransforms.ContainsKey(blocks[x,y,z].Type))
                        {
                            tempBlockTransforms[blocks[x, y, z].Type].Add(BlockData.GetRotationMatrix(blocks[x, y, z]) * Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)));
                        }
                    }
                }
            }
            sr.Close();

            if (objectiveVectors.Count > 0)
            {
                isTestMap = false;
            }

            foreach (byte b in tempBlockTransforms.Keys)
            {
                if (tempBlockTransforms[b].Count > 0)
                {
                    blockTransforms.Add(b, new VertexBuffer(device, instanceVertexDeclaration, tempBlockTransforms[b].Count, BufferUsage.None));
                    blockTransforms[b].SetData(tempBlockTransforms[b].ToArray());
                }
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

                        batch.DrawMesh(BlockData.GetMeshFromID(blocks[x, y, z].Type), BlockData.GetRotationMatrix(blocks[x, y, z]) * Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)), cam);
                    }
                }
            }
        }

        public void DrawInstanced(GameTime g, PrimitiveBatch batch, Camera cam, Effect customEffect)
        {
            foreach (byte b in blockTransforms.Keys)
            {
                batch.DrawInstancedMesh(BlockData.GetMeshFromID(b), cam, customEffect, blockTransforms[b]);
            }
        }

        /// <summary>
        /// Draws each block
        /// </summary>
        /// <param name="g"></param>
        /// <param name="batch"></param>
        /// <param name="cam"></param>
        public void DebugDraw(GameTime g, PrimitiveBatch batch, Camera cam, Effect customEffect)
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
                        
                        batch.DrawMesh(BlockData.GetMeshFromID(blocks[x, y, z].Type), BlockData.GetRotationMatrix(blocks[x,y,z]) * Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)), cam, customEffect);
                    }
                }
            }
        }

        /// <summary>
        /// Draws each block, makes blocks between camera and player translucent
        /// </summary>
        /// <param name="g"></param>
        /// <param name="batch"></param>
        /// <param name="cam"></param>
        public void DebugDraw(GameTime g, PrimitiveBatch batch, Effect customEffect, ThirdPersonCamera cam)
        {
            //find out if block is between camera and player
            Vector3 camPos = cam.Pos;
            Vector3 playerPos = cam.TargetPos;
            playerPos += (camPos - playerPos) * 0.1f;
            AABB3D bounds = AABB3D.CreateFromPoints(new Vector3[] { camPos, playerPos });
            Vector3 min = bounds.GetMin();
            Vector3 max = bounds.GetMax();

            int startRow = (int)min.X;
            startRow = (int)Math.Max(startRow, 0);

            int startCol = (int)min.Y;
            startCol = (int)Math.Max(startCol, 0);

            int startStack = (int)min.Z;
            startStack = (int)Math.Max(startStack, 0);

            int endRow = (int)max.X;
            endRow = (int)Math.Min(endRow, width - 1);

            int endCol = (int)max.Y;
            endCol = (int)Math.Min(endCol, height - 1);

            int endStack = (int)max.Z;
            endStack = (int)Math.Min(endStack, depth - 1);

            List<int[]> translucentBlocks = new List<int[]>();

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

                        if ((x >= startRow) && (x <= endRow))
                        {
                            if ((y >= startCol) && (y <= endCol))
                            {
                                if ((z >= startStack) && (z <= endStack))
                                {
                                    //block is between camera and player, so store it for drawing later
                                    translucentBlocks.Add(new int[3]{x, y, z});
                                    //batch.DrawMesh(BlockData.GetMeshFromID(blocks[x, y, z].Type), BlockData.GetRotationMatrix(blocks[x, y, z]) * Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)), cam, 0.5f);
                                    continue;
                                }
                            }
                        }
                        //block isn't between camera and player, so draw block normally
                        batch.DrawMesh(BlockData.GetMeshFromID(blocks[x, y, z].Type), BlockData.GetRotationMatrix(blocks[x, y, z]) * Matrix.CreateTranslation(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)), cam, customEffect);
                    }
                }
            }
            foreach (int[] coords in translucentBlocks)//draw translucent blocks that are between player and camera
            {
                batch.DrawMesh(BlockData.GetMeshFromID(blocks[coords[0], coords[1], coords[2]].Type), BlockData.GetRotationMatrix(blocks[coords[0], coords[1], coords[2]]) * Matrix.CreateTranslation(new Vector3(coords[0] + 0.5f, coords[1] + 0.5f, coords[2] + 0.5f)), cam, 0.6f);
            }
        }


        //private void generateMarkers()
        //private void generateMarkers()
        //{
        //    Random r = new Random();
        //    int indexOffset = r.Next(0, objectiveVectors.Count);
        //    bool isSuccessful = false;

        //    for (int i = 0; i < objectiveVectors.Count; i++)
        //    {
        //        if (isSuccessful)
        //        {
        //            break;
        //        }
        //        startLocation = objectiveVectors.ElementAt((i + indexOffset) % objectiveVectors.Count);
        //        for (int j = 0; j < objectiveVectors.Count; j++)
        //        {
        //            endLocation = objectiveVectors.ElementAt(j);
        //            if (endLocation == startLocation)
        //            {
        //                continue;
        //            }
        //            if (Math.Abs(endLocation.X - startLocation.X) + Math.Abs(endLocation.Y - startLocation.Y) + Math.Abs(endLocation.Z - startLocation.Z) < minManhattanDistance)
        //            {
        //                continue;
        //            }

        //            //if below code is reached, the start/end combination are valid
        //            isSuccessful = true;
        //            break;
        //        }
        //    }
        //    if (!isSuccessful)
        //    {
        //        throw new Exception("No valid start/end combinations.");
        //    }
        //}

        public Vector3 getNextObjective(Vector3 previousObjective)
        {
            objectiveVectors.Remove(previousObjective);
            if (objectiveVectors.Count == 0) //no objectives left; game is over
            {
                return new Vector3(-1, -1, -1);
            }
            Random r = new Random();
            return objectiveVectors.ElementAt(r.Next(0, objectiveVectors.Count));
        }
    }
}
