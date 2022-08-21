using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using System.Data;
using System.Threading;
using OpenCvSharp;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : System.Windows.Window
    {
        const double EPS = 0.00000000000001;
        public MainWindow()
        {
            InitializeComponent();
            Console_block.Text = "Ready";
        }
        private void Cube_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton pressed = (System.Windows.Controls.RadioButton)sender;
        }
        private void Cylinder_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton pressed = (System.Windows.Controls.RadioButton)sender;
        }
        void segmentation()
        {
            try
            {
                SegmentedImagesList.Items.Clear();
                //CT_Scan.Source = BitmapFrame.Create(new Uri((String)ImagesList.Items[0]));
                //StreamWriter fout = new StreamWriter(data_path.Text);
                //fout.WriteLine("Сюда записались какие-то данные, но я пока не знаю как их вычленить");
                //fout.Close();
                //Console_block.Text += "\nНовое изображение загружено";
                int k = 0;
                foreach (string fileName in ImagesList.Items)
                {
                    Mat img1 = new Mat(fileName);
                    Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
                    Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
                    Mat[] splitedHsv = new Mat[3];
                    Cv2.Split(hsv, out splitedHsv);
                    CT_Scan_Orig.Source = BitmapFrame.Create(new Uri(fileName));
                    for (int x = 0; x < hsv.Rows; x++)
                    {
                        for (int y = 0; y < hsv.Cols; y++)
                        {
                            int H = (int)(splitedHsv[0].At<byte>(x, y));        // Тон
                            int S = (int)(splitedHsv[1].At<byte>(x, y));        // Интенсивность
                            int V = (int)(splitedHsv[2].At<byte>(x, y));        // Яркость
                            if (V >= Convert.ToInt32(LowScope.Text) && V < Convert.ToInt32(MidScope.Text))
                            {
                                img1.At<Vec3b>(x, y)[0] = 0;
                                img1.At<Vec3b>(x, y)[1] = 0;
                                img1.At<Vec3b>(x, y)[2] = 0;
                            }
                            else
                                if (V >= Convert.ToInt32(MidScope.Text) && V < Convert.ToInt32(TopScope.Text))
                            {
                                img1.At<Vec3b>(x, y)[0] = 150;
                                img1.At<Vec3b>(x, y)[1] = 150;
                                img1.At<Vec3b>(x, y)[2] = 150;
                            }
                            else
                            {
                                img1.At<Vec3b>(x, y)[0] = 255;
                                img1.At<Vec3b>(x, y)[1] = 255;
                                img1.At<Vec3b>(x, y)[2] = 255;
                            }
                        }
                    }
                    Cv2.ImWrite(scans_path.Text + @"\res" + k + ".tif", img1);
                    CT_Scan.Source = BitmapFrame.Create(new Uri((String)scans_path.Text + @"\res" + k + ".tif"));
                    SegmentedImagesList.Items.Add(scans_path.Text + @"\res" + k + ".tif");
                    Console_block.Text += "\nNew scan segmented";
                    num_of_scans.Text = "Elements: " + Convert.ToString(ImagesList.Items.Count);
                    k++;
                }
            }
            catch (Exception e1)
            {
                System.Windows.MessageBox.Show("Error! " + e1.Message);
            }
        }
        void SquareGeoCreating()
        {
            try
            {
                string alpha = "0.002";
                StreamWriter fout = new StreamWriter(geometry_path.Text + @"\res" + ".geo");
                height.Text = height.Text.Replace('.', ',');
                radius.Text = radius.Text.Replace('.', ',');
                float RealRadius = (float)Convert.ToDouble(radius.Text);
                float Diameter = RealRadius * 2.0f;
                string StringDiameter = Convert.ToString(Diameter);
                height.Text = height.Text.Replace(',', '.');
                radius.Text = radius.Text.Replace(',', '.');
                StringDiameter = StringDiameter.Replace(',', '.');
                float PointParam = 0.001f;
                string Transfinition = Convert.ToString(PointParam);
                Transfinition = Transfinition.Replace(',', '.');
                fout.WriteLine("Point(1) = { 0, 0, 0, " + alpha + "};");
                fout.WriteLine("Point(2) = { 0.06, 0, 0, " + alpha + "};");
                fout.WriteLine("Point(3) = { 0.06, 0.06, 0, " + alpha + "};");
                fout.WriteLine("Point(4) = { 0, 0.06, 0, " + alpha + "};");
                fout.WriteLine("Line(1) = { 1, 2};");
                fout.WriteLine("Line(2) = { 2, 3};");
                fout.WriteLine("Line(3) = { 3, 4};");
                fout.WriteLine("Line(4) = { 4, 1};");
                fout.WriteLine("Line Loop(1) = { 1, 2, 3, 4};");
                fout.WriteLine("Plane Surface(1) = { 1};");
                fout.Close();
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("Error! " + e.Message);
            }
        }
        void LayerMeshCreating()
        {
            try
            {
                StreamReader Fin = new StreamReader((string)MeshesList.Items[0]);
                string HelpString = "";
                do
                {
                    HelpString = Fin.ReadLine();
                } while (HelpString != "$Nodes");
                HelpString = Fin.ReadLine();
                int nodeNum = Convert.ToInt32(HelpString);
                List<Node> Nodes = new List<Node>(nodeNum);               
                for (int i = 0; i < Nodes.Capacity; i++)
                {
                    HelpString = Fin.ReadLine();
                    string[] Subs = HelpString.Split(' ');
                    Subs[1] = Subs[1].Replace('.', ',');
                    Subs[2] = Subs[2].Replace('.', ',');
                    Subs[3] = Subs[3].Replace('.', ',');
                    Nodes.Add(new Node(Convert.ToInt32(Subs[0]), Convert.ToDouble(Subs[1]), Convert.ToDouble(Subs[2]), Convert.ToDouble(Subs[3])));                                     
                }
                Fin.ReadLine();
                Fin.ReadLine();
                int elementNum = Convert.ToInt32(Fin.ReadLine());
                List<Element> Elements = new List<Element>(elementNum);
                for (int i = 0; i < elementNum; i++)
                {
                    string[] Subs = Fin.ReadLine().Split(' ');
                    switch (Convert.ToInt32(Subs[1]))
                    {
                        case 1:
                            Elements.Add(new Element(Nodes[Convert.ToInt32(Subs[5]) - 1], Nodes[Convert.ToInt32(Subs[6]) - 1], i + 1));
                            break;
                        case 2:
                            Elements.Add(new Element(Nodes[Convert.ToInt32(Subs[5]) - 1], Nodes[Convert.ToInt32(Subs[6]) - 1], Nodes[Convert.ToInt32(Subs[7]) - 1], i + 1));
                            break;
                        case 15:
                            Elements.Add(new Element(Nodes[Convert.ToInt32(Subs[5]) - 1], i + 1));
                            break;
                    }
                }                              
                Fin.Close();
                int k = 0;
                height.Text = height.Text.Replace('.', ',');
                double Height = Convert.ToDouble(height.Text);                
                Nodes.Capacity *= SegmentedImagesList.Items.Count;
                Elements.Capacity *= SegmentedImagesList.Items.Count;
                for (int i = nodeNum; i < Nodes.Capacity; i++)
                {
                    if (i % nodeNum == 0)
                    {
                        k++;
                    }
                    Nodes.Add(new Node(i + 1, Nodes[i - nodeNum].X, Nodes[i - nodeNum].Y, Height / SegmentedImagesList.Items.Count * k));                    
                }
                for (int i = elementNum; i < Elements.Capacity; i++)
                {                    
                    switch (Elements[i - elementNum].Type)
                    {
                        case 1:
                            Elements.Add(new Element(Nodes[Elements[i - elementNum].FirstNode.Num + nodeNum - 1], Nodes[Elements[i - elementNum].SecondNode.Num + nodeNum - 1], i + 1));
                            break;
                        case 2:
                            Elements.Add(new Element(Nodes[Elements[i - elementNum].FirstNode.Num + nodeNum - 1], Nodes[Elements[i - elementNum].SecondNode.Num + nodeNum - 1], Nodes[Elements[i - elementNum].ThirdNode.Num + nodeNum - 1], i + 1));
                            break;
                        case 15:
                            Elements.Add(new Element(Nodes[Elements[i - elementNum].FirstNode.Num + nodeNum - 1], i + 1));
                            break;
                    }
                }                              
                k = 0;
                foreach (string fileName in SegmentedImagesList.Items)
                {
                    Mat Img = new Mat(fileName);
                    Mat HSV = new Mat(Img.Cols, Img.Rows, 8, 3);
                    Cv2.CvtColor(Img, HSV, ColorConversionCodes.BGR2HSV);
                    Mat[] splitedHsv = new Mat[3];
                    Cv2.Split(HSV, out splitedHsv);
                    height.Text = height.Text.Replace('.', ',');
                    radius.Text = radius.Text.Replace('.', ',');
                    double z_scan = Convert.ToDouble(height.Text);
                    double scan_radius = Convert.ToDouble(radius.Text);
                    for (int i = 0; i < Elements.Count; i++)
                    {
                        double h_centre = Elements[i].Centre.Z;
                        double scan_pos = k * z_scan / SegmentedImagesList.Items.Count;
                        if (Math.Abs(scan_pos - h_centre) < EPS)
                        {
                            switch (Elements[i].Type)
                            {
                                case 1:
                                    {
                                        byte[] Brightness = new byte[3];
                                        Brightness[0] = (byte)(splitedHsv[2].At<byte>((int)Math.Abs((Elements[i].FirstNode.X / (scan_radius * 2) * Img.Cols) - EPS), (int)Math.Abs((Elements[i].FirstNode.Y / (scan_radius * 2) * Img.Rows) - EPS)));
                                        Brightness[1] = (byte)(splitedHsv[2].At<byte>((int)Math.Abs((Elements[i].SecondNode.X / (scan_radius * 2) * Img.Cols) - EPS), (int)Math.Abs((Elements[i].SecondNode.Y / (scan_radius * 2) * Img.Rows) - EPS)));
                                        Brightness[2] = (byte)(splitedHsv[2].At<byte>((int)Math.Abs((Elements[i].Centre.X / (scan_radius * 2) * Img.Cols) - EPS), (int)Math.Abs((Elements[i].Centre.Y / (scan_radius * 2) * Img.Rows) - EPS)));
                                        byte[] PhysicalGroups = new byte[3];
                                        for (int Group = 0; Group < 3; Group++)
                                            PhysicalGroups[Group] = 0;
                                        for (int index = 0; index < 3; index++)
                                        {
                                            if (Brightness[index] == 0)
                                                PhysicalGroups[0]++;
                                            if (Brightness[index] == 150)
                                                PhysicalGroups[1]++;
                                            if (Brightness[index] == 255)
                                                PhysicalGroups[2]++;
                                        }
                                        if (PhysicalGroups[0] > PhysicalGroups[1] + PhysicalGroups[2])
                                        {
                                            Elements[i].Tag = 0;
                                        }
                                        else if (PhysicalGroups[1] > PhysicalGroups[0] + PhysicalGroups[2])
                                        {
                                            Elements[i].Tag = 1;
                                        }
                                        else
                                        {
                                            Elements[i].Tag = 2;
                                        }

                                        break;
                                    }

                                case 2:
                                    {
                                        byte[] Brightness = new byte[4];
                                        Brightness[0] = (byte)(splitedHsv[2].At<byte>((int)Math.Abs((Elements[i].FirstNode.X / (scan_radius * 2) * Img.Cols) - EPS), (int)Math.Abs((Elements[i].FirstNode.Y / (scan_radius * 2) * Img.Rows) - EPS)));
                                        Brightness[1] = (byte)(splitedHsv[2].At<byte>((int)Math.Abs((Elements[i].SecondNode.X / (scan_radius * 2) * Img.Cols) - EPS), (int)Math.Abs((Elements[i].SecondNode.Y / (scan_radius * 2) * Img.Rows) - EPS)));
                                        Brightness[2] = (byte)(splitedHsv[2].At<byte>((int)Math.Abs((Elements[i].Centre.X / (scan_radius * 2) * Img.Cols) - EPS), (int)Math.Abs((Elements[i].Centre.Y / (scan_radius * 2) * Img.Rows) - EPS)));
                                        Brightness[3] = (byte)(splitedHsv[2].At<byte>((int)Math.Abs((Elements[i].ThirdNode.X / (scan_radius * 2) * Img.Cols) - EPS), (int)Math.Abs((Elements[i].ThirdNode.Y / (scan_radius * 2) * Img.Rows) - EPS)));
                                        byte[] PhysicalGroups = new byte[3];
                                        for (int Group = 0; Group < 3; Group++)
                                            PhysicalGroups[Group] = 0;
                                        for (int index = 0; index < 4; index++)
                                        {
                                            if (Brightness[index] == 0)
                                                PhysicalGroups[0]++;
                                            if (Brightness[index] == 150)
                                                PhysicalGroups[1]++;
                                            if (Brightness[index] == 255)
                                                PhysicalGroups[2]++;
                                        }
                                        if (PhysicalGroups[0] > PhysicalGroups[1] + PhysicalGroups[2])
                                        {
                                            Elements[i].Tag = 0;
                                        }
                                        else if (PhysicalGroups[1] > PhysicalGroups[0] + PhysicalGroups[2])
                                        {
                                            Elements[i].Tag = 1;
                                        }
                                        else
                                        {
                                            Elements[i].Tag = 2;
                                        }

                                        break;
                                    }

                                case 15:
                                    {
                                        int V = (int)(splitedHsv[2].At<byte>((int)Math.Abs((Elements[i].FirstNode.X / (scan_radius * 2) * Img.Cols) - EPS), (int)Math.Abs((Elements[i].FirstNode.Y / (scan_radius * 2) * Img.Rows) - EPS)));
                                        if (V == 0)
                                        {
                                            Elements[i].Tag = 0;
                                        }
                                        else if (V == 150)
                                        {
                                            Elements[i].Tag = 1;
                                        }
                                        else if (V == 255)
                                        {
                                            Elements[i].Tag = 2;
                                        }

                                        break;
                                    }
                            }
                        }
                    }
                    k++;
                }
                for(int i = 0; i < Elements.Count; i++)
                {
                    switch (Elements[i].Type)
                    {
                        case 15:
                            switch (Elements[i].Tag)
                            {
                                case 0:
                                    Elements[i].FirstNode.TagCounter[0]++;
                                    break;
                                case 1:
                                    Elements[i].FirstNode.TagCounter[1]++;
                                    break;
                                case 2:
                                    Elements[i].FirstNode.TagCounter[2]++;
                                    break;
                            }
                            break;
                        case 1:
                            switch (Elements[i].Tag)
                            {
                                case 0:
                                    Elements[i].FirstNode.TagCounter[0]++;
                                    Elements[i].SecondNode.TagCounter[0]++;
                                    break;
                                case 1:
                                    Elements[i].FirstNode.TagCounter[1]++;
                                    Elements[i].SecondNode.TagCounter[1]++;
                                    break;
                                case 2:
                                    Elements[i].FirstNode.TagCounter[2]++;
                                    Elements[i].SecondNode.TagCounter[2]++;
                                    break;
                            }
                            break;
                        case 2:
                            switch (Elements[i].Tag)
                            {
                                case 0:
                                    Elements[i].FirstNode.TagCounter[0]++;
                                    Elements[i].SecondNode.TagCounter[0]++;
                                    Elements[i].ThirdNode.TagCounter[0]++;
                                    break;
                                case 1:
                                    Elements[i].FirstNode.TagCounter[1]++;
                                    Elements[i].SecondNode.TagCounter[1]++;
                                    Elements[i].ThirdNode.TagCounter[1]++;
                                    break;
                                case 2:
                                    Elements[i].FirstNode.TagCounter[2]++;
                                    Elements[i].SecondNode.TagCounter[2]++;
                                    Elements[i].ThirdNode.TagCounter[2]++;
                                    break;
                            }
                            break;
                    }
                }
                for (int i = 0; i < Nodes.Count; i++)
                {
                    if (Nodes[i].TagCounter[0] > Nodes[i].TagCounter[1] && Nodes[i].TagCounter[0] > Nodes[i].TagCounter[2])
                    {
                        Nodes[i].Tag = 0;
                    }
                    else if (Nodes[i].TagCounter[1] > Nodes[i].TagCounter[0] && Nodes[i].TagCounter[1] > Nodes[i].TagCounter[2])
                    {
                        Nodes[i].Tag = 1;
                    }
                    else if (Nodes[i].TagCounter[2] > Nodes[i].TagCounter[0] && Nodes[i].TagCounter[2] > Nodes[i].TagCounter[1])
                    {
                        Nodes[i].Tag = 2;
                    }
                    else if (Nodes[i].TagCounter[0] == Nodes[i].TagCounter[1] && Nodes[i].TagCounter[0] > Nodes[i].TagCounter[2])
                    {
                        Nodes[i].Tag = 1; // Граница 1 и 2 фаз
                    }
                    else if (Nodes[i].TagCounter[0] == Nodes[i].TagCounter[2] && Nodes[i].TagCounter[0] > Nodes[i].TagCounter[1])
                    {
                        Nodes[i].Tag = 2; // Граница 1 и 3 фаз
                    }
                    else if (Nodes[i].TagCounter[1] == Nodes[i].TagCounter[2] && Nodes[i].TagCounter[1] > Nodes[i].TagCounter[0])
                    {
                        Nodes[i].Tag = 2; // Граница 2 и 3 фаз
                    }
                    else
                    {
                        Nodes[i].Tag = 0; // Граница всех фаз
                    }
                }
                int OldCount = Elements.Count;
                for (int i = elementNum; i < OldCount; i++)
                {
                    if (Elements[i].Tag == Elements[i - elementNum].Tag && Elements[i].Type == 2 && Elements[i - elementNum].Type == 2)
                    {
                        Elements.Capacity += 3;                
                        Elements.Add(new Element(Elements[i].FirstNode, Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i - elementNum].FirstNode, Elements.Count, Elements[i].Tag));
                        Elements.Add(new Element(Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements[i - elementNum].ThirdNode, Elements[i].ThirdNode, Elements.Count, Elements[i].Tag));
                        Elements.Add(new Element(Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements.Count, Elements[i].Tag));                                            
                    }
                    else if (Elements[i].Tag != Elements[i - elementNum].Tag && Elements[i].Type == 2 && Elements[i - elementNum].Type == 2)
                    {
                        List<bool> ThisNodesTags = new List<bool>(3);
                        if (Elements[i].FirstNode.Tag == Elements[i].Tag)
                        {
                            ThisNodesTags.Add(true);
                        }
                        else
                        {
                            ThisNodesTags.Add(false);
                        }
                        if (Elements[i].SecondNode.Tag == Elements[i].Tag)
                        {
                            ThisNodesTags.Add(true);
                        }
                        else
                        {
                            ThisNodesTags.Add(false);
                        }
                        if (Elements[i].ThirdNode.Tag == Elements[i].Tag)
                        {
                            ThisNodesTags.Add(true);
                        }
                        else
                        {
                            ThisNodesTags.Add(false);
                        }
                        NewTetrahedron(ref Elements, TetrahedronsConfiguration(ThisNodesTags), i, elementNum);
                    }
                }
                StreamWriter Fout = new StreamWriter(meshes_path.Text + @"\resmesh" + ".msh");
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
                    Fout.WriteLine(Num + " " + X + " " + Y + " " + Z);
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
                            Fout.WriteLine(Num + " " + Elements[i].Type + " " + 2 + " " + Elements[i].Tag + " " + Elements[i].Tag + " " + Elements[i].FirstNode.Num);
                            break;
                        case 1:
                            Fout.WriteLine(Num + " " + Elements[i].Type + " " + 2 + " " + Elements[i].Tag + " " + Elements[i].Tag + " " + Elements[i].FirstNode.Num + " " + Elements[i].SecondNode.Num);
                            break;
                        case 2:
                            Fout.WriteLine(Num + " " + Elements[i].Type + " " + 2 + " " + Elements[i].Tag + " " + Elements[i].Tag + " " + Elements[i].FirstNode.Num + " " + Elements[i].SecondNode.Num + " " + Elements[i].ThirdNode.Num);
                            break;
                        case 4:
                            Fout.WriteLine(Num + " " + Elements[i].Type + " " + 2 + " " + Elements[i].Tag + " " + Elements[i].Tag + " " + Elements[i].FirstNode.Num + " " + Elements[i].SecondNode.Num + " " + Elements[i].ThirdNode.Num + " " + Elements[i].FourthNode.Num);
                            break;
                    }
                }
                Fout.WriteLine("$EndElements");
                Fout.Close();
                Console_block.Text += "\nNew mesh segmented;";
                SegmentedMeshesList.Items.Add(meshes_path.Text + @"\resmesh" + ".msh");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error! " + e.Message);
            }
        }
        byte TetrahedronsConfiguration(List<bool> Config)
        {
            try
            {
                byte Result = 0;
                switch (Config[0])
                {
                    case true when Config[1] == true && Config[2] == true:
                        Result = 0;
                        break;
                    case true when Config[1] == true && Config[2] == false:
                        Result = 1;
                        break;
                    case true when Config[1] == false && Config[2] == true:
                        Result = 2;
                        break;
                    case false when Config[1] == true && Config[2] == true:
                        Result = 3;
                        break;
                    case true when Config[1] == false && Config[2] == false:
                        Result = 4;
                        break;
                    case false when Config[1] == true && Config[2] == false:
                        Result = 5;
                        break;
                    case false when Config[1] == false && Config[2] == true:
                        Result = 6;
                        break;
                    case false when Config[1] == false && Config[2] == false:
                        Result = 7;
                        break;
                }
                return Result;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error! " + e.Message);
                return 10;
            }
        }
        void NewTetrahedron(ref List<Element> Elements, byte Config, int i, int elementNum)
        {
            try
            {
                switch (Config)
                {
                    case 1:
                        Elements.Capacity += 3;
                        Elements.Add(new Element(Elements[i].FirstNode, Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i - elementNum].FirstNode, Elements.Count, Elements[i].Tag));
                        Elements.Add(new Element(Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements[i - elementNum].ThirdNode, Elements[i].ThirdNode, Elements.Count, Elements[i - elementNum].Tag));
                        Elements.Add(new Element(Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements.Count, Elements[i].Tag));
                        break;
                    case 2:
                        Elements.Capacity += 3;
                        Elements.Add(new Element(Elements[i].ThirdNode, Elements[i].FirstNode, Elements[i].SecondNode, Elements[i - elementNum].ThirdNode, Elements.Count, Elements[i].Tag));
                        Elements.Add(new Element(Elements[i - elementNum].ThirdNode, Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements[i].SecondNode, Elements.Count, Elements[i - elementNum].Tag));
                        Elements.Add(new Element(Elements[i].FirstNode, Elements[i].SecondNode, Elements[i - elementNum].ThirdNode, Elements[i - elementNum].FirstNode, Elements.Count, Elements[i].Tag));
                        break;
                    case 3:
                        Elements.Capacity += 3;
                        Elements.Add(new Element(Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i].FirstNode, Elements[i - elementNum].SecondNode, Elements.Count, Elements[i].Tag));
                        Elements.Add(new Element(Elements[i - elementNum].SecondNode, Elements[i - elementNum].ThirdNode, Elements[i - elementNum].FirstNode, Elements[i].FirstNode, Elements.Count, Elements[i - elementNum].Tag));
                        Elements.Add(new Element(Elements[i].ThirdNode, Elements[i].FirstNode, Elements[i - elementNum].SecondNode, Elements[i - elementNum].ThirdNode, Elements.Count, Elements[i].Tag));
                        break;
                    case 4:
                        Elements.Capacity += 3;
                        Elements.Add(new Element(Elements[i].FirstNode, Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i - elementNum].FirstNode, Elements.Count, Elements[i].Tag));
                        Elements.Add(new Element(Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements[i - elementNum].ThirdNode, Elements[i].ThirdNode, Elements.Count, Elements[i - elementNum].Tag));
                        Elements.Add(new Element(Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements.Count, Elements[i - elementNum].Tag));
                        break;
                    case 5:
                        Elements.Capacity += 3;
                        Elements.Add(new Element(Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i].FirstNode, Elements[i - elementNum].SecondNode, Elements.Count, Elements[i].Tag));
                        Elements.Add(new Element(Elements[i - elementNum].SecondNode, Elements[i - elementNum].ThirdNode, Elements[i - elementNum].FirstNode, Elements[i].FirstNode, Elements.Count, Elements[i - elementNum].Tag));
                        Elements.Add(new Element(Elements[i].ThirdNode, Elements[i].FirstNode, Elements[i - elementNum].SecondNode, Elements[i - elementNum].ThirdNode, Elements.Count, Elements[i - elementNum].Tag));
                        break;
                    case 6:
                        Elements.Capacity += 3;
                        Elements.Add(new Element(Elements[i].ThirdNode, Elements[i].FirstNode, Elements[i].SecondNode, Elements[i - elementNum].ThirdNode, Elements.Count, Elements[i].Tag));
                        Elements.Add(new Element(Elements[i - elementNum].ThirdNode, Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements[i].SecondNode, Elements.Count, Elements[i - elementNum].Tag));
                        Elements.Add(new Element(Elements[i].FirstNode, Elements[i].SecondNode, Elements[i - elementNum].ThirdNode, Elements[i - elementNum].FirstNode, Elements.Count, Elements[i - elementNum].Tag));
                        break;
                    case 7:
                        Elements.Capacity += 3;
                        Elements.Add(new Element(Elements[i].FirstNode, Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i - elementNum].FirstNode, Elements.Count, Elements[i - elementNum].Tag));
                        Elements.Add(new Element(Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements[i - elementNum].ThirdNode, Elements[i].ThirdNode, Elements.Count, Elements[i - elementNum].Tag));
                        Elements.Add(new Element(Elements[i].SecondNode, Elements[i].ThirdNode, Elements[i - elementNum].FirstNode, Elements[i - elementNum].SecondNode, Elements.Count, Elements[i - elementNum].Tag));
                        break;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error! " + e.Message);
            }
        }
        void filter()
        {
            int l = 0;
            int o = 0;
            string[] filenames = new string[SegmentedImagesList.Items.Count];
            foreach(string fileName in SegmentedImagesList.Items)
            {
                filenames[o] = fileName;
                o++;
            }
            SegmentedImagesList.Items.Clear();
            foreach(string fileName in filenames)
            {
                Mat img1 = new Mat(fileName);
                Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
                Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
                Mat[] splitedHsv = new Mat[3];
                Cv2.Split(hsv, out splitedHsv);
                CT_Scan_Orig.Source = BitmapFrame.Create(new Uri(fileName));
                for (int x = 0; x < hsv.Rows; x++)
                {
                    for (int y = 0; y < hsv.Cols; y++)
                    {
                        if (x != 0 && y != 0 && x != img1.Cols - 1 && y != img1.Rows - 1)
                        {
                            int[][] filter_matrix = new int[3][];
                            int main_V = 0;
                            for (int i = 0; i < 3; i++)
                            {
                                filter_matrix[i] = new int[3];
                                for (int j = 0; j < 3; j++)
                                {
                                    filter_matrix[i][j] = (int)(splitedHsv[2].At<byte>(x - 1 + i, y - 1 + j));
                                    main_V += filter_matrix[i][j];
                                }
                            }
                            main_V /= 9;
                            if (main_V < 150)
                            {
                                img1.At<Vec3b>(x, y)[0] = 0;
                                img1.At<Vec3b>(x, y)[1] = 0;
                                img1.At<Vec3b>(x, y)[2] = 0;
                            }
                            else if (main_V < 255)
                            {
                                img1.At<Vec3b>(x, y)[0] = 150;
                                img1.At<Vec3b>(x, y)[1] = 150;
                                img1.At<Vec3b>(x, y)[2] = 150;
                            }
                            else
                            {
                                img1.At<Vec3b>(x, y)[0] = 255;
                                img1.At<Vec3b>(x, y)[1] = 255;
                                img1.At<Vec3b>(x, y)[2] = 255;
                            }
                        }
                    }
                }
                Cv2.ImWrite(scans_path.Text + @"\res_filtered" + l + ".tif", img1);
                CT_Scan.Source = BitmapFrame.Create(new Uri((String)scans_path.Text + @"\res_filtered" + l + ".tif"));
                SegmentedImagesList.Items.Add(scans_path.Text + @"\res_filtered" + l + ".tif");
                //Console_block.Text += "\nNew scan filtered";
                //num_of_scans.Text = "Elements: " + Convert.ToString(ImagesList.Items.Count);
                l++;
            }
        }
        void CylinderMeshCreating()
        {
            try
            {
                StreamWriter fout = new StreamWriter(geometry_path.Text + @"\res" + ".geo");
                int k = 0;
                height.Text = height.Text.Replace('.', ',');
                radius.Text = radius.Text.Replace('.', ',');
                float real_radius = (float)Convert.ToDouble(radius.Text);
                float diameter = real_radius * 2.0f;
                string string_diameter = Convert.ToString(diameter);
                float z = 0.0f + (k * ((float)Convert.ToDouble(height.Text) / (float)SegmentedImagesList.Items.Count));
                string string_z = z.ToString();
                string_z = string_z.Replace(',', '.');
                height.Text = height.Text.Replace(',', '.');
                radius.Text = radius.Text.Replace(',', '.');
                string_diameter = string_diameter.Replace(',', '.');
                float point_param = 0.001f;
                String Transfinition = Convert.ToString(point_param);
                Transfinition = Transfinition.Replace(',', '.');
                fout.WriteLine("Point(1) = {" + radius.Text + ", 0, 0, " + Transfinition + "};");
                fout.WriteLine("Point(2) = {" + radius.Text + ", " + radius.Text + ", 0, " + Transfinition + "};");
                fout.WriteLine("Point(3) = {" + radius.Text + ", " + string_diameter + ", 0, " + Transfinition + "};");
                fout.WriteLine("Point(4) = {" + radius.Text + ", 0, " + height.Text + ", " + Transfinition + "};");
                fout.WriteLine("Point(5) = {" + radius.Text + ", " + radius.Text + ", " + height.Text + ", " + Transfinition + "};");
                fout.WriteLine("Point(6) = {" + radius.Text + ", " + string_diameter + ", " + height.Text + ", " + Transfinition + "};");
                fout.WriteLine("Circle(1) = {1, 2, 3};");
                fout.WriteLine("Circle(2) = {3, 2, 1};");
                fout.WriteLine("Circle(3) = {4, 5, 6};");
                fout.WriteLine("Circle(4) = {6, 5, 4};");
                fout.WriteLine("Spline(5) = {1, 4};");
                fout.WriteLine("Spline(6) = {3, 6};");
                fout.WriteLine("Line Loop(1) = {2, 1};");
                fout.WriteLine("Plane Surface(1) = {1};");
                fout.WriteLine("Line Loop(2) = {4, 3};");
                fout.WriteLine("Plane Surface(2) = {2};");
                fout.WriteLine("Line Loop(3) = {3, -6, -1, 5};");
                fout.WriteLine("Surface(3) = {3};");
                fout.WriteLine("Line Loop(4) = {4, -5, -2, 6};");
                fout.WriteLine("Surface(4) = {4};");
                fout.WriteLine("Surface Loop(1) = {3, 2, 4, 1};");
                fout.WriteLine("Volume(1) = {1};");                
                
                fout.Close();
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        void cube_mesh_creating()
        {
            try
            {
                //int k = 0;
                //int p = 1;
                StreamWriter fout = new StreamWriter(geometry_path.Text + @"\res" + ".geo");
                fout.WriteLine("SetFactory(\"OpenCASCADE\");");
                string side = height.Text.Replace(',', '.');
                fout.WriteLine("Box(1) = {0, 0, 0, " + side + ", " + side + ", " + side + "};");
                
                fout.Close();
            }
            catch (Exception e)
            {

            }
        }
        void mesh_segmentation()
        {
            try
            {
                StreamReader fin = new StreamReader((string)MeshesList.Items[0]);
                string help_str = "";
                do
                {
                    help_str = fin.ReadLine();
                } while (help_str != "$Nodes");
                help_str = fin.ReadLine();
                int Nodes = Convert.ToInt32(help_str);
                List<List<double>> NodeCoordinate = new List<List<double>>(Nodes);//[Номер узла][Номер координаты](Значение координаты)
                for (int i = 0; i < Nodes; i++)
                    NodeCoordinate.Add(new List<double>(3));              
                for (int i = 0; i < NodeCoordinate.Capacity; i++)
                {
                    help_str = fin.ReadLine();
                    string[] subs = help_str.Split(' ');
                    subs[1] = subs[1].Replace('.', ',');
                    subs[2] = subs[2].Replace('.', ',');
                    subs[3] = subs[3].Replace('.', ',');
                    NodeCoordinate[i].Add(Convert.ToDouble(subs[1]));
                    NodeCoordinate[i].Add(Convert.ToDouble(subs[2]));
                    NodeCoordinate[i].Add(Convert.ToDouble(subs[3]));
                }
                fin.ReadLine();
                fin.ReadLine();
                int Elements = Convert.ToInt32(fin.ReadLine());
                int borders = 0;
                List<int> ElementType = new List<int>(Elements);
                List<List<int>> NodeOfElement = new List<List<int>>(Elements);//[Номер элемента][Порядковый номер узла в элементе](Номер узла из списка узлов в файле .msh)
                for (int i = 0; i < Elements; i++)
                    NodeOfElement.Add(new List<int>(4));              
                List<List<double>> CentreOfElement = new List<List<double>>(Elements);//[Номер элемента][Номер координаты](Значение координаты)
                for (int i = 0; i < Elements; i++)
                {
                    CentreOfElement.Add(new List<double>(3));
                }
                List<int> Tag = new List<int>(Elements);
                for (int i = 0; i < Elements; i++)
                {
                    string[] subs = fin.ReadLine().Split(' ');
                    if (Convert.ToInt32(subs[1]) == 1)
                    {
                        ElementType.Add(1);
                        Tag.Add(Convert.ToInt32(subs[3]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[5]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[6]));
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][0] + NodeCoordinate[NodeOfElement[i][1] - 1][0]) / 2);
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][1] + NodeCoordinate[NodeOfElement[i][1] - 1][1]) / 2);
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][2] + NodeCoordinate[NodeOfElement[i][1] - 1][2]) / 2);
                    }
                    else if (Convert.ToInt32(subs[1]) == 2)
                    {
                        ElementType.Add(2);
                        Tag.Add(Convert.ToInt32(subs[3]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[5]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[6]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[7]));
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][0] + NodeCoordinate[NodeOfElement[i][1] - 1][0] + NodeCoordinate[NodeOfElement[i][2] - 1][0]) / 3);
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][1] + NodeCoordinate[NodeOfElement[i][1] - 1][1] + NodeCoordinate[NodeOfElement[i][2] - 1][1]) / 3);
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][2] + NodeCoordinate[NodeOfElement[i][1] - 1][2] + NodeCoordinate[NodeOfElement[i][2] - 1][2]) / 3);
                    }
                    else if (Convert.ToInt32(subs[1]) == 15)
                    {
                        ElementType.Add(15);
                        Tag.Add(Convert.ToInt32(subs[3]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[5]));
                        CentreOfElement[i].Add(NodeCoordinate[NodeOfElement[i][0] - 1][0]);
                        CentreOfElement[i].Add(NodeCoordinate[NodeOfElement[i][0] - 1][1]);
                        CentreOfElement[i].Add(NodeCoordinate[NodeOfElement[i][0] - 1][2]);
                    }
                    else if (Convert.ToInt32(subs[1]) == 4)
                    {
                        ElementType.Add(4);
                        Tag.Add(Convert.ToInt32(subs[3]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[5]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[6]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[7]));
                        NodeOfElement[i].Add(Convert.ToInt32(subs[8]));
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][0] + NodeCoordinate[NodeOfElement[i][1] - 1][0] + NodeCoordinate[NodeOfElement[i][2] - 1][0] + NodeCoordinate[NodeOfElement[i][3] - 1][0]) / 4);
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][1] + NodeCoordinate[NodeOfElement[i][1] - 1][1] + NodeCoordinate[NodeOfElement[i][2] - 1][1] + NodeCoordinate[NodeOfElement[i][3] - 1][1]) / 4);
                        CentreOfElement[i].Add((NodeCoordinate[NodeOfElement[i][0] - 1][2] + NodeCoordinate[NodeOfElement[i][1] - 1][2] + NodeCoordinate[NodeOfElement[i][2] - 1][2] + NodeCoordinate[NodeOfElement[i][3] - 1][2]) / 4);
                    }
                }
                fin.Close();
                fin.DiscardBufferedData();
                StreamWriter fout = new StreamWriter(meshes_path.Text + @"\resmesh" + ".msh");
                //fout.WriteLine("$MeshFormat");
                //fout.WriteLine("2.2 0 8");
                //fout.WriteLine("$EndMeshFormat");
                //fout.WriteLine("$Nodes");
                //fout.WriteLine(NodeCoordinate.Count);
                //for (int i = 0; i < nodes; i++)
                //{
                //    string x_node = Convert.ToString(x_nodes[i]);
                //    string y_node = Convert.ToString(y_nodes[i]);
                //    string z_node = Convert.ToString(z_nodes[i]);
                //    x_node = x_node.Replace(',', '.');
                //    y_node = y_node.Replace(',', '.');
                //    z_node = z_node.Replace(',', '.');
                //    fout.WriteLine((i + 1) + " " + x_node + " " + y_node + " " + z_node);
                //}
                //fout.WriteLine("$EndNodes");
                //fout.WriteLine("$Elements");
                //fout.WriteLine(elements);
                int k = 0;
                foreach (string fileName in SegmentedImagesList.Items)
                {
                    Mat img1 = new Mat(fileName);
                    Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
                    Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
                    Mat[] splitedHsv = new Mat[3];
                    Cv2.Split(hsv, out splitedHsv);
                    height.Text = height.Text.Replace('.', ',');
                    radius.Text = radius.Text.Replace('.', ',');
                    float z_scan = (float)Convert.ToDouble(height.Text);
                    float scan_radius = (float)Convert.ToDouble(radius.Text);
                    bool TetrahedronsCreated = false;
                    for (int i = 0; i < Elements; i++)
                    {
                        float h_centre = (float)CentreOfElement[i][2];
                        float scan_pos = k * z_scan / SegmentedImagesList.Items.Count;
                        if (Math.Abs(scan_pos - h_centre) < z_scan / SegmentedImagesList.Items.Count / 2)
                        {
                            if (ElementType[i] == 1)
                            {
                                byte[] Brightness = new byte[3];
                                Brightness[0] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][0] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][0] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[1] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][1] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][1] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[2] = (byte)(splitedHsv[2].At<byte>((int)(CentreOfElement[i][0] / (scan_radius * 2) * img1.Cols), (int)(CentreOfElement[i][1] / (scan_radius * 2) * img1.Rows)));
                                byte[] PhysicalGroups = new byte[3];
                                for (int Group = 0; Group < 3; Group++)
                                    PhysicalGroups[Group] = 0;
                                for (int index = 0; index < 3; index++)
                                {
                                    if (Brightness[index] == 0)
                                        PhysicalGroups[0]++;
                                    if (Brightness[index] == 150)
                                        PhysicalGroups[1]++;
                                    if (Brightness[index] == 255)
                                        PhysicalGroups[2]++;
                                }
                                if (PhysicalGroups[0] > PhysicalGroups[1] + PhysicalGroups[2])
                                {
                                    Tag[i] = 0;
                                }
                                else if (PhysicalGroups[1] > PhysicalGroups[0] + PhysicalGroups[2])
                                {
                                    Tag[i] = 1;
                                }
                                else
                                {
                                    Tag[i] = 4;
                                }
                                //else
                                //{
                                //    tag[i] = 5;
                                //    borders++;
                                //}
                            }
                            else if (ElementType[i] == 2)
                            {
                                byte[] Brightness = new byte[4];
                                Brightness[0] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][0] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][0] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[1] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][1] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][1] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[2] = (byte)(splitedHsv[2].At<byte>((int)(CentreOfElement[i][0] / (scan_radius * 2) * img1.Cols), (int)(CentreOfElement[i][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[3] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][2] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][2] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                byte[] PhysicalGroups = new byte[3];
                                for (int Group = 0; Group < 3; Group++)
                                    PhysicalGroups[Group] = 0;
                                for (int index = 0; index < 4; index++)
                                {
                                    if (Brightness[index] == 0)
                                        PhysicalGroups[0]++;
                                    if (Brightness[index] == 150)
                                        PhysicalGroups[1]++;
                                    if (Brightness[index] == 255)
                                        PhysicalGroups[2]++;
                                }
                                if (PhysicalGroups[0] > PhysicalGroups[1] + PhysicalGroups[2])
                                {
                                    Tag[i] = 0;
                                }
                                else if (PhysicalGroups[1] > PhysicalGroups[0] + PhysicalGroups[2])
                                {
                                    Tag[i] = 1;
                                }
                                else
                                {
                                    Tag[i] = 4;
                                }
                                //else
                                //{
                                //    tag[i] = 5;
                                //    borders++;
                                //}
                            }
                            else if (ElementType[i] == 15)
                            {
                                int V = (int)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][0] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][0] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                if (V == 0)
                                {
                                    Tag[i] = 0;
                                }
                                else if (V == 150)
                                {
                                    Tag[i] = 1;
                                }
                                else if (V == 255)
                                {
                                    Tag[i] = 4;
                                }
                                //else
                                //{
                                //    tag[i] = 5;
                                //}
                            }
                            else if (ElementType[i] == 4)
                            {
                                byte[] Brightness = new byte[5];
                                Brightness[0] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][0] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][0] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[1] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][1] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][1] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[2] = (byte)(splitedHsv[2].At<byte>((int)(CentreOfElement[i][0] / (scan_radius * 2) * img1.Cols), (int)(CentreOfElement[i][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[3] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][2] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][2] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                Brightness[4] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[i][3] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[i][3] - 1][1] / (scan_radius * 2) * img1.Rows)));
                                byte[] PhysicalGroups = new byte[3];
                                for (int Group = 0; Group < 3; Group++)
                                    PhysicalGroups[Group] = 0;
                                for (int index = 0; index < 5; index++)
                                {
                                    if (Brightness[index] == 0)
                                        PhysicalGroups[0]++;
                                    if (Brightness[index] == 150)
                                        PhysicalGroups[1]++;
                                    if (Brightness[index] == 255)
                                        PhysicalGroups[2]++;
                                }
                                if (PhysicalGroups[0] > PhysicalGroups[1] + PhysicalGroups[2])
                                {
                                    Tag[i] = 0;
                                }
                                else if (PhysicalGroups[1] > PhysicalGroups[0] + PhysicalGroups[2])
                                {
                                    Tag[i] = 1;
                                }
                                else
                                {
                                    Tag[i] = 4;
                                }
                            }
                        }
                    //            if (PhysicalGroups[0] == 5)
                    //            {
                    //                Tag[i] = 0;
                    //            }
                    //            else if (PhysicalGroups[1] == 5)
                    //            {
                    //                Tag[i] = 1;
                    //            }
                    //            else if (PhysicalGroups[2] == 5)
                    //            {
                    //                Tag[i] = 4;
                    //            }
                    //            else
                    //            {
                    //                NewTetrahedrons(i, ref NodeCoordinate, ref NodeOfElement, ref CentreOfElement, ref Tag, ref ElementType);
                    //                TetrahedronsCreated = true;
                    //            }
                    //        }
                    //    }
                    //}
                    //if (TetrahedronsCreated)
                    //{
                    //    for (int ElemIndex = Elements; ElemIndex < NodeOfElement.Count; ElemIndex++)
                    //    {
                    //        byte[] Brightness = new byte[5];
                    //        Brightness[0] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[ElemIndex][0] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[ElemIndex][0] - 1][1] / (scan_radius * 2) * img1.Rows)));
                    //        Brightness[1] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[ElemIndex][1] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[ElemIndex][1] - 1][1] / (scan_radius * 2) * img1.Rows)));
                    //        Brightness[2] = (byte)(splitedHsv[2].At<byte>((int)(CentreOfElement[ElemIndex][0] / (scan_radius * 2) * img1.Cols), (int)(CentreOfElement[ElemIndex][1] / (scan_radius * 2) * img1.Rows)));
                    //        Brightness[3] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[ElemIndex][2] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[ElemIndex][2] - 1][1] / (scan_radius * 2) * img1.Rows)));
                    //        Brightness[4] = (byte)(splitedHsv[2].At<byte>((int)(NodeCoordinate[NodeOfElement[ElemIndex][3] - 1][0] / (scan_radius * 2) * img1.Cols), (int)(NodeCoordinate[NodeOfElement[ElemIndex][3] - 1][1] / (scan_radius * 2) * img1.Rows)));
                    //        byte[] PhysicalGroups = new byte[3];
                    //        for (int Group = 0; Group < 3; Group++)
                    //            PhysicalGroups[Group] = 0;
                    //        for (int index = 0; index < 5; index++)
                    //        {
                    //            if (Brightness[index] == 0)
                    //                PhysicalGroups[0]++;
                    //            if (Brightness[index] == 150)
                    //                PhysicalGroups[1]++;
                    //            if (Brightness[index] == 255)
                    //                PhysicalGroups[2]++;
                    //        }
                    //        if (PhysicalGroups[0] > PhysicalGroups[1] + PhysicalGroups[2])
                    //        {
                    //            Tag[ElemIndex] = 0;
                    //        }
                    //        else if (PhysicalGroups[1] > PhysicalGroups[0] + PhysicalGroups[2])
                    //        {
                    //            Tag[ElemIndex] = 1;
                    //        }
                    //        else
                    //        {
                    //            Tag[ElemIndex] = 4;
                    //        }
                    //    }
                    //    Elements = NodeOfElement.Count();
                    }
                    k++;
                }
                Elements = NodeOfElement.Count();
                fout.WriteLine("$MeshFormat");
                fout.WriteLine("2.2 0 8");
                fout.WriteLine("$EndMeshFormat");
                fout.WriteLine("$Nodes");
                fout.WriteLine(NodeCoordinate.Count);
                for (int i = 0; i < NodeCoordinate.Count; i++)
                {
                    string x_node = Convert.ToString(NodeCoordinate[i][0]);
                    string y_node = Convert.ToString(NodeCoordinate[i][1]);
                    string z_node = Convert.ToString(NodeCoordinate[i][2]);
                    x_node = x_node.Replace(',', '.');
                    y_node = y_node.Replace(',', '.');
                    z_node = z_node.Replace(',', '.');
                    fout.WriteLine((i + 1) + " " + x_node + " " + y_node + " " + z_node);
                }
                fout.WriteLine("$EndNodes");
                fout.WriteLine("$Elements");
                fout.WriteLine(Elements);
                for (int i = 0; i < NodeOfElement.Count; i++)
                {
                    if (ElementType[i] == 15)
                    {
                        fout.WriteLine((i + 1) + " " + ElementType[i] + " " + 2 + " " + Tag[i] + " " + Tag[i] + " " + NodeOfElement[i][0]);
                    }
                    else if (ElementType[i] == 1)
                    {
                        fout.WriteLine((i + 1) + " " + ElementType[i] + " " + 2 + " " + Tag[i] + " " + Tag[i] + " " + NodeOfElement[i][0] + " " + NodeOfElement[i][1]);
                    }
                    else if (ElementType[i] == 2)
                    {
                        fout.WriteLine((i + 1) + " " + ElementType[i] + " " + 2 + " " + Tag[i] + " " + Tag[i] + " " + NodeOfElement[i][0] + " " + NodeOfElement[i][1] + " " + NodeOfElement[i][2]);
                    }
                    else if (ElementType[i] == 4)
                    {
                        fout.WriteLine((i + 1) + " " + ElementType[i] + " " + 2 + " " + Tag[i] + " " + Tag[i] + " " + NodeOfElement[i][0] + " " + NodeOfElement[i][1] + " " + NodeOfElement[i][2] + " " + NodeOfElement[i][3]);
                    }
                }
                fout.WriteLine("$EndElements");
                fout.Close();
                float border_percent = (float)borders * 100.0f / (float)Elements;
                Console_block.Text += "\nNew mesh segmented\nBorders: " + border_percent + "%";
                SegmentedMeshesList.Items.Add(meshes_path.Text + @"\resmesh" + ".msh");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error! " + e.Message);
            }
        }
        private void convert_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                segmentation();
                filter();
                //filter();
                //filter();
                if (Cylinder.IsChecked == true)
                    CylinderMeshCreating();
                else
                    SquareGeoCreating();              
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        private void btnOpenSegmentedScans_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpeg;*.jpg;*.tif;*.tiff;*.bmp|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    ImagesList.Items.Clear();
                    foreach (string filename in openFileDialog.FileNames)
                        SegmentedImagesList.Items.Add(System.IO.Path.GetFullPath(filename));
                }
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        private void btnOpenScans_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Image files (*.png;*.jpg)|*.png;*.jpeg;*.jpg;*.tif;*.tiff;*.bmp|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    ImagesList.Items.Clear();
                    foreach (string filename in openFileDialog.FileNames)
                        ImagesList.Items.Add(System.IO.Path.GetFullPath(filename));
                }
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        private void btnSaveScans_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog saveFileDialog = new FolderBrowserDialog();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    scans_path.Text = saveFileDialog.SelectedPath;
                }
            }
            catch (Exception e3)
            {
                System.Windows.MessageBox.Show("Error! " + e3.Message);
            }
        }
        private void btnOpenMeshes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Mesh files (*.msh)|*.msh|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                        MeshesList.Items.Add(System.IO.Path.GetFullPath(filename));
                }
            }
            catch (Exception e2)
            {
                System.Windows.MessageBox.Show("Error! " + e2.Message);
            }
        }
        private void btnSaveMeshes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog saveFileDialog = new FolderBrowserDialog();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    meshes_path.Text = saveFileDialog.SelectedPath;
                }
            }
            catch (Exception e3)
            {
                System.Windows.MessageBox.Show("Error! " + e3.Message);
            }
        }
        private void mesh_button_Click(object sender, RoutedEventArgs e)
        {
            LayerMeshCreating();
            //mesh_segmentation();
        }
        private void btnSaveGeo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FolderBrowserDialog saveFileDialog = new FolderBrowserDialog();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    geometry_path.Text = saveFileDialog.SelectedPath;
                }
            }
            catch (Exception e3)
            {
                System.Windows.MessageBox.Show("Error! " + e3.Message);
            }
        }
        private void ImagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CT_Scan_Orig.Source = BitmapFrame.Create(new Uri((string)ImagesList.Items[ImagesList.SelectedIndex]));
        }
        private void SegmentedImagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CT_Scan.Source = BitmapFrame.Create(new Uri((string)SegmentedImagesList.Items[SegmentedImagesList.SelectedIndex]));
        }       
    }
}