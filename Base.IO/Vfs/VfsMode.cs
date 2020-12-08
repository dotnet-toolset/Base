using System;

namespace Base.IO.Vfs
{
    [Flags]
    public enum VfsMode : uint
    {
        None = 0,
        OwnerRead = 1 << 8,
        OwnerWrite = 1 << 7,
        OwnerExecute = 1 << 6,
        OwnerMask = OwnerRead | OwnerWrite | OwnerExecute,
        GroupRead = 1 << 5,
        GroupWrite = 1 << 4,
        GroupExecute = 1 << 3,
        GroupMask = GroupRead | GroupWrite | GroupExecute,
        OthersRead = 1 << 2,
        OthersWrite = 1 << 1,
        OthersExecute = 1 << 0,
        AllRead = OwnerRead | GroupRead | OthersRead,
        AllWrite = OwnerWrite | GroupWrite | OthersWrite,
        AllReadWrite = AllRead | AllWrite,
        OthersMask = OthersRead | OthersWrite | OthersExecute,
        Sticky = 1 << 9,
        SetGid = 1 << 10,
        SetUid = 1 << 11,
        AccessMask = OwnerMask | GroupMask | OthersMask | Sticky | SetGid | SetUid,
        Fifo = 1 << 12,
        Character = 2 << 12,
        Directory = 4 << 12,
        Block = 6 << 12,
        Regular = 8 << 12,
        Link = 10 << 12,
        Socket = 12 << 12,
        TypeMask = 15 << 12
    }
}