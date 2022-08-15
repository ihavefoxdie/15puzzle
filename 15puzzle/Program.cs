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
            return 0;
        }

        private void ExpandNodes(StateNode<T> node)
        {
            T temporary;
            T[,] newState;
            var column = node.EmptyColumn;
            var row = node.EmptyRow;
            StateNode<T> newNode;

            //going up
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

        }
    }
}