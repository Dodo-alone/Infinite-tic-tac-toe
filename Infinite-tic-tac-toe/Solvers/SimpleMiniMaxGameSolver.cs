using Infinite_tic_tac_toe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinite_tic_tac_toe.Solvers
{
      public class SimpleMiniMaxGameSolver : GameSolverBase
      {
            #region Private Const Members

            private const double CONVERGENCE_THRESHOLD = 1e-6;
            private const int MAX_ITERATIONS = 1000;

            #endregion

            #region Constructor

            public SimpleMiniMaxGameSolver() : base() { }

            #endregion

            #region Override Methods

            protected override void EvaluateGameGraph()
            {
                  // Initialize all states
                  foreach (var kvp in _stateCache)
                  {
                        var node = kvp.Value;
                        var key = kvp.Key;

                        var winner = node.Board.CheckWinner();
                        if (winner != PositionEnum.Empty)
                        {
                              // Initialize terminal states with their winning values
                              _evaluationCache[key] = new NodeValue
                              {
                                    CrossWinProbability = winner == PositionEnum.Cross ? 1.0 : 0.0,
                                    NaughtWinProbability = winner == PositionEnum.Naught ? 1.0 : 0.0,
                                    IsTerminal = true
                              };
                        }
                        else
                        {
                              // Initialize non-terminal states with neutral values
                              _evaluationCache[key] = new NodeValue
                              {
                                    CrossWinProbability = 0.5,
                                    NaughtWinProbability = 0.5,
                                    IsTerminal = false
                              };
                        }
                  }

                  // Iteratively update probabilities until convergence
                  for (int iteration = 0; iteration < MAX_ITERATIONS; iteration++)
                  {
                        bool converged = true;

                        foreach (var kvp in _stateCache)
                        {
                              var node = kvp.Value;
                              var key = kvp.Key;

                              if (_evaluationCache[key].IsTerminal || node.Children.Count == 0)
                                    continue;

                              var oldValue = _evaluationCache[key];
                              var newValue = EvaluateNode(node);

                              if (Math.Abs(newValue.CrossWinProbability - oldValue.CrossWinProbability) > CONVERGENCE_THRESHOLD ||
                                  Math.Abs(newValue.NaughtWinProbability - oldValue.NaughtWinProbability) > CONVERGENCE_THRESHOLD)
                              {
                                    converged = false;
                              }

                              _evaluationCache[key] = newValue;
                        }

                        if (converged)
                              break;
                  }
            }

            protected override NodeValue EvaluateNode(GameNode node)
            {
                  if (node.Children.Count == 0)
                  {
                        // Leaf node that's not terminal - shouldn't happen in a well-formed graph
                        return new NodeValue
                        {
                              CrossWinProbability = 0.5,
                              NaughtWinProbability = 0.5,
                              IsTerminal = false
                        };
                  }

                  var childValues = new List<NodeValue>();
                  foreach (var child in node.Children)
                  {
                        string childKey = GenerateStateKey(child.Board, child.CurrentPlayer);
                        if (_evaluationCache.ContainsKey(childKey))
                              childValues.Add(_evaluationCache[childKey]);
                  }

                  if (childValues.Count == 0)
                        return new NodeValue { CrossWinProbability = 0.5, NaughtWinProbability = 0.5, IsTerminal = false };

                  double crossWinProb, naughtWinProb;

                  if (node.CurrentPlayer == PlayerEnum.Cross)
                  {
                        // Cross player chooses the move that maximizes their win probability
                        crossWinProb = childValues.Max(v => v.CrossWinProbability);

                        // Find the corresponding naught probability for that optimal move
                        var bestMove = childValues.Where(v => Math.Abs(v.CrossWinProbability - crossWinProb) < 1e-9).First();
                        naughtWinProb = bestMove.NaughtWinProbability;
                  }
                  else
                  {
                        // Naught player chooses the move that maximizes their win probability
                        naughtWinProb = childValues.Max(v => v.NaughtWinProbability);

                        // Find the corresponding cross probability for that optimal move
                        var bestMove = childValues.Where(v => Math.Abs(v.NaughtWinProbability - naughtWinProb) < 1e-9).First();
                        crossWinProb = bestMove.CrossWinProbability;
                  }

                  return new NodeValue
                  {
                        CrossWinProbability = crossWinProb,
                        NaughtWinProbability = naughtWinProb,
                        IsTerminal = false
                  };
            }

            protected override double CalculateMoveScore(NodeValue value, PlayerEnum player)
            {
                  if (player == PlayerEnum.Cross)
                  {
                        // Prioritize winning, but also consider avoiding loss
                        return value.CrossWinProbability * 2.0 - value.NaughtWinProbability;
                  }
                  else
                  {
                        return value.NaughtWinProbability * 2.0 - value.CrossWinProbability;
                  }
            }

            #endregion
      }
}
