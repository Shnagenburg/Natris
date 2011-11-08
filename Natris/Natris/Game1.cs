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

// TO DO: *extract all game logic/variables from primitive line so that it's more of a standalone primitives library (references to boxpixelsize, etc)
//        *there's way too much game logic in Game1.cs, build another class in between myGame and Board to handle game logic. myGame should really just handle input and call draw functions i think.
//        *fix canRotate so that it works on the edges. not sure how to do this yet. sigh. might have to make the gameboard have two extra rows and columns that are always 'filled'
//        *clean up some variable scope or something. 
//        *add a better game over condition
//        *optimize the fuck out of everything (especially drawing... redrawing the whole screen is not necessary)

namespace Natris
{


    public class myGame : Microsoft.Xna.Framework.Game
    {
        public static Color[] myColors = new Color[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet, Color.Black };

        enum PlayingStates { Playing, Freezing, Erasing }  //Freezing isn't used yet.

        public static GraphicsDeviceManager graphics;
        public static SpriteBatch spriteBatch;
        public static GraphicsDevice device;
        
        public static Texture2D pixel;
        public static Texture2D logo;

        public static Vector2 gameBoardOffset = new Vector2(16, 16);
        public static Vector2 logoOffset = new Vector2(390, 30);

        int playerScore = 0;
        Vector2 ScoreLabelPos = new Vector2(400, 140);
        Vector2 ScoreStringPos = new Vector2(530, 140);

        int playerLevel = 1;
        int linesCleared = 0;
        Vector2 LevelLabelPos = new Vector2(430, 235);
        Vector2 LevelStringPos = new Vector2(560, 235);

        Vector2 NextPieceStringPos = new Vector2(410, 420);
        public static Vector2 NextPiecePos = new Vector2(420, 450);

        SpriteFont UIMono24;
        SpriteFont UIMono18;

        IList<Rectangle> uiRects;

        public static int numPieces = 7;

        const float minTimeSinceLastInput = .1f;
        float timeSinceLastInput = 0f;

        float timeBetweenDrops = .5f;
        float timeSinceLastDrop = 0f;

        const float minTimeSinceLastRotate = .13f;
        float timeSinceLastRotate = 0f;

        const float lockingTime = .5f;
        float lockingTimer = 0f;

        const float erasingTime = .5f;
        float erasingTimer = 0f;
   
        PlayingStates PlayingState = PlayingStates.Playing;
        Queue<int> rowsToRemove;

        Board gameBoard;

        public myGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            gameBoard = new Board();

            device = GraphicsDevice;
     
            uiRects = new List<Rectangle>();
            uiRects.Add(new Rectangle((int)gameBoardOffset.X,(int)gameBoardOffset.Y,gameBoard.Width, gameBoard.Height)); //the outside of the game board
            uiRects.Add(new Rectangle(380, 20, 260, 60));
            uiRects.Add(new Rectangle(380, 100, 260, 260));
            uiRects.Add(new Rectangle(380, 400, 260, 260));
            //todo: add some more rectangles here to hold the scores and shit

            graphics.PreferredBackBufferWidth = 2* (int)gameBoardOffset.X + gameBoard.Width + 300;
            graphics.PreferredBackBufferHeight = 2* (int)gameBoardOffset.Y + gameBoard.Height;
            graphics.ApplyChanges();



            base.Initialize();

        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            pixel = Content.Load<Texture2D>("pixel");
            logo = Content.Load<Texture2D>("natris");

            UIMono18 = Content.Load<SpriteFont>("UIMono18");
            UIMono24 = Content.Load<SpriteFont>("UIMono24");
        }


        protected override void UnloadContent()
        {
    
        }


        protected override void Update(GameTime gameTime)
        {
            switch (PlayingState)
            {

                case PlayingStates.Playing:

                    timeSinceLastDrop += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timeSinceLastInput += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timeSinceLastRotate += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (timeSinceLastInput >= minTimeSinceLastInput)
                    {
                        if (Keyboard.GetState().IsKeyDown(Keys.Up))
                        {
                            if (timeSinceLastRotate >= minTimeSinceLastRotate)
                            {
                                gameBoard.currentPiece.rotatePiece();
                                timeSinceLastRotate = 0;
                            }
                        }
                        else if (Keyboard.GetState().IsKeyDown(Keys.Left))
                        {
                            if (gameBoard.currentPiece.CanMove(Directions.Left)) gameBoard.currentPiece.X -= 1;
                        }
                        else if (Keyboard.GetState().IsKeyDown(Keys.Right))
                        {
                            if (gameBoard.currentPiece.CanMove(Directions.Right)) gameBoard.currentPiece.X += 1;
                        }
                        else if (Keyboard.GetState().IsKeyDown(Keys.Down))
                        {
                            while (gameBoard.currentPiece.CanMove(Directions.Down)) { gameBoard.currentPiece.Y += 1; }
                        }
                        timeSinceLastInput = 0f;
                    }

                    if (timeSinceLastDrop >= timeBetweenDrops)
                    {
                        if (gameBoard.currentPiece.CanMove(Directions.Down)) gameBoard.currentPiece.Y += 1;
                        timeSinceLastDrop = 0f;
                    }


                    if (!gameBoard.currentPiece.CanMove(Directions.Down)) lockingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    
                    if (lockingTimer >= lockingTime)
                    {
                        gameBoard.currentPiece.lockPiece();
                        rowsToRemove = gameBoard.checkRowsForCompleteness();
                        linesCleared += rowsToRemove.Count;
                        if (rowsToRemove.Count > 0)
                        {
                            PlayingState = PlayingStates.Erasing;    
                        }
                        else
                        {
                            playerScore += 5 * playerLevel;
                            gameBoard.QueueNextPiece();
                        }
                        lockingTimer = 0;
                    }
                    break;

                case PlayingStates.Erasing:

                    erasingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (erasingTimer >= erasingTime)
                    {
                        rowsToRemove = gameBoard.checkRowsForCompleteness();

                        if (rowsToRemove.Count > 0)
                        {
                            gameBoard.removeRow(rowsToRemove.Dequeue());
                            playerScore += 15 * playerLevel;
                        }
                        else
                        {
                            PlayingState = PlayingStates.Playing;
                            if (linesCleared >= 10)
                            {
                                linesCleared = 0;
                                playerLevel++;
                                timeBetweenDrops -= .05f;
                            }
                            gameBoard.QueueNextPiece();
                        }
                        erasingTimer = 0f;                   
                    }
                    break;
            }

            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
                     
            spriteBatch.Begin(); // ALL DRAWING NEEDS TO GO BETWEEN HERE..

            spriteBatch.Draw(logo, logoOffset, Color.White);

            foreach (Rectangle thisRect in uiRects)
            {
                PrimitiveLine.CreateRectangle(thisRect,Color.White,spriteBatch,GraphicsDevice);
            }

            for (int y = 0; y < gameBoard.gridHeight; y++)
            {
                for (int x = 0; x < gameBoard.gridWidth; x++)
                {
                    if (Board.CellMap[x, y, 0] == 1)
                    {    
                        gameBoard.drawFilledSquare(x, y, myColors[Board.CellMap[x, y, 1]-1], spriteBatch,GraphicsDevice );
                    }
                }
            }

            if (PlayingState == PlayingStates.Playing)
            {
                gameBoard.currentPiece.draw(false);
            }

            gameBoard.nextPiece.draw(true);
            spriteBatch.DrawString(UIMono24, "Score:", ScoreLabelPos, Color.White);
            spriteBatch.DrawString(UIMono24, playerScore.ToString(), ScoreStringPos, Color.White);
            spriteBatch.DrawString(UIMono24, "Level:", LevelLabelPos, Color.White);
            spriteBatch.DrawString(UIMono24, playerLevel.ToString(), LevelStringPos, Color.White);
            spriteBatch.DrawString(UIMono24, "Next Piece:", NextPieceStringPos, Color.White);

            spriteBatch.End(); // ...AND HERE

            base.Draw(gameTime);
        }
    }
}
