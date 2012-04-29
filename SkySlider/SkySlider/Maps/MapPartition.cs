using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GraphicsToolkit.Physics._3D;
using GraphicsToolkit.Physics._3D.Partitions;
using GraphicsToolkit.Physics._3D.Bodies;
using GraphicsToolkit.Physics._3D.Geometry;

namespace SkySlider.Maps
{
    public class MapPartition : Partition3D
    {
        private Map map;

        public MapPartition(Map m)
        {
            map = m;
        }

        public override void GenerateContacts(ref List<RigidBody3D> bodies, ref List<Contact3D> contacts, float dt)
        {
            //Using the motion bounds, figure out all the cells the bodies are in (see grid partition)
            //Iterate over each of these cells, and generate contacts between the sphere and the block
            //Also generate contacts between the spheres and the blocks

            for (int i = 0; i < bodies.Count; i++)
            {
                AABB3D bounds = bodies[i].MotionBounds;
                Vector3 min = bounds.GetMin();
                Vector3 max = bounds.GetMax();

                int startRow = (int)min.X;
                startRow = (int)Math.Max(startRow, 0);

                int startCol = (int)min.Y;
                startCol = (int)Math.Max(startCol, 0);

                int startStack = (int)min.Z;
                startStack = (int)Math.Max(startStack, 0);

                int endRow = (int)max.X;
                endRow = (int)Math.Min(endRow, map.Width-1);

                int endCol = (int)max.Y;
                endCol = (int)Math.Min(endCol, map.Height-1);

                int endStack = (int)max.Z;
                endStack = (int)Math.Min(endStack, map.Depth-1);

                for (int j = startRow; j <= endRow; j++)
                {
                    for (int k = startCol; k <= endCol; k++)
                    {
                        for (int l = startStack; l <= endStack; l++)
                        {
                            Block b = map.GetBlockAt(j, k, l);
                            RigidBody3D blockBody = BlockData.GetBlockBody(b);
                            if (blockBody == null)
                            {
                                continue;
                            }

                            Matrix transform = BlockData.GetRotationMatrix(map.GetBlockAt(j, k, l)) * Matrix.CreateTranslation(new Vector3(j + 0.5f, k + 0.5f, l + 0.5f));
                            Matrix inverseTransform = Matrix.Invert(transform);

                            bodies[i].Pos = Vector3.Transform(bodies[i].Pos, inverseTransform);

                            Contact3D contact = blockBody.GenerateContact(bodies[i], dt);
                            contact.Normal = Vector3.TransformNormal(contact.Normal, transform);

                            contact.pointA = Vector3.Transform(contact.pointA, transform);
                            contact.pointB = Vector3.Transform(contact.pointB, transform);

                            bodies[i].Pos = Vector3.Transform(bodies[i].Pos, transform);

                            contacts.Add(contact);
                        }
                    }
                }
            }
        }

        //Returns the squared distance between point c and segment ab
        private float sqDistPointSegment(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 bc = c - b;
            float e = Vector3.Dot(ac, ab);

            if (e <= 0.0f)
            {
                return Vector3.Dot(ac, ac);
            }

            float f = Vector3.Dot(ab, ab);
            if (e >= f)
            {
                return Vector3.Dot(bc, bc);
            }

            return Vector3.Dot(ac, ac) - e * e / f;
        }
    }
}
