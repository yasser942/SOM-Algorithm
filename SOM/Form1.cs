using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SOM
{
    public partial class Form1 : Form
    {
        private List<string> clusteredItems = new List<string>();
        private Map map;
        private string selectedFileName;

        public Form1()
        {
            InitializeComponent();
        }

        private void InsertButtonsIntoGrid(int gridDim)
        {
            // Clear existing rows and columns
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            // Add 3 columns without labels
            for (var i = 0; i < gridDim; i++) dataGridView1.Columns.Add("Column" + i, ""); // No column label

            // Add 3 rows
            for (var i = 0; i < gridDim; i++) dataGridView1.Rows.Add();

            // Insert buttons into cells
            for (var row = 0; row < gridDim; row++)
            for (var col = 0; col < gridDim; col++)
            {
                // Create a button cell
                var buttonCell = new DataGridViewButtonCell();

                // Set button label
                buttonCell.Value = row + "," + col;

                // Center align the button label
                buttonCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;


                // Assign button cell to the cell in the grid
                dataGridView1[col, row] = buttonCell;
            }
        }
        


        private void RunSom()
        {
            var gridDim = textBox2.Text == "" ? 3 : int.Parse(textBox2.Text);
            var sr = new StreamReader(selectedFileName);
            var lng = sr.ReadLine().Split(',').Length;
            map = new Map(lng - 1, gridDim, selectedFileName);
            InsertButtonsIntoGrid(gridDim);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the clicked cell is a button cell and get its coordinates
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 &&
                dataGridView1[e.ColumnIndex, e.RowIndex] is DataGridViewButtonCell)
            {
                var xCoord = e.RowIndex;
                var yCoord = e.ColumnIndex;

                // Call the method to handle the selected coordinates
                HandleSelectedCoordinates(xCoord, yCoord);
            }
        }

        private void HandleSelectedCoordinates(int xCoord, int yCoord)
        {
            // Display the selected coordinates
            clusteredItems = map.GetClusteredItems(xCoord, yCoord);
            var instances = "";
            foreach (var item in clusteredItems)
            {
                Console.WriteLine(item);
                instances += item + "\n";
            }

            MessageBox.Show(string.Format(instances), "Clustered Items in Cell (" + xCoord + ", " + yCoord + ")");
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Create an instance of OpenFileDialog
            var openFileDialog1 = new OpenFileDialog();

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
            RunSom();
        }
    }

    public class Map
    {
        private readonly int dimensions; // Number of input dimensions.
        private int iteration; // Current iteration.

        private readonly List<string> labels = new List<string>();
        private readonly int length; // Side length of output grid.
        private Neuron[,] outputs; // Collection of weights.
        private readonly List<double[]> patterns = new List<double[]>();

        private readonly List<string> patterns_with_target_attr = new List<string>();
        private readonly Random rnd = new Random();

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
            for (var i = 0; i < outputs.GetLength(0); i++)
            for (var j = 0; j < outputs.GetLength(1); j++)
                sse += outputs[i, j].sse1();

            Console.WriteLine(sse);
        }

        private void Initialise()
        {
            outputs = new Neuron[length, length];
            for (var i = 0; i < length; i++)
            for (var j = 0; j < length; j++)
            {
                outputs[i, j] = new Neuron(i, j, length);
                outputs[i, j].Weights = new double[dimensions];
                outputs[i, j].total_attr_values = new double[dimensions];
                for (var k = 0; k < dimensions; k++) outputs[i, j].Weights[k] = rnd.NextDouble();
            }
        }

        private void LoadData(string file)
        {
            var reader = File.OpenText(file);
            reader.ReadLine(); // Ignore first line.
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine().Split(',');
                var instance = string.Join(",", line);

                labels.Add(line[0]);
                var inputs = new double[dimensions];
                for (var i = 0; i < dimensions; i++) inputs[i] = double.Parse(line[i]);
                patterns.Add(inputs);
                patterns_with_target_attr.Add(instance);
            }

            reader.Close();
        }

        private void NormalisePatterns()
        {
            for (var j = 0; j < dimensions; j++)
            {
                double max = 0;
                for (var i = 0; i < patterns.Count; i++)
                    if (patterns[i][j] > max)
                        max = patterns[i][j];
                for (var i = 0; i < patterns.Count; i++) patterns[i][j] = patterns[i][j] / max;
            }
        }

        private void Train(double maxError)
        {
            var currentError = double.MaxValue;
            while (currentError > maxError)
            {
                currentError = 0;
                var TrainingSet = new List<double[]>();
                foreach (var pattern in patterns) TrainingSet.Add(pattern);
                for (var i = 0; i < patterns.Count; i++)
                {
                    var pattern = TrainingSet[rnd.Next(patterns.Count - i)];
                    currentError += TrainPattern(pattern);
                    TrainingSet.Remove(pattern);
                }

                Console.WriteLine(currentError.ToString("0.0000000"));
            }
        }

        private double TrainPattern(double[] pattern)
        {
            double error = 0;
            var winner = Winner(pattern);
            for (var i = 0; i < length; i++)
            for (var j = 0; j < length; j++)
                error += outputs[i, j].UpdateWeights(pattern, winner, iteration);
            iteration++;
            return Math.Abs(error / (length * length));
        }

        private void DumpCoordinates()
        {
            for (var i = 0; i < patterns.Count; i++)
            {
                var n = Winner(patterns[i]);
                n.data.Add(patterns[i]);
                n.tr.Add(patterns_with_target_attr[i]);
                for (var j = 0; j < patterns[i].Length; j++)
                    n.total_attr_values[j] += patterns[i][j];
                Console.WriteLine("{0},{1},{2}", i, n.X, n.Y);
            }
        }

        private Neuron Winner(double[] pattern)
        {
            Neuron winner = null;
            var min = double.MaxValue;
            for (var i = 0; i < length; i++)
            for (var j = 0; j < length; j++)
            {
                var d = Distance(pattern, outputs[i, j].Weights);
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
            for (var i = 0; i < vector1.Length; i++) value += Math.Pow(vector1[i] - vector2[i], 2);
            return Math.Sqrt(value);
        }

        public List<string> GetClusteredItems(int xCoord, int yCoord)
        {
            var clusteredItems = new List<string>();

            foreach (var neuron in outputs)
                if (neuron.X == xCoord && neuron.Y == yCoord)
                {
                    foreach (var item in neuron.tr) clusteredItems.Add(item);
                    break;
                }

            return clusteredItems;
        }
    }

    public class Neuron
    {
        public List<double[]> data;
        private readonly int length;
        private readonly double nf;

        public double[] total_attr_values;
        public List<string> tr;
        public double[] Weights;
        public int X;
        public int Y;

        public Neuron(int x, int y, int length)
        {
            X = x;
            Y = y;
            this.length = length;
            nf = 1000 / Math.Log(length);
            data = new List<double[]>();
            tr = new List<string>();
        }

        public double sse1()
        {
            double retV = 0;
            for (var i = 0; i < data.Count; i++) retV += Distance(Weights, data[i]);
            return retV;
        }

        private double Distance(double[] vector1, double[] vector2)
        {
            double value = 0;
            for (var i = 0; i < vector1.Length; i++) value += Math.Pow(vector1[i] - vector2[i], 2);
            return value;
        }

        private double Gauss(Neuron win, int it)
        {
            var distance = Math.Sqrt(Math.Pow(win.X - X, 2) + Math.Pow(win.Y - Y, 2));
            return Math.Exp(-Math.Pow(distance, 2) / Math.Pow(Strength(it), 2));
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
            for (var i = 0; i < Weights.Length; i++)
            {
                var delta = LearningRate(it) * Gauss(winner, it) * (pattern[i] - Weights[i]);
                Weights[i] += delta;
                sum += delta;
            }

            return sum / Weights.Length;
        }
    }
}