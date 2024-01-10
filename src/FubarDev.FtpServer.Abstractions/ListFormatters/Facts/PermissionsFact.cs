// <copyright file="PermissionsFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    /// <summary>
    /// The <c>perm</c> fact.
    /// </summary>
    public class PermissionsFact : IFact
    {
        public PermissionsFact(IUnixFileSystemEntry entry)
        {
            int perms = 0;

            perms += entry.Permissions.User.Read ? 256 : 0;
            perms += entry.Permissions.User.Write ? 128 : 0;
            perms += entry.Permissions.User.Execute ? 64 : 0;

            perms += entry.Permissions.Group.Read ? 32 : 0;
            perms += entry.Permissions.Group.Write ? 16 : 0;
            perms += entry.Permissions.Group.Execute ? 8 : 0;

            perms += entry.Permissions.Other.Read ? 4 : 0;
            perms += entry.Permissions.Other.Write ? 2 : 0;
            perms += entry.Permissions.Other.Execute ? 1 : 0;

            Value = Convert.ToString(perms, 8).PadLeft(4, '0');
        }

        public string Name => "UNIX.mode";

        public string Value { get; }
    }
}
