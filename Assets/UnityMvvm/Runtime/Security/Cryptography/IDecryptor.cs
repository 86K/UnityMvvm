

namespace Fusion.Mvvm
{
    public interface IDecryptor
    {
        string AlgorithmName { get; }

        byte[] Decrypt(byte[] buffer);

    }
}