using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public struct Gesture
{
    public string name;
    public List<Vector3> fingerDatas;
    public UnityEvent onRecognized;
}

public class GestureDetector : MonoBehaviour
{
    public string id;
    public float threshold = 0;
    public OVRSkeleton skeleleton;
    public List<Gesture> gestures;
    private List<OVRBone> fingerBones;
    public bool debugMode = true;
    private Gesture prevGesture;
    public GameObject textObject;
    
    public AudioClip audioClip;
    public AudioSource source;
    public float volume = 1;


    // Start is called before the first frame update
    void Start()
    {
        fingerBones = new List<OVRBone>(skeleleton.Bones);
        prevGesture = new Gesture();
        // source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        fingerBones = new List<OVRBone>(skeleleton.Bones);
        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }

        Gesture currentGesture = Recognize();
        bool found = !currentGesture.Equals(new Gesture());
        if (found && !currentGesture.Equals(prevGesture))
        {
            Debug.Log(currentGesture.name);

            source.PlayOneShot(audioClip, volume);

            // textObject = GameObject.Find("Monitor");
            Text txt = textObject.GetComponent<Text>();
            txt.text = currentGesture.name;

            prevGesture = currentGesture;
            currentGesture.onRecognized.Invoke();
        }
    }

    void Save()
    {
        Gesture g = new Gesture();
        g.name = "new Gesture";
        List<Vector3> data = new List<Vector3>();
        foreach(var bone in fingerBones)
        {
            data.Add(skeleleton.transform.InverseTransformPoint(bone.Transform.position));
        }
        g.fingerDatas = data;
        gestures.Add(g);
    }

    Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures) 
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < fingerBones.Count; i++)
            {
                Vector3 currentData = skeleleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingerDatas[i]);
             
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }
            if(!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        return currentGesture;
    }
}
