// Guids.cs
// MUST match guids.h
using System;

namespace LigerShark.WebJobsVs
{
    static class GuidList
    {
        public const string guidWebJobsVsPkgString = "e42a8517-258b-48d1-a92e-aba474d8def8";
        public const string guidWebJobsVsCmdSetString = "c18c51ba-3e45-4ff2-acae-e8b665020949";

        public static readonly Guid guidWebJobsVsCmdSet = new Guid(guidWebJobsVsCmdSetString);
    };
}