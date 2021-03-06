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

namespace Emakio.GUI.Elements
{
    /// <summary>
    /// Base class for all UI elements to be drawn by the GUI manager.
    /// </summary>
    public abstract class AElement
    {
        /// <summary>
        /// The position for the element to be drawn at.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The color to draw the element with.
        /// </summary>
        public Color Color;
        public Vector2 Origin;

        /// <summary>
        /// The scale factor of the element.
        /// </summary>
        public float Scale;

        /// <summary>
        /// The rotation angle of the element.
        /// </summary>
        public float Theta;

        /// <summary>
        /// The game instance this element is associated with.
        /// </summary>
        protected Game InternalGame;

        /// <summary>
        /// A constructor accepting a game instance.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this UI element with.
        /// </param>
        public AElement(Game game)
        {
            Position = new Vector2();
            Color = Color.White;
            Origin = new Vector2();
            Scale = 1.0f;
            InternalGame = game;
        }

        /// <summary>
        /// A base initialize method that should be overwritten by children classes.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// A base update method that should be overwritten by children classes.
        /// </summary>
        /// <param name="time">
        /// The GameTime object passed in by the game's main Update method.
        /// </param>
        public abstract void Update(GameTime time);

        /// <summary>
        /// A base draw method that should be overwritten by children classes.
        /// </summary>
        /// <param name="batch">
        /// The sprite batch to draw to.
        /// </param>
        public abstract void Draw(SpriteBatch batch);
    }
}
