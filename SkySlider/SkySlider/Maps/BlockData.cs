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
        private static Mesh[] blockMeshes;

        public static void BuildMeshes(GraphicsDevice g, ContentManager content)
        {
            blockMeshes = new Mesh[5];
            blockMeshes[1] = BuildBoxMesh(g, content);
            blockMeshes[2] = BuildHalfBoxMesh(g, content);
            blockMeshes[3] = BuildQuarterBoxMesh(g, content);
        }

        private static Mesh BuildBoxMesh(GraphicsDevice g, ContentManager c)
        {
            MeshBuilder mb = new MeshBuilder(g);

            float width = 1f;
            float height = 1f;
            float depth = 1f;

            mb.Begin();
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false);
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            return mb.End();
        }
        
        //INCORRECT!
        private static Mesh BuildHalfBoxMesh(GraphicsDevice g, ContentManager c)
        {
            MeshBuilder mb = new MeshBuilder(g);

            float width = 1f;
            float height = 0.5f;
            float depth = 1f;

            mb.Begin();
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false);
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            return mb.End();
        }

        private static Mesh BuildQuarterBoxMesh(GraphicsDevice g, ContentManager c)
        {
            MeshBuilder mb = new MeshBuilder(g);

            float width = 1f;
            float height = 0.5f;
            float depth = 0.5f;

            mb.Begin();
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            mb.AddQuad(new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), false);
            mb.AddQuad(new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), -(depth / 2)), new Vector3((width / 2), (height / 2), (depth / 2)), new Vector3(-(width / 2), (height / 2), (depth / 2)), false);
            mb.AddQuad(new Vector3(-(width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), (depth / 2)), new Vector3((width / 2), -(height / 2), -(depth / 2)), new Vector3(-(width / 2), -(height / 2), -(depth / 2)), false);
            return mb.End();
        }

        public static Mesh GetMeshFromID(byte id)
        {
            return blockMeshes[id];
        }
    }
}
