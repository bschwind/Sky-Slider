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
        //0 = box
        //1 = half box
        private static Dictionary<byte, Mesh> blockMeshes;

        public static void BuildMeshes(GraphicsDevice g, ContentManager content)
        {
            blockMeshes = new Dictionary<byte, Mesh>();
            blockMeshes[0] = BuildBoxMesh(g, content);
        }

        private static Mesh BuildBoxMesh(GraphicsDevice g, ContentManager c)
        {
            Mesh m = new Mesh();    

            return m;
        }

        public static Mesh GetMeshFromID(byte id)
        {
            return new Mesh();
        }
    }
}
