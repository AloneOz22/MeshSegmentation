using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    class Triangle
    {
        public Node FirstNode { set => FirstNode = value; get => FirstNode; }
        public Node SecondNode { set => SecondNode = value; get => SecondNode; }
        public Node ThirdNode { set => ThirdNode = value; get => ThirdNode; }
        public Node Centre { set => Centre = value; get => Centre; }
        public int Num { set => Num = value; get => Num; }
        public int Tag { set => Tag = value; get => Tag; }
        public readonly int Type = 2;
        public Triangle(Node first, Node second, Node third, int num, int tag)
        {
            FirstNode = first;
            SecondNode = second;
            ThirdNode = third;
            Centre = new Node();
            Num = num;
            Tag = tag;
            Centre.X = ((first.X + second.X + third.X) / 3);
            Centre.Y = ((first.Y + second.Y + third.Y) / 3);
            Centre.Z = ((first.Z + second.Z + third.Z) / 3);
        }
        public Triangle(Node first, Node second, Node third, int num)
        {
            FirstNode = first;
            SecondNode = second;
            ThirdNode = third;
            Centre = new Node();
            Num = num;
            Tag = 0;
            Centre.X = (first.X + second.X + third.X) / 3;
            Centre.Y = (first.Y + second.Y + third.Y) / 3;
            Centre.Z = (first.Z + second.Z + third.Z) / 3;
        }
    }
}
