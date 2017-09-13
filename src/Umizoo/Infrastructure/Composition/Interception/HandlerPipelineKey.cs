// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-09-09.

using System;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    public struct HandlerPipelineKey : IEquatable<HandlerPipelineKey>
    {
        private readonly Module _module;
        private readonly int _methodMetadataToken;

        private HandlerPipelineKey(Module module, int methodMetadataToken)
        {
            _module = module;
            _methodMetadataToken = methodMetadataToken;
        }

        /// <summary>
        /// Compare two <see cref="HandlerPipelineKey"/> instances.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>True if the two keys are equal, false if not.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is HandlerPipelineKey))
            {
                return false;
            }
            return this == (HandlerPipelineKey)obj;
        }

        /// <summary>
        /// Calculate a hash code for this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return _module.GetHashCode() ^ _methodMetadataToken;
        }

        /// <summary>
        /// Compare two <see cref="HandlerPipelineKey"/> instances for equality.
        /// </summary>
        /// <param name="left">First of the two keys to compare.</param>
        /// <param name="right">Second of the two keys to compare.</param>
        /// <returns>True if the values of the keys are the same, else false.</returns>
        public static bool operator ==(HandlerPipelineKey left, HandlerPipelineKey right)
        {
            return left._module == right._module &&
                   left._methodMetadataToken == right._methodMetadataToken;
        }

        /// <summary>
        /// Compare two <see cref="HandlerPipelineKey"/> instances for inequality.
        /// </summary>
        /// <param name="left">First of the two keys to compare.</param>
        /// <param name="right">Second of the two keys to compare.</param>
        /// <returns>false if the values of the keys are the same, else true.</returns>
        public static bool operator !=(HandlerPipelineKey left, HandlerPipelineKey right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Compare two <see cref="HandlerPipelineKey"/> instances.
        /// </summary>
        /// <param name="other">Object to compare to.</param>
        /// <returns>True if the two keys are equal, false if not.</returns>
        public bool Equals(HandlerPipelineKey other)
        {
            return this == other;
        }


        public static HandlerPipelineKey ForMethod(MethodBase method)
        {
            Assertions.NotNull(method, "method");
            return new HandlerPipelineKey(method.DeclaringType.Module, method.MetadataToken);
        }
    }
}
