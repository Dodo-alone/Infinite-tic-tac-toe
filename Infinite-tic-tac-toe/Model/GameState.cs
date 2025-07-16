using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Model
{
      public class GameState
      {
            #region Private Readonly Members

            private readonly GameBoard _board;
            private readonly PlayerEnum _player;

            #endregion

            #region Public Properties

            public GameBoard Board { get => _board; }
            public PlayerEnum CurrentPlayer { get => _player; }

            #endregion

            #region Constructor

            public GameState(GameBoard board, PlayerEnum player)
            {
                  _board = board;
                  _player = player;
            }

            #endregion

            #region Public Methods

            public PositionEnum PlayerToPosition()
            {
                  return CurrentPlayer == PlayerEnum.Cross ? PositionEnum.Cross : PositionEnum.Naught;
            }

            public PlayerEnum NextPlayer()
            {
                  return CurrentPlayer == PlayerEnum.Cross ? PlayerEnum.Naught : PlayerEnum.Cross;
            }

            #endregion
      }
}
