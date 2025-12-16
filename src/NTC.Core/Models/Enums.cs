using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace NTC.Core.Models
{
    public enum AppRole
    {
        [EnumMember(Value = "user")]
        User,

        [EnumMember(Value = "admin")]
        Admin
    }

    public enum FamilyStatus
    {
        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "approved")]
        Approved,

        [EnumMember(Value = "rejected")]
        Rejected
    }
}
