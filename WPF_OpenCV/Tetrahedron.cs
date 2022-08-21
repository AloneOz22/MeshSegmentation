using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    class Tetrahedron
    {
        public Node FirstNode { set => FirstNode = value; get => FirstNode; }
        public Node SecondNode { set => SecondNode = value; get => SecondNode; }
        public Node ThirdNode { set => ThirdNode = value; get => ThirdNode; }
        public Node FourthNode { set => FourthNode = value; get => FourthNode; }
        public Node Centre { set => Centre = value; get => Centre; }
        public int Num { set => Num = value; get => Num; }
        public int Tag { set => Tag = value; get => Tag; }
        public readonly int Type = 4;
        public Tetrahedron(Node first, Node second, Node third, Node fourth, int num, int tag)
        {
            FirstNode = first;
            SecondNode = second;
            ThirdNode = third;
            FourthNode = fourth;
            Centre = new Node();
            Num = num;
            Tag = tag;
            Centre.X = (first.X + second.X + third.X + fourth.X) / 4;
            Centre.Y = (first.Y + second.Y + third.Y + fourth.Y) / 4;
            Centre.Z = (first.Z + second.Z + third.Z + fourth.Z) / 4;
        }
        public Tetrahedron(Node first, Node second, Node third, Node fourth, int num)
        {
            FirstNode = first;
            SecondNode = second;
            ThirdNode = third;
            FourthNode = fourth;
            Centre = new Node();
            Num = num;
            Tag = 0;
            Centre.X = (first.X + second.X + third.X + fourth.X) / 4;
            Centre.Y = (first.Y + second.Y + third.Y + fourth.Y) / 4;
            Centre.Z = (first.Z + second.Z + third.Z + fourth.Z) / 4;
        }
    }
}
