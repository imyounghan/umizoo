// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System.Threading;

namespace Umizoo.Infrastructure
{
    public sealed class SingleEntryGate
    {

        private const int NOT_ENTERED = 0;
        private const int ENTERED = 1;

        private int _status;

        // returns true if this is the first call to TryEnter(), false otherwise
        public bool TryEnter()
        {
            int oldStatus = Interlocked.Exchange(ref _status, ENTERED);
            return (oldStatus == NOT_ENTERED);
        }
    }
}