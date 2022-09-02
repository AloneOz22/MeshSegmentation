using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    class Element
    {
        public Node FirstNode;
        public Node SecondNode;
        public Node ThirdNode;
        public Node FourthNode;
        public Node Centre;
        public int Num;
        public int Tag;
        public readonly int Type;
        public Element(Node first, Node second, Node third, Node fourth, int num, int tag)
        {
            FirstNode = first;
            SecondNode = second;
            ThirdNode = third;
            FourthNode = fourth;
            Centre = new Node();
            Num = num;
            Tag = tag;
            Type = 4;
            Centre.X = (first.X + second.X + third.X + fourth.X) / 4;
            Centre.Y = (first.Y + second.Y + third.Y + fourth.Y) / 4;
            Centre.Z = (first.Z + second.Z + third.Z + fourth.Z) / 4;
        }
        public Element(Node first, Node second, Node third, Node fourth, int num)
        {
            FirstNode = first;
            SecondNode = second;
            ThirdNode = third;
            FourthNode = fourth;
            Centre = new Node();
            Num = num;
            Tag = 0;
            Type = 4;
            Centre.X = (first.X + second.X + third.X + fourth.X) / 4;
            Centre.Y = (first.Y + second.Y + third.Y + fourth.Y) / 4;
            Centre.Z = (first.Z + second.Z + third.Z + fourth.Z) / 4;
        }
        public Element(Node first, Node second, Node third, int num, int tag)
        {
            FirstNode = first;
            SecondNode = second;
            ThirdNode = third;
            Centre = new Node();
            Num = num;
            Tag = tag;
            Type = 2;
            Centre.X = ((first.X + second.X + third.X) / 3);
            Centre.Y = ((first.Y + second.Y + third.Y) / 3);
            Centre.Z = ((first.Z + second.Z + third.Z) / 3);
        }
        public Element(Node first, Node second, Node third, int num)
        {
            FirstNode = first;
            SecondNode = second;
            ThirdNode = third;
            Centre = new Node();
            Num = num;
            Tag = 0;
            Type = 2;
            Centre.X = (first.X + second.X + third.X) / 3;
            Centre.Y = (first.Y + second.Y + third.Y) / 3;
            Centre.Z = (first.Z + second.Z + third.Z) / 3;
        }
        public Element(Node first, Node second, int num, int tag)
        {
            FirstNode = first;
            SecondNode = second;
            Centre = new Node();
            Num = num;
            Tag = tag;
            Type = 1;
            Centre.X = (first.X + second.X) / 2;
            Centre.Y = (first.Y + second.Y) / 2;
            Centre.Z = (first.Z + second.Z) / 2;
        }
        public Element(Node first, Node second, int num)
        {
            FirstNode = first;
            SecondNode = second;
            Centre = new Node();
            Num = num;
            Tag = 0;
            Type = 1;
            Centre.X = (first.X + second.X) / 2;
            Centre.Y = (first.Y + second.Y) / 2;
            Centre.Z = (first.Z + second.Z) / 2;
        }
        public Element(Node node, int num, int tag)
        {
            FirstNode = node;
            Num = num;
            Tag = tag;
            Type = 15;
            Centre = node;
        }
        public Element(Node node, int num)
        {
            FirstNode = node;
            Num = num;
            Tag = node.Tag;
            Type = 15;
            Centre = node;
        }
    }
}
