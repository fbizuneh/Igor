#if IGOR_RUNTIME || UNITY_EDITOR
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System;
using System.Reflection;
using System.Xml.Serialization;

namespace Igor
{
	public class IgorModuleBase : IIgorModule
	{
		public virtual string GetModuleName()
		{
			return "";
		}

		public virtual void RegisterModule()
		{
		}

		public virtual void ProcessArgs(IIgorStepHandler StepHandler)
		{
		}

		public virtual bool IsDependentOnModule(IIgorModule ModuleInst)
		{
			return false;
		}

        public virtual void PostJobCleanup()
        {
        }

#if UNITY_EDITOR
        public static bool bIsDrawingInspector = false;

		public virtual string DrawJobInspectorAndGetEnabledParams(string CurrentParams)
		{
			return CurrentParams;
		}

		public virtual bool ShouldDrawInspectorForParams(string CurrentParams)
		{
			return true;
		}

		public virtual bool DrawBoolParam(ref string CurrentParams, string BoolLabel, string BoolParam)
		{
			bool bIsEnabled = IgorRuntimeUtils.IsBoolParamSet(CurrentParams, BoolParam);

			bIsEnabled = EditorGUILayout.Toggle(new GUIContent(BoolLabel, BoolLabel), bIsEnabled);

			CurrentParams = IgorRuntimeUtils.SetBoolParam(CurrentParams, BoolParam, bIsEnabled);

            return bIsEnabled;
		}

		public virtual void DrawStringParam(ref string CurrentParams, string StringLabel, string StringParam)
		{
			string CurrentStringValue = IgorRuntimeUtils.GetStringParam(CurrentParams, StringParam);

			CurrentStringValue = EditorGUILayout.TextField(new GUIContent(StringLabel, StringLabel), string.IsNullOrEmpty(CurrentStringValue) ? string.Empty : CurrentStringValue);

			CurrentParams = IgorRuntimeUtils.SetStringParam(CurrentParams, StringParam, CurrentStringValue);
		}

		public virtual void DrawFloatParam(ref string CurrentParams, string FloatLabel, string FloatParam, string NumberFormatter = "F0", float UnsetValue = float.NegativeInfinity)
		{
			string CurrentFloatValue = IgorRuntimeUtils.GetStringParam(CurrentParams, FloatParam);
			float CurrentFloatNum = UnsetValue;

			if(!string.IsNullOrEmpty(CurrentFloatValue))
			{
				float.TryParse(CurrentFloatValue, out CurrentFloatNum);
			}

			CurrentFloatNum = EditorGUILayout.FloatField(new GUIContent(FloatLabel, FloatLabel), CurrentFloatNum);

			if(CurrentFloatNum == UnsetValue)
			{
				CurrentFloatValue = "";
			}
			else
			{
				CurrentFloatValue = CurrentFloatNum.ToString(NumberFormatter);
			}

			CurrentParams = IgorRuntimeUtils.SetStringParam(CurrentParams, FloatParam, CurrentFloatValue);
		}

		protected static Texture2D LabelFieldBGGreen = null;
		protected static Texture2D TextFieldBGGreenNormal = null;
		protected static Texture2D TextFieldBGGreenActive = null;

		public static Texture2D TintTextureWithColor(Texture2D InTexture, Color TintColor, float TintAmount)
		{
			Texture2D TintedTexture = InTexture;

			if(InTexture != null)
			{
				RenderTexture TempRenderTexture = new RenderTexture(InTexture.width, InTexture.height, 32);

				Graphics.Blit(InTexture, TempRenderTexture);

				RenderTexture.active = TempRenderTexture;

				TintedTexture = new Texture2D(InTexture.width, InTexture.height);

				TintedTexture.ReadPixels(new Rect(0.0f, 0.0f, InTexture.width, InTexture.height), 0, 0);

				RenderTexture.active = null;

				for(int CurrentX = 0; CurrentX < InTexture.width; ++CurrentX)
				{
					for(int CurrentY = 0; CurrentY < InTexture.height; ++CurrentY)
					{
						Color OriginalColor = TintedTexture.GetPixel(CurrentX, CurrentY);
						Color NewColor = new Color((TintColor.r*TintAmount) + (OriginalColor.r*(1.0f-TintAmount)), (TintColor.g*TintAmount) + (OriginalColor.g*(1.0f-TintAmount)),
						                           (TintColor.b*TintAmount) + (OriginalColor.b*(1.0f-TintAmount)), OriginalColor.a);

						TintedTexture.SetPixel(CurrentX, CurrentY, NewColor);
					}
				}

				TintedTexture.Apply();
			}
			
			return TintedTexture;
		}
		
		public static Texture2D GetTextFieldBGGreenNormal()
		{
			if(TextFieldBGGreenNormal == null)
			{
				GUISkin CurrentSkin = GUI.skin;

				Texture2D TextFieldBG = CurrentSkin.textField.normal.background;

				TextFieldBGGreenNormal = TintTextureWithColor(TextFieldBG, Color.green, 0.25f);				
			}

			return TextFieldBGGreenNormal;
		}

		public static Texture2D GetTextFieldBGGreenActive()
		{
			if(TextFieldBGGreenActive == null)
			{
				GUISkin CurrentSkin = GUI.skin;

				Texture2D TextFieldBG = CurrentSkin.textField.focused.background;

				TextFieldBGGreenActive = TintTextureWithColor(TextFieldBG, Color.green, 0.25f);				
			}

			return TextFieldBGGreenActive;
		}

		public static Texture2D GenerateTexture2DWithColor(Color InColor)
		{
			Texture2D NewTexture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
			NewTexture.SetPixel(0, 1, Color.Lerp(InColor, Color.white, 0.0f));
			NewTexture.Apply();
			
			return NewTexture;
		}

		public static Texture2D GetLabelFieldBGGreen()
		{
			if(LabelFieldBGGreen == null)
			{
				GUISkin CurrentSkin = GUI.skin;

				Texture2D LabelFieldBG = CurrentSkin.label.normal.background;

				LabelFieldBGGreen = TintTextureWithColor(LabelFieldBG, Color.green, 0.25f);				

				if(LabelFieldBGGreen == null)
				{
					LabelFieldBGGreen = GenerateTexture2DWithColor(new Color(Color.green.r * 0.45f, Color.green.g * 0.45f, Color.green.b * 0.45f));
				}
			}

			return LabelFieldBGGreen;
		}

		public virtual void DrawStringConfigParamUseValue(ref string CurrentParams, string StringLabel, string StringOverrideAndConfigKey, string CurrentValue)
		{
			DrawStringConfigParamDifferentOverride(ref CurrentParams, StringLabel, StringOverrideAndConfigKey, StringOverrideAndConfigKey, CurrentValue);
		}

		public virtual void DrawStringConfigParam(ref string CurrentParams, string StringLabel, string StringOverrideAndConfigKey)
		{
			DrawStringConfigParamDifferentOverride(ref CurrentParams, StringLabel, StringOverrideAndConfigKey, StringOverrideAndConfigKey);
		}

		public virtual void DrawStringConfigParamDifferentOverride(ref string CurrentParams, string StringLabel, string StringOverrideParam, string ConfigKey, string OverrideCurrentValue = null)
		{
			string CurrentStringValue = "";
			string CurrentConfigValue = IgorConfig.GetModuleString(this, ConfigKey);

			bool bDisplayConfigValue = false;

			if(OverrideCurrentValue == null)
			{
				if(IgorRuntimeUtils.IsStringParamSet(CurrentParams, StringOverrideParam))
				{
					CurrentStringValue = IgorRuntimeUtils.GetStringParam(CurrentParams, StringOverrideParam);
				}
				else
				{
					bDisplayConfigValue = true;
				}
			}
			else
			{
				if(CurrentConfigValue == OverrideCurrentValue)
				{
					bDisplayConfigValue = true;
				}
				else
				{
					CurrentStringValue = OverrideCurrentValue;
				}
			}

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(new GUIContent(StringLabel, StringLabel), GUILayout.MaxWidth(100.0f));

			EditorGUILayout.BeginHorizontal();

			string DisplayString = bDisplayConfigValue ? CurrentConfigValue : CurrentStringValue;

			GUIStyle TextFieldStyle = new GUIStyle(GUI.skin.textField);

			if(!bDisplayConfigValue)
			{
				TextFieldStyle.normal.background = GetTextFieldBGGreenNormal();
				TextFieldStyle.focused.background = GetTextFieldBGGreenActive();
			}

			string NewStringValue = GUILayout.TextField(DisplayString, TextFieldStyle, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100.0f));
            if(!NewStringValue.Contains("\"") && !(NewStringValue.Length == 1 && NewStringValue[0] == ' '))
            {
                CurrentStringValue = NewStringValue;
            }

			if(bDisplayConfigValue && CurrentStringValue == CurrentConfigValue)
			{
				CurrentStringValue = "";
			}

			GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);

			ButtonStyle.border = new RectOffset();
			ButtonStyle.margin = new RectOffset();

			if(GUILayout.Button(new GUIContent("<-", "Use the config value"), ButtonStyle, GUILayout.Width(25.0f)))
			{
				CurrentStringValue = "";
			}

			if(GUILayout.Button(new GUIContent("->", "Update the config value to the current value (This will change all other jobs that haven't overridden this value!)"), ButtonStyle, GUILayout.Width(25.0f)))
			{
				if(!bDisplayConfigValue)
				{
					CurrentConfigValue = CurrentStringValue;
				}

				IgorConfig.SetModuleString(this, ConfigKey, CurrentConfigValue);
			}

			string ConfigLabel = CurrentConfigValue + " - From Global Config Key: " + GetModuleName() + "." + ConfigKey;
//			string ConfigLabel = "Global Config: \"" + CurrentConfigValue + "\" from Key: \"" + GetModuleName() + "." + ConfigKey + "\"";

			GUIStyle LabelFieldStyle = new GUIStyle(GUI.skin.label);

			LabelFieldStyle.alignment = TextAnchor.MiddleLeft;
			LabelFieldStyle.border = new RectOffset();
			LabelFieldStyle.contentOffset = new Vector2();
			LabelFieldStyle.margin = new RectOffset();

			if(bDisplayConfigValue)
			{
				LabelFieldStyle.normal.background = GetLabelFieldBGGreen();
			}

			GUILayout.Label(new GUIContent(ConfigLabel, ConfigLabel), LabelFieldStyle, GUILayout.MinWidth(20.0f));

			if(GUILayout.Button(new GUIContent("X", "Clear the config value (This will change all other jobs that haven't overridden this value!)"), ButtonStyle, GUILayout.Width(25.0f)))
			{
				CurrentConfigValue = "";

				IgorConfig.SetModuleString(this, ConfigKey, CurrentConfigValue);
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndHorizontal();

			CurrentParams = IgorRuntimeUtils.SetStringParam(CurrentParams, StringOverrideParam, CurrentStringValue);
		}

		public virtual void DrawStringOptionsParam(ref string CurrentParams, string StringLabel, string StringParam, List<string> ValidOptions)
		{
			DrawStringOptionsParam(ref CurrentParams, StringLabel, StringParam, ValidOptions.ToArray());
		}

		public virtual void DrawStringOptionsParam(ref string CurrentParams, string StringLabel, string StringParam, string[] ValidOptions)
		{
			string CurrentStringValue = IgorRuntimeUtils.GetStringParam(CurrentParams, StringParam);

			if(CurrentStringValue == "")
			{
				CurrentStringValue = "Not set";
			}

			List<GUIContent> AllOptions = new List<GUIContent>();

			AllOptions.Add(new GUIContent("Not set", "Not set"));

			foreach(string CurrentOption in ValidOptions)
			{
				AllOptions.Add(new GUIContent(CurrentOption, CurrentOption));
			}

			int ChosenIndex = -1;

			for(int CurrentIndex = 0; CurrentIndex < AllOptions.Count; ++CurrentIndex)
			{
				if(AllOptions[CurrentIndex].text == CurrentStringValue)
				{
					ChosenIndex = CurrentIndex;
				}
			}

			if(ChosenIndex == -1)
			{
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField(new GUIContent(StringLabel, StringLabel));

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.LabelField(CurrentStringValue);

				if(GUILayout.Button(new GUIContent("Reset invalid value", "This value is set to an invalid value.")))
				{
					CurrentStringValue = "";

					CurrentParams = IgorRuntimeUtils.SetStringParam(CurrentParams, StringParam, CurrentStringValue);
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndHorizontal();
			}
			else
			{
			    int NewIndex = EditorGUILayout.Popup(new GUIContent(StringLabel, StringLabel), ChosenIndex, AllOptions.ToArray());
		        if(NewIndex != ChosenIndex)
		        {
		            ChosenIndex = NewIndex;
		            CurrentStringValue = AllOptions[ChosenIndex].text;

		            if(CurrentStringValue == "Not set")
		            {
		                CurrentStringValue = "";
		            }

		            CurrentParams = IgorRuntimeUtils.SetStringParam(CurrentParams, StringParam, CurrentStringValue);
		        }
            }
		}
#endif //UNITY_EDITOR

		public virtual void Log(string Message)
		{
			IgorDebug.Log(this, Message);
		}

		public virtual void LogWarning(string Message)
		{
			IgorDebug.LogWarning(this, Message);
		}

		public virtual void LogError(string Message)
		{
			IgorDebug.LogError(this, Message);
		}

		public virtual void CriticalError(string Message)
		{
			IgorDebug.CriticalError(this, Message);
		}

		public virtual string GetParamOrConfigString(string StringKey, string EmptyStringWarningMessage = "", string DefaultValue = "", bool bCheckForEmpty = true)
		{
#if UNITY_EDITOR
			if(bIsDrawingInspector && EmptyStringWarningMessage != "")
			{
				LogError("Don't call this from within a DrawJobInspectorAndGetEnabledParams implementation!  This isn't accessing the right job config value since it hasn't been saved to disk yet.");
			}
#endif // UNITY_EDITOR

			string StringValue = DefaultValue;

			if(IgorJobConfig.IsStringParamSet(StringKey))
			{
				StringValue = IgorJobConfig.GetStringParam(StringKey);
			}
			else
			{
				StringValue = IgorConfig.GetModuleString(this, StringKey);
			}

			if(StringValue == DefaultValue && bCheckForEmpty && EmptyStringWarningMessage != "")
			{
				LogWarning(EmptyStringWarningMessage);
			}

			if(StringValue == "")
			{
				StringValue = DefaultValue;
			}

			return StringValue;
		}

		public virtual bool GetConfigBool(string BoolKey, bool bDefaultValue = false)
		{
			return IgorConfig.GetModuleBool(this, BoolKey, bDefaultValue);
		}

		public virtual void SetConfigBool(string BoolKey, bool bValue)
		{
			IgorConfig.SetModuleBool(this, BoolKey, bValue);
		}

		public virtual string GetConfigString(string StringKey, string DefaultValue = "")
		{
			return IgorConfig.GetModuleString(this, StringKey, DefaultValue);
		}

		public virtual void SetConfigString(string StringKey, string Value)
		{
			IgorConfig.SetModuleString(this, StringKey, Value);
		}
	}
}

#endif // IGOR_RUNTIME || UNITY_EDITOR
