using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorCar : MonoBehaviour
{
    public Renderer rend;
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;
    public Text redSliderText;
    public Text greenSliderText;
    public Text blueSliderText;
    public Color col;

    // Start is called before the first frame update
    void Start()
    {
        col = IntToColor(PlayerPrefs.GetInt("Red"), PlayerPrefs.GetInt("Green"), PlayerPrefs.GetInt("Blue"));
        rend.material.color = col;
        redSlider.value = (int)(col.r * 255f);
        greenSlider.value = (int)(col.g * 255f);
        blueSlider.value = (int)(col.b * 255f);
    }

    public static Color IntToColor(int red, int green, int blue)
    {
        float r = (float)red / 255;
        float g = (float)green / 255;
        float b = (float)blue / 255;
        Color col = new Color(r, g, b);
        return col;
    }

    // Update is called once per frame
    void Update()
    {
        SetCarColor((int)redSlider.value, (int)greenSlider.value, (int)blueSlider.value);
        redSliderText.text = Mathf.RoundToInt(redSlider.value).ToString();
        greenSliderText.text = Mathf.RoundToInt(greenSlider.value).ToString();
        blueSliderText.text = Mathf.RoundToInt(blueSlider.value).ToString();
    }

    void SetCarColor(int red, int green, int blue)
    {
        Color col = IntToColor(red, green, blue);
        rend.material.color = col;
        PlayerPrefs.SetInt("Red", red);
        PlayerPrefs.SetInt("Green", green);
        PlayerPrefs.SetInt("Blue", blue);
    }
}
