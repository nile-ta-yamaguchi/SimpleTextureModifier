using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Linq;
using System;

namespace SimpleTextureModifier {
[InitializeOnLoad]
public class StartupTextureModifier {
    static StartupTextureModifier() {
        Debug.Log("Initialized TextureModifier");
        EditorUserBuildSettings.activeBuildTargetChanged += OnChangePlatform;
    }

    [UnityEditor.MenuItem("Assets/Texture Util/Reimport All Texture", false, 1)]
    static void OnChangePlatform() {
        Debug.Log(" TextureModifier Convert Compress Texture");
        string labels = "t:Texture";
        string clabels = "t:Texture";
		foreach(var type in TextureModifier.compressOutputs){
			clabels+=" l:"+type.ToString();
		}
		string rlabels = "t:Texture";
		foreach(var type in TextureModifier.RGBA16bitsOutputs){
			rlabels+=" l:"+type.ToString();
		}
		string plabels = "t:Texture";
		foreach(var type in TextureModifier.PNGOutputs){
			plabels+=" l:"+type.ToString();
		}
		string jlabels = "t:Texture";
		foreach(var type in TextureModifier.JPGOutputs){
			jlabels+=" l:"+type.ToString();
		}
		AssetDatabase.StartAssetEditing ();
        {
            var assets = AssetDatabase.FindAssets(labels, null);
            foreach (var asset in assets) {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var obj = AssetDatabase.LoadAssetAtPath(path,typeof(Texture));
                var importer = AssetImporter.GetAtPath(path);
                if (obj != null && importer != null) {
                    List<string> lb = new List<string>(AssetDatabase.GetLabels(obj));
                    importer.userData = String.Join(",", lb.ToArray());
                    AssetDatabase.WriteImportSettingsIfDirty(path);
                }
            }
        }
        {
			var assets = AssetDatabase.FindAssets (clabels, null);
			foreach (var asset in assets) {
				var path = AssetDatabase.GUIDToAssetPath (asset);
				if (CheckTargetCompressTexture (path))
					AssetDatabase.ImportAsset (path);
			}
		}
		{
			var assets = AssetDatabase.FindAssets (rlabels, null);
			foreach (var asset in assets) {
				var path = AssetDatabase.GUIDToAssetPath (asset);
				if (CheckTargetPNGTexture (path))
					AssetDatabase.ImportAsset (path);
			}
		}
		{
			var assets = AssetDatabase.FindAssets (plabels, null);
			foreach (var asset in assets) {
				var path = AssetDatabase.GUIDToAssetPath (asset);
				if (CheckTargetJPGTexture (path))
					AssetDatabase.ImportAsset (path);
			}
		}
		AssetDatabase.StopAssetEditing ();
	}

	static bool CheckTargetCompressTexture(string path){
		if (String.IsNullOrEmpty (path))
			return false;
		Texture2D tex=AssetDatabase.LoadAssetAtPath(path,typeof(Texture2D)) as Texture2D;
		switch (EditorUserBuildSettings.activeBuildTarget) {
		case BuildTarget.Android:
			if(tex.format==TextureFormat.ETC_RGB4)
				return false;
			break;
#if UNITY_5
		case BuildTarget.iOS:
#else
		case BuildTarget.iPhone:
#endif
			if(tex.format==TextureFormat.PVRTC_RGB4 || tex.format==TextureFormat.PVRTC_RGBA4)
				return false;
			break;
		default:
			if(tex.format==TextureFormat.DXT1 || tex.format==TextureFormat.DXT5)
				return false;
			break;
		}
		return true;
	}

	static bool CheckTargetRGBA16bitsTexture(string path){
		if (String.IsNullOrEmpty (path))
			return false;
		Texture2D tex=AssetDatabase.LoadAssetAtPath(path,typeof(Texture2D)) as Texture2D;
		if(tex.format==TextureFormat.RGBA4444 || tex.format==TextureFormat.ARGB4444)
			return false;
		return true;
	}
	static bool CheckTargetPNGTexture(string path){
		if (String.IsNullOrEmpty (path))
			return false;
		Texture2D tex=AssetDatabase.LoadAssetAtPath(path+"RGBA",typeof(Texture2D)) as Texture2D;
		if(tex!=null)
			return false;
		return true;
	}
	static bool CheckTargetJPGTexture(string path){
		if (String.IsNullOrEmpty (path))
			return false;
		Texture2D tex=AssetDatabase.LoadAssetAtPath(path+"RGB",typeof(Texture2D)) as Texture2D;
		if(tex!=null)
			return false;
		return true;
	}
}

public class TextureModifier : AssetPostprocessor {
	public enum TextureModifierType {
		None,
		PremultipliedAlpha,
		AlphaBleed,
		FloydSteinberg,
		Reduced16bits,
        C16bits,
        CCompressed,
        CCompressedNA,
        CCompressedWA,
        T32bits,
		T16bits,
		TCompressed,
        TCompressedNA,
        TCompressedWA,
		TPNG,
		TJPG,
	}

	static TextureFormat CompressionFormat {
		get {
			switch (EditorUserBuildSettings.activeBuildTarget) {
			case BuildTarget.Android:
				return TextureFormat.ETC_RGB4;
#if UNITY_5
			case BuildTarget.iOS:
#else
			case BuildTarget.iPhone:
#endif
				return TextureFormat.PVRTC_RGB4;
			default:
				return TextureFormat.DXT1;
			}
		}
	}

	static TextureFormat CompressionWithAlphaFormat {
		get {
			switch (EditorUserBuildSettings.activeBuildTarget) {
			case BuildTarget.Android:
				return TextureFormat.ETC_RGB4;
#if UNITY_5
			case BuildTarget.iOS:
#else
			case BuildTarget.iPhone:
#endif
				return TextureFormat.PVRTC_RGBA4;
			default:
				return TextureFormat.DXT5;
			}
		}
	}

	struct Position2 {
		public int x,y;
		public Position2(int p1, int p2)
		{
			x = p1;
			y = p2;
		}
	}
	
	readonly static Type inspectorWindowType = Assembly.GetAssembly(typeof(EditorWindow)).GetType ("UnityEditor.InspectorWindow");
	readonly static Type labelGUIType = Assembly.GetAssembly(typeof(EditorWindow)).GetType ("UnityEditor.LabelGUI");
	readonly static FieldInfo m_LabelGUIField = inspectorWindowType.GetField("m_LabelGUI"
	                                                                         ,BindingFlags.GetField | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
	readonly static FieldInfo m_CurrentAssetsSetField = labelGUIType.GetField("m_CurrentAssetsSet"
	                                                                          ,BindingFlags.GetField | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance);
	static void SetLabelSetingsDirty() {
		if (m_LabelGUIField == null || m_CurrentAssetsSetField == null)
			return;
		var editorWindows = Resources.FindObjectsOfTypeAll (typeof(EditorWindow)) as EditorWindow[];
//		Debug.Log("m_LabelGUIField="+m_LabelGUIField);
		foreach (var ew in editorWindows) {
//			Debug.Log("editorWindow="+ew.title);
			if (ew.title=="UnityEditor.InspectorWindow" || ew.title=="Inspector") { // "UnityEditor.InspectorWindow"<=v4 "Inspector">=v5
				var labelGUIObject = m_LabelGUIField.GetValue(ew);
				m_CurrentAssetsSetField.SetValue(labelGUIObject,null);
			}
		}
	}

	public readonly static List<TextureModifierType> effecters=new List<TextureModifierType>{TextureModifierType.PremultipliedAlpha,TextureModifierType.AlphaBleed};
    public readonly static List<TextureModifierType> modifiers = new List<TextureModifierType> { TextureModifierType.FloydSteinberg, TextureModifierType.Reduced16bits };
    public readonly static List<TextureModifierType> outputs = new List<TextureModifierType>{TextureModifierType.TJPG,TextureModifierType.TPNG,TextureModifierType.T32bits,TextureModifierType.T16bits,TextureModifierType.C16bits
                                                                            ,TextureModifierType.CCompressed,TextureModifierType.CCompressedNA,TextureModifierType.CCompressedWA
																			,TextureModifierType.TCompressed,TextureModifierType.TCompressedNA,TextureModifierType.TCompressedWA};
    public readonly static List<TextureModifierType> compressOutputs = new List<TextureModifierType>{
                                                                             TextureModifierType.CCompressed,TextureModifierType.CCompressedNA,TextureModifierType.CCompressedWA
                                                                             ,TextureModifierType.TCompressed,TextureModifierType.TCompressedNA,TextureModifierType.TCompressedWA};
	public readonly static List<TextureModifierType> RGBA16bitsOutputs = new List<TextureModifierType>{TextureModifierType.C16bits};
	public readonly static List<TextureModifierType> PNGOutputs = new List<TextureModifierType>{TextureModifierType.TPNG};
	public readonly static List<TextureModifierType> JPGOutputs = new List<TextureModifierType>{TextureModifierType.TJPG};

	static void ClearLabel(List<TextureModifierType> types, bool ImportAsset = true) {
		List<UnityEngine.Object> objs=new List<UnityEngine.Object>(Selection.objects);
		foreach(var obj in objs){
			if(obj is Texture2D){
				List<string> labels=new List<string>(AssetDatabase.GetLabels(obj));
				var newLabels=new List<string>();
				labels.ForEach((string l)=>{
					if(Enum.IsDefined(typeof(TextureModifierType),l)){
						if(!types.Contains((TextureModifierType)Enum.Parse(typeof(TextureModifierType),l)))
							newLabels.Add(l);
					}
				});
				AssetDatabase.SetLabels(obj,newLabels.ToArray());
                var importer=AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
				if(newLabels.Count>0)
					importer.userData = String.Join(",", newLabels.ToArray());
				else
					importer.userData = null;
				EditorUtility.SetDirty(obj);
                AssetDatabase.WriteImportSettingsIfDirty(AssetDatabase.GetAssetPath(obj));
            }
		}
	}

	static void SetLabel(string label,List<TextureModifierType> types){
		ClearLabel(types,false);
		List<UnityEngine.Object> objs=new List<UnityEngine.Object>(Selection.objects);
		foreach(var obj in objs){
			if(obj is Texture2D){
				List<string> labels=new List<string>(AssetDatabase.GetLabels(obj));
				labels.Add(label);
				AssetDatabase.SetLabels(obj,labels.ToArray());
                var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
				if(labels.Count>0)
	                importer.userData = String.Join(",", labels.ToArray());
				else
					importer.userData = null;
                EditorUtility.SetDirty(obj);
                AssetDatabase.WriteImportSettingsIfDirty(AssetDatabase.GetAssetPath(obj));
			}
		}
	}

	[UnityEditor.MenuItem("Assets/Texture Util/Clear Texture Effecter Label",false,20)]
	static void ClearTextureEffecterLabel(){
		ClearLabel(effecters);
		SetLabelSetingsDirty ();
	}
	[UnityEditor.MenuItem("Assets/Texture Util/Set Label PremultipliedAlpha",false,20)]
	static void SetLabelPremultipliedAlpha(){
		SetLabel(TextureModifierType.PremultipliedAlpha.ToString(),effecters);
		SetLabelSetingsDirty ();
	}
	[UnityEditor.MenuItem("Assets/Texture Util/Set Label AlphaBleed",false,20)]
	static void SetLabelAlphaBleed(){
		SetLabel(TextureModifierType.AlphaBleed.ToString(),effecters);
		SetLabelSetingsDirty ();
	}

	[UnityEditor.MenuItem("Assets/Texture Util/Clear Texture Modifier Label",false,40)]
	static void ClearTextureModifierLabel(){
		ClearLabel(modifiers);
		SetLabelSetingsDirty ();
	}
	[UnityEditor.MenuItem("Assets/Texture Util/Set Label FloydSteinberg",false,40)]
	static void SetLabelFloydSteinberg(){
		SetLabel(TextureModifierType.FloydSteinberg.ToString(),modifiers);
		SetLabelSetingsDirty ();
	}
	[UnityEditor.MenuItem("Assets/Texture Util/Set Label Reduced16bits",false,40)]
	static void SetLabelReduced16bits(){
		SetLabel(TextureModifierType.Reduced16bits.ToString(),modifiers);
		SetLabelSetingsDirty ();
	}

	[UnityEditor.MenuItem("Assets/Texture Util/Clear Texture Output Label",false,60)]
	static void ClearTextureOutputLabel(){
		ClearLabel(outputs);
		SetLabelSetingsDirty ();
	}
    [UnityEditor.MenuItem("Assets/Texture Util/Set Label Convert 16bits", false, 60)]
    static void SetLabelC16bits() {
        SetLabel(TextureModifierType.C16bits.ToString(), outputs);
		SetLabelSetingsDirty ();
	}
    [UnityEditor.MenuItem("Assets/Texture Util/Set Label Convert Compressed", false, 60)]
    static void SetLabelCCompressed() {
        SetLabel(TextureModifierType.CCompressed.ToString(), outputs);
		SetLabelSetingsDirty ();
	}
    [UnityEditor.MenuItem("Assets/Texture Util/Set Label Convert Compressed no alpha", false, 60)]
    static void SetLabelCCompressedNA() {
        SetLabel(TextureModifierType.CCompressedNA.ToString(), outputs);
		SetLabelSetingsDirty ();
	}
    [UnityEditor.MenuItem("Assets/Texture Util/Set Label Convert Compressed with alpha", false, 60)]
    static void SetLabelCCompressedWA() {
        SetLabel(TextureModifierType.CCompressedWA.ToString(), outputs);
		SetLabelSetingsDirty ();
	}
#if false
    [UnityEditor.MenuItem("Assets/Texture Util/Set Label Texture 16bits", false, 60)]
	static void SetLabel16bits(){
		SetLabel(TextureModifierType.T16bits.ToString(),outputs);
		SetLabelSetingsDirty ();
	}
	[UnityEditor.MenuItem("Assets/Texture Util/Set Label Texture 32bits",false,60)]
	static void SetLabel32bits(){
		SetLabel(TextureModifierType.T32bits.ToString(),outputs);
		SetLabelSetingsDirty ();
	}
	[UnityEditor.MenuItem("Assets/Texture Util/Set Label Texture Compressed",false,60)]
	static void SetLabelCompressed(){
		SetLabel(TextureModifierType.TCompressed.ToString(),outputs);
		SetLabelSetingsDirty ();
	}
    [UnityEditor.MenuItem("Assets/Texture Util/Set Label Texture Compressed no alpha", false, 60)]
    static void SetLabelCompressedNA() {
        SetLabel(TextureModifierType.TCompressedNA.ToString(), outputs);
		SetLabelSetingsDirty ();
	}
    [UnityEditor.MenuItem("Assets/Texture Util/Set Label Texture Compressed with alpha", false, 60)]
	static void SetLabelCompressedWA(){
		SetLabel(TextureModifierType.TCompressedWA.ToString(),outputs);
		SetLabelSetingsDirty ();
	}
#endif
	[UnityEditor.MenuItem("Assets/Texture Util/Set Label Texture PNG",false,60)]
	static void SetLabelPNG(){
		SetLabel(TextureModifierType.TPNG.ToString(),outputs);
		SetLabelSetingsDirty ();
	}
	[UnityEditor.MenuItem("Assets/Texture Util/Set Label Texture JPG",false,60)]
	static void SetLabelJPG(){
		SetLabel(TextureModifierType.TJPG.ToString(),outputs);
		SetLabelSetingsDirty ();
	}

	static readonly string TTextureModifier="modifier";
	static readonly string TTextureSetting="setting";

	TextureModifierType effecterType=TextureModifierType.None;
	TextureModifierType modifierType=TextureModifierType.None;
	TextureModifierType outputType=TextureModifierType.None;

	void OnPreprocessTexture(){
		//return;
		var importer = (assetImporter as TextureImporter);
		if(SimpleTextureModifierSettings.ForceFileNameIndicator) {
			string filename = System.IO.Path.GetFileNameWithoutExtension (assetPath);
			var filenameParts = filename.ToLower().Split("_".ToCharArray()).ToList ();
			foreach (string part in filenameParts) {
				if (part.StartsWith (TTextureModifier)) {
					PersModifier (part.Substring (TTextureModifier.Length));
				}
				if (part.StartsWith (TTextureSetting)) {
					PersSetting (part.Substring (TTextureSetting.Length));
				}
			}
		}

		if(SimpleTextureModifierSettings.ForceQualitytSetting){
			importer.compressionQuality=SimpleTextureModifierSettings.CompressQuality;
		}
		if(SimpleTextureModifierSettings.ForceIOSQualitytSetting){
			int maxTextureSize;
			TextureImporterFormat textureFormat;
			int compressionQuality;
			if(importer.GetPlatformTextureSettings("iPhone",out maxTextureSize,out textureFormat,out compressionQuality)){
				importer.SetPlatformTextureSettings("iPhone",maxTextureSize,textureFormat,SimpleTextureModifierSettings.CompressIOSQuality,false);
			}
			if(EditorUserBuildSettings.activeBuildTarget==BuildTarget.iOS){
				importer.compressionQuality=SimpleTextureModifierSettings.CompressIOSQuality;
			}
		}
		if (SimpleTextureModifierSettings.ForceAndroidQualitytSetting) {
			int maxTextureSize;
			TextureImporterFormat textureFormat;
			int compressionQuality;
			if (importer.GetPlatformTextureSettings (BuildTarget.Android.ToString(), out maxTextureSize, out textureFormat, out compressionQuality)) {
				importer.SetPlatformTextureSettings (BuildTarget.Android.ToString(), maxTextureSize, textureFormat, SimpleTextureModifierSettings.CompressAndroidQuality,false);
			}
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) {
				importer.compressionQuality = SimpleTextureModifierSettings.CompressAndroidQuality;
			}
		}
		if(EditorUserBuildSettings.activeBuildTarget==BuildTarget.Android && SimpleTextureModifierSettings.ChangAndroidAutoCompressSetting && importer.textureFormat == TextureImporterFormat.AutomaticCompressed){
			int maxTextureSize;
			TextureImporterFormat textureFormat;
			int compressionQuality;
			if( importer.DoesSourceTextureHaveAlpha() && !importer.GetPlatformTextureSettings(BuildTarget.Android.ToString(),out maxTextureSize,out textureFormat,out compressionQuality)){
				importer.SetPlatformTextureSettings(BuildTarget.Android.ToString(),maxTextureSize,TextureImporterFormat.RGBA16,compressionQuality,false);
			}
		}


		UnityEngine.Object obj=AssetDatabase.LoadAssetAtPath(assetPath,typeof(Texture2D));
		var labels=new List<string>(AssetDatabase.GetLabels(obj));
        if (labels == null || labels.Count == 0) {
			if(!String.IsNullOrEmpty(importer.userData)) {
				labels = importer.userData.Split ("," [0]).ToList ();
				AssetDatabase.SetLabels(obj,labels.ToArray());
				SetLabelSetingsDirty ();
			}
		}
		foreach(string label in labels){
            if (Enum.IsDefined(typeof(TextureModifierType), label))
            {
				TextureModifierType type=(TextureModifierType)Enum.Parse(typeof(TextureModifierType),label);
				if(effecters.Contains(type)){
					effecterType=type;
				}
				if(modifiers.Contains(type)){
					modifierType=type;
				}
				if(outputs.Contains(type)){
					outputType=type;
				}
			}
		}



		if (!String.IsNullOrEmpty (importer.spritePackingTag))
			return;
		if(effecterType!=TextureModifierType.None || modifierType!=TextureModifierType.None || outputType!=TextureModifierType.None){
			if(!SimpleTextureModifierSettings.ForceFormatSetting)
				return;
			importer.alphaIsTransparency=false;
//			importer.compressionQuality = (int)TextureCompressionQuality.Best;
			if(importer.textureFormat==TextureImporterFormat.Automatic16bit)
				importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			else if(importer.textureFormat==TextureImporterFormat.AutomaticCompressed)
				importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
			else if(importer.textureFormat==TextureImporterFormat.RGB16)
				importer.textureFormat = TextureImporterFormat.RGB24;
			else if(importer.textureFormat==TextureImporterFormat.RGBA16)
				importer.textureFormat = TextureImporterFormat.RGBA32;
			else if(importer.textureFormat==TextureImporterFormat.ARGB16)
				importer.textureFormat = TextureImporterFormat.ARGB32;
		}
	}

	void PersModifier(string tokens)
	{
		var queue = new Queue<char> (tokens.ToCharArray ());
		while (queue.Count > 0) {
			char token = queue.Dequeue ();
			switch (token) {
			case 'm':
				effecterType = TextureModifierType.PremultipliedAlpha;
				break;
			case 'a':
				effecterType = TextureModifierType.AlphaBleed;
				break;
			case 'd':
				effecterType = TextureModifierType.FloydSteinberg;
				break;
			case 'r':
				modifierType = TextureModifierType.Reduced16bits;
				break;
			case 'f':
				modifierType = TextureModifierType.C16bits;
				break;
			case 'c':
				if (queue.Count > 0) {
					if (queue.Peek () == 'n') {
						outputType = TextureModifierType.CCompressedNA;
						queue.Dequeue ();
						break;
					} else if (queue.Peek () == 'w') {
						outputType = TextureModifierType.CCompressedWA;
						queue.Dequeue ();
						break;
					}
				}
				outputType = TextureModifierType.CCompressed;
				break;
			default:
				break;
			}
		}
	}

	void PersSetting(string tokens)
	{
		var importer = (assetImporter as TextureImporter);
		var queue = new Queue<char> (tokens.ToCharArray ());
		while (queue.Count > 0) {
			char token = queue.Dequeue ();
			switch (token) {
			case 't':
				importer.textureType = TextureImporterType.Image;
				break;
			case 'n':
				importer.textureType = TextureImporterType.Bump;
				break;
			case 'e':
				importer.textureType = TextureImporterType.GUI;
				break;
			case 's':
				importer.textureType = TextureImporterType.Sprite;
				break;
			case 'l':
				importer.textureType = TextureImporterType.Lightmap;
				break;
			case 'a':
				importer.textureType = TextureImporterType.Advanced;
				break;
			case 'f':
				if (queue.Count > 0) {
					if (queue.Peek () == 'c') {
						importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
						queue.Dequeue ();
						break;
					} else if (queue.Peek () == 't') {
						importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
						queue.Dequeue ();
						break;
					} else if (queue.Peek () == 'f') {
						importer.textureFormat = TextureImporterFormat.Automatic16bit;
						queue.Dequeue ();
						break;
					}
				}
				importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
				break;
			case 'm':
				int num = GetNum (queue);
				if (num <= 0)
					break;
				int max = (int)Mathf.Pow (2,Mathf.Floor (Mathf.Log (num - 1,2)) + 1);
				if (max >= 32 && max <= 8192)
					importer.maxTextureSize = max;
				break;
			case 'd':
				importer.SetAllowsAlphaSplitting (true);
				break;
			}
		}
	}

	int GetNum(Queue<char> queue)
	{
		var digit = new List<char> ();
		while (queue.Count > 0 && Char.IsDigit (queue.Peek ())) {
			digit.Add (queue.Dequeue ());
		}
		if (digit.Count > 0) {
			return int.Parse (new String (digit.ToArray ()));
		}
		return 0;
	}
	
	static TextureCompressionQuality GetPlatformTextureCompressionQuality(){
		TextureCompressionQuality quality = TextureCompressionQuality.Normal;
		if(SimpleTextureModifierSettings.ForceIOSQualitytSetting && EditorUserBuildSettings.activeBuildTarget==BuildTarget.iOS){
			quality=(TextureCompressionQuality)SimpleTextureModifierSettings.CompressIOSQuality;
		}else if(SimpleTextureModifierSettings.ForceAndroidQualitytSetting && EditorUserBuildSettings.activeBuildTarget==BuildTarget.Android){
			quality=(TextureCompressionQuality)SimpleTextureModifierSettings.CompressAndroidQuality;
		}else if(SimpleTextureModifierSettings.ForceQualitytSetting){
			quality=(TextureCompressionQuality)SimpleTextureModifierSettings.CompressQuality;
		}
		return quality;
	}

	static void createMaterialWithAlpha(string textureName) {
		string alphaName= textureName.Substring(0,textureName.LastIndexOf('.'))+"Alpha.asset";
		string matName = Path.ChangeExtension(textureName, ".mat");
		if (!File.Exists(matName)){
			Texture2D texture2D = AssetDatabase.LoadAssetAtPath(textureName, typeof(Texture2D)) as Texture2D;
			Texture2D texture2D1 = AssetDatabase.LoadAssetAtPath(alphaName, typeof(Texture2D)) as Texture2D;
			if (texture2D1 == null) {
				return;
			}
			bool flag = true;
			Shader shader = Shader.Find("Custom/WithMask");
			if (shader == null) {
				return;
			}
			Material material = new Material(shader);
			material.SetTexture("_MainTex", texture2D);
			material.SetTexture("_MaskTex", texture2D1);
			AssetDatabase.CreateAsset(material, matName);
			EditorUtility.SetDirty(material);
		}
	}
	
	void OnPostprocessTexture (Texture2D texture){
		if(effecterType==TextureModifierType.None && modifierType==TextureModifierType.None && outputType==TextureModifierType.None)
			return;
		AssetDatabase.StartAssetEditing();
		var pixels = texture.GetPixels ();
		switch (effecterType){
		case TextureModifierType.PremultipliedAlpha:{
			pixels=PremultipliedAlpha(pixels);
			break;
		}
		case TextureModifierType.AlphaBleed:{
			pixels=AlphaBleed(pixels,texture.width,texture.height);
			break;
		}}
		switch (modifierType){
		case TextureModifierType.FloydSteinberg:{
			pixels=FloydSteinberg(pixels,texture.width,texture.height);
			break;
		}
		case TextureModifierType.Reduced16bits:{
			pixels=Reduced16bits(pixels,texture.width,texture.height);
			break;
		}}
        //return;
		if (SimpleTextureModifierSettings.Output) {
            switch (outputType) {
                case TextureModifierType.C16bits: {
                    texture.SetPixels(pixels);
                    texture.Apply(true, true);
						EditorUtility.CompressTexture(texture, TextureFormat.RGBA4444, GetPlatformTextureCompressionQuality()); 
                    break;
                }
                case TextureModifierType.CCompressed: {
                    texture.SetPixels(pixels);
                    texture.Apply(true, true);
					EditorUtility.CompressTexture(texture, CompressionWithAlphaFormat, GetPlatformTextureCompressionQuality());
                    break;
                }
                case TextureModifierType.CCompressedNA: {
                    texture.SetPixels(pixels);
                    texture.Apply(true, true);
					EditorUtility.CompressTexture(texture, CompressionFormat, GetPlatformTextureCompressionQuality());
                    break;
                }
                case TextureModifierType.CCompressedWA: {
                    WriteAlphaTexture(pixels, texture);
                    texture.SetPixels(pixels);
                    texture.Apply(true, true);
					EditorUtility.CompressTexture(texture, CompressionFormat, GetPlatformTextureCompressionQuality());
					if(SimpleTextureModifierSettings.BuildSplitAlphaMaterial)
						createMaterialWithAlpha (assetPath);
                    break;
                }
                case TextureModifierType.TCompressed: {
                   var tex = BuildTexture(texture, TextureFormat.RGBA32);
                   tex.SetPixels(pixels);
                   tex.Apply(true, true);
                   WriteTexture(tex, CompressionWithAlphaFormat, assetPath, ".asset");
                   break;
               }
               case TextureModifierType.TCompressedNA: {
                   var tex = BuildTexture(texture, TextureFormat.RGBA32);
                   tex.SetPixels(pixels);
                   tex.Apply(true, true);
                   WriteTexture(tex, CompressionFormat, assetPath, ".asset");
                   break;
               }
               case TextureModifierType.TCompressedWA: {
                   WriteAlphaTexture(pixels, texture);
                   var tex = BuildTexture(texture, TextureFormat.RGBA32);
                   tex.SetPixels(pixels);
                   tex.Apply(true, true);
                   WriteTexture(tex, CompressionFormat, assetPath, ".asset");
					if(SimpleTextureModifierSettings.BuildSplitAlphaMaterial)
						createMaterialWithAlpha (Path.ChangeExtension(assetPath,".asset"));						
                   break;
               }
               case TextureModifierType.T16bits: {
                   var tex = BuildTexture(texture, TextureFormat.RGBA32);
                   tex.SetPixels(pixels);
                   tex.Apply(true, true);
                   WriteTexture(tex, TextureFormat.RGBA4444, assetPath, ".asset");
                   break;
               }
               case TextureModifierType.T32bits: {
                   var tex = BuildTexture(texture, TextureFormat.RGBA32);
                   tex.SetPixels(pixels);
                   tex.Apply(true, true);
                   WriteTexture(tex, TextureFormat.RGBA32, assetPath, ".asset");
                   break;
               }
               case TextureModifierType.TPNG: {
                   var tex = BuildTexture(texture, TextureFormat.RGBA32);
                   tex.SetPixels(pixels);
                   tex.Apply(true);
                   WritePNGTexture(tex, TextureFormat.RGBA32, assetPath, "RGBA.png");
                   break;
               }
               case TextureModifierType.TJPG: {
                   var tex = BuildTexture(texture, TextureFormat.RGBA32);
                   tex.SetPixels(pixels);
                   tex.Apply(true);
                   WriteJPGTexture(tex, TextureFormat.RGBA32, assetPath, "RGB.jpg");
                   break;
               }
               default: {
                   if (effecterType != TextureModifierType.None || modifierType != TextureModifierType.None) {
                       texture.SetPixels(pixels);
                       texture.Apply(true);
                   }
                   break;
                }
            }
        }
		AssetDatabase.Refresh();
		AssetDatabase.StopAssetEditing();
	}

	Texture2D BuildTexture(Texture2D texture,TextureFormat format){
		var tex = new Texture2D (texture.width, texture.height, format, texture.mipmapCount>1);
		tex.wrapMode = texture.wrapMode;
		tex.filterMode = texture.filterMode;
		tex.mipMapBias = texture.mipMapBias;
		tex.anisoLevel = texture.anisoLevel;
		return tex;
	}

	Texture2D WriteTexture(Texture2D texture,TextureFormat format,string path,string extension){
		EditorUtility.CompressTexture (texture,format,GetPlatformTextureCompressionQuality());
		var writePath = path.Substring(0,path.LastIndexOf('.'))+extension;
		var writeAsset = AssetDatabase.LoadAssetAtPath (writePath,typeof(Texture2D)) as Texture2D;
		if (writeAsset == null) {
			AssetDatabase.CreateAsset (texture, writePath);
			writeAsset = AssetDatabase.LoadAssetAtPath (writePath,typeof(Texture2D)) as Texture2D;
		} else {
			EditorUtility.CopySerialized (texture, writeAsset);
		}
		return writeAsset;
	}

	void WritePNGTexture(Texture2D texture,TextureFormat format,string path,string extension){
		EditorUtility.CompressTexture (texture,format,GetPlatformTextureCompressionQuality());
		byte[] pngData=texture.EncodeToPNG();
		//var nPath=path.Substring(0,path.LastIndexOf('.'))+extension;
		var writePath = Application.dataPath+(path.Substring(0,path.LastIndexOf('.'))+extension).Substring(6);
		File.WriteAllBytes(writePath, pngData);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
	}

	void WriteJPGTexture(Texture2D texture,TextureFormat format,string path,string extension){
		EditorUtility.CompressTexture (texture,format,GetPlatformTextureCompressionQuality());
		byte[] jpgData=texture.EncodeToJPG();
		//var nPath=path.Substring(0,path.LastIndexOf('.'))+extension;
		var writePath = Application.dataPath+(path.Substring(0,path.LastIndexOf('.'))+extension).Substring(6);
		File.WriteAllBytes(writePath, jpgData);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
	}

	void WriteCompressTexture(Color[] pixels,Texture2D texture,TextureFormat format){
		var mask = BuildTexture(texture,TextureFormat.RGB24);
		for (int i = 0; i < pixels.Length; i++) {
			var a = pixels [i].a;
			pixels [i] = new Color (a, a, a);
		}
		mask.SetPixels (pixels);
		mask.Apply(true,true);
		WriteTexture(mask,CompressionFormat,assetPath,"Alpha.asset");
	}

	Texture2D WriteAlphaTexture(Color[] pixels,Texture2D texture){
		var mask = new Texture2D (texture.width, texture.height, TextureFormat.RGB24, false);
		mask.wrapMode = texture.wrapMode;
		mask.filterMode = texture.filterMode;
		mask.mipMapBias = texture.mipMapBias;
		mask.anisoLevel = texture.anisoLevel;
		var aPixels = new Color[pixels.Length];
		for (int i = 0; i < pixels.Length; i++) {
			var a = pixels [i].a;
			aPixels [i] = new Color (a, a, a);
		}
		mask.SetPixels (aPixels);
		mask.Apply(true,true);
		return WriteTexture(mask,CompressionFormat,assetPath,"Alpha.asset");
	}

	static public Color[] PremultipliedAlpha(Color[] pixels){
		Color[] np= new Color[pixels.Length];
		for (int i = 0; i < pixels.Length; i++) {
			var a = pixels [i].a;
			np[i] = new Color (pixels[i].r*a,pixels[i].g*a,pixels[i].b*a,a);
		}
		return np;
	}

	enum BlockMode : byte {
		Untreated,
		Processing,
		Termination
	};
	static readonly int BlockSize=4;
	static public Color[] AlphaBleed(Color[] pixels,int width,int height){
		Color[] np= new Color[height*width];
		int blockHeight= (int)(height-1)/BlockSize+1;
		int blockWidth= (int)(width-1)/BlockSize+1;
		Color[] bc= new Color[blockHeight*blockWidth];
		BlockMode[] bf= new BlockMode[blockHeight*blockWidth];
		int remaining = 0;
		bool exitFlag = true;
		for (var yb = 0; yb < blockHeight; yb++) {
			for (var xb = 0; xb < blockWidth; xb++) {
				float r = 0.0f;
				float g = 0.0f;
				float b = 0.0f;
				float c = 0.0f;
				int n = 0;
				for (var y = 0; y < BlockSize; y++) {
					for (var x = 0; x < BlockSize; x++) {
						int xpos = xb * BlockSize + x;
						int ypos = yb * BlockSize + y;
						if (xpos < width && ypos < height) {
							int pos = ypos * width + xpos;
							float ad = pixels [pos].a;
							r += pixels [pos].r * ad;	
							g += pixels [pos].g * ad;	
							b += pixels [pos].b * ad;
							c += ad;
							if (ad <= 0.02f)
								n++;
						}
					}
				}
				var block = yb * blockWidth + xb;
				if (n > 0) {
					bf [block] = BlockMode.Processing;
					remaining++;
				} else {
					bf [block] = BlockMode.Termination;
				}
				if (c <= 0.02f) {
					bc [ block ] = new Color (0.0f, 0.0f, 0.0f, 0.0f);
					bf [ block ] = BlockMode.Untreated;
				} else {
					bc [ block ] = new Color (r / c, g / c, b / c, c / (float)(BlockSize * BlockSize));
					exitFlag = false;
				}
			}
		}
		if ( exitFlag || remaining==0 )
			return pixels;
		for (var y = 0; y < height; y++) {
			for (var x = 0; x < width; x++) {
				int pos = y * width + x;
				np [pos] = pixels [pos];
			}
		}
		BlockMode[] be= new BlockMode[blockHeight*blockWidth];
		for (int count=16; count > 0 && remaining > 0; count-- ) {
			for (int i=0;i < blockHeight*blockWidth ; i++ )
				be[i] = bf[i];
			for (var yb = 0; yb < blockHeight; yb++) {
				for (var xb = 0; xb < blockWidth; xb++) {
					var block = yb * blockWidth + xb;
					if (be [ block ] == BlockMode.Termination)
						continue;
					float r = 0.0f;
					float g = 0.0f;
					float b = 0.0f;
					float c = 0.0f;
					Color ccol = bc [yb * blockWidth + xb];
					r += (ccol.r * ccol.a * 16.0f);
					g += (ccol.g * ccol.a * 16.0f);
					b += (ccol.b * ccol.a * 16.0f);
					c += (ccol.a * 16.0f);
					int n = 0;
					for (var yp = yb - 1; yp <= yb + 1; yp++) {
						for (var xp = xb - 1; xp <= xb + 1; xp++) {
							var x = ( xp + blockWidth) % blockWidth;
							var y = ( yp + blockHeight) % blockHeight;
							if (be [y * blockWidth + x] != BlockMode.Untreated)
								n++;
							Color col = bc [y * blockWidth + x];
							r += col.r * col.a;
							g += col.g * col.a;
							b += col.b * col.a;
							c += col.a;
						}
					}
					if( n > 0 ) {
						if (c > 0.0f) {
							r /= c; g /= c; b /= c; c /= 24.0f;
						} else {
							r = 0.0f; g = 0.0f; b = 0.0f; c = 0.0f;
						}
						for (var y = 0; y < BlockSize; y++) {
							for (var x = 0; x < BlockSize; x++) {
								int xpos = xb * BlockSize + x;
								int ypos = yb * BlockSize + y;
								if (xpos < width && ypos < height) {
									int pos = ypos * width + xpos;
									if (pixels [pos].a <= 0.02f) {
										float ar = 1.0f - pixels [pos].a;
										np [pos] = new Color (r * ar + pixels [pos].r * (1.0f - ar)
											, g * ar + pixels [pos].g * (1.0f - ar)
											, b * ar + pixels [pos].b * (1.0f - ar)
											, pixels [pos].a);
									} else
										np [pos] = pixels[pos];
								}
							}
						}
						if( be [yb * blockWidth + xb] == BlockMode.Untreated )
							bc [yb * blockWidth + xb] = new Color (r, g, b, c);
						bf [yb * blockWidth + xb] = BlockMode.Termination;
						remaining--;
					}
				}
			}
		}
		return np;
	}

	const float k1Per256 = 1.0f / 255.0f;
	const float k1Per16 = 1.0f / 15.0f;
	const float k3Per16 = 3.0f / 15.0f;
	const float k5Per16 = 5.0f / 15.0f;
	const float k7Per16 = 7.0f / 15.0f;

	static public Color[] Reduced16bits(Color[] pixels,int texw,int texh){
		Color[] np= new Color[texh*texw];
		var offs = 0;
		for (var y = 0; y < texh; y++) {
			for (var x = 0; x < texw; x++) {
				float a = pixels [offs].a;
				float r = pixels [offs].r;
				float g = pixels [offs].g;
				float b = pixels [offs].b;
				
				var a2 = Mathf.Round(a * 15.0f) * k1Per16;
				var r2 = Mathf.Round(r * 15.0f) * k1Per16;
				var g2 = Mathf.Round(g * 15.0f) * k1Per16;
				var b2 = Mathf.Round(b * 15.0f) * k1Per16;

				np [offs].a = a2;
				np [offs].r = r2;
				np [offs].g = g2;
				np [offs].b = b2;
				offs++;
			}
		}
		return np;
	}

	static public Color[] FloydSteinberg(Color[] pixels,int texw,int texh){
		var offs = 0;
		for (var y = 0; y < texh; y++) {
			for (var x = 0; x < texw; x++) {
				float a = pixels [offs].a;
				float r = pixels [offs].r;
				float g = pixels [offs].g;
				float b = pixels [offs].b;
				
				var a2 = Mathf.Round(a * 15.0f) * k1Per16;
				var r2 = Mathf.Round(r * 15.0f) * k1Per16;
				var g2 = Mathf.Round(g * 15.0f) * k1Per16;
				var b2 = Mathf.Round(b * 15.0f) * k1Per16;
				
				var ae = Mathf.Round((a - a2)*255.0f)*k1Per256;
				var re = Mathf.Round((r - r2)*255.0f)*k1Per256;
				var ge = Mathf.Round((g - g2)*255.0f)*k1Per256;
				var be = Mathf.Round((b - b2)*255.0f)*k1Per256;
				
				pixels [offs].a = a2;
				pixels [offs].r = r2;
				pixels [offs].g = g2;
				pixels [offs].b = b2;
				
				var n1 = offs + 1;
				var n2 = offs + texw - 1;
				var n3 = offs + texw;
				var n4 = offs + texw + 1;
				
				if (x < texw - 1) {
					pixels [n1].a += ae * k7Per16;
					pixels [n1].r += re * k7Per16;
					pixels [n1].g += ge * k7Per16;
					pixels [n1].b += be * k7Per16;
				}
				
				if (y < texh - 1) {
					pixels [n3].a += ae * k5Per16;
					pixels [n3].r += re * k5Per16;
					pixels [n3].g += ge * k5Per16;
					pixels [n3].b += be * k5Per16;
					
					if (x > 0) {
						pixels [n2].a += ae * k3Per16;
						pixels [n2].r += re * k3Per16;
						pixels [n2].g += ge * k3Per16;
						pixels [n2].b += be * k3Per16;
					}
					
					if (x < texw - 1) {
						pixels [n4].a += ae * k1Per16;
						pixels [n4].r += re * k1Per16;
						pixels [n4].g += ge * k1Per16;
						pixels [n4].b += be * k1Per16;
					}
				}
				offs++;
			}
		}
		return pixels;
	}
}
}