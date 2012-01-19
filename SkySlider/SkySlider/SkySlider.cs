using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using GraphicsToolkit;
using GraphicsToolkit.Input;
using GraphicsToolkit.GUI;
using SkySlider.Panels;

namespace SkySlider
{
    public class SkySlider : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        GUIManager gManager;

        public SkySlider()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Config.ScreenWidth = 1280;
            Config.ScreenHeight = 720;

            Components.Add(new InputHandler(this));

            gManager = new GUIManager(this, graphics);
            gManager.AddPanel(new MapViewerPanel());
            Components.Add(gManager);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (InputHandler.IsNewKeyPress(Keys.Escape))
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
