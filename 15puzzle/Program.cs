using System;
using System.Collections.Generic;

namespace Testing
{
    class StateNode<T> : IComparable<StateNode<T>> where T : IComparable
    {
        public double F { get; set; }
        public T[,] State { get; set; }
        public int EmptyColumn { get; private set; }
        public int EmptyRow { get; private set; }
        public int Depth { get; set; }
        public string StringRepresentation { get; set; }

        public StateNode() { }

        public StateNode(T[,] state, int emptyColumn, int emptyRow, int depth)
        {
            if (state.GetLength(0) != state.GetLength(1))
                throw new Exception("Number of columns and rows isn't the same!.");

            State = (T[,]) state.Clone();
            EmptyColumn = emptyColumn;
            EmptyRow = emptyRow;
            Depth = depth;
            
            for(var i = 0; i < State.GetLength(0); i++)
            {
                for (var j = 0; j < State.GetLength(1); j++)
                    StringRepresentation += state[i, j] + ",";
            }
        }

        public int Size
        {
            get { return State.GetLength(0); }
        }

        public void Print()
        {
            for (var i = 0; i < State.GetLength(0); i++)
            {
                for (var j = 0; j < State.GetLength(1); j++)
                    Console.Write(State[i, j] + ", ");
                Console.WriteLine();
            }
        }

        public int CompareTo(StateNode<T> other)
        {
            if (F > other.F)
                return 1;
            if (F < other.F)
                return -1;

            return 0;
        }
    }

    class AStar<T> where T : IComparable
    {
        public int StatesChecked { get; set; }
        private readonly StateNode<T> SolutionState;
        private T EmptyTile { get; set; }
        private readonly PriorityQueue<StateNode<T>, double> queue;
        private readonly HashSet<string> hash;

        public AStar(StateNode<T> initial, StateNode<T> solution, T empty)
        {
            queue.Enqueue(initial, initial.F);
            SolutionState = solution;
            EmptyTile = empty;
            hash = new HashSet<string>();
        }

        private double Heuristics(StateNode<T> node)
        {
            var result = 0;

            for (var i = 0; i < node.State.GetLength(0); i++)
            {
                for (var j = 0; j < node.State.GetLength(1); j++)
                    if (!node.State[i, j].Equals(SolutionState.State[i, j]) && !node.State[i, j].Equals(EmptyTile))
                        result++;
            }

            return result;
        }

        private void ExpandNodes(StateNode<T> node)
        {
            T temporary;
            T[,] newState;
            var column = node.EmptyColumn;
            var row = node.EmptyRow;
            StateNode<T> newNode;

            //Moving empty tile up
            if (row > 0)
            {
                newState = (T[,])node.State.Clone();
                temporary = newState[row - 1, column];
                newState[row - 1, column] = EmptyTile;
                newState[row, column] = temporary;
                newNode = new StateNode<T>(newState, row - 1, column, node.Depth + 1);

                if (!hash.Contains(newNode.StringRepresentation))
                {
                    newNode.F = node.Depth + Heuristics(newNode);
                    queue.Enqueue(newNode, newNode.F);
                    hash.Add(newNode.StringRepresentation);
                }
            }

            //Moving empty tile down
            if (row < node.Size - 1)
            {
                newState = (T[,])node.State.Clone();
                temporary = newState[row + 1, column];
                newState[row + 1, column] = EmptyTile;
                newState[row, column] = temporary;
                newNode = new StateNode<T>(newState, row + 1, column, node.Depth + 1);

                if (!hash.Contains(newNode.StringRepresentation))
                {
                    newNode.F = node.Depth + Heuristics(newNode);
                    queue.Enqueue(newNode, newNode.F);
                    hash.Add(newNode.StringRepresentation);
                }
            }

            //Moving empty tile left
            if (column > 0)
            {
                newState = (T[,])node.State.Clone();
                temporary = newState[row, column - 1];
                newState[row, column - 1] = EmptyTile;
                newState[row, column] = temporary;
                newNode = new StateNode<T>(newState, row, column - 1, node.Depth + 1);

                if (!hash.Contains(newNode.StringRepresentation))
                {
                    newNode.F = node.Depth + Heuristics(newNode);
                    queue.Enqueue(newNode, newNode.F);
                    hash.Add(newNode.StringRepresentation);
                }
            }

            //Moving empty tile right
            if (column < node.Size - 1)
            {
                newState = (T[,])node.State.Clone();
                temporary = newState[row, column + 1];
                newState[row, column + 1] = EmptyTile;
                newState[row, column] = temporary;
                newNode = new StateNode<T>(newState, row, column + 1, node.Depth + 1);

                if(!hash.Contains(newNode.StringRepresentation))
                {
                    newNode.F = node.Depth + Heuristics(newNode);
                    queue.Enqueue(newNode, newNode.F);
                    hash.Add(newNode.StringRepresentation);
                }
            }
        }

        public StateNode<T> Execute()
        {
            StateNode<T> dequeuedState = queue.Dequeue();
            hash.Add(dequeuedState.StringRepresentation);

            while(queue.Count > 0)
            {
                StatesChecked++;

                if (dequeuedState.StringRepresentation.Equals(SolutionState.StringRepresentation))
                    return dequeuedState;

                ExpandNodes(dequeuedState);
                dequeuedState = queue.Dequeue();
            }

            return null;
        }
    }


    class Program
    {
        static void Main()
        {
            var initWorstConfig3x3 = new[,] {   {8,6,7},
                                                {2,5,4},
                                                {3,0,1}
                                    };

            var initConfig4x4 = new[,] {     {5,10,14,7},
                                             {8,3,6,1},
                                             {15,0,12,9},
                                             {2,11,4,13}
                                    };

            var finalConfig3x3 = new[,] {    {1,2,3},
                                             {4,5,6},
                                             {7,8,0}
                                    };

            var finalConfig4x4 = new[,] {    {1,2,3,4},
                                             {5,6,7,8},
                                             {9,10,11,12},
                                             {13,14,15,0}
                                    };

            var initialState = new StateNode<int>(initWorstConfig3x3, 2, 1, 0);
            var finalState = new StateNode<int>(finalConfig3x3, 2, 2, 0);

            var aStar = new AStar<int>(initialState, finalState, 0);

            var node = aStar.Execute();

            Console.WriteLine("Node at depth {0}", node.Depth);
            Console.WriteLine("States visited {0}", aStar.StatesChecked);
            Console.Read();
        }
    }
}