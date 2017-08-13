using System;

namespace Umizoo.Infrastructure
{
    public interface ITopicProvider
    {
        string GetTopic(Type type);
    }
}