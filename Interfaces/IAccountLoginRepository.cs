﻿using Data_Organizer_Server.Models;
using Google.Cloud.Firestore;

namespace Data_Organizer_Server.Interfaces
{
    public interface IAccountLoginRepository
    {
        Task<DocumentReference> CreateAccountLoginAsync(AccountLogin accountLogin);
    }
}
