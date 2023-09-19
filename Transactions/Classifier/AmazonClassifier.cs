using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using TorchSharp.Data;
using TorchSharp.Modules;

namespace SpendingInfo.Transactions.Classifier
{
    public class AmazonClassifier : ITransactionClassifier
    {
        public static AmazonClassifier LoadModel(String modelPath)
        {
            //            var seq = Sequential(("lin1", Linear(100, 10)));
            return new AmazonClassifier();
        }
        
        public string Classify(ITransaction t)
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
    }
}
