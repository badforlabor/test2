/****************************************************************************
Copyright (c) 2013-2014,Dalian-LingYOU tech.
 This is not a free-ware .DO NOT use it without any authorization.
 * 
 * date : 2016/4/18 10:37:03
 * author : Labor
 * purpose : 自动切割动画
****************************************************************************/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using System.Text;
using System.Reflection;

namespace MLGame
{
    static class SerializedPropertyExtension
    {
        public static void SetStringValue(this SerializedProperty self, string propertyName, string value)
        {
            self.FindPropertyRelative(propertyName).stringValue = value;
        }
		
		public static T GetColumnUnsafe<T>(this ConfigLine self, string column)
        {
            try
            {
                return self.GetColumn<T>(column);
            }
            catch (Exception e)
            { }
            return default(T);
        }
    }


    class AnimationAutoClip
    {
        [MenuItem("liubo/自动切割骨骼动画")]
        public static void AutoClip()
        {
            string path = Selection.activeObject == null ? "" : AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log("[切割动画] path=" + path);

            ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi == null)
            {
                Debug.Log("[切割动画] 没找到FBX文件！ path=" + path);
            }
            else
            {
                Debug.Log("[切割动画] 开始切割：path=" + path);

                // 注意保留就动画的一些数据，比如绑定的event

                List<ModelImporterClipAnimation> oldClips = new List<ModelImporterClipAnimation>();
                List<ModelImporterClipAnimation> newClips = new List<ModelImporterClipAnimation>();
                if (mi.clipAnimations != null)
                {
                    oldClips.AddRange(mi.clipAnimations);
                }

#if true//TEST
                //AnimationUtility.GetAnimationClipSettings(null);
                ModelImporterClipAnimation test = new ModelImporterClipAnimation();
                test.name = "test";
                test.takeName = "auto";
                test.firstFrame = 0;
                test.lastFrame = 30;
                test.wrapMode = WrapMode.Default;
                test.keepOriginalPositionY = true;
                test.loop = false;
                test.loopTime = false;
                test.loopPose = false;
                test.keepOriginalOrientation = false;
                test.keepOriginalPositionY = false;
                test.keepOriginalPositionXZ = false;
                test.cycleOffset = 0;
                test.heightOffset = 0;
                test.rotationOffset = 0;

                newClips.Add(test);
                newClips.AddRange(oldClips);
#endif

                mi.clipAnimations = newClips.ToArray();
                EditorUtility.SetDirty(Selection.activeObject);


            }
        }

        [MenuItem("liubo/自动切割骨骼动画2")]
        public static void AutoClip2()
        {
            /*
             *  读取excel表，获取动画的信息：名字，起始帧，结束帧
             *  再读取fbx的动画信息，对名字相同的进行信息merge
             *      merge：起始帧，结束帧     
             *      保留：event信息，mask信息，curve信息
             *      如果有新加的，那么加入新的信息
             * **/

            // 匿名函数

            System.Func<bool, ConfigLine, SerializedProperty, int> SetPropertyFunction = delegate(bool IsHumanClip, ConfigLine csv_line, SerializedProperty sp)
            {
                sp.FindPropertyRelative("firstFrame").floatValue = csv_line.GetColumnUnsafe<float>("firstFrame");
                sp.FindPropertyRelative("lastFrame").floatValue = csv_line.GetColumnUnsafe<float>("lastFrame");

                // LoopTime, LoopPose
                sp.FindPropertyRelative("loopTime").boolValue = csv_line.GetColumnUnsafe<int>("loopTime") > 0;
                sp.FindPropertyRelative("loopBlend").boolValue = csv_line.GetColumnUnsafe<int>("loopBlend") > 0;

                // Rotation_BakeIntoPose, Rotation_BaseUpon
                sp.FindPropertyRelative("loopBlendOrientation").boolValue = csv_line.GetColumnUnsafe<int>("loopBlendOrientation") > 0;
                sp.FindPropertyRelative("keepOriginalOrientation").boolValue = csv_line.GetColumnUnsafe<int>("keepOriginalOrientation") == 0;

                // PositionY_BakeIntoPose, PositionY_BaseUpon
                sp.FindPropertyRelative("loopBlendPositionY").boolValue = csv_line.GetColumnUnsafe<int>("loopBlendPositionY") > 0;
                if (IsHumanClip)
                {
                    // 如果是humanoid类型的动画，那么keepOriginalPositionY的取值为0，1，2三种。
                    sp.FindPropertyRelative("keepOriginalPositionY").boolValue = csv_line.GetColumnUnsafe<int>("keepOriginalPositionY") == 0;
                    sp.FindPropertyRelative("heightFromFeet").boolValue = csv_line.GetColumnUnsafe<int>("keepOriginalPositionY") == 2;
                }
                else
                {
                    sp.FindPropertyRelative("keepOriginalPositionY").boolValue = csv_line.GetColumnUnsafe<int>("keepOriginalPositionY") == 0;
                }

                // PositionXZ_BakeIntoPose, PositionXZ_BaseUpon
                sp.FindPropertyRelative("loopBlendPositionXZ").boolValue = csv_line.GetColumnUnsafe<int>("loopBlendPositionXZ") > 0;
                sp.FindPropertyRelative("keepOriginalPositionXZ").boolValue = csv_line.GetColumnUnsafe<int>("keepOriginalPositionXZ") == 0;
                return 0;
            };


            // 读取excel
            string[] files = Directory.GetFiles(Application.dataPath + "/Art/", "animations*.xls", SearchOption.AllDirectories);
            foreach (var fileName in files)
            {
                DateTime excelTime = File.GetLastWriteTime(fileName);
                Debug.Log("读取xls文件：" + fileName + ", file-write-time=" + excelTime.ToString());
                Dictionary<string, ConfigFile> excels = ReadXML(fileName);// ReadXML(Application.dataPath + "/Art/animations.xls");
                foreach (var it in excels)
                {
                    Debug.Log("处理：" + it.Key);
                    ConfigFile csv = it.Value;
                    string[] assets = AssetDatabase.FindAssets(it.Key, new string[] { "Assets/Art" });
                    if (assets.Length > 0)
                    {
                        string asset = AssetDatabase.GUIDToAssetPath(assets[0]);
                        
						// 如果.meta时间和excel时间不一样，说明有变化，否则没变化的就直接跳过
                        DateTime assetMetaTime = File.GetLastWriteTime(asset + ".meta");
                        if (DateTime.Equals(assetMetaTime, excelTime))
                        {
                            // 文件没有变化，不处理
                            Debug.Log("文件没有变化，不处理啦：" + asset);
                            continue;
                        }
                        File.SetLastWriteTime(asset + ".meta", excelTime);
						
                        ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(asset);

                        SerializedObject serializedObject = modelImporter == null ? null : new SerializedObject(modelImporter);
                        ModelImporterAnimationType m_AnimationType = (ModelImporterAnimationType)serializedObject.FindProperty("m_AnimationType").intValue;
                        bool IsHumanClip = m_AnimationType == ModelImporterAnimationType.Human;
                        HashSet<string> processed_clip = new HashSet<string>();
                        if (serializedObject == null)
                        {
                            Debug.Log("无法处理该文件：" + asset);
                        }
                        else
                        {
                            // 压缩方式
                            serializedObject.FindProperty("m_AnimationCompression").intValue = (int)ModelImporterAnimationCompression.Optimal;

                            // 切割或者merge动画
                            SerializedProperty m_ClipAnimations = serializedObject.FindProperty("m_ClipAnimations");
                            for (int i = 0; i < m_ClipAnimations.arraySize; i++)
                            {
                                SerializedProperty sp = m_ClipAnimations.GetArrayElementAtIndex(i);
                                string clip_name = sp.FindPropertyRelative("name").stringValue;
                                if (clip_name.Contains("@"))
                                {
                                    clip_name = clip_name.Substring(clip_name.LastIndexOf("@") + 1);
                                }
                                ConfigLine csv_line = csv.FindData(clip_name);
                                if (csv_line != null)
                                {
                                    processed_clip.Add(clip_name);

                                    // merge基本属性
                                    SetPropertyFunction(IsHumanClip, csv_line, sp);

                                    // 其他属性，如event，mask，都不处理，保持原来信息
                                }
                            }

                            // 把剩下没处理的内容，作为新的动画，分割到fbx中
                            Dictionary<string, ConfigLine> csv_data = csv.GetLines();
                            foreach (var kt in csv_data)
                            {
                                if (processed_clip.Contains(kt.Key))
                                {
                                    continue;
                                }
                                processed_clip.Add(kt.Key);
                                ConfigLine csv_line = kt.Value;

                                m_ClipAnimations.InsertArrayElementAtIndex(m_ClipAnimations.arraySize);
                                SerializedProperty sp = m_ClipAnimations.GetArrayElementAtIndex(m_ClipAnimations.arraySize - 1);

                                sp.FindPropertyRelative("name").stringValue = it.Key + "@" + kt.Key;
                                sp.FindPropertyRelative("takeName").stringValue = "Take 001";

                                // 处理基本属性
                                SetPropertyFunction(IsHumanClip, csv_line, sp);

                                // 设置mask
                                UnityEditorInternal.AvatarMask mask = new UnityEditorInternal.AvatarMask();
                                mask.transformCount = modelImporter.transformPaths.Length;
                                for (int i = 0; i < modelImporter.transformPaths.Length; i++)
                                {
                                    mask.SetTransformPath(i, modelImporter.transformPaths[i]);
                                    mask.SetTransformActive(i, true);
                                }
                                SerializedProperty bodyMask = sp.FindPropertyRelative("bodyMask");
                                if (bodyMask != null && bodyMask.isArray)
                                {
                                    for (int i = 0; i < mask.humanoidBodyPartCount; i++)
                                    {
                                        if (i >= bodyMask.arraySize) bodyMask.InsertArrayElementAtIndex(i);
                                        bodyMask.GetArrayElementAtIndex(i).intValue = mask.GetHumanoidBodyPartActive(i) ? 1 : 0;
                                    }
                                }
                                SerializedProperty transformMask = sp.FindPropertyRelative("transformMask");
                                //ModelImporter.UpdateTransformMask(mask, transformMask);
                                Type ty = typeof(ModelImporter);
                                MethodInfo mi = ty.GetMethod("UpdateTransformMask", BindingFlags.Static | BindingFlags.NonPublic);
                                if (mi != null)
                                {
                                    mi.Invoke(null, new object[] { mask, transformMask });
                                }
                                else
                                {
                                    Debug.LogError("无法找到此方法！");
                                }
                            }

                            serializedObject.ApplyModifiedProperties();
                            AssetDatabase.WriteImportSettingsIfDirty(asset);
                            AssetDatabase.ImportAsset(asset);
                        }
                    }
                    else
                    {
                        Debug.LogError("无法找到这个FBX文件：" + it.Key);
                    }
                }
            }
        }
        [MenuItem("liubo/切割选中的动画3")]
        public static void AutoClip3()
        {


            // 获取m_AnimationClip，然后处理

            ModelImporter modelImporter = Selection.activeObject == null ? null : (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            SerializedObject serializedObject = modelImporter == null ? null : new SerializedObject(modelImporter);
            //if (serializedObject != null)
            //{
            //    Debug.Log("[切割动画] 切割对象：" + serializedObject.targetObject.name);
            //    SerializedProperty attr = serializedObject.GetIterator();
            //    attr.Next(true);
            //    do
            //    {
            //        Debug.Log("[切割动画] attr-name=" + attr.name);
            //    }
            //    while (attr.Next(true));
            //}
            SerializedProperty m_ClipAnimations = serializedObject == null ? null : serializedObject.FindProperty("m_ClipAnimations");
            if (m_ClipAnimations != null)
            {
                if (m_ClipAnimations.arraySize > 0)
                {
                    //AnimationClip ac = m_ClipAnimations.GetArrayElementAtIndex(0).serializedObject.targetObject as AnimationClip;
                    //Debug.Log("[切割动画] ac=" + (ac == null ? "none" : ac.name) + ", ref-obj=" + m_ClipAnimations.GetArrayElementAtIndex(0).serializedObject.targetObject.name);

                    // 这个返回的是 ModelImporterClipAnimation 对象
                    SerializedProperty sp = m_ClipAnimations.GetArrayElementAtIndex(0);

                    //ShowProperties(sp);
                    Debug.Log("[切割动画] ac.name=" + sp.FindPropertyRelative("name").stringValue);

                    // 更改他的信息
                    // 修改event
                    {
                        sp.FindPropertyRelative("firstFrame").floatValue = 10;
                        sp.FindPropertyRelative("lastFrame").floatValue = 20;
                    }


                    SerializedProperty sp_event = sp.FindPropertyRelative("events");
                    if (sp_event != null && sp_event.arraySize > 0)
                    {
                        Debug.Log("[切割动画] sp_event.arraySize=" + sp_event.arraySize);

                        // 修改event
                        if (false)
                        {
                            sp_event.InsertArrayElementAtIndex(sp_event.arraySize);
                            SerializedProperty sp_one_event = sp_event.GetArrayElementAtIndex(sp_event.arraySize - 1);
                            sp_one_event.SetStringValue("functionName", "123");
                        }
                    }
                    serializedObject.ApplyModifiedProperties();
                    AssetDatabase.WriteImportSettingsIfDirty(AssetDatabase.GetAssetPath(Selection.activeObject));
                }
                EditorUtility.SetDirty(Selection.activeObject);
                AssetDatabase.SaveAssets();
            }


        }


        static void ShowProperties(SerializedProperty sp)
        {
            SerializedProperty backup = sp.Copy();

            {
                sp = backup.Copy();
                Debug.Log("[显示属性] sp.name=" + sp.displayName + ", " + sp.name);
                sp.Next(true);
                do
                {
                    Debug.Log("[显示属性] sp.name=" + sp.displayName + ", " + sp.name);
                } while (sp.Next(false));
            }
        }

        static Dictionary<string, ConfigFile> ReadXML(string path)
        {
            string shortName = Path.GetFileNameWithoutExtension(path);
            //path = Application.dataPath + "/../Builder/builder_config.xls";

            Dictionary<string, ConfigFile> ret = new Dictionary<string, ConfigFile>();

            // !< 说明:客户端的表读取,就不走服务器那样的复杂处理啦..
            HSSFWorkbook wk = new HSSFWorkbook(File.OpenRead(path));
            HSSFSheet sheet = null;
            string sheetname = "";
            for (int a = 0; a < wk.NumberOfSheets; ++a)
            {
                sheet = wk.GetSheetAt(a) as HSSFSheet;
                sheetname = wk.GetSheetName(a);
                if (sheet == null)
                {
                    Debug.LogError("异常!" + shortName);
                    continue;
                }
                if (ret.ContainsKey(sheetname))
                {
                    Debug.LogError("该excel表的sheet名字重复了：" + wk.GetSheetName(a));
                }
                else
                {
                    ConfigFile result = new ConfigFile(shortName + "_" + sheetname);
                    List<String> header = new List<string>();
                    for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; ++i)
                    {
                        HSSFRow row = sheet.GetRow(i) as HSSFRow;
                        if (row == null) continue;
                        ArrayList line = new ArrayList();
                        for (int j = row.FirstCellNum; j <= row.LastCellNum; ++j)
                        {
                            HSSFCell cell = row.GetCell(j) as HSSFCell;
                            if (cell == null) continue;
                            if (i == sheet.FirstRowNum)
                            {
                                header.Add(cell.ToString());
                            }
                            else
                            {
                                line.Add(cell.ToString());
                            }
                        }
                        if (i == sheet.FirstRowNum)
                        {
                            result.SetTitles(header.ToArray());
                        }
                        else
                        {
                            if (line.Count > 0)
                            {
                                result.AddData(line[0] as String, line);
                            }
                            else
                            {
                                // !< 读完了?
                                break;
                            }
                        }
                    }
                    ret.Add(sheetname, result);
                }
            }

            return ret;
        }
    }
}
