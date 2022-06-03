using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCL
{
    public class MemoryReadWriteException : ApplicationException {
        public MemoryReadWriteException() : base() { }
        public MemoryReadWriteException(string message) : base(message) { }
    }
    public class ProcessNotFoundException : ApplicationException { 
        public ProcessNotFoundException() : base() { }
        public ProcessNotFoundException(string message) : base(message) { }
    }
    public class ProcessHasExitedException : ApplicationException {
        public ProcessHasExitedException() : base() { }
        public ProcessHasExitedException(string message) : base(message) { }
    }
}
