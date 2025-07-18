using Infinite_tic_tac_toe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Solvers
{
      public abstract class GameSolverBase
      {
            #region Private Members

            protected Dictionary<string, GameNode> _stateCache;
            protected Dictionary<string, NodeValue> _evaluationCache;

            #endregion

            #region Constructor

            public GameSolverBase()
            {
                  _stateCache = new Dictionary<string, GameNode>();
                  _evaluationCache = new Dictionary<string, NodeValue>();
            }

            #endregion

            #region Public methods

            /// <summary>
            /// Returns the best move to make next
            /// </summary>
            /// <param name="currentBoard">The current game board</param>
            /// <param name="player">The player whose turn it is</param>
            /// <returns>A tuple containing then next best move, if a none loosing move exists, and a description of the move</returns>
            public (GameBoard? nextBoard, string moveDescription) GetBestMove(GameBoard currentBoard, PlayerEnum player)
            {
                  var startNode = BuildGameGraph(currentBoard, player);
                  EvaluateGameGraph();
                  var bestChild = FindBestChild(startNode, player);

                  if (bestChild != null)
                  {
                        string moveDesc = GenerateMoveDescription(currentBoard, bestChild.Board, player);
                        return (bestChild.Board, moveDesc);
                  }

                  return (null, "No valid moves");
            }

            /// <summary>
            /// Builds a Directed Cyclic graph representing the entire game. a nodes children are 
            /// possible game states reached from that state
            /// </summary>
            /// <param name="startBoard">The starting board</param>
            /// <param name="startPlayer">the starting player</param>
            /// <returns>The initial game node, populated with children</returns>
            public GameNode BuildGameGraph(GameBoard startBoard, PlayerEnum startPlayer)
            {
                  string startKey = GenerateStateKey(startBoard, startPlayer);
                  if (_stateCache.ContainsKey(startKey))
                        return _stateCache[startKey];

                  var rootNode = new GameNode(startBoard, startPlayer);
                  _stateCache[startKey] = rootNode;

                  var queue = new Queue<GameNode>();
                  queue.Enqueue(rootNode);

                  // BFS to walk the entire tree 
                  while (queue.Count > 0)
                  {
                        var currentNode = queue.Dequeue();
                        string currentKey = GenerateStateKey(currentNode.Board, currentNode.CurrentPlayer);

                        if (currentNode.Children.Count > 0 || IsTerminalState(currentNode.Board))
                              continue;

                        var moves = GeneratePossibleMoves(currentNode.Board, currentNode.CurrentPlayer);

                        foreach (var (newBoard, moveDesc) in moves)
                        {
                              PlayerEnum nextPlayer = currentNode.CurrentPlayer == PlayerEnum.Cross ? PlayerEnum.Naught : PlayerEnum.Cross;
                              string childKey = GenerateStateKey(newBoard, nextPlayer);

                              GameNode childNode;
                              if (_stateCache.ContainsKey(childKey))
                              {
                                    childNode = _stateCache[childKey];
                              }
                              else
                              {
                                    childNode = new GameNode(newBoard, nextPlayer);
                                    _stateCache[childKey] = childNode;
                                    if (!IsTerminalState(newBoard))
                                          queue.Enqueue(childNode);
                              }

                              currentNode.Children.Add(childNode);
                        }
                  }

                  return rootNode;
            }

            #endregion

            #region Private methods

            private GameNode? FindBestChild(GameNode node, PlayerEnum player)
            {
                  GameNode? bestChild = null;
                  double bestScore = double.NegativeInfinity;

                  foreach (var child in node.Children)
                  {
                        string key = GenerateStateKey(child.Board, child.CurrentPlayer);
                        if (!_evaluationCache.ContainsKey(key))
                              continue;

                        var value = _evaluationCache[key];
                        double score = CalculateMoveScore(value, player);

                        if (score > bestScore)
                        {
                              bestScore = score;
                              bestChild = child;
                        }
                  }

                  return bestChild;
            }

            private List<(GameBoard board, string description)> GeneratePossibleMoves(GameBoard board, PlayerEnum player)
            {
                  var moves = new List<(GameBoard, string)>();

                  PositionEnum playerPos = player == PlayerEnum.Cross ? PositionEnum.Cross : PositionEnum.Naught;
                  int playerPieceCount = board.CountPieces(playerPos);

                  if (playerPieceCount < 3)
                  {
                        // Placement phase - place pieces on empty positions
                        var emptyPositions = board.GetEmptyPositions();
                        foreach (var (x, y) in emptyPositions)
                        {
                              var newBoard = new GameBoard(board);
                              newBoard.SetPosition(x, y, playerPos);
                              moves.Add((newBoard, $"Place at ({x},{y})"));
                        }
                  }
                  else
                  {
                        // Movement phase - move existing pieces to empty positions
                        var playerPositions = board.GetPlayerPositions(playerPos);
                        var emptyPositions = board.GetEmptyPositions();

                        foreach (var (fromX, fromY) in playerPositions)
                        {
                              foreach (var (toX, toY) in emptyPositions)
                              {
                                    var newBoard = new GameBoard(board);
                                    newBoard.SetPosition(fromX, fromY, PositionEnum.Empty);
                                    newBoard.SetPosition(toX, toY, playerPos);
                                    moves.Add((newBoard, $"Move from ({fromX},{fromY}) to ({toX},{toY})"));
                              }
                        }
                  }

                  return moves;
            }

            #endregion

            #region Protected Methods

            /// <summary>
            /// A simple key for this game state
            /// </summary>
            /// <param name="board">The current board</param>
            /// <param name="currentPlayer">The current player</param>
            /// <returns></returns>
            protected static string GenerateStateKey(GameBoard board, PlayerEnum currentPlayer)
            {
                  var positions = board.GetBoardArray();
                  return string.Join("", positions.Select(p => (int)p)) + "_" + (int)currentPlayer;
            }

            /// <summary>
            /// True if someone is winning in this state
            /// </summary>
            /// <param name="board">The board to determine the winner from</param>
            /// <returns></returns>
            protected static bool IsTerminalState(GameBoard board) => board.CheckWinner() != PositionEnum.Empty;

            /// <summary>
            /// Generates a move description string
            /// </summary>
            /// <param name="oldBoard">The board we moved from</param>
            /// <param name="newBoard">The board we moved to</param>
            /// <param name="player">The player who moved</param>
            /// <returns>A string describing the move</returns>
            protected static string GenerateMoveDescription(GameBoard oldBoard, GameBoard newBoard, PlayerEnum player)
            {
                  PositionEnum playerPos = player == PlayerEnum.Cross ? PositionEnum.Cross : PositionEnum.Naught;

                  (int x, int y) placedAt = (-1, -1);
                  (int x, int y) movedFrom = (-1, -1);
                  (int x, int y) movedTo = (-1, -1);

                  // Check all positions for changes
                  for (int x = 0; x < 3; x++)
                  {
                        for (int y = 0; y < 3; y++)
                        {
                              var oldPos = oldBoard.GetPosition(x, y);
                              var newPos = newBoard.GetPosition(x, y);

                              if (oldPos != newPos)
                              {
                                    if (oldPos == PositionEnum.Empty && newPos == playerPos)
                                    {
                                          if (movedFrom == (-1, -1))
                                                placedAt = (x, y);
                                          else
                                                movedTo = (x, y);
                                    }
                                    else if (oldPos == playerPos && newPos == PositionEnum.Empty)
                                    {
                                          movedFrom = (x, y);
                                    }
                              }
                        }
                  }

                  if (placedAt != (-1, -1) && movedFrom == (-1, -1))
                  {
                        return $"Place piece at position ({placedAt.x},{placedAt.y})";
                  }
                  else if (movedFrom != (-1, -1) && movedTo != (-1, -1))
                  {
                        return $"Move piece from ({movedFrom.x},{movedFrom.y}) to ({movedTo.x},{movedTo.y})";
                  }

                  return "Move made";
            }

            /// <summary>
            /// Will Evaluate all or part of the graph to score moves
            /// </summary>
            protected abstract void EvaluateGameGraph();

            /// <summary>
            /// Calculates a score associated with a specific move
            /// </summary>
            /// <param name="value">The value assigned to the node in evaluate game graph</param>
            /// <param name="player">The current player</param>
            /// <returns>A double representing how good the move is</returns>
            protected abstract double CalculateMoveScore(NodeValue value, PlayerEnum player);

            /// <summary>
            /// Generates a score for a node
            /// </summary>
            /// <param name="node">The game node we are scoring</param>
            /// <returns>A node value object</returns>
            protected abstract NodeValue EvaluateNode(GameNode node);

            #endregion
      }
}