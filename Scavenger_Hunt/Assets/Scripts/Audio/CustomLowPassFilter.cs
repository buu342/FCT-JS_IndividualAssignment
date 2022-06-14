/****************************************************************
                     CustomLowPassFilter.cs
    
This script handles a custom low-pass audio filter to substitute
Unity's default one. The code for the filter itself is based on
the code from the Music.DSP Source Code Archive forum (May it 
rest in peace, it had a lot of goodies in there):
http://web.archive.org/web/20130224102707/http://musicdsp.org/showArchiveComment.php?ArchiveID=185
****************************************************************/


using UnityEngine;

public class CustomLowPassFilter : MonoBehaviour
{
    private const float CutoffMax = 128.0f;
    private const float ResonanceMax = 128.0f;
    
    private float c = 0.0f;
    private float r = 0.0f;
    private float v0 = 0.0f;
    private float v1 = 0.0f;
    
    private float m_CutoffPercent = CustomLowPassFilter.CutoffMax;
    private float m_ResonancePercent = 0.00f;
    
    
    /*==============================
        OnAudioFilterRead
        Allows for custom DSP effects
        @param The sound data itself
        @param The number of channels in the data
    ==============================*/

    void OnAudioFilterRead(float[] data, int channels)
    {
        c = Mathf.Pow(0.5f, (128.0f - this.m_CutoffPercent)/16.0f);
        r = Mathf.Pow(0.5f, (this.m_ResonancePercent+24.0f)/16.0f);

        for (int i=0; i<data.Length; i++)
        {
            v0 = ((1.0f - r*c)*v0) - (c*v1) + (c*data[i]);
            v1 = ((1.0f - r*c)*v1) + (c*v0);
            data[i] = Mathf.Clamp(v1, -1.0f, 1.0f);
        }
    }
    
    
    /*==============================
        SetCutoffPercent
        Sets the percentage of the frequencies to cutoff
        @param The percentage cutoff
    ==============================*/
    
    public void SetCutoffPercent(float percent)
    {
        this.m_CutoffPercent = CustomLowPassFilter.CutoffMax*percent;
    }
    
    
    /*==============================
        SetResonancePercent
        Sets the percentage of the low pass resonance
        @param The percentage resonance
    ==============================*/
    
    public void SetResonancePercent(float percent)
    {
        this.m_ResonancePercent = CustomLowPassFilter.ResonanceMax*percent;
    }
    
    
    /*==============================
        GetCutoffPercent
        Gets the percentage of the frequencies that will be cutoff
        @return The percentage cutoff
    ==============================*/
    
    public float GetCutoffPercent()
    {
        return this.m_CutoffPercent;
    }
    
    
    /*==============================
        GetResonancePercent
        Gets the low pass resonance percentage
        @param The percentage resonance
    ==============================*/
    
    public float GetResonancePercent()
    {
        return this.m_ResonancePercent;
    }
}