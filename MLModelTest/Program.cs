using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using MLmodel;

namespace MLModelTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Checking model -");
            // Create an instance of your MLModel class
            var modelClass = new TechnicianPredictionModel();

            // Prepare the data
            var data = modelClass.PrepareData();

            // Experiment with different algorithms
            modelClass.ExperimentWithAlgorithms(data);

            // Train the model
            var model = modelClass.TrainTechnicianModel(data);

            // Now you can predict the best technician for a new problem
            var input = new ModelInput
            {
                ProblemType = modelClass.EncodeProblemType("תקלת חדירת שמן"), // Now encoded with dictionary
                Priority = 3
            };
            var bestTechnician = modelClass.PredictTechnician(model, input);

            // Evaluate the model
            var testData = modelClass.PrepareData(); // If you have separate test data, use it here.
            modelClass.EvaluateModel(model, testData);

            Console.WriteLine($"The best technician for this problem is: {bestTechnician}");
            Console.ReadLine();
        }
    }
}
