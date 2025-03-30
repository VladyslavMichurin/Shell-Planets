using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    public bool UpdateVariables = true;

    [Header("Shell Components")]
    public Shader shellShader;
    public Gradient groundColor;
    public Gradient skyColor;

    [Header("Ground Shell Variables")]
    [Range(0, 128)]
    public int groundShellCount = 16;
    [Range(0.0f, 1.0f)]
    public float groundShellLenght = 0.15f;
    [Range(1.0f, 1000.0f)]
    public float groundDensity = 100.0f;
    [Range(0.0f, 1.0f)]
    public float groundNoiseMin = 0.0f;
    [Range(0.0f, 1.0f)]
    public float groundNoiseMax = 1.0f;

    [Header("Sky Shell Variables")]
    [Range(0, 128)]
    public int skyShellCount = 16;
    [Range(0.0f, 1.0f)]
    public float skyShellLenght = 0.15f;
    [Range(1.0f, 1000.0f)]
    public float skyDensity = 100.0f;
    [Range(0.0f, 1.0f)]
    public float skyCutoff = 0.5f;

    [Header("Other")]
    [Range(-1.0f, 1.0f)]
    public float ground_sky_delta = 0.5f;
    [Range(0.001f, 100.0f)]
    public float groundFlow = 80.0f;
    [Range(0.001f, 100.0f)]
    public float skyFlow = 10.0f;

    private Mesh shellMesh;
    private Material shellMaterial;
    private List<GameObject> shells;
    private void OnEnable()
    {
        GameObject shellPrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shellMesh = shellPrimitive.GetComponent<MeshFilter>().mesh;
        Destroy(shellPrimitive);

        shellMaterial = new Material(shellShader);
        shells = new List<GameObject>();

        GameObject groundHolder = new GameObject("Ground Shell Holder");
        groundHolder.transform.SetParent(this.transform, false);

        GameObject skyHolder = new GameObject("Sky Shell Holder");
        skyHolder.transform.SetParent(this.transform, false);

        for (int i = 0; i < groundShellCount + skyShellCount; i++) 
        {
            shells.Add(new GameObject("Shell " + i.ToString()));
            shells[i].AddComponent<MeshFilter>();
            shells[i].AddComponent<MeshRenderer>();

            shells[i].GetComponent<MeshFilter>().mesh = shellMesh;
            shells[i].GetComponent<MeshRenderer>().material = shellMaterial;
            if(i < groundShellCount)
            {
                shells[i].transform.SetParent(groundHolder.transform, false);
            }
            else
            {
                shells[i].transform.SetParent(skyHolder.transform, false);
            }

            UpdateShaderVariables(i);
        }
    }

    private void FixedUpdate()
    {
        if(UpdateVariables)
        {
            for (int i = 0; i < groundShellCount + skyShellCount; i++)
            {
                UpdateShaderVariables(i);
            }
        }
    }

    void UpdateShaderVariables(int _index)
    {
        Color c;
        if (_index < groundShellCount)
        {
            float h = (float)_index / (float)groundShellCount;
            c = groundColor.Evaluate((h));
        }
        else
        {
            int gradientIndex = _index - groundShellCount;
            float h = (float)gradientIndex / (float)skyShellCount;
            c = skyColor.Evaluate((h));
        }
        shells[_index].GetComponent<MeshRenderer>().material.SetColor("_Color", c);

        shells[_index].GetComponent<MeshRenderer>().material.SetInt("_ShellIndex", _index);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_Ground_Sky_Delta", ground_sky_delta);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_GroundFlow", groundFlow);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_SkyFlow", skyFlow);

        shells[_index].GetComponent<MeshRenderer>().material.SetInt("_GroundShellCount", groundShellCount);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_GroundShellLength", groundShellLenght);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_GroundDensity", groundDensity);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_GroundNoiseMin", groundNoiseMin);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_GroundNoiseMax", groundNoiseMax);

        shells[_index].GetComponent<MeshRenderer>().material.SetInt("_SkyShellCount", skyShellCount);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_SkyShellLength", skyShellLenght);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_SkyDensity", skyDensity);
        shells[_index].GetComponent<MeshRenderer>().material.SetFloat("_SkyCutoff", skyCutoff);
    }

    void OnDisable()
    {
        for (int i = 0; i < shells.Count; ++i)
        {
            Destroy(shells[i]);
        }
        shells.Clear();
    }
}
