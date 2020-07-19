using mbtech.faceDetection.core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IComponent = mbtech.faceDetection.core.IComponent;

namespace mbtech.faceDetection
{
    public partial class FrmMain : Form
    {
        PictureBox picture = new PictureBox();
        static readonly string detectedType = Convert.ToString(ConfigurationManager.AppSettings["detectedType"]);

        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtDirectory.Text = fbd.SelectedPath;
                }
            }
        }


        private void btnCancel_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(txtDirectory.Text);

            List<object> arguments = new List<object>();
            arguments.Add(1);
            arguments.Add(files);
            if (!worker.IsBusy)
                worker.RunWorkerAsync(argument: arguments);
        }

        private void WriteLine(string linea, int percentaje = 0)
        {
            worker.ReportProgress(percentaje, new Tuple<string, string>(linea, ""));
        }

        private void ModeConsole(int flag ,string[] files)
        {
            int count = files.Count();
            var cantidadTotal = 0;
            decimal index = 1;
            var percentageFinal = 0;
            WriteLine("Face Detection [Versión 1.0.0.0]");
            WriteLine("(c) 2020 Mb Tech & Emgu & Azure Cognitive Services. Todos los derechos reservados.");
            WriteLine("");
            WriteLine(string.Format("Se detectaron {0} imagenes, iniciando el procesamiento...", count));
            WriteLine("");
            Stopwatch stopWatchTotal = new Stopwatch();
            stopWatchTotal.Start();
            foreach (var file in files)
            {
                percentageFinal = ProcesarItem(flag, count, ref cantidadTotal, ref index, file);
            }

            stopWatchTotal.Stop();
            TimeSpan tts = stopWatchTotal.Elapsed;
            string elapsedTimeTotal = String.Format("{1:00}:{2:00}.{3:00}", tts.Hours, tts.Minutes, tts.Seconds, tts.Milliseconds / 10);
            WriteLine("", percentageFinal);
            WriteLine(string.Format("El procesamiento finalizó!!!, imagenes-procesadas {0}, rostros-detectados: {1}, tiempo-procesamiento: {2}", count, cantidadTotal, elapsedTimeTotal), percentageFinal);


        }

        private int ProcesarItem(int flag, int count, ref int cantidadTotal, ref decimal index, string file, bool stop = false)
        {
            int percentageFinal;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var percentage = index / count;
            picture.Image = Image.FromFile(file);
            IComponent component = null;
            switch (detectedType)
            {
                case "AZURE": component = new AzureComponent(); break;
                case "EMGU": component = new EmguComponent(); ; break;
            }
            var response = component.ProcessImage(file);
            var result = response.items ?? new List<Rectangle>().ToArray();
            var rectangles = result;
            if (flag == 2)
                ModePicture(rectangles, file);
            percentageFinal = (int)(percentage * 100);
            cantidadTotal += result.Count();
            var cantidadRostros = result.Count().ToString();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string tiempoProcesamiento = String.Format("{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            string line = string.Format("Se procesó ({0}), rostros-detectados {1}, tiempo-procesamiento {2}", file, cantidadRostros, tiempoProcesamiento);

            if (response.error != null && response.error.code == "429")
            {
                WriteLine(response.error.message, percentageFinal);
                Thread.Sleep(40000);
                if(!stop)
                ProcesarItem(flag, count,  ref cantidadTotal, ref index, file, true);
            }

            WriteLine(line, percentageFinal);


            index++;
            return percentageFinal;
        }

        private void ModePicture(Rectangle[] rectangles, string file, bool flag = false)
        {
            Thread.Sleep(500);
          

            var fileInfo = new FileInfo(file);
            try
            {
                picture.Image = Image.FromFile(file);
                var image = Image.FromFile(file, true);
                Graphics graphics = Graphics.FromImage(image);
                Pen pen = new Pen(Color.Red, 2);
                foreach (var rec in rectangles)
                {
                    graphics.DrawRectangle(pen, rec);
                }

                picture.Image = image;
            }
            catch 
            {
                Image imageIn = Image.FromFile(file);
                string tempPath = Path.GetTempPath();
                string newPath = Path.Combine(tempPath, string.Format("{0}.jpg", Guid.NewGuid()));
                imageIn.Save(newPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                file = newPath;
                if (!flag)
                ModePicture(rectangles, file ,true);
            }
      
            Thread.Sleep(500);

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var arguments = (List<object>)e.Argument;
            var flag = (int)arguments[0];
            var files = (string[])arguments[1];
            ModeConsole(flag, files);
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var args = (Tuple<string, string>)e.UserState;
            string line = args.Item1;
            lbConsole.Items.Add(line);
            progressB.Value = e.ProgressPercentage;
            lbConsole.SelectedIndex = lbConsole.Items.Count - 1;

        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void btnStartPreview_Click(object sender, EventArgs e)
        {
            tableLayoutPanel1.Controls.Remove(this.lbConsole);
            picture.Dock = System.Windows.Forms.DockStyle.Fill;
            picture.TabIndex = 5;
            this.picture.Location = new System.Drawing.Point(3, 43);
            this.picture.Name = "picture";
            this.picture.Size = new System.Drawing.Size(958, 650);
            picture.TabStop = false;
            tableLayoutPanel1.SetColumnSpan(picture, 4);
            tableLayoutPanel1.Controls.Add(picture, 0, 1);

            string[] files = Directory.GetFiles(txtDirectory.Text);
            List<object> arguments = new List<object>();
            arguments.Add(2);
            arguments.Add(files);
            if (!worker.IsBusy)
                worker.RunWorkerAsync(argument: arguments);
        }
    }
}
