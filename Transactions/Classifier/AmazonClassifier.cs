using System;
using System.Collections.Generic;
using SpendingInfo.Transactions.Transactions;
using TorchSharp;

namespace SpendingInfo.Transactions.Classifier
{
    public class AmazonClassifier : ITransactionClassifier<AmazonTransaction>
    {
        public static AmazonClassifier LoadModel(String modelPath)
        {
            return new AmazonClassifier();
        }

        static ITransactionClassifier<AmazonTransaction> ITransactionClassifier<AmazonTransaction>.LoadModel(string path)
        {
            throw new NotImplementedException();
        }

        public string Classify(AmazonTransaction t)
        {
            throw new NotImplementedException();
        }

        public DeviceType GetDeviceType()
        {
            return DeviceType.CPU;
            throw new NotImplementedException();
        }

        public IDictionary<string, float> GetProbabilities()
        {
            throw new NotImplementedException();
        }

        public DeviceType LoadDevice()
        {
            throw new NotImplementedException();
        }

        public void SaveModel(string path)
        {
            throw new NotImplementedException();
        }
    }
}
