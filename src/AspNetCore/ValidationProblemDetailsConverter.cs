using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rocket.Surgery.AspNetCore.FluentValidation
{
    /// <summary>
    /// A RFC 7807 compliant <see cref="JsonConverter"/> for <see cref="FluentValidationProblemDetails"/>.
    /// </summary>
    public sealed class ValidationProblemDetailsConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => objectType == typeof(FluentValidationProblemDetails);

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var annotatedProblemDetails = serializer.Deserialize<AnnotatedProblemDetails>(reader);
            if (annotatedProblemDetails == null)
            {
                return null;
            }

            var problemDetails = (FluentValidationProblemDetails)existingValue ?? new FluentValidationProblemDetails();
            annotatedProblemDetails.CopyTo(problemDetails);

            return problemDetails;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var problemDetails = (FluentValidationProblemDetails)value;
            var annotatedProblemDetails = new AnnotatedProblemDetails(problemDetails);

            serializer.Serialize(writer, annotatedProblemDetails);
        }

        internal class AnnotatedProblemDetails
        {
#pragma warning disable CS8618
            public AnnotatedProblemDetails() { }
#pragma warning restore CS8618

            public AnnotatedProblemDetails(FluentValidationProblemDetails problemDetails)
            {
                Detail = problemDetails.Detail;
                Instance = problemDetails.Instance;
                Status = problemDetails.Status;
                Title = problemDetails.Title;
                Type = problemDetails.Type;

                foreach (var kvp in problemDetails.Extensions)
                {
                    Extensions[kvp.Key] = kvp.Value;
                }

                Rules = problemDetails.Rules;
                foreach (var kvp in problemDetails.Errors)
                {
                    Errors[kvp.Key] = kvp.Value;
                }
            }

            [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
            public int? Status { get; set; }

            [JsonProperty(PropertyName = "detail", NullValueHandling = NullValueHandling.Ignore)]
            public string Detail { get; set; }

            [JsonProperty(PropertyName = "instance", NullValueHandling = NullValueHandling.Ignore)]
            public string Instance { get; set; }

            [JsonExtensionData]
            public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>(StringComparer.Ordinal);

            [JsonProperty(PropertyName = "errors")]
            public IDictionary<string, FluentValidationProblemDetail[]> Errors { get; } =
                new Dictionary<string, FluentValidationProblemDetail[]>(StringComparer.Ordinal);

            [JsonProperty(PropertyName = "rules")] public string[] Rules { get; internal set; } = Array.Empty<string>();

            public void CopyTo(FluentValidationProblemDetails problemDetails)
            {
                problemDetails.Type = Type;
                problemDetails.Title = Title;
                problemDetails.Status = Status;
                problemDetails.Instance = Instance;
                problemDetails.Detail = Detail;

                foreach (var kvp in Extensions)
                {
                    problemDetails.Extensions[kvp.Key] = kvp.Value;
                }

                Rules = problemDetails.Rules;
                foreach (var kvp in problemDetails.Errors)
                {
                    Errors[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
