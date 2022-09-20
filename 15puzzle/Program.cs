using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Testing
{
    class StateNode<T>
    {
        public double F { get; set; }
        public T[,] State { get; set; }
        public int EmptyColumn { get; private set; }
        public int EmptyRow { get; private set; }
        public int Depth { get; set; }
        public string StringRepresentation { get; set; }

        public StateNode(T[,] state, int emptyRow, int emptyColumn, int depth)
        {
            if (state.GetLength(0) != state.GetLength(1))
                throw new Exception("Number of columns and rows isn't the same!.");

            State = (T[,])state.Clone();
            EmptyColumn = emptyColumn;
            EmptyRow = emptyRow;
            Depth = depth;
            StringRepresentation = "";

            for (var i = 0; i < State.GetLength(0); i++)
            {
                for (var j = 0; j < State.GetLength(1); j++)
                    StringRepresentation += state[i, j] + ",";
            }
            if (StringRepresentation is null)
                throw new Exception("the array is empty!");
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
    }

    class AStar<T>
    {
        public int StatesChecked { get; set; }
        private readonly StateNode<T> SolutionState;
        private T EmptyTile { get; set; }
        private readonly PriorityQueue<StateNode<T>, double> queue;
        private readonly HashSet<string> hash;
        private readonly HashSet<double> _hash;

        public AStar(StateNode<T> initial, StateNode<T> solution, T empty)
        {
            queue = new PriorityQueue<StateNode<T>, double>();
            queue.Enqueue(initial, initial.F);
            SolutionState = solution;
            EmptyTile = empty;
            hash = new HashSet<string>();
        }

        private int LinearConflicts(StateNode<T> node)
        {
            var result = 0;
            var state = node.State;

            // Row Conflicts
            for (var i = 0; i < state.GetLength(0); i++)
                result += FindConflicts(state, i, 1);

            // Column Conflicts
            for (var i = 0; i < state.GetLength(1); i++)
                result += FindConflicts(state, i, 0);

            return result;
        }

        private int FindConflicts(T[,] state, int i, int dimension)
        {
            var result = 0;
            var tilesRelated = new List<int>();

            // Loop foreach pair of elements in the row/column
            for (var h = 0; h < state.GetLength(dimension) - 1 && !tilesRelated.Contains(h); h++)
            {
                for (var k = h + 1; k < state.GetLength(dimension) && !tilesRelated.Contains(h); k++)
                {
                    // Avoid the empty tile
                    if (dimension == 1 && state[i, h].Equals(EmptyTile)) continue;
                    if (dimension == 0 && state[h, i].Equals(EmptyTile)) continue;
                    if (dimension == 1 && state[i, k].Equals(EmptyTile)) continue;
                    if (dimension == 0 && state[k, i].Equals(EmptyTile)) continue;

                    var moves = dimension == 1
                        ? InConflict(i, state[i, h], state[i, k], h, k, dimension)
                        : InConflict(i, state[h, i], state[k, i], h, k, dimension);

                    if (moves == 0) continue;
                    result += 2;
                    tilesRelated.AddRange(new List<int> { h, k });
                    break;
                }
            }

            return result;
        }

        private int InConflict(int index, T a, T b, int indexA, int indexB, int dimension)
        {
            var indexGoalA = -1;
            var indexGoalB = -1;

            for (var c = 0; c < SolutionState.State.GetLength(dimension); c++)
            {
                if (dimension == 1 && SolutionState.State[index, c].Equals(a))
                    indexGoalA = c;
                else if (dimension == 1 && SolutionState.State[index, c].Equals(b))
                    indexGoalB = c;
                else if (dimension == 0 && SolutionState.State[c, index].Equals(a))
                    indexGoalA = c;
                else if (dimension == 0 && SolutionState.State[c, index].Equals(b))
                    indexGoalB = c;
            }

            return (indexGoalA >= 0 && indexGoalB >= 0) && ((indexA < indexB && indexGoalA > indexGoalB) ||
                                                            (indexA > indexB && indexGoalA < indexGoalB))
                       ? 2
                       : 0;
        }

        private double Heuristics(StateNode<T> node)
        {
            double result = 0.0;
            //loop for calculating MD for each element in the array
            for (var i = 0; i < node.State.GetLength(0); i++)
            {
                for (var j = 0; j < node.State.GetLength(1); j++)
                {
                    var elem = node.State[i, j];
                    if (elem!.Equals(EmptyTile)) continue; //MD calculation for empty tiles is not needed

                    var found = false;

                    //loop for finding the same element in the goal state to calculate MD
                    for (var h = 0; h < SolutionState.State.GetLength(0); h++)
                    {
                        for (var k = 0; k < SolutionState.State.GetLength(1); k++)
                        {
                            if (SolutionState.State[h, k]!.Equals(elem))
                            {
                                result += Math.Abs(h - i) + Math.Abs(j - k);
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                }
            }
            return result;
        }

        //this method creates and enqueues new nodes that are not already included in the queue.
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

                if (!hash.Contains(newNode.StringRepresentation))
                {
                    newNode.F = node.Depth + Heuristics(newNode);
                    queue.Enqueue(newNode, newNode.F);
                    hash.Add(newNode.StringRepresentation);
                }
            }
        }

        //this method goes through every state enqueued, creating new ones with the ExpandNodes method until solution is found.
        public StateNode<T> Execute()
        {
            hash.Add(queue.Peek().StringRepresentation);

            while (true)
            {
                var dequeuedElement = queue.Dequeue();

                //var element = queue.Dequeue();
                if (queue.Count > 100)
                {
                    StreamWriter file = new StreamWriter("myfile", true);
                    while (queue.Count > 0)
                    {
                        var element = queue.Dequeue();
                        file.WriteLine("{0}|{1}|{2},{3},{4}", element.F, element.StringRepresentation, element.EmptyColumn, element.EmptyRow, element.Depth);
                    }
                    file.Close();
                }

                StatesChecked++;

                if (dequeuedElement.StringRepresentation.Equals(SolutionState.StringRepresentation))
                    return dequeuedElement;

                ExpandNodes(dequeuedElement);

                if (queue.Count == 0)
                {
                    foreach (var line in File.ReadLines("myfile"))
                    {
                        string text = line.ToString();
                        int counter = 0;
                        string number = " ";

                        while (text[counter] != '|') { number = number.Insert(number.Length - 1, text[counter].ToString()); counter++; }
                        counter++;
                        int FValue = Convert.ToInt32(number);

                        int[,] state = new int[dequeuedElement.State.GetLength(0), dequeuedElement.State.GetLength(1)];
                        for (int i = 0; i < state.GetLength(0); i++)
                            for (int j = 0; j < state.GetLength(1); j++)
                            {
                                number = " ";
                                while (text[counter] != ',')
                                {
                                    number = number.Insert(number.Length - 1, text[counter].ToString());
                                    counter++;
                                }
                                state[i, j] = Convert.ToInt32(number);
                                counter++;
                            }
                        counter++;

                        number = " ";
                        while (text[counter] != ',')
                        {
                            number = number.Insert(number.Length - 1, text[counter].ToString());
                            counter++;
                        }
                        int emptyCol = Convert.ToInt32(number);
                        counter++;

                        number = " ";
                        while (text[counter] != ',')
                        {
                            number = number.Insert(number.Length - 1, text[counter].ToString());
                            counter++;
                        }
                        int emptyRow = Convert.ToInt32(number);
                        counter++;

                        number = " ";
                        while (counter < text.Length)
                        {
                            number = number.Insert(number.Length - 1, text[counter].ToString());
                            counter++;
                        }
                        int depth = Convert.ToInt32(number);
                        counter++;

                        var node = new StateNode<T>((T[,])(object)state, emptyRow, emptyCol, depth)
                        {
                            F = FValue
                        };


                        queue.Enqueue(node, node.F);

                        /*string? text = file.ReadLine();
                        File.ReadAllLines("myfile.txt");
                        file.Close();
                        StreamWriter fileErase = new StreamWriter("myfile.txt");*/
                        var Lines = File.ReadAllLines(@"myfile");
                        var newLines = Lines.Where(lines => !lines.Contains(text));
                        File.WriteAllLines(@"temp", newLines);



                        break;
                    }

                    File.Replace("temp", "myfile", "tempBak");
                    /*var Lines = File.ReadAllLines(@"myfile.txt");
                    var newLines = Lines.Where(lines => !lines.Contains(text));
                    File.WriteAllLines(@"myfile.txt", newLines);*/

                    /*StreamReader file = new StreamReader("myfile.txt");
                    file.Close();*/
                }
            }

            throw new Exception("ERROR! No solution found! Perhaps given puzzle configuration is unsolvable?");
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

            var initConfig4x4 = new[,] {     {7,0,13,5},
                                             {8,3,2,9},
                                             {4,10,11,14},
                                             {15,6,12,1}
                                    };

            var initConfig4x43 = new[,] {     {5,10,14,7},
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

            var initialState = new StateNode<int>(initConfig4x43, 2, 1, 0);
            var finalState = new StateNode<int>(finalConfig4x4, 3, 3, 0);

            var aStar = new AStar<int>(initialState, finalState, 0);

            var node = aStar.Execute();
            Console.WriteLine(new System.IO.FileInfo("myfile").Length);
            File.Delete("myfile.txt");

            Console.WriteLine("Node at depth {0}", node.Depth);
            Console.WriteLine("States visited {0}", aStar.StatesChecked);
            Console.Read();

        }
    }
}