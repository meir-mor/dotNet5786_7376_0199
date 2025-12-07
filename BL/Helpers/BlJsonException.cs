
namespace Helpers
{
    [Serializable]
    internal class BlJsonException : Exception
    {
        public BlJsonException()
        {
        }

        public BlJsonException(string? message) : base(message)
        {
        }

        public BlJsonException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}