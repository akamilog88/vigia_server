// Service class which implements the service contract.    
using System;
using System.ServiceModel;
using System.Configuration;

using SenApi.ServicesContract.PubSubContract;

public class CalculatorService : ICalculatorService
{
    public double Add(double n1, double n2)
    {
        double result = n1 + n2;
        Console.WriteLine("Received Add({0},{1})", n1, n2);
        Console.WriteLine("Return: {0}", result);
        this.PublishEvent(result);
        return result;
    }

    public double Subtract(double n1, double n2)
    {
        double result = n1 - n2;
        Console.WriteLine("Received Subtract({0},{1})", n1, n2);
        Console.WriteLine("Return: {0}", result);
        return result;
    }

    public double Multiply(double n1, double n2)
    {
        double result = n1 * n2;
        Console.WriteLine("Received Multiply({0},{1})", n1, n2);
        Console.WriteLine("Return: {0}", result);
        return result;
    }

    public double Divide(double n1, double n2)
    {
        double result = n1 / n2;
        Console.WriteLine("Received Divide({0},{1})", n1, n2);
        Console.WriteLine("Return: {0}", result);
        return result;
    }

    private void PublishEvent(Object result){
        ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("WSHttpBinding_IPubSubEventAPI");
        IPubSubEventAPI client = factory.CreateChannel();
        EventData[] data = {new EventData{Data_ID="result",Data_Val=result}};        
        client.PublishEvent("CalcOperationComplete", new Uri( ConfigurationManager.AppSettings["service_address"]), data);
        factory.Close();        
    }

}