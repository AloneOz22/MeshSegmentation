using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    public class Node
    {
        public int Num;
        public int Tag;
        public int[] TagCounter = new int[3];
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
            for (int i = 0; i < TagCounter.Length; i++)
            {
                TagCounter[i] = 0;
            }
        }
        public Node()
        {
            Num = -1;
            Tag = 0;
            X = default;
            Y = default;
            Z = default;
            for (int i = 0; i < TagCounter.Length; i++)
            {
                TagCounter[i] = 0;
            }
        }
    }
}
