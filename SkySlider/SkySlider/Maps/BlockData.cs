using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.Graphics;
using GraphicsToolkit.Physics._3D;
using GraphicsToolkit.Physics._3D.Bodies;
using SkySlider.Maps.BlockBodies;

namespace SkySlider.Maps
{
    /// <summary>
    /// This static class contains methods for the creation and handling of the blocks
    /// that make up the map.
    /// </summary>
    public class BlockData
    {
        //0 = empty
        //1 = box
        //2 = half box
        //3 = quarter box
        //4 = eighth box
        //5 = convex ramp
        //6 = slope1 ramp
        //7 = slope2 ramp
        //8 = slopehalf ramp
        //9 = concave ramp
        //10 = slope2 base
        //11 = slopehalf base

        private static Mesh[] blockMeshes;
        private static int sphereIterations = 6; //level of detail for curved blocks
        private static Matrix[,] rotations;
        private static Matrix[,] inverseRotations;
        private static RigidBody3D[] blockBodies;

        //Texture coordinates for blocks

        static Vector2[] front = new Vector2[4];
        static Vector2[] left = new Vector2[4];
        static Vector2[] top = new Vector2[4];
        static Vector2[] bottom = new Vector2[4];

        /// <summary>
        /// Uses MeshBuilder to create a mesh for each block type, storing them into an array
        /// </summary>
        /// <param name="g"></param>
        /// <param name="content"></param>
        public static void Initialize(GraphicsDevice g, ContentManager content)
        {
            front[0] = new Vector2(.5f, .5f);
            front[1] = new Vector2(1, .5f);
            front[2] = new Vector2(1, 1);
            front[3] = new Vector2(.5f, 1);

            left[0] = new Vector2(0, .5f);
            left[1] = new Vector2(.5f, .5f);
            left[2] = new Vector2(.5f, 1);
            left[3] = new Vector2(0, 1);

            top[0] = new Vector2(.5f, 0);
            top[1] = new Vector2(1, 0);
            top[2] = new Vector2(1, .5f);
            top[3] = new Vector2(.5f, .5f);

            bottom[0] = new Vector2(0, 0);
            bottom[1] = new Vector2(.5f, 0);
            bottom[2] = new Vector2(.5f, .5f);
            bottom[3] = new Vector2(0, .5f);


            MeshBuilder mb = new MeshBuilder(g);

            blockMeshes = new Mesh[20];
            blockMeshes[1] = BuildBoxMesh(mb, content);
            blockMeshes[2] = BuildHalfBoxMesh(mb, content);
            blockMeshes[3] = BuildQuarterBoxMesh(mb, content);
            blockMeshes[4] = BuildEighthBoxMesh(mb, content);
            blockMeshes[5] = BuildConvexRamp(mb, content);
            blockMeshes[6] = BuildSlope1Ramp(mb, content);
            blockMeshes[7] = BuildSlope2Ramp(mb, content);
            blockMeshes[8] = BuildSlopeHalfRamp(mb, content);
            blockMeshes[9] = BuildConcaveRamp(mb, content);
            blockMeshes[10] = BuildSlope2Base(mb, content);
            blockMeshes[11] = BuildSlopeHalfBase(mb, content);

            BuildRotations();
            AssembleBlockBodies();
        }

        private static void AssembleBlockBodies()
        {
            blockBodies = new RigidBody3D[15];

            blockBodies[1] = new BoxBody();
            blockBodies[2] = new HalfBoxBody();
            blockBodies[3] = new QuarterBoxBody();
            blockBodies[4] = new EightBoxBody();
            blockBodies[5] = new ConvexRampBody();
            blockBodies[6] = new Slope1RampBody();
            blockBodies[7] = new Slope2RampBody();
            blockBodies[8] = new SlopeHalfRampBody();
            blockBodies[9] = new ConcaveRampBody();
            blockBodies[10] = new Slope2BaseBody();
        }

        private static void BuildRotations()
        {
            rotations = new Matrix[6, 4];

            float threePiOverTwo = (MathHelper.Pi * 3f) / 2f;

            rotations[0, 0] = Matrix.Identity;
            rotations[0, 1] = Matrix.CreateRotationX(MathHelper.PiOver2);
            rotations[0, 2] = Matrix.CreateRotationX(MathHelper.Pi);
            rotations[0, 3] = Matrix.CreateRotationX(threePiOverTwo);

            rotations[1, 0] = Matrix.CreateRotationY(-MathHelper.PiOver2);
            rotations[1, 1] = Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.PiOver2);
            rotations[1, 2] = Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateRotationZ(MathHelper.Pi);
            rotations[1, 3] = Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateRotationZ(threePiOverTwo);

            rotations[2, 0] = Matrix.CreateRotationY(-MathHelper.Pi);
            rotations[2, 1] = Matrix.CreateRotationY(-MathHelper.Pi) * Matrix.CreateRotationX(-MathHelper.PiOver2);
            rotations[2, 2] = Matrix.CreateRotationY(-MathHelper.Pi) * Matrix.CreateRotationX(-MathHelper.Pi);
            rotations[2, 3] = Matrix.CreateRotationY(-MathHelper.Pi) * Matrix.CreateRotationX(-threePiOverTwo);

            rotations[3, 0] = Matrix.CreateRotationY(-threePiOverTwo);
            rotations[3, 1] = Matrix.CreateRotationY(-threePiOverTwo) * Matrix.CreateRotationZ(-MathHelper.PiOver2);
            rotations[3, 2] = Matrix.CreateRotationY(-threePiOverTwo) * Matrix.CreateRotationZ(-MathHelper.Pi);
            rotations[3, 3] = Matrix.CreateRotationY(-threePiOverTwo) * Matrix.CreateRotationZ(-threePiOverTwo);

            rotations[4, 0] = Matrix.CreateRotationZ(MathHelper.PiOver2);
            rotations[4, 1] = Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2);
            rotations[4, 2] = Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.Pi);
            rotations[4, 3] = Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateRotationY(threePiOverTwo);

            rotations[5, 0] = Matrix.CreateRotationZ(-MathHelper.PiOver2);
            rotations[5, 1] = Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2);
            rotations[5, 2] = Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.Pi);
            rotations[5, 3] = Matrix.CreateRotationZ(-MathHelper.PiOver2) * Matrix.CreateRotationY(threePiOverTwo);

            inverseRotations = new Matrix[6, 4];

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    inverseRotations[i, j] = Matrix.Invert(rotations[i, j]);
                }
            }            
        }

        public static Matrix GetRotationMatrix(Block b)
        {
            return rotations[b.RotationAxis, b.Rotation];
        }

        public static Matrix GetInverseRotationMatrix(Block b)
        {
            return inverseRotations[b.RotationAxis, b.Rotation];
        }

        public static RigidBody3D GetBlockBody(Block b)
        {
            return blockBodies[(int)b.Type];
        }

        /*The following methods (BuildBoxMesh(), BuildHalfBoxMesh(), BuildSlopeHalfBase(), etc.)
         * each use MeshBuilder to build a mesh of the appropriate block type*/

        private static Mesh BuildBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false, front[0], front[1], front[2], front[3]);
            //left
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, left[0], left[1], left[2], left[3]);
            //back
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false, front[1], front[0], front[3], front[2]);
            //right
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false, left[1], left[0], left[3], left[2]);
            //top
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false, top[0], top[1], top[2], top[3]);
            //bottom
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, bottom[0], bottom[1], bottom[2], bottom[3]);

            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block1");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");
            return m;
        }

        private static Mesh BuildHalfBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 0.5f;
            float depth = 1f;

            Vector2[] localFront = new Vector2[4];
            Vector2[] localLeft = new Vector2[4];
            Array.Copy(front, localFront, localFront.Length);
            Array.Copy(left, localLeft, localLeft.Length);

            for (int i = 0; i < 2; i++)
            {
                localFront[i].Y = (front[i].Y - 1) / 2 + 1;
                localLeft[i].Y = (left[i].Y - 1) / 2 + 1;
            }

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false, localFront[0], localFront[1], localFront[2], localFront[3]);
            //left
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, localLeft[0], localLeft[1], localLeft[2], localLeft[3]);
            //back
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false, localFront[1], localFront[0], localFront[3], localFront[2]);
            //right
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false, localLeft[1], localLeft[0], localLeft[3], localLeft[2]);
            //top
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false, top[0], top[1], top[2], top[3]);
            //bottom
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, bottom[0], bottom[1], bottom[2], bottom[3]);
            mb.OffsetAllVerts(new Vector3(0, -0.25f, 0)); //Offset the verts, because the above code centers the tile over the wrong position

            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block2");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");

            return m;
        }

        private static Mesh BuildQuarterBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 0.5f;
            float depth = 0.5f;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false, front[0] + new Vector2(0, .25f), front[1] + new Vector2(0, .25f), front[2], front[3]);
            //left
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, left[0] + new Vector2(0, .25f), left[1] + new Vector2(0, .25f), left[2], left[3]);
            //back
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false, front[1] + new Vector2(0, .25f), front[0] + new Vector2(0, .25f), front[3], front[2]);
            //right
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false, left[1] + new Vector2(0, .25f), left[0] + new Vector2(0, .25f), left[3], left[2]);
            //top
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false, top[0], top[1], top[2] - new Vector2(0, .25f), top[3] - new Vector2(0, .25f));
            //bottom
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, bottom[0] + new Vector2(0, .25f), bottom[1] + new Vector2(0, .25f), bottom[2], bottom[3]);
            mb.OffsetAllVerts(new Vector3(0, -0.25f, -0.25f)); //Offset the verts, because the above code centers the tile over the wrong position
            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block2");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");

            return m;
        }

        private static Mesh BuildEighthBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 0.5f;
            float height = 0.5f;
            float depth = 0.5f;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false, front[0], front[1], front[2], front[3]);
            //left
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, left[0], left[1], left[2], left[3]);
            //back
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false, front[1], front[0], front[3], front[2]);
            //right
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false, left[1], left[0], left[3], left[2]);
            //top
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false, top[0], top[1], top[2], top[3]);
            //bottom
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, bottom[0], bottom[1], bottom[2], bottom[3]);
            mb.OffsetAllVerts(new Vector3(-0.25f, -0.25f, -0.25f)); //Offset the verts, because the above code centers the tile over the wrong position
            
            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block1");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");

            return m;
        }

        private static Mesh BuildConvexRamp(MeshBuilder mb, ContentManager c)
        {

            mb.Begin();
            //mb.AddQuad(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f), false);
            mb.AddQuad(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), false, left[0], left[1], left[2], left[3]);
            mb.AddQuad(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), false, bottom[0], bottom[1], bottom[2], bottom[3]);

            for (int i = 0; i < sphereIterations; i++)
            {
                //Draw the curved surface, and the two sides
                float angle = ((float)i / sphereIterations) * MathHelper.PiOver2;
                float nextAngle = ((float)(i + 1) / sphereIterations) * MathHelper.PiOver2;
                Vector3 offset = new Vector3(-0.5f);

                Vector3 v1 = new Vector3(0, (float)Math.Cos(angle), (float)Math.Sin(angle)) + offset;
                Vector3 v2 = new Vector3(1, (float)Math.Cos(angle), (float)Math.Sin(angle)) + offset;
                Vector3 v3 = new Vector3(1, (float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) + offset;
                Vector3 v4 = new Vector3(0, (float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) + offset;

                mb.AddQuad(v1, v2, v3, v4, true, new Vector2(0.5f, i * (0.5f / sphereIterations)), new Vector2(1, (i + 1) * (0.5f / sphereIterations)));

                //Draw the two sides
                v1 = new Vector3(1, 0, 0) + offset;
                v2 = new Vector3(1, (float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) + offset;
                v3 = new Vector3(1, (float)Math.Cos(angle), (float)Math.Sin(angle)) + offset;

                //calculate texture coordinates
                Vector2 v1Tex = new Vector2(v1.Z + .5f, -v1.Y + .5f);
                Vector2 v2Tex = new Vector2(v2.Z + .5f, -v2.Y + .5f);
                Vector2 v3Tex = new Vector2(v3.Z + .5f, -v3.Y + .5f);
                v1Tex /= 2;
                v2Tex /= 2;
                v3Tex /= 2;

                //add back side
                mb.AddTriangle(v1, v2, v3, new Vector2(v1Tex.X + .5f, v1Tex.Y + .5f), new Vector2(v2Tex.X + .5f, v2Tex.Y + .5f), new Vector2(v3Tex.X + .5f, v3Tex.Y + .5f), false);
                offset = new Vector3(-1, 0, 0);
                //add front side
                mb.AddTriangle(v1 + offset, v3 + offset, v2 + offset, new Vector2(v1Tex.X + .5f, v1Tex.Y + .5f), new Vector2(v3Tex.X + .5f, v3Tex.Y + .5f), new Vector2(v2Tex.X + .5f, v2Tex.Y + .5f), false);
            }

            mb.RotateAllVerts(Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2));

            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block5");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");

            return m;
        }

        private static Mesh BuildSlope1Ramp(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            float widthHalf = width / 2;
            float heightHalf = height / 2;
            float depthHalf = depth / 2;

            mb.Begin();
            //front
            mb.AddTriangle(new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), front[0], front[2], front[3], false);
            //left
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false, left[0], left[1], left[2], left[3]);
            //back
            mb.AddTriangle( new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), front[0], front[3], front[2], false);
            //right
            mb.AddQuad(new Vector3(-widthHalf, heightHalf, (depthHalf)), new Vector3(-widthHalf, heightHalf, -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), false, top[0], top[1], top[2], top[3]);
            //bottom
            mb.AddQuad(new Vector3(-widthHalf, -(heightHalf), (depthHalf)), new Vector3(widthHalf, -(heightHalf), (depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false, bottom[0], bottom[1], bottom[2], bottom[3]);
            
            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block6");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");

            return m;
        }

        private static Mesh BuildSlope2Ramp(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            float widthHalf = width / 2;
            float heightHalf = height / 2;
            float depthHalf = depth / 2;

            mb.Begin();
            //front
            mb.AddTriangle(new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3((0), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), front[0], front[2] - new Vector2(.25f, 0), front[3], false);
            //left
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false, left[0], left[1], left[2], left[3]);
            //back
            mb.AddTriangle(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), new Vector3((0), -(heightHalf), -(depthHalf)), front[0], front[3], front[2] - new Vector2(.25f, 0), false);
            //right
            mb.AddQuad(new Vector3(-widthHalf, heightHalf, (depthHalf)), new Vector3(-widthHalf, heightHalf, -(depthHalf)), new Vector3((0), -(heightHalf), -(depthHalf)), new Vector3((0), -(heightHalf), (depthHalf)), false, top[0], top[1], top[2], top[3]);
            //bottom
            mb.AddQuad(new Vector3(-widthHalf, -(heightHalf), (depthHalf)), new Vector3(0, -(heightHalf), (depthHalf)), new Vector3((0), -(heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false, bottom[0], bottom[1] - new Vector2(.25f, 0), bottom[2] - new Vector2(.25f, 0), bottom[3]);

            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block7");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");

            return m;
        }

        private static Mesh BuildSlopeHalfRamp(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            float widthHalf = width / 2;
            float heightHalf = height / 2;
            float depthHalf = depth / 2;

            mb.Begin();
            //front
            mb.AddTriangle(new Vector3(-widthHalf, (0), (depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), false);
            //left
            mb.AddQuad(new Vector3(-widthHalf, (0), -(depthHalf)), new Vector3(-widthHalf, (0), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            //back
            mb.AddTriangle(new Vector3(-widthHalf, (0), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), false);
            //right
            mb.AddQuad(new Vector3(-widthHalf, 0, (depthHalf)), new Vector3(-widthHalf, 0, -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), false);
            //bottom
            mb.AddQuad(new Vector3(-widthHalf, -(heightHalf), (depthHalf)), new Vector3(widthHalf, -(heightHalf), (depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            return mb.End();
        }

        private static Mesh BuildConcaveRamp(MeshBuilder mb, ContentManager c)
        {
            mb.Begin();
            //mb.AddQuad(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f), false);
            mb.AddQuad(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), false, left[0], left[1], left[2], left[3]);
            mb.AddQuad(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), false, bottom[0], bottom[1], bottom[2], bottom[3]);

            for (int i = 0; i < sphereIterations; i++)
            {
                //Draw the curved surface, and the two sides
                float angle = ((float)i / sphereIterations) * MathHelper.PiOver2 + MathHelper.Pi;
                float nextAngle = ((float)(i + 1) / sphereIterations) * MathHelper.PiOver2 + MathHelper.Pi;
                Vector3 offset = new Vector3(-0.5f, 0.5f, 0.5f);

                Vector3 v1 = new Vector3(0, (float)Math.Sin(angle), (float)Math.Cos(angle)) + offset;
                Vector3 v2 = new Vector3(1, (float)Math.Sin(angle), (float)Math.Cos(angle)) + offset;
                Vector3 v3 = new Vector3(1, (float)Math.Sin(nextAngle), (float)Math.Cos(nextAngle)) + offset;
                Vector3 v4 = new Vector3(0, (float)Math.Sin(nextAngle), (float)Math.Cos(nextAngle)) + offset;

                mb.AddQuad(v1, v2, v3, v4, true, new Vector2(0.5f, i * (0.5f / sphereIterations)), new Vector2(1, (i + 1) * (0.5f / sphereIterations)));

                //Draw the two sides
                v1 = new Vector3(1, -1, -1) + offset;
                v2 = new Vector3(1, (float)Math.Cos(angle), (float)Math.Sin(angle)) + offset;
                v3 = new Vector3(1, (float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) + offset;

                //calculate texture coordinates
                Vector2 v1Tex = new Vector2(v1.Z + .5f, -v1.Y + .5f);
                Vector2 v2Tex = new Vector2(v2.Z + .5f, -v2.Y + .5f);
                Vector2 v3Tex = new Vector2(v3.Z + .5f, -v3.Y + .5f);
                v1Tex /= 2;
                v2Tex /= 2;
                v3Tex /= 2;

                mb.AddTriangle(v1, v2, v3, new Vector2(v1Tex.X + .5f, v1Tex.Y + .5f), new Vector2(v2Tex.X + .5f, v2Tex.Y + .5f), new Vector2(v3Tex.X + .5f, v3Tex.Y + .5f), false);
                offset = new Vector3(-1, 0, 0);
                mb.AddTriangle(v1 + offset, v3 + offset, v2 + offset, new Vector2(v1Tex.X + .5f, v1Tex.Y + .5f), new Vector2(v3Tex.X + .5f, v3Tex.Y + .5f), new Vector2(v2Tex.X + .5f, v2Tex.Y + .5f), false);
            }

            mb.RotateAllVerts(Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2));

            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block9");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");

            return m;
        }

        private static Mesh BuildSlope2Base(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            float widthHalf = width / 2;
            float heightHalf = height / 2;
            float depthHalf = depth / 2;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(0, heightHalf, (depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), false, front[0], front[1] - new Vector2(.25f, 0), front[2], front[3]);
            //left
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false, left[0], left[1], left[2], left[3]);
            //back
            mb.AddQuad(new Vector3(0, heightHalf, -(depthHalf)), new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), false, front[1] - new Vector2(.25f, 0), front[0], front[3], front[2]);
            //right
            mb.AddQuad(new Vector3(0, heightHalf, (depthHalf)), new Vector3(0, heightHalf, -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), false, left[0], left[3], left[2], left[1]);
            //top
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(0, heightHalf, -(depthHalf)), new Vector3((0), heightHalf, (depthHalf)), new Vector3(-(widthHalf), (heightHalf), (depthHalf)), false, top[0], top[1] - new Vector2(.25f, 0), top[2] - new Vector2(.25f, 0), top[3]);
            //bottom
            mb.AddQuad(new Vector3(-widthHalf, -(heightHalf), (depthHalf)), new Vector3(widthHalf, -(heightHalf), (depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false, bottom[0], bottom[1], bottom[2], bottom[3]);

            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block10");
            m.NormalMap = c.Load<Texture2D>("Textures/BlockTextures/Block1N");

            return m;
        }

        private static Mesh BuildSlopeHalfBase(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            float widthHalf = width / 2;
            float heightHalf = height / 2;
            float depthHalf = depth / 2;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(widthHalf, 0, (depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), false);
            //left
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            //back
            mb.AddQuad(new Vector3(widthHalf, 0, -(depthHalf)), new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), false);
            //right
            mb.AddQuad(new Vector3(widthHalf, 0, (depthHalf)), new Vector3(widthHalf, 0, -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), false);
            //top
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(widthHalf, 0, -(depthHalf)), new Vector3((widthHalf), 0, (depthHalf)), new Vector3(-(widthHalf), (heightHalf), (depthHalf)), false);
            //bottom
            mb.AddQuad(new Vector3(-widthHalf, -(heightHalf), (depthHalf)), new Vector3(widthHalf, -(heightHalf), (depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            return mb.End();
        }

        /// <summary>
        /// Returns a block mesh of the requested type
        /// </summary>
        /// <param name="id">Type of block to return a mesh of</param>
        /// <returns>A block mesh of the requested type</returns>
        public static Mesh GetMeshFromID(byte id)
        {
            return blockMeshes[id];
        }
    }
}
