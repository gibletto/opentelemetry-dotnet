﻿// <copyright file="ITracer.cs" company="OpenTelemetry Authors">
// Copyright 2018, OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

namespace OpenTelemetry.Trace
{
    using OpenTelemetry.Context;
    using OpenTelemetry.Context.Propagation;

    /// <summary>
    /// Tracer to record distributed tracing information.
    /// </summary>
    public interface ITracer
    {
        /// <summary>
        /// Gets the current span from the context.
        /// </summary>
        ISpan CurrentSpan { get; }

        /// <summary>
        /// Gets the <see cref="IBinaryFormat"/> for this implementation.
        /// </summary>
        IBinaryFormat BinaryFormat { get; }

        /// <summary>
        /// Gets the <see cref="ITextFormat"/> for this implementation.
        /// </summary>
        ITextFormat TextFormat { get; }

        /// <summary>
        /// Associates the span with the current context.
        /// </summary>
        /// <param name="span">Span to associate with the current context.</param>
        /// <returns>Scope object to control span to current context association.</returns>
        IScope WithSpan(ISpan span);

        /// <summary>
        /// Gets the span builder for the span with the given name.
        /// </summary>
        /// <param name="spanName">Span name.</param>
        /// <returns>Span builder for the span with the given name.</returns>
        ISpanBuilder SpanBuilder(string spanName);
    }
}
