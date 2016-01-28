namespace System.ServiceModel.Channels
{
    // Our custom property. Includes the payload and the name so that we can distinguish it from other message properties
    class OutOfBandPayloadProperty
    {
        public const string Name = "OutOfBandPayloadProperty";
        public object TypedPayload;
                 
        public OutOfBandPayloadProperty(ArraySegment<byte> payload)
        {
            this.TypedPayload = this.Deserialize(payload);
        }
 
        public ArraySegment<byte> GetPayload()
        {
            byte[] payloadArray = Text.Encoding.ASCII.GetBytes((string)this.TypedPayload);
 
            return new ArraySegment<byte>(payloadArray);
        }
 
        public object Deserialize(byte[] payload)
        {
            return Text.Encoding.ASCII.GetString(payload);
        }
 
        public object Deserialize(ArraySegment<byte> payload)
        {
            return Text.Encoding.ASCII.GetString(payload.Array, payload.Offset, payload.Count);
        }
    }
}