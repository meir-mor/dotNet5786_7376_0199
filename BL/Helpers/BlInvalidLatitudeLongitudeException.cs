
namespace Helpers
{
    [Serializable]
    internal class BlInvalidLatitudeLongitudeException : Exception
    {
        public BlInvalidLatitudeLongitudeException()
        {
        }

        public BlInvalidLatitudeLongitudeException(string? message) : base(message)
        {
        }

        public BlInvalidLatitudeLongitudeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}