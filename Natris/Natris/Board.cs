using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna;
using Microsoft.Xna.Framework.Graphics;


namespace Natris
{
    enum pieceColors { Red, Orange, Yellow, Green, Blue, Indigo, Violet, Black };

    class Board
    {
        public int gridWidth = 10;
        public int gridHeight = 20;

        const int block=0;
        const int color=1;       

        public static int[,,] CellMap;

        public BasePiece currentPiece;
        public BasePiece nextPiece;

        public static int borderpadding = 2;
        public static int innerpadding = 1;
        public static int boxPixelSize = 32;


        public int Width;
        public int Height;


        //initialize the board
        public Board()
        {
           CellMap = new int[gridWidth, gridHeight, 2]; //depth of 2: [,,0] refers to whether the object is there, [,,1] is the color

           ClearBoard();     
           currentPiece = GetNewRandomPiece();
           nextPiece = GetNewRandomPiece();
           Width = gridWidth * boxPixelSize + 2 * borderpadding + ((gridWidth + 1) * innerpadding);
           Height = gridHeight * boxPixelSize + 2 * borderpadding + ((gridHeight + 1) * innerpadding);

        }

        //clear the board
        void ClearBoard()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    CellMap[x, y, block] = 0;
                    CellMap[x, y, color] = (int)pieceColors.Black;
                }
            }
        }

        public void QueueNextPiece()
        {
            currentPiece = nextPiece;
            nextPiece = GetNewRandomPiece();
        }

        BasePiece GetNewRandomPiece()
        {
            Random random = new Random();
            int randomNumber = random.Next(1, myGame.numPieces);

            //this switch thing pisses me off.
            switch (randomNumber)
            {
                case 1:
                    return new IPiece();
                case 2:
                    return new JPiece();
                case 3:
                    return new LPiece();
                case 4:
                    return new OPiece();
                case 5:
                    return new SPiece();
                case 6:
                    return new TPiece();
                case 7:
                    return new ZPiece();
            }
            return null;


            /* Is this any better? Shrug..
 
            IList<BasePiece> PieceList = new List<BasePiece>;
            PieceList.Add(new IPiece());
            PieceList.Add(new JPiece());
            PieceList.Add(new LPiece());
            PieceList.Add(new OPiece());
            PieceList.Add(new SPiece());
            PieceList.Add(new TPiece());
            PieceList.Add(new ZPiece());

            return PieceList.ElementAt(randomNumber);
            */
        }

        public void drawFilledSquare(int X, int Y, Color myColor, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            int x = X;
            int y = Y;
            int xRect = (int)myGame.gameBoardOffset.X + borderpadding + innerpadding*x + boxPixelSize*x ;
            int yRect = (int)myGame.gameBoardOffset.Y + borderpadding + innerpadding*y + boxPixelSize * y;

            Rectangle myRect = new Rectangle(xRect, yRect, boxPixelSize, boxPixelSize);
            PrimitiveLine.CreateFilledRect(myRect, myColor, spriteBatch, graphicsDevice);
        }


        public Queue<int> checkRowsForCompleteness()
        {
            Queue<int> rowsToErase = new Queue<int>();
            
            for (int row = 19; row >= 0; row--)
            {
                bool isTrue = true;
                for (int x = 0; x < 10; x++)
                {
                    if (CellMap[x, row, 0] == 0) isTrue = false;
                }
                if (isTrue == true) rowsToErase.Enqueue(row);
            }

            return rowsToErase;

        }


        public void removeRow(int row)
        {
            int i = 0;
            for (int y = row; y > 0; y--)
            {
                for (int x = 0; x < 10; x++)
                {
                    CellMap[x, y, 0] = CellMap[x, y - 1, 0];
                    CellMap[x, y, 1] = CellMap[x, y - 1, 1];
                }
            }
            /*
            for (int x = 0; x < 10; x++)
            {
                CellMap[x, 0, 0] = 0;
                CellMap[x, 0, 1] = 7;
            }
             */
        }
    }
}
