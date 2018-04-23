﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.Core.Models.Table;
using Core.Models.Data;
using System;
using System.IO;

namespace Bit.Core.Services
{
    public interface ICipherService
    {
        Task SaveAsync(Cipher cipher, Guid savingUserId, bool orgAdmin = false);
        Task SaveDetailsAsync(CipherDetails cipher, Guid savingUserId);
        Task CreateAttachmentAsync(Cipher cipher, Stream stream, string fileName, long requestLength, Guid savingUserId,
            bool orgAdmin = false);
        Task CreateAttachmentShareAsync(Cipher cipher, Stream stream, string fileName, long requestLength, string attachmentId,
            Guid organizationShareId);
        Task DeleteAsync(Cipher cipher, Guid deletingUserId, bool orgAdmin = false);
        Task DeleteManyAsync(IEnumerable<Guid> cipherIds, Guid deletingUserId);
        Task DeleteAttachmentAsync(Cipher cipher, string attachmentId, Guid deletingUserId, bool orgAdmin = false);
        Task MoveManyAsync(IEnumerable<Guid> cipherIds, Guid? destinationFolderId, Guid movingUserId);
        Task SaveFolderAsync(Folder folder);
        Task DeleteFolderAsync(Folder folder);
        Task ShareAsync(Cipher originalCipher, Cipher cipher, Guid organizationId, IEnumerable<Guid> collectionIds, Guid userId);
        Task SaveCollectionsAsync(Cipher cipher, IEnumerable<Guid> collectionIds, Guid savingUserId, bool orgAdmin);
        Task ImportCiphersAsync(List<Folder> folders, List<CipherDetails> ciphers,
            IEnumerable<KeyValuePair<int, int>> folderRelationships);
        Task ImportCiphersAsync(List<Collection> collections, List<CipherDetails> ciphers,
            IEnumerable<KeyValuePair<int, int>> collectionRelationships, Guid importingUserId);
    }
}
