namespace GlobalTypes.Interfaces
{
    /// <summary>
    /// Interface for objects that should be initialized in the <c>Initialize</c> method of the <c>Game</c> class. 
    /// The <c>Initialize</c> method automatically creates instances of objects that implement this interface.
    /// </summary>
    public interface IInitable
    {
        /// <summary>
        /// The name of the <c>Init</c> method that should be called to initialize the object.
        /// </summary>
        public static string MethodName => nameof(Init);

        /// <summary>
        /// Method that will be called from the <c>Initialize</c> method after an instance of the object is created. 
        /// This method should be explicitly implemented in the class.
        /// </summary>
        protected void Init();
    }

    /// <summary>
    /// Interface for objects that should be loaded in the <c>Load</c> method of the <c>Game</c> class.
    /// The <c>Load</c> method automatically creates instances of objects that implement this interface.
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        /// The name of the <c>Load</c> method that should be called to load the object.
        /// </summary>
        public static string MethodName => nameof(Load);

        /// <summary>
        /// Method that will be called from the <c>Load</c> method after an instance of the object is created. 
        /// This method should be explicitly implemented in the class.
        /// </summary>
        protected void Load();
    }
}