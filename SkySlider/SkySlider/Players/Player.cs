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
        private CharacterController controller;
        private float maxSpeed = 1.5f;
        private float acceleration = 0.4f;

        public Player(Vector3 initialPlayerPosition)
        {
            cam = new ThirdPersonCamera(initialPlayerPosition, 1.4f, 1f);
            sphereBody = new SphereBody(initialPlayerPosition, Vector3.Zero, 1f, 0.18f);
            controller = new CharacterController(sphereBody);
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
            if (InputHandler.IsKeyPressed(Keys.W))
            {
                Vector3 dir = Vector3.Cross(new Vector3(0f, 1f, 0f), Vector3.Cross(cam.Forward, new Vector3(0f, 1f, 0f)));
                dir.Normalize();
                controller.AddForce(dir * acceleration);
                sphereBody.Vel = Vector3.Clamp(sphereBody.Vel, -new Vector3(maxSpeed, maxSpeed, maxSpeed), new Vector3(maxSpeed, maxSpeed, maxSpeed));
            }

            if (InputHandler.IsKeyPressed(Keys.S))
            {
                Vector3 dir = Vector3.Cross(new Vector3(0f, 1f, 0f), Vector3.Cross(cam.Forward, new Vector3(0f, 1f, 0f)));
                dir.Normalize();
                dir *= -1f;
                controller.AddForce(dir * acceleration);
                sphereBody.Vel = Vector3.Clamp(sphereBody.Vel, -new Vector3(maxSpeed, maxSpeed, maxSpeed), new Vector3(maxSpeed, maxSpeed, maxSpeed));
            }

            if (InputHandler.IsKeyPressed(Keys.D))
            {
                Vector3 dir = cam.Right;
                dir.Normalize();
                controller.AddForce(dir * acceleration);
                sphereBody.Vel = Vector3.Clamp(sphereBody.Vel, -new Vector3(maxSpeed, maxSpeed, maxSpeed), new Vector3(maxSpeed, maxSpeed, maxSpeed));
            }

            if (InputHandler.IsKeyPressed(Keys.A))
            {
                Vector3 dir = cam.Right;
                dir.Normalize();
                dir *= -1f;
                controller.AddForce(dir * acceleration);
                sphereBody.Vel = Vector3.Clamp(sphereBody.Vel, -new Vector3(maxSpeed, maxSpeed, maxSpeed), new Vector3(maxSpeed, maxSpeed, maxSpeed));
            }

            if (InputHandler.IsKeyPressed(Keys.Space))
            {
                controller.AddForce(new Vector3(0f, 1f, 0f) * acceleration);
                sphereBody.Vel = Vector3.Clamp(sphereBody.Vel, -new Vector3(maxSpeed, maxSpeed, maxSpeed), new Vector3(maxSpeed, maxSpeed, maxSpeed));
            }

        }
    }
}