using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Model
{
      /// <summary>
      /// Possible states for each position to be in
      /// </summary>
      public enum PositionEnum
      {
            Empty = 0,
            Naught,
            Cross
      }

      /// <summary>
      /// The two possible players 
      /// </summary>
      public enum PlayerEnum
      {
            Naught,
            Cross
      }

      /// <summary>
      /// A class representing a specific game board
      /// </summary>
      public class GameBoard
      {
            #region Member Variables

            private readonly PositionEnum[] _board;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs an instance of this game board with nine spaces
            /// </summary>
            public GameBoard()
            {
                  _board = new PositionEnum[9];
                  for (int i = 0; i < 9; i++)
                        _board[i] = PositionEnum.Empty;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="other"></param>
            public GameBoard(GameBoard other)
            {
                  _board = new PositionEnum[9];
                  Array.Copy(other._board, _board, 9);
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Gets the piece at the specified coordinates
            /// </summary>
            /// <param name="x">The X coordinate</param>
            /// <param name="y">The Y coordinate</param>
            /// <returns></returns>
            public PositionEnum GetPosition(int x, int y)
            {
                  return _board[y * 3 + x];
            }

            /// <summary>
            /// Sets the piece at the specified coordinate
            /// </summary>
            /// <param name="x">The X coordinate</param>
            /// <param name="y">The Y coordinate</param>
            /// <param name="value">The piece to set to</param>
            public void SetPosition(int x, int y, PositionEnum value)
            {
                  _board[y * 3 + x] = value;
            }

            /// <summary>
            /// Get a copy of the game board array
            /// </summary>
            /// <returns>A deep copy of the board array</returns>
            public PositionEnum[] GetBoardArray() => (PositionEnum[])_board.Clone();

            /// <summary>
            /// Returns the winning piece for this board
            /// </summary>
            /// <returns>The winning peice or empty if there are no winners on this board</returns>
            public PositionEnum CheckWinner()
            {
                  int[,] winConditions = new int[,]
                  {
                          { 0, 1, 2 },
                          { 3, 4, 5 },
                          { 6, 7, 8 },
                          { 0, 3, 6 },
                          { 1, 4, 7 },
                          { 2, 5, 8 },
                          { 0, 4, 8 },
                          { 2, 4, 6 }
                  };

                  for (int i = 0; i < 8; i++)
                  {
                        int a = winConditions[i, 0];
                        int b = winConditions[i, 1];
                        int c = winConditions[i, 2];

                        if (_board[a] != PositionEnum.Empty &&
                            _board[a] == _board[b] &&
                            _board[b] == _board[c])
                              return _board[a];
                  }

                  return PositionEnum.Empty;
            }

            /// <summary>
            /// A list of coordinates for positions that are empty
            /// </summary>
            /// <returns>A list of x, y tuples where positions are empty</returns>
            public List<(int x, int y)> GetEmptyPositions()
            {
                  List<(int, int)> positions = new List<(int, int)>();
                  for (int i = 0; i < 9; i++)
                  {
                        if (_board[i] == PositionEnum.Empty)
                              positions.Add((i % 3, i / 3));
                  }
                  return positions;
            }

            /// <summary>
            /// A list of positions for positions that are occupied by a specific player
            /// </summary>
            /// <param name="player">The player we are querying</param>
            /// <returns>A list of x, y tuples where positions correspond to the supplied player</returns>
            public List<(int x, int y)> GetPlayerPositions(PositionEnum player)
            {
                  List<(int, int)> positions = new List<(int, int)>();
                  for (int i = 0; i < 9; i++)
                  {
                        if (_board[i] == player)
                              positions.Add((i % 3, i / 3));
                  }
                  return positions;
            }

            /// <summary>
            /// Count the number of pieces a specific player has on the board
            /// </summary>
            /// <param name="player">The player whose pieces we are counting</param>
            /// <returns>an int for the number of pieces the player has</returns>
            public int CountPieces(PositionEnum player)
            {
                  return _board.Count(p => p == player);
            }

            /// <summary>
            /// Return a new gameboard with one change
            /// </summary>
            /// <param name="x">The X coordinate to change</param>
            /// <param name="y">The Y coordinate to change</param>
            /// <param name="newValue">The new piece for that position</param>
            /// <returns>A new gameboard with the change applied</returns>
            public GameBoard CloneWithChange(int x, int y, PositionEnum newValue)
            {
                  var clone = new GameBoard(this);
                  clone.SetPosition(x, y, newValue);
                  return clone;
            }

            /// <summary>
            /// Converts from an absolute index to a coordinate
            /// </summary>
            /// <param name="index">The index to convert from</param>
            /// <returns>an x, y tuple representing the coordinates for this index</returns>
            public static (int x, int y) FromIndex(int index)
            {
                  ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 8);

                  return (index % 3, index / 3);
            }

            #endregion

            #region Public Overide Methods

            public override bool Equals(object? obj)
            {
                  if (obj is GameBoard other)
                  {
                        return _board.SequenceEqual(other._board);
                  }
                  return false;
            }

            public override int GetHashCode()
            {
                  return string.Join(",", _board).GetHashCode();
            }

            #endregion
      }
}
