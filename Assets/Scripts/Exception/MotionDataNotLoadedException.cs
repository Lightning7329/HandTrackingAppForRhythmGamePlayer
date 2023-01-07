using System;
using System.Runtime.Serialization;

namespace KW_Mocap
{
    [Serializable]
    public class MotionDataNotLoadedException : Exception
    {
        public MotionDataNotLoadedException()
            : base()
        {
        }

        public MotionDataNotLoadedException(string message)
            : base(message)
        {
        }

        public MotionDataNotLoadedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MotionDataNotLoadedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
