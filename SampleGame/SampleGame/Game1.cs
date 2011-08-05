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
using Omi.Xna.Collada.Model;

namespace SampleGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont font;

        float aspectRatio;
        Matrix world;
        Matrix view;
        Matrix projection;

        Vector3 pos = Vector3.Zero;
        Vector3 rot = Vector3.Zero;
        bool showHints = true;

        List<IModel> models = new List<IModel>();
        int selectedModel = 1;

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
            Window.AllowUserResizing = true;

            aspectRatio = (float)GraphicsDevice.Viewport.Width /
                (float)GraphicsDevice.Viewport.Height;

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio,
                1.0f, 10000.0f);

            view = Matrix.CreateLookAt(new Vector3(0, 0, -10), Vector3.Zero, Vector3.Up);            

            pos = new Vector3(0, -40, 100);
            rot = new Vector3(0, 0, 0);

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

            string[] modelPaths = { "3ds Max/Marcus/marcus_animated",
                "3ds Max/APC/APC_animation",                
                "3ds Max/Custom-Fx-Test/Custom-APC",
                "3ds Max/block-dude",
                "3ds Max/vertex-painted-box",
                "Maya/Seymour",
                "Mudbox/t-rex",
                "Mudbox/Monster/monster",
                "Spore/Bulldogtopus",
                "Spore/Toomer"
                 };            

            foreach (string path in modelPaths)
            {
                ModelData modelData = Content.Load<ModelData>(path);                

                IModel model = modelData.JointAnimations.Any() ? 
                    new SoftwareSkinnedModel(modelData) as IModel : 
                    new StaticModel(modelData) as IModel;

                models.Add(model);
            }

            font = Content.Load<SpriteFont>("Segoe UI Mono");
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

            if (models[selectedModel] is SoftwareSkinnedModel)
                (models[selectedModel] as SoftwareSkinnedModel).
                    PlayFirstAnimation(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            var keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Down))
                rot.X -= 0.015f;
            else if (keyboard.IsKeyDown(Keys.Up))
                rot.X += 0.015f;

            if (keyboard.IsKeyDown(Keys.Left))
                rot.Y += 0.015f;
            else if (keyboard.IsKeyDown(Keys.Right))
                rot.Y -= 0.020f;

            if (keyboard.IsKeyDown(Keys.S))
                pos.Z += 1f;
            else if (keyboard.IsKeyDown(Keys.W))
                pos.Z -= 1f;

            if (keyboard.IsKeyDown(Keys.A))
                pos.X += 1f;
            else if (keyboard.IsKeyDown(Keys.D))
                pos.X -= 1f;

            if (keyboard.IsKeyDown(Keys.PageUp))
                pos.Y += 1f;
            else if (keyboard.IsKeyDown(Keys.PageDown))
                pos.Y -= 1f;

            if (keyboard.IsKeyDown(Keys.F1))
                showHints = !showHints;

            for (int i = 0; i < 9; i++)
            {
                if (keyboard.IsKeyDown(Keys.D1 + i))
                {
                    if (models.Count > i)
                    {
                        selectedModel = i;
                        break;
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            world = Matrix.CreateFromYawPitchRoll(rot.Y, rot.X, rot.Z) * Matrix.CreateTranslation(pos);

            models[selectedModel].Draw(world, view, projection);

            if (showHints)
            {
                spriteBatch.Begin();

                spriteBatch.DrawString(font, "WASD - Move X/Z\nArrows - Rotate\nPgUp/Dn - Move Y\nDigits - Choose Model",
                    new Vector2(25, 25), Color.White);

                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
