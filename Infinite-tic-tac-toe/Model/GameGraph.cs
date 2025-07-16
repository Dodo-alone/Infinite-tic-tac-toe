using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Model
{
      public class NodeValue
      {
            public double CrossWinProbability { get; set; }
            public double NaughtWinProbability { get; set; }
            public bool IsTerminal { get; set; }
      }

      public class GameNode
      {
            public GameBoard Board { get; }
            public PlayerEnum CurrentPlayer { get; }
            public List<GameNode> Children { get; }

            public GameNode(GameBoard board, PlayerEnum player)
            {
                  Board = board;
                  CurrentPlayer = player;
                  Children = [];
            }
      }
}
