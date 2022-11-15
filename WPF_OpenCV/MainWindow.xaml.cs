using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {
        object imagesListLock = new object();
        object segmentedimagesListLock = new object();
        object lowscopeLock = new object();
        object midscopeLock = new object();
        object topscopeLock = new object();
        object scanPathLock = new object();
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
        void SegmentationOfScan(int i)
        {
            
            string scansPath, fileName;
            int topScope, midScope, lowScope;
            lock (scanPathLock)
            {
                scansPath = scans_path.Text;
            }
            lock (lowscopeLock)
            {
                lowScope = Convert.ToInt32(LowScope.Text);
            }
            lock (midscopeLock)
            {
                midScope = Convert.ToInt32(MidScope.Text);
            }
            lock (topscopeLock)
            {
                topScope = Convert.ToInt32(TopScope.Text);
            }
            lock (imagesListLock)
            {
                fileName = (string)ImagesList.Items[i];
            }

            
            Mat img1 = new Mat(fileName);
            Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
            Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
            Mat[] splitedHsv = new Mat[3];
            Cv2.Split(hsv, out splitedHsv);
            //CT_Scan_Orig.Source = BitmapFrame.Create(new Uri(fileName));
            for (int x = 0; x < hsv.Rows; x++)
            {
                for (int y = 0; y < hsv.Cols; y++)
                {
                    int H = (int)(splitedHsv[0].At<byte>(x, y));        // Тон
                    int S = (int)(splitedHsv[1].At<byte>(x, y));        // Интенсивность
                    int V = (int)(splitedHsv[2].At<byte>(x, y));        // Яркость
                    if (V >= lowScope && V < midScope)
                    {
                        img1.At<Vec3b>(x, y)[0] = 0;
                        img1.At<Vec3b>(x, y)[1] = 0;
                        img1.At<Vec3b>(x, y)[2] = 0;
                    }
                    else
                        if (V >= midScope && V < topScope)
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
            Cv2.ImWrite(scansPath + @"\res" + i + ".tif", img1);           
        }
        async void ParallelSegmentation()
        {
            try
            {
                Task[] tasks = new Task[ImagesList.Items.Count];
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Run(() => SegmentationOfScan(i));
                }
                await Task.WhenAll(tasks);
                for (int i = 0; i < ImagesList.Items.Count; i++)
                {
                    CT_Scan.Source = BitmapFrame.Create(new Uri((String)scans_path.Text + @"\res" + i + ".tif"));
                    SegmentedImagesList.Items.Add(scans_path.Text + @"\res" + i + ".tif");
                    Console_block.Text += "\nNew scan segmented";
                    num_of_scans.Text = "Elements: " + Convert.ToString(ImagesList.Items.Count);
                }
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("Error! " + e.Message);
            }
        }
        void FiltrationOfScan(int i)
        {
            string fileName;
            lock (segmentedimagesListLock)
            {
                fileName = (string)SegmentedImagesList.Items[i];
            }
            Mat img1 = new Mat(fileName);
            Mat hsv = new Mat(img1.Cols, img1.Rows, 8, 3);
            Cv2.CvtColor(img1, hsv, ColorConversionCodes.BGR2HSV);
            Mat[] splitedHsv = new Mat[3];
            Cv2.Split(hsv, out splitedHsv);
            //CT_Scan_Orig.Source = BitmapFrame.Create(new Uri(fileName));
            for (int x = 0; x < hsv.Rows; x++)
            {
                for (int y = 0; y < hsv.Cols; y++)
                {
                    if (x != 0 && y != 0 && x != img1.Cols - 1 && y != img1.Rows - 1)
                    {
                        int[][] filter_matrix = new int[3][];
                        int main_V = 0;
                        for (int k = 0; k < 3; i++)
                        {
                            filter_matrix[i] = new int[3];
                            for (int j = 0; j < 3; j++)
                            {
                                filter_matrix[k][j] = (int)(splitedHsv[2].At<byte>(x - 1 + k, y - 1 + j));
                                main_V += filter_matrix[k][j];
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
            lock (scanPathLock)
            {
                Cv2.ImWrite(scans_path.Text + @"\res_filtered" + i + ".tif", img1);
            }
            //CT_Scan.Source = BitmapFrame.Create(new Uri((String)scans_path.Text + @"\res_filtered" + i + ".tif"));
            //SegmentedImagesList.Items.Add(scans_path.Text + @"\res_filtered" + i + ".tif");
        }
        async void ParallelFilter()
        {
            try
            {
                int oldCount;
                lock (segmentedimagesListLock)
                {
                    oldCount = SegmentedImagesList.Items.Count;
                }
                
                Task[] tasks = new Task[oldCount];
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Run(() => FiltrationOfScan(i));
                }
                await Task.WhenAll(tasks);
                lock (segmentedimagesListLock)
                {
                    SegmentedImagesList.Items.Clear();
                }
                for (int i = 0; i < oldCount; i++)
                {
                    CT_Scan.Source = BitmapFrame.Create(new Uri((String)scans_path.Text + @"\res_filtered" + i + ".tif"));
                    SegmentedImagesList.Items.Add(scans_path.Text + @"\res_filtered" + i + ".tif");
                }                
            }
            catch(Exception e)
            {
                System.Windows.MessageBox.Show("Error! " + e.Message);
            }
        }
        void CylinderGeoCreating()
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
        void CubeGeoCreating()
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
        private void convert_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParallelSegmentation();
                ParallelFilter();
                if (Cylinder.IsChecked == true)
                    CylinderGeoCreating();
                else
                    CubeGeoCreating();              
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
            Mesh mesh = new Mesh((string)MeshesList.Items[0]);
            mesh.NodeTagsInitialising(SegmentedMeshesList, height.Text, radius.Text, SegmentedImagesList.Items.Count);
            mesh.ElementTagsInitialising();
            mesh.SaveMesh(meshes_path.Text);
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