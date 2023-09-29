using System;
using System.Collections.Generic;
using SpendingInfo.Transactions.Transactions;
using TorchSharp;

namespace SpendingInfo.Transactions.Classifier
{
    public interface ITransactionClassifier<T> where T : ITransaction
    {
        DeviceType GetDeviceType(); // If called after LoadDevice(), returns the selected device. Otherwise, assumes CPU is used
        DeviceType LoadDevice(); // determines whether to use CPU or GPU
        String Classify(T t); // Classifies which category the transaction belongs to and returns the label
        IDictionary<String, float> GetProbabilities(); // return the labels and their probabilities

        public void SaveModel(string path);
        public abstract static ITransactionClassifier<T> LoadModel(string path);
    }
}
