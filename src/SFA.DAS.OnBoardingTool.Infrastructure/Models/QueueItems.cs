using System.Collections.Generic;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{    
    public class QueueItems
    {
        public int Size { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
        public bool IsLastPage { get; set; }
        public List<Value> Values { get; set; }
    }
}