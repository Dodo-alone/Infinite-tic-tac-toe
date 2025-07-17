using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Model
{
      public enum PositionEnum
      {
            Empty = 0,
            Naught,
            Cross
      }

      public enum PlayerEnum
      {
            Naught,
            Cross
      }

      public class GameBoard
      {
            #region Member Variables

            private readonly PositionEnum[] _board;

            #endregion

            #region Constructor

            public GameBoard()
            {
                  _board = new PositionEnum[9];
                  for (int i = 0; i < 9; i++)
                        _board[i] = PositionEnum.Empty;
            }

            public GameBoard(GameBoard other)
            {
                  _board = new PositionEnum[9];
                  Array.Copy(other._board, _board, 9);
            }

            #endregion

            #region Public Methods

            public PositionEnum GetPosition(int x, int y)
            {
                  return _board[y * 3 + x];
            }


            public void SetPosition(int x, int y, PositionEnum value)
            {
                  _board[y * 3 + x] = value;
            }

            public PositionEnum[] GetBoardArray() => (PositionEnum[])_board.Clone();

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

            public int CountPieces(PositionEnum player)
            {
                  return _board.Count(p => p == player);
            }

            public GameBoard CloneWithChange(int x, int y, PositionEnum newValue)
            {
                  var clone = new GameBoard(this);
                  clone.SetPosition(x, y, newValue);
                  return clone;
            }

            public PositionEnum GetAtIndex(int index)
            {
                  return _board[index];
            }

            public static (int x, int y) FromIndex(int index)
            {
                  return (index % 3, index / 3);
            }

            public static int ToIndex(int x, int y)
            {
                  return y * 3 + x;
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
