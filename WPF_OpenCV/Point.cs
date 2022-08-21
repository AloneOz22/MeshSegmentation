using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    class Point
    {
        public Node Node { set => Node = value; get => Node; }
        public int Num { set => Num = value; get => Num; }
        public int Tag { set => Tag = value; get => Tag; }
        public readonly int Type = 15;
        public Point(Node node, int num, int Tag)
        {
            Node = node;
            Num = num;
            Tag = Tag;
        }
        public Point(Node node, int num)
        {
            Node = node;
            Num = num;
            Tag = Node.Tag;
        }
    }
}
