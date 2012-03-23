using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicsToolkit.Physics._3D.Bodies;
using GraphicsToolkit.Physics._3D;
using Microsoft.Xna.Framework;

namespace SkySlider.Maps.BlockBodies
{
    public class BoxBody : RigidBody3D
    {
        private float sizeHalf; //half of the box's size

        public float SizeHalf
        {
            get
            {
                return sizeHalf;
            }
        }

        public BoxBody(Vector3 pos, Vector3 vel, float mass, float size)
            : base(pos, vel, 0f, mass, 1f)
        {
            this.sizeHalf = size/2;
        }

        public override void GenerateMotionAABB(float dt)
        {
            throw new NotImplementedException();
        }

        public override GraphicsToolkit.Physics._3D.Contact3D GenerateContact(RigidBody3D rb, float dt)
        {
            if (rb as SphereBody != null)
            {
                SphereBody c = rb as SphereBody; //sphere that this body is colliding with
                Vector3 pa; //point on this body closest to the sphere

                //find x coord of pa
                if (c.Pos.X < this.Pos.X - sizeHalf)
                {
                    pa.X = this.Pos.X - sizeHalf;
                }
                else if (c.Pos.X > this.Pos.X + sizeHalf)
                {
                    pa.X = this.Pos.X + sizeHalf;
                }
                else
                {
                    pa.X = c.Pos.X;
                }
                //find y coord of pa
                if (c.Pos.Y < this.Pos.Y - sizeHalf)
                {
                    pa.Y = this.Pos.Y - sizeHalf;
                }
                else if (c.Pos.Y > this.Pos.Y + sizeHalf)
                {
                    pa.Y = this.Pos.Y + sizeHalf;
                }
                else
                {
                    pa.Y = c.Pos.Y;
                }
                //find z coord of pa
                if (c.Pos.Z < this.Pos.Z - sizeHalf)
                {
                    pa.Z = this.Pos.Z - sizeHalf;
                }
                else if (c.Pos.Z > this.Pos.Z + sizeHalf)
                {
                    pa.Z = this.Pos.Z + sizeHalf;
                }
                else
                {
                    pa.Z = c.Pos.Z;
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
