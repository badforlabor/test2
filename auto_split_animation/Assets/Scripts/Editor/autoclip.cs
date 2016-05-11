/****************************************************************************
Copyright (c) 2013-2014,Dalian-LingYOU tech.
 This is not a free-ware .DO NOT use it without any authorization.
 * 
 * date : 2016/4/18 10:37:03
 * author : Labor
 * purpose : �Զ��и��
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
        [MenuItem("liubo/�Զ��и��������")]
        public static void AutoClip()
        {
            string path = Selection.activeObject == null ? "" : AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log("[�и��] path=" + path);

            ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi == null)
            {
                Debug.Log("[�и��] û�ҵ�FBX�ļ��� path=" + path);
            }
            else
            {
                Debug.Log("[�и��] ��ʼ�иpath=" + path);

                // ע�Ᵽ���Ͷ�����һЩ���ݣ�����󶨵�event

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

        [MenuItem("liubo/�Զ��и��������2")]
        public static void AutoClip2()
        {
            /*
             *  ��ȡexcel����ȡ��������Ϣ�����֣���ʼ֡������֡
             *  �ٶ�ȡfbx�Ķ�����Ϣ����������ͬ�Ľ�����Ϣmerge
             *      merge����ʼ֡������֡     
             *      ������event��Ϣ��mask��Ϣ��curve��Ϣ
             *      ������¼ӵģ���ô�����µ���Ϣ
             * **/

            // ��������

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
                    // �����humanoid���͵Ķ�������ôkeepOriginalPositionY��ȡֵΪ0��1��2���֡�
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


            // ��ȡexcel
            string[] files = Directory.GetFiles(Application.dataPath + "/Art/", "animations*.xls", SearchOption.AllDirectories);
            foreach (var fileName in files)
            {
                DateTime excelTime = File.GetLastWriteTime(fileName);
                Debug.Log("��ȡxls�ļ���" + fileName + ", file-write-time=" + excelTime.ToString());
                Dictionary<string, ConfigFile> excels = ReadXML(fileName);// ReadXML(Application.dataPath + "/Art/animations.xls");
                foreach (var it in excels)
                {
                    Debug.Log("����" + it.Key);
                    ConfigFile csv = it.Value;
                    string[] assets = AssetDatabase.FindAssets(it.Key, new string[] { "Assets/Art" });
                    if (assets.Length > 0)
                    {
                        string asset = AssetDatabase.GUIDToAssetPath(assets[0]);
                        
						// ���.metaʱ���excelʱ�䲻һ����˵���б仯������û�仯�ľ�ֱ������
                        DateTime assetMetaTime = File.GetLastWriteTime(asset + ".meta");
                        if (DateTime.Equals(assetMetaTime, excelTime))
                        {
                            // �ļ�û�б仯��������
                            Debug.Log("�ļ�û�б仯������������" + asset);
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
                            Debug.Log("�޷�������ļ���" + asset);
                        }
                        else
                        {
                            // ѹ����ʽ
                            serializedObject.FindProperty("m_AnimationCompression").intValue = (int)ModelImporterAnimationCompression.Optimal;

                            // �и����merge����
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

                                    // merge��������
                                    SetPropertyFunction(IsHumanClip, csv_line, sp);

                                    // �������ԣ���event��mask��������������ԭ����Ϣ
                                }
                            }

                            // ��ʣ��û��������ݣ���Ϊ�µĶ������ָfbx��
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

                                // �����������
                                SetPropertyFunction(IsHumanClip, csv_line, sp);

                                // ����mask
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
                                    Debug.LogError("�޷��ҵ��˷�����");
                                }
                            }

                            serializedObject.ApplyModifiedProperties();
                            AssetDatabase.WriteImportSettingsIfDirty(asset);
                            AssetDatabase.ImportAsset(asset);
                        }
                    }
                    else
                    {
                        Debug.LogError("�޷��ҵ����FBX�ļ���" + it.Key);
                    }
                }
            }
        }
        [MenuItem("liubo/�и�ѡ�еĶ���3")]
        public static void AutoClip3()
        {


            // ��ȡm_AnimationClip��Ȼ����

            ModelImporter modelImporter = Selection.activeObject == null ? null : (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            SerializedObject serializedObject = modelImporter == null ? null : new SerializedObject(modelImporter);
            //if (serializedObject != null)
            //{
            //    Debug.Log("[�и��] �и����" + serializedObject.targetObject.name);
            //    SerializedProperty attr = serializedObject.GetIterator();
            //    attr.Next(true);
            //    do
            //    {
            //        Debug.Log("[�и��] attr-name=" + attr.name);
            //    }
            //    while (attr.Next(true));
            //}
            SerializedProperty m_ClipAnimations = serializedObject == null ? null : serializedObject.FindProperty("m_ClipAnimations");
            if (m_ClipAnimations != null)
            {
                if (m_ClipAnimations.arraySize > 0)
                {
                    //AnimationClip ac = m_ClipAnimations.GetArrayElementAtIndex(0).serializedObject.targetObject as AnimationClip;
                    //Debug.Log("[�и��] ac=" + (ac == null ? "none" : ac.name) + ", ref-obj=" + m_ClipAnimations.GetArrayElementAtIndex(0).serializedObject.targetObject.name);

                    // ������ص��� ModelImporterClipAnimation ����
                    SerializedProperty sp = m_ClipAnimations.GetArrayElementAtIndex(0);

                    //ShowProperties(sp);
                    Debug.Log("[�и��] ac.name=" + sp.FindPropertyRelative("name").stringValue);

                    // ����������Ϣ
                    // �޸�event
                    {
                        sp.FindPropertyRelative("firstFrame").floatValue = 10;
                        sp.FindPropertyRelative("lastFrame").floatValue = 20;
                    }


                    SerializedProperty sp_event = sp.FindPropertyRelative("events");
                    if (sp_event != null && sp_event.arraySize > 0)
                    {
                        Debug.Log("[�и��] sp_event.arraySize=" + sp_event.arraySize);

                        // �޸�event
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
                Debug.Log("[��ʾ����] sp.name=" + sp.displayName + ", " + sp.name);
                sp.Next(true);
                do
                {
                    Debug.Log("[��ʾ����] sp.name=" + sp.displayName + ", " + sp.name);
                } while (sp.Next(false));
            }
        }

        static Dictionary<string, ConfigFile> ReadXML(string path)
        {
            string shortName = Path.GetFileNameWithoutExtension(path);
            //path = Application.dataPath + "/../Builder/builder_config.xls";

            Dictionary<string, ConfigFile> ret = new Dictionary<string, ConfigFile>();

            // !< ˵��:�ͻ��˵ı��ȡ,�Ͳ��߷����������ĸ��Ӵ�����..
            HSSFWorkbook wk = new HSSFWorkbook(File.OpenRead(path));
            HSSFSheet sheet = null;
            string sheetname = "";
            for (int a = 0; a < wk.NumberOfSheets; ++a)
            {
                sheet = wk.GetSheetAt(a) as HSSFSheet;
                sheetname = wk.GetSheetName(a);
                if (sheet == null)
                {
                    Debug.LogError("�쳣!" + shortName);
                    continue;
                }
                if (ret.ContainsKey(sheetname))
                {
                    Debug.LogError("��excel���sheet�����ظ��ˣ�" + wk.GetSheetName(a));
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
                                // !< ������?
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
