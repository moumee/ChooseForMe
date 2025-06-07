using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;

public class VoteResultManager : MonoBehaviour
{
    public Text resultText;

    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadResults(PlayerPrefs.GetString("selectedVoteId"));
    }

    void LoadResults(string voteId)
    {
        db.Collection("votes").Document(voteId).Collection("results").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                string results = "Results:\n";
                foreach(DocumentSnapshot doc in task.Result.Documents)
                {
                    string option = doc.Id;
                    int count = doc.GetValue<int>("count");
                    results += $"Option {option}: {count} votes\n";
                }
                resultText.text = results;
            }
        });
    }
}
