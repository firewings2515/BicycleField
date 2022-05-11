using UnityEditor;
using UnityEngine;
using System.Collections;
using Unity.EditorCoroutines.Editor;

//NOTE: You MUST have the EditorCorountines package installed through the package manager.

//Original script attributed to BarShiftGames: https://answers.unity.com/questions/1538195/unity-lod-billboard-asset-example.html
//Modifications by RobProductions

#if UNITY_EDITOR
public class MGenerateBillboard : ScriptableWizard
{
	[Header("This should be a Nature/Speedtree billboard material")]
	public Material m_material;
	[Range(0, 1)]
	public float topWidth = 1;
	[Range(0, 1)]
	public float midWidth = 1;
	[Range(0, 1)]
	public float botWidth = 1;

	[Tooltip("Units in height of the object, roughly, this can be fine-tuned later on the final asset")]
	public float objectHeight = 5.0f;
	[Tooltip("Units in width of the object, roughly, this can be fine-tuned later on the final asset")]
	public float objectWidth = 4.0f;
	[Tooltip("Usually negative and small, to make it sit in the ground slightly, can be modifed on final asset")]
	public float bottomOffset = 0.0f;

	[Min(1)]
	public int atlasRowImageCount = 3;
	[Min(1)]
	public int atlasColumnImageCount = 3;
	[Min(1)]
	public int totalImageCount = 8;

	[Header("This dictates the center of the render for the object")]
	[Tooltip("This dictates the center of the render for the object")]
	public GameObject toRotateCamera;
	[Header("This should be child of toRotateCamera, and on the local +x axis from it, facing center")]
	[Tooltip("This should be child of toRotateCamera, and on the local +x axis from it, facing center")]
	public Camera renderCamera;

	public int atlasPixelWidth = 1024;
	public int atlasPixelHeight = 1024;

	[Header("When Rendering cameras in the Editor the snapshot includes the GREY SPACE of EDITOR UI")]
	//You must shift it to match YOUR EDITOR's Playmode Aspect Ratio until no corners of UI are seen
	//See more about this issue here: https://answers.unity.com/questions/41195/readpixels-requires-an-offset-while-in-the-editor.html
	//Setting the game view to "Free aspect" could help eliminate these corners
	[Tooltip("Adjust this to match YOUR EDITOR's Playmode Aspect Ratio until the object is centered on the X axis")]
	public int cameraShiftX = 128;
	[Tooltip("Adjust this to match YOUR EDITOR's Playmode Aspect Ratio until the object is centered on the Y axis")]
	public int cameraShiftY = 10;
	[Tooltip("Adjust this to scale the camera render rect up to get a fuller coverage")]
	public float cameraScale = 2f;
	[Tooltip("Color to replace with complete alpha - set Camera to depth only and this to black")]
	public Color clearColor = Color.black;

	[Header("Optional Billboard asset to dump data to")]

	public BillboardRenderer optionalBillboardRenderer;

	void OnWizardUpdate()
	{
		//string helpString = "";
		bool isValid = (m_material != null && objectHeight != 0 && objectWidth != 0 && renderCamera != null && toRotateCamera != null);

		if (toRotateCamera != null)
		{
			Camera cam = toRotateCamera.GetComponentInChildren<Camera>();
			if (cam != null) { renderCamera = cam; }
		}
	}

	private void OnWizardCreate()
	{
		EditorCoroutineUtility.StartCoroutine(InternalMakeBillboard(), this);
	}


	IEnumerator InternalMakeBillboard()
	{
		//function to execute on submit

		BillboardAsset billboard = new BillboardAsset();

		billboard.material = m_material;
		Vector4[] texCoords = new Vector4[totalImageCount];

		ushort[] indices = new ushort[12];
		Vector2[] vertices = new Vector2[6];

		//make texture to save at end
		var texture = new Texture2D(atlasPixelWidth, atlasPixelHeight, TextureFormat.ARGB32, false);

		//Set all pixels to alpha 0
		Color[] allColors = texture.GetPixels();
		for (int i = 0; i < allColors.Length; ++i)
		{
			allColors[i].a = 0.0f;
		}
		texture.SetPixels(allColors);

		//make render texture to copy to texture and assign it to camera
		//renderCamera.targetTexture = RenderTexture.GetTemporary(atlasPixelWidth, atlasPixelHeight, 16);
		//var renderTex = renderCamera.targetTexture;
		//renderCamera.targetTexture = renderTex;

		//reset rotation, but camera should be on local +x axis from rotating object
		toRotateCamera.transform.eulerAngles = Vector3.zero;
		int imageAt = 0;
		for (int j = 0; j < atlasRowImageCount; j++)
		{
			for (int i = 0; i < atlasColumnImageCount; i++)
			{
				//i is x, j is y
				if (imageAt < totalImageCount)
				{
					//atlas them left-right, top-bottom, 0,0 is bottom left
					float xRatio = (float)i / atlasColumnImageCount;
					float yRatio = (float)(atlasRowImageCount - j - 1) / atlasRowImageCount;

					//Debug.Log("X: " + xRatio + " Y: " + yRatio + " 1overatlasCol: " + ((float)1 / atlasColumnImageCount * atlasPixelWidth));

					//starts at viewing from +x, and rotates camera clockwise around object, uses amount of vertices set (later down) to tell how many angles to view from
					texCoords[imageAt].Set(xRatio, yRatio, (float)1 / atlasColumnImageCount, (float)1 / atlasRowImageCount);
					imageAt++;

					//set rect of where to render texture to
					//renderCamera.rect = new Rect(xRatio, yRatio, (float)1 / atlasColumnImageCount, (float)1 / atlasRowImageCount);
					renderCamera.rect = new Rect(0, 0, 1f / atlasColumnImageCount * cameraScale, 1f / atlasRowImageCount * cameraScale);
					renderCamera.Render();

					//read pixels on rec
					Rect rec = new Rect(xRatio * atlasPixelWidth, yRatio * atlasPixelHeight, (float)1 / atlasColumnImageCount * atlasPixelWidth, (float)1 / atlasRowImageCount * atlasPixelHeight);
					//texture.ReadPixels(rec, i / atlasColumnImageCount * atlasPixelWidth, (atlasRowImageCount - j - 1) / atlasRowImageCount * atlasPixelHeight);
					//Override rect for modified code
					rec = new Rect(cameraShiftX, cameraShiftY,
						atlasPixelWidth / atlasColumnImageCount, atlasPixelHeight / atlasRowImageCount);
					//rec = new Rect(0, 0, Screen.width, Screen.height);
					float xPos = xRatio * atlasPixelWidth;
					float yPos = yRatio * atlasPixelWidth;

					yield return new WaitForEndOfFrame();

					//Debug.Log("XPOS: " + xPos + " YPOS: " + yPos);
					texture.ReadPixels(rec, Mathf.FloorToInt(xPos), Mathf.FloorToInt(yPos));

					//toRotateCamera.transform.eulerAngles -= Vector3.up * (360 / totalImageCount);
					var eulerStore = toRotateCamera.transform.eulerAngles;
					eulerStore.y -= (360f / totalImageCount);
					toRotateCamera.transform.eulerAngles = eulerStore;
				}
			}
		}


		Color[] cols = texture.GetPixels();
		for (int i = 0; i < cols.Length; ++i)
		{
			//cols[i] = Color.Lerp(cols[i], colors[mip], 0.33f);
			if (cols[i] == clearColor)
			{
				cols[i].a = 0.0f;
			}
		}
		texture.SetPixels(cols);

		//Apply the texture changes
		texture.Apply();

		toRotateCamera.transform.eulerAngles = Vector3.zero;
		renderCamera.rect = new Rect(0, 0, 1, 1);

		//Copytexture is not necessary - we already used ReadPixels() on texture 
		//Graphics.CopyTexture(renderTex, texture);

		//texCoords[0].Set(0.230981f, 0.33333302f, 0.230981f, -0.33333302f);
		//texCoords[1].Set(0.230981f, 0.66666603f, 0.230981f, -0.33333302f);
		//texCoords[2].Set(0.33333302f, 0.0f, 0.33333302f, 0.23098099f);
		//texCoords[3].Set(0.564314f, 0.23098099f, 0.23098099f, -0.33333302f);
		//texCoords[4].Set(0.564314f, 0.564314f, 0.23098099f, -0.33333403f);
		//texCoords[5].Set(0.66666603f, 0.0f, 0.33333302f, 0.23098099f);
		//texCoords[6].Set(0.89764804f, 0.23098099f, 0.230982f, -0.33333302f);
		//texCoords[7].Set(0.89764804f, 0.564314f, 0.230982f, -0.33333403f);

		//make basic box out of four trinagles, to be able to pinch the top/bottom/middle to cut extra transparent pixels
		//still not sure how this works but it connects vertices to make the mesh
		indices[0] = 4;
		indices[1] = 3;
		indices[2] = 0;
		indices[3] = 1;
		indices[4] = 4;
		indices[5] = 0;
		indices[6] = 5;
		indices[7] = 4;
		indices[8] = 1;
		indices[9] = 2;
		indices[10] = 5;
		indices[11] = 1;

		//set vertices positions on mesh
		vertices[0].Set(-botWidth / 2 + 0.5f, 0);
		vertices[1].Set(-midWidth / 2 + 0.5f, 0.5f);
		vertices[2].Set(-topWidth / 2 + 0.5f, 1);
		vertices[3].Set(botWidth / 2 + 0.5f, 0);
		vertices[4].Set(midWidth / 2 + 0.5f, 0.5f);
		vertices[5].Set(topWidth / 2 + 0.5f, 1);

		//assign data
		billboard.SetImageTexCoords(texCoords);
		billboard.SetIndices(indices);
		billboard.SetVertices(vertices);

		billboard.width = objectWidth;
		billboard.height = objectHeight;
		billboard.bottom = bottomOffset;

		//save assets
		string path;
		int nameLength = AssetDatabase.GetAssetPath(m_material).Length;
		Debug.Log("nameLength " + nameLength);
		//take out ".mat" prefix
		path = AssetDatabase.GetAssetPath(m_material).Substring(0, nameLength - 4) + ".asset";
		AssetDatabase.CreateAsset(billboard, path);
		path = AssetDatabase.GetAssetPath(m_material).Substring(0, nameLength - 4) + ".png";
		byte[] byteArray = ImageConversion.EncodeToPNG(texture);
		System.IO.File.WriteAllBytes(path, byteArray);

		Debug.Log("Billboard Asset Created & PNG File Bytes Saved: " + path);

		if (optionalBillboardRenderer != null)
		{
			optionalBillboardRenderer.billboard = billboard;
		}

		//cleanup / qol things
		//Debug.LogWarning("You will want to assign the saved texture to the material now");
		//renderCamera.targetTexture = null;

		//Do not set the generated texture to the material! 
		//It is not linked to the PNG asset we just created and the reference to the temp texture will be lost after a while
		//m_material.SetTexture("_MainTex", texture);

		//Files have changed in the database, refresh them
		AssetDatabase.Refresh();

		//RenderTexture.ReleaseTemporary(renderTex);

		//NOTE: You may have to rotate and/or scale your billboard renderer to match the final rotation of your original object
		//Luckily, Billboard renderer reflects changes to your gameobject's transform
		yield return null;
	}

	[MenuItem("Window/Rendering/Generate Billboard of Object")]
	static void MakeBillboard()
	{
		ScriptableWizard.DisplayWizard<MGenerateBillboard>(
			"Make Billboard from Game Camera View", "Create");
	}
}
#endif