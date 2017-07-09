using System;

namespace Umizoo.Configurations
{
    [Flags]
    public enum ProcessingFlags
    {
        All,
        Command,
        Event,
        PublishableException,
        Query
    }
}
