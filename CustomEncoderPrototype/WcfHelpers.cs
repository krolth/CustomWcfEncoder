using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace CustomEncoderPrototype
{
    // Helpers for creating WCF messages, custom bindings and the test service
    static class WcfHelpers
    {
        public static Binding CreateCustomBinding()
        {
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            
            return new CustomBinding(CreateWrappedEncodingBinding(binding));
        }

        public static Binding CreateCustomSecureBinding()
        {
            NetTcpBinding binding = new NetTcpBinding();

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            
            return new CustomBinding(CreateWrappedEncodingBinding(binding));
        }

        public static void AddOutOfBandPropertyToMessage(string propertyPayload, Message message)
        {
            byte[] payloadArray = Encoding.ASCII.GetBytes(propertyPayload);
            message.Properties[OutOfBandPayloadProperty.Name] = new OutOfBandPayloadProperty(new ArraySegment<byte>(payloadArray));
        }

        public static Message CreateMessage()
        {
            Message msg = Message.CreateMessage(MessageVersion.Soap12WSAddressing10, "hello", "hello2");
            msg.Headers.To = new Uri("http://contoso.com/encoderTest");
            msg.Headers.Add(MessageHeader.CreateHeader("LinkInfo", "http://test.com/testNamespace", new SampleMessage()));
            return msg;
        }

        private static CustomBinding CreateWrappedEncodingBinding(NetTcpBinding binding)
        {
            CustomBinding cb = new CustomBinding(binding);
            MessageEncodingBindingElement encoder = cb.Elements.Find<BinaryMessageEncodingBindingElement>();
            int index = cb.Elements.IndexOf(encoder);

            cb.Elements[index] = new WrappedEncodingBindingElement(encoder);

            // Set long timeouts to ease debugging
            cb.SendTimeout = TimeSpan.MaxValue;
            cb.ReceiveTimeout = TimeSpan.MaxValue;
            cb.OpenTimeout = TimeSpan.MaxValue;
            TcpTransportBindingElement tcpTransport = cb.Elements.Find<TcpTransportBindingElement>();
            tcpTransport.ChannelInitializationTimeout = TimeSpan.FromSeconds(120);
            return cb;
        }
    }

    // Service Contract and Interface
    [ServiceContract]
    interface IService1
    {
        [OperationContract(Action = "*", ReplyAction = "*")]
        Message Operation(Message message);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class Service : IService1
    {
        public const string TransportModifier = "__Modified";

        public Message Operation(Message message)
        {
            //Forwarder. Modifies the payload to simulate header modifications common in a proxy
            object returnPayload;
            if (message.Properties.TryGetValue(OutOfBandPayloadProperty.Name, out returnPayload))
            {
                OutOfBandPayloadProperty property = (OutOfBandPayloadProperty)returnPayload;
                string newPayload = property.Deserialize(property.GetPayload()) + TransportModifier;

                byte[] payloadArray = Encoding.ASCII.GetBytes(newPayload);

                message.Properties[OutOfBandPayloadProperty.Name] = new OutOfBandPayloadProperty(new ArraySegment<byte>(payloadArray));
            }

            return message;
        }
    }
}