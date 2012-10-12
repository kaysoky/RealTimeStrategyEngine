using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RealTimeStrategyEngine.Processor
{
    public class SpawnRateController
    {
        /// <summary>
        /// Holds a 2D array of double where the first dimension indicates rows and the second dimension indicates columns
        /// </summary>
        class Matrix
        {
            double[,] Values;
            public int NumberOfRows
            {
                get
                {
                    return Values.GetLength(0);
                }
            }
            public int NumberOfColumns
            {
                get
                {
                    return Values.GetLength(1);
                }
            }

            /// <summary>
            /// Converts a list of DataPoints into a Matrix where each row is DataPoint
            /// </summary>
            public Matrix(List<DataPoint> Data, bool isVector)
            {
                if (isVector)
                {
                    Values = new double[Data.Count, 1];
                    for (int i = 0; i < Data.Count; i++)
                    {
                        Values[i, 0] = Data[i].ProcessingTime;
                    }
                }
                else
                {
                    Values = new double[Data.Count, (int)DataPoint.Type.End];
                    for (int i = 0; i < Values.GetLength(0); i++)
                    {
                        for (int j = 0; j < Values.GetLength(1); j++)
                        {
                            Values[i, j] = Data[i].EnumeratedData[j];
                        }
                    }
                }
            }
            /// <summary>
            /// Converts an array of double into a Vector Matrix
            /// </summary>
            public Matrix(double[] Data)
            {
                Values = new double[Data.Length, 1];
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    Values[i, 0] = Data[i];
                }
            }
            private Matrix(double[,] Values)
            {
                this.Values = Values;
            }

            /// <summary>
            /// Adds a row filled with Value at the specified Index
            /// </summary>
            public void AddRow(int Index, double Value)
            {
                double[,] ValuesTemp = new double[Values.GetLength(0) + 1, Values.GetLength(1)];
                for (int i = 0; i < Index; i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        ValuesTemp[i, j] = Values[i, j];
                    }
                }
                for (int i = 0; i < Values.GetLength(1); i++)
                {
                    ValuesTemp[Index, i] = Value;
                }
                for (int i = Index + 1; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        ValuesTemp[i, j] = Values[i - 1, j];
                    }
                }
                Values = ValuesTemp;
            }
            public void RemoveRow(int Index)
            {
                double[,] ValuesTemp = new double[Values.GetLength(0) - 1, Values.GetLength(1)];
                for (int i = 0; i < Index; i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        ValuesTemp[i, j] = Values[i, j];
                    }
                }
                for (int i = Index + 1; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        ValuesTemp[i - 1, j] = Values[i, j];
                    }
                }
                Values = ValuesTemp;
            }
            /// <summary>
            /// Adds a row of Data at the end
            /// </summary>
            public void AppendRow(double[] Data)
            {
                if (Values.GetLength(1) != Data.Length)
                {
                    throw new Exception("Data length is " + Data.Length + " while row size is " + Values.GetLength(1));
                }
                else
                {
                    double[,] ValuesTemp = new double[Values.GetLength(0) + 1, Values.GetLength(1)];
                    for (int i = 0; i < Values.GetLength(0); i++)
                    {
                        for (int j = 0; j < Values.GetLength(1); j++)
                        {
                            ValuesTemp[i, j] = Values[i, j];
                        }
                    }
                    for (int i = 0; i < Data.Length; i++)
                    {
                        ValuesTemp[Values.GetLength(0), i] = Data[i];
                    }
                    Values = ValuesTemp;
                }
            }
            /// <summary>
            /// Adds several rows of Data at the end
            /// </summary>
            public void AppendRows(Matrix Data)
            {
                if (Values.GetLength(1) != Data.Values.GetLength(1))
                {
                    throw new Exception("Data row size is " + Data.Values.GetLength(1) + " while row size is " + Values.GetLength(1));
                }
                else
                {
                    double[,] ValuesTemp = new double[Values.GetLength(0) + Data.Values.GetLength(0), Values.GetLength(1)];
                    for (int i = 0; i < Values.GetLength(0); i++)
                    {
                        for (int j = 0; j < Values.GetLength(1); j++)
                        {
                            ValuesTemp[i, j] = Values[i, j];
                        }
                    }
                    for (int i = Values.GetLength(0); i < ValuesTemp.GetLength(0); i++)
                    {
                        for (int j = 0; j < Values.GetLength(1); j++)
                        {
                            ValuesTemp[i, j] = Data.Values[i - Values.GetLength(0), j];
                        }
                    }
                    Values = ValuesTemp;
                }
            }
            /// <summary>
            /// Adds a row filled with Value at the specified Index
            /// </summary>
            public void AddColumn(int Index, double Value)
            {
                double[,] ValuesTemp = new double[Values.GetLength(0), Values.GetLength(1) + 1];
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Index; j++)
                    {
                        ValuesTemp[i, j] = Values[i, j];
                    }
                }
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    ValuesTemp[i, Index] = Value;
                }
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    for (int j = Index + 1; j < Index; j++)
                    {
                        ValuesTemp[i, j] = Values[i, j - 1];
                    }
                }
                Values = ValuesTemp;
            }

            /// <summary>
            /// NOT USED YET
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static Matrix operator *(Matrix A, Matrix B)
            {
                if (A.Values.GetLength(1) != B.Values.GetLength(0))
                {
                    throw new Exception("First matrix is " + A.Values.GetLength(0) + " by " + A.Values.GetLength(1)
                        + " while the second matrix is " + B.Values.GetLength(0) + " by " + B.Values.GetLength(1));
                }
                else
                {
                    double[,] Result = new double[A.Values.GetLength(0), B.Values.GetLength(1)];
                    for (int i = 0; i < Result.GetLength(0); i++)
                    {
                        for (int j = 0; j < Result.GetLength(1); j++)
                        {
                            for (int k = 0; k < A.Values.GetLength(1); k++)
                            {
                                Result[i, j] += A.Values[i, k] * B.Values[k, j];
                            }
                        }
                    }
                    return new Matrix(Result);
                }
            }
            public Matrix Multiply(Matrix Other)
            {
                if (Values.GetLength(1) != Other.Values.GetLength(0))
                {
                    throw new Exception("First matrix is " + Values.GetLength(0) + " by " + Values.GetLength(1) 
                        + " while the second matrix is " + Other.Values.GetLength(0) + " by " + Other.Values.GetLength(1));
                }
                else
                {
                    double[,] Result = new double[Values.GetLength(0), Other.Values.GetLength(1)];
                    for (int i = 0; i < Result.GetLength(0); i++)
                    {
                        for (int j = 0; j < Result.GetLength(1); j++)
                        {
                            for (int k = 0; k < Values.GetLength(1); k++)
                            {
                                Result[i, j] += Values[i, k] * Other.Values[k, j];
                            }
                        }
                    }
                    return new Matrix(Result);
                }
            }
            public Matrix Multiply(double Value)
            {
                double[,] Result = new double[Values.GetLength(0), Values.GetLength(1)];
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        Result[i, j] = Values[i, j] * Value;
                    }
                }
                return new Matrix(Result);
            }
            public Matrix Add(Matrix Other)
            {
                if (Values.GetLength(0) != Other.Values.GetLength(0) || Values.GetLength(1) != Other.Values.GetLength(1))
                {
                    throw new Exception("First matrix is " + Values.GetLength(0) + " by " + Values.GetLength(1)
                        + " while the second matrix is " + Other.Values.GetLength(0) + " by " + Other.Values.GetLength(1));
                }
                else
                {
                    double[,] Result = new double[Values.GetLength(0), Values.GetLength(1)];
                    for (int i = 0; i < Result.GetLength(0); i++)
                    {
                        for (int j = 0; j < Result.GetLength(1); j++)
                        {
                            Result[i, j] = Values[i, j] + Other.Values[i, j];
                        }
                    }
                    return new Matrix(Result);
                }
            }
            public Matrix Add(double Value)
            {
                double[,] Result = new double[Values.GetLength(0), Values.GetLength(1)];
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        Result[i, j] = Values[i, j] + Value;
                    }
                }
                return new Matrix(Result);
            }
            public Matrix Subtract(Matrix Other)
            {
                if (Values.GetLength(0) != Other.Values.GetLength(0) || Values.GetLength(1) != Other.Values.GetLength(1))
                {
                    throw new Exception("First matrix is " + Values.GetLength(0) + " by " + Values.GetLength(1)
                        + " while the second matrix is " + Other.Values.GetLength(0) + " by " + Other.Values.GetLength(1));
                }
                else
                {
                    double[,] Result = new double[Values.GetLength(0), Values.GetLength(1)];
                    for (int i = 0; i < Result.GetLength(0); i++)
                    {
                        for (int j = 0; j < Result.GetLength(1); j++)
                        {
                            Result[i, j] = Values[i, j] - Other.Values[i, j];
                        }
                    }
                    return new Matrix(Result);
                }
            }
            public Matrix Power(double Value)
            {
                double[,] Result = new double[Values.GetLength(0), Values.GetLength(1)];
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        Result[i, j] = Math.Pow(Values[i, j], Value);
                    }
                }
                return new Matrix(Result);
            }
            public double Sum()
            {
                double Result = 0.0;
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        Result += Values[i, j];
                    }
                }
                return Result;
            }
            public Matrix Transpose()
            {
                double[,] Result = new double[Values.GetLength(1), Values.GetLength(0)];
                for (int i = 0; i < Values.GetLength(0); i++)
                {
                    for (int j = 0; j < Values.GetLength(1); j++)
                    {
                        Result[j, i] = Values[i, j];
                    }
                }
                return new Matrix(Result);
            }
            public double GetValue(int Row, int Column)
            {
                if (Row >= Values.GetLength(0) || Column >= Values.GetLength(1))
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    return Values[Row, Column];
                }
            }
            public void SetValue(int Row, int Column, double Value)
            {
                if (Row >= Values.GetLength(0) || Column >= Values.GetLength(1))
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    Values[Row, Column] = Value;
                }
            }
        }
        public class DataPoint
        {
            public enum Type
            {
                Displacement
                , UnitCount
                , UnitCountSquare
                , HPAverage
                , HPVariance
                , DmgAverage
                , DmgVariance
                , SpeedAverage
                , SpeedVariance
                , RangeAverage
                , RangeVariance
                , End
            }
            public double ProcessingTime;
            public double[] EnumeratedData;

            public DataPoint()
            {
                ProcessingTime = 0.0;
                EnumeratedData = new double[(int)Type.End];
                EnumeratedData[(int)Type.Displacement] = 1.0;
                for (int i = 1; i < EnumeratedData.Length; i++)
                {
                    EnumeratedData[i] = 0.0;
                }
            }
        }

        public static double OptimalProcessingTime = 7.5;
        /// <summary>
        /// Adds onto data such that the data has a mean of 0
        /// </summary>
        public static double[] NormalizationMu;
        /// <summary>
        /// Multiplies data such that the data falls between -1 and 1
        /// </summary>
        public static double[] NormalizationSigma;
        public static Random Chaos;

        List<DataPoint> TrainingSetQueue = new List<DataPoint>();
        public void AddDataToQueue(DataPoint Entry, int NumIterations)
        {
            Entry.ProcessingTime /= NumIterations;
            for (int i = 1; i < (int)DataPoint.Type.End; i++)
            {
                //Divide all data by the number of iterations
                Entry.EnumeratedData[i] /= NumIterations;
                //Normalize the data to match existing data
                Entry.EnumeratedData[i] = (Entry.EnumeratedData[i] + NormalizationMu[i] ) * NormalizationSigma[i];
            }
            TrainingSetQueue.Add(Entry);
        }

        List<DataPoint> RawTrainingSet = new List<DataPoint>();
        List<DataPoint> RawTestSet = new List<DataPoint>();
        Matrix TrainingSet;
        Matrix FittingGoal;
        Matrix Theta;
        double Lambda = 2.0;
        int FailureCount = 0;
        public double RecommendedGeneralSpawnTimer;
        /// <summary>
        /// When positive, represents the error of the current regression Theta + TrainingSet
        /// When negative, represents how much data has been gathered out of what it should have to begin regression
        /// </summary>
        public double FittingError;
        /// <summary>
        /// When the TestSet.Count > 0, TestError is calculated
        /// </summary>
        public double TestError;

        public SpawnRateController()
        {
            Chaos = new Random();
            double[] Theta = new double[(int)DataPoint.Type.End];
            for (int i = 0; i < Theta.Length; i++)
            {
                Theta[i] = 0.0;
            }
            this.Theta = new Matrix(Theta);
            RecommendedGeneralSpawnTimer = Manager.GeneralSpawnTimer;
            NormalizationMu = new double[(int)DataPoint.Type.End];
            NormalizationSigma = new double[(int)DataPoint.Type.End];
            for (int i = 0; i < (int)DataPoint.Type.End; i++)
            {
                NormalizationMu[i] = 0.0;
                NormalizationSigma[i] = 1.0;
            }
        }

        public void ProcessData(object notUsed)
        {
            while (true)
            {
                //Wait for there to be at least 25 DataPoints before starting to learn patterns
                if (TrainingSet == null)
                {
                    int BeginningAmount = 25;
                    FittingError = -(double)TrainingSetQueue.Count / BeginningAmount;
                    if (TrainingSetQueue.Count >= BeginningAmount)
                    {
                        List<DataPoint> newData = new List<DataPoint>();
                        while (TrainingSetQueue.Count > 0)
                        {
                            newData.Add(TrainingSetQueue[0]);
                            TrainingSetQueue.RemoveAt(0);
                        }
                        RawTrainingSet.AddRange(newData);
                        TrainingSet = new Matrix(newData, false);
                        FittingGoal = new Matrix(newData, true); 
                        //Bound the data neatly
                        ReinitializeAndNormalizeData(BeginningAmount);
                        //Run Gradient Descent
                        MinimizeCost(200);
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    //Add any new DataPoints to the TrainingSet
                    if (TrainingSetQueue.Count > 0)
                    {
                        List<DataPoint> newData = new List<DataPoint>();
                        while (TrainingSetQueue.Count > 0)
                        {
                            newData.Add(TrainingSetQueue[0]);
                            TrainingSetQueue.RemoveAt(0);
                        }
                        RawTrainingSet.AddRange(newData);
                        if (newData.Count > 1)
                        {
                            TrainingSet.AppendRows(new Matrix(newData, false));
                            FittingGoal.AppendRows(new Matrix(newData, true));
                        }
                        else
                        {
                            TrainingSet.AppendRow(newData[0].EnumeratedData);
                            FittingGoal.AppendRow(new double[] { newData[0].ProcessingTime });
                        }
                        //Every 25 data points, the data is reprocessed
                        if ((TrainingSet.NumberOfRows + RawTestSet.Count) % 25 == 0)
                        {
                            //Restrict the number of data points so that the algorithm runs in a timely manner
                            if (TrainingSet.NumberOfRows >= 200)
                            {
                                ReinitializeAndNormalizeData(150);
                            }
                            else
                            {
                                ReinitializeAndNormalizeData((TrainingSet.NumberOfRows + RawTestSet.Count) * 3 / 4);
                            }
                        }
                        //Re-run Gradient Descent on the changed data
                        MinimizeCost(Chaos.Next(50, 200));
                    }
                    AdjustProcessingTime();
                }
            }
        }
        private void ReinitializeAndNormalizeData(int OptimalDataLength)
        {
            //Add the test set back into the training set
            RawTrainingSet.AddRange(RawTestSet);
            RawTestSet.Clear();
            //Normalize the dataset to between -1 and 1
            for (int i = 1; i < (int)DataPoint.Type.End; i++)
            {
                //Find the bounds of the dataset
                double Max = 0.0;
                double Min = 0.0;
                for (int j = 0; j < RawTrainingSet.Count; j++)
                {
                    if (RawTrainingSet[j].EnumeratedData[i] > Max)
                    {
                        Max = RawTrainingSet[j].EnumeratedData[i];
                    }
                    if (RawTrainingSet[j].EnumeratedData[i] < Min)
                    {
                        Min = RawTrainingSet[j].EnumeratedData[i];
                    }
                }
                //Calculate the data modification Mu and Sigma
                double deltaMu = -(Max + Min) / 2;
                double deltaSigma = 2.0 / (Max - Min);
                //Calculate the new Normalization Mu and Sigma
                NormalizationMu[i] = NormalizationMu[i] + deltaMu * NormalizationSigma[i];
                NormalizationSigma[i] = NormalizationSigma[i] * deltaSigma;
                //Use the delta Mu and Sigma
                for (int j = 0; j < RawTrainingSet.Count; j++)
                {
                    RawTrainingSet[j].EnumeratedData[i] = (RawTrainingSet[j].EnumeratedData[i] + deltaMu) * deltaSigma;
                }
            }

            //Reinitialize Theta and the other Matrices
            double[] ZeroTheta = new double[(int)DataPoint.Type.End];
            for (int i = 0; i < ZeroTheta.Length; i++)
            {
                ZeroTheta[i] = 0.0;
            }
            //Remove data randomly to keep the data within a certain size
            while (RawTrainingSet.Count > OptimalDataLength)
            {
                int RandomRow = Chaos.Next(RawTrainingSet.Count);
                //Keep the data in a separate set
                RawTestSet.Add(RawTrainingSet[RandomRow]);
                RawTrainingSet.RemoveAt(RandomRow);
            }
            TrainingSet = new Matrix(RawTrainingSet, false);
            FittingGoal = new Matrix(RawTrainingSet, true);

            //Determine an optimal value of the regularization constant by randomization
            List<ThetaAndLambda> TestResults = new List<ThetaAndLambda>();
            for (int i = 0; i < 10; i++)
            {
                this.Theta = new Matrix(ZeroTheta);
                //Run some iterations of Gradient Descent with the new Lambda and Theta
                MinimizeCost(100);
                TestResults.Add(new ThetaAndLambda(Theta, Lambda, CalculateTestCost(RawTestSet, Theta, Lambda)));
                //Randomize afterwards so the original value of Lambda is always in the set
                Lambda = 1.0 + Lambda * 0.5 * (1.0 + Chaos.NextDouble() + Chaos.NextDouble());
            }
            ThetaAndLambda BestLambda = TestResults[0];
            for (int i = 1; i < TestResults.Count; i++)
            {
                if (TestResults[i].Cost < BestLambda.Cost)
                {
                    BestLambda = TestResults[i];
                }
            }
            Theta = BestLambda.Theta;
            Lambda = BestLambda.Lambda;
            //Run more iterations of Gradient Descent on the supposedly best Lambda
            MinimizeCost(200);
        }
        private class ThetaAndLambda
        {
            public Matrix Theta;
            public double Lambda;
            public double Cost;
            public ThetaAndLambda(Matrix Theta, double Lambda, double Cost)
            {
                this.Theta = Theta;
                this.Lambda = Lambda;
                this.Cost = Cost;
            }
        }

        private void MinimizeCost(int Iterations)
        {
            List<Gradient> Gradients = new List<Gradient>();
            //Fill in the values for the current value of Theta
            CalculateGradientDescent(ref Gradients, TrainingSet, FittingGoal, Theta, Lambda);
            //The Learning Rate can start off larger given more Iterations
            double LearningRate = Math.Sqrt(Iterations) / 100;
            double PreviousLearningRate = LearningRate;
            for (int i = 0; i < Iterations; i++)
            {
                //Increment a random Theta by a random amount
                int RandomTheta = Chaos.Next(Gradients.Count);
                CalculateGradientDescent(ref Gradients, TrainingSet, FittingGoal
                    , Gradients[RandomTheta].Theta.Subtract(Gradients[RandomTheta].Differential.Multiply(LearningRate * Chaos.NextDouble()))
                    , Lambda);
                //Decide where next to increment Theta
                Gradient Smallest = Gradients[0];
                for (int j = 1; j < Gradients.Count; j++)
                {
                    if (Gradients[j].Cost < Smallest.Cost)
                    {
                        Smallest = Gradients[j];
                    }
                }
                CalculateGradientDescent(ref Gradients, TrainingSet, FittingGoal
                    , Smallest.Theta.Subtract(Smallest.Differential.Multiply(LearningRate))
                    , Lambda);
                //Check how the cost was changed by the learning rate
                if (Gradients.Last<Gradient>().Cost > Smallest.Cost)
                {
                    PreviousLearningRate = LearningRate;
                    LearningRate *= 0.5;
                }
                else
                {
                    LearningRate = (PreviousLearningRate + LearningRate) / 2.0;
                }
            }
            Gradient BestTheta = Gradients[0];
            for (int i = 1; i < Gradients.Count; i++)
            {
                if (Gradients[i].Cost < BestTheta.Cost)
                {
                    BestTheta = Gradients[i];
                }
            }
            Theta = BestTheta.Theta;
            //Save the costs
            FittingError = BestTheta.Cost;
            TestError = CalculateTestCost(RawTestSet, Theta, Lambda);
        }
        private static void CalculateGradientDescent(ref List<Gradient> Gradients
            , Matrix TrainingSet, Matrix FittingGoal, Matrix Theta, double Lambda)
        {
            //Calculate and add the current Gradient to the lists
            Matrix Regularizer = Theta.Multiply(Lambda);
            Regularizer.SetValue(0, 0, 0.0);
            Regularizer.SetValue(1, 0, 1.0);
            Matrix Gradient = TrainingSet.Transpose()
                .Multiply(TrainingSet.Multiply(Theta).Subtract(FittingGoal));
            Gradient = Gradient.Add(Regularizer).Multiply(1.0 / FittingGoal.NumberOfRows);

            //Add the current Theta to the lists
            //And Calculate and add the current Cost to the lists
            Gradients.Add(new Gradient(Theta, Gradient, CalculateCost(TrainingSet, FittingGoal, Theta, Lambda)));
        }
        private class Gradient
        {
            public Matrix Theta;
            public Matrix Differential;
            public double Cost;
            public Gradient(Matrix Theta, Matrix Differential, double Cost)
            {
                this.Theta = Theta;
                this.Differential = Differential;
                this.Cost = Cost;
            }
        }
        private static double CalculateCost(Matrix TrainingSet, Matrix FittingGoal, Matrix Theta, double Lambda)
        {
            //Given Matrix X = TrainingSet, and Vector Y = FittingGoal
            //Differential = (X * Theta - Y) ^ 2 / NumberOfDataPoints * 0.5
            Matrix Differential = TrainingSet.Multiply(Theta).Subtract(FittingGoal).Power(2.0);
            //Some regularization is applied to penalize large values of the non-linear components of Theta
            Matrix Regularization = Theta.Power(2.0);
            Regularization.SetValue(0, 0, 0.0);
            Regularization.SetValue(1, 0, Theta.GetValue(1, 0));
            return (Differential.Sum() + Lambda * Regularization.Sum()) / (FittingGoal.NumberOfRows * 2.0);
        }
        private static double CalculateTestCost(List<DataPoint> RawTestSet, Matrix Theta, double Lambda)
        {
            if (RawTestSet.Count > 1)
            {
                Matrix TestSet = new Matrix(RawTestSet, false);
                Matrix FittingTest = new Matrix(RawTestSet, true);
                //Given Matrix X = TrainingSet, and Vector Y = FittingGoal
                //Differential = (X * Theta - Y) ^ 2 / NumberOfDataPoints * 0.5
                Matrix Differential = TestSet.Multiply(Theta).Subtract(FittingTest).Power(2.0);
                //Some regularization is applied to penalize large values of the non-linear components of Theta
                Matrix Regularization = Theta.Power(2.0);
                Regularization.SetValue(0, 0, 0.0);
                Regularization.SetValue(1, 0, Theta.GetValue(1, 0));
                return (Differential.Sum() + Lambda * Regularization.Sum()) / (FittingTest.NumberOfRows * 2.0);
            }
            else
            {
                return 0.0;
            }
        }

        private void AdjustProcessingTime()
        {
            //Get the current status of the Game
            double[] Status = new double[(int)DataPoint.Type.End];
            Array.Copy(Manager.LatestStatistics.EnumeratedData, Status, Status.Length);
            for (int i = 1; i < (int)DataPoint.Type.End; i++)
            {
                //Normalize the data to match existing data
                Status[i] = (Status[i] + NormalizationMu[i]) * NormalizationSigma[i];
            }

            double LowerBound = -1.0;  //This is equivalent to a unit count of 0
            double SearchPivot = 0.0;
            double UpperBound = 1.0;

            bool ApproximateFound = false;
            int ExitCounter = 0;
            //Use an expanding binary search to find a good value for X
            while (true)
            {
                //Just change the number of units
                Status[(int)DataPoint.Type.UnitCount] = SearchPivot;
                Status[(int)DataPoint.Type.UnitCountSquare] = 
                    (Math.Pow(SearchPivot / NormalizationSigma[(int)DataPoint.Type.UnitCount] 
                        - NormalizationMu[(int)DataPoint.Type.UnitCount], 2)
                        + NormalizationMu[(int)DataPoint.Type.UnitCountSquare])
                    * NormalizationSigma[(int)DataPoint.Type.UnitCountSquare];
                //Then predice the processing time based on that hypothetical unit count
                Matrix PredictionMatrix = new Matrix(Status);
                PredictionMatrix = Theta.Transpose().Multiply(PredictionMatrix); //Should be a 1x1 matrix
                double Prediction = PredictionMatrix.Sum();
                if (Math.Abs(Prediction - OptimalProcessingTime) < 0.1)
                {
                    ApproximateFound = true;
                    break;
                }
                else
                {
                    //Change the bounds of the search for OptimalProcessingTime
                    if (Prediction > OptimalProcessingTime)
                    {
                        UpperBound = SearchPivot;
                    }
                    else
                    {
                        //Expand the UpperBound while shrinking the LowerBound
                        UpperBound += (UpperBound - LowerBound) / 2;
                        LowerBound = SearchPivot;
                    }
                    SearchPivot = (UpperBound + LowerBound) / 2;
                }
                ExitCounter++;
                if (ExitCounter > 1000)
                {
                    break;
                }
            }
            //Sometimes the function never intersects the Optimal Processing Time
            if (!ApproximateFound)
            {
                //In that case, set the SearchPivot to be equivalent to zero units
                ApproximateFound = true;
                SearchPivot = -1.0;
                FailureCount++;  //And increment the failure count
                RecommendedGeneralSpawnTimer = 1.0;  //And set the timer to its default
            }
            if (ApproximateFound)
            {
                //The program tries to reach the peak processing time within about one second
                //So, un-normalize the SearchPivot and subtract the current unit count
                double SpawnRate = SearchPivot / NormalizationSigma[(int)DataPoint.Type.UnitCount] - NormalizationMu[(int)DataPoint.Type.UnitCount]
                    - Manager.LatestStatistics.EnumeratedData[(int)DataPoint.Type.UnitCount];
                if (SpawnRate > 0.0)
                {
                    RecommendedGeneralSpawnTimer = Manager.controllers.Count / SpawnRate;
                    //Prevent the timers from going berserk or stagnating
                    if (RecommendedGeneralSpawnTimer < 0.01)
                    {
                        RecommendedGeneralSpawnTimer = 0.01;
                    }
                    else if (RecommendedGeneralSpawnTimer > 1.0)
                    {
                        RecommendedGeneralSpawnTimer = 1.0;
                    }
                }
                else
                {
                    //Spawning should never be super slow
                    RecommendedGeneralSpawnTimer = 1.0;
                }
                if (FailureCount > 0)
                {
                    //A success cancels out a failure
                    FailureCount--;
                }
            }
            else
            {
                RecommendedGeneralSpawnTimer = 1.0;
            }
            //When the number of Failed searches becomes too large...
            //There must be a splitable amount of data
            int DataAvailable = RawTrainingSet.Count + RawTestSet.Count;
            if (FailureCount > 100 && DataAvailable > 50)
            {
                ReinitializeAndNormalizeData(
                    DataAvailable > 200 ? 150 : DataAvailable * 3 / 4);
            }
            Thread.Sleep(1);
        }
        private class ValueAndPrediction
        {
            double Value;
            double Prediction;
            public ValueAndPrediction(double Value, double Prediction)
            {
                this.Value = Value;
                this.Prediction = Prediction;
            }
        }
    }
}
