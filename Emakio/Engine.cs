﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Emakio
{
    public class Engine
    {
        private GraphicsDevice GraphicsDevice;
        private SpriteBatch SpriteBatch;

        public List<ITickable> Updated;
        public List<IDrawable> Drawn;

        public RenderTarget2D RenderTarget;

        public Color ClearColor;

        public Engine(Microsoft.Xna.Framework.Game game)
        {
            this.GraphicsDevice = game.GraphicsDevice;

            this.Updated = new List<ITickable>();
            this.Drawn = new List<IDrawable>();

            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);

            this.ClearColor = Color.CornflowerBlue;

            // Ensure that all the singleton systems are initialized
            SoundManager.Create(game);
            InputManager.Create();
            TimerManager.Create();
        }

        public void Draw()
        {
            this.GraphicsDevice.SetRenderTarget(this.RenderTarget);
            this.GraphicsDevice.Clear(this.ClearColor);

            SoundManager.Draw(this.SpriteBatch);

            foreach (IDrawable drawn in this.Drawn)
                drawn.Draw(this.SpriteBatch);

            this.GraphicsDevice.SetRenderTarget(null);
        }

        public void Update(GameTime time)
        {
            SoundManager.Update();
            InputManager.Update(time);

            foreach (ITickable updated in this.Updated)
                updated.Update(time);
        }
    }
}
