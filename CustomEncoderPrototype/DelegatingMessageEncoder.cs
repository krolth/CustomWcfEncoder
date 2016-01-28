namespace System.ServiceModel.Channels
{
    using IO;
 
    public abstract class DelegatingMessageEncodingBindingElement : MessageEncodingBindingElement
    {
        readonly MessageEncodingBindingElement innerBindingElement;

        protected DelegatingMessageEncodingBindingElement(MessageEncodingBindingElement innerBindingElement)
        {
            this.innerBindingElement = innerBindingElement;
        }
 
        protected DelegatingMessageEncodingBindingElement(DelegatingMessageEncodingBindingElement toBeCloned)
            : base((MessageEncodingBindingElement)toBeCloned.InnerBindingElement.Clone())
        {
        }
 
        protected MessageEncodingBindingElement InnerBindingElement
        {
            get { return this.innerBindingElement; }
        }
 
        public override MessageVersion MessageVersion
        {
            get { return this.innerBindingElement.MessageVersion; }
            set { this.innerBindingElement.MessageVersion = value; }
        }
 
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            context.RemainingBindingElements.Remove(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }
 
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            context.BindingParameters.Add(this);
            context.RemainingBindingElements.Remove(this);
            return context.BuildInnerChannelListener<TChannel>();
        }
 
        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return this.InnerBindingElement.CanBuildChannelFactory<TChannel>(context);
        }
 
        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return this.InnerBindingElement.CanBuildChannelListener<TChannel>(context);
        }
        public override T GetProperty<T>(BindingContext context)
        {
            return this.innerBindingElement.GetProperty<T>(context);
        }
 
        public override BindingElement Clone()
        {
            throw new NotImplementedException();
        }
    }
 
    public abstract class DelegatingMessageEncoderFactory : MessageEncoderFactory
    {
        readonly MessageEncoderFactory innerFactory;

        protected DelegatingMessageEncoderFactory(MessageEncoderFactory innerFactory)
        {
            this.innerFactory = innerFactory;
        }
 
        protected MessageEncoderFactory InnerFactory
        {
            get { return this.innerFactory; }
        }
 
        public override MessageEncoder CreateSessionEncoder()
        {
            return this.innerFactory.CreateSessionEncoder();
        }
 
        public override MessageVersion MessageVersion
        {
            get { return this.innerFactory.MessageVersion; }
        }
 
        public override MessageEncoder Encoder
        {
            get { return this.innerFactory.Encoder; }
        }
    }
 
    public abstract class DelegatingMessageEncoder : MessageEncoder
    {
        readonly MessageEncoder innerEncoder;

        protected DelegatingMessageEncoder(MessageEncoder innerEncoder)
        {
            this.innerEncoder = innerEncoder;
        }
 
        protected MessageEncoder InnerEncoder
        {
            get { return this.innerEncoder; }
        }
 
        public override string ContentType
        {
            get { return this.innerEncoder.ContentType; }
        }
 
        public override string MediaType
        {
            get { return this.innerEncoder.MediaType; }
        }
 
        public override MessageVersion MessageVersion
        {
            get { return this.innerEncoder.MessageVersion; }
        }
 
        public override T GetProperty<T>()
        {
            return this.innerEncoder.GetProperty<T>();
        }
 
        public override bool IsContentTypeSupported(string contentType)
        {
            return this.innerEncoder.IsContentTypeSupported(contentType);
        }
 
        public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
        {
            return this.innerEncoder.ReadMessage(buffer, bufferManager, contentType);
        }
 
        public override Message ReadMessage(Stream stream, int maxSizeOfHeaders, string contentType)
        {
            return this.innerEncoder.ReadMessage(stream, maxSizeOfHeaders, contentType);
        }
 
        public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
        {
            return this.innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
        }
 
        public override void WriteMessage(Message message, Stream stream)
        {
            this.innerEncoder.WriteMessage(message, stream);
        }
    }
}