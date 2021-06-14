using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.OnBoardingTool.Infrastructure.Models
{
    public class Issuetype
    {
        public string Self { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Name { get; set; }
        public bool Subtask { get; set; }
        public int AvatarId { get; set; }
    }
}
