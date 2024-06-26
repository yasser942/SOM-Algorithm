# Self-Organizing Map (SOM) Application

## Overview
This is a simple implementation of a Self-Organizing Map (SOM) application using C# and Windows Forms. SOM is a type of artificial neural network used for clustering and visualizing high-dimensional data in lower dimensions. The application provides a graphical interface to visualize the SOM grid and interact with clustered data.

## Features
- **SOM Training**: Trains a SOM based on input data provided through a text file.
- **Grid Visualization**: Displays the SOM grid as a grid of buttons, where each button represents a neuron in the SOM.
- **Data Clustering**: Allows users to click on grid cells to view the clustered data associated with that specific neuron.
- **Data Normalization**: Normalizes input data to facilitate effective training of the SOM.
- **Error Monitoring**: Displays the Sum of Squared Errors (SSE) during SOM training to monitor the convergence of the model.

## Usage
1. **Data Input**: Click on the "Browse" button to select a text file containing the input data. The format of the text file should be comma-separated values (CSV), where each row represents a data instance.
2. **Grid Dimension**: Optionally, specify the dimension of the SOM grid using the provided textbox. If left empty, the default dimension is 3.
3. **Training**: Click on the "Run SOM" button to start training the SOM based on the provided input data and grid dimension.
4. **Grid Interaction**: After training, the SOM grid will be displayed. Click on any grid cell (button) to view the clustered data associated with the corresponding neuron.
5. **Error Monitoring**: During training, the application will print the Sum of Squared Errors (SSE) in the console window.

## Dependencies
- **C#**: The application is developed using C# programming language.
- **Windows Forms**: The graphical user interface (GUI) is built using Windows Forms.
- **.NET Framework**: Requires .NET Framework to run.

## Installation
1. Clone or download the repository to your local machine.
2. Open the project in Visual Studio or any compatible IDE.
3. Build the solution to compile the application.
4. Run the executable file (.exe) generated after compilation.


