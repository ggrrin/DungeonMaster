﻿using DungeonMasterEngine.GameConsoleContent.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DungeonMasterEngine.DungeonContent;
using DungeonMasterEngine.GameConsoleContent.Factories;

namespace DungeonMasterEngine.GameConsoleContent
{
    public class GameConsole : DrawableGameComponent
    {
        private int cursor = 0;
        private readonly StringBuilder line = new StringBuilder();
        public SpriteBatch Batcher { get; private set; }
        private KeyboardState prevKeyState = new KeyboardState();
        private KeyboardState keyState;
        private readonly KeyboardStream input;
        private readonly BaseInterpreter interpreter;
        public Texture2D WhiteTexture { get; private set; }
        private SpriteFont font;
        public TextWriter Out { get; }

        public bool Activated { get; set; }

        public TextReader In { get; }

        public Color BackgroundColor { get; set; } = new Color(new Vector4(0, 0, 0, 0.45f));

        private static readonly IEnumerable<ICommandFactory<ConsoleContext<Dungeon>>> defaultFactories =
        new ICommandFactory<ConsoleContext<Dungeon>>[] {
            HelpFactory.Instance,
            HandCommandFactory.Instance,
            ChampionCommandFactory.Instance,
            //ItemFactory.Instance,
            //TeleportFactory.Instance,
            SpellCommandFactory.Instance,
            FightCommandFactory.Instance
            //TODO add default factories
        };
        protected readonly ScreenStream output;

        public Rectangle Window { get; set; }

        public static void InitializeConsole(Game game, Dungeon dungeon, IEnumerable<ICommandFactory<ConsoleContext<Dungeon>>> thirdPartyFactories)
        {
            if (Instance != null)
                throw new InvalidOperationException($"Cannot create multiple instances of {Instance.GetType()}. Instead access instance through {nameof(Instance)}");

            Instance = new GameConsole(game, dungeon, thirdPartyFactories);
        }

        public static void InitializeConsole(Game game, Dungeon dungeon) => InitializeConsole(game, dungeon, new ICommandFactory<ConsoleContext<Dungeon>>[0]);

        private GameConsole(Game game, Dungeon dungeon, IEnumerable<ICommandFactory<ConsoleContext<Dungeon>>> thirdPartyFactories) : base(game)
        {
            In = new KeyboardStreamReader(input = new KeyboardStream());
            var s = new StreamWriter(output = new ScreenStream())
            { AutoFlush = true };
            Out = s;
            interpreter = new BaseInterpreter(defaultFactories.Concat(thirdPartyFactories), In, Out, input)
            { ConsoleContext = new ConsoleContext<Dungeon>(defaultFactories.Concat(thirdPartyFactories), dungeon) };
            new Action(async () => await interpreter.Run())();

            LoadResources();
            game.Components.Add(this);
        }

        public int CursorPosition
        {
            get
            {
                return cursor;
            }

            set
            {
                if (value >= 0 && value <= line.Length)
                    cursor = value;
            }
        }

        public static GameConsole Instance { get; private set; }

        public async void RunCommand(IInterpreter<ConsoleContext<Dungeon>> cmd)
        {
            interpreter.RunCommand(cmd);
            await Task.Delay(100); //delay because of input
            ActivateDeactivateConsole();
        }

        protected string lastCommand = "";

        private void ReadKeyBoard()
        {

            bool shift = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);
            //process letters
            foreach (var k in keyState.GetPressedKeys())
            {
                if (prevKeyState.IsKeyUp(k))
                {
                    int num = -1;
                    string c = Enum.GetName(k.GetType(), k);
                    if (c.Length == 1)
                    {
                        line.Insert(CursorPosition, shift ? c : c.ToLower()); //opposite cause letters are in uppercase
                        CursorPosition++;
                    }
                    else if (c.Length == 2 && c[0] == 'D' && int.TryParse(c[1].ToString(), out num))
                    {
                        line.Insert(CursorPosition, num);
                        CursorPosition++;
                    }
                }
            }

            if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
            {
                Out.WriteLine(line.ToString());
                input.WriteLineToInput(line.ToString());
                lastCommand = line.ToString();
                CursorPosition = 0;
                line.Clear();
            }
            else if (keyState.IsKeyDown(Keys.Back) && prevKeyState.IsKeyUp(Keys.Back))
            {
                int index = --CursorPosition;
                if (index >= 0)
                    line.Remove(index, 1);
            }
            else if (keyState.IsKeyDown(Keys.Delete) && prevKeyState.IsKeyUp(Keys.Delete))
            {
                if (CursorPosition < line.Length)
                    line.Remove(CursorPosition, 1);
            }
            else if (keyState.IsKeyDown(Keys.Left) && prevKeyState.IsKeyUp(Keys.Left))
                CursorPosition--;
            else if (keyState.IsKeyDown(Keys.Right) && prevKeyState.IsKeyUp(Keys.Right))
                CursorPosition++;
            else if (keyState.IsKeyDown(Keys.Space) && prevKeyState.IsKeyUp(Keys.Space))
            {
                line.Insert(CursorPosition, ' ');
                CursorPosition++;
            }
            else if (line.Length == 0 && keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))
            {
                line.Append(lastCommand);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            keyState = Keyboard.GetState();

            if (Activated)
                ReadKeyBoard();

            if (keyState.IsKeyDown(Keys.Tab) && prevKeyState.IsKeyUp(Keys.Tab))
                ActivateDeactivateConsole();

            prevKeyState = keyState;
        }

        private void ActivateDeactivateConsole()
        {
            if (Activated && interpreter.ExecutingCommand && !interpreter.RunningCommand.CanRunBackground)
            {
                Out.WriteLine("Cannot close until command is done!!!");
                return;
            }


            Activated ^= true;
            interpreter.ConsoleContext.AppContext.Enabled = !Activated;
            if (Activated)
                SetWindowSize(0.8f);
            else
                SetWindowSize(0.2f);
        }

        private void LoadResources()
        {
            Batcher = new SpriteBatch(Game.GraphicsDevice);
            WhiteTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
            WhiteTexture.SetData(new Color[] { Color.White });
            font = Game.Content.Load<SpriteFont>("Fonts/Default");

            SetWindowSize(0.2f);
        }

        private void SetWindowSize(float v)
        {
            var viewport = Game.GraphicsDevice.Viewport;
            int height = (int)(viewport.Height * v);
            Window = new Rectangle(0, viewport.Height - height, viewport.Width, height);
        }

        public override void Draw(GameTime gameTime)
        {

            Batcher.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            //background
            Batcher.Draw(WhiteTexture, Window, BackgroundColor);
            //cursor
            int textY = Window.Y + Window.Height - font.LineSpacing;
            if (Activated)
                Batcher.Draw(WhiteTexture, new Rectangle(Window.X + (int)font.MeasureString(line.ToString().Substring(0, CursorPosition)).X, textY, 2, font.LineSpacing), Color.White);
            //command line
            Batcher.DrawString(font, line, new Vector2(Window.X, textY), Color.White);
            //output
            foreach (var outputLine in output.Lines.Reverse())
            {
                textY -= font.LineSpacing;

                if (textY < Window.Y)
                    break;
                Batcher.DrawString(font, outputLine, new Vector2(Window.X, textY), Color.White);
            }

            Batcher.End();

            base.Draw(gameTime);
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                In.Dispose();
                WhiteTexture.Dispose();
            }
        }
    }

}
