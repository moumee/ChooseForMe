using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class VoteDeatailManager : MonoBehaviour
{
    public Text titleText;
    //Two buttons for vote options
    public Button[] optionButtons;

    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadVoteDetails(PlayerPrefs.GetString("selectedVoteId"));
    }

    void LoadVoteDetails(string voteId)
    {
        DocumentReference docRef = db.Collection("votes").Document(voteId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var doc = task.Result;
                titleText.text = doc.GetValue<string>("title");
                List<string> options = new List<string>((string[])doc.GetValue<object>("options"));

                // Assign button text and vote actions
                for (int i = 0; i < options.Count && i < optionButtons.Length; i++)
                {
                    optionButtons[i].GetComponentInChildren<Text>().text = options[i];
                    int index = i;
                    optionButtons[i].onClick.AddListener(() => SubmitVote(voteId, index));
                }
            }
        });
    }

    void SubmitVote(string voteId, int optionIndex)
    {
        DocumentReference resultRef = db.Collection("votes").Document(voteId).Collection("results").Document(optionIndex.ToString());

        //Transaction to increment vote count safely
        db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(resultRef).ContinueWith(t =>
            {
            int current = 0;
            if (t.Result.Exists) current = t.Result.GetValue<int>("count");
            transaction.Set(resultRef, new Dictionary<string, object>{ { "count", current + 1} });
            });
        });

        UnityEngine.SceneManagement.SceneManager.LoadScene("VoteResultScene");
    
    }


}
