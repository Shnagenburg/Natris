using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Natris
{
    public class OPiece : BasePiece
    {
        public OPiece()
        {
            
            
            rotationList.Add(new int[,] {  {0,0,0,0},
                                           {0,1,1,0},
                                           {0,1,1,0},
                                           {0,0,0,0}  });

            pieceColor = Color.Green;
            base.Setup();
        }
    }
}
