using Keras.Models;
using Numpy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace keras_net_catsNdogs
{
    public partial class Form1 : Form
    {
        string app_startup_path = "";
        string models_path = "";
        string model_file = "";
        Stopwatch sw = new Stopwatch();
        NDarray x_normimg = null;
        string[] class_names = null;
        Keras.Models.BaseModel catsndogs_model = null;
        byte[] byteImage = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            app_startup_path = Application.StartupPath;
            int pos = app_startup_path.IndexOf("keras_net_catsNdogs\\bin");
            models_path = app_startup_path.Substring(0, pos) + "models";

            class_names = new string[] { "cat", "dog" };
        }

        private void btnLoadModel_Click(object sender, EventArgs e)
        {
            // model file : cats_and_dogs_xfer.h5
            // Made by Transfer learning based on Xception model in python (jupyter notebook)
            model_file = Path.Combine(models_path, "cats_and_dogs_xfer.h5");
            sw.Reset();
            sw.Start(); // start to measure time
            catsndogs_model = Sequential.LoadModel(model_file);
            if (sw.IsRunning) sw.Stop(); // end to measure time
            string strResult = string.Format("LoadModel : {0} (Elapsed Time : {1:0.000} [msec])", model_file, sw.ElapsedTicks / Stopwatch.Frequency * 1E3);
            lblResult.Text = strResult;
        }

        private void btnLoadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Images (*.JPG)|*.JPG|.BMP)|*.BMP|" + "All files (*.*)|*.*";
            openFileDialog1.Title = "Select Cats & Dogs Image";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap srcBitmap = new Bitmap(openFileDialog1.FileName);
                int width = srcBitmap.Width;
                int height = srcBitmap.Height;
                Size resize = new Size(224, 224);
                Bitmap myBitmap = new Bitmap(srcBitmap, resize);

                picImage.Image = myBitmap;

                // Image to Byte Array
                Rectangle rect = new Rectangle(0, 0, myBitmap.Width, myBitmap.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    myBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = Math.Abs(bmpData.Stride) * myBitmap.Height;
                byteImage = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(ptr, byteImage, 0, bytes);

                // Unlock the bits.
                myBitmap.UnlockBits(bmpData);
            }
        }

        private void btnPredict_Click(object sender, EventArgs e)
        {
            if (catsndogs_model == null || picImage.Image == null)
                return;

            int n = byteImage.Length;
            double[] doubleArr = new double[n];
            int resize_difference = 9; // Approximate difference between resizing using opencv and resizing with bitmap .. not exact same
            for (int i = 0; i < n; i++)
            {
                doubleArr[i] = (byteImage[i] + resize_difference) / (double)255;
            }

            sw.Reset();
            sw.Start(); // start to measure time
            x_normimg = np.array(doubleArr);
            x_normimg = x_normimg.reshape(1, 224, 224, 3);
            var pred = catsndogs_model.Predict(x_normimg);
            if (sw.IsRunning) sw.Stop(); // end to measure time
            var vmax = pred.argmax();
            string str_max = vmax.str;
            int imax = int.Parse(str_max);
            float[] res = pred.GetData<float>();
            string strResult = string.Format("Result : {0}, Probobility : {1:0.00}% (Elapsed Time : {2:0.000} [msec])", 
                class_names[imax], res[imax] * 100, sw.ElapsedTicks / Stopwatch.Frequency * 1E3);
            lblResult.Text = strResult;
        }
    }
}
