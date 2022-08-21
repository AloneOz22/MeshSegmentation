using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    class Line
    {
        public Node FirstNode { set => FirstNode = value; get => FirstNode; }
        public Node SecondNode { set => SecondNode = value; get => SecondNode; }
        public Node Centre { set => Centre = value; get => Centre; }
        public int Num { set => Num = value; get => Num; }
        public int Tag { set => Tag = value; get => Tag; }
        public readonly int Type = 1;
        public Line(Node first, Node second, int num, int tag)
        {
            FirstNode = first;
            SecondNode = second;
            Centre = new Node();
            Num = num;
            Tag = tag;
            Centre.X = (first.X + second.X) / 2;
            Centre.Y = (first.Y + second.Y) / 2;
            Centre.Z = (first.Z + second.Z) / 2;
        }
        public Line(Node first, Node second, int num)
        {
            FirstNode = first;
            SecondNode = second;
            Centre = new Node();
            Num = num;
            Tag = 0;
            Centre.X = (first.X + second.X) / 2;
            Centre.Y = (first.Y + second.Y) / 2;
            Centre.Z = (first.Z + second.Z) / 2;
        }
    }
}
