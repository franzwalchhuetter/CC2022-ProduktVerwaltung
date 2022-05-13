using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ProduktVerwaltung.Services;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System;

namespace QueueTriggerAzureFunction
{
    public class MyMessageEntry
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string Text { get; set; }
    }

    public static class QueueFunctions
    {
        [FunctionName("QueueTrigger")]
        [return: Table("MyMessageEntry")]
        public static MyMessageEntry Run([QueueTrigger("produkte", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            myQueueItem = myQueueItem.Trim(new char[] { '"' });

            char[] spearator = { ' ' };
            string[] _message = myQueueItem.Split(spearator);

            float value = float.Parse(_message[_message.Length - 1], CultureInfo.InvariantCulture.NumberFormat);
            string name = _message[_message.Length - 2];
            string _text = "Name: " + name + ", Preis: " + value;
            string _id = _message[0];

            if (value > 100.0)
            {
                return new MyMessageEntry { PartitionKey = "Products", RowKey = _id, Text = _text };
            }
            else
            {
                log.LogInformation("No reason for saving this value: " + value);
                return null;
            }
        }
    }
}