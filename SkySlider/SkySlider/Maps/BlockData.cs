﻿using System;
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

        static Vector2 frontStart = new Vector2(.333f, .5f);
        static Vector2 frontEnd = new Vector2(.667f, 1f);

        static Vector2 rightStart = new Vector2(.667f, .5f);
        static Vector2 rightEnd = new Vector2(1, 1);

        static Vector2 leftStart = new Vector2(0, .5f);
        static Vector2 leftEnd = new Vector2(.333f, 1f);

        static Vector2 topStart = new Vector2(.333f, 0);
        static Vector2 topEnd = new Vector2(.667f, 0.5f);

        static Vector2 bottomStart = new Vector2(0, 0);
        static Vector2 bottomEnd = new Vector2(.333f, 0.5f);

        static Vector2 backStart = new Vector2(.667f, 0);
        static Vector2 backEnd = new Vector2(1, 0.5f);

        /// <summary>
        /// Uses MeshBuilder to create a mesh for each block type, storing them into an array
        /// </summary>
        /// <param name="g"></param>
        /// <param name="content"></param>
        public static void Initialize(GraphicsDevice g, ContentManager content)
        {
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
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false, frontStart, frontEnd);
            //left
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, leftStart, leftEnd);
            //back
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false, backStart, backEnd);
            //right
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false, rightStart, rightEnd);
            //top
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false, topStart, topEnd);
            //bottom
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, bottomStart, bottomEnd);

            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block1");

            return m;
        }

        private static Mesh BuildHalfBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 0.5f;
            float depth = 1f;

            frontStart.Y = (frontStart.Y - 1) / 2 + 1;
            frontEnd.Y = (frontEnd.Y - 1) / 2 + 1;
            leftStart.Y = (leftStart.Y - 1) / 2 + 1;
            leftEnd.Y = (leftEnd.Y - 1) / 2 + 1;
            backStart.Y = backStart.Y / 2 + 0.25f;
            backEnd.Y = backEnd.Y / 2 + 0.25f;
            rightStart.Y = (rightStart.Y - 1) / 2 + 1;
            rightEnd.Y = (frontEnd.Y - 1) / 2 + 1;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false, frontStart, frontEnd);
            //left
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, leftStart, leftEnd);
            //back
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false, backStart, backEnd);
            //right
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false, rightStart, rightEnd);
            //top
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false, topStart, topEnd);
            //bottom
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false, bottomStart, bottomEnd);
            mb.OffsetAllVerts(new Vector3(0, -0.25f, 0)); //Offset the verts, because the above code centers the tile over the wrong position
            
            Mesh m = mb.End();
            m.Texture = c.Load<Texture2D>("Textures/BlockTextures/Block2");

            return m;
        }

        private static Mesh BuildQuarterBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 0.5f;
            float depth = 0.5f;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false);
            //left
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            //back
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false);
            //right
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false);
            //top
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false);
            //bottom
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            mb.OffsetAllVerts(new Vector3(0, -0.25f, -0.25f)); //Offset the verts, because the above code centers the tile over the wrong position
            return mb.End();
        }

        private static Mesh BuildEighthBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 0.5f;
            float height = 0.5f;
            float depth = 0.5f;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false);
            //left
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            //back
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false);
            //right
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false);
            //top
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false);
            //bottom
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            mb.OffsetAllVerts(new Vector3(-0.25f, -0.25f, -0.25f)); //Offset the verts, because the above code centers the tile over the wrong position
            return mb.End();
        }

        private static Mesh BuildConvexRamp(MeshBuilder mb, ContentManager c)
        {
            mb.Begin();
            //mb.AddQuad(new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f), false);
            mb.AddQuad(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), false);
            mb.AddQuad(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), false);

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

                mb.AddQuad(v1, v2, v3, v4, true);

                //Draw the two sides
                v1 = new Vector3(1, 0, 0) + offset;
                v2 = new Vector3(1, (float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) + offset;
                v3 = new Vector3(1, (float)Math.Cos(angle), (float)Math.Sin(angle)) + offset;

                mb.AddTriangle(v1, v2, v3, false);
                offset = new Vector3(-1, 0, 0);
                mb.AddTriangle(v1+offset, v3+offset, v2+offset, false);
            }

            mb.RotateAllVerts(Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2));

            return mb.End();
        }

        private static Mesh BuildSlope1Ramp(MeshBuilder mb, ContentManager content)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            float widthHalf = width / 2;
            float heightHalf = height / 2;
            float depthHalf = depth / 2;

            mb.Begin();
            //front
            mb.AddTriangle(new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), false);
            //left
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            //back
            mb.AddTriangle( new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), false);
            //right
            mb.AddQuad(new Vector3(-widthHalf, heightHalf, (depthHalf)), new Vector3(-widthHalf, heightHalf, -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), false);
            //bottom
            mb.AddQuad(new Vector3(-widthHalf, -(heightHalf), (depthHalf)), new Vector3(widthHalf, -(heightHalf), (depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            return mb.End();
        }

        private static Mesh BuildSlope2Ramp(MeshBuilder mb, ContentManager content)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            float widthHalf = width / 2;
            float heightHalf = height / 2;
            float depthHalf = depth / 2;

            mb.Begin();
            //front
            mb.AddTriangle(new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3((0), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), false);
            //left
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            //back
            mb.AddTriangle(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), new Vector3((0), -(heightHalf), -(depthHalf)), false);
            //right
            mb.AddQuad(new Vector3(-widthHalf, heightHalf, (depthHalf)), new Vector3(-widthHalf, heightHalf, -(depthHalf)), new Vector3((0), -(heightHalf), -(depthHalf)), new Vector3((0), -(heightHalf), (depthHalf)), false);
            //bottom
            mb.AddQuad(new Vector3(-widthHalf, -(heightHalf), (depthHalf)), new Vector3(0, -(heightHalf), (depthHalf)), new Vector3((0), -(heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            return mb.End();
        }

        private static Mesh BuildSlopeHalfRamp(MeshBuilder mb, ContentManager content)
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
            mb.AddQuad(new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), false);
            mb.AddQuad(new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), false);

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

                mb.AddQuad(v1, v2, v3, v4, true);

                //Draw the two sides
                v1 = new Vector3(1, -1, -1) + offset;
                v2 = new Vector3(1, (float)Math.Cos(angle), (float)Math.Sin(angle)) + offset;
                v3 = new Vector3(1, (float)Math.Cos(nextAngle), (float)Math.Sin(nextAngle)) + offset;

                mb.AddTriangle(v1, v2, v3, false);
                offset = new Vector3(-1, 0, 0);
                mb.AddTriangle(v1 + offset, v3 + offset, v2 + offset, false);
            }

            mb.RotateAllVerts(Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.PiOver2));

            return mb.End();
        }

        private static Mesh BuildSlope2Base(MeshBuilder mb, ContentManager content)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

            float widthHalf = width / 2;
            float heightHalf = height / 2;
            float depthHalf = depth / 2;

            mb.Begin();
            //front
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(0, heightHalf, (depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), false);
            //left
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-widthHalf, (heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), (depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            //back
            mb.AddQuad(new Vector3(0, heightHalf, -(depthHalf)), new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), false);
            //right
            mb.AddQuad(new Vector3(0, heightHalf, (depthHalf)), new Vector3(0, heightHalf, -(depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3((widthHalf), -(heightHalf), (depthHalf)), false);
            //top
            mb.AddQuad(new Vector3(-widthHalf, (heightHalf), -(depthHalf)), new Vector3(0, heightHalf, -(depthHalf)), new Vector3((0), heightHalf, (depthHalf)), new Vector3(-(widthHalf), (heightHalf), (depthHalf)), false);
            //bottom
            mb.AddQuad(new Vector3(-widthHalf, -(heightHalf), (depthHalf)), new Vector3(widthHalf, -(heightHalf), (depthHalf)), new Vector3((widthHalf), -(heightHalf), -(depthHalf)), new Vector3(-(widthHalf), -(heightHalf), -(depthHalf)), false);
            return mb.End();
        }

        private static Mesh BuildSlopeHalfBase(MeshBuilder mb, ContentManager content)
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
