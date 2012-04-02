using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicsToolkit.Graphics;
using GraphicsToolkit.Physics._3D.Bodies;
using Microsoft.Xna.Framework;

namespace SkySlider
{
    public class Player
    {
        private ThirdPersonCamera cam;
        private SphereBody sphereBody;

        public Player(Vector3 initialPlayerPosition)
        {
            cam = new ThirdPersonCamera(initialPlayerPosition, 0.7f, 1f);
            sphereBody = new SphereBody(initialPlayerPosition, Vector3.Zero, 1f, 0.18f);
        }

        public ThirdPersonCamera Cam
        {
            get
            {
                return cam;
            }
        }

        public SphereBody Body
        {
            get
            {
                return sphereBody;
            }
        }

        public void Update(GameTime g)
        {
            cam.TargetPos = sphereBody.Pos;
            cam.Update(g);
        }
    }
}