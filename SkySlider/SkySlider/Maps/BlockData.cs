using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GraphicsToolkit.Graphics;

namespace SkySlider.Maps
{
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
        private static int sphereIterations = 6;

        public static void BuildMeshes(GraphicsDevice g, ContentManager content)
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
        }

        private static Mesh BuildBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 1f;
            float depth = 1f;

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
            return mb.End();
        }

        private static Mesh BuildHalfBoxMesh(MeshBuilder mb, ContentManager c)
        {
            float width = 1f;
            float height = 0.5f;
            float depth = 1f;

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
            mb.OffsetAllVerts(new Vector3(0, -0.25f, 0)); //Offset the verts, because the above code centers the tile over the wrong position
            return mb.End();
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


        public static Mesh GetMeshFromID(byte id)
        {
            return blockMeshes[id];
        }
    }
}
