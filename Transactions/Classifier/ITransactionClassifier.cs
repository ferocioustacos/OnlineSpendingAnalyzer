using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using TorchSharp;
using TorchSharp.Modules;

namespace SpendingInfo.Transactions.Classifier
{
    public interface ITransactionClassifier
    {
        DeviceType GetDeviceType(); // If called after LoadDevice(), returns the selected device. Otherwise, assumes CPU is used
        DeviceType LoadDevice(); // determines whether to use CPU or GPU
        String Classify(ITransaction t); // Classifies which category the transaction belongs to and returns the label
        IDictionary<String, float> GetProbabilities(); // return the labels and their probabilities
    }
}
