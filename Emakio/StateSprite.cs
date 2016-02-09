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
    /// The StateSprite class is an animated sprite whose actual animation
    /// contents are controlled via named states. Each state can contain
    /// its own sprite sheet, frame size, frame count, etc. Each state may
    /// also have listeners attached to its start and end points. State selection
    /// logic is implementing using a recursive branch, leaf system where the
    /// branches and leafs contain an expression to be evaluated which influences
    /// the decisions made by the animation picker.
    /// </summary>
    public class StateSprite<T> : AnimatedSprite
    {
        /// <summary>
        /// For use with the StateSprite. The StateSelector is a class with which you can
        /// represent arbitrary state machines with a hierarchy of lambda expressions and/or delegate
        /// methods that either return a result or branch the animation state system.
        /// 
        /// A textual representation of a basic utilization might look like:
        /// TopLevel
        ///     IsFalling
        ///         FacingDirection == Left: "FallLeft"
        ///         FacingDirection == Right: "FallRight"
        ///     IsWalking
        ///         FacingDirection == Left: "WalkLeft"
        ///         FacingDirection == Right: "WalkRight"
        ///     IsCrouching
        ///         FacingDirection == Left: "CrouchLeft"
        ///         FacingDirection == Right: "CrouchRight"
        /// </summary>
        public class StateSelector
        {
            public Dictionary<String, State> States;

            public abstract class ANode
            {
                public delegate bool EvaluatorMethod(T obj);

                protected EvaluatorMethod Evaluator;

                public ANode(EvaluatorMethod eval)
                {
                    Evaluator = eval;
                }
            }

            public class Branch : ANode
            {
                public List<ANode> Nodes;

                public Branch(EvaluatorMethod eval) : base(eval)
                {
                    this.Nodes = new List<ANode>();
                }

                public Branch AddBranch(EvaluatorMethod eval)
                {
                    Branch result = new Branch(eval);
                    this.Nodes.Add(result);
                    return result;
                }

                public State AddState(EvaluatorMethod eval, string name)
                {
                    State result = new State(eval, name);
                    this.Nodes.Add(result);
                    return result;
                }

                public State Evaluate(object obj)
                {
                    T target = (T)obj;

                    if (!this.Evaluator(target))
                        return null;

                    foreach (ANode node in this.Nodes)
                    {
                        if (node is Branch)
                        {
                            Branch branch = (Branch)node;
                            State result = branch.Evaluate(obj);

                            if (result != null)
                                return result;
                        }
                        else
                        {
                            State leaf = (State)node;

                            if (leaf.Evaluate(obj))
                                return leaf;
                        }
                    }

                    return null;
                }
            }

            public class State : ANode
            {
                public string Name;

                /// <summary>
                /// A list of transitions that this animation state cannot transition to.
                /// </summary>
                public List<String> IncompatibleTransitions;

                /// <summary>
                /// A boolean representing whether or not the state sprite should lock state transitions for the
                /// duration of this state.
                /// </summary>
                public bool Lock;

                public AnimatedSprite.Animation Animation;

                public State(EvaluatorMethod eval, string name) : base(eval)
                {
                    this.Name = name;
                }

                public bool Evaluate(object obj)
                {
                    T result = (T)obj;
                    return this.Evaluator(result);
                }
            }

            public Branch Top;

            public StateSelector()
            {
                this.Top = new Branch((sprite) => true);
            }

            public State SelectState(object obj)
            {
                State result = Top.Evaluate(obj);
                return result;
            }
        }
    
        /// <summary>
        /// The current frame in our current animation state to use.
        /// </summary>
        public int CurrentFrameIdentifier;

        /// <summary>
        /// The currently state animation state.
        /// </summary>
        protected StateSelector.State CurrentState;
        
        /// <summary>
        /// A dictionary mapping the shorthand state names to their respective animation state information.
        /// </summary>
        private Dictionary<String, Animation> AnimationStates;

        public StateSelector Selector;

        /// <summary>
        /// A constructor accepting a game object.
        /// </summary>
        /// <param name="game">
        /// The game instance to associate this state sprite with.
        /// </param>
        public StateSprite(Game game) : base(game)
        {

        }

        /// <summary>
        /// Initializes the state sprite by creating its animation state dictionary.
        /// </summary>
        public override void Initialize()
        {
            this.AnimationStates = new Dictionary<String, Animation>();
        }

        /// <summary>
        /// Resets the currently active animation to its beginning.
        /// </summary>
        public void ResetAnimation()
        {
            this.CurrentFrameIdentifier = 0;
        }

        /// <summary>
        /// Registers a new animation state with the given information. If a state already exists by the given name, the existing state
        /// is simply returned.If not, a new one is created under that name.
        /// </summary>
        /// <param name="sheet">
        /// The sprite sheet to use for this state.
        /// </param>
        /// <param name="name">
        /// The name of the animation state.
        /// </param>
        /// <param name="startFrame">
        /// The X,Y of the start frame in sprite frames.
        /// </param>
        /// <param name="modifier">
        /// The frame advance delta in sprite frames.
        /// </param>
        /// <param name="frameSize">
        /// The size of an individual frame in the animation.
        /// </param>
        /// <param name="frameCount">
        /// How many frames there are in the animation.
        /// </param>
        /// <returns>
        /// The newly created animation state. If there is already an animation state with the given name, that 
        /// one is simply returned.
        /// </returns>
        /*
        public Animation AddAnimationState(Texture2D sheet, String name, Point startFrame, Point modifier, Point frameSize, int frameCount)
        {
            name = name.ToLower();

            Animation result = GetAnimationState(name);
            if (result == null)
            {
                result = new Animation(name, sheet, startFrame, modifier, frameSize, frameCount);
                AnimationStates[name] = result;
            }

            return result;
        }
        */

        /// <summary>
        /// Gets an existing animation state. If there is not an animation state under the given name, null is returned.
        /// </summary>
        /// <param name="name">
        /// The name of the animation state to get.
        /// </param>
        /// <returns>
        /// The animation state with the given name. This is null if there is no state by that name.
        /// </returns>
        public Animation GetAnimationState(String name)
        {
            name = name.ToLower();

            if (!this.AnimationStates.ContainsKey(name))
                return null;

            return this.AnimationStates[name];
        }

        /// <summary>
        /// A read-only property representing whether or not the state sprite's current animation state is complete.
        /// </summary>
        public bool StateComplete
        {
            get
            {
                if (this.CurrentState == null)
                    return true;

                return this.CurrentFrameIdentifier >= this.CurrentState.Animation.FrameCount && this.TimeSinceLastFrame >= this.MillisecondsPerFrame;
            }
        }

        /// <summary>
        /// Sets the current animation state. If the name is null or "none", the state is set to nothing. If
        /// The given animation state otherwise doesn't exist, a no-op occurs.
        /// </summary>
        /// <param name="name">
        /// The name of the animation state to use.
        /// </param>
        public void SetAnimationState(String name)
        {
            if (name == null || name == "none")
            {
                this.CurrentState = null;
                return;
            }

            name = name.ToLower();
            if (!this.AnimationStates.ContainsKey(name) || this.CurrentState.Animation == AnimationStates[name])
                return;

            if ((!this.StateComplete && this.CurrentState.Lock) || (this.CurrentState != null && this.CurrentState.IncompatibleTransitions.Contains(name)))
                return;

            this.CurrentState = this.Selector.States[name];

            // Now that we've selected a state, grab its animation data
            this.CurrentFrame = CurrentState.Animation.StartFrame;
            this.InternalSheet = CurrentState.Animation.Sheet;
            this.FrameSize = CurrentState.Animation.FrameSize;
            this.MillisecondsPerFrame = CurrentState.Animation.MillisecondsPerFrame;

            this.CurrentFrameRectangle.X = CurrentFrame.X * FrameSize.X;
            this.CurrentFrameRectangle.Y = CurrentFrame.Y * FrameSize.Y;
            this.CurrentFrameRectangle.Width = FrameSize.X;
            this.CurrentFrameRectangle.Height = FrameSize.Y;
            this.Effects = CurrentState.Animation.Effects;

            if (this.CurrentState.Animation.AnimationStartListener != null)
                this.CurrentState.Animation.AnimationStartListener();
        }

        /// <summary>
        /// Updates the state sprite.
        /// </summary>
        /// <param name="time">
        /// The GameTime passed into the game's main Update method.
        /// </param>
        public override void Update(GameTime time)
        {
            if (!this.Updated || this.Selector == null)
                return;

            // Perform Selection Logic
            if (this.CurrentState != null && !this.CurrentState.Lock)
               this.SetAnimationState(this.Selector.SelectState(this).Name);

            // Don't do anything if we don't have an animation state set.
            if (this.CurrentState == null)
                return;

            this.TimeSinceLastFrame += time.ElapsedGameTime.Milliseconds;

            if (this.TimeSinceLastFrame >= this.MillisecondsPerFrame)
            {
                if (!this.CurrentState.Lock)
                    this.TimeSinceLastFrame -= this.MillisecondsPerFrame;

                if (this.CurrentFrameIdentifier >= CurrentState.Animation.FrameCount)
                {
                    if (this.CurrentState.Animation.AnimationEndListener != null)
                        this.CurrentState.Animation.AnimationEndListener();

                    if (this.CurrentState.Animation.Looping)
                    {
                        this.CurrentFrameIdentifier = 0;
                        this.CurrentFrame = CurrentState.Animation.StartFrame;

                        if (this.CurrentState.Animation.AnimationStartListener != null)
                            this.CurrentState.Animation.AnimationStartListener();
                    }
                    else
                        return;
                }
                else
                    CurrentFrame = new Point(this.CurrentFrame.X + this.CurrentState.Animation.Modifier.X, 
                        this.CurrentFrame.Y + this.CurrentState.Animation.Modifier.Y);

                ++this.CurrentFrameIdentifier;

                if (!Repeat)
                    this.Updated = false;

                this.CurrentFrameRectangle.X = this.CurrentFrame.X * this.CurrentState.Animation.FrameSize.X;
                this.CurrentFrameRectangle.Y = this.CurrentFrame.Y * this.CurrentState.Animation.FrameSize.Y;
                this.CurrentFrameRectangle.Width = this.CurrentState.Animation.FrameSize.X;
                this.CurrentFrameRectangle.Height = this.CurrentState.Animation.FrameSize.Y;
            }
        }
    }
}
