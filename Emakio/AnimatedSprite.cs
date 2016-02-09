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

namespace Emakio
{
    /// <summary>
    /// A class representing a basic algorithmic frame-advance sprite.
    /// </summary>
    public class AnimatedSprite : IDrawable, ITickable
    {
        /// <summary>
        /// The AnimationState class contains animation state information for a particular state that
        /// our sprite may be in.
        /// </summary>
        public class Animation
        {
            /// <summary>
            /// A delegate declaring a listener for the animation state starts and ends.
            /// </summary>
            public delegate void AnimationStartEndListener();

            /// <summary>
            /// What frame in the sprite sheet do we start at?
            /// </summary>
            public Point StartFrame;

            /// <summary>
            /// By what vector are we supposed to translate by when changing sprites?
            /// </summary>
            public Point Modifier;

            /// <summary>
            /// What is the size in pixels of each animation frame?
            /// </summary>
            public Point FrameSize;

            /// <summary>
            /// How many frames are there total?
            /// </summary>
            public int FrameCount;

            /// <summary>
            /// The sprite sheet to use.
            /// </summary>
            public Texture2D Sheet;

            /// <summary>
            /// Should we loop? If this is true, the state start and state end listeners are
            /// called in a looping fashion as well.
            /// </summary>
            public bool Looping;

            /// <summary>
            /// How many milliseconds to wait for each frame?
            /// </summary>
            public int MillisecondsPerFrame;

            /// <summary>
            /// A delegate function that is called when the animation state starts.
            /// </summary>
            public AnimationStartEndListener AnimationStartListener;

            /// <summary>
            /// A delegate function that is called when the animation state ends.
            /// </summary>
            public AnimationStartEndListener AnimationEndListener;

            public SpriteEffects Effects;

            /// <summary>
            /// A constructor accepting a sprite sheet, the start frame, the frame modifier, the frame size
            /// and the frame count.
            /// </summary>
            /// <param name="name">
            /// The name of the new animation state.
            /// </param>
            /// <param name="sheet">
            /// The sprite sheet associated with this animation state.
            /// </param>
            /// <param name="startFrame">
            /// The starting X,Y position animation frame in sprite frames.
            /// </param>
            /// <param name="modifier">
            /// The frame advance modifier in sprite frames.
            /// </param>
            /// <param name="frameSize">
            /// The size of a single frame in the animation.
            /// </param>
            /// <param name="frameCount">
            /// The number of frames in the animation.
            /// </param>
            public Animation(Texture2D sheet, Point startFrame, Point modifier, Point frameSize, int frameCount)
            {
                this.FrameCount = frameCount;
                this.Modifier = modifier;
                this.StartFrame = startFrame;
                this.FrameSize = frameSize;
                this.Sheet = sheet;
                this.Effects = SpriteEffects.None;
                this.MillisecondsPerFrame = 80;

                this.Looping = true;
            }
        };

        public SpriteEffects Effects { get; set; }

        /// <summary>
        /// Whether or not the animation should repeat once there is no frames
        /// left to play.
        /// </summary>
        public Boolean Repeat;

        /// <summary>
        /// The size of each frame in the sheet.
        /// </summary>
        public Point FrameSize;

        /// <summary>
        /// The position to be drawn at.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Whether or not this animated sprite should be drawn to the screen.
        /// </summary>
        public bool Drawn { get; set; }

        /// <summary>
        /// The scale factor.
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// The color to be drawn with.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The origin of the sprite. This is basically the draw offset relative to the top left corner of the image.
        /// </summary>
        public Vector2 Origin { get; set; }

        /// <summary>
        /// The rotation of the sprite.
        /// </summary>
        public float Theta { get; set; }

        /// <summary>
        /// Whether or not this animated sprite should be updated.
        /// </summary>
        public bool Updated { get; set; }

        /// <summary>
        /// The current X,Y coordinates in sprite frames to draw.
        /// </summary>
        public Point CurrentFrame;

        /// <summary>
        /// The game instance this animated sprite is associated with.
        /// </summary>
        protected Game InternalGame;

        /// <summary>
        /// The path to the texture to load and use as the sprite sheet.
        /// </summary>
        private String InternalTexturePath;

        /// <summary>
        /// The internally used texture for the sprite sheet.
        /// </summary>
        protected Texture2D InternalSheet;

        /// <summary>
        /// How much time has passed since the last frame advance.
        /// </summary>
        protected int TimeSinceLastFrame;

        /// <summary>
        /// The internally used sheet size that is represented in sprite frames.
        /// </summary>
        protected Point? SheetSize;

        /// <summary>
        /// The internally used rectangle for drawing individual frames out of the sprite sheet. This is calculated for each
        /// update that triggers a frame advance.
        /// </summary>
        protected Rectangle CurrentFrameRectangle;

        /// <summary>
        /// A read-only property that returns a rectangle representing the animated sprite's current collision bounds.
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Math.Floor(Position.X), (int)Math.Floor(Position.Y), FrameSize.X, FrameSize.Y);
            }
        }

        /// <summary>
        /// A read-only property that enforces that the sprite sheet should not be changed
        /// once the animated sprite is created.
        /// </summary>
        public Texture2D SpriteSheet
        {
            get
            {
                return this.InternalSheet;
            }
        }

        public int MillisecondsPerFrame { set; get; }

        /// <summary>
        /// A prot
        /// </summary>
        /// <param name="game"></param>
        protected AnimatedSprite(Game game)
        {
            this.Scale = 1.0f;
            this.Color = Color.White;
            this.Position = new Vector2(0, 0);
            CurrentFrame = new Point(0, 0);
            Repeat = true;

            InternalGame = game;

            Drawn = true;
            Updated = true;
        }

        /// <summary>
        /// A constructor accepting a game instance, a path to the sprite sheet, the size of the frames and
        /// the sprite arrangement on the sheet.
        /// </summary>
        /// <param name="game">
        /// The game to associate this animated sprite with.
        /// </param>
        /// <param name="texturePath">
        /// The path to the image to be used as the sprite sheet.
        /// </param>
        /// <param name="sizeOfFrames">
        /// The size of each frame to use.
        /// </param>
        /// <param name="sizeOfSheet">
        /// The size of the sheet represented in number of sprites.
        /// </param>
        public AnimatedSprite(Game game, String texturePath, Point sizeOfFrames, Point? sizeOfSheet)
        {
            this.Scale = 1.0f;
            this.Color = Color.White;
            this.Origin = new Vector2(0, 0);
            this.Position = new Vector2(0, 0);
            FrameSize = sizeOfFrames;
            CurrentFrame = new Point(0, 0);
            SheetSize = sizeOfSheet;
            Repeat = true;

            CurrentFrameRectangle = new Rectangle(CurrentFrame.X * FrameSize.X,
                                                    CurrentFrame.Y * FrameSize.Y,
                                                    FrameSize.X,
                                                    FrameSize.Y);

            this.MillisecondsPerFrame = 50;
            InternalGame = game;
            InternalTexturePath = texturePath;
        }

        /// <summary>
        /// Initializes the animated sprite by loading the sprite sheet.
        /// </summary>
        public virtual void Initialize()
        {
            this.InternalSheet = InternalGame.Content.Load<Texture2D>(InternalTexturePath);

            if (this.SheetSize == null)
                this.SheetSize = new Point(this.InternalSheet.Width / this.FrameSize.X, this.InternalSheet.Height / this.FrameSize.Y);
        }

        /// <summary>
        /// Updates the animated sprite by advancing the frames where necessary.
        /// </summary>
        /// <param name="time">
        /// The game time passed in by the game's main Update method.
        /// </param>
        public virtual void Update(GameTime time)
        {
            if (!Updated)
                return;

            TimeSinceLastFrame += time.ElapsedGameTime.Milliseconds;

            if (TimeSinceLastFrame >= MillisecondsPerFrame)
            {
                TimeSinceLastFrame -= MillisecondsPerFrame;

                ++CurrentFrame.X;

                Point sheetSize = (Point)SheetSize;
                if (CurrentFrame.X >= sheetSize.X)
                {
                    CurrentFrame.X = 0;
                    ++CurrentFrame.Y;

                    if (CurrentFrame.Y >= sheetSize.Y)
                    {
                        CurrentFrame.Y = 0;

                        if (!Repeat)
                            Updated = false;
                    }
                }

                CurrentFrameRectangle.X = CurrentFrame.X * FrameSize.X;
                CurrentFrameRectangle.Y = CurrentFrame.Y * FrameSize.Y;
            }
        }

        /// <summary>
        /// Draws the animated sprite to the screen buffer.
        /// </summary>
        /// <param name="batch">
        /// The sprite batch to draw to.
        /// </param>
        public virtual void Draw(SpriteBatch batch, Vector2? position = null)
        {
            if (!Drawn)
                return;

            if (position == null)
                position = Position;// + InternalGame.DrawOffset;

            batch.Draw(SpriteSheet,
                (Vector2)position,
                CurrentFrameRectangle,
                Color,
                Theta,
                Origin,
                Scale,
                Effects,
                0);
        }
    }
}
