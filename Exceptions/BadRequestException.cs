using System.Net;

namespace Assignment_Example_HU.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
