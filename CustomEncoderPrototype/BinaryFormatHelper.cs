namespace System.ServiceModel.Channels
{
    // This is the Helper class that modifies the Message Buffer to include the desired payload (AppendPayloadAsHeader).
    // it also verifies if a given buffer includes our payload and if it does it extracts it from the buffer (GetAndRemoveHeaderPayload) 
    // Note that we use bufferManager for pooling our buffers. We also use Buffer.BlockCopy to copy data efficiently
    class BinaryFormatHelper
    {
        // This is what we write before our payload to mark it: 0xBE
        // This value will not be at the beginning of a regular message (it's a reserved value not in use according to the spec/code)
        const byte PayloadMark = 0xBE;
        public const int PayloadMarkLength = 1;
        public const int PayloadFrameLength = PayloadLengthByteCount + PayloadMarkLength;
        const int PayloadLengthByteCount = 4;
 
        public static ArraySegment<byte> AppendPayloadAsHeader(ArraySegment<byte> buffer, ArraySegment<byte> payload, BufferManager bufferManager)
        {
            // We will insert at the beginning of the buffer just before the dictionary or any message content
            int posToInsert = buffer.Offset;
             
            int frameSize = PayloadMarkLength + PayloadLengthByteCount + payload.Count;
 
            byte[] newBuffer = bufferManager.TakeBuffer(buffer.Count + frameSize + buffer.Offset);
 
            int afterContent = posToInsert + frameSize;
 
            // Insert our custom mark 0xBE
            int insertPoint = posToInsert;
            newBuffer[insertPoint++] = PayloadMark;
 
            // Insert the length of the payload
            byte[] lenInBytes = BitConverter.GetBytes(payload.Count);
            newBuffer[insertPoint++] = lenInBytes[0];
            newBuffer[insertPoint++] = lenInBytes[1];
            newBuffer[insertPoint++] = lenInBytes[2];
            newBuffer[insertPoint++] = lenInBytes[3];
 
            Buffer.BlockCopy(payload.Array, payload.Offset, newBuffer, insertPoint, payload.Count);
 
            // Copy from old to new array
            Buffer.BlockCopy(buffer.Array, 0, newBuffer, 0, posToInsert); // Copy until the subheader pointer
            Buffer.BlockCopy(buffer.Array, posToInsert, newBuffer, afterContent, buffer.Count - posToInsert + buffer.Offset); // Now copy the rest
 
            return new ArraySegment<byte>(newBuffer, buffer.Offset, buffer.Count + frameSize);
        }
 
        public static bool GetAndRemoveHeaderPayload(ref ArraySegment<byte> buffer, BufferManager bufferManager, out OutOfBandPayloadProperty payload)
        {
            int ptrPayload;
            if (DoesBufferContainPayload(buffer, out ptrPayload))
            {
                int endOfPayload;
 
                // Get the payload content
                ArraySegment<byte> payloadSegment = BinaryFormatHelper.GetPayloadSegment(buffer, ptrPayload, out endOfPayload);
                payload = new OutOfBandPayloadProperty(payloadSegment);
 
                // Remove it from the message (In place)
                int totalPayloadSize = endOfPayload - ptrPayload;
                int newBufferSize = buffer.Offset + buffer.Count - totalPayloadSize;
 
                Buffer.BlockCopy(buffer.Array, endOfPayload, buffer.Array, ptrPayload, buffer.Count - endOfPayload);
                 
                // Return the new buffer without the payload
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset, buffer.Count - totalPayloadSize);
 
                return true;
            }
 
            payload = null;
            return false;
        }
 
        public static ArraySegment<byte> GetPayloadSegment(ArraySegment<byte> buffer, int posPayloadMark, out int endOfPayload)
        {
            int ptrPayloadStart = posPayloadMark + PayloadMarkLength;
             
            int lenPayload = BitConverter.ToInt32(buffer.Array, ptrPayloadStart);
 
            endOfPayload = ptrPayloadStart + PayloadLengthByteCount + lenPayload;
 
            return new ArraySegment<byte>(buffer.Array, ptrPayloadStart + PayloadLengthByteCount, lenPayload);
        }
         
        public static bool DoesBufferContainPayload(ArraySegment<byte> buffer, out int ptrPayload)
        {
            ptrPayload = buffer.Offset;
 
            // Check for our custom mark for the payload 0xBE.
            // If the buffer includes a Dictionary, the first element will never start with a "1" because it is a MultiByteUInt31
            // http://msdn.microsoft.com/en-us/library/cc219225(v=prot.10).aspx
            // if the buffer does not include a dictionary the spec defines that 0xBE is not one of their supported tags 
 
            return (buffer.Array[ptrPayload] == PayloadMark);
        }
    }
}