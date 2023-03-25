// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*============================================================
**
**
**
** Purpose: Attribute for functions, etc that will be removed.
**
**
===========================================================*/

namespace RayGui_cs
{
    // Temporary attribute used for marking methods as being not reviewed and thus will either stay as is or be moved to ControlResponses.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum |
        AttributeTargets.Interface | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate,
        Inherited = false)]
#if SYSTEM_PRIVATE_CORELIB
    public
#else
    internal
#endif
    sealed class NotReviewedAttribute : Attribute
    {
        public NotReviewedAttribute()
        {
        }

        public NotReviewedAttribute(string? message)
        {
            Message = message;
        }

        public NotReviewedAttribute(string? message, bool error)
        {
            Message = message;
            IsError = error;
        }

        public string? Message { get; }

        public bool IsError { get; }

        public string? DiagnosticId { get; set; }

        public string? UrlFormat { get; set; }
    }
}
