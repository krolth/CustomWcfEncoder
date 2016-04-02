# CustomWcfEncoder
Custom WCF encoder that inserts properties as part of the message buffer.

Adding and receiving headers can be expensive in WCF because it uses XML. So to read any value we need to create a valid XML document and deserialize it to grab the property value. 
This solution allows a service to set a property instead of a header. However since typically these are not passed through the wire, my extension takes the message properties and appends them to the message buffer. The reader layer would have another piece that removes the property from the message buffer and creates a new property with that value. 

This means that instead of serializing/deserializing to XML the only cost will be moving data around. In my micro benchmarks I saw around 6-7% improvement.  

Here are the files doing the bulk of the work:

* WrappedEncodingBindingElement – This bindingElement class is the one that actually modifies the MessageBuffer on the WritePath and extracts our property from the MessageBuffer on the ReadPath
* BinaryFormatHelper – This is the Helper class that modifies a Buffer to include the desired payload (AppendPayloadAsHeader). It also verifies if a given buffer includes our payload and if it does it extracts it from the buffer (GetAndRemoveHeaderPayload). It uses bufferManager for pooling our buffers (avoid generating garbage) and it also uses Buffer.BlockCopy to copy data efficiently.
* OutOfBandPayloadProperty – Our custom property. Includes the payload and the name so that we can distinguish it from other message properties
* WcfHelpers – Helpers for creating WCF messages, custom bindings and the test service
* Program.cs – includes 3 tests: EncodeDecodeNoWcf, EncodeDecodeWithWcf and EncodeDecodeWithSecureWcf

Full writeup here: https://blogs.msdn.microsoft.com/shacorn/2016/01/14/wcf-headers-as-properties/
