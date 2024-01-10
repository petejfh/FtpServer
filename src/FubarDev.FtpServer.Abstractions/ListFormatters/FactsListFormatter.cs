// <copyright file="FactsListFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Compatibility;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters.Facts;
using FubarDev.FtpServer.Utilities;

namespace FubarDev.FtpServer.ListFormatters
{
    /// <summary>
    /// A formatter for the <c>MLST</c> command.
    /// </summary>
    public class FactsListFormatter : IListFormatter
    {
        private readonly ClaimsPrincipal _user;

        private readonly DirectoryListingEnumerator _enumerator;

        private readonly ISet<string> _activeFacts;

        private readonly bool _absoluteName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactsListFormatter"/> class.
        /// </summary>
        /// <param name="user">The user to create this formatter for.</param>
        /// <param name="enumerator">The enumerator for the directory listing to format.</param>
        /// <param name="activeFacts">The active facts to return for the entries.</param>
        /// <param name="absoluteName">Returns an absolute entry name.</param>
        [Obsolete("Use the overload with ClaimsPrincipal.")]
        public FactsListFormatter(IFtpUser user, DirectoryListingEnumerator enumerator, ISet<string> activeFacts, bool absoluteName)
        {
            _user = user.CreateClaimsPrincipal();
            _enumerator = enumerator;
            _activeFacts = activeFacts;
            _absoluteName = absoluteName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactsListFormatter"/> class.
        /// </summary>
        /// <param name="user">The user to create this formatter for.</param>
        /// <param name="enumerator">The enumerator for the directory listing to format.</param>
        /// <param name="activeFacts">The active facts to return for the entries.</param>
        /// <param name="absoluteName">Returns an absolute entry name.</param>
        public FactsListFormatter(ClaimsPrincipal user, DirectoryListingEnumerator enumerator, ISet<string> activeFacts, bool absoluteName)
        {
            _user = user;
            _enumerator = enumerator;
            _activeFacts = activeFacts;
            _absoluteName = absoluteName;
        }

        /// <inheritdoc/>
        public string Format(IUnixFileSystemEntry entry, string? name)
        {
            switch (name)
            {
                case ".":
                    return FormatThisDirectoryEntry();
                case "..":
                    return FormatParentDirectoryEntry();
                default:
                {
                    var currentDirEntry = _enumerator.CurrentDirectory;
                    if (entry is IUnixDirectoryEntry dirEntry)
                    {
                        return BuildLine(BuildFacts(currentDirEntry, dirEntry, new TypeFact(dirEntry)), dirEntry.IsRoot ? string.Empty : name ?? entry.Name);
                    }

                    return BuildLine(BuildFacts(_enumerator.FileSystem, currentDirEntry, (IUnixFileEntry)entry), name ?? entry.Name);
                }
            }
        }

        private string FormatThisDirectoryEntry()
        {
            return BuildLine(BuildFacts(_enumerator.ParentDirectory, _enumerator.CurrentDirectory, new CurrentDirectoryFact()), ".");
        }

        private string FormatParentDirectoryEntry()
        {
            if (_enumerator.ParentDirectory == null)
            {
                throw new InvalidOperationException("Internal program error: The parent directory could not be determined.");
            }

            return BuildLine(BuildFacts(_enumerator.GrandParentDirectory, _enumerator.ParentDirectory, new ParentDirectoryFact()), "..");
        }

        private string BuildLine(IEnumerable<IFact> facts, string entryName)
        {
            var result = new StringBuilder();
            foreach (var fact in facts.Where(fact => _activeFacts.Contains(fact.Name)))
            {
                result.AppendFormat("{0}={1};", fact.Name, fact.Value);
            }

            var fullName = _absoluteName ? _enumerator.GetFullPath(entryName) : entryName;
            result.AppendFormat(" {0}", fullName);
            return result.ToString();
        }

        private IReadOnlyList<IFact> BuildFacts(IUnixDirectoryEntry? parentEntry, IUnixDirectoryEntry currentEntry, TypeFact typeFact)
        {
            var result = new List<IFact>()
            {
                new PermissionsFact(currentEntry),
                typeFact,
            };
            if (currentEntry.LastWriteTime.HasValue)
            {
                result.Add(new ModifyFact(currentEntry.LastWriteTime.Value));
            }

            if (currentEntry.CreatedTime.HasValue)
            {
                result.Add(new CreateFact(currentEntry.CreatedTime.Value));
            }

            result.Add(new UnixOwnerFact(currentEntry.Owner));
            result.Add(new UnixGroupFact(currentEntry.Group));

            return result;
        }

        private IReadOnlyList<IFact> BuildFacts(IUnixFileSystem fileSystem, IUnixDirectoryEntry directoryEntry, IUnixFileEntry entry)
        {
            var result = new List<IFact>()
            {
                new PermissionsFact(entry),
                new SizeFact(entry.Size),
                new TypeFact(entry),
            };
            if (entry.LastWriteTime.HasValue)
            {
                result.Add(new ModifyFact(entry.LastWriteTime.Value));
            }

            if (entry.CreatedTime.HasValue)
            {
                result.Add(new CreateFact(entry.CreatedTime.Value));
            }

            result.Add(new UnixOwnerFact(directoryEntry.Owner));
            result.Add(new UnixGroupFact(directoryEntry.Group));

            return result;
        }
    }
}
