using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Model
{
      /// <summary>
      /// A simple object representing the value of a node in the solver graph
      /// </summary>
      public class NodeValue
      {
            public double CrossWinProbability { get; set; }
            public double NaughtWinProbability { get; set; }
            public bool IsTerminal { get; set; }
      }

      /// <summary>
      /// A node in the game solver graph, a node has children, a board and a current player. 
      /// there are some 5k possible nodes
      /// </summary>
      public class GameNode
      {
            public GameBoard Board { get; }
            public PlayerEnum CurrentPlayer { get; }
            public List<GameNode> Children { get; }

            /// <summary>
            /// Constructs an instance of this node
            /// </summary>
            /// <param name="board">The board represented</param>
            /// <param name="player">The player whos turn it is on this board</param>
            public GameNode(GameBoard board, PlayerEnum player)
            {
                  Board = board;
                  CurrentPlayer = player;
                  Children = [];
            }
      }
}
