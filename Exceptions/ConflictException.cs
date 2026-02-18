using System.Net;

namespace Assignment_Example_HU.Exceptions
{
    public class ConflictException : BaseException
    {
        public ConflictException(string message) : base(message, HttpStatusCode.Conflict)
        {
        }
    }
}
