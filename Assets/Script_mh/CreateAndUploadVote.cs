using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Firestore;
using System;
using System.IO;
using System.Collections.Generic;
using TMPro;


public class CreateAndUploadVote : MonoBehaviour
{
    //UI input fields for title and two voting options and dropdown category
    public TMP_InputField option1Input;
    public TMP_InputField option2Input;
    //UI buttons to pick images and upload the vote
    public Button image1Button;
    public Button image2Button;
    public Button uploadVoteButton;
    //paths to selected images on device
    private string image1Path = null;
    private string image2Path = null;
    //Firebase instances for Firestore(database) and Storage
    FirebaseFirestore db;
    FirebaseStorage storage;

    void Start()
    {
        //Initialize Firebase Firestore and Storage
        db = FirebaseFirestore.DefaultInstance;
        storage = FirebaseStorage.DefaultInstance;

        //Attach click listener to buttons
        image1Button.onClick.AddListener(() => PickImageForOption(1));
        image2Button.onClick.AddListener(() => PickImageForOption(2));
        uploadVoteButton.onClick.AddListener(CreateVote);
    }


    //Method to let users pick an image from their device for one of the option
    void PickImageForOption(int optionNumber)
    {
#if UNITY_ANDROID || UNITY_IOS
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                if (optionNumber == 1)
                    image1Path = path;
                else
                    image2Path = path;

                Debug.Log($"Image selected for Option {optionNumber}: {path}");
            }
        },
#else
        Debug.LogWarning("Image picking only supported on mobile with NativeGallery");
#endif

    }

    //When user presses "Upload Vote"
    void CreateVote()
    {
        string opt1 = option1Input.text;
        string opt2 = option2Input.text;

        //Validate that all text fields are filled
        if (string.IsNullOrEmpty(opt1) || string.IsNullOrEmpty(opt2))
        {
            Debug.LogWarning("All fields must be filled out.");
            return;
        }

        //start coroutine to upload images (if any) and save the vote
        StartCoroutine(UploadImagesAndSaveVote(opt1, opt2));
    }

    //coroutine to handle uploading images and saving vote data to Firestone
    System.Collections.IEnumerator UploadImagesAndSaveVote(string opt1, string opt2)
    {
        string imageUrl1 = null;
        string imageUrl2 = null;

        //Upload image for option 1, if selected
        if (image1Path != null)
        {
            yield return UploadImage(image1Path, url => imageUrl1 = url);
        }
        //Upload image for option 2, if selected
        if (image2Path != null)
        {
            yield return UploadImage(image2Path, url => imageUrl2 = url);
        }

        //Build Firestone document data
        Dictionary<string, object> voteData = new Dictionary<string, object>
        {
            {"options", new[] {opt1, opt2} },
            {"CreatedAt", Timestamp.GetCurrentTimestamp() }
        };

        //If either image is uploaded, add their URLs under "optionImages"
        if (imageUrl1 != null || imageUrl2 != null)
        {
            voteData["optionImages"] = new Dictionary<string, string>
            {
                {"0", imageUrl1 ?? "" },
                {"1", imageUrl2 ?? "" }
            };
        }

        //Add the document to the "votes" collection in Firestone
        DocumentReference voteRef = db.Collection("votes").Document();
        voteRef.SetAsync(voteData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("Vote created Successfully!");
            else
                Debug.LogError("Error creating vote: " + task.Exception);
        });
    }

    //Upload image to Firebase storage and gets a downloadable URL 
    System.Collections.IEnumerator UploadImage(string path, Action<string> onComplete)
    {
        byte[] bytes = File.ReadAllBytes(path);     //Read image file as bytes
        string fileName = Guid.NewGuid().ToString() + ".jpg";       //Generate unique filename
        StorageReference imageRef = storage.GetReference($"voteImages/{fileName}");     //Storage path

        var metadata = new MetadataChange { ContentType = "image/jpeg" };
        var task = imageRef.PutBytesAsync(bytes, metadata);     //Upload image

        yield return new WaitUntil(() => task.IsCompleted);     //Wait until upload finishes

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Image upload failed.");
            onComplete(null);
        }
        else
        {
            var urlTask = imageRef.GetDownloadUrlAsync();       //Get public URL
            yield return new WaitUntil(() => urlTask.IsCompleted);

            if (!urlTask.IsFaulted && !urlTask.IsCanceled)
                onComplete(urlTask.Result.ToString());          //Return the URL
            else
                onComplete(null);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
