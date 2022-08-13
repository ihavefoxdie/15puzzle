namespace Testing
{
    class StateNode<T> : IComparable<StateNode<T>> where T : IComparable
    {
        public double ValueF { get; set; }
        public T[,] State { get; set; }
        public int EmptyCol { get; private set; }
        public int EmptyRow { get; private set; }
        public int ValueG { get; set; }
        public string Representation { get; set; }

        public StateNode(T[,] state, int emptyCol, int emptyRow, int valueG)
        {
            if (state.GetLength(0) != state.GetLength(1))
                throw new Exception("Number of columns and rows isn't the same!.");

            State = state.Clone() as T[,];
            EmptyCol = emptyCol;
            EmptyRow = emptyRow;
            ValueG = valueG;
            
            for(var i = 0; i < State.GetLength(0); i++)
            {
                for (var j = 0; j < State.GetLength(1); j++)
                    Representation += state[i, j] + ",";
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

        public int CompareTo (StateNode<T> other)
        {
            if (ValueF > other.ValueF)
                return 1;
            if (ValueF < other.ValueF)
                return -1;

            return 0;
        }
    }
    class Program
    {
        static void Main()
        {

        }
    }
}