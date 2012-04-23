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
        private float airTime = 0.15f;
        private float currentAirTime = 0f;
        private bool canJump, falling, clinging;
        private Vector3 verticalJumpForce = new Vector3(0, 0.65f, 0);

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
            if ((sphereBody.InContact && (sphereBody.Normal.Y > 0.1f)) || clinging) //touching ground or clinging to wall
            {
                canJump = true;
                falling = false;
                currentAirTime = 0f;
            }
            else
            {
                canJump = false;
            }


            UpdateKeyPresses(g);
            cam.TargetPos = sphereBody.Pos;
            cam.Update(g);
            
        }

        private void UpdateKeyPresses(GameTime g)
        {
            Vector3 intendedDirection = Vector3.Zero; //used to determine if player is pushing against a wall
            float accelerationFactor;

            Vector3 damping = this.sphereBody.Vel * 0.002f;
            if (!InputHandler.IsKeyPressed(Keys.W) && !InputHandler.IsKeyPressed(Keys.S) && !InputHandler.IsKeyPressed(Keys.A) && !InputHandler.IsKeyPressed(Keys.D))
            {
                damping = this.sphereBody.Vel * 0.05f; //increase damping if no keys are pressed
            }

            damping *= new Vector3(1f, 0f, 1f); //damping should only be in the x and z directions

            if (sphereBody.InContact)
            {
                accelerationFactor = 1f;
            }
            else
            {
                accelerationFactor = 0.5f; //if character is in the air, reduce mobility
            }

            if (clinging)
            {
                accelerationFactor = 6f; //cannot move while clinging
                sphereBody.AddForce(-sphereBody.Normal * 0.01f);
            }

            if (InputHandler.IsKeyPressed(Keys.W))
            {
                intendedDirection += cam.Forward;
                Vector3 dir = Vector3.Cross(new Vector3(0f, 1f, 0f), Vector3.Cross(cam.Forward, new Vector3(0f, 1f, 0f)));
                dir.Normalize();
                sphereBody.AddForce(dir * acceleration * accelerationFactor);
            }

            if (InputHandler.IsKeyPressed(Keys.S))
            {
                intendedDirection -= cam.Forward;
                Vector3 dir = Vector3.Cross(new Vector3(0f, 1f, 0f), Vector3.Cross(cam.Forward, new Vector3(0f, 1f, 0f)));
                dir.Normalize();
                dir *= -1f;
                sphereBody.AddForce(dir * acceleration * accelerationFactor);
            }

            if (InputHandler.IsKeyPressed(Keys.D))
            {
                intendedDirection += cam.Right;
                Vector3 dir = cam.Right;
                dir.Normalize();
                sphereBody.AddForce(dir * acceleration * accelerationFactor);
            }

            if (InputHandler.IsKeyPressed(Keys.A))
            {
                intendedDirection -= cam.Right;
                Vector3 dir = cam.Right;
                dir.Normalize();
                dir *= -1f;
                sphereBody.AddForce(dir * acceleration * accelerationFactor);
            }

            if (!canJump) //extended jump
            {
                currentAirTime += (float)g.ElapsedGameTime.TotalSeconds;

                if (!falling && currentAirTime <= airTime && InputHandler.IsKeyPressed(Keys.Space)) 
                {
                    //continue to add force while space is held until currentAirTime reaches AirTime
                    sphereBody.AddForce(((airTime-currentAirTime)/airTime) * verticalJumpForce);
                }
                else
                {
                    falling = true;
                }
            }

            if (InputHandler.IsNewKeyPress(Keys.Space) && canJump && !falling) //initial jump
            {
                Vector3 dir = sphereBody.Normal;
                dir *= new Vector3(1f, 0, 1f);
                sphereBody.AddForce(Vector3.Dot(dir, intendedDirection) * dir * .8f + dir * 2f + verticalJumpForce);
                canJump = false;
                clinging = false;
            }

            if (((sphereBody.InContact) && (Math.Abs(sphereBody.Normal.X) + Math.Abs(sphereBody.Normal.Z) >= 0.97f)) || clinging)
            {
                intendedDirection.Normalize();
                if ((Math.Abs(Vector3.Dot(intendedDirection, sphereBody.Normal)) > 0.6f) || clinging) //player is pushing against wall
                {
                    if ((sphereBody.Vel.Y < -0.1) || clinging)//if moving downward, start clinging
                    {
                        clinging = true;
                        falling = false;
                        sphereBody.Vel = sphereBody.Vel * new Vector3(0, 0, 0);
                        sphereBody.AddForce(new Vector3(0, 0.01f, 0));
                    }
                    else //if moving upward, don't cling yet
                    {
                        clinging = false;
                    }
                }

                if ((Vector3.Dot(intendedDirection, sphereBody.Normal) > 0) || !sphereBody.InContact)
                {
                    //stop clinging is player is pushing away
                    clinging = false;
                }

            }


            sphereBody.AddForce(-damping);
        }

        public void givePoint()
        {
            this.score++;
        }
    }
}