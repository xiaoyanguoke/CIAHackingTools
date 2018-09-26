// Guids.cs
// MUST match guids.h
using System;

namespace None.MarbleExtension
{
    static class GuidList
    {
        public const string guidMarbleExtensionPkgString = "57d2c9b6-9c5b-4c9c-8da2-f889519e6ba7";
        public const string guidMarbleExtensionCmdSetString = "0741be6d-a155-4446-b7fd-3ceb6cca3252";

        public static readonly Guid guidMarbleExtensionCmdSet = new Guid(guidMarbleExtensionCmdSetString);
    };
}