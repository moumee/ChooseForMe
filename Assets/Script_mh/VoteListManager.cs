using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class VoteListManager : MonoBehaviour
{
    public Transform voteListContainer;
    public GameObject voteItemPrefab;

    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        LoadVotes();
    }

    void LoadVotes()
    {
        db.Collection("votes").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    GameObject item = Instantiate(voteItemPrefab, voteListContainer);
                    item.GetComponentInChildren<Text>().text = doc.GetValue<string>("title");

                    string voteId = doc.Id;
                    item.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        PlayerPrefs.SetString("selectedVoteId", voteId);
                        UnityEngine.SceneManagement.SceneManager.LoadScene("VoteDetailsSCene");
                    });
                }
            }
        });
    }

}
