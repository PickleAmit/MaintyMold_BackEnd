using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using DATA;
using Microsoft.ML;
using MLmodel;
using static Microsoft.ML.DataOperationsCatalog;

namespace MLmodel
{
    public class TechnicianPredictionModel
    {
        NetafimDbContext db = new NetafimDbContext();
        MLContext mlContext = new MLContext();
        public Dictionary<string, int> problemTypeEncoding = new Dictionary<string, int>();



        public IDataView PrepareData()
        {
            try
            {
                // Fetch all the rows where ClosingDate, TreatmentHours, Priority and LeadingTechnicianID are not null
                var rawData = db.Errors
                    .Where(e => e.ClosingDate != null && e.TreatmentHours != null && e.PriorityID != null && e.LeadingTechnicianID != null)
                    .ToList();

                // Create a dictionary to store the mapping between 'ProblemType' and an integer

                int currentProblemType = 1; // Start encoding from 1

                foreach (var error in rawData)
                {
                    // If we haven't seen this 'ProblemType' before, add it to the dictionary
                    if (!problemTypeEncoding.ContainsKey(error.Description))
                    {
                        problemTypeEncoding[error.Description] = currentProblemType;
                        currentProblemType++;
                    }
                }

                // Now replace the 'ProblemType' in each record with its corresponding integer
                var data = rawData
                    .Select(e => new ModelInput
                    {
                        ProblemType = (float)problemTypeEncoding[e.Description], // Replace 'ProblemType' string with integer
                        Priority = (float)e.PriorityID.Value,
                        TechnicianID = (uint)e.LeadingTechnicianID.Value,
                        TimeToResolve = (float)e.TreatmentHours.Value
                    })
                    .ToList();

                //foreach (var error in data) { Console.Write($"TimeToResolve - {error.TimeToResolve} "); };

                // Load data into IDataView
                IDataView allData = mlContext.Data.LoadFromEnumerable(data);
                var dataEnumerable = mlContext.Data.CreateEnumerable<ModelInput>(allData, false);
                //foreach (var record in dataEnumerable)
                //{
                //    // Print or log the values to ensure they are correct
                //    Console.WriteLine($"ProblemType: {record.ProblemType}, Priority: {record.Priority}, TechnicianID: {record.TechnicianID}, TimeToResolve: {record.TimeToResolve}|");
                //}


                return allData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in PrepareData: " + ex.Message);
                throw;
            }
        }


        public float EncodeProblemType(string problemType)
        {
            try
            {
                if (problemTypeEncoding.ContainsKey(problemType))
                {
                    return (float)problemTypeEncoding[problemType];
                }
                else
                {
                    return 0f;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in EncodeProblemType: " + ex.Message);
                throw;
            }
        }


        public ITransformer TrainTechnicianModel(IDataView allData)
        {
            try
            {
                var pipeline = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "TechnicianID", outputColumnName: "Label")
                             .Append(mlContext.Transforms.NormalizeMinMax("ProblemType"))
                             .Append(mlContext.Transforms.Concatenate("Features", "ProblemType", "Priority", "TimeToResolve"))
                             .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy())
                             .Append(mlContext.Transforms.Conversion.MapKeyToValue(inputColumnName: "PredictedLabel", outputColumnName: "TechnicianID"));

                var model = pipeline.Fit(allData);

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in TrainTechnicianModel: " + ex.Message);
                throw;
            }
        }


        public void ExperimentWithAlgorithms(IDataView allData)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "experiment_log.txt");
                using (StreamWriter sw = new StreamWriter(logPath, true))
                {
                    // List of algorithms to experiment with
                    var trainers = new List<IEstimator<ITransformer>>()
            {
                mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated(),
                mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(),
                mlContext.MulticlassClassification.Trainers.OneVersusAll(mlContext.BinaryClassification.Trainers.AveragedPerceptron())
            };

                    // Evaluate models trained with different algorithms
                    foreach (var trainer in trainers)
                    {
                        try
                        {
                            sw.WriteLine($"Training with {trainer.ToString()}...");

                            // Define the pipeline
                            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "TechnicianID", outputColumnName: "Label")
                                    .Append(mlContext.Transforms.NormalizeMinMax("ProblemType"))
                                    .Append(mlContext.Transforms.Concatenate("Features", "ProblemType", "Priority", "TimeToResolve"))
                                    .Append(trainer)
                                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(inputColumnName: "PredictedLabel", outputColumnName: "TechnicianID"));

                            // Perform cross-validation to assess model performance
                            var cvResults = mlContext.MulticlassClassification.CrossValidate(allData, pipeline, numberOfFolds: 5);

                            // Average log-loss across the cross-validation folds
                            var averageLogLoss = cvResults.Average(cvResult => cvResult.Metrics.LogLoss);
                            sw.WriteLine($"Average Log-loss for {trainer.ToString()}: {averageLogLoss}");
                        }
                        catch (Exception ex)
                        {
                            sw.WriteLine($"Error while training with {trainer.ToString()}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ExperimentWithAlgorithms: {ex.Message}");
            }
        }



        public uint PredictTechnician(ITransformer model, ModelInput input)
        {
            try
            {
                var predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
                ModelOutput prediction = predictionEngine.Predict(input);
                return prediction.TechnicianID;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in PredictTechnician: " + ex.Message);
                throw;
            }
        }



        public void EvaluateModel(ITransformer model, IDataView testData)
        {
            try
            {
                IDataView predictions = model.Transform(testData);
                var metrics = mlContext.MulticlassClassification.Evaluate(predictions);

                Console.WriteLine($"Log-loss: {metrics.LogLoss}");
                for (int i = 0; i < metrics.PerClassLogLoss.Count; i++)
                {
                    Console.WriteLine($"Log-loss for class {i + 1}: {metrics.PerClassLogLoss[i]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in EvaluateModel: " + ex.Message);
                throw;
            }
        }
        public ITransformer LoadModel()
        {
            ITransformer model;
            string modelDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
            Directory.CreateDirectory(modelDirectory); // Create the directory if it does not exist
            string modelPath = Path.Combine(modelDirectory, "model.zip");

            try
            {
                try
                {
                    model = mlContext.Model.Load(modelPath, out var modelSchema);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading the model: " + ex.Message);

                    var allData = PrepareData();

                    // Experiment with different algorithms
                    ExperimentWithAlgorithms(allData);

                    model = TrainTechnicianModel(allData);

                    // Save your model using modelPath
                    mlContext.Model.Save(model, allData.Schema, modelPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in LoadModel: " + ex.Message);
                throw;
            }
            return model;
        }




        //public ITransformer LoadModel()
        //{
        //    ITransformer model;
        //    try
        //    {
        //        try
        //        {
        //            model = mlContext.Model.Load("model.zip", out var modelSchema);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("Error loading the model: " + ex.Message);

        //            var allData = PrepareData();
        //            model = TrainTechnicianModel(allData);

        //            string modelDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
        //            Directory.CreateDirectory(modelDirectory);  // Create the directory if it does not exist
        //            string modelPath = Path.Combine(modelDirectory, "model.zip");

        //            // Save and load your model using modelPath
        //            mlContext.Model.Save(model, allData.Schema, modelPath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error in LoadModel: " + ex.Message);
        //        throw;
        //    }
        //    return model;
        //}

    }
}