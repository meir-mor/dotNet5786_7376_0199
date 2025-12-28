namespace Helpers
{
    [Serializable]
    internal class BlInvalidAddressCoordinatesException : Exception
    {
        public BlInvalidAddressCoordinatesException()
        {
        }

        public BlInvalidAddressCoordinatesException(string? message) : base(message)
        {
        }

        public BlInvalidAddressCoordinatesException(string? message, Exception? innerException) : base(message,
            innerException)
        {
        }
    }
}