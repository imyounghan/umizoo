
namespace Umizoo.Infrastructure
{
    /// <summary>
    /// Exposes the property names of standard metadata added to all messages going through the bus.
    /// </summary>
    public static class StandardMetadata
    {
        /// <summary>
        /// An event message.
        /// </summary>
        public const string EventKind = "Event";

        /// <summary>
        /// A command message.
        /// </summary>
        public const string CommandKind = "Command";
        
        /// <summary>
        /// Kind of message, either <see cref="EventKind"/> or <see cref="CommandKind"/>.
        /// </summary>
        public const string Kind = "Kind";

        /// <summary>
        /// The simple assembly name of the message payload (i.e. event or command).
        /// </summary>
        public const string AssemblyName = "AssemblyName";

        /// <summary>
        /// The namespace of the message payload (i.e. event or command).
        /// </summary>
        public const string Namespace = "Namespace";

        /// <summary>
        /// The full type name of the message payload (i.e. event or command).
        /// </summary>
        public const string TypeFullName = "TypeFullName";

        /// <summary>
        /// The simple type name (without the namespace) of the message payload (i.e. event or command).
        /// </summary>
        public const string TypeName = "TypeName";

        public const string MessageId = "MessageId";

        public const string CommandInfo = "CommandInfo";

        /// <summary>
        /// Identifier of the object that originated the command, if any.
        /// </summary>
        public const string SourceInfo = "SourceInfo";

        public const string TraceInfo = "TraceInfo";
    }
}
