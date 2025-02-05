﻿// <copyright file="SpanExtensions.cs" company="OpenTelemetry Authors">
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

namespace OpenTelemetry.Exporter.Stackdriver.Implementation
{
    using System.Collections.Generic;
    using System.Linq;
    using Google.Cloud.Trace.V2;
    using Google.Protobuf.WellKnownTypes;
    using OpenTelemetry.Trace;
    using OpenTelemetry.Trace.Config;

    internal static class SpanExtensions
    {
        private static readonly IDictionary<string, string> LabelMappings = new Dictionary<string, string>
        {
            { "http.method", "/http/method" },
            { "http.status_code", "/http/status_code" },
            { "http.user_agent", "/http/user_agent" },
            { "http.path", "/http/path" },
            { "http.host", "/http/host" },
            { "http.url", "/http/url" },
            { "http.request.size", "/http/request_size" },
            { "http.response.size", "/http/response_size" },
            { "http.route", "/http/route" },
        };

        /// <summary>
        /// Translating <see cref="Trace.Span"/> to Stackdriver's Span
        /// According to <see href="https://cloud.google.com/trace/docs/reference/v2/rpc/google.devtools.cloudtrace.v2"/> specifications.
        /// </summary>
        /// <param name="spanData">Span in OpenTelemetry format.</param>
        /// <param name="projectId">Google Cloud Platform Project Id.</param>
        /// <returns><see cref="ISpan"/>.</returns>
        public static Google.Cloud.Trace.V2.Span ToSpan(this Trace.Span spanData, string projectId)
        {
            var spanId = spanData.Context.SpanId.ToHexString();

            // Base span settings
            var span = new Google.Cloud.Trace.V2.Span
            {
                SpanName = new SpanName(projectId, spanData.Context.TraceId.ToHexString(), spanId),
                SpanId = spanId,
                DisplayName = new TruncatableString { Value = spanData.Name },
                StartTime = spanData.StartTimestamp.ToTimestamp(),
                EndTime = spanData.EndTimestamp.ToTimestamp(),
                ChildSpanCount = null,
            };
            if (spanData.ParentSpanId != null)
            {
                var parentSpanId = spanData.ParentSpanId.ToHexString();
                if (!string.IsNullOrEmpty(parentSpanId))
                {
                    span.ParentSpanId = parentSpanId;
                }
            }

            // Span Links
            if (spanData.Links != null)
            {
                span.Links = new Google.Cloud.Trace.V2.Span.Types.Links
                {
                    Link = { spanData.Links.Select(l => l.ToLink()) },
                };
            }

            // Span Attributes
            if (spanData.Attributes != null)
            {
                span.Attributes = new Google.Cloud.Trace.V2.Span.Types.Attributes
                {
                    AttributeMap =
                    {
                        spanData.Attributes?.ToDictionary(
                                        s => FormatLabel(s.Key),
                                        s => s.Value?.ToAttributeValue()),
                    },
                };
            }

            return span;
        }

        public static Google.Cloud.Trace.V2.Span.Types.Link ToLink(this ILink link)
        {
            var ret = new Google.Cloud.Trace.V2.Span.Types.Link();
            ret.SpanId = link.Context.SpanId.ToHexString();
            ret.TraceId = link.Context.TraceId.ToHexString();

            if (link.Attributes != null)
            {
                ret.Attributes = new Google.Cloud.Trace.V2.Span.Types.Attributes
                {
                    DroppedAttributesCount = TraceConfig.Default.MaxNumberOfAttributes - link.Attributes.Count,

                    AttributeMap =
                    {
                        link.Attributes.ToDictionary(
                         att => att.Key,
                         att => att.Value.ToAttributeValue()),
                    },
                };
            }

            return ret;
        }

        public static Google.Cloud.Trace.V2.AttributeValue ToAttributeValue(this object av)
        {
            switch (av)
            {
                case string s:
                    return new Google.Cloud.Trace.V2.AttributeValue()
                    {
                        StringValue = new TruncatableString() { Value = s },
                    };
                case bool b:
                    return new Google.Cloud.Trace.V2.AttributeValue() { BoolValue = b };
                case long l:
                    return new Google.Cloud.Trace.V2.AttributeValue() { IntValue = l };
                case double d:
                    return new Google.Cloud.Trace.V2.AttributeValue()
                    {
                        StringValue = new TruncatableString() { Value = d.ToString() },
                    };
                default:
                    return new Google.Cloud.Trace.V2.AttributeValue()
                    {
                        StringValue = new TruncatableString() { Value = av.ToString() },
                    };
            }
        }

        private static string FormatLabel(string label)
        {
            if (LabelMappings.ContainsKey(label))
            {
                return LabelMappings[label];
            }

            return label;
        }
    }
}
