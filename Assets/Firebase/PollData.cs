// PollData.cs
using System.Collections.Generic;

public class PollData
{
    public string Id { get; set; }
    public string Question { get; set; }
    public List<string> Options { get; set; }
    public Dictionary<string, string> OptionImages { get; set; } // Firestore map은 Dictionary로 받음
    public string CreatorNickname { get; set; }
    public long TotalVoteCount { get; set; }
    public long Option1Votes { get; set; }
    public long Option2Votes { get; set; }
}