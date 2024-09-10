namespace GlobalTypes.Interfaces
{
    /// <summary>
    /// Interface for objects that should be initialized in the <c>Initialize</c> method of the <c>Game</c> class. 
    /// The <c>Initialize</c> method automatically creates instances of objects that implement this interface.
    /// </summary>
    public interface IInitable
    {
        /// <summary>
        /// Method that will be called from the <c>Initialize</c> method after an instance of the object is created. 
        /// This method should be explicitly implemented in the class.
        /// </summary>
        protected void Init();
        public static void InitAll() 
            => Reflector
            .CreateInstances<IInitable>()
            .ForEach(i => Reflector.CallMethod(i, nameof(Init)));
    }

    /// <summary>
    /// Interface for objects that should be loaded in the <c>Load</c> method of the <c>Game</c> class.
    /// The <c>Load</c> method automatically creates instances of objects that implement this interface.
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        /// Method that will be called from the <c>Load</c> method after an instance of the object is created. 
        /// This method should be explicitly implemented in the class.
        /// </summary>
        protected void Load();
        public static void LoadAll()
            => Reflector
            .CreateInstances<ILoadable>()
            .ForEach(i => Reflector.CallMethod(i, nameof(Load)));
    }
}