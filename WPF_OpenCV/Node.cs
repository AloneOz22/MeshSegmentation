namespace WpfApp
{
    public class Node
    {
        public int Num;
        public int Tag;
        public double X;
        public double Y;
        public double Z;
        public Node(int num, double x, double y, double z)
        {
            Num = num;
            X = x;
            Y = y;
            Z = z;
            Tag = 0;
        }
        public Node()
        {
            Num = -1;
            Tag = 0;
            X = default;
            Y = default;
            Z = default;
        }
    }
}
