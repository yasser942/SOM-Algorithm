using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SOM
{
    
    public partial class Form1 : Form
    {
        private string selectedFileName;
        List<string> clusteredItems = new List<string>();
        Map map;
        public Form1()
        {
            InitializeComponent();
            
            
            
          
            


            

        }
        void InsertButtonsIntoGrid()
        {
            // Clear existing rows and columns
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            // Add 3 columns without labels
            for (int i = 0; i < 3; i++)
            {
                dataGridView1.Columns.Add("Column" + i, ""); // No column label
            }

            // Add 3 rows
            for (int i = 0; i < 3; i++)
            {
                dataGridView1.Rows.Add();
            }

            // Insert buttons into cells
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    // Create a button cell
                    DataGridViewButtonCell buttonCell = new DataGridViewButtonCell();
            
                    // Set button label
                    buttonCell.Value = row + "," + col;

                    // Center align the button label
                    buttonCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    // Assign button cell to the cell in the grid
                    dataGridView1[col, row] = buttonCell;
                }
            }
        }

        private void RunSOM()
        {
            StreamReader sr = new StreamReader(selectedFileName);
            int lng = sr.ReadLine().Split(',').Length;
             map = new Map(lng - 1, 3, selectedFileName);
            
           

            
            
            InsertButtonsIntoGrid();
            

        }
       
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the clicked cell is a button cell and get its coordinates
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && dataGridView1[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell)
            {
                int xCoord = e.ColumnIndex;
                int yCoord = e.RowIndex;

                // Call the method to handle the selected coordinates
                HandleSelectedCoordinates(xCoord, yCoord);
            }
        }

        private void HandleSelectedCoordinates(int xCoord, int yCoord)
        {
            // Display the selected coordinates
            MessageBox.Show(string.Format("Button clicked at coordinates: ({0}, {1})", xCoord, yCoord));
            clusteredItems = map.GetClusteredItems(xCoord, yCoord);
            foreach (string item in clusteredItems)
            {
                Console.WriteLine(item);
            }
            

         
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set properties of the dialog
            openFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            // Show the dialog and check if the user clicked OK
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file name
                selectedFileName = openFileDialog1.FileName;
                Console.WriteLine(selectedFileName);
                textBox1.Text = selectedFileName;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RunSOM();
        }

       
    }
    
        public class Map
    {
        private Neuron[,] outputs;  // Collection of weights.
        private int iteration;      // Current iteration.
        private int length;        // Side length of output grid.
        private int dimensions;    // Number of input dimensions.
        private Random rnd = new Random();

        private List<string> labels = new List<string>();
        private List<double[]> patterns = new List<double[]>();

        private List<string> patterns_with_target_attr = new List<string>();

        public Map(int dimensions, int length, string file)
        {
            this.length = length;
            this.dimensions = dimensions;
            Initialise();
            LoadData(file);
            NormalisePatterns();
            Train(0.0000001);
            DumpCoordinates();
            

            double sse = 0;
            for (int i = 0; i < outputs.GetLength(0); i++)
            {
                for (int j = 0; j < outputs.GetLength(1); j++)
                {
                    sse += outputs[i, j].sse1();
                }
            }

            Console.WriteLine(sse);
        }

        private void Initialise()
        {
            outputs = new Neuron[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    outputs[i, j] = new Neuron(i, j, length);
                    outputs[i, j].Weights = new double[dimensions];
                    outputs[i, j].total_attr_values = new double[dimensions];
                    for (int k = 0; k < dimensions; k++)
                    {
                        outputs[i, j].Weights[k] = rnd.NextDouble();
                    }
                }
            }
        }

        private void LoadData(string file)
        {
            StreamReader reader = File.OpenText(file);
            reader.ReadLine(); // Ignore first line.
            while (!reader.EndOfStream)
            {
                string[] line = reader.ReadLine().Split(',');
                labels.Add(line[0]);
                double[] inputs = new double[dimensions];
                for (int i = 0; i < dimensions; i++)
                {
                    inputs[i] = double.Parse(line[i]);
                }
                patterns.Add(inputs);
                patterns_with_target_attr.Add(line[dimensions]);
            }
            reader.Close();
        }

        private void NormalisePatterns()
        {
            for (int j = 0; j < dimensions; j++)
            {
                double max = 0;
                for (int i = 0; i < patterns.Count; i++)
                {
                    if (patterns[i][j] > max) max = patterns[i][j];
                }
                for (int i = 0; i < patterns.Count; i++)
                {
                    patterns[i][j] = patterns[i][j] / max;
                }
            }
        }

        private void Train(double maxError)
        {
            double currentError = double.MaxValue;
            while (currentError > maxError)
            {
                currentError = 0;
                List<double[]> TrainingSet = new List<double[]>();
                foreach (double[] pattern in patterns)
                {
                    TrainingSet.Add(pattern);
                }
                for (int i = 0; i < patterns.Count; i++)
                {
                    double[] pattern = TrainingSet[rnd.Next(patterns.Count - i)];
                    currentError += TrainPattern(pattern);
                    TrainingSet.Remove(pattern);
                }
                Console.WriteLine(currentError.ToString("0.0000000"));
            }
        }

        private double TrainPattern(double[] pattern)
        {
            double error = 0;
            Neuron winner = Winner(pattern);
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    error += outputs[i, j].UpdateWeights(pattern, winner, iteration);
                }
            }
            iteration++;
            return Math.Abs(error / (length * length));
        }

        private void DumpCoordinates()
        {
            for (int i = 0; i < patterns.Count; i++)
            {
                Neuron n = Winner(patterns[i]);
                n.data.Add(patterns[i]);
                n.tr.Add(patterns_with_target_attr[i]);
                for (int j = 0; j < patterns[i].Length; j++)
                    n.total_attr_values[j] += patterns[i][j];
                Console.WriteLine("{0},{1},{2}", i, n.X, n.Y);
            }
        }

        private Neuron Winner(double[] pattern)
        {
            Neuron winner = null;
            double min = double.MaxValue;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                {
                    double d = Distance(pattern, outputs[i, j].Weights);
                    if (d < min)
                    {
                        min = d;
                        winner = outputs[i, j];
                    }
                }
            return winner;
        }

        private double Distance(double[] vector1, double[] vector2)
        {
            double value = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                value += Math.Pow((vector1[i] - vector2[i]), 2);
            }
            return Math.Sqrt(value);
        }

        public List<string> GetClusteredItems(int xCoord, int yCoord)
        {
            List<string> clusteredItems = new List<string>();

            // Iterate over neurons in the same row as the clicked neuron
            for (int j = 0; j < length; j++)
            {
                if (outputs[xCoord, j].Y == yCoord) // Check if same Y coordinate
                {
                    foreach (string item in outputs[xCoord, j].tr)
                    {
                        clusteredItems.Add(item);
                    }
                }
            }

            return clusteredItems;
        }

    }

    public class Neuron
    {
        public double[] Weights;
        public int X;
        public int Y;
        private int length;
        private double nf;
        public List<double[]> data;
        public List<string> tr;

        public double sse1()
        {
            double retV = 0;
            for (int i = 0; i < data.Count; i++)
            {
                retV += Distance(Weights, data[i]);
            }
            return retV;
        }

        private double Distance(double[] vector1, double[] vector2)
        {
            double value = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                value += Math.Pow((vector1[i] - vector2[i]), 2);
            }
            return value;
        }

        public double[] total_attr_values;

        public Neuron(int x, int y, int length)
        {
            X = x;
            Y = y;
            this.length = length;
            nf = 1000 / Math.Log(length);
            data = new List<double[]>();
            tr = new List<string>();
        }

        private double Gauss(Neuron win, int it)
        {
            double distance = Math.Sqrt(Math.Pow(win.X - X, 2) + Math.Pow(win.Y - Y, 2));
            return Math.Exp(-Math.Pow(distance, 2) / (Math.Pow(Strength(it), 2)));
        }

        private double LearningRate(int it)
        {
            return Math.Exp(-it / 1000) * 0.1;
        }

        private double Strength(int it)
        {
            return Math.Exp(-it / nf) * length;
        }

        public double UpdateWeights(double[] pattern, Neuron winner, int it)
        {
            double sum = 0;
            for (int i = 0; i < Weights.Length; i++)
            {
                double delta = LearningRate(it) * Gauss(winner, it) * (pattern[i] - Weights[i]);
                Weights[i] += delta;
                sum += delta;
            }
            return sum / Weights.Length;
        }
    }
}