using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Api.Atlassian
{
    public class CustomerRequestFieldValueDTO
    {
        private JsonDocument _document;

        public string FieldId { get; set; }
        public string Label { get; set; }
        public object Value { get; set; }

        public string ValueAsString
        {
            get
            {
                if(Value != null)
                {
                    switch (FieldId)
                    {
                        case "customfield_12105":
                            return ValueAsJsonObject().RootElement.GetProperty("value").GetString();
                        case "duedate":
                            return ((JsonElement)Value).GetString();
                        case "customfield_12104":
                            return ((JsonElement)Value).GetString();
                        case "customfield_12056":
                            return ValueAsJsonObject().RootElement.GetProperty("value").GetString();
                        default:
                        return string.Empty;
                    }
                }

                return string.Empty;
            }
        }

        private JsonDocument ValueAsJsonObject() => _document ?? (_document = JsonDocument.Parse(Value.ToString()));

    }
}