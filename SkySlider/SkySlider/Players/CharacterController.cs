using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using GraphicsToolkit.Physics._3D.Controllers;
using GraphicsToolkit.Physics._3D.Bodies;
using GraphicsToolkit.Input;

namespace SkySlider.Players
{
    public class CharacterController : Controller3D
    {
        public CharacterController(RigidBody3D body)
            : base(body)
        {

        }

        public override void Update(float dt)
        {
            
        }


    }
}
