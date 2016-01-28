namespace System.ServiceModel.Channels
{
    using System;

    // This bindingElement class is the one that actually modifies the MessageBuffer on the WritePath and extracts our property from the MessageBuffer on the ReadPath
    public class WrappedEncodingBindingElement : DelegatingMessageEncodingBindingElement
    {
        public WrappedEncodingBindingElement(BindingElement innerBE)
            : base((MessageEncodingBindingElement)innerBE)
        {
            System.Diagnostics.Debug.Assert(innerBE is BinaryMessageEncodingBindingElement, "WrappedEncodingBindingElement can only wrap a BinaryMessageEncodingBindingElement");
        }
 
        public WrappedEncodingBindingElement()
            : this(new BinaryMessageEncodingBindingElement())
        {
        }
 
        public override MessageEncoderFactory CreateMessageEncoderFactory()
        {
            return new WrappedEncoderFactory(this.InnerBindingElement.CreateMessageEncoderFactory());
        }
         
        public override BindingElement Clone()
        {
            return new WrappedEncodingBindingElement(this.InnerBindingElement.Clone());
        }
 
        class WrappedEncoderFactory : DelegatingMessageEncoderFactory
        {
            MessageEncoder wrappedEncoder;
 
            public WrappedEncoderFactory(MessageEncoderFactory factory)
                : base(factory)
            {
                this.wrappedEncoder = new WrappedEncoder(this.InnerFactory.Encoder);
            }
 
            public override MessageEncoder CreateSessionEncoder()
            {
                return new WrappedEncoder(this.InnerFactory.CreateSessionEncoder());
            }
 
            public override MessageEncoder Encoder
            {
                get { return this.wrappedEncoder; }
            }
        }
 
        class WrappedEncoder : DelegatingMessageEncoder
        {
            public WrappedEncoder(MessageEncoder innerEncoder)
                : base(innerEncoder)
            {}
 
            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                OutOfBandPayloadProperty payload;
 
                if (BinaryFormatHelper.GetAndRemoveHeaderPayload(ref buffer, bufferManager, out payload))
                {
                    Message msg = this.InnerEncoder.ReadMessage(buffer, bufferManager, contentType);
                    msg.Properties.Add(OutOfBandPayloadProperty.Name, payload);
 
                    return msg;
                }
 
                return this.InnerEncoder.ReadMessage(buffer, bufferManager, contentType);
            }
 
            public override ArraySegment<byte> WriteMessage(Message msg, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                // If the message has the payload as a property, we inject it to the message buffer
                object property;
                if (msg.Properties.TryGetValue(OutOfBandPayloadProperty.Name, out property))
                {
                    OutOfBandPayloadProperty payloadProperty = (OutOfBandPayloadProperty)property;
 
                    ArraySegment<byte> origMsgArray = this.InnerEncoder.WriteMessage(msg, maxMessageSize, bufferManager, messageOffset);
 
                    ArraySegment<byte> payload = payloadProperty.GetPayload();
 
                    ArraySegment<byte> msgWithPayload = BinaryFormatHelper.AppendPayloadAsHeader(origMsgArray, payload, bufferManager);
 
                    bufferManager.ReturnBuffer(origMsgArray.Array);
 
                    return msgWithPayload;
                }
                else
                {
                    return this.InnerEncoder.WriteMessage(msg, maxMessageSize, bufferManager, messageOffset);
                }
            }
        }
    }
}