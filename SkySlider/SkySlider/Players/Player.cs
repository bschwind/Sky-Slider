using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphicsToolkit.Graphics;
using GraphicsToolkit.Input;
using GraphicsToolkit.Physics._3D.Bodies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SkySlider.Players
{
    public class Player
    {
        private ThirdPersonCamera cam;
        private SphereBody sphereBody;
        private float maxSpeed = 1.5f;
        private float acceleration = 0.1f;
        private int score = 0;

        public Player(Vector3 initialPlayerPosition)
        {
            cam = new ThirdPersonCamera(initialPlayerPosition, 1.4f, 1f);
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

            UpdateKeyPresses(g);
            cam.TargetPos = sphereBody.Pos;
            cam.Update(g);
            
        }

        private void UpdateKeyPresses(GameTime g)
        {
            float inAirAccelerationFactor;

            Vector3 damping = this.sphereBody.Vel * 0.002f;
            if (!InputHandler.IsKeyPressed(Keys.W) && !InputHandler.IsKeyPressed(Keys.S) && !InputHandler.IsKeyPressed(Keys.A) && !InputHandler.IsKeyPressed(Keys.D))
            {
                damping = this.sphereBody.Vel * 0.05f; //increase damping if no keys are pressed
            }
            damping *= new Vector3(1f, 0f, 1f); //damping should only be in the x and z directions

            if (sphereBody.InContact) 
            {
                inAirAccelerationFactor = 1f;
            }
            else
            {
                 inAirAccelerationFactor = 0.5f; //if character is in the air, reduce mobility
            }

            if (InputHandler.IsKeyPressed(Keys.W))
            {
                Vector3 dir = Vector3.Cross(new Vector3(0f, 1f, 0f), Vector3.Cross(cam.Forward, new Vector3(0f, 1f, 0f)));
                dir.Normalize();
                sphereBody.AddForce(dir * acceleration * inAirAccelerationFactor);
            }

            if (InputHandler.IsKeyPressed(Keys.S))
            {
                Vector3 dir = Vector3.Cross(new Vector3(0f, 1f, 0f), Vector3.Cross(cam.Forward, new Vector3(0f, 1f, 0f)));
                dir.Normalize();
                dir *= -1f;
                sphereBody.AddForce(dir * acceleration * inAirAccelerationFactor);
            }

            if (InputHandler.IsKeyPressed(Keys.D))
            {
                Vector3 dir = cam.Right;
                dir.Normalize();
                sphereBody.AddForce(dir * acceleration * inAirAccelerationFactor);
            }

            if (InputHandler.IsKeyPressed(Keys.A))
            {
                Vector3 dir = cam.Right;
                dir.Normalize();
                dir *= -1f;
                sphereBody.AddForce(dir * acceleration * inAirAccelerationFactor);
            }

            if (InputHandler.IsKeyPressed(Keys.Space) && sphereBody.InContact && (sphereBody.Normal.Y >= 0))
            {
                    Vector3 dir = sphereBody.Normal;
                    dir *= new Vector3(1f, 0, 1f);
                    sphereBody.AddForce(dir * 2f + new Vector3(0, 3f, 0));
            }

            sphereBody.AddForce(-damping);
        }

        public void givePoint()
        {
            this.score++;
        }
    }
}