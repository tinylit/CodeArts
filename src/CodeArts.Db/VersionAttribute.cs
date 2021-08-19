using System;

namespace CodeArts.Db
{
    /// <summary>
    /// 版本（幂等）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public sealed class VersionAttribute : Attribute
    {
    }
}
