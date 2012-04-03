﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicsToolkit.Physics._3D.Bodies;
using GraphicsToolkit.Physics._3D;
using Microsoft.Xna.Framework;

namespace SkySlider.Maps.BlockBodies
{
    public class ConcaveRampBody : RigidBody3D
    {
        public ConcaveRampBody()
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
                Vector3 pa = c.Pos; //point on this body closest to the sphere

                Vector2 XYVec = new Vector2(pa.X - 0.5f, pa.Y - 0.5f);
                if (XYVec.LengthSquared() < 1) //if inside bounding box but outside actual block
                {
                    pa.X = ((pa.X - 0.5f) / XYVec.Length()) + 0.5f;
                    pa.Y = ((pa.Y - 0.5f) / XYVec.Length()) + 0.5f;
                }

                pa.X = MathHelper.Clamp(pa.X, -0.5f, 0.5f);
                pa.Y = MathHelper.Clamp(pa.Y, -0.5f, 0.5f);
                pa.Z = MathHelper.Clamp(pa.Z, -0.5f, 0.5f);

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
