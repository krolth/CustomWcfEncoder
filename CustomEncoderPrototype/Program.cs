namespace CustomEncoderPrototype
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    // Adds a custom payload into a binary message and then extracts it out before passing it onto the upper encoder.
    // Includes 3 tests to validate functionality
    class CustomEncoderPrototype
    {
        static void Main(string[] args)
        {
            var test = new CustomEncoderPrototype();
            test.EncodeDecodeNoWcf(); 
            test.EncodeDecodeWithWcf();
            test.EncodeDecodeWithSecureWcf();
        }
 
        // This test does not go through all the WCF layers. It just uses a MessageEncoderFactory to Write and Read the message.
        // It also validates that the property that we received from the outputMsg has the same content we set originally
        void EncodeDecodeNoWcf()
        {
            var buffermanager = BufferManager.CreateBufferManager(1024 * 1024, 1024 * 1024); 
            Message msg = WcfHelpers.CreateMessage();

            string originalPayloadValue = Guid.NewGuid().ToString();
            WcfHelpers.AddOutOfBandPropertyToMessage(originalPayloadValue, msg);
 
            MessageEncoder encoder = new WrappedEncodingBindingElement().CreateMessageEncoderFactory().Encoder;
            ArraySegment<byte> msgBytes = encoder.WriteMessage(msg, Int32.MaxValue, buffermanager);
 
            MessageEncoder decoder = new WrappedEncodingBindingElement().CreateMessageEncoderFactory().Encoder;
            Message outputMsg = decoder.ReadMessage(msgBytes, buffermanager);

            Console.WriteLine("\nEncoding with no WCF");
            CustomEncoderPrototype.ValidateResult(originalPayloadValue, outputMsg, string.Empty);
        }
 
        // This test sets a service that user our custom encoder/decoder.
        // the service actually modifies the property after deserializing it to simulate what
        // a forwarder/gateway/proxy might do.
        void EncodeDecodeWithWcf()
        {
            string address = "net.tcp://localhost:8080";
            ServiceHost host = new ServiceHost(new Service(), new Uri(address));
 
            Binding binding = WcfHelpers.CreateCustomBinding();
             
            host.AddServiceEndpoint(typeof(IService1), binding, address);
            host.Open();
 
            ChannelFactory<IService1> factory = new ChannelFactory<IService1>(binding, address);
            factory.Open();
            IService1 proxy = factory.CreateChannel();
            ((IChannel)proxy).Open();
 
            Message msg = WcfHelpers.CreateMessage();
            string originalPayloadValue = Guid.NewGuid().ToString();
            WcfHelpers.AddOutOfBandPropertyToMessage(originalPayloadValue, msg);
 
            Message outputMsg = proxy.Operation(msg);

            Console.WriteLine("\nEncoding with WCF");
            ValidateResult(originalPayloadValue, outputMsg, Service.TransportModifier);
 
            ((IChannel)proxy).Close();
            host.Close();
        }

        // This test sets a secure service that user our custom encoder/decoder.
        // the service actually modifies the property after deserializing it to simulate what
        // a forwarder/gateway/proxy might do.
        void EncodeDecodeWithSecureWcf()
        {
            Binding binding = WcfHelpers.CreateCustomSecureBinding();
 
            string address = "net.tcp://localhost:8080";
            ServiceHost host = new ServiceHost(new Service(), new Uri(address));
             
            host.AddServiceEndpoint(typeof(IService1), binding, address);
            host.Open();
 
            ChannelFactory<IService1> factory = new ChannelFactory<IService1>(binding, address);
            factory.Open();
            IService1 proxy = factory.CreateChannel();
            ((IChannel)proxy).Open();
 
            Message msg = WcfHelpers.CreateMessage();

            string originalPayloadValue = Guid.NewGuid().ToString();
            WcfHelpers.AddOutOfBandPropertyToMessage(originalPayloadValue, msg);
            
            Message outputMsg = proxy.Operation(msg);

            Console.WriteLine("\nEncoding with secure WCF");
            ValidateResult(originalPayloadValue, outputMsg, Service.TransportModifier);
 
            ((IChannel)proxy).Close();
            host.Close();
        }
 
        static void ValidateResult(string originalPayload, Message outputMsg, string transportModifier)
        {
            object returnPayload;
 
            if (outputMsg.Properties.TryGetValue(OutOfBandPayloadProperty.Name, out returnPayload))
            {
                OutOfBandPayloadProperty payloadProperty = (OutOfBandPayloadProperty)returnPayload;
                ArraySegment<byte> payloadBytes = payloadProperty.GetPayload();
 
                string receivedPayload = Encoding.ASCII.GetString(payloadBytes.Array, payloadBytes.Offset, payloadBytes.Count );

                bool receivedMatchesOriginal = (originalPayload == receivedPayload);
                bool receivedMatchedModifiedOriginal = (originalPayload + transportModifier) == receivedPayload;

                Console.WriteLine("Data from the payload is equal to what I originally inserted: " + receivedMatchesOriginal.ToString());
                Console.WriteLine("Data from the payload is equal to what I originally inserted plus transport modifier: " + receivedMatchedModifiedOriginal.ToString());
                Console.WriteLine("Sent Payload: " + originalPayload);
                Console.WriteLine("Received Payload: " + receivedPayload);
            }
            else
            {
                Console.WriteLine("PAYLOAD NOT RECEIVED!!!");
            }
 
        }
    }
}