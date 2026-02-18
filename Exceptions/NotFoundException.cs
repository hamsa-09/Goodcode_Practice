using System.Net;

namespace Assignment_Example_HU.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string message) : base(message, HttpStatusCode.NotFound)
        {
        }
    }
}
