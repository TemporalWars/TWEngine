using System;

namespace Xbox360Generics.NGenerics
{
    // Note: From NGenerics http://code.google.com/p/ngenerics/.
    public static class Guard
    {
        // Methods
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void ArgumentNotNullOrEmptyString(string argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (argumentValue.Length == 0)
            {
                throw new ArgumentException("String cannot be empty.", argumentName);
            }
        }
    }

 

}
