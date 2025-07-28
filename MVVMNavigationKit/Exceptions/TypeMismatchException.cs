using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMNavigationKit.Exceptions
{
    public class TypeMismatchException : InvalidOperationException
    {
        public Type? ExpectedType { get; }
        public Type? ActualType { get; }

        public TypeMismatchException(Type expectedType, Type? actualType)
            : base($"Expected type {expectedType.Name}, but got {actualType?.Name ?? "null"}")
        {
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public TypeMismatchException()
            : base(string.Empty) { }

        public TypeMismatchException(string? message)
            : base(message) { }

        public TypeMismatchException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
