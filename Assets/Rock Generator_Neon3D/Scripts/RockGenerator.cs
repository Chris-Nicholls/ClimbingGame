
//This script and asset developed by Neon3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RockGenerator_Neon3D;

namespace RockGenerator_Neon3D
{
    public class RockGenerator : MonoBehaviour
    {
        public bool urp;
        public bool hdrp;
        public bool builtIn;

        private string localSavePath = "";
        private string localSavePathMaterial = "";

        public bool stopRotate;
        public bool generate;
        public bool saveRock;
        public string savedRockName;

        //UI
        public InputField saveRockNameUi;
        public Dropdown dropdown;

        SkinnedMeshRenderer rockMaterials;
        Material matRock;

        float rotationSpeed = 6f; // the speed at which the object rotates

        Transform currentRock;

        private List<Transform> allChildsRocks = new List<Transform>();

        // Start is called before the first frame update
        void Start()
        {

            var childrenAllRocks = GetComponentsInChildren<Transform>();
            foreach (var child in childrenAllRocks)
            {
                if (child.name.Contains("Generator"))
                {
                    allChildsRocks.Add(child);
                }
            }

            if (hdrp)
            {
                builtIn = false;
                urp = false;
            }
            else if (urp)
            {
                hdrp = false;
                builtIn = false;
            }
            else if (builtIn)
            {
                hdrp = false;
                urp = false;
            }
            else
            {
                Debug.Log("Please choose the render pipeline you are using!");
            }

            localSavePath = "Assets/Rock Generator_Neon3D/Saved Rocks/";
            localSavePathMaterial = "Assets/Rock Generator_Neon3D/Saved Rocks/Materials/";

        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < allChildsRocks.Count; i++)
            {
                if (dropdown.value == allChildsRocks[i].GetSiblingIndex())
                {
                    currentRock = allChildsRocks[i];
                    currentRock.GetComponent<SkinnedMeshRenderer>().enabled = true;
                }
                else
                {
                    allChildsRocks[i].GetComponent<SkinnedMeshRenderer>().enabled = false;
                }
            }
            rockMaterials = currentRock.GetComponent<SkinnedMeshRenderer>();


            if (!stopRotate)
            {
               currentRock.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }

            if (generate)
            {
                RockEditRandom();
                generate = false;
            }

            if (saveRock)
            {
                    SavedRocks();
                    saveRock = false;
            }

            //rockMaterials.sharedMaterials[0].color = rockColor;

            if (saveRockNameUi.text != "")
            {
                savedRockName = saveRockNameUi.text;
            }
        }

        public void RockEditRandom()
        {
            if (rockMaterials == null)
            {
                return;
            }

            rockMaterials.SetBlendShapeWeight(0,Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(1, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(2, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(3, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(4, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(5, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(6, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(7, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(8, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(9, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(10, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(11, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(12, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(13, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(14, Random.Range(0.0f, 100.0f));
            rockMaterials.SetBlendShapeWeight(15, Random.Range(0.0f, 100.0f));
        }

        public void SavedRocks()
        {
            if (savedRockName != "")
            {
                if (hdrp)
                {
                    // project is using HDRP
                    matRock = new Material(Shader.Find("HDRP/Lit"));
                }
                else if (urp)
                {
                    // project is using URP
                    matRock = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                }
                else if (builtIn)
                {
                    // project is using built-in render pipeline
                    matRock = new Material(Shader.Find("Standard"));
                }

                GameObject CurrentRockCopy = Instantiate(currentRock.gameObject);
                Renderer rockMaterialsCopy = CurrentRockCopy.GetComponent<Renderer>();
                Material[] materials = rockMaterialsCopy.sharedMaterials; //Get whole array

                matRock.CopyPropertiesFromMaterial(materials[0]); //get specific material from the array
                materials[0] = matRock;

                UnityEditor.AssetDatabase.CreateAsset(matRock, localSavePathMaterial + (savedRockName + ".mat"));
                UnityEditor.AssetDatabase.Refresh();

                rockMaterialsCopy.sharedMaterials = materials; // set the updated material array to the currentRockCopy renderer
                Vector3 newPosition = new Vector3(0, 0, 0);
                Quaternion newRotation = Quaternion.identity;
                CurrentRockCopy.transform.SetPositionAndRotation(newPosition, newRotation); DestroyImmediate(CurrentRockCopy.GetComponent<RockGenerator>());

                UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(CurrentRockCopy, localSavePath + savedRockName + ".prefab", UnityEditor.InteractionMode.UserAction);
                UnityEditor.SceneView.RepaintAll();
                DestroyImmediate(CurrentRockCopy);
                Debug.Log("Rock has been saved! Location: Assets/Rock Generator_Neon3D/Saved Rocks/");
            }
            else
            {

            }
        }

        public void SaveRockBtn()
        {
            saveRock = true;
        }

    }
}
