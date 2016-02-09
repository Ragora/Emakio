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
using System.Linq.Expressions;

using Emakio;

namespace Tests
{
    public class Thing : Emakio.StateSprite<Thing>
    {
        public bool IsWalking;
        public bool IsCrouching;
        public bool IsFalling;

        public enum Direction
        {
            Left,
            Right,
        }

        public Direction MoveDirection;

        public Thing(Game1 game) : base(game)
        {

        }
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private Emakio.Engine Engine;

        public bool IsWalking;
        public bool IsCrouching;
        public bool IsFalling;

        public enum Direction
        {
            Left,
            Right,
        }

        public Direction MoveDirection;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Engine = new Emakio.Engine(this);

            this.IsWalking = false;
            this.IsCrouching = false;
            this.IsFalling = true;
            this.MoveDirection = Direction.Right;

            Thing.StateSelector.Leaf leaf = null;
            Thing.StateSelector.Branch branch = null;
            Thing.StateSelector selector = new Thing.StateSelector();

            // The walking branch
            branch = new Thing.StateSelector.Branch((obj) => obj.IsWalking);
            selector.Top.Nodes.Add(branch);

            // Each leaf of the walking branch -- walking left and right
            leaf = new Thing.StateSelector.Leaf((obj) => obj.MoveDirection == Thing.Direction.Left, "walkleft");
            branch.Nodes.Add(leaf);
            leaf = new Thing.StateSelector.Leaf((obj) => obj.MoveDirection == Thing.Direction.Right, "walkright");
            branch.Nodes.Add(leaf);

            // Crouching?
            branch = new Thing.StateSelector.Branch((obj) => obj.IsCrouching);
            selector.Top.Nodes.Add(branch);

            leaf = new Thing.StateSelector.Leaf((obj) => obj.MoveDirection == Thing.Direction.Left, "crouchleft");
            branch.Nodes.Add(leaf);
            leaf = new Thing.StateSelector.Leaf((obj) => obj.MoveDirection == Thing.Direction.Right, "crouchright");
            branch.Nodes.Add(leaf);

            // Falling?
            branch = new Thing.StateSelector.Branch((obj) => obj.IsFalling);
            selector.Top.Nodes.Add(branch);

            leaf = new Thing.StateSelector.Leaf((obj) => obj.MoveDirection == Thing.Direction.Left, "fallleft");
            branch.Nodes.Add(leaf);
            leaf = new Thing.StateSelector.Leaf((obj) => obj.MoveDirection == Thing.Direction.Right, "fallright");
            branch.Nodes.Add(leaf);

            // Create a simple state sprite
            Thing sprite = new Thing(this);
            sprite.AnimationSelector = selector;

            // Print Test
            Console.WriteLine("Selected: {0}", selector.SelectAnimation(sprite));
            this.IsFalling = false;
            this.IsWalking = true;
            Console.WriteLine("Selected: {0}", selector.SelectAnimation(sprite));

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            Engine.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
          //  GraphicsDevice.Clear(Color.CornflowerBlue);

            Engine.Draw();

            base.Draw(gameTime);
        }
    }
}
