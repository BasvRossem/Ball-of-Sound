using System;
using UnityEngine;
using UnityEngine.Audio;

public class MicrophoneInput : MonoBehaviour
{
    public Material spiky;

    AudioSource src;
    public AudioMixer masterMixer;

    float timeSinceRestart = 0.0f;
    public int sampleDataLength = 2048;

    void Start()
    {
        // Set the audiosource to what we gave the object
        src = GetComponent<AudioSource>();

        // Set the timer
        timeSinceRestart = Time.time;

        // Turn off the microphone mixer volume so 
        // that you don't hear the microphone input
        // -80 DB is the minimum value
        masterMixer.SetFloat("Volume", -80.0f);
    }

    void Update()
    {
        // Read mic audio
        MicrophoneIntoAudioSource();

        // Get volume and pitch, apply a sigmoid function 
        // and scale to what looks nice on the shader
        float new_vol = (sigmoid(GetAverageLoudness() * 25) - 0.5f) * 2;
        float new_pit = (sigmoid(GetMostIntensePitch() / 100) - 0.5f) * 2;

        // Apply the new values
        spiky.SetFloat("_AudioVolume", new_vol);
        spiky.SetFloat("_AudioPitch", new_pit);
    }

    // A sigmoid functin to help with the scaling of the data
    private float sigmoid(float x)
    {
        return 1 / (1 + (float)Math.Exp(-x));
    }

    // Return the average loudness
    public float GetAverageLoudness()
    {
        // Make an array to store the data
        float[] clipSampleData = new float[sampleDataLength];

        // Get the data from the audioclip
        src.clip.GetData(clipSampleData, src.timeSamples); 

        // Get the total loudness of the 
        // clip by adding al the sample data values
        float clipLoudness = 0f;
        foreach (var sample in clipSampleData)
        {
            // Because the samplae values range from 
            // -1.0f to 1.0f, we make them all positive. 
            // Without this, there would be (on average)
            // no change to the volume
            clipLoudness += Mathf.Abs(sample);
        }

        // Average out the loudness over the amount of samples
        clipLoudness /= sampleDataLength;

        // Return the average loudness of the clip
        return clipLoudness;
    }

    // Return the pitch with the most intensity
    public float GetMostIntensePitch()
    {
        // Make an array to store the data
        // Must be a power of 2 for the GetSpectrumData to work
        float[] clipSampleData = new float[sampleDataLength];

        // Get the spectrum data from the clip
        // We will use the Hamming calculation to get the 
        // spectrum, it works quite well and has not a
        // complex calculation in comparison to the other 
        // analysis types
        src.GetSpectrumData(clipSampleData, 0, FFTWindow.Hamming);

        // Get the index of the highest pitch
        int highest_index = 0;
        for (int i = 0; i < sampleDataLength; ++i)
        {
            if (clipSampleData[i] > clipSampleData[highest_index])
            {
                highest_index = i;
            }
        }

        // Return the index of the highest pitch
        return highest_index;
    }

    // Put the microphone rcording into the audiosource
    void MicrophoneIntoAudioSource()
    {
        // A little pause is needed to reduce the amount of lag in the recording
        if (Time.time - timeSinceRestart > 0.5f && !Microphone.IsRecording(null))
        {
            // Start the recording of the audioclip
            // We use null to make use of the default microphone
            // We use loop for when the recording stops, that it will use whatever it had recorded
            // 300 seconds to record should be enough to demonstrate the shader that we use this for
            src.clip = Microphone.Start(null, true, 300, AudioSettings.outputSampleRate);

            // This is used to control latency
            // It checks how many samples that the microphone
            // has been recording into the audioclip
            // We will use 0 because we want to have an immediate effect
            while (!(Microphone.GetPosition(null) > 0))
            {
            }

            // Play back the audio that is in the clip, we will use this playback
            // to determine the volume and pitch
            src.Play();
        }
    }
}
