using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Extensions
{
    public static class UriBuilderExtensions
    {
        public static UriBuilder BuildQueryString(this UriBuilder uriBuilder, IDictionary<string,string> queryParams)
        {               
            queryParams = queryParams ?? new Dictionary<string,string>();        
            
            if (queryParams?.Count == 0)
            {
                uriBuilder.Query = String.Empty;
            }
            else
            {
                var sb = new StringBuilder();
                var first = true;
                foreach(var keyVal in queryParams)
                {
                    if(sb.Length == 0)
                    {
                        sb.Append("?");                        
                    }

                    if (sb.Length > 0 && !first)
                    {
                        sb.Append('&');
                    }

                    string text = keyVal.Key;
                    string val = (text != null) ? (text + "=" + keyVal.Value) : string.Empty;

                    sb.Append(val);   
                    first = false;                 
                }

                uriBuilder.Query = sb.ToString();                
            }            

            return uriBuilder;
        }
    }
}