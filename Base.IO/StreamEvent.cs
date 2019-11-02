using System;
using System.IO;

namespace Base.IO
{
    public abstract class StreamEvent
    {
        public readonly IStreamWrapper Wrapper;

        protected StreamEvent(IStreamWrapper wrapper)
        {
            Wrapper = wrapper;
        }

        public class Disposed : StreamEvent
        {
            public Disposed(IStreamWrapper wrapper) 
                : base(wrapper)
            {
            }
        }

        public class AfterRead : StreamEvent
        {
            public readonly ArraySegment<byte> Buffer;
            public int Result;
            
            internal AfterRead(IStreamWrapper wrapper, byte[] buf, int ofs, int count) 
                : base(wrapper)
            {
                Buffer=new ArraySegment<byte>(buf, ofs, count);
            }
        }

        public class BeforeWrite : StreamEvent
        {
            public readonly ArraySegment<byte> Buffer;

            internal BeforeWrite(IStreamWrapper wrapper, byte[] buf, int ofs, int count) 
                : base(wrapper)
            {
                Buffer=new ArraySegment<byte>(buf, ofs, count);
            }
        }
    }
}