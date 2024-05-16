

namespace Fusion.Mvvm
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractFactory : IFactory
    {
        private IPrefsEncryptor encryptor;
        private ISerializer serializer;
        /// <summary>
        /// 
        /// </summary>
        public AbstractFactory() : this(null, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        public AbstractFactory(ISerializer serializer) : this(serializer, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="encryptor"></param>
        public AbstractFactory(ISerializer serializer, IPrefsEncryptor encryptor)
        {
#if UNITY_IOS
			Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
            this.serializer = serializer;
            this.encryptor = encryptor;

            if (this.serializer == null)
                this.serializer = new DefaultSerializer();

            if (this.encryptor == null)
                this.encryptor = new DefaultEncryptor();
        }
        
        /// <summary>
        /// 
        /// </summary>
        public IPrefsEncryptor Encryptor
        {
            get => encryptor;
            protected set => encryptor = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public ISerializer Serializer
        {
            get => serializer;
            protected set => serializer = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public abstract Preferences Create(string name);
    }
}
