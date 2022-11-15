using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace WpfApp
{
    class Mesh
    {
        private StreamReader Fin;
        private StreamWriter Fout;
        private List<Node> Nodes;
        private List<Element> Elements;
        private int nodeCount;
        private int elementCount;
        public Mesh(string path)
        {
            Fin = new StreamReader(path);
            string HelpString = "";
            do
            {
                HelpString = Fin.ReadLine();
            } while (HelpString != "$Nodes");
            HelpString = Fin.ReadLine();
            nodeCount = Convert.ToInt32(HelpString);
            Nodes = new List<Node>(nodeCount);
            for (int i = 0; i < Nodes.Capacity; i++)
            {
                NodesReading(i);
            }
            Fin.ReadLine();
            Fin.ReadLine();
            elementCount = Convert.ToInt32(Fin.ReadLine());
            Elements = new List<Element>(elementCount);
            for (int i = 0; i < elementCount; i++)
            {
                ElementsReading(i);
            }
            Fin.Close();
        }
        public void NodesReading(int i)
        {
            string cycleHelpString = Fin.ReadLine();
            string[] Subs = cycleHelpString.Split(' ');
            Subs[1] = Subs[1].Replace('.', ',');
            Subs[2] = Subs[2].Replace('.', ',');
            Subs[3] = Subs[3].Replace('.', ',');
            Nodes.Add(new Node(Convert.ToInt32(Subs[0]), Convert.ToDouble(Subs[1]), Convert.ToDouble(Subs[2]), Convert.ToDouble(Subs[3])));
        }
        public void ElementsReading(int i)
        {
            string[] Subs = Fin.ReadLine().Split(' ');
            switch (Convert.ToInt32(Subs[1]))
            {
                case 1:
                    Elements.Add(new Element(Nodes[Convert.ToInt32(Subs[5]) - 1], Nodes[Convert.ToInt32(Subs[6]) - 1], i + 1));
                    break;
                case 2:
                    Elements.Add(new Element(Nodes[Convert.ToInt32(Subs[5]) - 1], Nodes[Convert.ToInt32(Subs[6]) - 1], 
                        Nodes[Convert.ToInt32(Subs[7]) - 1], i + 1));
                    break;
                case 4:
                    Elements.Add(new Element(Nodes[Convert.ToInt32(Subs[5]) - 1], Nodes[Convert.ToInt32(Subs[6]) - 1],
                        Nodes[Convert.ToInt32(Subs[7]) - 1], Nodes[Convert.ToInt32(Subs[8]) - 1], i + 1));
                    break;
                case 15:
                    Elements.Add(new Element(Nodes[Convert.ToInt32(Subs[5]) - 1], i + 1));
                    break;
            }
        }
        public void NodeTagsInitialising(ListBox scanList, string height, string radius, int scanCount)
        {
            int scanCounter = 0;
            foreach (string fileName in scanList.Items)
            {
                Mat img = new Mat(fileName);
                Mat hsv = new Mat(img.Cols, img.Rows, 8, 3);
                Cv2.CvtColor(img, hsv, ColorConversionCodes.BGR2HSV);
                Mat[] splitedHsv = new Mat[3];
                Cv2.Split(hsv, out splitedHsv);
                height = height.Replace('.', ',');
                radius = radius.Replace('.', ',');
                double kernHeight = Convert.ToDouble(height);
                double scanRadius = Convert.ToDouble(radius);
                for (int i = 0; i < Nodes.Count; i++)
                {
                    double scanZPos = scanCounter * kernHeight / scanCount - 1;
                    if (Math.Abs(scanZPos - Nodes[i].Z) < kernHeight / (2 * (scanCount - 1)))
                    {
                        byte brightness = (byte)(splitedHsv[2].At<byte>((int)(Nodes[i].X / 
                            (scanRadius * 2) * img.Cols), (int)(Nodes[i].Y / (scanRadius * 2) * img.Rows)));
                        switch (brightness)
                        {
                            case 0:
                                Nodes[i].Tag = 0;
                                break;
                            case 150:
                                Nodes[i].Tag = 2;
                                break;
                            case 255:
                                Nodes[i].Tag = 4;
                                break;
                            default:
                                Nodes[i].Tag = 9;
                                break;
                        }
                    }
                }
                scanCounter++;
            }
        }
        public void ElementTagsInitialising()
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                byte[] tags;
                IEnumerable<byte> query;
                switch (Elements[i].Type)
                {
                    case 2:
                        tags = new byte[2];
                        switch (Elements[i].FirstNode.Tag)
                        {
                            case 0:
                                tags[0]++;
                                break;
                            case 2:
                                tags[1]++;
                                break;
                            case 4:
                                tags[2]++;
                                break;
                            default:

                                break;
                        }
                        switch (Elements[i].SecondNode.Tag)
                        {
                            case 0:
                                tags[0]++;
                                break;
                            case 2:
                                tags[1]++;
                                break;
                            case 4:
                                tags[2]++;
                                break;
                            default:

                                break;
                        }
                        switch (Elements[i].ThirdNode.Tag)
                        {
                            case 0:
                                tags[0]++;
                                break;
                            case 2:
                                tags[1]++;
                                break;
                            case 4:
                                tags[2]++;
                                break;
                            default:

                                break;
                        }
                        query = from value in tags
                                                    orderby tags descending
                                                    select value;
                        Elements[i].Tag = tags[0];
                        break;
                    case 4:
                        tags = new byte[3];
                        switch (Elements[i].FirstNode.Tag)
                        {
                            case 0:
                                tags[0]++;
                                break;
                            case 2:
                                tags[1]++;
                                break;
                            case 4:
                                tags[2]++;
                                break;
                            default:
                                tags[3]++;
                                break;
                        }
                        switch (Elements[i].SecondNode.Tag)
                        {
                            case 0:
                                tags[0]++;
                                break;
                            case 2:
                                tags[1]++;
                                break;
                            case 4:
                                tags[2]++;
                                break;
                            default:

                                break;
                        }
                        switch (Elements[i].ThirdNode.Tag)
                        {
                            case 0:
                                tags[0]++;
                                break;
                            case 2:
                                tags[1]++;
                                break;
                            case 4:
                                tags[2]++;
                                break;
                            default:

                                break;
                        }
                        switch (Elements[i].FourthNode.Tag)
                        {
                            case 0:
                                tags[0]++;
                                break;
                            case 2:
                                tags[1]++;
                                break;
                            case 4:
                                tags[2]++;
                                break;
                            default:

                                break;
                        }
                        query = from value in tags
                                                  orderby tags descending
                                                  select value;
                        Elements[i].Tag = tags[0];
                        break;
                    default:

                        break;
                }
            }
        }
        public void SaveMesh(string path)
        {
            Fout = new StreamWriter(path + @"\resmesh" + ".msh");
            Fout.WriteLine("$MeshFormat");
            Fout.WriteLine("2.2 0 8");
            Fout.WriteLine("$EndMeshFormat");
            Fout.WriteLine("$Nodes");
            Fout.WriteLine(Nodes.Count);
            for (int i = 0; i < Nodes.Count; i++)
            {
                int Num = i + 1;
                string X = Convert.ToString(Nodes[i].X);
                string Y = Convert.ToString(Nodes[i].Y);
                string Z = Convert.ToString(Nodes[i].Z);
                X = X.Replace(',', '.');
                Y = Y.Replace(',', '.');
                Z = Z.Replace(',', '.');
                Fout.WriteLine(Nodes[i].Num + " " + X + " " + Y + " " + Z);
            }
            Fout.WriteLine("$EndNodes");
            Fout.WriteLine("$Elements");
            Fout.WriteLine(Elements.Count);
            for (int i = 0; i < Elements.Count; i++)
            {
                int Num = i + 1;
                switch (Elements[i].Type)
                {
                    case 15:
                        Fout.WriteLine(Elements[i].Num + " " + Elements[i].Type + " " + 2 + " " + Elements[i].Tag + " " + Elements[i].Tag + " " + Elements[i].FirstNode.Num);
                        break;
                    case 1:
                        Fout.WriteLine(Elements[i].Num + " " + Elements[i].Type + " " + 2 + " " + Elements[i].Tag + " " + Elements[i].Tag + " " + Elements[i].FirstNode.Num + " " + Elements[i].SecondNode.Num);
                        break;
                    case 2:
                        Fout.WriteLine(Num + " " + Elements[i].Type + " " + 2 + " " + Elements[i].Tag + " " + Elements[i].Tag + " " + Elements[i].FirstNode.Num + " " + Elements[i].SecondNode.Num + " " + Elements[i].ThirdNode.Num);
                        break;
                    case 4:
                        Fout.WriteLine(Elements[i].Num + " " + Elements[i].Type + " " + 2 + " " + Elements[i].Tag + " " + Elements[i].Tag + " " + Elements[i].FirstNode.Num + " " + Elements[i].SecondNode.Num + " " + Elements[i].ThirdNode.Num + " " + Elements[i].FourthNode.Num);
                        break;
                }
            }
            Fout.WriteLine("$EndElements");
            Fout.Close();
        }
    }
}
