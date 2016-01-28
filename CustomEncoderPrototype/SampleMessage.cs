namespace CustomEncoderPrototype
{
    using System.Runtime.Serialization;
 
    [DataContract]
    class SampleMessage
    {
        [DataMember]
        public string EntityName;
 
        [DataMember]
        public string EntityType;
 
        [DataMember]
        public bool PropertyOne;
 
        [DataMember]
        public bool PropertyTwo;
 
        public SampleMessage()
        {
            this.EntityName = "MyEntity";
            this.EntityType = "MyType";
 
            this.PropertyTwo = true;
            this.PropertyOne = true;
        }
    }
}