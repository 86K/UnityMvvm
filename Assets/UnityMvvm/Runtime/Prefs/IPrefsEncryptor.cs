

namespace Fusion.Mvvm
{
    /// <summary>
    /// A interface for encoding and decoding preference data.
    /// </summary>
    public interface IPrefsEncryptor
    {
        /// <summary>
        /// encryption
        /// </summary>
        /// <param name="plainData">The plain data.</param>
        /// <returns></returns>
        byte[] Encode(byte[] plainData);

        /// <summary>
        /// decryption
        /// </summary>
        /// <param name="cipherData">The cipher data.</param>
        /// <returns></returns>
        byte[] Decode(byte[] cipherData);
    }
}
