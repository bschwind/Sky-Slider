using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicsToolkit.Physics._3D.Bodies;
using GraphicsToolkit.Physics._3D;
using Microsoft.Xna.Framework;

namespace SkySlider.Maps.BlockBodies
{
    public class SlopeHalfRampBody : RigidBody3D
    {
        public SlopeHalfRampBody()
            : base(Vector3.Zero, Vector3.Zero, 0f, 0f, 1f)
        {

        }

        public override void GenerateMotionAABB(float dt)
        {

        }

        public override GraphicsToolkit.Physics._3D.Contact3D GenerateContact(RigidBody3D rb, float dt)
        {
            if (rb as SphereBody != null)
            {
                SphereBody c = rb as SphereBody; //sphere that this body is colliding with
                Vector3 pa = new Vector3();
                float slope = -0.5f; //y = -0.5x - 0.25

                if (c.Pos.Y  < ((slope * c.Pos.X) - 0.25f)) //if the point is under the line
                {
                    pa.X = MathHelper.Clamp(c.Pos.X, -0.5f, 0.5f);
                    pa.Y = MathHelper.Clamp(c.Pos.Y, -0.5f, 0.0f);
                    pa.Z = MathHelper.Clamp(c.Pos.Z, -0.5f, 0.5f);
                }
                else
                {
                    pa.X = ((c.Pos.X / slope) + 0.25f + c.Pos.Y) / (slope + (1 / slope));
                    pa.Y = (-1f / slope) * (pa.X - c.Pos.X) + c.Pos.Y;
                    pa.Z = c.Pos.Z;

                    if (pa.X > 0.5f)
                    {
                        pa.X = 0.5f;
                        pa.Y = -0.5f;
                    }

                    if (pa.Y > 0f)
                    {
                        pa.X = -0.5f;
                        pa.Y = 0.0f;
                    }
                    pa.Z = MathHelper.Clamp(pa.Z, -0.5f, 0.5f);
                }


                Vector3 normal = rb.Pos - pa;
                float normLen = normal.Length();
                float dist = normLen - c.Radius; //distance from block to sphere
                normal /= normLen; //normalize normal
                Vector3 pb = rb.Pos - normal * c.Radius; //closest point on sphere

                return new Contact3D(normal, dist, this, rb, pa, pb);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
