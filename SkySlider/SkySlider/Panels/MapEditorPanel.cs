using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using GraphicsToolkit.GUI;
using GraphicsToolkit.Graphics;
using GraphicsToolkit.Graphics.SceneGraph;
using GraphicsToolkit.Input;
using GraphicsToolkit.Physics._3D;
using GraphicsToolkit.Physics._3D.Partitions;
using GraphicsToolkit.Physics._3D.Bodies;
using SkySlider.Maps;
using System.IO;
using System.Windows.Forms;

using Keys = Microsoft.Xna.Framework.Input.Keys;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Panel = GraphicsToolkit.GUI.Panel;

namespace SkySlider.Panels

    //use digits, -, and + to create blocks
    //up and down change rotation
    //left and right change rotation axis
    //Q saves the file
{
    public class MapEditorPanel : Panel
    {
        private PhysicsEngine3D engine;
        private MapPartition partition;
        private Map map;
        private FirstPersonCamera cam;
        private Mesh sphere;
        private MeshNode marker;
        private PrimitiveBatch primBatch;
        private MouseState mouseStateCurrent;
        private byte placeType;
        private Block placeBlock = new Block();
        private Vector3 placePos;
        private LinkedList<Vector3> startEndMarkers = new LinkedList<Vector3>();
        private byte rot;
        private byte axis;
        

        public MapEditorPanel()
            : base(Vector2.Zero, Vector2.One)
        {
            
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            base.LoadContent(content);

            BlockData.Initialize(Device, content);

            cam = new FirstPersonCamera(0.5f, 10);
            cam.Pos = new Vector3(3, 3, 13);


            map = new Map();

            partition = new MapPartition(map);
            engine = new PhysicsEngine3D(partition);

            MeshBuilder mb = new MeshBuilder(Device);
            sphere = mb.CreateSphere(0.1f, 10, 10);
            marker = new MeshNode(sphere);

            placeType = 1;

            primBatch = new PrimitiveBatch(Device);


        }

        public override void Update(GameTime g)
        {
            cam.Update(g);
            base.Update(g);

            placePos = cam.Pos + 2 * cam.Dir;

            mouseStateCurrent = Mouse.GetState();

            checkNumKeys();

            if (InputHandler.MouseState.LeftButton == ButtonState.Pressed)
            {
                //if (map.GetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z).Type == 0)
                //{
                    placeBlock.Type = placeType;
                    placeBlock.RotationAxis = axis;
                    placeBlock.Rotation = rot;
                    map.SetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z, placeBlock);
                //}
                //else
                //{
                //    placeBlock.Type = 0;
                //    map.SetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z, placeBlock);
                //    placeBlock.Type = placeType;
                //}
            }

            UpdateRotations(placePos);
            UpdateStartEndMarkers();
            UpdateSave();

      //      marker.SetPos(placePos);
           
            engine.Update(g);

        }

        private void UpdateRotations(Vector3 placePos)
        {
            if (InputHandler.IsNewKeyPress(Keys.Left))
            {
                Block b = map.GetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z);
                axis = b.RotationAxis;
                axis += 1;
                axis %= 6;
                b.RotationAxis = axis;
                map.SetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z, b);
            }

            if (InputHandler.IsNewKeyPress(Keys.Right))
            {
                Block b = map.GetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z);
                axis = b.RotationAxis;
                if (axis == 0)
                {
                    axis = 5;
                }
                else
                {
                    axis--;
                }
                b.RotationAxis = axis;
                map.SetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z, b);
            }

            if (InputHandler.IsNewKeyPress(Keys.Up))
            {
                Block b = map.GetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z);
                rot = b.Rotation;
                rot += 1;
                rot %= 4;
                b.Rotation = rot;
                map.SetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z, b);
            }

            if (InputHandler.IsNewKeyPress(Keys.Down))
            {
                Block b = map.GetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z);
                rot = b.Rotation;
                if (rot == 0)
                {
                    rot = 3;
                }
                else
                {
                    rot--;
                }
                b.Rotation = rot;
                map.SetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z, b);
            }
        }

        private void UpdateStartEndMarkers()
        {
            if (InputHandler.IsNewKeyPress(Keys.E))
            {
                Block b = map.GetBlockAt((int)placePos.X, (int)placePos.Y, (int)placePos.Z);
                if (startEndMarkers.Contains(new Vector3((int)placePos.X, (int)placePos.Y, (int)placePos.Z)))
                {
                    startEndMarkers.Remove(new Vector3((int)placePos.X, (int)placePos.Y, (int)placePos.Z));
                }
                else
                {
                    startEndMarkers.AddLast(new Vector3((int)placePos.X, (int)placePos.Y, (int)placePos.Z));
                }
            }

        }

        private void UpdateSave()
        {
            if (InputHandler.KeyState.IsKeyDown(Keys.Q) && InputHandler.LastKeyState.IsKeyUp(Keys.Q))
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.ShowDialog();
                StreamWriter sw = new StreamWriter(dialog.FileName);
                sw.WriteLine(map.Width + " " + map.Height + " " + map.Depth);
                for (int x = 0; x < map.Width; x++)
                {
                    for (int y = 0; y < map.Height; y++)
                    {
                        for (int z = 0; z < map.Depth; z++)
                        {
                            Block b = map.GetBlockAt(x, y, z);
                            sw.Write(b.Type + " " + b.Rotation + " " + b.RotationAxis);
                            if (startEndMarkers.Contains(new Vector3(x, y, z)))
                            {
                                sw.WriteLine(" " + 1);
                            }
                            else
                            {
                                sw.WriteLine();
                            }

                        }
                    }
                }


                sw.Close();


            }
        }

        public override void Draw(GameTime g)
        {
            base.Draw(g);
            marker.Draw(g, Matrix.Identity, primBatch, cam);
            primBatch.DrawMesh(sphere, Matrix.Identity, cam);

            primBatch.Begin(Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList, cam);
            primBatch.DrawXZGrid(map.Width, map.Depth, Color.Blue);        
            primBatch.DrawXYGrid(map.Width, map.Height, Color.Red);       
            primBatch.DrawYZGrid(map.Height, map.Depth, Color.Green);

            //Draw preview box
            primBatch.DrawAABB(new Vector3((int)placePos.X, (int)placePos.Y, (int)placePos.Z) + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), Color.Orange);

            foreach (Vector3 v in startEndMarkers)
            {
                primBatch.DrawAABB(v + new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), Color.HotPink);
            }
            
            primBatch.End();

            map.DebugDraw(g, primBatch, cam);
        }

        private void checkNumKeys()
        {
            if (InputHandler.KeyState.IsKeyDown(Keys.D1))
            {
                placeType = 1;
            }

            if (InputHandler.KeyState.IsKeyDown(Keys.D2))
            {
                placeType = 2;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.D3))
            {
                placeType = 3;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.D4))
            {
                placeType = 4;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.D5))
            {
                placeType = 5;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.D6))
            {
                placeType = 6;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.D7))
            {
                placeType = 7;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.D8))
            {
                placeType = 8;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.D9))
            {
                placeType = 9;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.D0))
            {
                placeType = 0;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.OemMinus))
            {
                placeType = 10;
            }
            if (InputHandler.KeyState.IsKeyDown(Keys.OemPlus))
            {
                placeType = 11;
            }
        }
    }
}
