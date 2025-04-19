namespace Data_Organizer_Server.Interfaces
{
    public interface IEncryptionService
    {
        string Decrypt(string cipherText);
        string Encrypt(string plainText);
    }
}
